using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCollection;
using GlmNet;
using System.ComponentModel;
using System.Drawing;
using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
using OpenGL;
using DDDSharp;

//slice data class
namespace DataCollection
{
    public enum planEnum
    {
        XOY = 0,
        XOZ = 1,
        YOZ = 2,
    }
    public class CSlicer : CMesh
    {
        public planEnum Plan = planEnum.XOY; // baseline in XOY plan
        public List<Vector64> pBaseLine = new List<Vector64>();
        public List<Vector64> pInterpolatedBaseLine = new List<Vector64>();
        public string headerLine = "Grid Slicer Line 2100"; //标记行（文件第一行）

        vec2[] textureCoords = null; //贴图坐标

        public List<Vector64> BaseLine 
        {
            get 
            {
                if (pInterpolatedBaseLine.Count > 1) return pInterpolatedBaseLine;
                else return pBaseLine;
            }
        }
        //blank
        public double m_BlankValue = C3DData.m_BlankedValue;
        public bool[] pBlankTable = null;

        public bool _Closed = false;
        [CategoryAttribute("Display"), DisplayNameAttribute("Close")]
        public bool Closed 
        {
            get { return _Closed; }
            set { _Closed = value; RenderMode = RenderingUpdateMode.Redraw; }
        }

        public bool _Smoothed = true;
        [CategoryAttribute("Display"), DisplayNameAttribute("Smooth")]
        public bool Smoothed 
        { 
            get { return _Smoothed; } 
            set { _Smoothed = value;} 
        }

        private double depth1 = 0; //minHeight of slicer
        private double depth2 = 0; //maxHeight of slicer        
        [CategoryAttribute("Display"), DisplayNameAttribute("Minimum height")]
        public double minHeight
        {
            get 
            { 
                if( depth1 >= depth2)
                {
                    if (Plan == planEnum.XOY) return minz;
                    else if (Plan == planEnum.XOZ) return miny;
                    else return minx;
                }
                else return depth1; 
            }
            set { depth1 = value; RenderMode = RenderingUpdateMode.Redraw; }
        }
        [CategoryAttribute("Display"), DisplayNameAttribute("Maximum height")]
        public double maxHeight
        {
            get 
            {
                if (depth1 >= depth2)
                {
                    if (Plan == planEnum.XOY) return maxz;
                    else if (Plan == planEnum.XOZ) return maxy;
                    else return maxx;
                }
                else return depth2; 
            }
            set { depth2 = value; RenderMode = RenderingUpdateMode.Redraw; }
        }

        public CSlicer(int rownum = 100, int colnum = 100)
        {
            nRow = rownum;
            nCol = colnum;
            type = ShapeEnum.Slicer;
            pData = new Vector64[nRow * nCol];
            EnableColorLevel = true;
        }

        /// <summary>
        /// 获取贴图坐标，Slicer可能不再是规则网格
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iy"></param>
        /// <returns></returns>
        public override vec2 GetTextureCoord(int ix, int iy)
        {
            if (textureCoords != null)
                return textureCoords[ix + iy * nCol];
            else return base.GetTextureCoord(ix,iy);
        }
        public bool CreateTextureCoords()
        {
            if (BaseLine.Count < 2) return false;
            textureCoords = new vec2[nRow*nCol];
            double len = Vector64.GetLength(pInterpolatedBaseLine);
            double lex = 0;
            for(int i = 0; i < nRow; i++ )
            {
                for(int j = 0; j < nCol; j++ )
                {
                    vec2 tex = new vec2();
                    lex = Vector64.GetLength(pInterpolatedBaseLine,j);
                    tex.x = (float)(lex / len);
                    tex.y = 1 - (float)i / (nRow - 1);
                    textureCoords[j+i*nCol] = tex;
                }
            }
            return true;
        }
        public CSlicer Copy()
        {
            CSlicer slicer = new CSlicer(nRow, nCol);
            slicer.CopyHeaderFrom(this);

            slicer.nRow = nRow;

            slicer.nCol = nCol;
            slicer.ShowMesh = ShowMesh;
            slicer.Name = Name + "_Copy";
            slicer.Plan = Plan;

            slicer.pBaseLine = new List<Vector64>();
            foreach (Vector64 p1 in pBaseLine) slicer.pBaseLine.Add(p1);

            slicer.pInterpolatedBaseLine = new List<Vector64>();
            foreach (Vector64 p1 in pInterpolatedBaseLine) slicer.pInterpolatedBaseLine.Add(p1);

            for (int i = 0; i < pData.Length; i++)
            {
                slicer.pData[i] = new Vector64();
                slicer.pData[i] = pData[i];
            }

            slicer.ColorScale = ColorScale.Copy();
            slicer.Closed = false;
            //contour
            slicer.marchingCube = marchingCube;
            slicer.ShowContour = ShowContour;
            slicer.EnableColorLevel = EnableColorLevel;
            slicer.LineColor = LineColor;
            slicer.LineWidth = LineWidth;
            //blank
            slicer.m_BlankValue = 1.70141E+038;
            slicer.pBlankTable = pBlankTable;

            slicer.depth1 = depth1; //minHeight of slicer
            slicer.depth2 = depth2; //maxHeight of slicer
            return slicer;
        }

        public CSlicer GetInterpolatedSlicer(CSlicer slicer)
        {
            CSlicer s1 = this;
            CSlicer s2 = slicer;
            
            int n1 = s1.pBaseLine.Count;
            int n2 = s2.pBaseLine.Count;
            int n = Math.Min(n1, n2);

            int col1 = s1.nCol;
            int col2 = s2.nCol;
            int col0 = Math.Min(col1, col2);
            int row1 = s1.nRow;
            int row2 = s2.nRow;
            int row0 = Math.Min(row1, row2);

            CSlicer s0 = new CSlicer(row0, col0);
            s0.Name = s1.Name + "_" + s2.Name;

            int id1, id2;
            Vector64 p1, p2, p0;
            double percent = 0;
            for( int i = 0; i < n; i++ )
            {
                percent = (double)i / (n-1);
                id1 = (int)( percent * (n1 - 1) + 0.1);
                id2 = (int)( percent * (n2 - 1) + 0.1);
                p1 = s1.pBaseLine[id1];
                p2 = s2.pBaseLine[id2];
                p0 = (p1 + p2) * 0.5;
                s0.AddBaseLine( p0 );
            }

                       
            s0.pData = new Vector64[row0*col0];
            int ix1, ix2, iy1, iy2;
            for (int i = 0; i < row0; i++)
            {
                percent = (double)i / (row0 - 1); //0 - 1
                iy1 = (int)(percent * (row1 - 1) + 0.1); 
                iy2 = (int)(percent * (row2 - 1) + 0.1);
                for ( int j = 0; j < col0; j++ )
                {
                    ix1 = (int)((double)j / (col0 - 1) * (col1 - 1) + 0.1);
                    ix2 = (int)((double)j / (col0 - 1) * (col2 - 1) + 0.1);
                    p1 = s1.GetPoint(iy1, ix1);
                    p2 = s2.GetPoint(iy2, ix2);
                    p0 = (p1 + p2) / 2;
                    s0.AddPoint(i, j, p0);    
                }
            }
            
            s0.UpdateRange();
            s0.ColorScale.SetValueRange(s0.minv,s0.maxv);
            return s0;
        }


        //用grid 数据替换pData
        public bool SetGridData(C2DGridData cs, bool inverse = false)
        {
            minv = cs.minv;
            maxv = cs.maxv;
            ColorScale.SetValueRange(minv, maxv);

            Vector64 p;
            double ix, iy;
            int nx = cs.xNum;
            int ny = cs.yNum;
            for (int i = 0; i < nRow; i++)
            {
                iy = (double)i / (nRow - 1) * (cs.yNum - 1);
                for (int j = 0; j < nCol; j++)
                {
                    ix = (double)j / (nCol - 1) * (cs.xNum - 1);
                    p = pData[i * nCol + j];
                    if (nx >= nCol && ny >= nRow)//不用插值
                    {
                        p.v = (float)cs.GetGridValue((int)ix, (int)iy);
                    }
                    else
                    {
                        double x = cs.minx + ix / nx * (cs.maxx - cs.minx);
                        double y = cs.minx + ix / nx * (cs.maxx - cs.minx);
                        p.v = (float)cs.GetGridValue(x, y);
                    }
                    pData[i * nCol + j] = p;
                }
            }

            return true;
        }

