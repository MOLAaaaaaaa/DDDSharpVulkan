using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using DataCollection;
namespace FloodFill2
{
    /// <summary>
    /// Implements the QueueLinear flood fill algorithm using array-based pixel manipulation.
    /// </summary>
    public class QueueLinearFloodFiller : AbstractFloodFiller
    {
        //Queue of floodfill ranges. We use our own class to increase performance.
        //To use .NET Queue class, change this to:
        //<FloodFillRange> ranges = new Queue<FloodFillRange>();
        FloodFillRangeQueue ranges = new FloodFillRangeQueue();
        struct RouteStruct
        {
            public int ix; //当前位置x
            public int iy; //当前位置y
            public int next;//下一方向 0,1,2,3,4,5,6,7 
            //          7  0   1
            //           \ | /
            //       6____\|/____2
            //            /|\
            //           / | \3
            //           5  4
            public RouteStruct(int _ix,int _iy,int _next = -1)
            {
                ix = _ix;
                iy = _iy;
                next = _next;
            }
            static public int InverseRoute(int direct)
            {
                if (direct <= 3) return direct + 4;
                else return direct - 4;
            }
        }
        public QueueLinearFloodFiller(AbstractFloodFiller configSource) : base(configSource) { }

        /// <summary>
        /// Fills the specified point on the bitmap with the currently selected fill color.
        /// </summary>
        /// <param name="pt">The starting point for the fill.</param>
        public override void FloodFill(System.Drawing.Point pt)
        {
            watch.Reset();
            watch.Start();

            filledIndices.Clear();        

            //***Prepare for fill.
            PrepareForFloodFill(pt);

            ranges = new FloodFillRangeQueue(((bitmapWidth+bitmapHeight)/2)*5);//new Queue<FloodFillRange>();

            //***Get starting color.
            int x = pt.X; int y = pt.Y;
            int idx = CoordsToByteIndex(ref x, ref y);
            startColor = new byte[] { bitmap.Bits[idx], bitmap.Bits[idx + 1], bitmap.Bits[idx + 2] };
            
            bool[] pixelsChecked=this.pixelsChecked;

            //***Do first call to floodfill.
            LinearFill(ref x, ref y);

            //***Call floodfill routine while floodfill ranges still exist on the queue
            while (ranges.Count > 0)
            {
                //**Get Next Range Off the Queue
                FloodFillRange range = ranges.Dequeue();

	            //**Check Above and Below Each Pixel in the Floodfill Range
                int downPxIdx = (bitmapWidth * (range.Y + 1)) + range.StartX;//CoordsToPixelIndex(lFillLoc,y+1);
                int upPxIdx = (bitmapWidth * (range.Y - 1)) + range.StartX;//CoordsToPixelIndex(lFillLoc, y - 1);
                int upY=range.Y - 1;//so we can pass the y coord by ref
                int downY = range.Y + 1;
                int tempIdx;
                for (int i = range.StartX; i <= range.EndX; i++)
                {
                    //*Start Fill Upwards
                    //if we're not above the top of the bitmap and the pixel above this one is within the color tolerance
                    tempIdx = CoordsToByteIndex(ref i, ref upY);
                    if (range.Y > 0 && (!pixelsChecked[upPxIdx]) && CheckPixel(ref tempIdx))
                        LinearFill(ref i, ref upY);
                    
                    //*Start Fill Downwards
                    //if we're not below the bottom of the bitmap and the pixel below this one is within the color tolerance
                    tempIdx = CoordsToByteIndex(ref i, ref downY);
                    if (range.Y < (bitmapHeight - 1) && (!pixelsChecked[downPxIdx]) && CheckPixel(ref tempIdx))
                        LinearFill(ref i, ref downY);
                    downPxIdx++;
                    upPxIdx++;
                }
                
            }

            watch.Stop();
        }

