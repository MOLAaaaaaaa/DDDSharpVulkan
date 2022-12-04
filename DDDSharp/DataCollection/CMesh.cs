using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Security.Cryptography;
using System.Globalization;
using System.Xml;
using GlmNet;
using TextReaderWriter;
using System.Drawing.Design;

namespace DataCollection
{
    //object based on triangles    
    public class CMesh : C3DObjectBase
    {
        public int nRow = 0;
        public int nCol = 0;
        public Vector64[] pData = null;
        public double xStep = 0;
        public double yStep = 0;        
        //contour lines
        public MarchingCubes2D marchingCube = new MarchingCubes2D();

        #region Meshes
        public bool _ShowMesh = true;
        [CategoryAttribute("Mesh"), DisplayNameAttribute("Show Mesh")]
        public bool ShowMesh 
        { 
            get { return _ShowMesh; } 
            set { _ShowMesh = value;
                  RenderMode = RenderingUpdateMode.Redraw; } 
        }

        public bool _ShowContour  = true;
        [CategoryAttribute("Mesh"), DisplayNameAttribute("Show Contour Line")]
        public bool ShowContour 
        {
            get { return _ShowContour; }
            set
            {
                _ShowContour = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public Color _LineColor = Color.Blue;
        [CategoryAttribute("Mesh"), DisplayNameAttribute("Line Color")]
        public Color LineColor
        {
            get { return _LineColor; }
            set
            {
                _LineColor = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public float _LineWidth = 1.0f;
        [CategoryAttribute("Mesh"), DisplayNameAttribute("Line Width")]
        public float LineWidth
        {
            get { return _LineWidth; }
            set
            {
                _LineWidth = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }
        
        public bool _IsFlat = false;
        [CategoryAttribute("Mesh"), DisplayNameAttribute("Is Flat")]
        public bool IsFlat
        {
            get { return _IsFlat; }
            set
            {
                _IsFlat = value;
                ZOffset = (float) (minz + maxz) / 2;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }
        [CategoryAttribute("Mesh"), DisplayNameAttribute("ZOffset")]
        public float ZOffset
        {
            get { return offset.z; }
            set
            {
                offset.z = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }
        #endregion Meshes

        //---color properties---------------
        #region Color section
        public Color _ObjColor = Color.FromArgb(128, 128, 128);
        [CategoryAttribute("Color"), DisplayNameAttribute("Object Color")]
        public Color ObjColor 
        { 
            get { return _ObjColor; }
            set { _ObjColor = value; 
                  if(!EnableColorLevel)RenderMode = RenderingUpdateMode.Redraw; 
                }
        }

        public bool _EnableColorLevel = false;
        [CategoryAttribute("Color"), DisplayNameAttribute("Enable Color Map")]
        public virtual bool EnableColorLevel
        {
            get { return _EnableColorLevel; }
            set
            {
                _EnableColorLevel = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public virtual vec2 GetTextureCoord(int ix, int iy)
        {
            vec2 tex = new vec2(-1,-1);
            if (nCol > 1) tex.x = (float)ix / (nCol - 1);
            if (nRow > 1) tex.x = (float)iy / (nRow - 1);
            return tex;
        }

        public bool IsColorScaleUpdated()
        {
            if (EnableColorLevel)
            {
                if (_ColorScale.DifferentFrom(_OlderColorScale))
                    return true;
            }
            return false;
        }
        public CColorScale _OlderColorScale = new CColorScale();
        public CColorScale _ColorScale = new CColorScale();
        [CategoryAttribute("Color"), DisplayNameAttribute("Color Map")]
        [Editor(typeof(ColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorScaleConverter))]
        public CColorScale ColorScale
        {
            get { return _ColorScale; }
            set
            {
                _OlderColorScale = _ColorScale;
                _ColorScale = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }


        #endregion Color section

        public CMesh()
        {
            type = ShapeEnum.Mesh;
            Version = 1.3f; //2022-9-27 updated
        }
        public CMesh(int row, int col)
        {
            nRow = row;
            nCol = col;
            type = ShapeEnum.Mesh;           
            pData = new Vector64[row * col];
            Version = 1.3f; //2022-9-27 updated
        }
        public Vector64 GetPoint(int row, int col)
        {
            return pData[row * nCol + col];
        }
        public void SetPoint(int row, int col, Vector64 p)
        {
            pData[row * nCol + col] = p;
        }
        public void SetPoint(int row, int col,double x,double y,double z,double v=0)
        {
            pData[row * nCol + col] = new Vector64(x,y,z,v);
        }
        public void SetBlanked(int id)
        {
            Vector64 p = pData[id];
            p.V = CSurferGrid.blankValue;
            pData[id] = p;
        }
        public void SetBlanked(int ix, int iy)
        {
            SetBlanked(ix + iy * nCol);
        }
        public bool IsInRange(double x, double y)
        {
            if (x < minx || x > maxx || y < miny || y > maxy)
                return false;
            else return true;
        }
        public virtual C3DLine[] CreateIntersectionLines(CMesh mesh)
        {
            return null;
        }
        // YTopLeft
        // |
        // |
        // O---------->X
        public bool fromGrid2D(CSurferGrid grid)
        {
            double x1,y1,z1,x2,y2, z2;
            x1 = grid.minx;
            y1 = grid.miny;
            z1 = grid.minv;
            x2 = grid.maxx;
            y2 = grid.maxy;
            z2 = grid.maxv;
            
            int nx = grid.xGrid;
            int ny = grid.yGrid;
            //Y(0,row-1)       (col-1,row-1)
            //|
            //|
            //|
            //0,0------------>X(col-1,0)
            double topleft = grid.GetZValue(0, ny - 1);
            double topright = grid.GetZValue(nx - 1, ny - 1);
            double bottomleft = grid.GetZValue(0, 0);
            double bottomright = grid.GetZValue(nx - 1, 0);
            Vector64 TopLeft = new Vector64(x1, y2, topleft , topleft);
            Vector64 TopRight = new Vector64(x2, y2,topright , topright);
            Vector64 BottomLeft = new Vector64(x1, y1, bottomleft, bottomleft);
            Vector64 BottomRight = new Vector64(x2, y1, bottomright, bottomright);

            Name = grid.Name;
            fromGrid2D(grid, 1, false);
            //fromGrid2D(grid, TopLeft, TopRight, BottomLeft, BottomRight, 1, false);            
            enbaleTexture = false;
            return true;
        }
        /// <summary>
        /// 输入grid 网格数据，四个角点坐标，创建Mesh
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="topLeft"></param>
        /// <param name="topRight"></param>
        /// <param name="bottomLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="scale">z scale</param>
        /// <param name="inverse">reverse z upward</param>
        /// <returns>true is success</returns>
        public bool fromGrid2D(CSurferGrid grid,double scale = 1.0,bool inverse = false)
        {
            nRow = grid.yGrid;
            nCol = grid.xGrid;
            type = ShapeEnum.Mesh;
            pData = new Vector64[nRow * nCol];
            double zscale = scale;
            if (inverse) zscale = -zscale;
            double z0 = (grid.minv + grid.maxv) / 2.0;
            // p1         p2
            // | -----------/
            // | y pn1  /   |
            // |    /  pn2  |
            // | /__________|__x
            // p3          p4
            Vector64 p;
            double x, y, z;
            for (int i = 0; i < nRow; i++)
            {
                y = grid.miny + i * grid.yStep;
                for (int j = 0; j < nCol; j++)
                {
                    x = grid.minx + j * grid.xStep;
                    z = grid.GetZValue(j, i);
                    p = new Vector64(x, y, z, z);

                    if ( !grid.IsBlankedValue(z) )
                    {
                        z = grid.minv + (z - grid.minv) * zscale;
                        p.z = p.v = z;
                    }
                    else //白化数据
                    {
                        p.z = z0;
                        p.v = CSurferGrid.blankValue;
                    }
                    pData[i * nCol + j] = p;
                }
            }

            UpdateRange();

            ColorScale = new CColorScale(minv, maxv);

            return true;
        }
        public bool fromGrid2D(CSurferGrid grid,
                                Vector64 topLeft,Vector64 topRight,
                                Vector64 bottomLeft,Vector64 bottomRight,
                                double scale = 1.0,
                                bool inverse = false)
        {
            nRow = grid.yGrid;
            nCol = grid.xGrid;
            type = ShapeEnum.Mesh;
            pData = new Vector64[nRow * nCol];
            double zscale = scale;
            if (inverse) zscale = -zscale;
            double z0 = (grid.minv + grid.maxv) / 2.0;
            double z = 0;
            // p1         p2
            // | -----------/
            // | y pn1  /   |
            // |    /  pn2  |
            // | /__________|__x
            // p3          p4
            Vector64 p1 = topLeft;
            Vector64 p2 = topRight;
            Vector64 p3 = bottomLeft;
            Vector64 p4 = bottomRight;
            
            Vector64 pn1 = ( (p3 - p2).Cross(p2 - p1) ).Normalize();
            Vector64 pn2 = ( (p4 - p2).Cross(p2 - p3) ).Normalize();
            Vector64 p13, p24;
            Vector64 p;
         
            int col0;
            for (int i = 0; i < nRow; i++)
            {
                col0 = (int)( (double)i * (nCol-1) / (nRow-1) );
                p13 = p3 + (p1 - p3) * i / (nRow - 1);
                p24 = p4 + (p2 - p4) * i / (nRow - 1);
             
                for (int j = 0; j < nCol; j++)
                {
                    p = p13 + (p24 - p13) * j / (nCol - 1);
                    z = grid.GetZValue(j, i);
                                        
                    if ( !grid.IsBlankedValue(z) )
                    {
                        z = z0 + ( z - grid.minv ) * zscale;
                        p.z = p.v = z;
                    }
                    else //白化数据
                    {
                        p.z = z0;
                        p.v = CSurferGrid.blankValue;
                    }
                    
                    pData[i * nCol + j] = p;
                }
            }
            
            UpdateRange();
            
            ColorScale = new CColorScale(minv, maxv);

            return true;
        }
        
        public double GetValue(int ix,int iy)
        {
           return pData[GetVerticIndex(ix, iy)].V;
        }

        public double GetValue(double x, double y)
        {
            int ix = (int)((x - minx) / xStep);
            int iy = (int)((y - miny) / yStep);

            //out of the range of this meshes
            if (ix < 0) ix = 0;
            if (iy < 0) iy = 0;
            if (ix > nCol - 1) ix = nCol - 1;
            if (iy > nRow - 1) iy = nRow - 1;

            int id0 = GetVerticIndex(ix, iy);
            int id1 = GetVerticIndex(ix + 1, iy);
            int id2 = GetVerticIndex(ix, iy + 1);
            int id3 = GetVerticIndex(ix + 1, iy + 1);

            if (id1 >= nCol * nRow) id1 = -1;
            if (id1 >= nCol * nRow) id2 = -1;
            if (id3 >= nCol * nRow) id3 = -1;
            
            if (id0 >= 0) if (IsBlankValue(pData[id0].V)) id0 = -1;
            if (id1 >= 0) if (IsBlankValue(pData[id1].V)) id1 = -1;
            if (id2 >= 0) if (IsBlankValue(pData[id2].V)) id2 = -1;
            if (id3 >= 0) if (IsBlankValue(pData[id3].V)) id3 = -1;

            if (id0 < 0 && id1 < 0 && id2 < 0 && id3 < 0) return CSurferGrid.blankValue;

            double x1 = minx + ix * xStep;
            double y1 = miny + iy * yStep;
            double x2 = x1 + xStep;
            double y2 = y1 + yStep;

            double v0 = 0, v1 = 0, v2 = 0, v3 = 0, v = 0;
            if (id0 >= 0) v0 = pData[id0].V;
            if (id1 >= 0) v1 = pData[id1].V;
            if (id2 >= 0) v2 = pData[id2].V;
            if (id3 >= 0) v3 = pData[id3].V;
            //    |y
            //    2----3
            //    |    |
            //    0----1---->x
            if (id0 >= 0 && id1 < 0 && id2 < 0 && id3 < 0) return v0;
            else if (id0 < 0 && id1 >= 0 && id2 < 0 && id3 < 0) return v1;
            else if (id0 < 0 && id1 < 0 && id2 >= 0 && id3 < 0) return v2;
            else if (id0 < 0 && id1 < 0 && id2 < 0 && id3 >= 0) return v3;

            else if (id0 >= 0 && id1 >= 0 && id2 < 0 && id3 < 0)//01
            {
                return v0 + (v1 - v0) * (x - x1) / xStep;
            }
            else if (id0 >= 0 && id2 >= 0 && id1 < 0 && id3 < 0)//02
            {
                return v0 + (v2 - v0) * (y - y1) / yStep;
            }
            else if (id0 >= 0 && id3 > 0 && id1 < 0 && id2 < 0)//03
            {
                double l = Math.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
                double ll = Math.Sqrt(xStep * yStep);
                return v0 + (v3 - v0) * l / ll;
            }
            else if (id1 >= 0 && id2 > 0 && id0 < 0 && id3 < 0)//12
            {
                double l = Math.Sqrt((x - x2) * (x - x2) + (y - y1) * (y - y1));
                double ll = Math.Sqrt(xStep * yStep);
                return v1 + (v2 - v1) * l / yStep;
            }
            else if (id1 >= 0 && id3 > 0 && id0 < 0 && id2 < 0)//13
            {
                return v1 + (v3 - v1) * (y - y1) / yStep;
            }
            else if (id0 >= 0 && id1 >= 0 && id2 >= 0 && id3 >= 0)//0123
            {
                double p1 = v0 + (x - x1) * (v1 - v0) / xStep;
                double p2 = v2 + (x - x1) * (v3 - v2) / xStep;
                return p1 + (p2 - p1) * (y - y1) / yStep;
            }
            else return v0;
        }

        /// <summary>
        /// 得到网格索引号
        /// </summary>
        /// <param name="ix">横向</param>
        /// <param name="iy">纵向</param>
        /// <returns>-1不在范围内</returns>
        public int GetVerticIndex(int ix, int iy)
        {
            if (ix < 0 || ix >= nCol) return -1;
            if (iy < 0 || iy >= nRow) return -1;
            return ix + iy * nCol;
        }
        /// <summary>
        /// 输入Image数据，四个角点坐标，创建Mesh
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="topLeft"></param>
        /// <param name="topRight"></param>
        /// <param name="bottomLeft"></param>
        /// <param name="bottomRight"></param>
        /// <returns></returns>
        public bool fromImage(Bitmap bmp, Vector64 topLeft, Vector64 topRight, Vector64 bottomLeft, Vector64 bottomRight)
        {
            nRow = 4;
            nCol = 4;
            type = ShapeEnum.Mesh;
            pData = new Vector64[nRow * nCol];
            double z;
            // p1         p2
            // | -----------/
            // | y pn1  /   |
            // |    /  pn2  |
            // | /__________|__x
            // p3          p4
            Vector64 p1 = topLeft;
            Vector64 p2 = topRight;
            Vector64 p3 = bottomLeft;
            Vector64 p4 = bottomRight;

            Vector64 pn1 = ((p3 - p2).Cross(p2 - p1)).Normalize();
            Vector64 pn2 = ((p4 - p2).Cross(p2 - p3)).Normalize();
            Vector64 p13, p24;
            Vector64 p;
            
            //CSurferGrid cs = new CSurferGrid();
            //cs.fromImage(bmp);

            int col0;
            for (int i = 0; i < nRow; i++)
            {
                col0 = (int)((double)i * (double)(nCol - 1) / (double)(nRow - 1));
                p13 = p3 + (p1 - p3) * (double)i / (nRow - 1);
                p24 = p4 + (p2 - p4) * (double)i / (nRow - 1);
                for (int j = 0; j < nCol; j++)
                {
                    p = p13 + (p24 - p13) * (double)j / (nCol - 1);
                    //p.v = cs.GetZValue(j,i);
                    p.v = 0;
                    pData[i * nCol + j] = p;
                }
            }

            UpdateRange();

            ColorScale = new CColorScale(0, 1);

            return true;
        }
       
        public Color GetColor(double v)
        {
            if ( !EnableColorLevel ) return ObjColor;
            else return ColorScale.GetColor(v);
        }       
        public void InitColorScale()
        {
            ColorScale = new CColorScale(minv, maxv);            
        }
        public void InitColorScale(double v1,double v2)
        {
            ColorScale = new CColorScale(v1,v2);
        }

        public void SetColorScale(CColorScale scale)
        {
            ColorScale = scale;
        }
        public void AddPoint(int irow,int icol,double x,double y,double z,double v)
        {
            if (pData == null) return;
            if (nRow < 1 || nCol < 1) return;
            pData[icol + irow * nCol] = new Vector64(x, y, z, v);
        }
        public void AddPoint(int irow, int icol, Vector64 p)
        {
            if (pData == null) return;
            if (nRow < 1 || nCol < 1) return;
            pData[icol + irow * nCol] = p;
        }
        /// <summary>
        /// 计算曲面法线方向
        /// </summary>
        /// <returns></returns>
        /// p1-----------p2
        ///   ------------
        /// p3------------p4
        public Vector64 GetFaceNormal()
        {
            Vector64 p1, p2,p3,p4; //偏移量

            //计算曲面法线方向
            p1 = pData[0];
            p2 = pData[nCol-1];
            p4 = pData[nRow * nCol - 1];
            p3 = pData[ (nRow - 1) * nCol - 1];
            Vector64 v1 = Vector64.GetNormal(p1, p3, p2);
            Vector64 v2 = Vector64.GetNormal(p4, p2, p3);
            //return (v1 + v2).Normalize();
            return v1;
        }
        /// <summary>
        /// 点是否在曲面上，
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool IsPointOnMesh(Vector64 p)
        {
            int id1, id2, id3,id4;
            CTriangle3f tr1 = new CTriangle3f();
            CTriangle3f tr2 = new CTriangle3f();
            for (int i = 0; i < nRow - 1; i++)
            {
                for (int j = 0; j < nCol - 1; j++)
                {
                    id1 = i * nCol + j;
                    id2 = id1 + 1;
                    id3 = id1 + nCol;
                    id4 = id2 + nCol;

                    tr1.p1 = pData[id1];
                    tr1.p2 = pData[id2];
                    tr1.p3 = pData[id3];
                    
                    if (tr1.IsPointInTriangle(p)) return true;
                    tr2.p1 = pData[id4];
                    tr2.p2 = pData[id3];
                    tr2.p3 = pData[id2];
                    if (tr2.IsPointInTriangle(p)) return true;                   
                }
            }
            return false;
        }       

        /// <summary>
        /// 点是否在曲面正面，
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>

        public bool IsPointAtFront(Vector64 p)
        {
            Vector64 face = GetFaceNormal();//曲面正方向
            if ( face.Dot(p.Normalize()) >= 0 ) return true;
            else return false;
        }
        //创建曲面缓冲区
        //size 缓冲区半径
        //sampledStep采样点距,沿直径采样点
        //method衰减函数：0 距离平方倒数衰减
        public List<Vector64> CreateBuffer(double size,int sampledNum, double farestValue = 0.0, double nearestValue = 1.0, int method = 0 )
        {
            try 
            {
                //p3,p4
                //p1,p2
                //
                //p3, p4------------
                //p1, p2--------

                List<Vector64> buffers = new List<Vector64>();

                Vector64[] normals = new Vector64[nRow*nCol];
                for(int i=0;i<nRow*nCol;i++)normals[i] = new Vector64(0, 0, 0, 0);
                
                int id1, id2, id3, id4;
                Vector64 p1, p2, p3,p4,pn;
                for(int i=0;i<nRow - 1;i++)
                {
                    for (int j = 0; j < nCol - 1; j++)
                    {
                        id1 = i * nCol + j;
                        id2 = id1 + 1;
                        id3 = id1 + nCol;
                        id4 = id2 + nCol;
                        p1 = pData[id1];
                        p2 = pData[id2];
                        p3 = pData[id3];
                        p4 = pData[id4];

                        pn = (p2 - p1).Cross(p3 - p2);
                        pn = pn.Normalize();

                        normals[id1] += pn;
                        normals[id2] += pn;
                        normals[id3] += pn;

                        pn = (p3 - p4).Cross(p2 - p3);
                        pn = pn.Normalize();

                        normals[id4] += pn;
                        normals[id3] += pn;
                        normals[id2] += pn;
                    }
                }
                double step = size / (sampledNum - 1);
                double dist = 0;
                for (int i = 0; i < nRow * nCol; i++)
                {
                    normals[i] = normals[i].Normalize();
                    p1 = pData[i];

                    for( int k= sampledNum - 1; k >= 0; k-- )
                    {
                        dist = k * step;
                        p1 = pData[i] - dist * normals[i];
                        p1.V = nearestValue - dist  /size * (nearestValue - farestValue);
                        buffers.Add(p1);
                    }
                    for (int k = 1; k < sampledNum; k++)
                    {
                        dist = k * step;
                        p1 = pData[i] + dist * normals[i];
                        p1.V = nearestValue - dist / size * (nearestValue - farestValue);
                        buffers.Add(p1);
                    }
                }
                
                normals = null;

                return buffers;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return null;
            }

        }
        public TriangleObj toTriangleObj( double thickness )
        {
            if (thickness == 0) return toTriangleObj();
            Vector64 pn = GetFaceNormal();

            TriangleObj obj = new TriangleObj();
            Vector64 p1,p2;
            for (int i = 0; i < pData.Length; i++)
            {
                p1 = pData[i] - pn * thickness / 2.0;
                obj.AddPoint(p1.x, p1.y, p1.z, pData[i].v);
            }
            for (int i = 0; i < pData.Length; i++)
            {
                p2 = pData[i] + pn * thickness / 2.0;
                obj.AddPoint(p2.x, p2.y, p2.z, pData[i].v);
            }
            //1 2
            //3 4
            //face1 -
            int id1, id2, id3, id4;
            for (int i = 0; i < nRow - 1; i++)
            {
                for (int j = 0; j < nCol - 1; j++)
                {
                    id1 = i * nCol + j;
                    id2 = id1 + 1;
                    id3 = id1 + nCol;
                    id4 = id2 + nCol;
                    obj.AddTriangleIndex(id1, id2, id3);
                    obj.AddTriangleIndex(id4, id3, id2);
                }
            }
            //1 2
            //3 4
            //face2 +
            int offset = nRow * nCol;
            for (int i = 0; i < nRow - 1; i++)
            {
                for (int j = 0; j < nCol - 1; j++)
                {
                    id1 = i * nCol + j + offset;
                    id2 = id1 + 1;
                    id3 = id1 + nCol;
                    id4 = id2 + nCol;
                    obj.AddTriangleIndex(id1, id3, id2);
                    obj.AddTriangleIndex(id4, id2, id3);
                }
            }
            
           //top 
           //1  2 
           //3+ 4+
           for (int j = 0; j < nCol - 1; j++)
           {
               id1 = j;
               id2 = id1 + 1;
               id3 = id1 + offset;
               id4 = id2 + offset;
               obj.AddTriangleIndex(id1, id3, id2);
               obj.AddTriangleIndex(id4, id2, id3);
           }
           //bottom 
           //1  2 
           //3+ 4+
           for (int j = 0; j < nCol - 1; j++)
           {
               id1 = ( nRow - 1 ) * nCol + j;
               id2 = id1 + 1;
               id3 = id1 + offset;
               id4 = id2 + offset;
               obj.AddTriangleIndex(id1, id2, id3);
               obj.AddTriangleIndex(id4, id3, id2);
           }
            
            //left side
            //1 2+ 
            //3 4+
            for (int i = 0; i < nRow - 1; i++)
            {
                id1 = i * nCol;
                id2 = id1 + offset;
                id3 = id1 + nCol;
                id4 = id2 + nCol;
                obj.AddTriangleIndex(id1, id3, id2);
                obj.AddTriangleIndex(id4, id2, id3);
            }
            //right side
            //1 2+ 
            //3 4+
            for (int i = 0; i < nRow - 1; i++)
            {
                id1 = i * nCol + nCol - 1;
                id2 = id1 + offset;
                id3 = id1 + nCol;
                id4 = id2 + nCol;
                obj.AddTriangleIndex(id1, id2, id3);
                obj.AddTriangleIndex(id4, id3, id2);
            }
            
            obj.UpdateRange();
            return obj;
        }

        // i1----i2---i1(j=0)
        // |     |
        // i3----i4---i3(j=0)
        static public List<int> DrawMesh4(bool b1, bool b2, bool b3, bool b4, int i1, int i2, int i3, int i4)
        {
            byte id = 0;
            if (b1) id += 1;
            if (b2) id += 2;
            if (b3) id += 4;
            if (b4) id += 8;

            List<int> lists = new List<int>();
            if (id == 7) { lists.Add(i1); lists.Add(i2); lists.Add(i3); }
            else if (id == 11) { lists.Add(i1); lists.Add(i2); lists.Add(i4); }
            else if (id == 13) { lists.Add(i1); lists.Add(i4); lists.Add(i3); }
            else if (id == 14) { lists.Add(i2); lists.Add(i4); lists.Add(i3); }
            else if (id == 15)
            {
                lists.Add(i1); lists.Add(i2); lists.Add(i3);
                lists.Add(i2); lists.Add(i4); lists.Add(i3);
            }
            return lists;
        }
        public virtual TriangleObj toTriangleObj()
        {
            Color c;
            TriangleObj obj = new TriangleObj();
            Vector64 p;
            if (EnableColorLevel) obj.IsUniformColor = false;
            else            
                obj.color = new GlmNet.vec4(ObjColor.R / 255f,
                                            ObjColor.G / 255f,
                                            ObjColor.B / 255f,
                                            ObjColor.A / 255f);
            /*
            Vector64 p1, p2, p3, p4;
            Vector64 p13, p24;
            Vector64 p0 = new Vector64(minx, miny, minz, minv);
            p1 = pData[0] - p0;
            p2 = pData[nCol - 1] - p0;
            p3 = pData[(nRow-1) * nCol] - p0;
            p4 = pData[nRow * nCol-1] - p0;
            //////////避免精度损失过大//////////////////////////////
            for (int i = 0; i < nRow; i++)
            {
                p13 = p3 + (p1 - p3) * i / (nRow - 1);
                p24 = p4 + (p2 - p4) * i / (nRow - 1);

                for (int j = 0; j < nCol; j++)
                {
                    p = p13 + (p24 - p13) * j / (nCol - 1) + p0;
                    p.z = pData[j + i * nCol].z;
                    p.V = pData[j + i * nCol].V;
                    obj.AddPoint( p.x , p.y , p.z, p.v);
                    if (EnableColorLevel)
                    {
                        c = GetColor(p.v);
                        obj.AddPointColor(c);
                    }
                }
            }
            */
            ///*
            for (int i=0;i<pData.Length;i++)
            {
                p = pData[i];
                obj.AddPoint(p.x,p.y,p.z,p.v);
                if (EnableColorLevel)
                {
                    c = GetColor(p.v);
                    obj.AddPointColor(c);
                }
            }
            //*/
            // id1  id2
            // id3  id4
            int id1, id2, id3, id4;
            for (int i = 0; i < nRow - 1; i++)
            {
                for (int j = 0; j < nCol - 1; j++)
                {
                    id1 = i * nCol + j;
                    id2 = id1 + 1;
                    id3 = id1 + nCol;
                    id4 = id2 + nCol;
                    obj.AddTriangleIndex(id1,id2,id3);
                    obj.AddTriangleIndex(id4, id3, id2);
                }
            }
            obj.UpdateRange();
            return obj;
        }   
        
        public override bool LoadFrom(string path)
        {
            return ImportData(path);
        }
        public override bool SaveAs(string path)
        {
            return ExportData(path);
        }
        public bool SaveAs10(BinaryWriter br)
        {
            if (!SaveObjHeader(br)) return false;
            try
            {
                br.Write(nRow);
                br.Write(nCol);
                if (nRow > 0 && nCol > 0)
                {
                    foreach (Vector64 p in pData)
                    {
                        br.Write(p.x);
                        br.Write(p.y);
                        br.Write(p.z);
                        br.Write(p.v);
                    }
                }
                ColorScale.WriteBinary(br);
                marchingCube.SaveBinary(br);
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public bool LoadFrom10(BinaryReader br)
        {
            if (!LoadObjHeader(br)) return false;
            double x, y, z, v;
            try
            {
                nRow = br.ReadInt32();
                nCol = br.ReadInt32();
                if (nRow > 0 && nCol > 0)
                {
                    pData = null;
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
                ColorScale.LoadBinary(br);
                marchingCube.LoadBinary(br);
                UpdateRange();
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public override bool SaveAs(BinaryWriter br)
        {
            if ( !SaveObjHeader(br) ) return false;
            try
            {
                br.Write(nRow);
                br.Write(nCol);
                if( nRow > 0 && nCol > 0 )
                {
                    foreach(Vector64 p in pData)
                    {
                        br.Write(p.x);
                        br.Write(p.y);
                        br.Write(p.z);
                        br.Write(p.v);
                    }
                }
                
                br.Write(ShowMesh);
                br.Write(ShowContour);
                br.Write(LineWidth);
                br.Write(LineColor.ToArgb());
                br.Write(ObjColor.ToArgb());
                br.Write(EnableColorLevel);

                ColorScale.WriteBinary(br);
                marchingCube.SaveBinary(br);

                //added after 2022-9-27,add isflat
                if (Version >= 1.3f) br.Write(_IsFlat);

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
            if ( !LoadObjHeader(br) ) return false;            
            double x, y, z, v;
            try
            {
                nRow = br.ReadInt32();
                nCol = br.ReadInt32();
                if (nRow > 0 && nCol > 0)
                {
                    pData = null;
                    pData = new Vector64[nRow*nCol];
                    for(int i = 0;i<pData.Length;i++)
                    {
                        x = br.ReadDouble();
                        y = br.ReadDouble();
                        z = br.ReadDouble();
                        v = br.ReadDouble();
                        pData[i] = new Vector64(x,y,z,v);
                    }
                }
                //added after 2022-4
                ShowMesh = br.ReadBoolean();
                ShowContour = br.ReadBoolean();
                LineWidth = br.ReadSingle();
                LineColor = Color.FromArgb(br.ReadInt32());
                ObjColor = Color.FromArgb(br.ReadInt32());
                EnableColorLevel = br.ReadBoolean();
                //added after 2022-4

                ColorScale.LoadBinary(br);
                marchingCube.LoadBinary(br);

                //added after 2022-9-27,add isflat
                if(Version >= 1.3f)_IsFlat = br.ReadBoolean();

                UpdateRange();
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public override bool ExportVRML(StreamWriter wr)
        {
            TriangleObj tri = toTriangleObj();
            return tri.ExportVRML(wr);            
        }
        public virtual bool ExportToGrid2D(string grdFilename)
        {
            CSurferGrid cs = new CSurferGrid();
            cs.yGrid = nRow;
            cs.xGrid = nCol;

            cs.minv = minv;
            cs.maxv = maxv;
            cs.miny = miny;
            cs.maxy = maxy;

            cs.minx = minx;
            cs.maxx = maxx;
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
        public bool ExportXYZVData(string path)
        {
            try
            {                
                StreamWriter wr = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write));
                wr.WriteLine("x,y,z,value");
                for (int i = 0; i < nRow; i++)
                    for (int j = 0; j < nCol; j++)
                    {
                        wr.WriteLine(pData[i * nCol + j].toString(4));
                    }
                wr.Close();
                return true;
            }
            catch(Exception ex)
            {
                errMessage = "export to file failed." + ex.Message;
                return false;
            }
        }
        public override bool ExportData(string path)
        {
            try
            {
                string line;                
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                StreamWriter wr = new StreamWriter(fs);
                
                Name = Path.GetFileName(path);
                
                line = "[MESH]";
                wr.WriteLine(line);

                line = "Row = " + nRow;
                wr.WriteLine(line);

                line = "Column = " + nCol;
                wr.WriteLine(line);

                line = "Color = " + ObjColor.ToArgb();
                wr.WriteLine(line);
                
                for (int i = 0; i < nRow; i++)
                    for (int j = 0; j < nCol; j++)
                    {
                        wr.WriteLine(pData[i * nCol + j].toString(4));
                    }

                line = "[Properties]";
                wr.WriteLine(line);

                line = "EnableColorLevel = " + EnableColorLevel;
                wr.WriteLine(line);
                line = "ShowMesh = " + ShowMesh;
                wr.WriteLine(line);
                line = "ShowCounterLine = " + ShowContour;
                wr.WriteLine(line);

                //"[TRANSLATIONS]"
                ExportTranslations(wr);
               
                //"[ColorMap]"
                if ( EnableColorLevel )
                {
                    ColorScale.WriteStream(wr);
                }

                wr.Close();
                fs.Close();
                return true;
            }
            catch(Exception ex)
            {
                errMessage = "export to file failed." + ex.Message;
                return false;
            }            
        }
        public override bool ImportData(string path)
        {
            try
            {
                string str;            
               
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                Name = Path.GetFileName(path);

                Clear();

                if ( !AscIIProfile.SeekSection("[MESH]", ref sr) )
                {
                    errMessage = "not a valid mesh file";
                    sr.Close();
                    fs.Close();
                    return false;
                }

                int color = 0;
                AscIIProfile.ReadIntValue(sr, "Row", out nRow);
                AscIIProfile.ReadIntValue(sr, "Column", out nCol);
                AscIIProfile.ReadIntValue(sr, "Color", out color);
                ObjColor = Color.FromArgb(color);

                if (nRow < 1 || nCol < 1)
                {
                    errMessage = "reading data error.";
                    sr.Close();
                    fs.Close();
                    return false;
                }              

                pData = new Vector64[nRow * nCol];                        

                for (int i = 0; i < nRow; i++)
                    for (int j = 0; j < nCol; j++)
                    {
                        str = AscIIProfile.ReadLine(sr);
                        AddPoint(i, j, Vector64.Parse(str, 4));
                    }

                if (AscIIProfile.SeekSection("[Properties]", ref sr))
                {
                    EnableColorLevel = AscIIProfile.ReadBoolValue(sr, "EnableColorLevel");
                    ShowMesh = AscIIProfile.ReadBoolValue(sr, "ShowMesh");
                    ShowContour = AscIIProfile.ReadBoolValue(sr, "ShowCounterLine");
                }

                if (AscIIProfile.SeekSection("[TRANSLATIONS]", ref sr))
                {
                    ImportTranslations(sr);
                }
                else UpdateRange();

                if (AscIIProfile.SeekSection("[ColorMap]", ref sr))
                {
                    ColorScale = new CColorScale(minv, maxv);
                    ColorScale.ReadStream(ref sr);
                    ColorScale.SetValueRange(minv, maxv);
                }

                sr.Close();
                fs.Close();
                return true;
            }
            catch
            {
                errMessage = "open file failed.";
                return false;
            }
        }
        /// <summary>
        /// 计算线段与地层面的交点
        /// </summary>
        /// <param name="lp1">线段端点1</param>
        /// <param name="lp2">线段端点2</param>
        /// <param name="p">交点坐标</param>
        /// <returns>是否有交点？</returns>
        public virtual bool GetInterSection(Vector64 lp1, Vector64 lp2,out Vector64 p)
        {
            Vector64 p1, p2, p3,p4;
            p = new Vector64(0, 0, 0);
            for (int i = 0; i < nRow-1; i++)
            {
                for (int j = 0; j < nCol-1; j++)
                {
                    p1 = pData[i * nCol + j];
                    p2 = pData[i * nCol + j+1];
                    p3 = pData[(i+1) * nCol + j];
                    p4 = pData[(i+1) * nCol + j + 1];
                    if (Vector64.TestLineThruTriangle(p1, p2, p3, lp1, lp2, out p))
                    {
                        p.V = 0;
                        int nk = 0;
                        if (!IsBlanked(p1.V)) { p.V += p1.V; nk++; }
                        if (!IsBlanked(p2.V)) { p.V += p2.V; nk++; }
                        if (!IsBlanked(p3.V)) { p.V += p3.V; nk++; }
                        if (nk > 0) p.V = p.V / nk;
                        else p.V = double.NaN;
                        return true;
                    }
                    else if (Vector64.TestLineThruTriangle(p4, p3, p2, lp1, lp2, out p))
                    {
                        p.V = 0;
                        int nk = 0;
                        if (!IsBlanked(p4.V)) { p.V += p4.V; nk++; }
                        if (!IsBlanked(p2.V)) { p.V += p2.V; nk++; }
                        if (!IsBlanked(p3.V)) { p.V += p3.V; nk++; }
                        if (nk > 0) p.V = p.V / nk;
                        else p.V = double.NaN;
                        return true; 
                    }
                }
            }
            return false;
        }
        public override void Clear()
        {
            nRow = 0;
            nCol = 0;
            pData = null;
        }
        public bool IsBlanked(Vector64 p)
        {
            return IsBlanked(p.v);
        }

        public override void UpdateRange()
        {
            minx = miny = minz = 0;
            maxx = maxy = maxz = 0;

            if (pData == null) return;

            Vector32 p;
            for (int i = 0; i < pData.Length; i++)
            {
                p = pData[i];
                if (i == 0)
                {
                    minx = maxx = p.x;
                    miny = maxy = p.y;
                    minz = maxz = p.z;
                }
                else
                {
                    if (p.x < minx) minx = p.x;
                    if (p.x > maxx) maxx = p.x;
                    if (p.y < miny) miny = p.y;
                    if (p.y > maxy) maxy = p.y;
                    if (p.z < minz) minz = p.z;
                    if (p.z > maxz) maxz = p.z;
                }
            }
            
            xStep = (maxx - minx) / (nCol - 1);
            yStep = (maxy - miny) / (nRow - 1);

            int k = 0;
            for (int i = 0; i < pData.Length; i++)
            {
                p = pData[i];
                if (IsBlankValue(p.V)) continue;
                if (k == 0)
                {
                    minv = maxv = p.v;
                    k++;
                }
                else
                {
                    if (p.v < minv) minv = p.v;
                    if (p.v > maxv) maxv = p.v;
                }
            }
        }
        //---------Counter Lines ----------------
        public int IsExistISOValue(double val)
        {
            for (int i = 0; i < marchingCube.p2DIsoSurfaces.Count; i++)
            {
                if (val == marchingCube.p2DIsoSurfaces[i].isoVale)
                    return i;
            }
            return -1;
        }
        public void DoMarchingCube()
        {
            marchingCube.Clear();
            marchingCube.SetData(this);
            int exist = -1;
            double scaledvalue;
            for (int i = 0; i < ColorScale.Count; i++)
            {
                scaledvalue = ColorScale.GetScaledValue(i);
                exist = IsExistISOValue(scaledvalue);
                if (ColorScale[i].Visible)
                {
                    if (exist < 0) marchingCube.DoSearchSurface(scaledvalue);
                }
                else
                {
                    if (exist >= 0) marchingCube.p2DIsoSurfaces.RemoveAt(exist);
                }
            }
        }
    }
}