        public void Trim(double x1, double y1, double z1, double x2, double y2, double z2)
        {

            DoubleRect rect = new DoubleRect();
            if (Plan == planEnum.XOY)
            {
                rect.X1 = x1;
                rect.Y1 = y1;
                rect.X2 = x2;
                rect.Y2 = y2;
            }
            else if (Plan == planEnum.YOZ)
            {
                rect.X1 = y1;
                rect.Y1 = z1;
                rect.X2 = y2;
                rect.Y2 = z2;
            }
            else //if (Plan == planEnum.XOZ)
            {
                rect.X1 = x1;
                rect.Y1 = z1;
                rect.X2 = x2;
                rect.Y2 = z2;
            }
            List<Vector64> lists = new List<Vector64>();

            Vector64 p, p1;
            for (int i = 0; i < pBaseLine.Count; i++)
            {
                p = p1 = toXYPoint(pBaseLine[i]);//当前点p
                //相邻点p1
                if (i > 0) p1 = toXYPoint(pBaseLine[i - 1]);
                else if (i < pBaseLine.Count - 1) p1 = toXYPoint(pBaseLine[i + 1]);

                //均为边界外点，忽略
                if (!rect.Contains(p.x, p.y) && !rect.Contains(p1.x, p1.y)) continue;

                p = pBaseLine[i];
                if (p.x < x1) p.x = (float)x1;
                if (p.y < y1) p.y = (float)y1;
                if (p.z < z1) p.z = (float)z1;
                if (p.x > x2) p.x = (float)x2;
                if (p.y > y2) p.y = (float)y2;
                if (p.z > z2) p.z = (float)z2;
                lists.Add(p);
            }
            pBaseLine.Clear();
            foreach (Vector64 p2 in lists) pBaseLine.Add(p2);
            lists.Clear();
        }
        public void RemoveDuplicated()
        {
            int n = 2;
            while (n > 0)
            {
                n = Vector64.RemoveLineDuplicated(ref pBaseLine);
            }
        }

        //BASE line to xy[]
        public List<Vector64> toXYPoints(List<Vector64> points)
        {
            List<Vector64> xyz = new List<Vector64>();
            for (int i = 0; i < points.Count; i++)
            {
                xyz.Add(toXYPoint(points[i]));
            }
            return xyz;
        }
        public List<Vector32> toXYPoints(List<Vector32> points)
        {
            List<Vector32> xyz = new List<Vector32>();
            for (int i = 0; i < points.Count; i++)
            {
                xyz.Add(toXYPoint(points[i]));
            }
            return xyz;
        }
        /// <summary>
        /// 将空间坐标点转换成二维格式xy
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector32 toXYPoint(Vector32 p)
        {
            if (Plan == planEnum.XOY) //on z top
            {
                return p;
            }
            else if (Plan == planEnum.YOZ) //on y bottom
            {
                return new Vector32(p.y, p.z, p.x);
            }
            else //if (Plan == planEnum.XOZ) //on y bottom
            {
                return new Vector32(p.x, p.z, p.y);
            }
        }
        public Vector64 toXYPoint(Vector64 p)
        {
            if (Plan == planEnum.XOY) //on z top
            {
                return p;
            }
            else if (Plan == planEnum.YOZ) //on y bottom
            {
                return new Vector64(p.y, p.z, p.x);
            }
            else //if (Plan == planEnum.XOZ) //on y bottom
            {
                return new Vector64(p.x, p.z, p.y);
            }
        }
        // xyz array to BaseLine
        public List<Vector32> fromXYZPoints(List<Vector32> points)
        {
            List<Vector32> xyz = new List<Vector32>();
            for (int i = 0; i < points.Count; i++)
            {
                xyz.Add(fromXYZPoint(points[i]));
            }
            return xyz;
        }
        public List<Vector64> fromXYZPoints(List<Vector64> points)
        {
            List<Vector64> xyz = new List<Vector64>();
            for (int i = 0; i < points.Count; i++)
            {
                xyz.Add(fromXYZPoint(points[i]));
            }
            return xyz;
        }
        /// <summary>
        /// 二维点转换成三维点
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector32 fromXYZPoint(Vector32 p)
        {
            if (Plan == planEnum.XOY) //on z top
            {
                return new Vector32(p.x, p.y, p.z);
            }
            else if (Plan == planEnum.YOZ) //on y bottom
            {
                return new Vector32(p.z, p.x, p.y);
            }
            else //if (Plan == planEnum.XOZ) //on y bottom
            {
                return new Vector32(p.x, p.z, p.y);
            }
        }
        public Vector64 fromXYZPoint(Vector64 p)
        {
            if (Plan == planEnum.XOY) //on z top
            {
                return new Vector64(p.x, p.y, p.z);
            }
            else if (Plan == planEnum.YOZ) //on y bottom
            {
                return new Vector64(p.z, p.x, p.y);
            }
            else //if (Plan == planEnum.XOZ) //on y bottom
            {
                return new Vector64(p.x, p.z, p.y);
            }
        }
        public bool CreateSmoothBaseLine()
        {
            List<Vector64> interpolated;
            if (Closed)
            {
                BezierSmooth bz = new BezierSmooth();
                bz.AddPoint(toXYPoints(pBaseLine)); //转换成二维点
                interpolated = bz.Smooth().points;
            }
            else
            {
                CubicSpline spline = new CubicSpline();
                interpolated = spline.CreateSpline(toXYPoints(pBaseLine));
                //Spline spline = new Spline(pBaseLine);
                //interpolated = spline.CreateSpline(false);
            }
            pBaseLine.Clear();
            pBaseLine = fromXYZPoints(interpolated);//转换成三维点
            
            Smoothed = true;

            return true;
        }
        public bool IsZero(double val)
        {
            double Zero = 1.0E-20;
            double v = val;
            if (v < 0) v = -v;
            if (v <= Zero) return true;
            else return false;
        }

        public bool GetLineIntersection(CLine line, out Vector64 p)
        {
            p = new Vector64();
            if (pBaseLine.Count < 2) return false;
            CLine sline = new CLine();
            for (int i = 0; i < pBaseLine.Count - 1; i++)
            {
                sline.p1 = pBaseLine[i];
                sline.p2 = pBaseLine[i + 1];

                if (!sline.GetIntersection(line, out p))
                    continue;

                if (sline.IsOnLine(p)) return true;
            }
            return false;
        }