        /// <summary>
        /// Finds the furthermost left and right boundaries of the fill area
        /// on a given y coordinate, starting from a given x coordinate, filling as it goes.
        /// Adds the resulting horizontal range to the queue of floodfill ranges,
        /// to be processed in the main loop.
        /// </summary>
        /// <param name="x">The x coordinate to start from.</param>
        /// <param name="y">The y coordinate to check at.</param>        
        void LinearFill(ref int x, ref int y)
        {
            //cache some bitmap and fill info in local variables for a little extra speed
            byte[] bitmapBits = this.bitmapBits;
            bool[] pixelsChecked = this.pixelsChecked;
            byte[] byteFillColor = this.byteFillColor;
            int bitmapPixelFormatSize = this.bitmapPixelFormatSize;
            int bitmapWidth = this.bitmapWidth;

            //***Find Left Edge of Color Area
            int lFillLoc = x; //the location to check/fill on the left
            int idx = CoordsToByteIndex(ref x, ref y); //the byte index of the current location
            int pxIdx = (bitmapWidth * y) + x;//CoordsToPixelIndex(x,y);
            while (true)
            {
                //**fill with the color
                bitmapBits[idx] = byteFillColor[0];
                bitmapBits[idx + 1] = byteFillColor[1];
                bitmapBits[idx + 2] = byteFillColor[2];
                filledIndices.Add(pxIdx); // modified by jian 2020.1.31,filled points index
                //**indicate that this pixel has already been checked and filled
                pixelsChecked[pxIdx] = true;
                //**screen update for 'slow' fill
                if (slow) UpdateScreen(ref lFillLoc, ref y);
                //**de-increment
                lFillLoc--;     //de-increment counter
                pxIdx--;        //de-increment pixel index
                idx -= bitmapPixelFormatSize;//de-increment byte index
                //**exit loop if we're at edge of bitmap or color area
                if (lFillLoc <= 0 || (pixelsChecked[pxIdx]) || !CheckPixel(ref idx))
                    break;

            }
            lFillLoc++;

            //***Find Right Edge of Color Area
            int rFillLoc = x; //the location to check/fill on the left
            idx = CoordsToByteIndex(ref x, ref y);
            pxIdx = (bitmapWidth * y) + x;
            while (true)
            {
                //**fill with the color
                bitmapBits[idx] = byteFillColor[0];
                bitmapBits[idx + 1] = byteFillColor[1];
                bitmapBits[idx + 2] = byteFillColor[2];
                filledIndices.Add(pxIdx); // modified by jian 2020.1.31,filled points index

                //**indicate that this pixel has already been checked and filled
                pixelsChecked[pxIdx] = true;
                //**screen update for 'slow' fill
                if (slow) UpdateScreen(ref rFillLoc, ref y);
                //**increment
                rFillLoc++;     //increment counter
                pxIdx++;        //increment pixel index
                idx += bitmapPixelFormatSize;//increment byte index
                //**exit loop if we're at edge of bitmap or color area
                if (rFillLoc >= bitmapWidth || pixelsChecked[pxIdx] || !CheckPixel(ref idx))
                    break;

            }
            rFillLoc--;

            //add range to queue
            FloodFillRange r = new FloodFillRange(lFillLoc, rFillLoc, y);
            ranges.Enqueue(ref r);
        }        
        
        /// <summary>Traced the edges of the filled area
        /// re-write by jian 2021-7-20
        /// </summary>
        /// <param name="filled">list of the filled position indices
        /// returned by LinearFill
        /// <para name = drawrect> draw rectangle without margine </para>
        /// </param>
        /// <returns> polygon edges points</returns>
        public override List<Vector32> TraceFilledEdges(Rectangle drawrect, double minx, double miny, double maxx, double maxy)
        {
            if (filledIndices.Count < 1) return null;
            
            int width = bitmap.Bitmap.Width;
            int height = bitmap.Bitmap.Height;

            int ix, iy, x1=0, x2=0, y1=0, y2=0 ;

            //check the rect boundary of the grids
            // and store to x1, x2, y1, y2
            for(int i = 0; i < filledIndices.Count; i++ )
            {
                iy = filledIndices[i] / width;
                ix = filledIndices[i] % width;
                if( i == 0 )
                {
                    x1 = x2 = ix;
                    y1 = y2 = iy;
                }
                else
                {
                    if (ix < x1) x1 = ix;
                    if (iy < y1) y1 = iy;
                    if (ix > x2) x2 = ix;
                    if (iy > y2) y2 = iy;
                }
            }            
            
            //create rect grids 
            int nx = x2 - x1 + 1;
            int ny = y2 - y1 + 1;

            bool[] grids = new bool[nx * ny];
            for (int i = 0; i < grids.Length; i++) grids[i] = false;
            for (int i = 0; i < filledIndices.Count; i++)
            {
                iy = filledIndices[i] / width - y1;
                ix = filledIndices[i] % width - x1;
                grids[ix + iy * nx] = true; //set filled grid as 1,otherwise as 0
            }

            //search edge points            
            List<Point> edges = new List<Point>();            
            for ( iy = 0; iy < ny; iy++)//从上到下
            {
                for (ix = 0; ix < nx; ix++)//从左到右
                {
                    if( grids[ix + iy * nx] )
                    {
                        edges.Add( new Point(ix - 1, iy) );
                        break;
                    }
                }
            }

            iy = ny - 1;//最底一行
            for (ix = 0; ix < nx; ix++)
            {
                if (grids[ix + iy * nx])
                {
                    edges.Add(new Point(ix, iy+1));
                }
            }

            for (iy = ny-1; iy >= 0; iy--)//从下到上
            {
                for (ix = nx-1; ix >= 0; ix--)//从右到左
                {
                    if (grids[ix + iy * nx])
                    {
                        edges.Add(new Point(ix + 1, iy));
                        break;
                    }
                }
            }
            iy = 0;//最顶一行
            for (ix = nx-1; ix >= 0; ix--)//从右到左
            {
                if (grids[ix + iy * nx])
                {
                    edges.Add(new Point(ix, iy-1));
                }
            }

            if (edges.Count < 2) //failed
            {
                edges.Clear();
                return null;
            }     
            
            //移除冗余点
            //RemoveLineRedundant(ref edges);
            
            List<Vector32> poly = new List<Vector32>();
            double x, y;
            for (int i = 0; i < edges.Count; i++) 
            {
                x = minx + (maxx - minx) * (edges[i].X + x1 - drawrect.Left) / drawrect.Width;
                y = maxy - (maxy - miny) * (edges[i].Y + y1 - drawrect.Top ) / drawrect.Height;
                poly.Add(new Vector32(x, y, 0));
            }
            edges.Clear();
            return poly;
           // return new List<Vector32>( Polygon2D.OrderClockwise(poly.ToArray()) );
            /*
            CSlicer slicer = new CSlicer(ny, nx);
            slicer.CreateGridSlicer(grids, ny, nx, mx1, my1, mx2, my2);
            MarchingCubes2D mc = new MarchingCubes2D();
            mc.SetData(slicer);
            mc.DoSearchSurface(0.9);
            if( mc.p2DIsoSurfaces.Count > 0 )
            {
                return mc.p2DIsoSurfaces[0].toPointArray();
            }
            */

           // return null;
        }