        /*
        //       y    slicer
        //       3-----/2----2-  
        //       |    /      |
        //     3 |   / 0     |1
        //       0--/--------1-->x
        public Polygon2D RectIntersect(double x1, double y1, double z1, double x2, double y2, double z2, bool left = true)
        {
            int n = pBaseLine.Count;
            if (n < 2) return null;

            CubeModel64 model = new CubeModel64(x1, y1, z1, x2, y2, z2);
            Vector32 b0 = pBaseLine[0];
            Vector32 b1 = pBaseLine[1];
            Vector32 bn1 = pBaseLine[n - 1];
            Vector32 bn2 = pBaseLine[n - 2];

            Vector32 p0 = new Vector32((float)x1, (float)y1, (float)z2);
            Vector32 p1 = new Vector32((float)x2, (float)y1, (float)z2);
            Vector32 p2 = new Vector32((float)x2, (float)y2, (float)z2);
            Vector32 p3 = new Vector32((float)x1, (float)y2, (float)z2);

            Polygon2D polygon = new Polygon2D();
            List<Vector32> ends = new List<Vector32>();

            CLine[] lines = new CLine[4];
            lines[0] = new CLine(p0, p1);
            lines[1] = new CLine(p1, p2);
            lines[2] = new CLine(p2, p3);
            lines[3] = new CLine(p3, p0);

            CLine line1 = new CLine(b0, b1);
            Vector32 p01 = new Vector32();
            int id1 = 0;
            for (int i = 0; i < 4; i++)
            {
                if (!line1.GetIntersectionExt(lines[i], out p01)) continue;
                if (!lines[i].IsOnLine(p01)) continue;
                if (model.IsPointIn(b0))//b1--b0--p01(edge)
                {
                    CLine l1 = new CLine(b1, p01);
                    if (l1.IsOnLine(b0))
                    {
                        polygon.Add(p01);
                        polygon.Add(b0);
                        id1 = i;
                        break;
                    }
                }
                else //b0 -- p01(edge)-- b1
                {
                    CLine l1 = new CLine(b0, b1);
                    if (l1.IsOnLine(p01))
                    {
                        polygon.Add(p01);
                        id1 = i;
                        break;
                    }
                }
            }//for(int i=0;i<4;i++)
            CLine line2 = new CLine(bn2, bn1);
            Vector32 p02 = new Vector32();
            int id2 = 0;
            for (int i = 0; i < 4; i++)
            {
                if (!line2.GetIntersectionExt(lines[i], out p02)) continue;
                if (!lines[i].IsOnLine(p02)) continue;
                if (model.IsPointIn(bn1))//bn2-->bn1-->p02(edge)
                {
                    CLine l1 = new CLine(bn2, p02);
                    if (l1.IsOnLine(bn1))
                    {
                        ends.Add(bn1);
                        ends.Add(p02);
                        id2 = i;
                        break;
                    }
                }
                else //bn2 --> p02(edge)--> bn1
                {
                    CLine l1 = new CLine(bn2, bn1);
                    if (l1.IsOnLine(p02))
                    {
                        ends.Add(p02);
                        id2 = i;
                        break;
                    }
                }
            }//for(int i=0;i<4;i++)
             //       y    slicer
             //       3-----/2----2-  
             //       |    /      |
             //     3 |   / 0     |1
             //       0--/--------1-->x
             //id2 -->id1
            int step = 1;
            if (!left) step = -1;
            Vector32[] pp = new Vector32[] { p0, p1, p2, p3, p0, p1, p2, p3, p0 };
            int id = id2;
            for (; ; )
            {
                if (id == id1) break;
                if (left) ends.Add(pp[id + 1]);
                else ends.Add(pp[id]);

                id = id + step;
                if (id > 3) id = 0;
                if (id < 0) id = 3;
            }
            ///*
            Vector32.RemoveLineDuplicated(ref ends);
            for (int i = 1; i < pBaseLine.Count - 1; i++)
            {
                polygon.Add(pBaseLine[i]);
            }
            for (int i = 0; i < ends.Count; i++)
            {
                polygon.Add(ends[i]);
            }
            lines = null;
            pp = null;
            ends.Clear();
            return polygon;
        }
        */
        /// <summary>
        /// 线与矩形的求交，返回相交多边形（顺时针）
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="z1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="z2"></param>       
        /// <returns>返回相交多边形，null失败</returns>        
        public Polygon2D RectIntersect(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            int n = pBaseLine.Count;
            if (n < 2) return null;

            int id1 = -1;//第1交点边号0-1-2-3
            int id2 = -1;//第2交点边号0-1-2-3

            double maxlen = x2 - x1;
            if (y2 - y1 > maxlen) maxlen = y2 - y1;
            if (z2 - z1 > maxlen) maxlen = z2 - z1;

            Vector64 p1, p2;
            CLine line1 = new CLine();
            CLine line2 = new CLine();

            DoubleRect rect = new DoubleRect(x1 - 1, y1 - 1, x2 + 1, y2 + 1);
            if (Plan == planEnum.XOY) rect = new DoubleRect(x1 - 1, y1 - 1, x2 + 1, y2 + 1);
            else if (Plan == planEnum.YOZ) rect = new DoubleRect(y1 - 1, z1 - 1, y2 + 1, z2 + 1);
            else rect = new DoubleRect(x1 - 1, z1 - 1, x2 + 1, z2 + 1);

            //起始射线line1,反向
            for (int i = 0; i < pBaseLine.Count - 1; i++)
            {
                p2 = toXYPoint(pBaseLine[i]); //转换为2维点
                p1 = toXYPoint(pBaseLine[i + 1]);//转换为2维点
                //均为边界外点
                if (!rect.Contains(p1.x, p1.y) && !rect.Contains(p2.x, p2.y)) continue;
                else
                {
                    line1.p1 = p1;
                    line1.p2 = p2;
                    break;
                }
            }
            //结束射线line2
            for (int i = pBaseLine.Count - 1; i >= 1; i--)
            {
                p2 = toXYPoint(pBaseLine[i]);//转换为2维点
                p1 = toXYPoint(pBaseLine[i - 1]);//转换为2维点
                //均为边界外点
                if (!rect.Contains(p1.x, p1.y) && !rect.Contains(p2.x, p2.y)) continue;
                else
                {
                    line2.p1 = p1;
                    line2.p2 = p2;
                    break;
                }
            }

            //延长线段至边界外
            line1.p2 = line1.p1 + line1.Direction * 10 * maxlen;
            line2.p2 = line2.p1 + line2.Direction * 10 * maxlen;

            Polygon2D polygon = new Polygon2D();

            //判断line1,line2是否自相交
            Vector64 p00 = new Vector64();
            if (line1.GetIntersection(line2, out p00)) //有交点
            {
                for (int i = 0; i < pBaseLine.Count; i++)
                {
                    polygon.Add(toXYPoint(pBaseLine[i]));
                }
                polygon.Add(p00);
                return polygon;
            }
            //       y    XOY slicer
            //       1-----/1----2-  
            //       |    /      |
            //     0 |   /       |2
            //       0--/--3-----3-->x                   
            Vector64[] corners = new Vector64[4];//矩形角点坐标
            if (Plan == planEnum.XOY)
            {
                corners[0] = new Vector64(x1 - 1, y1 - 1, z2);
                corners[1] = new Vector64(x1 - 1, y2 + 1, z2);
                corners[2] = new Vector64(x2 + 1, y2 + 1, z2);
                corners[3] = new Vector64(x2 + 1, y1 - 1, z2);
            }
            ///      z    YOZ slicer
            //       1-----/1----2-  
            //       |    /      |
            //     0 |   /       |2
            //       0--/--3-----3-->y
            //
            else if (Plan == planEnum.YOZ)
            {
                corners[0] = new Vector64(y1 - 1, z1 - 1, x2);
                corners[1] = new Vector64(y1 - 1, z2 + 1, x2);
                corners[2] = new Vector64(y2 + 1, z2 + 1, x2);
                corners[3] = new Vector64(y2 + 1, z1 - 1, x2);
            }
            ///      z    XOZ slicer
            //       1-----/1----2-  
            //       |    /      |
            //     0 |   /       |2
            //   x<--0--/--3-----3
            else //if (Plan == planEnum.XOZ)
            {
                corners[0] = new Vector64(x2 + 1, z1 - 1, y2);
                corners[1] = new Vector64(x2 + 1, z2 + 1, y2);
                corners[2] = new Vector64(x1 - 1, z2 + 1, y2);
                corners[3] = new Vector64(x1 - 1, z1 - 1, y2);
            }

            Vector64 p01 = new Vector64();
            Vector64 p02 = new Vector64();
            CLine line = new CLine();

            //第一个交点 
            for (int i = 0; i < 4; i++)
            {
                line.p1 = corners[i];
                if (i == 3) line.p2 = corners[0];
                else line.p2 = corners[i + 1];
                if (line.GetIntersection(line1, out p01)) //有交点
                {
                    id1 = i;
                    break;
                }
            }//for(int i=0;i<4;i++)

            //第二个交点            
            for (int i = 0; i < 4; i++)
            {
                line.p1 = corners[i];
                if (i == 3) line.p2 = corners[0];
                else line.p2 = corners[i + 1];
                if (line.GetIntersection(line2, out p02)) //有交点
                {
                    id2 = i;
                    break;
                }
            }//for(int i=0;i<4;i++)

            //失败，不知道为什么
            if (id1 == -1 || id2 == -1) return null;

            //组装多边形点，按顺时针方向
            polygon.Add(p01);
            for (int i = 0; i < pBaseLine.Count; i++)
            {
                polygon.Add(toXYPoint(pBaseLine[i]));
            }
            polygon.Add(p02);

            for (int i = id2; i != id1;)
            {
                if (i >= 3)
                {
                    p1 = corners[i];
                    p2 = corners[0];
                    i = 0;
                }
                else
                {
                    p1 = corners[i];
                    p2 = corners[i + 1];
                    i++;
                }
                polygon.Add(p2);
            }

            //去除线上的重复点
            Vector64.RemoveLineDuplicated(ref polygon.points, 1.0E-10);

            return polygon;
        }

        public PointLineRelation CheckPointPosition(Vector32 p)
        {
            return CheckPointPosition(p.x, p.y, p.z);
        }
        public PointLineRelation CheckPointPosition(double x, double y, double z)
        {
            double dx = maxx - minx;
            double dy = maxy - miny;

            if (Plan == planEnum.XOY)
            {
                Vector64 p0;
                Vector32 p = new Vector32((float)x, (float)y, (float)z);
                Vector32 p1 = new Vector32((float)x, (float)y, (float)z);
                Vector32 p2 = new Vector32((float)(minx - dx), (float)y, (float)z);
                CLine line1 = new CLine(p1, p2);
                CLine line2 = new CLine(p1, p2);
                for (int i = 0; i < pBaseLine.Count - 1; i++)
                {
                    line2.p1 = pBaseLine[i];
                    line2.p2 = pBaseLine[i + 1];
                    if (line2.GetIntersection(line1, out p0))
                    {
                        if (line2.IsOnLine(p0))
                            return line2.GetPointReletion(p);
                    }
                }
                line1.p2 = new Vector64(x, miny - dy, z);
                for (int i = 0; i < pBaseLine.Count - 1; i++)
                {
                    line2.p1 = pBaseLine[i];
                    line2.p2 = pBaseLine[i + 1];
                    if (line2.GetIntersection(line1, out p0))
                    {
                        if (line2.IsOnLine(p0))
                            return line2.GetPointReletion(p);
                    }
                }
            }
            return PointLineRelation.None;
        }
        /*
        public PointLineRelation CheckPointPosition(double x,double y,double z)
        {
            Vector32 p1, p2,p0;
            PointLineRelation relation = PointLineRelation.Left;
            CLine line = new CLine();
            if (Plan == planEnum.XOY)
            {  
                Vector32 p = new Vector32((float)x, (float)y, (float)z);
                for ( int i=0;i<pBaseLine.Count-1;i++ )
                {
                    line.p1 = pBaseLine[i];
                    line.p2 = pBaseLine[i+1];

                    relation = line.GetPointReletion(p);

                    if (relation == PointLineRelation.OnLine) return PointLineRelation.OnLine;
                    else if (relation == PointLineRelation.Right || relation == PointLineRelation.Left)
                    {
                        p0 = line.GetPointProjection(p);
                        if (line.IsOnLine(p0))//p0垂足而且在此线段之间
                            return relation;                        
                    }
                    else if (relation == PointLineRelation.OnLineExtend )
                    {

                    }
                    else //none
                    {

                    }
                }//
                //no return,p is out of the range
                line.p1 = pBaseLine[0];
                line.p2 = pBaseLine[pBaseLine.Count - 1];                
                return line.GetPointReletion(p);
            }
            return relation;
        }
        */
        //to check the point is between min and max height
        public bool IsValidHeight(Vector32 p)
        {
            return IsValidMinAndMaxHeight(p.x, p.y, p.z);
        }
        public bool IsValidMinAndMaxHeight(double x, double y, double z)
        {
            //截断高度未设置
            if (depth1 >= depth2) return true;

            if (Plan == planEnum.XOY)
            {
                if (z < depth1 || z > depth2) return false;
                else return true;
            }
            else if (Plan == planEnum.XOZ)
            {
                if (y < depth1 || y > depth2) return false;
                else return true;
            }
            else //if (Plan == planEnum.YOZ)
            {
                if (x < depth1 || x > depth2) return false;
                else return true;
            }
        }
        public override bool SaveAs(BinaryWriter br)
        {
            if ( !SaveObjHeader(br) ) return false;
            try
            {
                br.Write(nRow);
                br.Write(nCol);
                br.Write((Int16)Plan);

                br.Write(depth1);
                br.Write(depth2);
                br.Write(LineWidth);
                
                br.Write(ShowMesh);
                br.Write(ShowContour);
                br.Write(EnableColorLevel);
                br.Write(Closed);

                C3DData.SaveColor(br, LineColor);
                ColorScale.WriteBinary(br);
                marchingCube.SaveBinary(br);

                //pData
                int n = 0;
                if (pData != null) n = pData.Length;
                br.Write(pData.Length);
                if (pData != null)
                {                    
                    foreach (Vector64 p in pData)
                    {
                        br.Write(p.x);
                        br.Write(p.y);
                        br.Write(p.z);
                        br.Write(p.v);
                    }
                }
                //pBaseLine
                br.Write(pBaseLine.Count);
                foreach (Vector64 p in pBaseLine)
                {
                    br.Write(p.x);
                    br.Write(p.y);
                    br.Write(p.z);
                }

                //pInterpolatedBaseLine
                br.Write(pInterpolatedBaseLine.Count);
                foreach(Vector64 p in pInterpolatedBaseLine)
                {
                    br.Write(p.x);
                    br.Write(p.y);
                    br.Write(p.z);
                }                
                
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public override bool LoadFrom(BinaryReader br)
        {
            if (!LoadObjHeader(br)) return false;
            double x, y, z, v;
            try
            {
                Clear();
                nRow = br.ReadInt32();
                nCol = br.ReadInt32();
                Plan = (planEnum)br.ReadInt16();

                depth1 = br.ReadDouble();
                depth2 = br.ReadDouble();
                LineWidth = br.ReadSingle();
                
                ShowMesh = br.ReadBoolean();
                ShowContour = br.ReadBoolean();
                EnableColorLevel = br.ReadBoolean();
                Closed = br.ReadBoolean();

                LineColor = C3DData.LoadColor(br);

                ColorScale.LoadBinary(br);
                marchingCube.LoadBinary(br);
                marchingCube.mesh = this;
                marchingCube.colorScale = ColorScale;

                //pData
                int n = br.ReadInt32();                
                if (n > 0)
                {
                    pData = new Vector64[nRow * nCol];
                    for (int i = 0; i < pData.Length; i++)
                    {
                        x = br.ReadDouble();
                        y = br.ReadDouble();
                        z = br.ReadDouble();
                        v = br.ReadDouble();
                        pData[i] = new Vector64(x, y, z, v);
                    }
                }                
                //pBaseLine
                n = br.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    x = br.ReadDouble();
                    y = br.ReadDouble();
                    z = br.ReadDouble();
                    pBaseLine.Add(new Vector64(x, y, z));
                }
                //pInterpolatedBaseLine
                n = br.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    x = br.ReadDouble();
                    y = br.ReadDouble();
                    z = br.ReadDouble();
                    pInterpolatedBaseLine.Add(new Vector64(x, y, z));
                }
                
                UpdateRange();

                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }        

        public override void Clear()
        {
            nRow = nCol = 0;
            pBaseLine.Clear();
            pInterpolatedBaseLine.Clear();
            pData = null;
            marchingCube.Clear();
        }
        public void DoMarchingCube(double val)
        {
            marchingCube.Clear();
            marchingCube.SetData(this);
            marchingCube.DoSearchSurface(val);
        }
        /// <summary>
        /// 将线段进行均分
        /// pBaseLine-均分前的线段点数组
        /// pInterpolatedBaseLine-均分后的线段点数组
        /// </summary>
        /// <param name="step">均分最小步长</param>
        public void CreateInterpolatedBaseline(double step)
        {
            pInterpolatedBaseLine.Clear();
            if ( pBaseLine.Count < 2 ) return;
            Vector64 p1, p2;
            double dist;
            double x, y, z;
            p1 = pBaseLine[0];
            pInterpolatedBaseLine.Add(p1);
            for (int i = 1; i < pBaseLine.Count; i++)
            {
                p2 = pBaseLine[i];
                dist = p2.Distance(p1);
                if (dist > step)
                {
                    int nd = (int)(dist / step);                    
                    for (int j = 1; j < nd; j++)
                    {
                        x = p1.X + (p2.X - p1.X) * (double)j / nd;
                        y = p1.Y + (p2.Y - p1.Y) * (double)j / nd;
                        z = p1.Z + (p2.Z - p1.Z) * (double)j / nd;
                        pInterpolatedBaseLine.Add(new Vector64(x, y, z));
                    }
                }
                pInterpolatedBaseLine.Add(p2);
                p1 = pBaseLine[i];
            }
        }
        public override void Normalize()
        {
            UpdateRange();
            for (int i = 0; i < pData.Length; i++)
            {
                pData[i] = TransformedPoint(pData[i]);
            }

            scale = new vec3(1, 1, 1);
            offset = new vec3(0, 0, 0);
            rotate = new vec3(0, 0, 0);

            UpdateRange();
        }
        
        public double GetGridX(int ix, int iy)
        {
            int id = ix + iy * nCol;
            return pData[id].X;
            /*
            int n = pInterpolatedBaseLine.Count;
            if (n < 1)
            {
                double step = (maxx - minx) / (nCol - 1);
                return minx + ix * step;
            }
            return pInterpolatedBaseLine[ix].X;
            */
        }      
        
        public bool CreateSlicer(planEnum plan, C3DGridData data)
        {
            if (data.xNum < 2 || data.yNum < 2 || data.zNum < 2) return false;
            if (pBaseLine.Count < 2) return false;

            Visible = true;
            offset = data.offset;
            rotate = data.rotate;
            scale = data.scale;
            ColorScale = data.ColorScale;
            minv = data.minv;
            maxv = data.maxv;
            m_BlankValue = data.m_blankvalue;

            double step;
            if (plan == planEnum.XOY) //xy
            {
                //1/2网格间接
                step = Math.Sqrt(data.xStep * data.xStep + data.yStep * data.yStep) / 2.0;
                CreateInterpolatedBaseline(step);

                m_BlankValue = data.m_blankvalue;
                nRow = data.zNum;
                nCol = pInterpolatedBaseLine.Count;
                pData = new Vector64[nRow * nCol];

                pBlankTable = new bool[nRow * nCol];
                for (int i = 0; i < nRow * nCol; i++)
                    pBlankTable[i] = false;

                return CreateXOYSlicer(data);
            }
            if (plan == planEnum.XOZ)//xz
            {
                step = Math.Sqrt(data.xStep * data.xStep + data.zStep * data.zStep) / 2.0;
                CreateInterpolatedBaseline(step);
                m_BlankValue = data.m_blankvalue;
                nRow = data.yNum;
                nCol = pInterpolatedBaseLine.Count;
                pData = new Vector64[nRow * nCol];
                pBlankTable = new bool[nRow * nCol];
                for (int i = 0; i < nRow * nCol; i++)
                    pBlankTable[i] = false;

                return CreateXOZSlicer(data);
            }
            if (plan == planEnum.YOZ)//yz
            {
                step = Math.Sqrt(data.yStep * data.yStep + data.zStep * data.zStep) / 2.0;
                CreateInterpolatedBaseline(step);

                m_BlankValue = data.m_blankvalue;
                nRow = data.xNum;
                nCol = pInterpolatedBaseLine.Count;
                pData = new Vector64[nRow * nCol];
                pBlankTable = new bool[nRow * nCol];
                for (int i = 0; i < nRow * nCol; i++)
                    pBlankTable[i] = false;

                return CreateYOZSlicer(data);
            }

            ColorScale = data.ColorScale.Copy();

            return false;
        }
        /// <summary>
        /// 创建XOY平面内的垂直切片
        /// </summary>
        /// <param name="data">三维网格数据体</param>
        /// <returns>是否成功</returns>
        public bool CreateXOYSlicer(C3DGridData data)
        {
            double x, y, z, v;
            Vector32 p;
            nRow = data.zNum;
            nCol = pInterpolatedBaseLine.Count;
            Plan = planEnum.XOY;
            minv = data.minv;
            maxv = data.maxv;
            minHeight = data.minz;
            maxHeight = data.maxz;
            m_BlankValue = data.m_blankvalue;
            pData = new Vector64[nRow * nCol];
            for (int iz = 0; iz < nRow; iz++)
            {
                z = data.minz + iz * data.zStep;
                for (int i = 0; i < nCol; i++)
                {
                    p = pInterpolatedBaseLine[i];
                    x = p.x;
                    y = p.y;
                    //grid in blanked area
                    if (data.IsInBlankArea(x, y, z))
                    {
                        v = m_BlankValue;
                        pBlankTable[iz * nCol + i] = true;
                    }
                    else
                    {
                        v = data.GetGridValue(x, y, z);
                        pBlankTable[iz * nCol + i] = false;
                    }
                    SetPoint(iz, i, x, y, z, v);
                }
            }
            UpdateRange();

            return true;
        }
        //XOY baseline on XOY plan
        public bool CreateGridSlicer(float[] grids, int nrow, int ncol, double x1, double y1, double x2, double y2)
        {
            nRow = nrow;
            nCol = ncol;

            long id;
            double x, y;
            double xstep = (x2 - x1) / (nCol - 1);
            double ystep = (y2 - y1) / (nRow - 1);

            minHeight = y1;
            maxHeight = y2;
            Plan = planEnum.XOY;

            for (int iy = 0; iy < nRow; iy++)
            {
                y = y2 - ystep * iy;
                for (int ix = 0; ix < nCol; ix++)
                {
                    x = x1 + xstep * ix;
                    id = ix + iy * nCol;
                    SetPoint(iy, ix, x, y, 0, grids[id]);
                }
            }
            UpdateRange();
            return true;
        }

        public bool CreateXOZSlicer(C3DGridData data)
        {
            double x, y, z, v;
            Vector32 p;

            m_BlankValue = data.m_blankvalue;
            nRow = data.yNum;
            nCol = pInterpolatedBaseLine.Count;
            minv = data.minv;
            maxv = data.maxv;
            minHeight = data.miny;
            maxHeight = data.maxy;
            Plan = planEnum.XOZ;
            pData = new Vector64[nRow * nCol];
            for (int iy = 0; iy < data.yNum; iy++)
            {
                y = data.miny + iy * data.yStep;
                for (int i = 0; i < pInterpolatedBaseLine.Count; i++)
                {
                    p = pInterpolatedBaseLine[i];
                    x = p.x;
                    z = p.z;
                    //grid in blanked area
                    if (data.IsInBlankArea(x, y, z))
                    {
                        v = m_BlankValue;
                        pBlankTable[iy * nCol + i] = true;
                    }
                    else
                    {
                        v = data.GetGridValue(x, y, z);
                        pBlankTable[iy * nCol + i] = false;
                    }
                    SetPoint(iy, i, x, y, z, v);
                }
            }
            UpdateRange();

            return true;
        }
        public bool CreateYOZSlicer(C3DGridData data)
        {
            double x, y, z, v;
            Vector32 p;
            nRow = data.xNum;
            nCol = pInterpolatedBaseLine.Count;
            minv = data.minv;
            maxv = data.maxv;
            minHeight = data.minx;
            maxHeight = data.maxx;
            Plan = planEnum.YOZ;
            for (int ix = 0; ix < data.xNum; ix++)
            {
                x = data.minx + ix * data.xStep;
                for (int i = 0; i < pInterpolatedBaseLine.Count; i++)
                {
                    p = pInterpolatedBaseLine[i];
                    z = p.z;
                    y = p.y;
                    v = data.GetGridValue(x, y, z);
                    SetPoint(ix, i, x, y, z, v);
                }
            }
            UpdateRange();

            return true;
        }

        public void AddBaseLine(Vector64 p)
        {
            pBaseLine.Add(p);
        }
        public void AddBaseLine(double x, double y, double z)
        {
            pBaseLine.Add(new Vector64(x, y, z));
        }


        public void SetColorScaleRange(double v1, double v2)
        {
            ColorScale.SetValueRange(v1, v2);
        }        
        public bool SeekSection(string section, ref StreamReader sr,bool frombegin = true)
        {
            if ( frombegin )
            {
                sr.BaseStream.Seek(0, SeekOrigin.Begin);
                sr.DiscardBufferedData();
            }

            string str;
            while ((str = sr.ReadLine()) != null)
            {
                str = str.Trim(' ');
                if (str.Length < 1) continue;
                if (str == section) return true;
            }
            return false;
        }
       
        /// <summary>
        /// 平面与平面的相交
        /// </summary>
        /// <param name="slicer1"></param>
        /// <returns>另一条线</returns>
        public virtual C3DLine[] CreateIntersectionLines(CSlicer slicer1)
        {
            Vector64 p;
            Vector64 p1, p2;
            C3DLine[] lines = null;

            //line and line intersection
            if ( Plan == slicer1.Plan ) //垂直于同一个平面
            {
                int n1 = BaseLine.Count;
                int n2 = slicer1.BaseLine.Count;

                double x, y, z,v, xstep = 0, ystep = 0, zstep = 0;
                
                if (Plan == planEnum.XOY) zstep = (maxz - minz) / (nRow - 1);
                else if (Plan == planEnum.XOZ) ystep = (maxy - miny) / (nRow - 1);
                else if (Plan == planEnum.YOZ) xstep = (maxx - minx) / (nRow - 1);

                Vector64 p3, p4;
                List<Vector64> intersections = new List<Vector64>();

                for (int i = 0; i < n1 - 1; i++)
                {
                    p1 = TransformedPoint(BaseLine[i]);
                    p2 = TransformedPoint(BaseLine[i + 1]);
                    CLine l1 = new CLine(p1, p2);
                    for (int j = 0; j < n2 - 1; j++)
                    {
                        p3 = slicer1.TransformedPoint(slicer1.BaseLine[j]);
                        p4 = slicer1.TransformedPoint(slicer1.BaseLine[j + 1]);
                        CLine l2 = new CLine(p3, p4);
                        if (l1.GetIntersection(l2, out p)) 
                        {
                            p.V = j; //V表示grid对象的列编号
                            intersections.Add(p); 
                        }
                    }
                }

                lines = new C3DLine[intersections.Count];
                for (int i = 0; i < intersections.Count; i++)
                {
                    lines[i] = new C3DLine();
                    lines[i].Name = Name + "_" + slicer1.Name + "_" + (i + 1);
                    p = intersections[i];
                    x = p.x;
                    y = p.y;
                    z = p.z;
                    for (int j = 0; j < nRow; j++)
                    {
                        if (Plan == planEnum.XOY) z = minz + j * zstep;
                        else if (Plan == planEnum.YOZ) x = minx + j * xstep;
                        else if (Plan == planEnum.XOZ) y = miny + j * ystep;
                        //获得点的（颜色）V值
                        v = slicer1.GetPoint(j, (int)p.V).V;
                        lines[i].AddPoint(x, y, z,v);
                    }
                    lines[i].UpdateRange();
                    lines[i].minv = slicer1.minv;
                    lines[i].maxv = slicer1.maxv;
                    lines[i].ColorScale.SetValueRange(slicer1.minv, slicer1.maxv);                    
                }                
                return lines;
            } //if ( Plan == slicer1.Plan ) //垂直于同一个平面
            else  //不垂直于同一个平面
            {
                lines = new C3DLine[1];
                lines[0] = new C3DLine();
                lines[0].Name = Name + "_" + slicer1.Name;

                Vector64 p0;
                int n = BaseLine.Count;

                //slicer and slicer intersect
                for (int i = 0; i < n; i++)
                {
                    p = BaseLine[i];
                    p1 = p2 = p;
                    if (Plan == planEnum.XOY)   //xy - z1z2
                    {
                        p1.z = (float)minz;
                        p2.z = (float)maxz;
                    }
                    else if (Plan == planEnum.YOZ) //yz - x1x2
                    {
                        p1.x = (float)minx;
                        p2.x = (float)maxx;
                    }
                    else //if (Plan == planEnum.XOZ) //xz - y1y2
                    {
                        p1.y = (float)miny;
                        p2.y = (float)maxy;
                    }
                    if (slicer1.GetInterSection(p1, p2, out p0))
                        lines[0].AddPoint(p0);
                }//for (int i = 0; i < n; i++)

                lines[0].UpdateRange();
                lines[0].minv = slicer1.minv;
                lines[0].maxv = slicer1.maxv;
                lines[0].ColorScale.SetValueRange(slicer1.minv, slicer1.maxv);

                return lines;
            }            
        }
        /// <summary>
        /// 切面与平面相交
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public override C3DLine[] CreateIntersectionLines(CMesh mesh)
        {
            C3DLine[] lines = new C3DLine[1];
            lines[0] = new C3DLine();
            lines[0].Name = Name + "_" + mesh.Name;
            Vector64 p, p1, p2, p0;

            int n = BaseLine.Count;

            for (int i = 0; i < n; i++)
            {
                p = BaseLine[i];
                p1 = p2 = p;
                if (Plan == planEnum.XOY)   //xy - z1z2
                {
                    p1.z = (float)minz;
                    p2.z = (float)maxz;
                }
                else if (Plan == planEnum.YOZ) //yz - x1x2
                {
                    p1.x = (float)minx;
                    p2.x = (float)maxx;
                }
                else //if (Plan == planEnum.XOZ) //xz - y1y2
                {
                    p1.y = (float)miny;
                    p2.y = (float)maxy;
                }
                //有交点坐标
                if (mesh.GetInterSection(p1, p2, out p0))
                { 
                    lines[0].AddPoint(p0); 
                }
            }

            lines[0].UpdateRange();
            lines[0].minv = mesh.minv;
            lines[0].maxv = mesh.maxv;
            lines[0].ColorScale.SetValueRange(mesh.minv, mesh.maxv);
            
            return lines;
        }
        
        public bool FromSurferGrid(CSurferGrid cs, double ypos = 0)
        {
            //   / y
            //  x -->
            nRow = cs.yGrid;
            nCol = cs.xGrid;
            Plan = planEnum.XOY;

            minv = cs.minv;
            maxv = cs.maxv;

            m_BlankValue = CSurferGrid.blankValue;

            pData = new Vector64[nRow * nCol];
            pBaseLine.Clear();
            double x, y, z, val = 0;
            y = ypos;
            for (int i = 0; i < nRow; i++)
            {
                z = cs.miny + i * cs.yStep;
                for (int j = 0; j < nCol; j++)
                {
                    x = cs.minx + j * cs.xStep;
                    val = cs.GetZValue(j, i);
                    SetPoint(i, j, x, y, z, val);
                    if (i == 0) AddBaseLine(x, y, z);
                }
            }
            UpdateRange();
            minv = cs.minv;
            maxv = cs.maxv;
            minHeight = cs.miny;
            maxHeight = cs.maxy;
            ColorScale = new CColorScale(minv, maxv);
            return true;
        }

        public bool LoadSlicer(string file)
        {
            return ImportData(file);
        }

        public override bool SaveAs(string path,int version = 0)
        {
            return ExportSlicer(path);
        }
        public override bool ExportData(string path)
        {
            return ExportSlicer(path);
        }
        public override bool ImportData(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open);
                StreamReader sr = new StreamReader(fs); 

                string header1 = "Grid Slicer Line"; //version 2000
                string header2 = headerLine; // "Grid Slicer Line 2100"; //version 2100

                int version = 2100;
                string str = sr.ReadLine();
                str = str.Trim().ToLower();
                if (str == header1.ToLower()) version = 2000;
                else if (str == header2.ToLower()) version = 2100;
                else
                {
                    errMessage = "not a slicer file.";
                    sr.Close();
                    fs.Close();
                    return false;
                }

                Clear();

                SeekSection("[HEADER]", ref sr);
                //name = 
                str = sr.ReadLine();
                Name = GetLineValue(str, "name");

                str = sr.ReadLine();

                Plan = (planEnum)Convert.ToInt32(Enum.Parse(typeof(planEnum), GetLineValue(str, "Plan")));

                str = sr.ReadLine();
                minv = double.Parse(GetLineValue(str, "MinValue"));
                str = sr.ReadLine();
                maxv = double.Parse(GetLineValue(str, "MaxValue"));

                str = sr.ReadLine();
                depth1 = double.Parse(GetLineValue(str, "MinHeight"));
                str = sr.ReadLine();
                depth2 = double.Parse(GetLineValue(str, "MaxHeight"));
                
                SeekSection("[BASE LINE]", ref sr);
                
                str = sr.ReadLine();
                int n = Convert.ToInt32(GetLineValue(str, "Count"));
                if (version >= 2100)
                {
                    str = sr.ReadLine();
                    Closed = Convert.ToBoolean(GetLineValue(str, "Closed"));
                }

                //load base line
                for (int i = 0; i < n && (str = sr.ReadLine()) != null; i++)
                {
                    pBaseLine.Add(Vector64.Parse(str, 3));
                }

                if (pBaseLine.Count < 2)
                {
                    errMessage = "read baseline data error.";
                    sr.Close();
                    fs.Close();
                    Clear();
                    return false;
                }

                SeekSection("[GRID]", ref sr);
                str = sr.ReadLine();
                nRow = int.Parse(GetLineValue(str, "Row"));
                str = sr.ReadLine();
                nCol = int.Parse(GetLineValue(str, "Column"));
                str = sr.ReadLine();
                m_BlankValue = double.Parse(GetLineValue(str, "BlankValue"));

                pData = new Vector64[nRow * nCol];

                for (int i = 0; i < nRow; i++)
                for (int j = 0; j < nCol; j++)
                {   
                  SetPoint(i, j, Vector64.Parse(sr.ReadLine(), 4));
                }

                if (SeekSection("[ColorMap]", ref sr))
                {
                    ColorScale.ReadStream(ref sr);
                }
                if (SeekSection("[CONTOUR]", ref sr))
                {
                    LoadContourLines(ref sr);
                }
                if (SeekSection("[TRANSLATIONS]", ref sr))
                {
                    ImportTranslations(sr);
                }
                else
                {
                    double v1 = minv;
                    double v2 = maxv;
                    UpdateRange();
                    minv = v1;
                    maxv = v2;
                }

                sr.Close();
                fs.Close();
                UpdateRange();
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public bool ExportSlicer(string file)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Create);
                StreamWriter wr = new StreamWriter(fs);
                