        public int RemoveLineRedundant(ref List<Point> points, double tanErr = 1.0E-3)
        {
            if (points.Count < 3) return 0;

            Stack<Point> lists = new Stack<Point>();
            for (int i = points.Count - 1; i >= 0; i--) lists.Push(points[i]);
            
            int removed = 0;
            points.Clear();

            Point p;
            Point p1 = lists.Pop();
            Point p2 = p1;
            
            points.Add(p1);
            
            CLine line = new CLine();
            
            while (lists.Count > 0)
            {
                p = lists.Pop(); // new point
                p2 = points[points.Count - 1];                
                if ( Vector32.IsEqual(new Vector32(p2.X, p2.Y, 0), new Vector32(p.X, p.Y, 0), 1.0E-6))
                {
                    p2 = p1;
                    removed++;
                    continue;
                }

                if (points.Count < 2) 
                {
                    points.Add(p);
                    continue; 
                }

                p1 = points[points.Count - 2];
                
                line.p1 = new Vector64(p1.X, p1.Y,0);
                line.p2 = new Vector64(p2.X, p2.Y,0);

                if( line.IsOnLineExt( new Vector64(p.X,p.Y,0), tanErr) )                
                {
                    //删除前一点
                    points.RemoveAt(points.Count-1);
                    removed++;
                    continue;
                }
                points.Add(p);
            }            
            return removed;
        }

        ///<summary>Sees if a pixel is within the color tolerance range.</summary>
        ///<param name="px">The byte index of the pixel to check, passed by reference to increase performance.</param>
        protected bool CheckPixel(ref int px)
        {
            //tried a 'for' loop but it adds an 8% overhead to the floodfill process
            /*bool ret = true;
            for (byte i = 0; i < 3; i++)
            {
                ret &= (bitmap.Bits[px] >= (startColor[i] - tolerance[i])) && bitmap.Bits[px] <= (startColor[i] + tolerance[i]);
                px++;
            }
            return ret;*/
            
            return (bitmapBits[px] >= (startColor[0] - tolerance[0])) && bitmapBits[px] <= (startColor[0] + tolerance[0]) &&
                (bitmapBits[px + 1] >= (startColor[1] - tolerance[1])) && bitmapBits[px + 1] <= (startColor[1] + tolerance[1]) &&
                (bitmapBits[px + 2] >= (startColor[2] - tolerance[2])) && bitmapBits[px + 2] <= (startColor[2] + tolerance[2]);
        }

        ///<summary>Calculates and returns the byte index for the pixel (x,y).</summary>
        ///<param name="x">The x coordinate of the pixel whose byte index should be returned.</param>
        ///<param name="y">The y coordinate of the pixel whose byte index should be returned.</param>
        protected int CoordsToByteIndex(ref int x, ref int y)
        {
            return (bitmapStride * y) + (x * bitmapPixelFormatSize);
        }

        /// <summary>
        /// Returns the linear index for a pixel, given its x and y coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the pixel.</param>
        /// <param name="y">The y coordinate of the pixel.</param>
        /// <returns></returns>
        protected int CoordsToPixelIndex(int x, int y)
        {
            return (bitmapWidth * y) + x;
        }

    }

    /// <summary>
    /// Represents a linear range to be filled and branched from.
    /// </summary>
    public struct FloodFillRange
    {
        public int StartX;
        public int EndX;
        public int Y;

        public FloodFillRange(int startX, int endX, int y)
        {
            StartX=startX;
            EndX = endX;
            Y = y;
        }
    }
}