                wr.WriteLine(headerLine);//version 2021-00

                string str = "[HEADER]";
                wr.WriteLine(str);
                str = "Name = " + Name;
                wr.WriteLine(str);
                str = "Plan = " + Plan.ToString();
                wr.WriteLine(str);
                str = "MinValue = " + minv;
                wr.WriteLine(str);
                str = "MaxValue = " + maxv;
                wr.WriteLine(str);
                str = "MinHeight = " + minHeight;
                wr.WriteLine(str);
                str = "MaxHeight = " + maxHeight;
                wr.WriteLine(str);

                str = "[BASE LINE]";
                wr.WriteLine(str);
                str = "Count = " + pBaseLine.Count;
                wr.WriteLine(str);
                str = "Closed = " + Closed.ToString(); //new added 2100
                wr.WriteLine(str);

                for (int i = 0; i < pBaseLine.Count; i++)
                {
                    str = pBaseLine[i].X + "," + pBaseLine[i].Y + "," + pBaseLine[i].Z;
                    wr.WriteLine(str);
                }

                str = "[GRID]";
                wr.WriteLine(str);
                str = "Row = " + nRow;
                wr.WriteLine(str);
                str = "Column = " + nCol;
                wr.WriteLine(str);
                str = "BlankValue = " + m_BlankValue;
                wr.WriteLine(str);

                Vector32 p;
                for (int i = 0; i < nRow; i++)
                    for (int j = 0; j < nCol; j++)
                    {
                        p = pData[i * nCol + j];
                        str = p.toString(4);
                        wr.WriteLine(str);
                    }

                ColorScale.WriteStream(wr);
                ExportContourLines(ref wr);
                ExportTranslations(wr);

                str = "[END]";
                wr.WriteLine(str);

                wr.Close();
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public bool ExportContourLinesToDXF(string dxfFilename)
        {
            int id1, id2, n;
            FLOAT_POINT p;
            C2DISOSurface sf;
            List<int> indices = new List<int>();
            List<Vector64> points = new List<Vector64>();

            DxfFileObj dxfObj = new DxfFileObj();
            DxfFile dxffile = new DxfFile();
            for (int i = 0; i < marchingCube.p2DIsoSurfaces.Count; i++)
            {
                sf = marchingCube.p2DIsoSurfaces[i];
                for (int j = 0; j < sf.pLineIndex.Count / 2; j++)
                {
                    id1 = sf.pLineIndex[2 * j];
                    //n = indices.Count;
                    //if ( n < 1 || indices[n - 1] != id1 ) indices.Add(id1);

                    id2 = sf.pLineIndex[2 * j + 1];
                    //n = indices.Count;
                    //if (n < 1 || indices[n - 1] != id2) indices.Add(id2);

                    indices.Add(id1);
                    indices.Add(id2);
                }

                for (int j = 0; j < indices.Count; j++)
                {
                    p = sf.pCoordArray[indices[j]];
                    points.Add(new Vector64(p.x, p.y, p.z));
                }
                if (points.Count > 1)
                {
                    dxffile.Entities.Add(dxfObj.CreateDxfPolyline(points));
                }

                points.Clear();
                indices.Clear();
            }

            dxffile.Save(dxfFilename);

            return true;
        }
        //p点距离BaseLine开始点的距离
        double GetDistance(double x, double y, double z = 0)
        {
            if (pBaseLine.Count < 2) return 0;
            double sum = 0;
            //搜索p点所在位置
            Vector64 p1 = pBaseLine[0];
            Vector64 p2 = pBaseLine[0];
            Vector64 p = new Vector64(x, y, z);
            for (int i = 1; i < pBaseLine.Count; i++)
            {
                p2 = pBaseLine[i];
                p.z = p1.z = p2.z = 0;
                if (Vector64.IsPointOnLine(p, p1, p2))
                {
                    sum += p.Distance(p1);
                    break;
                }
                else sum += p1.Distance(p2);
            }
            return sum;
        }
        /// <summary>
        /// 按二维剖面格式输出
        /// X,沿pBaseLine的剖面长度
        /// Y，高程
        /// Z = 0
        /// </summary>
        /// <param name="dxfFilename"></param>
        /// <returns></returns>
        public bool ExportContourLinesToDXF2D(string dxfFilename)
        {
            int id1, id2, n;
            FLOAT_POINT p;
            C2DISOSurface sf;
            List<int> indices = new List<int>();
            List<Vector64> points = new List<Vector64>();

            DxfFileObj dxfObj = new DxfFileObj();
            DxfFile dxffile = new DxfFile();
            for (int i = 0; i < marchingCube.p2DIsoSurfaces.Count; i++)
            {
                sf = marchingCube.p2DIsoSurfaces[i];
                for (int j = 0; j < sf.pLineIndex.Count / 2; j++)
                {
                    id1 = sf.pLineIndex[2 * j];
                    //n = indices.Count;
                    //if ( n < 1 || indices[n - 1] != id1 ) indices.Add(id1);

                    id2 = sf.pLineIndex[2 * j + 1];
                    //n = indices.Count;
                    //if (n < 1 || indices[n - 1] != id2) indices.Add(id2);

                    indices.Add(id1);
                    indices.Add(id2);
                }

                float x, y, z;
                for (int j = 0; j < indices.Count; j++)
                {
                    p = sf.pCoordArray[indices[j]];
                    // x = GetDistance();
                    points.Add(new Vector64(p.x, p.y, p.z));
                }
                if (points.Count > 1)
                {
                    dxffile.Entities.Add(dxfObj.CreateDxfPolyline(points));
                }

                points.Clear();
                indices.Clear();
            }

            dxffile.Save(dxfFilename);

            return true;
        }

        //剖面长度
        public double SlicerLength
        {
            get
            {
                if (pBaseLine.Count < 2) return 0;
                double len = 0;
                Vector64 p1 = pBaseLine[0], p2;
                for (int i = 1; i < pBaseLine.Count; i++)
                {
                    p2 = pBaseLine[i];
                    len += p2.Distance(p1);
                    p1 = p2;
                }
                return len;
            }
        }
        //网格切片转换成PolygonSlicer
        public PolygonSlicer toPolygonSlicer()
        {
            //剖面长度
            double x1 = 0;
            double x2 = SlicerLength;

            //剖面深度
            double y1 = minHeight;
            double y2 = maxHeight;

            PolygonSlicer slicer = new PolygonSlicer();
            slicer.Name = Name + "_slicer";

            if (Plan == planEnum.XOY) slicer.axis = AxisEnum.zAxis;
            if (Plan == planEnum.XOZ) slicer.axis = AxisEnum.yAxis;
            if (Plan == planEnum.YOZ) slicer.axis = AxisEnum.xAxis;

            slicer.minx = x1;
            slicer.maxx = x2;
            slicer.miny = y1;
            slicer.maxy = y2;
            slicer.minz = y1;
            slicer.maxz = y2;

            Vector64 p1 = pBaseLine[0];
            Vector64 p2 = pBaseLine[pBaseLine.Count - 1];
            if (Plan == planEnum.XOY)
            {
                p1.Z = minHeight;
                p2.Z = maxHeight;
            }
            if (Plan == planEnum.XOZ)
            {
                p1.Y = minHeight;
                p2.Y = maxHeight;
            }
            if (Plan == planEnum.YOZ)
            {
                p1.X = minHeight;
                p2.X = maxHeight;
            }

            double x = 0;
            Vector64 v1 = pBaseLine[0], v2;
            //add locations---多个瞄点坐标转换有问题，取消中间节点的瞄点2025-9-19
            for (int i = 0; i < pBaseLine.Count; i++)
            {
                v2 = pBaseLine[i];
                x += v2.Distance(v1);

                if (i == 0) slicer.AddLocationPoint(new Vector64(x, minHeight, 0), p1);
                else if (i == pBaseLine.Count - 1) slicer.AddLocationPoint(new Vector64(x, maxHeight, 0), p2);
                //else slicer.AddLocationPoint(new Vector64(x, maxHeight, 0), pBaseLine[i]);
                v1 = pBaseLine[i];
            }

            slicer.UpdateTraced();

            return slicer;
        }

        //Slicer转换成图像文件
        public ImageStruct toImage()
        {
            //剖面长度
            double x1 = 0;
            double x2 = SlicerLength;
            //剖面深度
            double y1 = minHeight;
            double y2 = maxHeight;
            if( y1 >= y2 )
            {
                if (Plan == planEnum.XOY) { y1 = Minz;y2 = Maxz; }
                if (Plan == planEnum.XOZ) { y1 = Miny; y2 = Maxy; }
                if (Plan == planEnum.YOZ) { y1 = Minx; y2 = Maxx; }
            }
            ImageStruct im = new ImageStruct();
            im.rect = new DoubleRect(x1, y1, x2, y2);

            double width = Math.Abs(x2 - x1);
            double height = Math.Abs(y2 - y1);
            double ww = width;
            if (height > ww) ww = height;

            width = 1024 * width / ww;
            height = 1024 * height / ww;
            Bitmap bmp = new Bitmap((int)width, (int)height);

            Graphics g = Graphics.FromImage(bmp);

            double xstep = width / (nCol - 1);
            double ystep = height / (nRow - 1);

            Color color;
            Brush brush;
            Rectangle rect;
            Vector64 p;

            int row1 = 0, row2 = 0;
            if (Plan == planEnum.XOY)
            {
                double step = (maxz - minz) / (nRow - 1);
                row1 = (int)((y1 - minz) / step) + 1;
                row2 = (int)((y2 - minz) / step) + 1;
            }
            else if (Plan == planEnum.XOZ)
            {
                double step = (maxy - miny) / (nRow - 1);
                row1 = (int)((y1 - miny) / step) + 1;
                row2 = (int)((y2 - miny) / step) + 1;
            }
            else if (Plan == planEnum.YOZ)
            {
                double step = (maxx - minx) / (nRow - 1);
                row1 = (int)((y1 - minx) / step) + 1;
                row2 = (int)((y2 - minx) / step) + 1;
            }
            for (int i = row1; i < row2; i++)
            {
                for (int j = 0; j < nCol - 1; j++)
                {
                    p = GetPoint(i, j);
                    if (IsBlanked(p.v)) continue;

                    color = GetColor(p.V);
                    x1 = (double)j / (nCol - 1) * (double)width;
                    y1 = (double)i / (nRow - 1) * (double)height;
                    brush = new SolidBrush(color);
                    rect = new Rectangle((int)x1, (int)(height - y1), (int)xstep, (int)ystep);
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(new Pen(brush), rect);
                    brush.Dispose();
                }
            }
            im.bmp = bmp;
            return im;
        }

        public override TriangleObj toTriangleObj()
        {
            TriangleObj obj = new TriangleObj();
            obj.CopyTransformFrom(this);
            Vector64 p;
            Color color;
            for (int i = 0; i < nRow; i++)
            {
                for (int j = 0; j < nCol; j++)
                {
                    p = GetPoint(i, j);
                    if (EnableColorLevel) color = GetColor(p.V);
                    else color = ObjColor;                    
                    if( enbaleTexture )
                    {
                        obj.AddTexture((float)j / (nCol - 1), 1 - (float)i / (nRow - 1));                        
                    }
                    obj.AddPoint(p.toVector32());
                    obj.AddPointColor(color);
                }
            }
            int i1, i2, i3, i4;
            bool b1, b2, b3, b4;

            int ncol = nCol;
            if (Closed) ncol++;

            for (int i = 0; i < nRow - 1; i++)
            {
                for (int j = 0; j < ncol - 1; j++)
                {
                    // i1----i2  i1(j=0)
                    // i3----i4  i3(j=0)
                    i1 = i * nCol + j;
                    i3 = i1 + nCol;
                    if (j == nCol - 1)
                    {
                        i2 = i * nCol;
                        i4 = i2 + nCol;
                    }
                    else
                    {
                        i2 = i1 + 1;
                        i4 = i3 + 1;
                    }

                    //all 3 triangle points are valid
                    b1 = !IsBlanked(pData[i1].v) && IsValidHeight(pData[i1]);
                    b2 = !IsBlanked(pData[i2].v) && IsValidHeight(pData[i2]);
                    b3 = !IsBlanked(pData[i3].v) && IsValidHeight(pData[i3]);
                    b4 = !IsBlanked(pData[i4].v) && IsValidHeight(pData[i4]);

                    //Draw Grid Mesh to Fit the Blanked Edge
                    List<int> lists = DrawMesh4(b1, b2, b3, b4, i1, i2, i3, i4);
                    for (int k = 0; k < lists.Count; k += 3)
                    {
                        obj.AddTriangleIndex(lists[k], lists[k+1], lists[k+2]);
                    }
                    lists.Clear();
                }//for (int j = 0; j < ncol - 1; j++)
            }//for (int i = 0; i < nRow - 1; i++)
            return obj;
        }//toTriangleObj();

        public override bool ExportToGrid2D(string grdFilename)
        {
            CSurferGrid cs = new CSurferGrid();
            cs.yGrid = nRow;
            cs.xGrid = nCol;

            cs.minv = minv;
            cs.maxv = maxv;
            
            double dist = 0;
            Vector64 p1 = pBaseLine[0];
            for (int i = 1; i < pBaseLine.Count; i++)
            {
                dist += p1.Distance(pBaseLine[i]);
                p1 = pBaseLine[i];
            }
            cs.minx = 0;
            cs.maxx = dist;

            if( Plan == planEnum.XOY )
            {
                cs.miny = minz;
                cs.maxy = maxz;
            }
            else if (Plan == planEnum.XOZ )
            {
                cs.miny = miny;
                cs.maxy = maxy;
            }
            else if (Plan == planEnum.YOZ)
            {
                cs.miny = minx;
                cs.maxy = maxx;
            }

            double val = 0;
            cs.pData = new float[nRow * nCol];
            for (int i = 0; i < nRow; i++)
            {
                for (int j = 0; j < nCol; j++)
                {
                    val = GetPoint(i, j).V;
                    if (IsBlanked(val)) cs[j, i] = CSurferGrid.blankValue;
                    else cs[j, i] = val;
                }
            }

            return cs.SaveAs(grdFilename);

        }
        public bool ExportContourLines(ref StreamWriter wr)
        {
            string str = "[CONTOUR]";
            wr.WriteLine(str);

            str = "surfaces = " + marchingCube.p2DIsoSurfaces.Count;
            wr.WriteLine(str);

            FLOAT_POINT p;
            C2DISOSurface sf;
            for (int i = 0; i < marchingCube.p2DIsoSurfaces.Count; i++)
            {
                sf = marchingCube.p2DIsoSurfaces[i];

                str = "value = " + sf.isoVale;
                wr.WriteLine(str);

                str = "Points = " + sf.pCoordArray.Count;
                wr.WriteLine(str);

                str = "Indices = " + sf.pLineIndex.Count;
                wr.WriteLine(str);

                for (int j = 0; j < sf.pCoordArray.Count; j++)
                {
                    p = sf.pCoordArray[j];
                    str = p.x + "," + p.y + "," + p.z;
                    wr.WriteLine(str);
                }
                for (int j = 0; j < sf.pLineIndex.Count / 2; j++)
                {
                    str = sf.pLineIndex[2 * j] + "," + sf.pLineIndex[2 * j + 1];
                    wr.WriteLine(str);
                }
            }
            return true;
        }
        public bool LoadContourLines(ref StreamReader sr)
        {
            marchingCube.Clear();
            string str = sr.ReadLine();
            int count = Convert.ToInt32(GetLineValue(str, "surfaces"));

            double x, y, z, val;
            int num, indices;
            string[] ss;

            for (int i = 0; i < count; i++)
            {
                str = sr.ReadLine();
                val = Convert.ToDouble(GetLineValue(str, "value"));

                str = sr.ReadLine();
                num = Convert.ToInt32(GetLineValue(str, "Points"));

                str = sr.ReadLine();
                indices = Convert.ToInt32(GetLineValue(str, "Indices"));

                C2DISOSurface sf = new C2DISOSurface(val);
                for (int j = 0; j < num; j++)
                {
                    str = sr.ReadLine();
                    ss = str.Split(new Char[] { ',', ',' }, 3);
                    x = Convert.ToDouble(ss[0]);
                    y = Convert.ToDouble(ss[1]);
                    z = Convert.ToDouble(ss[2]);
                    sf.AddPoint((float)x, (float)y, (float)z);
                }
                for (int j = 0; j < indices / 2; j++)
                {
                    str = sr.ReadLine();
                    ss = str.Split(new Char[] { ',', ',' }, 2);
                    sf.AddIndex(Convert.ToInt32(ss[0]));
                    sf.AddIndex(Convert.ToInt32(ss[1]));
                }
                marchingCube.p2DIsoSurfaces.Add(sf);
            }

            return true;
        }
    }
}
