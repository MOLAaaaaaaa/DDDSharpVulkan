using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using GlmNet;
using Graphics3D;

namespace DataCollection
{
    // linear devided method, rapid computed the intersected point
    // devided NUMBER such as 2,4,8, 
    // get the nearest divided point as the intersected point
    //index of the x,y,z coord in coords array
    public struct TRIANGLE_INDEX
    {
        public int iPoint1;    //opengl coord
        public int iPoint2;    //opengl coord,y is upwards
        public int iPoint3;    //opengl coord,        
    }
    public struct TRIANGLE_INDEX_EXT
    {
        public int iPoint1;    //opengl coord
        public int iPoint2;    //opengl coord,y is upwards
        public int iPoint3;    //opengl coord,        
        public byte iColor;      //color index
    }
    public struct FLOAT_POINT
    {
        public float x, y, z;
        public FLOAT_POINT(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }        
        public static implicit operator FLOAT_POINT(FLOAT_POINT_EXT p)
        {
            return new FLOAT_POINT(p.x, p.y, p.z);
        }
    }
    public struct FLOAT_POINT_EXT
    {
        public float x, y, z;
        public Int16 icolor;
        public byte alpha;
        public FLOAT_POINT_EXT(float _x, float _y, float _z,int color)
        {
            x = _x;
            y = _y;
            z = _z;
            alpha = 255;
            icolor = (Int16)color;
        }
        public FLOAT_POINT_EXT(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
            alpha = 255;
            icolor = 0;
        }
        public static implicit operator FLOAT_POINT_EXT(FLOAT_POINT p)
        {
            return new FLOAT_POINT_EXT(p.x,p.y,p.z);
        }
    }
    public class EDGE_INDEX_PROCESS
    {
        public int edno;    //0-11
        public int index;   //index of the vertic
        public EDGE_INDEX_PROCESS(int _edge,int _index)
        {
            edno = _edge;
            index = _index;
        }
    }
    public struct VERTIC_EDGE_RELATION
    {
        public int vertics;
        public int edge1;
        public int edge2;
        public int edge3;
    }
    public class GRID_EDGE_INDEX
    {
        public Int16 typeIndex;     // 0 - 256 	
        public byte[] pDivide;   // No. of divided each edge, from 1 to dividedNum ( ex 16 ),need No-1 when use it
                                 // 0 is no point at this edge
        public GRID_EDGE_INDEX()
        {
            typeIndex = 0;
            pDivide = new byte[12];
            for (int i = 0; i < 12; i++)
                pDivide[i] = 0;
        }
    }
    public class EDGE_POINT_INDEX
    {
        //each point index in pCoordArray, -1 no point
        //边上交点在pCoordArray中的点索引号
        public int[] pIndex;    
        public int indexNum = 20;
        public EDGE_POINT_INDEX(int no = 20)
        {
            indexNum = no;
            pIndex = new int[indexNum];
            for (int i = 0; i < indexNum; i++)
                pIndex[i] = -1;
        }
        public void Reset()
        {
            for (int i = 0; i < indexNum; i++)
                pIndex[i] = -1;
        }
    }
    public class C2DISOSurface
    {
        public double isoVale;
        public List<FLOAT_POINT> pCoordArray;
        public List<int> pLineIndex;

        public C2DISOSurface(double value)
        {
            isoVale = value;
            pCoordArray = new List<FLOAT_POINT>();
            pLineIndex = new List<int>();
        }

        //查找指定id位置，返回pos index     
        int SearchNext(int id,List<int> indices)
        {
            for(int i =0;i<indices.Count;i++)            
                if (id == indices[i]) return i;
            return -1;
        }
        int SearchNearestNext(int id, List<int> indices)
        {
            FLOAT_POINT p1, p2;
            double dist, mindist = 1.0E20;
            int pos = -1;
            p1 = pCoordArray[id];            
            for (int i = 0; i < indices.Count; i++)
            {
                p2 = pCoordArray[indices[i]];
                dist = Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y) + Math.Abs(p1.z - p2.z);
                if ( i == 0 || dist < mindist ) 
                {
                    pos = i;
                    mindist = dist; 
                }
            }
            return pos;
        }
        //转换成多边形序列点   
        public List<Vector32>toPointArray()
        {
            List<Vector32> points = new List<Vector32>();
            List<int> indices = new List<int>();
            
            bool[] addedMarks = new bool[pCoordArray.Count];
            for(int i=0;i<pCoordArray.Count;i++)
                addedMarks[i] = false;
            
            for (int i = 0; i < pLineIndex.Count; i++)
                indices.Add(pLineIndex[i]);
            
            int id1 = indices[0];
            int id2 = indices[1];
            FLOAT_POINT p1= pCoordArray[id1];
            FLOAT_POINT p2 = pCoordArray[id1];
            points.Add(new Vector32(p1.x, p1.y, p1.z));
            points.Add(new Vector32(p2.x, p2.y, p2.z));
            addedMarks[id1] = true;
            addedMarks[id2] = true;

            indices.RemoveRange(0, 2);

            int pos = -1;
            while ( indices.Count > 0 )
            {                
                //search the next of id2
                pos = SearchNext(id2, indices);
                if ( pos < 0 ) //no exact matched,
                {
                    // search the nearest point
                    pos = SearchNearestNext(id2, indices);
                }

                if ( pos % 2 == 0 ) //first point of line
                {
                    id1 = indices[pos];
                    id2 = indices[pos+1];
                    indices.RemoveRange(pos, 2);
                }
                else //second point of line
                {
                    id1 = indices[pos];
                    id2 = indices[pos-1];
                    indices.RemoveRange(pos - 1, 2);
                }
                if (!addedMarks[id1])
                {
                    p1 = pCoordArray[id1];
                    points.Add(new Vector32(p1.x, p1.y, p1.z));
                    addedMarks[id1] = true;
                }
                if (!addedMarks[id2])
                {
                    p2 = pCoordArray[id2];
                    points.Add(new Vector32(p2.x, p2.y, p2.z));
                    addedMarks[id2] = true;
                }

            }//while ( indices.Count > 0 )

            addedMarks = null;
            indices.Clear();
            return points;
        }
        public bool SaveAs(BinaryWriter br)
        {
            try
            {
                br.Write(isoVale);
                br.Write(pCoordArray.Count);
                for (int i = 0; i < pCoordArray.Count; i++)
                {
                    br.Write(pCoordArray[i].x);
                    br.Write(pCoordArray[i].y);
                    br.Write(pCoordArray[i].z);
                }
                br.Write(pLineIndex.Count);
                for (int i = 0; i < pLineIndex.Count; i++)             
                    br.Write(pLineIndex[i]);
                
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
        public bool LoadFrom(BinaryReader br)
        {
            Clear();
            float x, y, z;
            try
            {
                isoVale = br.ReadDouble();
                int n = br.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();
                    pCoordArray.Add(new FLOAT_POINT(x, y, z));
                }
                n = br.ReadInt32();
                for (int i = 0; i < n; i++)                    
                    pLineIndex.Add(br.ReadInt32());

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public void AddPoint(FLOAT_POINT p)
        {
            pCoordArray.Add(p);
        }
        public void AddPoint(float x,float y,float z = 0)
        {
            pCoordArray.Add(new FLOAT_POINT(x, y, z));
        }
        public void AddIndex(int id)
        {
            pLineIndex.Add(id);
        }
        public void Clear()
        {
            pCoordArray.Clear();
            pLineIndex.Clear();
        }
    }
    public class CISOSurface:C3DObjectBase
    {
        public float isoVale;
        public List<FLOAT_POINT> pCoordArray;            //triangle coords array
        public List<int> pTriangleIndex;             //
        public List<FLOAT_POINT> pTriangleNormalArray;           //triangle coords array
        public TextureStruct texture = new TextureStruct();
        public CISOSurface()
        {
            isoVale = 0;
            pCoordArray = new List<FLOAT_POINT>();
            pTriangleIndex = new List<int>();
            pTriangleNormalArray = new List<FLOAT_POINT>();
            type = ShapeEnum.ISOSurface;
        }
        public TriangleObj toTriangleObj()
        {
            TriangleObj obj = new TriangleObj();
            obj.CopyHeaderFrom(this);
            obj.Name += "_";
            obj.Name += isoVale.ToString();
            for (int i = 0; i < pCoordArray.Count; i++)
            {
                obj.AddPoint( pCoordArray[i].x, pCoordArray[i].y, pCoordArray[i].z );
            }
            for (int i = 0; i < pTriangleIndex.Count/3; i++)
            {
                obj.AddTriangleIndex(pTriangleIndex[3*i], pTriangleIndex[3 * i+1], pTriangleIndex[3 * i+2]);
            }
            return obj;
        }

        public override void UpdateRange()
        {
            FLOAT_POINT p;
            for (int i = 0; i < pCoordArray.Count; i++)
            {
                p = pCoordArray[i];
                if (i == 0)
                {
                    minx = maxx = p.x;
                    miny = maxy = p.y;
                    minz = maxz = p.z;
                }
                else
                {
                    if (minx > p.x) minx = p.x;
                    if (miny > p.y) miny = p.y;
                    if (minz > p.z) minz = p.z;
                    if (maxx < p.x) maxx = p.x;
                    if (maxy < p.y) maxy = p.y;
                    if (maxz < p.z) maxz = p.z;
                }
            }
        }
        public override bool SaveAs(BinaryWriter br)
        {
            br.Write(isoVale);
            br.Write((Int32)pCoordArray.Count);
            br.Write((Int32)pTriangleIndex.Count);
            br.Write((Int32)pTriangleNormalArray.Count);

            for(int i=0;i< pCoordArray.Count;i++)
            {
                br.Write(pCoordArray[i].x);
                br.Write(pCoordArray[i].y);
                br.Write(pCoordArray[i].z);
            }
            for (int i = 0; i < pTriangleIndex.Count; i++)
            {
                br.Write((Int32)pTriangleIndex[i]);
            }
            for (int i = 0; i < pTriangleNormalArray.Count; i++)
            {
                br.Write(pTriangleNormalArray[i].x);
                br.Write(pTriangleNormalArray[i].y);
                br.Write(pTriangleNormalArray[i].z);
            }
            return true;
        }
        public override bool LoadFrom(BinaryReader br)
        {
            isoVale = br.ReadSingle();
            int n1 = br.ReadInt32();
            int n2 = br.ReadInt32();
            int n3 = br.ReadInt32();
            if (n1 < 0 || n2 < 0 || n3 < 0) return false;

            pCoordArray.Clear();
            pTriangleIndex.Clear();
            pTriangleNormalArray.Clear();
            float x, y, z;
            for (int i = 0; i < n1; i++)
            {
                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                pCoordArray.Add(new FLOAT_POINT(x, y, z));
            }
            for (int i = 0; i < n2; i++)
            {
                pTriangleIndex.Add(br.ReadInt32());                
            }
            for (int i = 0; i < n3; i++)
            {
                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                pTriangleNormalArray.Add(new FLOAT_POINT( x, y, z) );
            }
            return true;
        }
    }
    public class CISOSurfaceExt: C3DObjectBase,IDisposable
    {
        //triangle coords array
        public List<FLOAT_POINT_EXT> pCoordArray = new List<FLOAT_POINT_EXT>();
        //coord color table
        public List<ColorRGBA> pColor = new List<ColorRGBA>();
        //triangle coord index list
        public List<int> pTriangleIndex = new List<int>();             //
        //triangle normal array
        public List<FLOAT_POINT> pTriangleNormalArray = new List<FLOAT_POINT>();
        public int CoordsCount
        {
            get { return pCoordArray.Count; }
        }
        public int TriangleCount 
        {
            get { return pTriangleIndex.Count/3; }
        }
        public override void UpdateRange()
        {
            FLOAT_POINT_EXT p;
            minx = miny = minz = double.MaxValue;
            maxx = maxy = maxz = double.MinValue;
            for (int i=0;i<pCoordArray.Count;i++)
            {
                p = pCoordArray[i];
                if (!float.IsNaN(p.x) && minx > p.x) minx = p.x;
                if (!float.IsNaN(p.y) && miny > p.y) miny = p.y;
                if (!float.IsNaN(p.z) && minz > p.z) minz = p.z;
                if (!float.IsNaN(p.x) && maxx < p.x) maxx = p.x;
                if (!float.IsNaN(p.y) && maxy < p.y) maxy = p.y;
                if (!float.IsNaN(p.z) && maxz < p.z) maxz = p.z;
            }
        }
        public CISOSurfaceExt()
        {
            type = ShapeEnum.ISOSurfaceEX;
        }
        public TriangleObj toTriangleObj()
        {
            TriangleObj obj = new TriangleObj();
            obj.CopyHeaderFrom(this);
            obj.Name = Name;

            ColorRGBA c;
            for (int i = 0; i < pCoordArray.Count; i++)
            {
                obj.AddPoint(pCoordArray[i].x, pCoordArray[i].y, pCoordArray[i].z);
                c = GetColor(pCoordArray[i].icolor);
                obj.AddPointColor(c.R / 255f, c.G / 255f, c.B / 255f, c.A/255f);
            }
            
            for (int i = 0; i < pTriangleIndex.Count / 3; i++)
            {
                obj.AddTriangleIndex(pTriangleIndex[3 * i], pTriangleIndex[3 * i + 1], pTriangleIndex[3 * i + 2]);
            }
            return obj;
        }
        public bool SaveBinary( BinaryWriter br )
        {
            br.Write(pCoordArray.Count);            
            for (int i = 0; i < pCoordArray.Count; i++)
            {
                br.Write(pCoordArray[i].x);
                br.Write(pCoordArray[i].y);
                br.Write(pCoordArray[i].z);
                br.Write(pCoordArray[i].icolor);
            }

            br.Write(pTriangleIndex.Count);            
            for (int i = 0; i < pTriangleIndex.Count; i++)
            {
                br.Write(pTriangleIndex[i]);
            }

            br.Write(pTriangleNormalArray.Count);
            for (int i = 0; i < pTriangleNormalArray.Count; i++)
            {
                br.Write(pTriangleNormalArray[i].x);
                br.Write(pTriangleNormalArray[i].y);
                br.Write(pTriangleNormalArray[i].z);
            }
            br.Write(pColor.Count);
            for (int i = 0; i < pColor.Count; i++)
            {
                br.Write(pColor[i].R);
                br.Write(pColor[i].G);
                br.Write(pColor[i].B);
                br.Write(pColor[i].A);
            }
            return true;
        }
        public bool LoadBinary(BinaryReader br)
        {
            float x, y, z;
            Int16 icolor;

            pCoordArray.Clear();
            pTriangleIndex.Clear();
            pTriangleNormalArray.Clear();
            pColor.Clear();

            int n1 = br.ReadInt32();
            for (int i = 0; i < n1; i++)
            {
                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                icolor = br.ReadInt16();
                pCoordArray.Add(new FLOAT_POINT_EXT(x, y, z,icolor));
            }
            int n2 = br.ReadInt32();
            for (int i = 0; i < n2; i++)
            {
                pTriangleIndex.Add(br.ReadInt32());
            }
            int n3 = br.ReadInt32();
            for (int i = 0; i < n3; i++)
            {
                x = br.ReadSingle();
                y = br.ReadSingle();
                z = br.ReadSingle();
                pTriangleNormalArray.Add(new FLOAT_POINT(x, y, z));
            }
            byte r, g, b, a;
            int n4 = br.ReadInt32();
            for (int i = 0; i < n4; i++)
            {
                r = br.ReadByte();
                g = br.ReadByte();
                b = br.ReadByte();
                a = br.ReadByte();
                pColor.Add(new ColorRGBA(r,g,b,a));
            }
            return true;
        }
        public override void Clear()
        {
            pCoordArray.Clear();
            pColor.Clear();
            pTriangleIndex.Clear();
            pTriangleNormalArray.Clear();
        }
        public void AddColor(Color p)
        {
            pColor.Add(new ColorRGBA(p));
        }
        public FLOAT_POINT_EXT GetPoint(int id)
        {
            return pCoordArray[id];
        }
        public ColorRGBA GetColor(int index)
        {
            return pColor[index];
        }
        public void AddTriangleIndex(int index)
        {
            pTriangleIndex.Add(index);
        }
        public void AddCoord(FLOAT_POINT_EXT p )
        {
            pCoordArray.Add(p);
        }
        public void Dispose()
        {
            Clear();
        }
    }
    public class MarchingCubes
    {
        //input data
        protected int xGridNum, yGridNum, zGridNum, xyGrid;   //
        public double xMin, yMin, zMin;
        public double xMax, yMax, zMax;
        public double vMin, vMax;
        public double xGridStep, yGridStep, zGridStep;
        protected double xDividedStep, yDividedStep, zDividedStep;        
        protected int dividedNum;     // 2,4,8,16
        bool bUseOldVersion;        //
        public string m_ErrInfo;
        protected EDGE_POINT_INDEX[] pEdgePointArray1 = null;
        protected EDGE_POINT_INDEX[] pEdgePointArray2 = null;
        protected EDGE_POINT_INDEX[] pCurEdgePointArray = null;
        protected EDGE_POINT_INDEX[] pPrevEdgePointArray = null;
        //             y
        //             |-------> y2 
        //             |-------> y1
        //          ---0------->x
        //            / -- z2--
        //           /z -- z1--
        //   OpenGL coord compitable, 
        //	 Z is the elevation in geology
        //   data sort as Z(desend) to YOX

        public float[] pGridData = null;     // grid data array, data  
        public int searchedGridNo = 0;

        public MarchingCubes()
        {
            xGridNum = yGridNum = zGridNum = 0; //
            xMin = yMin = zMin = 0.0;
            xMax = yMax = zMax = 0.0;
            vMin = vMax = 0.0;
            xGridStep = yGridStep = zGridStep = 0.0;            
            m_ErrInfo = "";
            dividedNum = 8;

            pGridData = null;
            pGrigEdgeIndex = null;

            pIsoSurface = new List<CISOSurface>();

            bUseOldVersion = false;

            InitTables();
        }
        public virtual bool SaveBinary(BinaryWriter br)
        {
            br.Write((Int32)pIsoSurface.Count);
            for(int i=0;i< pIsoSurface.Count;i++)
            {
                pIsoSurface[i].SaveAs(br);
            }
            return true;
        }
        public virtual bool LoadBinary(BinaryReader br)
        {
            pIsoSurface.Clear();
            int n1 = br.ReadInt32();            
            for (int i = 0; i < n1; i++)
            {
                CISOSurface iso = new CISOSurface();
                iso.LoadFrom(br);
                pIsoSurface.Add(iso);
            }
            return true;
        }
        // 256 match table	
        protected int[] edgeTable;     //[256]
        protected int[,] triTable;	 //[256][36]
        protected Int16XYZ[] vertEdgeRelation = new Int16XYZ[12];
        protected Int16XYZ[] vertEdgeDirect = new Int16XYZ[12];
        protected void InitEdgeVerticTable()
        {
            vertEdgeDirect[0].x = 1; vertEdgeDirect[0].y = 0; vertEdgeDirect[0].z = 0;    //0-1
            vertEdgeDirect[1].x = 0; vertEdgeDirect[1].y = 0; vertEdgeDirect[1].z = 1;    //1-2
            vertEdgeDirect[2].x = 1; vertEdgeDirect[2].y = 0; vertEdgeDirect[2].z = 0;   //3-2
            vertEdgeDirect[3].x = 0; vertEdgeDirect[3].y = 0; vertEdgeDirect[3].z = 1;    //0-3
            vertEdgeDirect[4].x = 1; vertEdgeDirect[4].y = 0; vertEdgeDirect[4].z = 0;    //4-5
            vertEdgeDirect[5].x = 0; vertEdgeDirect[5].y = 0; vertEdgeDirect[5].z = 1;    //5-6
            vertEdgeDirect[6].x = 1; vertEdgeDirect[6].y = 0; vertEdgeDirect[6].z = 0;   //7-6
            vertEdgeDirect[7].x = 0; vertEdgeDirect[7].y = 0; vertEdgeDirect[7].z = 1;    //4-7
            vertEdgeDirect[8].x = 0; vertEdgeDirect[8].y = 1; vertEdgeDirect[8].z = 0;    //0-4
            vertEdgeDirect[9].x = 0; vertEdgeDirect[9].y = 1; vertEdgeDirect[9].z = 0;    //0-5
            vertEdgeDirect[10].x = 0; vertEdgeDirect[10].y = 1; vertEdgeDirect[10].z = 0;    //0-6
            vertEdgeDirect[11].x = 0; vertEdgeDirect[11].y = 1; vertEdgeDirect[11].z = 0;    //0-7

            vertEdgeRelation[0].x = 0; vertEdgeRelation[0].y = 1;
            vertEdgeRelation[1].x = 1; vertEdgeRelation[1].y = 2;
            vertEdgeRelation[2].x = 3; vertEdgeRelation[2].y = 2;
            vertEdgeRelation[3].x = 0; vertEdgeRelation[3].y = 3;
            vertEdgeRelation[4].x = 4; vertEdgeRelation[4].y = 5;
            vertEdgeRelation[5].x = 5; vertEdgeRelation[5].y = 6;
            vertEdgeRelation[6].x = 7; vertEdgeRelation[6].y = 6;
            vertEdgeRelation[7].x = 4; vertEdgeRelation[7].y = 7;
            vertEdgeRelation[8].x = 0; vertEdgeRelation[8].y = 4;
            vertEdgeRelation[9].x = 1; vertEdgeRelation[9].y = 5;
            vertEdgeRelation[10].x = 2; vertEdgeRelation[10].y = 6;
            vertEdgeRelation[11].x = 3; vertEdgeRelation[11].y = 7;
        }
        public virtual void InitTables()
        {
            InitEdgeVerticTable();
             //marching cubes table data
             edgeTable = new int[]
                {
                    0x0  , 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
                    0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
                    0x190, 0x99 , 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
                    0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
                    0x230, 0x339, 0x33 , 0x13a, 0x636, 0x73f, 0x435, 0x53c,
                    0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
                    0x3a0, 0x2a9, 0x1a3, 0xaa , 0x7a6, 0x6af, 0x5a5, 0x4ac,
                    0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
                    0x460, 0x569, 0x663, 0x76a, 0x66 , 0x16f, 0x265, 0x36c,
                    0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
                    0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff , 0x3f5, 0x2fc,
                    0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
                    0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55 , 0x15c,
                    0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
                    0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc ,
                    0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
                    0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
                    0xcc , 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
                    0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
                    0x15c, 0x55 , 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
                    0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
                    0x2fc, 0x3f5, 0xff , 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
                    0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
                    0x36c, 0x265, 0x16f, 0x66 , 0x76a, 0x663, 0x569, 0x460,
                    0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
                    0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa , 0x1a3, 0x2a9, 0x3a0,
                    0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
                    0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33 , 0x339, 0x230,
                    0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
                    0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99 , 0x190,
                    0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
                    0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0
                };            
            int[,] _triTable = new int[256,16]
            { 
                {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1},
	            {3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1},
	            {3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1},
	            {3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1},
	            {9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
	            {4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1},
                {1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1},
                {9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
                {2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1},
                {8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1},
                {9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
                {4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1},
                {3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1},
                {1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1},
                {4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1},
                {4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1},
                {9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1},
                {1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
                {5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1},
                {2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1},
                {9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
                {0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
                {2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1},
                {10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1},
                {4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1},
                {5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1},
                {5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1},
                {9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1},
                {0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1},
                {1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1},
                {10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1},
                {8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1},
                {2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1},
                {7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1},
                {9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1},
                {2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1},
                {11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1},
                {9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1},
                {5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1},
                {11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1},
                {11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
                {1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1},
                {9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1},
                {5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1},
                {2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
                {0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
                {5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1},
                {6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1},
                {0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1},
                {3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1},
                {6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1},
                {5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1},
                {1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
                {10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1},
                {6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1},
                {1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1},
                {8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1},
                {7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1},
                {3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
                {5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1},
                {0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1},
                {9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1},
                {8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1},
                {5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1},
                {0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1},
                {6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1},
                {10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1},
                {10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1},
                {8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1},
                {1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1},
                {3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1},
                {0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1},
                {10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1},
                {0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1},
                {3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1},
                {6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1},
                {9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1},
                {8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1},
                {3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1},
                {6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1},
                {0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1},
                {10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1},
                {10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1},
                {1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1},
                {2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1},
                {7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1},
                {7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1},
                {2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1},
                {1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1},
                {11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1},
                {8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1},
                {0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1},
                {7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
                {10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
                {2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
                {6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1},
                {7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1},
                {2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1},
                {1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1},
                {10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1},
                {10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1},
                {0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1},
                {7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1},
                {6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1},
                {8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1},
                {9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1},
                {6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1},
                {1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1},
                {4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1},
                {10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1},
                {8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1},
                {0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1},
                {1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1},
                {8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1},
                {10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1},
	            {4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1},
	            {10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
                {5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
                {11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1},
                {9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
                {6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1},
                {7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1},
                {3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1},
                {7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1},
                {9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1},
                {3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1},
                {6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1},
                {9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1},
                {1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1},
                {4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1},
                {7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1},
                {6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1},
                {3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1},
                {0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1},
                {6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1},
                {1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1},
                {0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1},
                {11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1},
                {6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1},
                {5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1},
                {9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1},
                {1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1},
                {1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1},
                {10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1},
                {0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1},
                {5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1},
                {10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1},
                {11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1},
                {0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1},
                {9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1},
                {7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1},
                {2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1},
                {8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1},
                {9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1},
                {9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1},
                {1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1},
                {9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1},
                {9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1},
                {5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1},
                {0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1},
                {10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1},
                {2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1},
                {0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1},
                {0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1},
                {9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1},
                {5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1},
                {3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1},
                {5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1},
                {8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1},
                {0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1},
                {9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1},
                {0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1},
                {1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1},
                {3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1},
                {4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1},
                {9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1},
                {11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1},
                {11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1},
                {2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1},
                {9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1},
                {3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1},
                {1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1},
                {4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1},
                {4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1},
                {0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1},
                {3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1},
                {3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1},
                {0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1},
                {9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1},
                {1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
                {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
          };

        triTable = new int[256,36];
        for ( int i = 0; i< 256; i++ )
		    for( int j = 0; j< 16; j++ )
		        triTable[i,j] = _triTable[i,j] ;
        }
	
	    //---result data----------
	    public List<CISOSurface> pIsoSurface;        //surface array
                                                     //--Functions-----------------
        public virtual void Clear()                       // clear all data 
        {
            pGridData = null;
            pIsoSurface.Clear();
        }
        public void SetDividedNum(int num)
        {
            if (num > 1 && num <= 255)
                dividedNum = num;

            xDividedStep = xGridStep / num;
            yDividedStep = yGridStep / num;
            zDividedStep = zGridStep / num;
        }
        public bool SetInputData(float[] pdata, int nx, int ny, int nz,
                                      double x1, double x2,
                                      double y1, double y2,
                                      double z1, double z2,
                                      double v1, double v2 )
        {
            Clear();

            if (nx < 2 || ny < 2 || nz < 2)
            {
                m_ErrInfo = "data grid not correct.";
                return false;
            }            
            pGridData = pdata;
            if (pGridData == null) return false;
            
            xGridNum = nx;
            yGridNum = ny;
            zGridNum = nz;
            xyGrid = xGridNum * yGridNum;

            SetGridDataRange(x1, x2, y1, y2, z1, z2, v1, v2);

            return true;
        }
        public bool SetGridDataRange( double x1, double x2, 
                                      double y1, double y2,
                                      double z1, double z2, 
                                      double v1, double v2 )
        {
            if (xGridNum < 2 || yGridNum < 2 || zGridNum < 2)
            {
                m_ErrInfo = "data grid is not correct.";
                return false;
            }
            if (x1 >= x2 || y1 >= y2 || z1 >= z2 || v1 >= v2)
            {
                m_ErrInfo = "data range is not correct.";
                return false;
            }
            xMin = x1;
            xMax = x2;
            yMin = y1;
            yMax = y2;
            zMin = z1;
            zMax = z2;
            vMin = v1;
            vMax = v2;
            xGridStep = (x2 - x1) / (xGridNum - 1);
            yGridStep = (y2 - y1) / (yGridNum - 1);
            zGridStep = (z2 - z1) / (zGridNum - 1);
            
            if (dividedNum > 0)
            {
                xDividedStep = xGridStep / dividedNum;
                yDividedStep = yGridStep / dividedNum;
                zDividedStep = zGridStep / dividedNum;
            }
            return true;
        }
        protected void InitEdgePointArray()
        {
            //已创建
            if (pEdgePointArray1 != null && pEdgePointArray2 != null) return;
            pEdgePointArray1 = new EDGE_POINT_INDEX[xyGrid];
            pEdgePointArray2 = new EDGE_POINT_INDEX[xyGrid];
            for (int i = 0; i < xyGrid; i++)
            {
                pEdgePointArray1[i] = new EDGE_POINT_INDEX();
                pEdgePointArray2[i] = new EDGE_POINT_INDEX();
            }
            pCurEdgePointArray = pEdgePointArray1;
            pPrevEdgePointArray = pEdgePointArray2;
        }
        protected void ResetCurEdgePointArray()
        {
            for (int i = 0; i < xyGrid; i++)
            {
                pCurEdgePointArray[i].Reset();
            }
        }
        protected void ReleaseEdgePointArray()
        {
            pEdgePointArray1 = null;
            pEdgePointArray2 = null;
            pCurEdgePointArray = null;
            pPrevEdgePointArray = null;
        }
        protected virtual double GetVerticValue(int icur)
        {
            return pGridData[icur];
        }
        protected virtual double GetVerticValue(int ix, int iy, int iz, int verno)
        {
            return pGridData[GetVerticIndex(ix, iy, iz, verno)];
        }        
        public virtual int GetVerticIndex(int ix, int iy, int iz, int verno)
        {
            int icur = ix + iy * xGridNum + iz * xyGrid;
            int icur1 = icur;
            switch (verno)
            {
                case 0:
                    break;
                case 1:
                    icur1 = icur + 1;
                    break;
                case 2:
                    icur1 = icur + xyGrid + 1;
                    break;
                case 3:
                    icur1 = icur + xyGrid;
                    break;
                case 4:
                    icur1 = icur + xGridNum;
                    break;
                case 5:
                    icur1 = icur + 1 + xGridNum;
                    break;
                case 6:
                    icur1 = icur + xyGrid + 1 + xGridNum;
                    break;
                case 7:
                    icur1 = icur + xyGrid + xGridNum;
                    break;
            }
            return icur1;
        }
        /// <summary>
        /// 根据边号来选择网格主节点(0)序号和坐标轴
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iy"></param>
        /// <param name="iz"></param>
        /// <param name="edgeno"></param>
        /// <returns></returns>
        public virtual int GetVerticIndexByEdge(int ix, int iy, int iz, int edgeno,out AxisEnum axis)
        {
            axis = AxisEnum.xAxis;
            int icur = ix + iy * xGridNum + iz * xyGrid;
            int icur1 = -1;
            switch (edgeno)
            {
                case 0:
                    icur1 = icur;
                    axis = AxisEnum.xAxis;
                    break;
                case 1:
                    icur1 = icur + 1;
                    axis = AxisEnum.zAxis;
                    break;
                case 2:
                    icur1 = icur + xyGrid;
                    axis = AxisEnum.xAxis;
                    break;
                case 3:
                    icur1 = icur;
                    axis = AxisEnum.zAxis;
                    break;
                case 4:
                    icur1 = icur + xGridNum;
                    axis = AxisEnum.xAxis;
                    break;
                case 5:
                    icur1 = icur + 1 + xGridNum;
                    axis = AxisEnum.zAxis;
                    break;
                case 6:
                    icur1 = icur + xyGrid + xGridNum;
                    axis = AxisEnum.xAxis;
                    break;
                case 7:
                    icur1 = icur + xGridNum;
                    axis = AxisEnum.zAxis;
                    break;
                case 8:
                    icur1 = icur;
                    axis = AxisEnum.yAxis;
                    break;
                case 9:
                    icur1 = icur + 1;
                    axis = AxisEnum.yAxis;
                    break;
                case 10:
                    icur1 = icur + 1 + xyGrid;
                    axis = AxisEnum.yAxis;
                    break;
                case 11:
                    icur1 = icur + xyGrid;
                    axis = AxisEnum.yAxis;
                    break;
            }
            return icur1;
        }
        public virtual void GetVerticIndex(int ix, int iy, int iz, int verno,
                                    out int ix1, out int iy1, out int iz1)
        {
            //     |y
            //     4--------5    
            //    /|       /|
            //  7--------6  |
            //  |  0-----|--1--->x
            //  | /      | /
            //  3--------2
            //  /z       
            ix1 = ix;
            iy1 = iy;
            iz1 = iz;
            switch (verno)
            {
                case 0:
                    break;
                case 1:
                    ix1++;
                    break;
                case 2:
                    ix1++;
                    iz1++;
                    break;
                case 3:
                    iz1++;
                    break;
                case 4:
                    iy1++;
                    break;
                case 5:
                    ix1++;
                    iy1++;
                    break;
                case 6:
                    ix1++;
                    iz1++;
                    iy1++;
                    break;
                case 7:
                    iz1++;
                    iy1++;
                    break;
            }
        }        
        protected virtual FLOAT_POINT_EXT GetVerticCoord(int ix, int iy, int iz, int ver)
        {
            FLOAT_POINT_EXT p = new FLOAT_POINT_EXT();
            p.x = (float)(xMin + ix * xGridStep);
            p.y = (float)(yMin + iy * yGridStep);
            p.z = (float)(zMin + iz * zGridStep);
            int icur = ix + iy * xGridNum + iz * xyGrid;
            switch (ver)
            {
                case 0:
                    break;
                case 1:
                    icur += 1;
                    p.x += (float)xGridStep;
                    break;
                case 2:
                    icur += 1;
                    icur += xyGrid;
                    p.x += (float)xGridStep;
                    p.z += (float)zGridStep;
                    break;
                case 3:
                    icur += xyGrid;
                    p.z += (float)zGridStep;
                    break;
                case 4:
                    icur += xGridNum;
                    p.y += (float)yGridStep;
                    break;
                case 5:
                    icur += 1;
                    icur += xGridNum;
                    p.x += (float)xGridStep;
                    p.y += (float)yGridStep;
                    break;
                case 6:
                    icur += 1;
                    icur += xGridNum;
                    icur += xyGrid;
                    p.x += (float)xGridStep;
                    p.y += (float)yGridStep;
                    p.z += (float)zGridStep;
                    break;
                case 7:
                    icur += xyGrid;
                    icur += xGridNum;
                    p.y += (float)yGridStep;
                    p.z += (float)zGridStep;
                    break;
            }           
            return p;
        }
        private GRID_EDGE_INDEX[] pGrigEdgeIndex;    //       
        /*
        public int DoSearchSurface(double isoValue,byte[]pgridshow=null)           // return >0 successs, <0 error
        {
            if (pGridData == null)
            {
                m_ErrInfo = "input data is not ready.";
                return -1;
            }
            if (xGridNum < 2 || yGridNum < 2 || zGridNum < 2)
            {
                m_ErrInfo = "input data is not ready.";
                return -1;
            }
            if (isoValue < vMin || isoValue > vMax)
            {
                m_ErrInfo = "the searched value is not correct.";
                return -2;
            }

            pGrigEdgeIndex = new GRID_EDGE_INDEX[zGridNum * yGridNum * xGridNum];
            if (pGrigEdgeIndex == null)
            {
                m_ErrInfo = "no enough memory.";
                return -3;
            }
            for (int i = 0; i < zGridNum * yGridNum * xGridNum;i++)
                pGrigEdgeIndex[i] = new GRID_EDGE_INDEX();

            if (pgridshow != null) bUseOldVersion = true;
            else bUseOldVersion = false;
            //     |z
            //     4--------5    
            //    /|       /|
            //  7--------6  |
            //  |  0-----|--1--->y
            //  | /      | /
            //  3--------2
            //  /x

            int icur = 0;
            Int16 iType = 0;
            int ix, iy, iz;

            float []gridValueTable = new float[8];    //
            int []pGridIndex = new int[8];  //	
            
            for (iz = 0; iz < zGridNum - 1; iz++)
                for (iy = 0; iy < yGridNum - 1; iy++)
                    for (ix = 0; ix < xGridNum - 1; ix++)
                    {
                        icur = iz * xyGrid + iy * xGridNum + ix;

                        if( pgridshow!= null )
                            if (pgridshow[icur] == 0) continue;

                        //get the index number				
                        pGridIndex[0] = icur;
                        pGridIndex[1] = pGridIndex[0] + xGridNum;
                        pGridIndex[2] = pGridIndex[1] + 1;
                        pGridIndex[3] = pGridIndex[0] + 1;
                        pGridIndex[4] = pGridIndex[0] + xyGrid;
                        pGridIndex[5] = pGridIndex[1] + xyGrid;
                        pGridIndex[6] = pGridIndex[2] + xyGrid;
                        pGridIndex[7] = pGridIndex[3] + xyGrid;
                        //get value of each cube grid points
                        gridValueTable[0] = pGridData[pGridIndex[0]];
                        gridValueTable[1] = pGridData[pGridIndex[1]];
                        gridValueTable[2] = pGridData[pGridIndex[2]];
                        gridValueTable[3] = pGridData[pGridIndex[3]];
                        gridValueTable[4] = pGridData[pGridIndex[4]];
                        gridValueTable[5] = pGridData[pGridIndex[5]];
                        gridValueTable[6] = pGridData[pGridIndex[6]];
                        gridValueTable[7] = pGridData[pGridIndex[7]];

                        iType = 0;
                        if (gridValueTable[0] < isoValue) iType |= 1;
                        if (gridValueTable[1] < isoValue) iType |= 2;
                        if (gridValueTable[2] < isoValue) iType |= 4;
                        if (gridValueTable[3] < isoValue) iType |= 8;
                        if (gridValueTable[4] < isoValue) iType |= 16;
                        if (gridValueTable[5] < isoValue) iType |= 32;
                        if (gridValueTable[6] < isoValue) iType |= 64;
                        if (gridValueTable[7] < isoValue) iType |= 128;

                        if (edgeTable[iType] != 0)
                        {
                            if(bUseOldVersion)
                                GetGridEdgeIntersetion_OldVersion(icur, ix, iy, iz, iType, (float)isoValue, gridValueTable);
                            else
                                GetGridEdgeIntersetion(icur, ix, iy, iz, iType, (float)isoValue, gridValueTable);
                        }

                    }//for( iz = 0; iz < zGridNum - 1; iz++ )

            // Extract Triangle from pGrigEdgeIndex
            ExtractTriangle((float)isoValue);
            //
            Array.Clear(pGrigEdgeIndex, 0, zGridNum * xyGrid);            
            
            return 1;
        }
        */        
        public virtual int GetGridType(int ix,int iy,int iz,double isoValue)
        {
            int iType = 0;
            for (int i = 0; i < 8; i++)
            {
                if ( GetVerticValue(ix, iy, iz, i) < isoValue)
                    iType |= (1 << i);
            }
            return iType;
        }
        // return >0 successs, <0 error
        public int DoSearchSurface(double isoValue, TextureStruct texture, byte[] pgridshow = null)
        {
            //     |z
            //     4--------5    
            //    /|       /|
            //  7--------6  |
            //  |  0-----|--1--->y
            //  | /      | /
            //  3--------2
            //  /x
            if (pGridData == null)
            {
                m_ErrInfo = "input data is not ready.";
                return -1;
            }
            if (xGridNum < 2 || yGridNum < 2 || zGridNum < 2)
            {
                m_ErrInfo = "input data is not ready.";
                return -1;
            }
            if (isoValue < vMin || isoValue > vMax)
            {
                m_ErrInfo = "the searched value is not correct.";
                return -2;
            }
            ////////////////////////////////////////
            int iType = 0;
            int ix, iy, iz;  
            
            EDGE_POINT_INDEX[] pnext;

            CISOSurface sf = new CISOSurface();
            sf.isoVale = (float)isoValue;
            sf.texture = texture;

            InitEdgePointArray();
            pCurEdgePointArray = pEdgePointArray1;
            pPrevEdgePointArray = pEdgePointArray2;

            for (iz = 0; iz < zGridNum - 1; iz++)
            {
                for (iy = 0; iy < yGridNum - 1; iy++)
                    for (ix = 0; ix < xGridNum - 1; ix++)
                    {
                        //icur = iz * xyGrid + iy * xGridNum + ix;

                        //if(pgridshow!=null)
                        // if (pgridshow[icur] == 0) continue;

                        //get the index number				
                        iType = GetGridType(ix,iy,iz,isoValue);
                        if (iType != 0 && iType != 255)
                        {
                            ExtractTriangleFromGrid(sf,isoValue,ix, iy, iz, iType);
                            searchedGridNo ++;
                        }
                        
                    }//for (iy = 0; iy < yGridNum - 1; iy++)

                pnext = pPrevEdgePointArray;
                pPrevEdgePointArray = pCurEdgePointArray;
                pCurEdgePointArray = pnext;
                ResetCurEdgePointArray();
            }//for( iz = 0; iz < zGridNum - 1; iz++ )
            ReleaseEdgePointArray();
            sf.UpdateRange();
            pIsoSurface.Add(sf);

            return 1;
        }

        public bool CaculateTriangleNormals(CISOSurface sf )//Caculate Triangles normal while visualization
        {
            int n1 = sf.pCoordArray.Count;
            if (n1 < 3) return false;

            int n2 = sf.pTriangleIndex.Count;
            int n3 = sf.pTriangleNormalArray.Count;
            if (n3 == n2 / 3) return true;

            sf.pTriangleNormalArray.Clear();
            FLOAT_POINT p1, p2, p3, p;

            float w0, w1, w2, v0, v1, v2, nr, nx, ny, nz;

            p.x = 0.0f;
            p.y = 0.0f;
            p.z = 1.0f;
            int i1, i2, i3;
            for (int i = 0; i < n2 / 3; i++)
            {
                i1 = sf.pTriangleIndex[3 * i] - 1;
                i2 = sf.pTriangleIndex[3 * i + 1] - 1;
                i3 = sf.pTriangleIndex[3 * i + 2] - 1;

                if (i1 < 0 || i2 < 0 || i3 < 0)
                {
                    p.x = 0.0f;
                    p.y = 0.0f;
                    p.z = 1.0f;
                    sf.pTriangleNormalArray.Add(p);
                    continue;
                }

                p1 = sf.pCoordArray[i1];
                p2 = sf.pCoordArray[i2];
                p3 = sf.pCoordArray[i3];

                w0 = p1.x - p2.x; w1 = p1.y - p2.y; w2 = p1.z - p2.z;
                v0 = p3.x - p2.x; v1 = p3.y - p2.y; v2 = p3.z - p2.z;

                nx = (w1 * v2 - w2 * v1);
                ny = (w2 * v0 - w0 * v2);
                nz = (w0 * v1 - w1 * v0);
                nr = (float)Math.Sqrt( (double)(nx * nx + ny * ny + nz * nz ) );
                p.x = nx / nr;
                p.y = ny / nr;
                p.z = nz / nr;
                sf.pTriangleNormalArray.Add(p);
            }
            return true;
        }
        public bool CaculateTriangleNormals(int i)
        {
            if (i < 0 || i >= pIsoSurface.Count ) return false;
            return CaculateTriangleNormals(pIsoSurface[i]);
        }
        public string GetErrorInfo() { return m_ErrInfo; }     // get error info while error occurred

        //	
        // the old version - not optimizted
        //create Triangle point index in a cube, and store to pGridPointArray
        protected int GetGridEdgeIntersetion_OldVersion(int iGrid, int ix, int iy, int iz, Int16 iType, float isoValue, float []gridValueTable )
        {
            //Find the vertices where the surface intersects the cube
            //x:0 4 8 9  === x-1: 2 6 11 10
            //y:3 7 8 11 === y-1: 1 5 9 10
            //z:0 1 2 3  === z-1: 4 5 6 7
            pGrigEdgeIndex[iGrid].typeIndex = iType;

            if ( (edgeTable[iType] & 1) > 0 )   // 0-->1  	
                pGrigEdgeIndex[iGrid].pDivide[0] = (byte)VertexInterp(gridValueTable[0], gridValueTable[1], isoValue);

            if ( (edgeTable[iType] & 2) > 0 )   // 1-->2	
                pGrigEdgeIndex[iGrid].pDivide[1] = (byte)VertexInterp(gridValueTable[1], gridValueTable[2], isoValue);

            if ( (edgeTable[iType] & 4) > 0 )   // 3-->2	
                pGrigEdgeIndex[iGrid].pDivide[2] = (byte)VertexInterp(gridValueTable[3], gridValueTable[2], isoValue);

            if ( (edgeTable[iType] & 8) > 0 )   // 0-->3	
                pGrigEdgeIndex[iGrid].pDivide[3] = (byte)VertexInterp(gridValueTable[0], gridValueTable[3], isoValue);

            if ( (edgeTable[iType] & 16) > 0 )  // 4-->5	
                pGrigEdgeIndex[iGrid].pDivide[4] = (byte)VertexInterp(gridValueTable[4], gridValueTable[5], isoValue);

            if ( (edgeTable[iType] & 32) > 0 )  // 5-->6
                pGrigEdgeIndex[iGrid].pDivide[5] = (byte)VertexInterp(gridValueTable[5], gridValueTable[6], isoValue);

            if ( (edgeTable[iType] & 64) > 0 )  // 7-->6
                pGrigEdgeIndex[iGrid].pDivide[6] = (byte)VertexInterp(gridValueTable[7], gridValueTable[6], isoValue);

            if ( (edgeTable[iType] & 128) > 0 ) // 4-->7
                pGrigEdgeIndex[iGrid].pDivide[7] = (byte)VertexInterp(gridValueTable[4], gridValueTable[7], isoValue);

            if ( (edgeTable[iType] & 256) > 0 ) // 0-->4
                pGrigEdgeIndex[iGrid].pDivide[8] = (byte)VertexInterp(gridValueTable[0], gridValueTable[4], isoValue);

            if ( (edgeTable[iType] & 512) > 0 ) // 1-->5
                pGrigEdgeIndex[iGrid].pDivide[9] = (byte)VertexInterp(gridValueTable[1], gridValueTable[5], isoValue);

            if ( (edgeTable[iType] & 1024) > 0 )    // 2-->6
                pGrigEdgeIndex[iGrid].pDivide[10] = (byte)VertexInterp(gridValueTable[2], gridValueTable[6], isoValue);

            if ( (edgeTable[iType] & 2048) > 0 )    // 3-->7
                pGrigEdgeIndex[iGrid].pDivide[11] = (byte)VertexInterp(gridValueTable[3], gridValueTable[7], isoValue);

            return 1;
        }
        private int GetGridEdgeIntersetion( int iGrid, int ix, int iy, int iz, 
                                            Int16 iType, float isoValue, float []gridValueTable )
        {
            //Find the vertices where the surface intersects the cube
            //x:0 4 8 9  === x-1: 2 6 11 10
            //y:3 7 8 11 === y-1: 1 5 9 10
            //z:0 1 2 3  === z-1: 4 5 6 7
            pGrigEdgeIndex[iGrid].typeIndex = iType;

            if ( (edgeTable[iType] & 1) >0 )   // 0-->1  
            {
                if (ix > 0) pGrigEdgeIndex[iGrid].pDivide[0] = pGrigEdgeIndex[iGrid - 1].pDivide[2];
                else if (iz > 0) pGrigEdgeIndex[iGrid].pDivide[0] = pGrigEdgeIndex[iGrid - xyGrid].pDivide[4];
                else pGrigEdgeIndex[iGrid].pDivide[0] = (byte)VertexInterp(gridValueTable[0], gridValueTable[1], isoValue);
            }
            if ( (edgeTable[iType] & 2) >0 )   // 1-->2
            {
                if (iz > 0) pGrigEdgeIndex[iGrid].pDivide[1] = pGrigEdgeIndex[iGrid - xyGrid].pDivide[5];
                else pGrigEdgeIndex[iGrid].pDivide[1] = (byte)VertexInterp(gridValueTable[1], gridValueTable[2], isoValue);
            }
            if ( (edgeTable[iType] & 4) > 0 )   // 3-->2
            {
                if (iz > 0) pGrigEdgeIndex[iGrid].pDivide[2] = pGrigEdgeIndex[iGrid - xyGrid].pDivide[6];
                else pGrigEdgeIndex[iGrid].pDivide[2] = (byte)VertexInterp(gridValueTable[3], gridValueTable[2], isoValue);
            }
            if ( (edgeTable[iType] & 8) > 0)   // 0-->3
            {
                if (iy > 0) pGrigEdgeIndex[iGrid].pDivide[3] = pGrigEdgeIndex[iGrid - xGridNum].pDivide[1];
                else if (iz > 0) pGrigEdgeIndex[iGrid].pDivide[3] = pGrigEdgeIndex[iGrid - xyGrid].pDivide[7];
                else pGrigEdgeIndex[iGrid].pDivide[3] = (byte)VertexInterp(gridValueTable[0], gridValueTable[3], isoValue);
            }
            if ( (edgeTable[iType] & 16) > 0)  // 4-->5
            {
                if (ix > 0) pGrigEdgeIndex[iGrid].pDivide[4] = pGrigEdgeIndex[iGrid - 1].pDivide[6];
                else pGrigEdgeIndex[iGrid].pDivide[4] = (byte)VertexInterp(gridValueTable[4], gridValueTable[5], isoValue);
            }
            if ( (edgeTable[iType] & 32) > 0)  // 5-->6
            {
                pGrigEdgeIndex[iGrid].pDivide[5] = (byte)VertexInterp(gridValueTable[5], gridValueTable[6], isoValue);
            }
            if ( (edgeTable[iType] & 64) > 0)  // 7-->6
            {
                pGrigEdgeIndex[iGrid].pDivide[6] = (byte)VertexInterp(gridValueTable[7], gridValueTable[6], isoValue);
            }
            if ( (edgeTable[iType] & 128) > 0) // 4-->7
            {
                if (iy > 0) pGrigEdgeIndex[iGrid].pDivide[7] = pGrigEdgeIndex[iGrid - xGridNum].pDivide[5];
                else pGrigEdgeIndex[iGrid].pDivide[7] = (byte)VertexInterp(gridValueTable[4], gridValueTable[7], isoValue);
            }
            if ( (edgeTable[iType] & 256) > 0) // 0-->4
            {
                if (ix > 0) pGrigEdgeIndex[iGrid].pDivide[8] = pGrigEdgeIndex[iGrid - 1].pDivide[11];
                else if (iy > 0) pGrigEdgeIndex[iGrid].pDivide[8] = pGrigEdgeIndex[iGrid - xGridNum].pDivide[9];
                else pGrigEdgeIndex[iGrid].pDivide[8] = (byte)VertexInterp(gridValueTable[0], gridValueTable[4], isoValue);
            }
            if ( (edgeTable[iType] & 512) > 0) // 1-->5
            {
                if (ix > 0) pGrigEdgeIndex[iGrid].pDivide[9] = pGrigEdgeIndex[iGrid - 1].pDivide[10];
                else pGrigEdgeIndex[iGrid].pDivide[9] = (byte)VertexInterp(gridValueTable[1], gridValueTable[5], isoValue);
            }
            if ( (edgeTable[iType] & 1024) > 0)    // 2-->6
            {
                pGrigEdgeIndex[iGrid].pDivide[10] = (byte)VertexInterp(gridValueTable[2], gridValueTable[6], isoValue);
            }
            if ( (edgeTable[iType] & 2048) > 0)    // 3-->7
            {
                if (iy > 0) pGrigEdgeIndex[iGrid].pDivide[11] = pGrigEdgeIndex[iGrid - xGridNum].pDivide[10];
                else pGrigEdgeIndex[iGrid].pDivide[11] = (byte)VertexInterp(gridValueTable[3], gridValueTable[7], isoValue);
            }
            return 1;

        }
        //the old version - including duplicated points
        private int ExtractTriangleFromGrid_OldVersion(CISOSurface sf, int ix, int iy, int iz, GRID_EDGE_INDEX ed)
        {
            int edno;
            int it; //triangle point index
            FLOAT_POINT p0, p;
            p0.x = (float)(xMin + ix * xGridStep);
            p0.y = (float)(yMin + iy * yGridStep);
            p0.z = (float)(zMin + iz * zGridStep);

            for (int i = 0; ; i++)
            {
                edno = triTable[ed.typeIndex,i];
                if (edno == -1) break;
                switch (edno)
                {
                    case 0: // 0->1
                        p = p0;
                        p.y += (float)(yDividedStep * (ed.pDivide[0] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;

                    case 1: //1->2
                        p = p0;
                        p.y += (float)yGridStep;
                        p.x += (float)(xDividedStep * (ed.pDivide[1] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;
                    case 2: //3->2
                        p = p0;
                        p.x += (float)xGridStep;
                        p.y += (float)(yDividedStep * (ed.pDivide[2] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;

                    case 3: //0->3
                        p = p0;
                        p.x += (float)(xDividedStep * (ed.pDivide[3] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;
                    case 4: //4->5
                        p = p0;
                        p.z += (float)zGridStep;
                        p.y += (float)(yDividedStep * (ed.pDivide[4] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;

                    case 5: //5->6
                        p = p0;
                        p.z += (float)zGridStep;
                        p.y += (float)yGridStep;
                        p.x += (float)(xDividedStep * (ed.pDivide[5] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;

                    case 6: //7->6
                        p = p0;
                        p.z += (float)zGridStep;
                        p.x += (float)xGridStep;
                        p.y += (float)(yDividedStep * (ed.pDivide[6] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;

                    case 7: //4->7				
                        p = p0;
                        p.z += (float)zGridStep;
                        p.x += (float)(xDividedStep * (ed.pDivide[7] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;
                    //vert 
                    case 8: //0->4				
                        p = p0;
                        p.z += (float)(zDividedStep * (ed.pDivide[8] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;
                    case 9: //1->5				
                        p = p0;
                        p.y += (float)yGridStep;
                        p.z += (float)(zDividedStep * (ed.pDivide[9] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;

                    case 10:    //2->6				
                        p = p0;
                        p.x += (float)xGridStep;
                        p.y += (float)yGridStep;
                        p.z += (float)(zDividedStep * (ed.pDivide[10] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;
                    case 11:    //3->7				
                        p = p0;
                        p.x += (float)xGridStep;
                        p.z += (float)(zDividedStep * (ed.pDivide[11] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        break;
                }//switch
            }//for
            return 1;
        }

        //store vertic index,ix,iy,iz cube,
        private void StoreEdgeIndex(int ix, int iy, int iz, int edge, int id)
        {
            //current cube index
            int icur = ix + iy * xGridNum;
            // store to current cube first
            pCurEdgePointArray[icur].pIndex[edge] = id;
        }
        private int GetStoredEdgeIndex(int ix, int iy, int iz, int edge)
        {
            //     |z
            //     o---4----o    
            //   7/|8      /|
            //  o----6---o  |9
            //11|  o---0-|--o--->y
            //  |3/      | /1
            //  o----2---o
            //  /x  c     

            int icur = ix + iy * xGridNum;
            int iprev;

            // check the current layer first
            int edgeIndex = pCurEdgePointArray[icur].pIndex[edge];
            if (edgeIndex >= 0) return edgeIndex;

            //else check the previous Layer
            byte[] xedge = new byte[4] { 3, 7, 8, 11 };
            byte[] xedge1 = new byte[4] { 1, 5, 9, 10 };
            byte[] yedge = new byte[4] { 0, 1, 2, 3 };
            byte[] yedge1 = new byte[4] { 4, 5, 6, 7 };
            byte[] zedge = new byte[4] { 0, 4, 8, 9 };
            byte[] zedge1 = new byte[4] { 2, 6, 11, 10 };
            // current cube index

            if (ix > 0)
            {
                //37811-15910
                iprev = (ix - 1) + iy * xGridNum;
                for (int i = 0; i < 4; i++)
                {
                    if (edge == xedge[i])
                    {
                        if (pCurEdgePointArray[iprev].pIndex[xedge1[i]] >= 0)
                            return pCurEdgePointArray[iprev].pIndex[xedge1[i]];
                    }
                }
            }
            if (iy > 0)
            {
                //37811-15910
                iprev = ix + (iy - 1) * xGridNum;
                for (int i = 0; i < 4; i++)
                {
                    if (edge == yedge[i])
                    {
                        if (pCurEdgePointArray[iprev].pIndex[yedge1[i]] >= 0)
                            return pCurEdgePointArray[iprev].pIndex[yedge1[i]];
                    }
                }
            }
            if (iz > 0)
            {
                //37811-15910
                iprev = ix + iy * xGridNum;
                for (int i = 0; i < 4; i++)
                {
                    if (edge == zedge[i])
                    {
                        if (pPrevEdgePointArray[iprev].pIndex[zedge1[i]] >= 0)
                            return pPrevEdgePointArray[iprev].pIndex[zedge1[i]];
                    }
                }
            }

            return -1;
        }
        private FLOAT_POINT GetEdgeCoord(double isoValue,int ix, int iy, int iz, int edno)
        {
            double v1 = GetVerticValue(ix, iy, iz, vertEdgeRelation[edno].x);
            double v2 = GetVerticValue(ix, iy, iz, vertEdgeRelation[edno].y);
            if (v2 == v1) return new FLOAT_POINT(0, 0, 0);
            float scale = (float)( (isoValue - v1) / (v2 - v1) );

            FLOAT_POINT p1 = GetVerticCoord(ix, iy, iz, vertEdgeRelation[edno].x);
            FLOAT_POINT p2 = GetVerticCoord(ix, iy, iz, vertEdgeRelation[edno].y);
            return new FLOAT_POINT( p1.x + scale * (p2.x - p1.x),
                                    p1.y + scale * (p2.y - p1.y),
                                    p1.z + scale * (p2.z - p1.z) );
        }
        private int ExtractTriangleFromGrid(CISOSurface sf,double isoValue, int ix, int iy, int iz, int typeIndex)
        {
            //     |z
            //     4----f---5    
            //    /|       /|
            //  7--------6  |
            //  |  0---a-|--1--->y
            //  |d/      | /b
            //  3--------2
            //  /x  c
            int edno, edIndex;
            // searching the edge table,till -1 end
            for (int i = 0; i < 24; i++)
            {
                edno = triTable[typeIndex, i];
                if (edno < 0) break;

                // check if the same edge coords been stored
                edIndex = GetStoredEdgeIndex(ix, iy, iz, edno);
                if (edIndex >= 0)
                {
                    sf.pTriangleIndex.Add(edIndex);
                    StoreEdgeIndex(ix, iy, iz, edno, edIndex);
                }
                else
                {   //else if no stored, create new
                    FLOAT_POINT p = GetEdgeCoord(isoValue,ix, iy, iz, edno);
                    
                    sf.pTriangleIndex.Add( sf.pCoordArray.Count );
                    StoreEdgeIndex(ix, iy, iz, edno, sf.pCoordArray.Count );

                    sf.pCoordArray.Add(p);
                    
                }
            }//for
            return 1;
        }
        private int ExtractTriangleFromGrid( CISOSurface sf, int ix, int iy, int iz,
                                             GRID_EDGE_INDEX ed,
                                             EDGE_POINT_INDEX[] p1, EDGE_POINT_INDEX[] p2,
                                             int iloop)
        {
            //x:0 4 8 9  === x-1: 2 6 11 10
            //y:3 7 8 11 === y-1: 1 5 9 10
            //z:0 1 2 3  === z-1: 4 5 6 7

            int edno;
            int it; //triangle point index
            FLOAT_POINT p0, p;
            p0.x = (float)(xMin + ix * xGridStep);
            p0.y = (float)(yMin + iy * yGridStep);
            p0.z = (float)(zMin + iz * zGridStep);

            for (int i = 0; ; i++)
            {
                edno = triTable[ed.typeIndex, i];
                if (edno == -1) break;
                switch (edno)
                {
                    case 0: // 0->1
                        if (ix > 0) it = p2[iloop - 1].pIndex[2];
                        else if (iz > 0) it = p1[iloop].pIndex[4];
                        else
                        {
                            p = p0;
                            p.y += (float)(yDividedStep * (ed.pDivide[0] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[0] = it;
                        break;

                    case 1: //1->2
                        if (iz > 0) it = p1[iloop].pIndex[5];
                        else
                        {
                            p = p0;
                            p.y += (float)yGridStep;
                            p.x += (float)(xDividedStep * (ed.pDivide[1] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[1] = it;
                        break;

                    case 2: //3->2
                        if (iz > 0) it = p1[iloop].pIndex[6];
                        else
                        {
                            p = p0;
                            p.x += (float)xGridStep;
                            p.y += (float)(yDividedStep * (ed.pDivide[2] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[2] = it;
                        break;

                    case 3: //0->3
                        if (iy > 0) it = p2[iloop - xGridNum].pIndex[1];
                        else if (iz > 0) it = p1[iloop].pIndex[7];
                        else
                        {
                            p = p0;
                            p.x += (float)(xDividedStep * (ed.pDivide[3] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[3] = it;
                        break;

                    case 4: //4->5
                        if (ix > 0) it = p2[iloop - 1].pIndex[6];
                        else
                        {
                            p = p0;
                            p.z += (float)zGridStep;
                            p.y += (float)(yDividedStep * (ed.pDivide[4] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[4] = it;
                        break;

                    case 5: //5->6
                        p = p0;
                        p.z += (float)zGridStep;
                        p.y += (float)yGridStep;
                        p.x += (float)(xDividedStep * (ed.pDivide[5] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[5] = it;
                        break;

                    case 6: //7->6
                        p = p0;
                        p.z += (float)zGridStep;
                        p.x += (float)xGridStep;
                        p.y += (float)(yDividedStep * (ed.pDivide[6] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[6] = it;
                        break;

                    case 7: //4->7
                        if (iy > 0) it = p2[iloop - xGridNum].pIndex[5];
                        else
                        {
                            p = p0;
                            p.z += (float)zGridStep;
                            p.x += (float)(xDividedStep * (ed.pDivide[7] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[7] = it;
                        break;

                    //vert 
                    case 8: //0->4
                        if (ix > 0) it = p2[iloop - 1].pIndex[11];
                        else if (iy > 0) it = p2[iloop - xGridNum].pIndex[9];
                        else
                        {
                            p = p0;
                            p.z += (float)(zDividedStep * (ed.pDivide[8] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[8] = it;
                        break;

                    case 9: //1->5
                        if (ix > 0) it = p2[iloop - 1].pIndex[10];
                        else
                        {
                            p = p0;
                            p.y += (float)yGridStep;
                            p.z += (float)(zDividedStep * (ed.pDivide[9] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[9] = it;
                        break;

                    case 10:    //2->6				
                        p = p0;
                        p.x += (float)xGridStep;
                        p.y += (float)yGridStep;
                        p.z += (float)(zDividedStep * (ed.pDivide[10] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[10] = it;
                        break;
                    case 11:    //3->7
                        if (iy > 0) it = p2[iloop - xGridNum].pIndex[10];
                        else
                        {
                            p = p0;
                            p.x += (float)xGridStep;
                            p.z += (float)(zDividedStep * (ed.pDivide[11] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[11] = it;
                        break;
                }//switch
            }//for
            return 1;
        }
        /*
        private int ExtractTriangleFromGrid( CISOSurface sf, int ix, int iy, int iz, 
                                             GRID_EDGE_INDEX ed,
                                             EDGE_POINT_INDEX[] p1, EDGE_POINT_INDEX[] p2, 
                                             int iloop )
        {
            //x:0 4 8 9  === x-1: 2 6 11 10
            //y:3 7 8 11 === y-1: 1 5 9 10
            //z:0 1 2 3  === z-1: 4 5 6 7

            int edno;
            int it; //triangle point index
            FLOAT_POINT p0, p;
            p0.x = (float)(xMin + ix * xGridStep);
            p0.y = (float)(yMin + iy * yGridStep);
            p0.z = (float)(zMin + iz * zGridStep);

            for (int i = 0; ; i++)
            {
                edno = triTable[ed.typeIndex,i];
                if (edno == -1) break;
                switch (edno)
                {
                    case 0: // 0->1
                        if (ix > 0) it = p2[iloop-1].pIndex[2];
                        else if (iz > 0) it = p1[iloop].pIndex[4];
                        else
                        {
                            p = p0;
                            p.y += (float)(yDividedStep * (ed.pDivide[0] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[0] = it;
                        break;

                    case 1: //1->2
                        if (iz > 0) it = p1[iloop].pIndex[5];
                        else
                        {
                            p = p0;
                            p.y += (float)yGridStep;
                            p.x += (float)(xDividedStep * (ed.pDivide[1] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[1] = it;
                        break;

                    case 2: //3->2
                        if (iz > 0) it = p1[iloop].pIndex[6];
                        else
                        {
                            p = p0;
                            p.x += (float)xGridStep;
                            p.y += (float)(yDividedStep * (ed.pDivide[2] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[2] = it;
                        break;

                    case 3: //0->3
                        if (iy > 0) it = p2[iloop - xGridNum].pIndex[1];
                        else if (iz > 0) it = p1[iloop].pIndex[7];
                        else
                        {
                            p = p0;
                            p.x += (float)(xDividedStep * (ed.pDivide[3] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[3] = it;
                        break;

                    case 4: //4->5
                        if (ix > 0) it = p2[iloop - 1].pIndex[6];
                        else
                        {
                            p = p0;
                            p.z += (float)zGridStep;
                            p.y += (float)(yDividedStep * (ed.pDivide[4] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[4] = it;
                        break;

                    case 5: //5->6
                        p = p0;
                        p.z += (float)zGridStep;
                        p.y += (float)yGridStep;
                        p.x += (float)(xDividedStep * (ed.pDivide[5] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[5] = it;
                        break;

                    case 6: //7->6
                        p = p0;
                        p.z += (float)zGridStep;
                        p.x += (float)xGridStep;
                        p.y += (float)(yDividedStep * (ed.pDivide[6] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[6] = it;
                        break;

                    case 7: //4->7
                        if (iy > 0) it = p2[iloop - xGridNum].pIndex[5];
                        else
                        {
                            p = p0;
                            p.z += (float)zGridStep;
                            p.x += (float)(xDividedStep * (ed.pDivide[7] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[7] = it;
                        break;

                    //vert 
                    case 8: //0->4
                        if (ix > 0) it = p2[iloop - 1].pIndex[11];
                        else if (iy > 0) it = p2[iloop - xGridNum].pIndex[9];
                        else
                        {
                            p = p0;
                            p.z += (float)(zDividedStep * (ed.pDivide[8] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[8] = it;
                        break;

                    case 9: //1->5
                        if (ix > 0) it = p2[iloop - 1].pIndex[10];
                        else
                        {
                            p = p0;
                            p.y += (float)yGridStep;
                            p.z += (float)(zDividedStep * (ed.pDivide[9] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[9] = it;
                        break;

                    case 10:    //2->6				
                        p = p0;
                        p.x += (float)xGridStep;
                        p.y += (float)yGridStep;
                        p.z += (float)(zDividedStep * (ed.pDivide[10] - 1));
                        sf.pCoordArray.Add(p);
                        it = sf.pCoordArray.Count;
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[10] = it;
                        break;
                    case 11:    //3->7
                        if (iy > 0) it = p2[iloop - xGridNum].pIndex[10];
                        else
                        {
                            p = p0;
                            p.x += (float)xGridStep;
                            p.z += (float)(zDividedStep * (ed.pDivide[11] - 1));
                            sf.pCoordArray.Add(p);
                            it = sf.pCoordArray.Count;
                        }
                        sf.pTriangleIndex.Add(it);
                        p2[iloop].pIndex[11] = it;
                        break;
                }//switch
            }//for
            return 1;
        }
        */
        private int ExtractTriangle(float isoValue)
        {
            //x:0 4 8 9  === x-1: 2 6 11 10
            //y:3 7 8 11 === y-1: 1 5 9 10
            //z:0 1 2 3  === z-1: 4 5 6 7

            int icur = 0;
            int iloop = 0;
            int ix, iy, iz;

            EDGE_POINT_INDEX[] pm1 = new EDGE_POINT_INDEX[xGridNum * yGridNum];
            EDGE_POINT_INDEX[] pm2 = new EDGE_POINT_INDEX[xGridNum * yGridNum];
            EDGE_POINT_INDEX[] p1,p2,p;

            for(int i=0;i< xGridNum * yGridNum; i++)
            {
                pm1[i] = new EDGE_POINT_INDEX();
                pm2[i] = new EDGE_POINT_INDEX();
            }
            CISOSurface sf = new CISOSurface();
            sf.isoVale = isoValue;

            int iType = 0;
            p1 = pm1; p2 = pm2; p = null;
            for (iz = 0; iz < zGridNum - 1; iz++)
            {
                iloop = 0;
                for (iy = 0; iy < yGridNum - 1; iy++)
                {
                    for (ix = 0; ix < xGridNum - 1; ix++)
                    {
                        icur = iz * xyGrid + iy * xGridNum + ix;
                        iloop = iy * xGridNum + ix;
                        iType = pGrigEdgeIndex[icur].typeIndex;

                        if (triTable[iType, 0] >= 0)
                        {
                            if( bUseOldVersion )
                                ExtractTriangleFromGrid_OldVersion( sf, ix, iy, iz, pGrigEdgeIndex[icur] );
                             else ExtractTriangleFromGrid(sf, ix, iy, iz, pGrigEdgeIndex[icur], p1, p2, iloop);                            
                        }
                    }
                }
                p = p1;
                p1 = p2;
                p2 = p;
            }

            Array.Clear(pm1,0, xGridNum * yGridNum);
            Array.Clear(pm2, 0, xGridNum * yGridNum);
            
            //caculate the normal of each triangle
            CaculateTriangleNormals(sf);

            pIsoSurface.Add(sf);

            return sf.pCoordArray.Count;
        }

        private int VertexInterp(float v1, float v2, float isoValue)
        {
            float vstep = (v2 - v1) / dividedNum;
            float divide = (isoValue - v1) / vstep;

            if ((divide - (int)divide) >= 0.5)
                return (int)divide + 2;
            else return (int)divide + 1;
        }        
    }
    public class MarchingCubes2D : MarchingCubes
    {
        public List<C2DISOSurface> p2DIsoSurfaces = new List<C2DISOSurface>();
        public CColorScale colorScale = new CColorScale();
        private EDGE_POINT_INDEX []pEdgeIndex = null;
        
        float[] grid = null;

        public CMesh mesh = null;
        public MarchingCubes2D()
        {
            InitTables();
        }       

        public C3DLine[] to3DLine()
        {
            List<C3DLine> lines = new List<C3DLine>();
            C2DISOSurface sf;
           
            for (int i = 0; i < p2DIsoSurfaces.Count; i++)
            {
                sf = p2DIsoSurfaces[i];
                
                if (sf.pCoordArray.Count < 2) continue;

                C3DLine line = new C3DLine();
                line.Name = sf.isoVale.ToString();
                line.Color = colorScale.GetColor(sf.isoVale);

               List<Vector32>pps = sf.toPointArray();
               foreach(Vector32 p in pps) line.AddPoint(p);

                line.UpdateRange();
                lines.Add(line);
            }//for (int i=0;i<obj.marchingCube.pIsoSurfaces.Count;i++)
            /*
            C2DISOSurface sf;
            FLOAT_POINT p;
            int id;
            for(int i=0;i< pIsoSurfaces.Count;i++)
            {
                sf = pIsoSurfaces[i];
                C3DLine line = new C3DLine();
                line.name = sf.isoVale.ToString();
                line.color = colorScale.GetColor(sf.isoVale);
                //for( int j = 0; j < sf.pLineIndex.Count; j++ )
                for (int j = 0; j < sf.pCoordArray.Count; j++)
                {
                    id = j;// sf.pLineIndex[j];
                    p = sf.pCoordArray[id];
                    line.AddPoint(p.x, p.y, p.z);
                }
                line.UpdateRange();
                lines.Add(line);
            }
            */
            return lines.ToArray();
        }
     
        public override bool SaveBinary(BinaryWriter br)
        {
            colorScale.WriteBinary(br);
            br.Write(p2DIsoSurfaces.Count);
            foreach(C2DISOSurface iso in p2DIsoSurfaces)
            {
                if ( !iso.SaveAs(br) ) return false;
            }            
            return true;
        }
        public override bool LoadBinary(BinaryReader br)
        {
            Clear();
            colorScale.LoadBinary(br);
            int n = br.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                C2DISOSurface iso = new C2DISOSurface(0);
                if (!iso.LoadFrom(br)) return false;
                p2DIsoSurfaces.Add(iso);
            }            
            return true;
        }
        public override void Clear()
        {
            p2DIsoSurfaces.Clear();
            pEdgeIndex = null;
        }
        public override void InitTables()
        {
            //   y
            //  3 __2___2
            //   |3    1|
            //  0|__0___|1 x
            //triangle table expanded
            triTable = new int[16, 8]
            {                               //3210
                                            //8421
                {-1,-1,-1,-1,-1,-1,-1,-1},  //0000 - 0 
                { 0, 3,-1,-1,-1,-1,-1,-1},  //0001 - 1
                { 0, 1,-1,-1,-1,-1,-1,-1},  //0010 - 2
                { 3, 1,-1,-1,-1,-1,-1,-1},  //0011 - 3
                { 1, 2,-1,-1,-1,-1,-1,-1},  //0100 - 4
                { 0, 1, 3, 2,-1,-1,-1,-1},  //0101 - 5
                { 0, 2,-1,-1,-1,-1,-1,-1},  //0110 - 6
                { 3, 2,-1,-1,-1,-1,-1,-1},  //0111 - 7
                { 3, 2,-1,-1,-1,-1,-1,-1},  //1000 - 8
                { 0, 2,-1,-1,-1,-1,-1,-1},  //1001 - 9
                { 0, 3, 1, 2,-1,-1,-1,-1},  //1010 - 10
                { 1, 2,-1,-1,-1,-1,-1,-1},  //1011 - 11
                { 1, 3,-1,-1,-1,-1,-1,-1},  //1100 - 12
                { 0, 1,-1,-1,-1,-1,-1,-1},  //1101 - 13
                { 0, 3,-1,-1,-1,-1,-1,-1},  //1110 - 14
                {-1,-1,-1,-1,-1,-1,-1,-1},  //0010 - 15
            };
        }
        public double GetValue(int ix, int iy)
        {
            if (mesh == null)
            {
                int id = ix + iy * xGridNum;
                return pGridData[id];
            }
            else
            {
                return mesh.GetPoint(iy, ix).V;
            }
        }
        public bool SetData(CMesh _mesh )
        {
            Clear();

            mesh = _mesh;
            xGridNum = mesh.nCol;
            yGridNum = mesh.nRow;

            return true;
        }
        public override int GetGridType(int ix, int iy, int iz, double isoValue)
        {
            double v0 = GetValue(ix, iy);
            double v1 = GetValue(ix + 1, iy);
            double v2 = GetValue(ix + 1, iy + 1);
            double v3 = GetValue(ix, iy + 1);
            int type = 0;
            if (v0 > isoValue) type += 1;
            if (v1 > isoValue) type += 2;
            if (v2 > isoValue) type += 4;
            if (v3 > isoValue) type += 8;
            return type;
        }
        // return >0 successs, <0 error
        public int DoSearchSurface(double isoValue)
        {
            //     |z
            //     4--------5    
            //    /|       /|
            //  7--------6  |
            //  |  0-----|--1--->y
            //  | /      | /
            //  3--------2
            //  /x            
            if (xGridNum < 2 || yGridNum < 2)
            {
                m_ErrInfo = "input data is not ready.";
                return -1;
            }            
            ////////////////////////////////////////
            int iType = 0;
            int ix, iy;

            C2DISOSurface sf = new C2DISOSurface(isoValue);

            pEdgeIndex = new EDGE_POINT_INDEX[xGridNum* yGridNum];
            for(int i=0;i< xGridNum * yGridNum;i++)
            {
                pEdgeIndex[i] = new EDGE_POINT_INDEX(2);
            }

            for (iy = 0; iy < yGridNum - 1; iy++)
            {
                for (ix = 0; ix < xGridNum - 1; ix++)
                {
                    //get the index number				
                    iType = GetGridType(ix, iy, 0, isoValue);
                    if (iType != 0 && iType != 15)
                    {
                        ExtractLineFromGrid(ref sf, isoValue, ix, iy, iType);
                    }

                }//for (iy = 0; iy < yGridNum - 1; iy++)
            }//for( iz = 0; iz < zGridNum - 1; iz++ )

            pEdgeIndex = null;
            p2DIsoSurfaces.Add(sf);

            return 1;
        }
        private void ExtractLineFromGrid(ref C2DISOSurface sf, double isoValue, int ix, int iy, int iType)
        {
            int edno;
            for (int i = 0; ; i++)
            {
                edno = triTable[iType, i];
                if (edno == -1) break;
                GetEdgeCoord(ix,iy,edno,isoValue,ref sf);
            }            
        }
        private int GetStoredEdgeCoordIndex(int ix, int iy, int edno)
        {   
            int id = ix + iy * xGridNum;
            if (edno == 0) return pEdgeIndex[id].pIndex[0];
            else if (edno == 3) return pEdgeIndex[id].pIndex[1];
            else if (edno == 1)
            {
                id = ix + iy * xGridNum + 1;
                return pEdgeIndex[id].pIndex[1];
            }
            else if (edno == 2)
            {
                id = ix + (iy+1) * xGridNum;
                return pEdgeIndex[id].pIndex[0];
            }
            return -1;
        }
        private void StoreEdgeCoordIndex(int ix, int iy, int edno, int index)
        {
            int id = ix + iy * xGridNum;
            if (edno == 0)
            {
                pEdgeIndex[id].pIndex[0] = index;
            }
            else if (edno == 3)
            {
                pEdgeIndex[id].pIndex[1] = index;
            }
            else if (edno == 1)
            {
                id = ix + iy * xGridNum + 1;
                pEdgeIndex[id].pIndex[1] = index;
            }           
            else if (edno == 2)
            {
                id = ix + (iy + 1) * xGridNum;
                pEdgeIndex[id].pIndex[0] = index;
            }
        }
        public Vector32 GetVerticCoord(int ix, int iy)
        {
            return mesh.GetPoint(iy, ix);
        }        
        
        private void GetEdgeCoord(int ix,int iy,int edno,double isovalue,ref C2DISOSurface sf)
        {   
            int id = GetStoredEdgeCoordIndex(ix, iy, edno);
            if( id >=0 )
            {
                sf.AddIndex(id);
            }
            else //if ( id < 0 )
            {
                Vector32 p, p1, p2;
                if (edno==0)
                {
                    p1 = GetVerticCoord(ix, iy);
                    p2 = GetVerticCoord(ix+1, iy);
                }
                else if (edno == 1)
                {
                    p1 = GetVerticCoord(ix+1, iy);
                    p2 = GetVerticCoord(ix + 1, iy+1);
                }
                else if (edno == 2) //3-2
                {                   
                    p1 = GetVerticCoord(ix, iy+1);
                    p2 = GetVerticCoord(ix + 1, iy+1);
                }
                else //if (edno == 3) //0-3
                {
                    p1 = GetVerticCoord(ix, iy);
                    p2 = GetVerticCoord(ix, iy + 1);
                }                
                double scale = (isovalue - p1.V) / (p2.V - p1.V);
                p = p1 + (float)scale * (p2 - p1);

                int index = sf.pCoordArray.Count;
                StoreEdgeCoordIndex(ix, iy, edno, index);
                sf.AddPoint(new FLOAT_POINT(p.x,p.y,p.z));
                sf.AddIndex(index);
            }            
        }
    }
    /// <summary>
    /// 六面体6个表面轮廓提取
    /// </summary>
    public class MarchingCubes3DSurface
    {
        public int[,] triTable;	 //[256][36]
        //顶点对应表，侧面顶点和bottom底面0123对应表
        public int[] bottomVertics;
        public int[] topVertics;
        public int[] leftVertics;
        public int[] rightVertics;
        public int[] frontVertics;
        public int[] backVertics;
        //边号对应表，侧面边号和bottom底面0123对应表
        public int[] bottomEdges;
        public int[] topEdges;
        public int[] leftEdges;
        public int[] rightEdges;
        public int[] frontEdges;
        public int[] backEdges;
                
        //     |y
        //     4________5    
        //    /|       /|
        //  7/_|_____6/ |9
        //  |  0-----|--1--->x
        //11| /      | /
        //  |3-------2/
        //  /z           
        public enum CubeFaceEnum
        {
            Top = 0,
            Bottom = 1,
            Left = 2,
            Right = 3,
            Front = 4,
            Back = 5,
        }
        public MarchingCubes3DSurface()
        {
            //   z
            //   |
            //  3| ___2____2
            //   |        |
            //   |3      1|
            //  0|___0___ |1--> x
            //底部四边形对应的构型表，<4表示边号，>4表示顶点号
            triTable = new int[16, 9]
            {
                {-1,-1,-1,-1,-1,-1,-1,-1,-1},  //0000 - 0 
                { 10, 0,3,-1,-1,-1,-1,-1,-1},  //0001 - 1
                { 11, 1,0,-1,-1,-1,-1,-1,-1},  //0010 - 2
                { 10, 11,3,11,1,3,-1,-1,-1},  //0011 - 3
                { 12, 2,1,-1,-1,-1,-1,-1,-1},  //0100 - 4
                { 10, 0, 3, 12,2,1,-1,-1,-1},  //0101 - 5
                { 11, 12,0,12,2,0,-1,-1,-1},  //0110 - 6
                { 10, 11,3,11,12,2,11,2,3},  //0111 - 7
                { 13, 3,2,-1,-1,-1,-1,-1,-1},  //1000 - 8
                { 13, 10,2,10,0,2,-1,-1,-1},  //1001 - 9
                { 11, 1, 0, 13,3,2,-1,-1,-1},  //1010 - 10
                { 10, 11,1,13,10,2,10,1,2},  //1011 - 11
                { 12, 3,1,12,13,3,-1,-1,-1},  //1100 - 12
                { 13, 10,0,12,13,1,13,0,1},  //1101 - 13
                { 11, 12,0,12,13,3,12,3,0},  //1110 - 14
                { 10, 11,12,10,12,13,-1,-1,-1},  //0010 - 15
            };

            //顶点和边的对应表，侧面顶点边和bottom底面0123对应表，外侧面
            bottomVertics = new int[] { 0, 1, 2, 3 };
            bottomEdges = new int[] { 0, 1, 2, 3 };
            topVertics = new int[] {7,6,5,4}; 
            topEdges = new int[] { 6, 5, 4, 7 };
            leftVertics = new int[] { 0, 3, 7, 4 };
            leftEdges = new int[] { 3, 11,7,8 };
            rightVertics = new int[] { 2, 1, 5, 6 };
            rightEdges = new int[] { 1, 9, 5, 10 };
            frontVertics = new int[] { 3, 2, 6, 7 };
            frontEdges = new int[] { 2, 10, 6, 11 };
            backVertics = new int[] { 1, 0, 4, 5 };            
            backEdges = new int[] { 0, 8, 4, 9 };
        }
        
        /// <summary>
        /// 根据当前网格序列0-255，得到与bottom构型一致的序号0-14
        /// </summary>
        /// <param name="id">cube中的构型序号0-255</param>
        /// <param name="face">0-Top,1-bottom,2-left,3-</param>
        /// <returns></returns>
        public int GetTypeIndex(int id, CubeFaceEnum face )
        {
            int type = 0;
            int add = 0;
            for(int i = 0; i < 4; i++ )
            {
                if (face == CubeFaceEnum.Top) add = (1 << topVertics[i]);
                else if (face == CubeFaceEnum.Bottom) add = (1 << bottomVertics[i]);
                else if (face == CubeFaceEnum.Left) add = (1 << leftVertics[i]);
                else if (face == CubeFaceEnum.Right) add = (1 << rightVertics[i]);
                else if (face == CubeFaceEnum.Front) add = (1 << frontVertics[i]);
                else if (face == CubeFaceEnum.Back) add = (1 << backVertics[i]);
                if ( (add & id) > 0 )type += ( 1 << i );
            }
            return type;
        }
        /// <summary>
        /// 根据当前顶点编号0-3，转换到面的顶点编号
        /// </summary>
        /// <param name="ver">当前顶点编号0-3</param>
        /// <param name="face">0-Top,1-bottom,2-left,3-</param>
        /// <returns></returns>
        public int[] toVerticId(int ver, CubeFaceEnum face)
        {
            if (face == CubeFaceEnum.Top) return topVertics;
            else if (face == CubeFaceEnum.Bottom) return bottomVertics;
            else if (face == CubeFaceEnum.Left) return leftVertics;
            else if (face == CubeFaceEnum.Right) return rightVertics;
            else if (face == CubeFaceEnum.Front) return frontVertics;
            else if (face == CubeFaceEnum.Back) return backVertics;
            return null;
        }
        public int[] toEdgeId(int edge, CubeFaceEnum face)
        {
            if (face == CubeFaceEnum.Top) return topEdges;
            else if (face == CubeFaceEnum.Bottom) return bottomEdges;
            else if (face == CubeFaceEnum.Left) return leftEdges;
            else if (face == CubeFaceEnum.Right) return rightEdges;
            else if (face == CubeFaceEnum.Front) return frontEdges;
            else if (face == CubeFaceEnum.Back) return backEdges;
            return null;
        }
        //     |y
        //     |___4____    
        //   7/|       /|
        //   /_|_6____/ |9
        //  |  0---0-|--|--->x
        //11| /3   10| /1
        //  |/---2---|/
        //  /z 
    }

    #region Class of MarchingCubesExt
    //Extract the edge of the cubes
    public class MarchingCubesExt : MarchingCubes
    {
        public CISOSurfaceExt pISOSurfaceExt = new CISOSurfaceExt();
        public C3DGridData p3DData = null;
        public CColorScale m_ColorScale = new CColorScale();
        public List<vec2> pHideValues = new List<vec2>();
        public override bool SaveBinary(BinaryWriter br)
        {
            return pISOSurfaceExt.SaveBinary(br);            
        }
        public override bool LoadBinary(BinaryReader br)
        {
            return pISOSurfaceExt.LoadBinary(br);            
        }
        protected void InitClosedValue(CColorScale colorscale)
        {
            pHideValues.Clear();
            colorscale.CreateClosedValues();
            pHideValues.AddRange(colorscale.closedValues);
        }
        public void ClearClosedValues() { pHideValues.Clear(); }
        public void AddClosedValue(double v1,double v2)
        {
            pHideValues.Add(new vec2((float)v1, (float)v2));
        }

        private double GetEdgeValue(int ix, int iy, int iz, int edno)
        {
            double[] p = new double[10];
            int icur = ix + iy * xGridNum + iz * xyGrid;
            int no = 0;
            int icur1,icur2;
            int[] grid = new int[8];
            grid[0] = icur;
            grid[1] = icur+1;
            grid[2] = grid[1] + xyGrid;
            grid[3] = icur + xyGrid;
            grid[4] = grid[0] + xGridNum;
            grid[5] = grid[1] + xGridNum;
            grid[6] = grid[2] + xGridNum;
            grid[7] = grid[3] + xGridNum;
            switch (edno)
            {
                case 0:
                    p[no++] = p3DData[grid[0]];//0                                                           
                    p[no++] = p3DData[grid[1]];//1
                    p[no++] = (p3DData[grid[4]] + p3DData[grid[5]]) / 2.0;                    
                    p[no++] = (p3DData[grid[3]] + p3DData[grid[2]]) / 2.0;                    
                    if ( iy > 0  )//y--
                    {
                        icur1 = grid[0] - xGridNum;
                        icur2 = grid[1] - xGridNum;
                        p[no++] = (p3DData[icur1] + p3DData[icur2]) / 2.0;                        
                    }
                    if ( iz > 0 )//z--
                    {
                        icur1 = grid[0] - xyGrid;
                        icur2 = grid[1] - xyGrid;
                        p[no++] = (p3DData[icur1] + p3DData[icur2]) / 2.0;                        
                    }
                    break;
                case 1:
                    p[no++] = p3DData[grid[1]];//1
                    p[no++] = p3DData[grid[2]];//2
                    p[no++] = (p3DData[grid[5]] + p3DData[grid[6]]) / 2.0;
                    p[no++] = (p3DData[grid[0]] + p3DData[grid[3]]) / 2.0;
                    if (iy > 0)//y--
                    {
                        icur1 = grid[1] - xGridNum;
                        icur2 = grid[2] - xGridNum;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix < xGridNum - 2)
                    {
                        icur1 = grid[1] + 1;
                        icur2 = grid[2] + 1;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 2:
                    p[no++] = p3DData[grid[2]];//2
                    p[no++] = p3DData[grid[3]];//3
                    p[no++] = (p3DData[grid[0]] + p3DData[grid[1]]) / 2.0;
                    p[no++] = (p3DData[grid[6]] + p3DData[grid[7]]) / 2.0;
                    if (iy > 0)//y--
                    {
                        icur1 = grid[2] - xGridNum;
                        icur2 = grid[3] - xGridNum;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }                    
                    if (iz < zGridNum - 2)
                    {
                        icur1 = grid[2] + xyGrid;
                        icur2 = grid[3] + xyGrid;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 3:
                    p[no++] = p3DData[grid[0]];//0
                    p[no++] = p3DData[grid[3]];//3
                    p[no++] = (p3DData[grid[1]] + p3DData[grid[2]]) / 2.0;
                    p[no++] = (p3DData[grid[4]] + p3DData[grid[7]]) / 2.0;
                    if (iy > 0)//y--
                    {
                        icur1 = grid[0] - xGridNum;
                        icur2 = grid[3] - xGridNum;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix  >  0)
                    {
                        icur1 = grid[0] - 1;
                        icur2 = grid[3] - 1;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 4:
                    p[no++] = p3DData[grid[4]];//4
                    p[no++] = p3DData[grid[5]];//5
                    p[no++] = (p3DData[grid[0]] + p3DData[grid[1]]) / 2.0;
                    p[no++] = (p3DData[grid[6]] + p3DData[grid[7]]) / 2.0;
                    if (iz > 0)//z--
                    {
                        icur1 = grid[4] - xyGrid;
                        icur2 = grid[5] - xyGrid;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    if (iy < yGridNum - 2)
                    {
                        icur1 = grid[4] + xGridNum;
                        icur2 = grid[5] + xGridNum;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 5:
                    p[no++] = p3DData[grid[5]];//5
                    p[no++] = p3DData[grid[6]];//6
                    p[no++] = (p3DData[grid[1]] + p3DData[grid[2]]) / 2.0;
                    p[no++] = (p3DData[grid[4]] + p3DData[grid[7]]) / 2.0;
                    if (ix < xGridNum -2)//x++
                    {
                        icur1 = grid[5] + 1;
                        icur2 = grid[6] + 1;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    if (iy < yGridNum - 2)//y++
                    {
                        icur1 = grid[5] + xGridNum;
                        icur2 = grid[6] + xGridNum;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 6:
                    p[no++] = p3DData[grid[6]];//6
                    p[no++] = p3DData[grid[7]];//7
                    p[no++] = (p3DData[grid[2]] + p3DData[grid[3]]) / 2.0;
                    p[no++] = (p3DData[grid[4]] + p3DData[grid[5]]) / 2.0;
                    if (iy < yGridNum - 2)//y++
                    {
                        icur1 = grid[6] + xGridNum;
                        icur2 = grid[7] + xGridNum;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    if (iz < zGridNum - 2)//z++
                    {
                        icur1 = grid[6] + xyGrid;
                        icur2 = grid[7] + xyGrid;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 7:
                    p[no++] = p3DData[grid[4]];//4
                    p[no++] = p3DData[grid[7]];//7
                    p[no++] = (p3DData[grid[0]] + p3DData[grid[3]]) / 2.0;
                    p[no++] = (p3DData[grid[6]] + p3DData[grid[5]]) / 2.0;
                    if (iy < yGridNum - 2)//y++
                    {
                        icur1 = grid[4] + xGridNum;
                        icur2 = grid[7] + xGridNum;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix>0)//x--
                    {
                        icur1 = grid[4] - 1;
                        icur2 = grid[7] - 1;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 8:
                    p[no++] = p3DData[grid[4]];//4
                    p[no++] = p3DData[grid[7]];//0
                    p[no++] = (p3DData[grid[1]] + p3DData[grid[5]]) / 2.0;
                    p[no++] = (p3DData[grid[3]] + p3DData[grid[7]]) / 2.0;
                    if (iz >0)//z--
                    {
                        icur1 = grid[4] - xyGrid;
                        icur2 = grid[0] - xyGrid;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix > 0)//x--
                    {
                        icur1 = grid[4] - 1;
                        icur2 = grid[0] - 1;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 9:
                    p[no++] = p3DData[grid[1]];//1
                    p[no++] = p3DData[grid[5]];//5
                    p[no++] = (p3DData[grid[0]] + p3DData[grid[4]]) / 2.0;
                    p[no++] = (p3DData[grid[6]] + p3DData[grid[2]]) / 2.0;
                    if (iz > 0)//z--
                    {
                        icur1 = grid[1] - xyGrid;
                        icur2 = grid[5] - xyGrid;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix <xGridNum -2)//x++
                    {
                        icur1 = grid[1] + 1;
                        icur2 = grid[5] + 1;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 10:
                    p[no++] = p3DData[grid[2]];//2
                    p[no++] = p3DData[grid[6]];//6
                    p[no++] = (p3DData[grid[1]] + p3DData[grid[5]]) / 2.0;
                    p[no++] = (p3DData[grid[3]] + p3DData[grid[7]]) / 2.0;
                    if (iz <zGridNum-2)//z++
                    {
                        icur1 = grid[2] + xyGrid;
                        icur2 = grid[6] + xyGrid;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix < xGridNum - 2)//x++
                    {
                        icur1 = grid[2] + 1;
                        icur2 = grid[6] + 1;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 11:
                    p[no++] = p3DData[grid[3]];//3
                    p[no++] = p3DData[grid[7]];//7
                    p[no++] = (p3DData[grid[0]] + p3DData[grid[4]]) / 2.0;
                    p[no++] = (p3DData[grid[6]] + p3DData[grid[2]]) / 2.0;
                    if (iz < zGridNum - 2)//z++
                    {
                        icur1 = grid[3] + xyGrid;
                        icur2 = grid[7] + xyGrid;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix < xGridNum - 2)//x++
                    {
                        icur1 = grid[3] - 1;
                        icur2 = grid[7] - 1;
                        p[no] = (p3DData[icur1] + p3DData[icur2]) / 2.0;
                        no++;
                    }
                    break;
            }
            if (no > 0)
            {
                double sum = 0;
                for (int i = 0; i < no; i++)
                {
                    sum += p[i];
                }
                return (sum / no);
            }
            else return 0;
        }

        /// <summary>
        /// get the intersecion of hidevalues and edge
        /// pHideValues Cross edge vert1,vert2
        /// 计算pHideValues与边相交的值
        /// </summary>
        /// <param name="vert1"> value of vertex 1</param>
        /// <param name="vert2">value of vertex 2</param>
        /// <param name="b1">show state of vertex 1</param>
        /// <param name="b2">show state of vertex 1</param>
        /// <param name="secValue">intersection value</param>
        /// <returns>false, no intersection</returns>
        protected bool GetCrossValue(double vert1, double vert2, bool b1, bool b2, out double secValue)
        {
            secValue = double.NaN;
            double mv1 = vert1;
            double mv2 = vert2;
            if(vert1 > vert2) { mv1 = vert2;mv2 = vert1; }
            List<double> values = new List<double>();
            foreach(vec2 p in pHideValues)
            {
                if (p.x >= mv1 && p.x <= mv2) values.Add(p.x);
                if (p.y >= mv1 && p.y <= mv2) values.Add(p.y);
            }
            if (values.Count == 0) return false; //no section
            bool mini = false;//
            if (b1 && vert1 < vert2) mini = true;
            if (b2 && vert1 > vert2) mini = true;
            values.Sort();
            if (mini) secValue = values[0];
            else secValue = values[values.Count - 1];
            values.Clear();
            return true;
        }
        protected override double GetVerticValue(int icur)
        {
            return p3DData[icur];
        }
        protected override double GetVerticValue(int ix, int iy, int iz, int verno)
        {
            return p3DData[GetVerticIndex(ix, iy, iz, verno)];
        }
        protected bool GetVerticShowState(C3DGridData data,int ix, int iy, int iz, int verno)
        {
            if (data.pgridShowTable[GetVerticIndex(ix, iy, iz, verno)] == 0)
                return false;
            else return true;
        }
        protected bool GetVerticShowState(C3DGridData data,int id)
        {
            if (data.pgridShowTable[id] == 0)return false;
            else return true;
        }
        protected bool GetVerticShowState(int id)
        {
            if (p3DData.pgridShowTable[id] == 0) return false;
            else return true;
        }
        //获取顶点坐标
        //     |y
        //     4________5    
        //    /|       /|
        //  7/_|_____6/ |9
        //  |  0-----|--1--->x
        //11| /      | /
        //  |3-------2/
        //  /z                   

        protected override FLOAT_POINT_EXT GetVerticCoord(int ix, int iy, int iz, int ver)
        {
            FLOAT_POINT_EXT p = new FLOAT_POINT_EXT();
            p.x = (float)(xMin + ix * xGridStep);
            p.y = (float)(yMin + iy * yGridStep);
            p.z = (float)(zMin + iz * zGridStep);
            int icur = ix + iy * xGridNum + iz * xyGrid;
            switch (ver)
            {
                case 0:
                    break;
                case 1:
                    icur += 1;
                    p.x += (float)xGridStep;
                    break;
                case 2:
                    icur += 1;
                    icur += xyGrid;
                    p.x += (float)xGridStep;
                    p.z += (float)zGridStep;
                    break;
                case 3:
                    icur += xyGrid;
                    p.z += (float)zGridStep;
                    break;
                case 4:
                    icur += xGridNum;
                    p.y += (float)yGridStep;
                    break;
                case 5:
                    icur += 1;
                    icur += xGridNum;
                    p.x += (float)xGridStep;
                    p.y += (float)yGridStep;
                    break;
                case 6:
                    icur += 1;
                    icur += xGridNum;
                    icur += xyGrid;
                    p.x += (float)xGridStep;
                    p.y += (float)yGridStep;
                    p.z += (float)zGridStep;
                    break;
                case 7:
                    icur += xyGrid;
                    icur += xGridNum;
                    p.y += (float)yGridStep;
                    p.z += (float)zGridStep;
                    break;
            }
            p.icolor =(short) GetColorIndex(GetVerticValue(icur));
            p.alpha = (byte)(255*GetOverlapsBy(icur));

            return p;
        }
        /// <summary>
        /// 获取叠加通道的值
        /// </summary>
        /// <param name="icur"></param>
        /// <returns></returns>
        public double GetOverlapsBy(int icur)
        {
            if (p3DData.overlaps.Count > 0 && p3DData.enableOverlap)
            {
                return p3DData.overlaps[0].data[icur];
            }
            else
            {
                return p3DData.Alpha;
            }
        }
        public double GetOverlapsBy(int id1,int id2)
        {
            if (p3DData.overlaps.Count > 0 && p3DData.enableOverlap)
            {
                return p3DData.overlaps[0].data[id1] + p3DData.overlaps[0].data[id2];
            }
            else
            {
                return p3DData.Alpha;
            }
        }
        private FLOAT_POINT_EXT GetEdgeCoordTest(int ix, int iy, int iz, int edno)
        {
            FLOAT_POINT p1 = GetVerticCoord(ix, iy, iz, vertEdgeRelation[edno].x);
            FLOAT_POINT p2 = GetVerticCoord(ix, iy, iz, vertEdgeRelation[edno].y);
            FLOAT_POINT_EXT p = new FLOAT_POINT_EXT();

            p.x = (p1.x + p2.x) / 2;
            p.y = (p1.y + p2.y) / 2;
            p.z = (p1.z + p2.z) / 2;

            return p;
        }
        private bool IsBlankValue(double v)
        {
            return p3DData.IsBlankValue(v);
        }
        private bool IsBlankValue(float v)
        {
            return p3DData.IsBlankValue(v);
        }
        /*
        //get the intersection of blanked edge
        private FLOAT_POINT_EXT GetBlankEdgeIntersetion_old(int ix, int iy, int iz, int edge, bool blank1, bool blank2)
        {
            //     |y
            //     ____4____    
            //   7/|      5/|
            //   /_8_6____/ |9
            //  |  |---0-|------>x
            //11| /3   10| /1
            //  |---2----|/
            //  /z                   

            int ix1, iy1, iz1;
            int ix2, iy2, iz2;
            GetVerticIndex(ix, iy, iz, vertEdgeRelation[edge].x, out ix1, out iy1, out iz1);
            GetVerticIndex(ix, iy, iz, vertEdgeRelation[edge].y, out ix2, out iy2, out iz2);

            FLOAT_POINT p1 = GetVerticCoord(ix, iy, iz, vertEdgeRelation[edge].x);
            FLOAT_POINT p2 = GetVerticCoord(ix, iy, iz, vertEdgeRelation[edge].y);
            FLOAT_POINT_EXT p = new FLOAT_POINT_EXT(p1.x, p1.y, p1.z);
            int bn = 0;
            if (ix2 == 124 && iy2 == 209 && iz2 == 193)
            {
                int dd = 0;
            }
            if (blank1) bn = Testboundary(ix1, iy1, iz1);
            else bn = Testboundary(ix2, iy2, iz2);
            if (vertEdgeDirect[edge].z != 0)
            {
                if (DirectionCheck.Check(bn, DirectionEnum.up))
                    p.z = p3DData.GetUpperSurfaceValue(ix1, iy1);
                else if (DirectionCheck.Check(bn, DirectionEnum.down))
                    p.z = p3DData.GetLowerSurfaceValue(ix1, iy1);
            }
            // x or y direction -- interpolated it 
            else
            {
                float v1 = 0, v2 = 0, l1, l2;
                if (DirectionCheck.Check(bn, DirectionEnum.up))
                {
                    v1 = p3DData.GetUpperSurfaceValue(ix1, iy1);
                    v2 = p3DData.GetUpperSurfaceValue(ix2, iy2);
                }
                else if (DirectionCheck.Check(bn, DirectionEnum.down))
                {
                    v1 = p3DData.GetLowerSurfaceValue(ix1, iy1);
                    v2 = p3DData.GetLowerSurfaceValue(ix2, iy2);
                }

                l1 = v1 - p1.z;
                if (l1 < 0) l1 = -l1;
                l2 = v2 - p2.z;
                if (l2 < 0) l2 = -l2;

                if (l1 + l2 > 0)
                {
                    if (vertEdgeDirect[edge].x != 0)
                        p.x = (float)(p1.x + vertEdgeDirect[edge].x * l1 * xGridStep / (l2 + l1));
                    if (vertEdgeDirect[edge].y != 0)
                        p.y = (float)(p1.y + vertEdgeDirect[edge].y * l1 * yGridStep / (l2 + l1));
                }
            }

            return p;
        }
        */
        //get the intersection of blanked edge
        protected FLOAT_POINT_EXT GetBlankEdgeIntersetion(int ix, int iy, int iz, 
                            int edge, 
                            bool blank1, bool blank2,
                            double scale,
                            bool crossed )
        {
            //     |y
            //     ____4____    
            //   7/|      5/|
            //   /_8_6____/ |9
            //  |  |---0-|------>x
            //11| /3   10| /1
            //  |---2----|/
            //  /z   
            long id;
            vec3 sect;
            FLOAT_POINT_EXT p = new FLOAT_POINT_EXT(0,0,0);
            double crossvalue;
            switch (edge)
            {
                case 0://x 0-1
                    id = p3DData.GetVerticIndex(ix, iy, iz,0);
                    p = GetVerticCoord(ix, iy, iz, 0);
                    if ( p3DData.GetBlankedValue(id, out sect,0) )
                    {
                        if (!crossed) { p.x = sect.x; return p; }
                        crossvalue = p.x + scale * xGridStep;
                        p.x = sect.x;
                        if ( (blank1 && sect.x > crossvalue) || 
                             (blank2 && sect.x < crossvalue) )
                        {
                           p.x = (float)crossvalue;
                        }                        
                    }
                    break;
                case 1://z 1-2
                    id = p3DData.GetVerticIndex(ix, iy, iz, 1);
                    p = GetVerticCoord(ix, iy, iz, 1);
                    if (p3DData.GetBlankedValue(id, out sect,2))
                    {
                        if (!crossed) { p.z = sect.z; return p; }
                        crossvalue = p.z + scale * zGridStep;
                        p.z = sect.z;
                        if ((blank1 && sect.z > crossvalue) || 
                            (blank2 && sect.z < crossvalue))
                        {
                            p.z = (float)crossvalue;
                        }
                    }
                    break;
                case 2://x 3-2
                    id = p3DData.GetVerticIndex(ix, iy, iz, 3);
                    p = GetVerticCoord(ix, iy, iz, 3);
                    if (p3DData.GetBlankedValue(id, out sect,0))
                    {
                        if (!crossed) { p.x = sect.x; return p; }
                        crossvalue = p.x + scale * xGridStep;
                        p.x = sect.x;
                        if ((blank1 && sect.x > crossvalue) || 
                            (blank2 && sect.x < crossvalue))
                        {
                            p.x = (float)crossvalue;
                        }
                    }
                    break;
                case 3://z 0-3
                    id = p3DData.GetVerticIndex(ix, iy, iz, 0);
                    p = GetVerticCoord(ix, iy, iz, 0);
                    if (p3DData.GetBlankedValue(id, out sect,2))
                    {
                        if (!crossed) { p.z = sect.z; return p; }
                        crossvalue = p.z + scale * zGridStep;
                        p.z = sect.z;
                        if ((blank1 && sect.z > crossvalue) || 
                            (blank2 && sect.z < crossvalue))
                        {
                            p.z = (float)crossvalue;
                        }
                    }
                    break;
                case 4://x 4-5
                    id = p3DData.GetVerticIndex(ix, iy, iz, 4);
                    p = GetVerticCoord(ix, iy, iz, 4);
                    if (p3DData.GetBlankedValue(id, out sect,0))
                    {
                        if (!crossed) { p.x = sect.x; return p; }
                        crossvalue = p.x + scale * xGridStep;
                        p.x = sect.x;
                        if ((blank1 && sect.x > crossvalue) || 
                            (blank2 && sect.x < crossvalue))
                        {
                            p.x = (float)crossvalue;
                        }
                    }
                    break;
                case 5://z 5-6
                    id = p3DData.GetVerticIndex(ix, iy, iz, 5);
                    p = GetVerticCoord(ix, iy, iz, 5);
                    if (p3DData.GetBlankedValue(id, out sect,2))
                    {
                        if (!crossed) { p.z = sect.z; return p; }
                        crossvalue = p.z + scale * zGridStep;
                        p.z = sect.z;
                        if ((blank1 && sect.z > crossvalue) || 
                            (blank2 && sect.z < crossvalue))
                        {
                            p.z = (float)crossvalue;
                        }
                    }
                    break;
                case 6://x 7-6
                    id = p3DData.GetVerticIndex(ix, iy, iz, 7);
                    p = GetVerticCoord(ix, iy, iz, 7);
                    if (p3DData.GetBlankedValue(id, out sect,0))
                    {
                        if (!crossed) { p.x = sect.x; return p; }
                        crossvalue = p.x + scale * xGridStep;
                        p.x = sect.x;
                        if ((blank1 && sect.x > crossvalue) || 
                            (blank2 && sect.x < crossvalue))
                        {
                            p.x = (float)crossvalue;
                        }
                    }
                    break;
                case 7://z 4-7
                    id = p3DData.GetVerticIndex(ix, iy, iz, 4);
                    p = GetVerticCoord(ix, iy, iz, 4);
                    if (p3DData.GetBlankedValue(id, out sect,2))
                    {
                        if (!crossed) { p.z = sect.z; return p; }
                        crossvalue = p.z + scale * zGridStep;
                        p.z = sect.z;
                        if ((blank1 && sect.z > crossvalue) || 
                            (blank2 && sect.z < crossvalue))
                        {
                            p.z = (float)crossvalue;
                        }
                    }
                    break;
                case 8://y 0-4
                    id = p3DData.GetVerticIndex(ix, iy, iz, 0);
                    p = GetVerticCoord(ix, iy, iz, 0);
                    if (p3DData.GetBlankedValue(id, out sect,1))
                    {
                        if (!crossed) { p.y = sect.y; return p; }
                        crossvalue = p.y + scale * yGridStep;
                        p.y = sect.y;
                        if ((blank1 && sect.y > crossvalue) || 
                            (blank2 && sect.y < crossvalue))
                        {
                            p.y = (float)crossvalue;
                        }
                    }
                    break;
                case 9://y 1-5
                    id = p3DData.GetVerticIndex(ix, iy, iz, 1);
                    p = GetVerticCoord(ix, iy, iz, 1);
                    if (p3DData.GetBlankedValue(id, out sect, 1))
                    {
                        if (!crossed) { p.y = sect.y; return p; }
                        crossvalue = p.y + scale * yGridStep;
                        p.y = sect.y;
                        if ((blank1 && sect.y > crossvalue) || 
                            (blank2 && sect.y < crossvalue))
                        {
                            p.y = (float)crossvalue;
                        }
                    }
                    break;
                case 10://y 2-6
                    id = p3DData.GetVerticIndex(ix, iy, iz, 2);
                    p = GetVerticCoord(ix, iy, iz, 2);
                    if (p3DData.GetBlankedValue(id, out sect, 1))
                    {
                        if (!crossed) { p.y = sect.y; return p; }
                        crossvalue = p.y + scale * yGridStep;
                        p.y = sect.y;
                        if ((blank1 && sect.y > crossvalue) || 
                            (blank2 && sect.y < crossvalue))
                        {
                            p.y = (float)crossvalue;
                        }
                    }
                    break;
                case 11://y 3-7
                    id = p3DData.GetVerticIndex(ix, iy, iz, 3);
                    p = GetVerticCoord(ix, iy, iz, 3);
                    if (p3DData.GetBlankedValue(id, out sect, 1))
                    {
                        if (!crossed) { p.y = sect.y; return p; }
                        crossvalue = p.y + scale * yGridStep;
                        p.y = sect.y;
                        if ((blank1 && sect.y > crossvalue) || 
                            (blank2 && sect.y < crossvalue))
                        {
                            p.y = (float)crossvalue;
                        }
                    }
                    break;
                default:
                    throw new Exception("error edge no:" + edge);
#pragma warning disable CS0162 // 检测到无法访问的代码
                    break;
#pragma warning restore CS0162 // 检测到无法访问的代码
            }           
            
            return p;
        }       
        /*
        private int Testboundary(int ix, int iy, int iz)
        {
            double v1;
            int ret = 0;
            //test upper
            ret = 1;
            for (int k = iz; k < zGridNum; k++)
            {
                v1 = GetVerticValue(ix, iy, k, 0);
                if (!IsBlankValue(v1)) //failed
                {
                    ret = 0;
                    break;
                }
            }
            if (ret == 1) return 1;

            //test lower
            ret = 2;
            for (int k = iz; k >= 0; k--)
            {
                v1 = GetVerticValue(ix, iy, k, 0);
                if (!IsBlankValue(v1)) //failed
                {
                    ret = 0;
                    break;
                }
            }
            return ret;
        }*/

        ///*
        // to test if the grid is a boundary,
        // P(ix,iy,iz) is a blanked value
        private int Testboundary(int ix, int iy, int iz)
        {
            //     |z
            //     4----f---5    
            //    /|       /|
            //  7--------6  |
            //  |  0---a-|--1--->y
            //  |d/      | /b
            //  3--------2
            //  /x  c

            double v1;
            DirectionEnum ret = 0;
            //up
            if ( iz > 0 )
            {
                v1 = GetVerticValue(ix, iy, iz - 1, 0);
                if ( !IsBlankValue(v1) ) ret = ret | DirectionEnum.up;
            }
            //down
            if (iz < zGridNum - 1)
            {
                v1 = GetVerticValue(ix, iy, iz + 1, 0);
                if ( !IsBlankValue(v1) ) ret = ret | DirectionEnum.down;
            }
            //left
            if (iy < yGridNum - 1)
            {
                v1 = GetVerticValue(ix, iy + 1, iz, 0);
                if (!IsBlankValue(v1)) ret = ret | DirectionEnum.left;
            }
            //right
            if (iy > 0)
            {
                v1 = GetVerticValue(ix, iy-1, iz, 0);
                if (!IsBlankValue(v1)) ret = ret | DirectionEnum.right;
            }
            //front
            if (ix > 0)
            {
                v1 = GetVerticValue(ix - 1, iy, iz, 0);
                if (!IsBlankValue(v1)) ret = ret | DirectionEnum.front;
            }
            //back
            if ( ix < xGridNum - 1 )
            {
                v1 = GetVerticValue(ix+1, iy, iz, 0);
                if ( !IsBlankValue(v1) ) ret = ret | DirectionEnum.back;
            }
            return (int)ret;
        }
       // */
        protected virtual FLOAT_POINT_EXT GetEdgeIntersection(int ix, int iy, int iz, int edno,out bool crossed)
        {
            crossed = false;
            FLOAT_POINT_EXT p = new FLOAT_POINT_EXT();
            //2 vertices value v1,v2 of this edge
            //边的顶点编号
            int id1 = GetVerticIndex(ix, iy, iz, vertEdgeRelation[edno].x);
            int id2 = GetVerticIndex(ix, iy, iz, vertEdgeRelation[edno].y);
            double v1 = p3DData.pGridData[id1];
            double v2 = p3DData.pGridData[id2];

            //bool blank1 = IsBlankValue(v1);
            //bool blank2 = IsBlankValue(v2);
            bool blank1 = p3DData.IsBlankedGrid(id1);
            bool blank2 = p3DData.IsBlankedGrid(id2);

            //2 vertices show states
            bool b1 = GetVerticShowState(id1);
            bool b2 = GetVerticShowState(id2);

            #if DEBUG
            //error may occurred
            if ( (blank1 && blank2) && (b1 && b2) && (!b1 && !b2) )
            {
                m_ErrInfo = "error occured while searching edges.";
                m_ErrInfo += "ix:" + ix.ToString() + " ";
                m_ErrInfo += "iy:" + iy.ToString() + " ";
                m_ErrInfo += "iz:" + iz.ToString() + " ";
                m_ErrInfo += "edge:" + edno.ToString() + "\n";
                m_ErrInfo += "value1:" + v1.ToString() + " ";
                m_ErrInfo += "value2:" + v2.ToString() + " ";
                m_ErrInfo += "show state1:" + b1.ToString() + " ";
                m_ErrInfo += "show state2:" + b2.ToString() + " ";
                throw new Exception(m_ErrInfo);
            }
            #endif            
            
            double scale = 0, crossvalue;
            if (v1 != v2)
            {
                crossed = GetCrossValue(v1, v2, b1, b2, out crossvalue);
                scale = (crossvalue - v1) / (v2 - v1);
            }
            
            // one of vertices are blank value
            if (!(blank1 & blank2) && (blank1 | blank2))
            {
                //p = GetBlankEdgeIntersetion(ix, iy, iz, edno, blank1, blank2);                
                p = GetBlankEdgeIntersetion(ix, iy, iz, edno, blank1, blank2, (float)scale, crossed);
                if (blank1) p.icolor = (short)GetColorIndex(v2);
                else p.icolor = (short)GetColorIndex(v1);
            }
            else if(crossed)
            {
                //double scale = GetNearestValue(v1, v2, b1, b2);                
                //GetCrossValue(v1, v2, b1, b2, out crossvalue);                
                p = GetVerticCoord(ix, iy, iz, vertEdgeRelation[edno].x);               
                p.x += (float)(xGridStep * scale * vertEdgeDirect[edno].x);
                p.y += (float)(yGridStep * scale * vertEdgeDirect[edno].y);
                p.z += (float)(zGridStep * scale * vertEdgeDirect[edno].z);
                if (b1) p.icolor = (short)GetColorIndex(v1);
                else p.icolor = (short)GetColorIndex(v2);
            }
            p.alpha = (byte)(255 * GetOverlapsBy(id1,id2));           

            return p;
        }
        /*
        private FLOAT_POINT_EXT GetEdgeCoord(int ix, int iy, int iz, int edno )
        {
            double v1 = GetVerticValue(ix, iy, iz, vertEdgeRelation[edno].x);
            double v2 = GetVerticValue(ix, iy, iz, vertEdgeRelation[edno].y);
            bool b1 = GetVerticShowState(ix, iy, iz, vertEdgeRelation[edno].x);
            bool b2 = GetVerticShowState(ix, iy, iz, vertEdgeRelation[edno].y);
            double scale = GetNearestValue(v1, v2, b1, b2);
            FLOAT_POINT_EXT p = GetVerticCoord(ix, iy, iz, vertEdgeRelation[edno].x);
            p.x += (float)(xGridStep*scale * vertEdgeDirect[edno].x);
            p.y += (float)(yGridStep * scale * vertEdgeDirect[edno].y);
            p.z += (float)(zGridStep * scale * vertEdgeDirect[edno].z);
            if( b1)p.icolor = (short)GetColorIndex(v1);
            else p.icolor = (short)GetColorIndex(v2);
            //p.icolor = (short)GetColorIndex(v1 + scale *(v2 - v1));
            //p.icolor = (short)GetColorIndex(p3DData.pGridData[icur]);
            return p;
        } 
        */
        private void StoreVerticIndex(int ix, int iy, int iz, int ver, int id)
        {
            //current cube index
            int icur = ix + iy * xGridNum;
            // store to current cube first
            pCurEdgePointArray[icur].pIndex[12+ver] = id;
            return;
        }
        protected void StoreEdgeIndex(int ix, int iy, int iz, int edge,int id)
        {
            //current cube index
            int icur = ix + iy * xGridNum;
            // store to current cube first
            pCurEdgePointArray[icur].pIndex[edge] = id;
        }
        private int GetStoredVerticIndex(int ix, int iy, int iz, int ver)
        {
            //     |y
            //     4--------5    
            //    /|       /|
            //  7--------6  |
            //  |  0-----|--1--->x
            //  | /      | /
            //  3--------2
            //  /z       
            int icur = ix + iy * xGridNum;
            int iprev;

            // check the current layer first
            int verIndex = pCurEdgePointArray[icur].pIndex[12 + ver];
            if (verIndex >= 0) return verIndex;

            // the check prevLayer
            if (  iz > 0 )
            {   
                //0145 - 3276
                iprev = icur;
                if( ver == 0 )
                {
                    if ( pPrevEdgePointArray[iprev].pIndex[12 + 3] >= 0 )
                        return pPrevEdgePointArray[iprev].pIndex[12 + 3];
                }
                if (ver == 1)
                {
                    if (pPrevEdgePointArray[iprev].pIndex[12 + 2] >= 0)
                        return pPrevEdgePointArray[iprev].pIndex[12 + 2];
                }
                if (ver == 4)
                {
                    if (pPrevEdgePointArray[iprev].pIndex[12 + 7] >= 0)
                        return pPrevEdgePointArray[iprev].pIndex[12 + 7];
                }
                if (ver == 5)
                {
                    if (pPrevEdgePointArray[iprev].pIndex[12 + 6] >= 0)
                        return pPrevEdgePointArray[iprev].pIndex[12 + 6];
                }
            }
            if (iy > 0)
            {
                //0123 - 4567
                iprev = icur - xGridNum;
                if (ver == 0)
                {
                    if (pCurEdgePointArray[iprev].pIndex[12 + 4] >= 0)
                        return pCurEdgePointArray[iprev].pIndex[12 + 4];
                }
                if (ver == 1)
                {
                    if (pCurEdgePointArray[iprev].pIndex[12 + 5] >= 0)
                        return pCurEdgePointArray[iprev].pIndex[12 + 5];
                }
                if (ver == 2)
                {
                    if (pCurEdgePointArray[iprev].pIndex[12 + 6] >= 0)
                        return pCurEdgePointArray[iprev].pIndex[12 + 6];
                }
                if (ver == 3)
                {
                    if (pCurEdgePointArray[iprev].pIndex[12 + 7] >= 0)
                        return pCurEdgePointArray[iprev].pIndex[12 + 7];
                }
            }
            if (ix > 0)
            {
                //0374 - 1265
                iprev = icur - 1;
                if (ver == 0)
                {
                    if (pCurEdgePointArray[iprev].pIndex[12 + 1] >= 0)
                        return pCurEdgePointArray[iprev].pIndex[12 + 1];
                }
                if (ver == 3)
                {
                    if (pCurEdgePointArray[iprev].pIndex[12 + 2] >= 0)
                        return pCurEdgePointArray[iprev].pIndex[12 + 2];
                }
                if (ver == 7)
                {
                    if (pCurEdgePointArray[iprev].pIndex[12 + 6] >= 0)
                        return pCurEdgePointArray[iprev].pIndex[12 + 6];
                }
                if (ver == 4)
                {
                    if (pCurEdgePointArray[iprev].pIndex[12 + 5] >= 0)
                        return pCurEdgePointArray[iprev].pIndex[12 + 5];
                }
            }
            return -1;
        }
        protected int GetStoredEdgeIndex(int ix,int iy,int iz,int edge)
        {
            //     |z
            //     o---4----o    
            //   7/|8      /|
            //  o----6---o  |9
            //11|  o---0-|--o--->y
            //  |3/      | /1
            //  o----2---o
            //  /x  c     

            int icur = ix + iy * xGridNum;
            int iprev;

            // check the current layer first
            int edgeIndex = pCurEdgePointArray[icur].pIndex[edge];
            if (edgeIndex >= 0) return edgeIndex;//已存储
            
            //else check the previous Layer
            byte[] xedge= new byte[4] { 3,7,8,11};
            byte[] xedge1 = new byte[4] { 1, 5, 9, 10 };
            byte[] yedge = new byte[4] { 0, 1, 2, 3 };
            byte[] yedge1 = new byte[4] { 4, 5, 6, 7 };
            byte[] zedge = new byte[4] { 0, 4, 8, 9 };
            byte[] zedge1 = new byte[4] { 2, 6, 11, 10 };
            
            if (ix > 0)
            {
                //37811-15910
                iprev = (ix - 1) + iy * xGridNum;
                for (int i = 0; i < 4; i++)
                {
                    if (edge == xedge[i])
                    {
                        if (pCurEdgePointArray[iprev].pIndex[xedge1[i]] >= 0)
                        return pCurEdgePointArray[iprev].pIndex[xedge1[i]];
                    }
                }
            }
            if (iy > 0)
            {
                //0123-4567
                iprev = ix + (iy-1) * xGridNum;
                for (int i = 0; i < 4; i++)
                {
                    if (edge == yedge[i])
                    {
                        if (pCurEdgePointArray[iprev].pIndex[yedge1[i]] >= 0)
                            return pCurEdgePointArray[iprev].pIndex[yedge1[i]];
                    }
                }
            }
            if (iz > 0)//从上层网格pPrevEdgePointArray中查找
            {
                //0489-261110
                iprev = ix + iy * xGridNum;
                for (int i = 0; i < 4; i++)
                {
                    if (edge == zedge[i])
                    {
                        if (pPrevEdgePointArray[iprev].pIndex[zedge1[i]] >= 0)
                            return pPrevEdgePointArray[iprev].pIndex[zedge1[i]];
                    }
                }
            }          
            
            return -1;
        }
        /// <summary>
        /// 获得坐标数组长度
        /// </summary>
        /// <returns>坐标数组长度</returns>
        protected int GetCoordSize()
        {
            return pISOSurfaceExt.pCoordArray.Count;
        }        
        protected void AddCoordArray(FLOAT_POINT_EXT p)
        {
            pISOSurfaceExt.AddCoord(p);
        }
        private void AddTiangleIndex(int index)
        {
            pISOSurfaceExt.AddTriangleIndex(index);
        }
        public override void InitTables()
        {
            // base.InitTables();
            InitEdgeVerticTable();
            //marching cubes table data
            edgeTable = new int[]
               {
                    0x0  , 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
                    0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
                    0x190, 0x99 , 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
                    0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
                    0x230, 0x339, 0x33 , 0x13a, 0x636, 0x73f, 0x435, 0x53c,
                    0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
                    0x3a0, 0x2a9, 0x1a3, 0xaa , 0x7a6, 0x6af, 0x5a5, 0x4ac,
                    0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
                    0x460, 0x569, 0x663, 0x76a, 0x66 , 0x16f, 0x265, 0x36c,
                    0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
                    0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff , 0x3f5, 0x2fc,
                    0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
                    0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55 , 0x15c,
                    0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
                    0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc ,
                    0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
                    0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
                    0xcc , 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
                    0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
                    0x15c, 0x55 , 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
                    0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
                    0x2fc, 0x3f5, 0xff , 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
                    0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
                    0x36c, 0x265, 0x16f, 0x66 , 0x76a, 0x663, 0x569, 0x460,
                    0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
                    0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa , 0x1a3, 0x2a9, 0x3a0,
                    0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
                    0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33 , 0x339, 0x230,
                    0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
                    0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99 , 0x190,
                    0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
                    0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0
               };

            //triangle table expanded
            //Created by CreateMarchingCubeTable Project
            triTable = new int[256, 24]
            {
                {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,8,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,8,3,1,9,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,10,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,3,2,2,10,8,10,1,0,0,8,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,9,0,2,10,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,8,3,2,10,8,10,9,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,3,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,11,2,0,8,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,2,1,1,9,11,9,0,3,3,11,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,11,2,1,9,11,9,8,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,3,11,10,1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,10,1,0,8,10,8,11,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,9,0,3,11,9,11,10,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,10,9,9,8,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,7,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,3,0,4,7,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,8,0,0,1,7,1,9,4,4,7,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,1,9,4,7,1,7,3,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,7,8,1,2,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,4,1,10,1,4,10,4,7,2,10,7,3,2,7,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,2,8,7,8,2,7,2,10,4,7,10,9,4,10,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,3,2,10,7,2,10,4,7,9,4,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,3,8,8,4,2,4,7,11,11,2,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,4,7,11,2,4,2,0,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,11,2,1,7,11,9,4,7,1,9,7,0,3,8,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,7,11,9,4,11,9,11,2,9,2,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,10,7,4,7,10,4,10,1,8,4,1,3,8,1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,10,7,7,10,4,4,10,1,1,0,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,3,8,11,10,7,7,10,4,10,9,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,10,9,4,7,10,7,11,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,9,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,0,9,9,5,3,5,4,8,8,3,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,5,4,0,1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,5,4,8,3,5,3,1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,9,1,1,2,4,2,10,5,5,4,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,5,4,8,10,5,3,2,10,8,3,10,0,9,1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,2,10,5,4,2,4,0,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,10,5,3,2,5,3,5,4,3,4,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,3,11,5,4,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,11,4,5,4,11,5,11,2,9,5,2,0,9,2,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,5,2,11,2,5,11,5,4,3,11,4,0,3,4,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,5,2,2,5,11,11,5,4,4,8,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,3,9,4,9,3,4,3,11,5,4,11,10,5,11,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,0,9,8,11,4,4,11,5,11,10,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,10,5,4,11,5,4,3,11,0,3,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,11,10,5,4,11,4,8,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,7,8,9,5,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,3,0,9,5,3,5,7,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,7,8,0,1,7,1,5,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,1,5,5,7,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,7,10,2,10,7,2,7,8,1,2,8,9,1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,9,1,5,7,10,10,7,2,7,3,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,7,10,10,7,2,2,7,8,8,0,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,7,3,2,10,7,10,5,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,9,3,2,3,9,2,9,5,11,2,5,7,11,5,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,7,11,2,5,11,2,9,5,0,9,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,0,3,1,5,2,2,5,11,5,7,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,5,7,11,2,5,2,1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,7,11,11,10,5,8,9,1,1,3,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,1,0,10,5,7,7,11,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,8,0,7,11,10,10,5,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,11,5,11,10,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,5,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,8,3,10,6,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,1,10,10,6,0,6,5,9,9,0,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,8,5,6,5,8,6,8,3,10,6,3,1,10,3,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,1,2,6,5,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,6,3,8,3,6,8,6,5,0,8,5,1,0,5,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,6,5,9,0,6,0,2,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,8,5,5,8,6,6,8,3,3,2,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,11,6,6,5,3,5,10,2,2,3,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,0,10,5,10,0,5,0,8,6,5,8,11,6,8,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,9,0,3,5,9,11,6,5,3,11,5,2,1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,1,10,9,8,5,5,8,6,8,11,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,3,11,6,5,3,5,1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,11,6,5,8,6,5,0,8,1,0,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,5,9,11,6,9,11,9,0,11,0,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,8,11,6,5,8,5,9,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,4,5,5,10,8,10,6,7,7,8,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,3,6,10,6,3,10,3,0,5,10,0,4,5,0,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,10,6,7,1,10,8,0,1,7,8,1,4,5,9,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,4,5,7,3,6,6,3,10,3,1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,1,4,8,4,1,8,1,2,7,8,2,6,7,2,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,4,5,5,1,0,7,3,2,2,6,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,9,4,0,2,8,8,2,7,2,6,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,5,9,6,7,3,3,2,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,8,4,5,3,8,10,2,3,5,10,3,6,7,11,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,11,6,2,0,10,10,0,5,0,4,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,0,3,2,1,10,4,5,9,6,7,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,7,11,10,2,1,4,5,9,6,10,5,1,9,4,4,2,1,4,7,2,7,11,2},
                {11,6,7,5,1,4,4,1,8,1,3,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,7,11,4,5,1,1,0,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,5,9,8,0,3,6,7,11,4,8,7,3,11,6,6,0,3,6,5,0,5,9,0},
                {6,7,11,4,5,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,10,6,4,9,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,10,0,3,0,10,3,10,6,8,3,6,4,8,6,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,0,1,10,6,0,6,4,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,1,10,6,3,10,6,8,3,4,8,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,4,9,1,2,4,2,6,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,1,0,2,6,3,3,6,8,6,4,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,6,4,4,0,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,6,4,8,3,6,3,2,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,4,11,3,11,4,3,4,9,2,3,9,10,2,9,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,0,9,9,10,2,8,11,6,6,4,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,10,2,6,4,11,11,4,3,4,0,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,2,1,11,6,4,4,8,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,3,9,9,3,4,4,3,11,11,6,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,9,1,4,8,11,11,6,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,4,0,3,11,4,11,6,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,8,6,8,11,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,10,6,7,8,10,8,9,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,10,0,0,10,3,3,10,6,6,7,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,1,10,8,0,10,8,10,6,8,6,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,3,1,10,6,3,6,7,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,9,1,2,8,1,2,7,8,6,7,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,0,9,3,2,6,6,7,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,2,6,7,8,2,8,0,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,7,2,7,3,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,7,11,8,9,3,3,9,2,9,10,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,6,7,10,2,0,0,9,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,1,10,11,6,7,0,3,8,2,11,3,7,8,0,0,6,7,0,1,6,1,10,6},
                {11,6,7,10,2,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,11,6,3,8,9,9,1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,11,6,1,0,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,8,0,7,11,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,11,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,11,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,8,7,7,6,0,6,11,3,3,0,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,11,7,9,0,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,1,11,6,11,1,6,1,9,7,6,9,8,7,9,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,6,10,10,1,7,1,2,11,11,7,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,7,6,10,8,7,1,0,8,10,1,8,2,11,3,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,9,6,7,6,9,7,9,0,11,7,0,2,11,0,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,2,11,10,9,6,6,9,7,9,8,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,7,6,2,3,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,0,8,7,6,0,6,2,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,7,0,9,0,7,9,7,6,1,9,6,2,1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,8,7,6,9,7,6,1,9,2,1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,7,6,10,1,7,1,3,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,8,7,1,0,7,1,7,6,1,6,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,9,6,6,9,7,7,9,0,0,3,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,9,8,7,6,9,6,10,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,8,4,6,11,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,6,11,3,0,6,0,4,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,6,9,1,9,6,1,6,11,0,1,11,8,0,11,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,1,11,11,1,6,6,1,9,9,4,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,8,2,1,2,8,1,8,4,10,1,4,6,10,4,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,3,2,0,4,1,1,4,10,4,6,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,6,10,10,9,4,11,8,0,0,2,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,11,3,6,10,9,9,4,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,2,3,8,4,2,4,6,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,4,6,6,2,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,8,0,4,6,9,9,6,1,6,2,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,6,2,1,9,6,9,4,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,3,8,4,1,8,4,10,1,6,10,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,4,6,10,1,4,1,0,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,0,3,9,4,6,6,10,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,10,4,10,9,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,7,4,4,9,11,9,5,6,6,11,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,3,0,9,11,3,5,6,11,9,5,11,4,8,7,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,0,7,11,7,0,11,0,1,6,11,1,5,6,1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,8,7,3,1,11,11,1,6,1,5,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,1,2,11,9,1,7,4,9,11,7,9,6,10,5,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,4,8,0,9,1,6,10,5,2,11,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,5,6,4,0,7,7,0,11,0,2,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,10,5,7,4,8,2,11,3,6,7,11,8,3,2,2,4,8,2,10,4,10,5,4},
                {6,2,5,9,5,2,9,2,3,4,9,3,7,4,3,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,7,4,6,2,5,5,2,9,2,0,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,2,1,1,5,6,3,7,4,4,0,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,4,8,5,6,2,2,1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,10,5,1,3,9,9,3,4,3,7,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,8,7,5,6,10,0,9,1,4,5,9,10,1,0,0,6,10,0,8,6,8,7,6},
                {5,6,10,7,4,0,0,3,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,4,8,5,6,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,9,5,6,11,9,11,8,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,11,3,5,6,3,5,3,0,5,0,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,8,0,1,11,0,1,6,11,5,6,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {6,1,5,6,11,1,11,3,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,6,10,11,8,2,2,8,1,8,9,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,11,3,1,0,9,6,10,5,2,1,10,9,5,6,6,0,9,6,11,0,11,3,0},
                {6,10,5,2,11,8,8,0,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,11,3,6,10,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,9,3,3,9,2,2,9,5,5,6,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,2,0,9,5,2,5,6,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,3,8,2,1,5,5,6,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,1,6,1,5,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,5,6,9,1,3,3,8,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,1,0,10,5,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,8,0,5,6,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,5,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,11,7,5,10,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,5,8,0,8,5,0,5,10,3,0,10,11,3,10,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,11,1,0,1,11,0,11,7,9,0,7,5,9,7,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,11,3,3,1,10,7,5,9,9,8,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,1,2,11,7,1,7,5,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,11,3,7,5,8,8,5,0,5,1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,5,9,0,7,9,0,11,7,2,11,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,3,2,8,7,5,5,9,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,5,10,2,3,5,3,7,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,5,8,8,5,0,0,5,10,10,2,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,2,1,3,7,0,0,7,9,7,5,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,10,2,5,9,8,8,7,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,5,1,1,3,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,5,1,0,8,5,8,7,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,7,5,9,0,7,0,3,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,7,9,7,5,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,8,4,5,10,8,10,11,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,11,3,0,10,3,0,5,10,4,5,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,5,9,10,11,1,1,11,0,11,8,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,9,4,1,10,11,11,3,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,1,4,4,1,8,8,1,2,2,11,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,2,11,1,0,4,4,5,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,4,5,8,0,2,2,11,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,3,2,9,4,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,3,8,10,2,8,10,8,4,10,4,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,0,4,5,10,0,10,2,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,3,8,9,4,5,2,1,10,0,9,1,5,10,2,2,4,5,2,3,4,3,8,4},
                {5,9,4,1,10,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,1,3,8,4,1,4,5,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,5,0,5,1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,0,3,9,4,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {5,9,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,11,7,4,9,11,9,10,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {7,4,8,9,10,0,0,10,3,10,11,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {10,11,1,1,11,0,0,11,7,7,4,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,7,4,11,3,1,1,10,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,9,1,7,4,1,7,1,2,7,2,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,9,1,3,2,11,4,8,7,0,3,8,11,7,4,4,2,11,4,9,2,9,1,2},
                {11,0,2,11,7,0,7,4,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,3,2,8,7,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,7,4,9,3,4,9,2,3,10,2,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,8,7,0,9,10,10,2,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,1,10,0,3,7,7,4,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,7,4,10,2,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,3,7,4,9,3,9,1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {4,8,7,0,9,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,3,4,3,7,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,7,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {8,9,10,10,11,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,10,11,3,0,10,0,9,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,11,8,0,1,11,1,10,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,3,10,3,1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,8,9,1,2,8,2,11,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,2,11,1,0,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,11,0,11,8,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {11,3,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {2,9,10,2,3,9,3,8,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,9,2,9,10,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {0,3,8,2,1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {1,10,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,8,1,8,9,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {9,1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {3,8,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
                {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
            };
            //end of triTable[256,24]           

        } //end of InitTable()

        public int GetColorIndex(double v)
        {
            return m_ColorScale.GetColorIndex(v);
        }
        public virtual void SetData(C3DGridData pdata)
        {
            p3DData = pdata;
            xGridNum = pdata.xNum;
            yGridNum = pdata.yNum;
            zGridNum = pdata.zNum;
            xyGrid = xGridNum * yGridNum;
            xMin = pdata.minx;
            yMin = pdata.miny;
            zMin = pdata.minz;
            xMax = pdata.maxx;
            yMax = pdata.maxy;
            zMax = pdata.maxz;
            vMin = pdata.minv;
            vMax = pdata.maxv;
            xGridStep = (xMax - xMin) / (xGridNum - 1);
            yGridStep = (yMax - yMin) / (yGridNum - 1);
            zGridStep = (zMax - zMin) / (zGridNum - 1);
            xDividedStep = xGridStep / 2;
            yDividedStep = yGridStep / 2;
            zDividedStep = zGridStep / 2;            
            m_ColorScale = pdata.ColorScale;
            pISOSurfaceExt.pColor.Clear();
            for (int i=0;i<m_ColorScale.Count; i++)
            {
                pISOSurfaceExt.AddColor(m_ColorScale.GetColor(i));
            }
            InitClosedValue(m_ColorScale);
        }
        public MarchingCubesExt()
        {
            InitTables();
        }
        public override void Clear()
        {
            searchedGridNo = 0;
            pISOSurfaceExt.Clear();
            ReleaseEdgePointArray();
            pGridData = null;
            p3DData = null;
        }
        public void ExtractTriangleFromCubeTest(int ix, int iy, int iz, int typeIndex)
        {
            pISOSurfaceExt.Clear();
            InitEdgePointArray();

            ExtractTriangleFromGridTest(ix, iy, iz, typeIndex);

            CreateFromSurface(ix, iy, iz, typeIndex);
            ReleaseEdgePointArray();
        }
        private int ExtractTriangleFromGridTest(int ix, int iy, int iz, int typeIndex)
        {
            //     |z
            //     4----f---5    
            //    /|       /|
            //  7--------6  |
            //  |  0---a-|--1--->y
            //  |d/      | /b
            //  3--------2
            //  /x  c
            int edno, edIndex;
            // searching the edge table,till -1 end
            for (int i = 0; i < 24; i++)
            {
                edno = triTable[typeIndex, i];
                if (edno < 0) break;

                // check if the same edge coords been stored                
                edIndex = GetStoredEdgeIndex(ix, iy, iz, edno);
                if (edIndex >= 0)
                {
                    pISOSurfaceExt.pTriangleIndex.Add(edIndex);
                    StoreEdgeIndex(ix, iy, iz, edno, edIndex);
                }
                else
                {   //else if no stored, create new
                    FLOAT_POINT_EXT p = GetEdgeCoordTest(ix, iy, iz, edno);
                    pISOSurfaceExt.pTriangleIndex.Add(GetCoordSize());
                    StoreEdgeIndex(ix, iy, iz, edno, GetCoordSize());
                    AddCoordArray(p);
                }
            }//for
            return 1;
        }
        public virtual int ExtractTriangleFromGrid(int ix, int iy, int iz, int typeIndex)
        {
            //     |z
            //     4----f---5    
            //    /|       /|
            //  7--------6  |
            //  |  0---a-|--1--->y
            //  |d/      | /b
            //  3--------2
            //  /x  c
            FLOAT_POINT_EXT p;
            int edno, edIndex;            
            // searching the edge table,till -1 end
            for (int i = 0; i < 24; i++)
            {
                edno = triTable[typeIndex, i];
                if (edno < 0) break;
                
                // check if the same edge coords been stored                
                edIndex = GetStoredEdgeIndex(ix, iy, iz, edno);
                if (edIndex >= 0)
                {   
                    pISOSurfaceExt.pTriangleIndex.Add(edIndex);
                    StoreEdgeIndex(ix, iy, iz, edno, edIndex);                    
                }   
                else
                {   //else if no stored, create new
                    p = GetEdgeIntersection(ix, iy, iz, edno,out bool crossed);
                    pISOSurfaceExt.pTriangleIndex.Add(GetCoordSize());
                    StoreEdgeIndex(ix, iy, iz, edno, GetCoordSize());
                    AddCoordArray(p);
                }                
            }//for
            return 1;
        }
        //bVertics[] 4 vertics show state,vertics[],4 Edges[](0-11)
        public virtual int ExtractTriangleFromFace(int ix, int iy, int iz, bool[] bVertics, int[]Vertics,int[] Edges)
        {
            //    
            //  1----1---2
            //  |        |
            // 0|        |2
            //  0--------3
            //     3
            int n = 0, type = 0;
            if (bVertics[0]) { n++; type += 1; }
            if (bVertics[1]) { n++; type += 2; }
            if (bVertics[2]) { n++; type += 4; }
            if (bVertics[3]) { n++; type += 8; }
            if (n == 0) return 0;           
            VERTIC_EDGE_RELATION[] Relation = new VERTIC_EDGE_RELATION[4];
            Relation[0].vertics = Vertics[0];
            Relation[0].edge1 = Edges[3];
            Relation[0].edge2 = Edges[0];
            Relation[1].vertics = Vertics[1];
            Relation[1].edge1 = Edges[0];
            Relation[1].edge2 = Edges[1];
            Relation[2].vertics = Vertics[2];
            Relation[2].edge1 = Edges[1];
            Relation[2].edge2 = Edges[2];
            Relation[3].vertics = Vertics[3];
            Relation[3].edge1 = Edges[2];
            Relation[3].edge2 = Edges[3];
            FLOAT_POINT_EXT p;            
            int n1 = 0, n2 = 0, n3 = 0;
#pragma warning disable CS0168 // 声明了变量“i31”，但从未使用过
            int i1, i2, i3,i11,i12,i21,i22,i31,i32;
#pragma warning restore CS0168 // 声明了变量“i31”，但从未使用过
            int verIndex, edgeIndex;
            switch (n)
            {
                case 1:
                    for(int i=0;i<4;i++)
                    {
                        if (!bVertics[i]) continue;
                        verIndex = GetStoredVerticIndex(ix, iy, iz, Relation[i].vertics);
                        if (verIndex >= 0)
                        {
                            i1 = verIndex;
                            StoreVerticIndex(ix, iy, iz, Relation[i].vertics, verIndex);
                        }
                        else
                        {
                            i1 = GetCoordSize();
                            p = GetVerticCoord(ix, iy, iz, Relation[i].vertics);
                            StoreVerticIndex(ix, iy, iz, Relation[i].vertics, GetCoordSize());
                            AddCoordArray(p);
                        }                        
                        //edge2
                        edgeIndex = GetStoredEdgeIndex(ix, iy, iz, Relation[i].edge2);
                        if (edgeIndex >= 0)
                        {
                            i12 = edgeIndex;
                            StoreEdgeIndex(ix, iy, iz, Relation[i].edge2, edgeIndex);
                        }
                        else
                        {
                            i12 = GetCoordSize();
                            p = GetEdgeIntersection(ix, iy, iz, Relation[i].edge2, out bool crossed);                            
                            StoreEdgeIndex(ix, iy, iz, Relation[i].edge2, GetCoordSize());
                            AddCoordArray(p);
                        }
                        //edge1
                        edgeIndex = GetStoredEdgeIndex(ix, iy, iz, Relation[i].edge1);
                        if (edgeIndex >= 0)
                        {
                            i11 = edgeIndex;
                            StoreEdgeIndex(ix, iy, iz, Relation[i].edge1, edgeIndex);
                        }
                        else
                        {
                            i11 = GetCoordSize();
                            p = GetEdgeIntersection(ix, iy, iz, Relation[i].edge1, out bool crossed);
                            StoreEdgeIndex(ix, iy, iz, Relation[i].edge1, GetCoordSize());
                            AddCoordArray(p);
                        }
                        //i11 i1 i12 
                        pISOSurfaceExt.pTriangleIndex.Add(i11);
                        pISOSurfaceExt.pTriangleIndex.Add(i1);
                        pISOSurfaceExt.pTriangleIndex.Add(i12);
                    }
                    break;
                case 2:
                    //01 12 23 03   
                    if( type == 3 || type == 6 || type == 12 || type == 9)
                    {
                        for (int i = 0; i < 4; i++)      { if (bVertics[i]) { n1 = i; break; } }
                        for (int i = n1 + 1; i < 4; i++) { if (bVertics[i]) { n2 = i; break; } }
                        if( type == 9) { n1 = 3;n2 = 0; }
                        //n1
                        verIndex = GetStoredVerticIndex(ix, iy, iz, Relation[n1].vertics);
                        if (verIndex >= 0)
                        {
                            i1 = verIndex;
                            StoreVerticIndex(ix, iy, iz, Relation[n1].vertics, verIndex);
                        }
                        else
                        {
                            i1 = GetCoordSize();
                            p = GetVerticCoord(ix, iy, iz, Relation[n1].vertics);                            
                            StoreVerticIndex(ix, iy, iz, Relation[n1].vertics, GetCoordSize());
                            AddCoordArray(p);
                        }
                        //n1-edge1
                        edgeIndex = GetStoredEdgeIndex(ix, iy, iz, Relation[n1].edge1);
                        if (edgeIndex >= 0)
                        {
                            i11 = edgeIndex;
                            StoreEdgeIndex(ix, iy, iz, Relation[n1].edge1, edgeIndex);
                        }
                        else
                        {
                            i11 = GetCoordSize();
                            p = GetEdgeIntersection(ix, iy, iz, Relation[n1].edge1, out bool crossed);                            
                            StoreEdgeIndex(ix, iy, iz, Relation[n1].edge1, GetCoordSize());
                            AddCoordArray(p);
                        }

                        //n2
                        verIndex = GetStoredVerticIndex(ix, iy, iz, Relation[n2].vertics);
                        if (verIndex >= 0)
                        {
                            i2 = verIndex;                            
                            StoreVerticIndex(ix, iy, iz, Relation[n2].vertics, verIndex);
                        }
                        else
                        {
                            i2 = GetCoordSize();
                            p = GetVerticCoord(ix, iy, iz, Relation[n2].vertics);                            
                            StoreVerticIndex(ix, iy, iz, Relation[n2].vertics, GetCoordSize());
                            AddCoordArray(p);
                        }

                        //n2-edge2
                        edgeIndex = GetStoredEdgeIndex(ix, iy, iz, Relation[n2].edge2);
                        if (edgeIndex >= 0)
                        {
                            i22 = edgeIndex;                            
                            StoreEdgeIndex(ix, iy, iz, Relation[n2].edge2, edgeIndex);
                        }
                        else
                        {
                            i22 = GetCoordSize();
                            p = GetEdgeIntersection(ix, iy, iz, Relation[n2].edge2, out bool crossed);                            
                            StoreEdgeIndex(ix, iy, iz, Relation[n2].edge2, GetCoordSize());
                            AddCoordArray(p);
                        }
                        //i11 i1 i2,i2 i22 i11
                        pISOSurfaceExt.pTriangleIndex.Add(i11);
                        pISOSurfaceExt.pTriangleIndex.Add(i1);
                        pISOSurfaceExt.pTriangleIndex.Add(i2);
                        pISOSurfaceExt.pTriangleIndex.Add(i2);
                        pISOSurfaceExt.pTriangleIndex.Add(i22);
                        pISOSurfaceExt.pTriangleIndex.Add(i11);
                    }
                    if (type == 5 || type == 10 )
                    {
                        if( bVertics[0] ) { n1 = 0;n2 = 2; }
                        else { n1 = 1; n2 = 3; }
                        //n1
                        verIndex = GetStoredVerticIndex(ix, iy, iz, Relation[n1].vertics);
                        if (verIndex >= 0)
                        {
                            i1 = verIndex;
                            StoreVerticIndex(ix, iy, iz, Relation[n1].vertics, verIndex);
                        }
                        else
                        {
                            i1 = GetCoordSize();
                            p = GetVerticCoord(ix, iy, iz, Relation[n1].vertics);
                            StoreVerticIndex(ix, iy, iz, Relation[n1].vertics, GetCoordSize());
                            AddCoordArray(p);
                        }
                        //n2
                        verIndex = GetStoredVerticIndex(ix, iy, iz, Relation[n2].vertics);
                        if (verIndex >= 0)
                        {
                            i2 = verIndex;
                            StoreVerticIndex(ix, iy, iz, Relation[n2].vertics, verIndex);
                        }
                        else
                        {
                            i2 = GetCoordSize();
                            p = GetVerticCoord(ix, iy, iz, Relation[n2].vertics);
                            StoreVerticIndex(ix, iy, iz, Relation[n2].vertics, GetCoordSize());
                            AddCoordArray(p);
                        }
                        //n1 edge1
                        edgeIndex = GetStoredEdgeIndex(ix, iy, iz, Relation[n1].edge1);
                        if (edgeIndex >= 0)
                        {
                            i11 = edgeIndex;
                            StoreEdgeIndex(ix, iy, iz, Relation[n1].edge1, edgeIndex);
                        }
                        else
                        {
                            i11 = GetCoordSize();
                            p = GetEdgeIntersection(ix, iy, iz, Relation[n1].edge1, out bool crossed);
                            StoreEdgeIndex(ix, iy, iz, Relation[n1].edge1, GetCoordSize());
                            AddCoordArray(p);
                        }
                        //n1 edge2
                        edgeIndex = GetStoredEdgeIndex(ix, iy, iz, Relation[n1].edge2);
                        if (edgeIndex >= 0)
                        {
                            i12 = edgeIndex;
                            StoreEdgeIndex(ix, iy, iz, Relation[n1].edge2, edgeIndex);
                        }
                        else
                        {
                            i12 = GetCoordSize();
                            p = GetEdgeIntersection(ix, iy, iz, Relation[n1].edge2, out bool crossed);
                            StoreEdgeIndex(ix, iy, iz, Relation[n1].edge2, GetCoordSize());
                            AddCoordArray(p);
                        }
                        //n1 edge1
                        edgeIndex = GetStoredEdgeIndex(ix, iy, iz, Relation[n2].edge1);
                        if (edgeIndex >= 0)
                        {
                            i21 = edgeIndex;
                            StoreEdgeIndex(ix, iy, iz, Relation[n2].edge1, edgeIndex);
                        }
                        else
                        {
                            i21 = GetCoordSize();
                            p = GetEdgeIntersection(ix, iy, iz, Relation[n2].edge1, out bool crossed);
                            StoreEdgeIndex(ix, iy, iz, Relation[n2].edge1, GetCoordSize());
                            AddCoordArray(p);
                        }
                        //n2 edge2
                        edgeIndex = GetStoredEdgeIndex(ix, iy, iz, Relation[n2].edge2);
                        if (edgeIndex >= 0)
                        {
                            i22 = edgeIndex;
                            StoreEdgeIndex(ix, iy, iz, Relation[n2].edge2, edgeIndex);
                        }
                        else
                        {
                            i22 = GetCoordSize();
                            p = GetEdgeIntersection(ix, iy, iz, Relation[n2].edge2, out bool crossed);
                            StoreEdgeIndex(ix, iy, iz, Relation[n2].edge2, GetCoordSize());
                            AddCoordArray(p);
                        }
                        pISOSurfaceExt.pTriangleIndex.Add(i11);
                        pISOSurfaceExt.pTriangleIndex.Add(i1);
                        pISOSurfaceExt.pTriangleIndex.Add(i12);
                        pISOSurfaceExt.pTriangleIndex.Add(i21);
                        pISOSurfaceExt.pTriangleIndex.Add(i2);
                        pISOSurfaceExt.pTriangleIndex.Add(i22);

                        pISOSurfaceExt.pTriangleIndex.Add(i11);
                        pISOSurfaceExt.pTriangleIndex.Add(i12);
                        pISOSurfaceExt.pTriangleIndex.Add(i21);
                        
                        pISOSurfaceExt.pTriangleIndex.Add(i21);
                        pISOSurfaceExt.pTriangleIndex.Add(i22);
                        pISOSurfaceExt.pTriangleIndex.Add(i11);
                    }
                    break;
                case 3:
                    if (!bVertics[0]) { n1 = 1;n2 = 2;n3 = 3; }
                    if (!bVertics[1]) { n1 = 2; n2 = 3; n3 = 0; }
                    if (!bVertics[2]) { n1 = 3; n2 = 0; n3 = 1; }
                    if (!bVertics[3]) { n1 = 0; n2 = 1; n3 = 2; }
                    verIndex = GetStoredVerticIndex(ix, iy, iz, Relation[n1].vertics);
                    if (verIndex >= 0)
                    {
                        i1 = verIndex;
                        StoreVerticIndex(ix, iy, iz, Relation[n1].vertics, verIndex);
                    }
                    else
                    {
                        i1 = GetCoordSize();
                        p = GetVerticCoord(ix, iy, iz, Relation[n1].vertics);
                        StoreVerticIndex(ix, iy, iz, Relation[n1].vertics, GetCoordSize());
                        AddCoordArray(p);
                    }
                    verIndex = GetStoredVerticIndex(ix, iy, iz, Relation[n2].vertics);
                    if (verIndex >= 0)
                    {
                        i2 = verIndex;
                        StoreVerticIndex(ix, iy, iz, Relation[n2].vertics, verIndex);
                    }
                    else
                    {
                        i2 = GetCoordSize();
                        p = GetVerticCoord(ix, iy, iz, Relation[n2].vertics);
                        StoreVerticIndex(ix, iy, iz, Relation[n2].vertics, GetCoordSize());
                        AddCoordArray(p);
                    }
                    verIndex = GetStoredVerticIndex(ix, iy, iz, Relation[n3].vertics);
                    if (verIndex >= 0)
                    {
                        i3 = verIndex;
                        StoreVerticIndex(ix, iy, iz, Relation[n3].vertics, verIndex);
                    }
                    else
                    {
                        i3 = GetCoordSize();
                        p = GetVerticCoord(ix, iy, iz, Relation[n3].vertics);
                        StoreVerticIndex(ix, iy, iz, Relation[n3].vertics, GetCoordSize());
                        AddCoordArray(p);
                    }
                    //i11
                    edgeIndex = GetStoredEdgeIndex(ix, iy, iz, Relation[n1].edge1);
                    if (edgeIndex >= 0)
                    {
                        i11 = edgeIndex;
                        StoreEdgeIndex(ix, iy, iz, Relation[n1].edge1, edgeIndex);
                    }
                    else
                    {
                        i11 = GetCoordSize();
                        p = GetEdgeIntersection(ix, iy, iz, Relation[n1].edge1, out bool crossed);
                        StoreEdgeIndex(ix, iy, iz, Relation[n1].edge1, GetCoordSize());
                        AddCoordArray(p);
                    }
                    //i32
                    edgeIndex = GetStoredEdgeIndex(ix, iy, iz, Relation[n3].edge2);
                    if (edgeIndex >= 0)
                    {
                        i32 = edgeIndex;
                        StoreEdgeIndex(ix, iy, iz, Relation[n3].edge2, edgeIndex);
                    }
                    else
                    {
                        i32 = GetCoordSize();
                        p = GetEdgeIntersection(ix, iy, iz, Relation[n3].edge2, out bool crossed);
                        StoreEdgeIndex(ix, iy, iz, Relation[n3].edge2, GetCoordSize());
                        AddCoordArray(p);
                    }
                    pISOSurfaceExt.pTriangleIndex.Add(i11);
                    pISOSurfaceExt.pTriangleIndex.Add(i1);
                    pISOSurfaceExt.pTriangleIndex.Add(i2);

                    pISOSurfaceExt.pTriangleIndex.Add(i11);
                    pISOSurfaceExt.pTriangleIndex.Add(i2);
                    pISOSurfaceExt.pTriangleIndex.Add(i32);

                    pISOSurfaceExt.pTriangleIndex.Add(i32);                    
                    pISOSurfaceExt.pTriangleIndex.Add(i2);
                    pISOSurfaceExt.pTriangleIndex.Add(i3);
                    break;
                case 4:
                    //0
                    int[] vid = new int[4];
                    for (int i = 0; i < 4; i++)
                    {
                        verIndex = GetStoredVerticIndex(ix, iy, iz, Relation[i].vertics);
                        if (verIndex >= 0)
                        {
                            vid[i] = verIndex;
                            StoreVerticIndex(ix, iy, iz, Relation[i].vertics, vid[i]);
                        }
                        else
                        {
                            p = GetVerticCoord(ix, iy, iz, Relation[i].vertics);
                            vid[i] = GetCoordSize();
                            StoreVerticIndex(ix, iy, iz, Relation[i].vertics, GetCoordSize());
                            AddCoordArray(p);
                        }
                    }
                    pISOSurfaceExt.pTriangleIndex.Add(vid[0]);
                    pISOSurfaceExt.pTriangleIndex.Add(vid[1]);
                    pISOSurfaceExt.pTriangleIndex.Add(vid[2]);
                    pISOSurfaceExt.pTriangleIndex.Add(vid[2]);
                    pISOSurfaceExt.pTriangleIndex.Add(vid[3]);
                    pISOSurfaceExt.pTriangleIndex.Add(vid[0]);
                    break;
            }
                       
            return 1;
        }
        public int GetGridType(int ix, int iy, int iz)
        {
            return GetGridType(p3DData, ix, iy, iz);
        }
        public int GetGridType(C3DGridData data, int ix,int iy,int iz)
        {
            int iType = 0;
            for (int k = 0; k < 8; k++)
            {
                if (GetVerticShowState(data, ix, iy, iz, k))
                {
                    iType |= (1 << k);                        
                }
            }            
            return iType;
        }
        private void InitISOColor()
        {
            pISOSurfaceExt.pColor.Clear();
            for(int i=0;i< m_ColorScale.Count; i++)
            {
                pISOSurfaceExt.AddColor(m_ColorScale.GetColor(i));
            }
        }
        public int DoSearchEdges()           // return >0 successs, <0 error
        {
            //     |z
            //     4--------5    
            //    /|       /|
            //  7--------6  |
            //  |  0-----|--1--->y
            //  | /      | /
            //  3--------2
            //  /x
            int iType = 0;
            int ix, iy, iz;
            Int32XYZ[] pGridIndex = new Int32XYZ[8];            
            EDGE_POINT_INDEX[] pnext;
            pISOSurfaceExt.Clear();
            InitISOColor();
            InitEdgePointArray();
            pCurEdgePointArray  = pEdgePointArray1;
            pPrevEdgePointArray = pEdgePointArray2;
            searchedGridNo = 0;
            for ( iz = 0; iz<zGridNum-1;iz++)
            {
                for (iy = 0; iy < yGridNum - 1; iy++)
                    for (ix = 0; ix < xGridNum - 1; ix++)
                    {
                        //icur = (uint)(ix + iy * xGridNum + iz * xGridNum * yGridNum);

                        iType = GetGridType(ix, iy, iz);

                        if ( iType != 0 && iType !=255 )
                        {
                            ExtractTriangleFromGrid(ix, iy, iz, iType);
                            //CreateFromSurface(ix, iy, iz, iType);
                            searchedGridNo++;
                        }
                        //if (iType !=0 )
                        //CreateFromSurface(ix, iy, iz, iType);
                    }//for (iy = 0; iy < yGridNum - 1; iy++)

                 pnext = pPrevEdgePointArray;
                 pPrevEdgePointArray = pCurEdgePointArray;
                 pCurEdgePointArray = pnext;
                 ResetCurEdgePointArray();
            }//for( iz = 0; iz < zGridNum - 1; iz++ )

            CreateSurfaces();

            ReleaseEdgePointArray();
            pISOSurfaceExt.UpdateRange();
            return 1;
        }

        //创建六个侧面的三角片
        protected void CreateSurfaces()
        {
            int icube,iType = 0;
            int ix, iy, iz,id;
            
            MarchingCubes3DSurface ms = new MarchingCubes3DSurface();
            FLOAT_POINT_EXT p1;
            
            //top
            iy = yGridNum-2;
            for (iz = 0; iz < zGridNum - 1; iz++)
                for (ix = 0; ix < xGridNum - 1; ix++)
                {
                    icube = GetGridType(ix, iy, iz);
                    if (icube == 0) continue;
                    iType = ms.GetTypeIndex(icube, MarchingCubes3DSurface.CubeFaceEnum.Top);
                    if (iType == 0) continue;

                    for (int i = 0; i < 9; i++)
                    {
                        id = ms.triTable[iType, i];
                        if (id < 0) break;

                        if (id >= 10) //vertic no
                        {
                            id = ms.topVertics[id - 10];
                            p1 = GetVerticCoord(ix, iy, iz, id);
                        }
                        else //edgeno
                        {
                            id = ms.topEdges[id];
                            p1 = GetEdgeIntersection(ix, iy, iz, id, out bool crossed);
                        }
                        AddTiangleIndex(GetCoordSize());
                        AddCoordArray(p1);
                    }//for( int i = 0 ; i < 9 ; i++ )
                }//for (ix = 0; ix < xGridNum - 1; ix++)

            //bottom
            iy = 0;
            for (iz = 0; iz < zGridNum - 1; iz++)
                for (ix = 0; ix < xGridNum - 1; ix++)
                {
                    icube = GetGridType(ix, iy, iz);
                    if (icube == 0) continue;
                    iType = ms.GetTypeIndex(icube,MarchingCubes3DSurface.CubeFaceEnum.Bottom);
                    if (iType == 0) continue;

                    for( int i = 0 ; i < 9 ; i++ )
                    {
                        id = ms.triTable[iType, i];
                        if ( id < 0 ) break;

                        if( id >= 10 ) //vertic no
                        {
                            id = ms.bottomVertics[id-10];
                            p1 = GetVerticCoord(ix, iy, iz, id);
                        }
                        else //edgeno
                        {
                            id = ms.bottomEdges[id];
                            p1 = GetEdgeIntersection(ix, iy, iz, id, out bool crossed);
                        }
                        AddTiangleIndex(GetCoordSize());
                        AddCoordArray(p1);
                    }//for( int i = 0 ; i < 9 ; i++ )
                }//for (ix = 0; ix < xGridNum - 1; ix++)

            //left
            ix = 0;
            for (iz = 0; iz < zGridNum - 1; iz++)
                for (iy = 0; iy < yGridNum - 1; iy++)
                {
                    icube = GetGridType(ix, iy, iz);
                    if (icube == 0) continue;
                    iType = ms.GetTypeIndex(icube, MarchingCubes3DSurface.CubeFaceEnum.Left);
                    if (iType == 0) continue;

                    for (int i = 0; i < 9; i++)
                    {
                        id = ms.triTable[iType, i];
                        if (id < 0) break;

                        if (id >= 10) //vertic no
                        {
                            id = ms.leftVertics[id - 10];
                            p1 = GetVerticCoord(ix, iy, iz, id);
                        }
                        else //edgeno
                        {
                            id = ms.leftEdges[id];
                            p1 = GetEdgeIntersection(ix, iy, iz, id, out bool crossed);
                        }
                        AddTiangleIndex(GetCoordSize());
                        AddCoordArray(p1);
                    }//for( int i = 0 ; i < 9 ; i++ )
                }//for (ix = 0; ix < xGridNum - 1; ix++)

            //right
            ix = xGridNum-2;
            for (iz = 0; iz < zGridNum - 1; iz++)
                for (iy = 0; iy < yGridNum - 1; iy++)
                {
                    icube = GetGridType(ix, iy, iz);
                    if (icube == 0) continue;
                    iType = ms.GetTypeIndex(icube, MarchingCubes3DSurface.CubeFaceEnum.Right);
                    if (iType == 0) continue;

                    for (int i = 0; i < 9; i++)
                    {
                        id = ms.triTable[iType, i];
                        if (id < 0) break;

                        if (id >= 10) //vertic no
                        {
                            id = ms.rightVertics[id - 10];
                            p1 = GetVerticCoord(ix, iy, iz, id);
                        }
                        else //edgeno
                        {
                            id = ms.rightEdges[id];
                            p1 = GetEdgeIntersection(ix, iy, iz, id, out bool crossed);
                        }
                        AddTiangleIndex(GetCoordSize());
                        AddCoordArray(p1);
                    }//for( int i = 0 ; i < 9 ; i++ )
                }//for (ix = 0; ix < xGridNum - 1; ix++)

            //front
            iz = zGridNum - 2;
            for (iy = 0; iy < yGridNum - 1; iy++)
                for (ix = 0; ix < xGridNum - 1; ix++)
                {
                    icube = GetGridType(ix, iy, iz);
                    if (icube == 0) continue;
                    iType = ms.GetTypeIndex(icube, MarchingCubes3DSurface.CubeFaceEnum.Front);
                    if (iType == 0) continue;

                    for (int i = 0; i < 9; i++)
                    {
                        id = ms.triTable[iType, i];
                        if (id < 0) break;

                        if (id >= 10) //vertic no
                        {
                            id = ms.frontVertics[id - 10];
                            p1 = GetVerticCoord(ix, iy, iz, id);
                        }
                        else //edgeno
                        {
                            id = ms.frontEdges[id];
                            p1 = GetEdgeIntersection(ix, iy, iz, id, out bool crossed);
                        }
                        AddTiangleIndex(GetCoordSize());
                        AddCoordArray(p1);
                    }//for( int i = 0 ; i < 9 ; i++ )
                }//for (ix = 0; ix < xGridNum - 1; ix++)

            //back
            iz = 0;
            for (iy = 0; iy < yGridNum - 1; iy++)
                for (ix = 0; ix < xGridNum - 1; ix++)
                {
                    icube = GetGridType(ix, iy, iz);
                    if (icube == 0) continue;
                    iType = ms.GetTypeIndex(icube, MarchingCubes3DSurface.CubeFaceEnum.Back);
                    if (iType == 0) continue;

                    for (int i = 0; i < 9; i++)
                    {
                        id = ms.triTable[iType, i];
                        if (id < 0) break;

                        if (id >= 10) //vertic no
                        {
                            id = ms.backVertics[id - 10];
                            p1 = GetVerticCoord(ix, iy, iz, id);
                        }
                        else //edgeno
                        {
                            id = ms.backEdges[id];
                            p1 = GetEdgeIntersection(ix, iy, iz, id, out bool crossed);
                        }
                        AddTiangleIndex(GetCoordSize());
                        AddCoordArray(p1);
                    }//for( int i = 0 ; i < 9 ; i++ )
                }//for (ix = 0; ix < xGridNum - 1; ix++)

        }//CreateSurfaces()
        protected void CreateFromSurface(int ix,int iy,int iz,int iType)
        {            
            bool[] bVertic = new bool[4];
            int[] verIndex = new int[4];
            int[] edgeIndex = new int[4];
            bool bx1, bx2, by1, by2, bz1, bz2;
            //search each 6 face
            bx1 = bx2 = false;
            by1 = by2 = false;
            bz1 = bz2 = false;
            if (ix == 0) bx1 = true;
            if (iy == 0) by1 = true;
            if (iz == 0) bz1 = true;
            if (ix == xGridNum - 2) bx2 = true;
            if (iy == yGridNum - 2) by2 = true;
            if (iz == zGridNum - 2) bz2 = true;
            bool[] pShow = new bool[8];
            for (int i = 0; i < 8; i++) pShow[i] = false;
            if ((iType & 1) > 0) pShow[0] = true;
            if ((iType & 2) > 0) pShow[1] = true;
            if ((iType & 4) > 0) pShow[2] = true;
            if ((iType & 8) > 0) pShow[3] = true;
            if ((iType & 16) > 0) pShow[4] = true;
            if ((iType & 32) > 0) pShow[5] = true;
            if ((iType & 64) > 0) pShow[6] = true;
            if ((iType & 128) > 0) pShow[7] = true;
            
            if (bx1)//3 7 4 0,11 7 8 3
            {
                verIndex[0] = 3; verIndex[1] = 7; verIndex[2] = 4; verIndex[3] = 0;
                for (int i = 0; i < 4; i++) bVertic[i] = pShow[verIndex[i]];
                edgeIndex[0] = 11; edgeIndex[1] = 7; edgeIndex[2] = 8; edgeIndex[3] = 3;
                ExtractTriangleFromFace(ix, iy, iz, bVertic, verIndex, edgeIndex);
            }
            if (bx2)//1562,95101
            {
                verIndex[0] = 1; verIndex[1] = 5; verIndex[2] = 6; verIndex[3] = 2;
                for (int i = 0; i < 4; i++) bVertic[i] = pShow[verIndex[i]];
                edgeIndex[0] = 9; edgeIndex[1] = 5; edgeIndex[2] = 10; edgeIndex[3] = 1;
                ExtractTriangleFromFace(ix, iy, iz, bVertic, verIndex, edgeIndex);
            }
            if (by1)//0123,0123
            {
                verIndex[0] = 0; verIndex[1] = 1; verIndex[2] = 2; verIndex[3] = 3;
                for (int i = 0; i < 4; i++) bVertic[i] = pShow[verIndex[i]];
                edgeIndex[0] = 0; edgeIndex[1] = 1; edgeIndex[2] = 2; edgeIndex[3] = 3;
                ExtractTriangleFromFace(ix, iy, iz, bVertic, verIndex, edgeIndex);
            }
            if (by2)//6547,5476
            {
                verIndex[0] = 6; verIndex[1] = 5; verIndex[2] = 4; verIndex[3] = 7;
                for (int i = 0; i < 4; i++) bVertic[i] = pShow[verIndex[i]];
                edgeIndex[0] = 5; edgeIndex[1] = 4; edgeIndex[2] = 7; edgeIndex[3] = 6;
                ExtractTriangleFromFace(ix, iy, iz, bVertic, verIndex, edgeIndex);
            }
            if (bz1)//0451,8490
            {
                verIndex[0] = 0; verIndex[1] = 4; verIndex[2] = 5; verIndex[3] = 1;
                for (int i = 0; i < 4; i++) bVertic[i] = pShow[verIndex[i]];
                edgeIndex[0] = 8; edgeIndex[1] = 4; edgeIndex[2] = 9; edgeIndex[3] = 0;
                ExtractTriangleFromFace(ix, iy, iz, bVertic, verIndex, edgeIndex);
            }
            if (bz2)//2673,106112
            {
                verIndex[0] = 2; verIndex[1] = 6; verIndex[2] = 7; verIndex[3] = 3;
                for (int i = 0; i < 4; i++) bVertic[i] = pShow[verIndex[i]];
                edgeIndex[0] = 10; edgeIndex[1] = 6; edgeIndex[2] = 11; edgeIndex[3] = 2;
                ExtractTriangleFromFace(ix, iy, iz, bVertic, verIndex, edgeIndex);
            }
        }  
    }
    #endregion //Class of MarchingCubesExt

    #region Class of MultiPropertiesMarchingCubes
    public class MultiPropertiesMarchingCubes : MarchingCubesExt
    {
        vec3 []pEdgeCoords = null;  //每个网格3个边的交点坐标
        Int32XYZ[] pEdgeCoordIndices = null;//每个网格3个边的交点坐标数组索引

        C3DGridData allGrid3d = null;
        int currentProperty = -1; //当前数据
        public List<C3DGridData> Properties = new List<C3DGridData>();
        public void AddProperty(C3DGridData data)
        {
            Properties.Add(data);
        }
        public override void Clear()
        {
            base.Clear();
            Properties.Clear(); 
        }
        void CreateEdgeCoords()
        {   
            pEdgeCoords = new vec3[xGridNum*yGridNum*zGridNum];
            for(int i=0;i<pEdgeCoords.Length;i++)
            {
                pEdgeCoords[i] = new vec3(float.NaN, float.NaN, float.NaN);
            }
            pEdgeCoordIndices = new Int32XYZ[xGridNum * yGridNum * zGridNum];
            for (int i = 0; i < pEdgeCoordIndices.Length; i++)
            {
                pEdgeCoordIndices[i] = new Int32XYZ(-1,-1,-1);
            }
        }
        bool IsExistEdgeCoord(int id, AxisEnum axis)
        {
            vec3 p = pEdgeCoords[id];
            if (axis == AxisEnum.xAxis && !float.IsNaN(p.x)) return true;
            if (axis == AxisEnum.yAxis && !float.IsNaN(p.y)) return true;
            if (axis == AxisEnum.zAxis && !float.IsNaN(p.z)) return true;
            return false;
        }
        bool IsExistEdgeCoordIndex(int id, AxisEnum axis)
        {
            Int32XYZ p = pEdgeCoordIndices[id];
            if (axis == AxisEnum.xAxis && p.x >= 0 ) return true;
            if (axis == AxisEnum.yAxis && p.y >= 0 ) return true;
            if (axis == AxisEnum.zAxis && p.z >= 0 ) return true;
            return false;
        }
        float GetEdgeCoord(int id, AxisEnum axis)
        {
            vec3 p = pEdgeCoords[id];
            if (axis == AxisEnum.xAxis) return p.x;
            if (axis == AxisEnum.yAxis) return p.y;
            if (axis == AxisEnum.zAxis) return p.z;
            return float.NaN;
        }
        int GetEdgeCoordIndex(int id, AxisEnum axis)
        {
            Int32XYZ p = pEdgeCoordIndices[id];
            if (axis == AxisEnum.xAxis) return p.x;
            if (axis == AxisEnum.yAxis) return p.y;
            if (axis == AxisEnum.zAxis) return p.z;
            return -1;            
        }
        void SetEdgeCoordIndex(int id, AxisEnum axis,int index)
        {
            Int32XYZ p = pEdgeCoordIndices[id];
            if (axis == AxisEnum.xAxis) p.x = index;
            if (axis == AxisEnum.yAxis) p.y = index;
            if (axis == AxisEnum.zAxis) p.z = index;
            pEdgeCoordIndices[id] = p;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <param name="p"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        bool SetEdgeCoord(int id, FLOAT_POINT_EXT p, AxisEnum axis)
        {
            bool modified = false;
            vec3 vp = pEdgeCoords[id];
            if (float.IsNaN(GetEdgeCoord(id, axis)))
            {
                if (axis == AxisEnum.xAxis) vp.x = p.x;
                if (axis == AxisEnum.yAxis) vp.y = p.y;
                if (axis == AxisEnum.zAxis) vp.z = p.z;
                modified = true;
            }
            else            
            {
                bool blank0 = allGrid3d.GetShowState(id);
                if (blank0) //保留低值
                {
                    if (axis == AxisEnum.xAxis && p.x < vp.x){ vp.x = p.x; modified = true;}
                    if (axis == AxisEnum.yAxis && p.y < vp.y){ vp.y = p.y; modified = true;}
                    if (axis == AxisEnum.zAxis && p.z < vp.z){ vp.z = p.z; modified = true;}
                }
                else //保留高值
                {
                    if (axis == AxisEnum.xAxis && p.x > vp.x) { vp.x = p.x; modified = true; }
                    if (axis == AxisEnum.yAxis && p.y > vp.y) { vp.y = p.y; modified = true; }
                    if (axis == AxisEnum.zAxis && p.z > vp.z) { vp.z = p.z; modified = true; }
                }
            }
            pEdgeCoords[id] = vp;
            return modified;
        }
        FLOAT_POINT_EXT SetEdgeCoord(int id, List<FLOAT_POINT_EXT>coords, AxisEnum axis)
        {            
            float v1 = 0, v2 = 0;
            vec3 vp = pEdgeCoords[id];
            FLOAT_POINT_EXT p0 = new FLOAT_POINT_EXT(vp.x, vp.y, vp.z, 0),p1;
            if (axis == AxisEnum.xAxis && !float.IsNaN(vp.x)) coords.Add(p0);
            else if (axis == AxisEnum.yAxis && !float.IsNaN(vp.y)) coords.Add(p0);
            else if (axis == AxisEnum.zAxis && !float.IsNaN(vp.z)) coords.Add(p0);

            if (axis == AxisEnum.xAxis)
            {
                coords.Sort((a, b) => { return a.x.CompareTo(b.x); });
                v1 = coords[0].x;
                v2 = coords[coords.Count - 1].x;
            }
            else if (axis == AxisEnum.yAxis)
            {
                coords.Sort((a, b) => { return a.y.CompareTo(b.y); });
                v1 = coords[0].y;
                v2 = coords[coords.Count - 1].y;
            }
            else if (axis == AxisEnum.zAxis)
            {
                coords.Sort((a, b) => { return a.z.CompareTo(b.z); });
                v1 = coords[0].z;
                v2 = coords[coords.Count - 1].z;
            }
            short icolor = 0;
            bool blank0 = allGrid3d.GetShowState(id);
            if (blank0) //保留低值
            {
                p1 = coords[0];
                if (axis == AxisEnum.xAxis) vp.x = p1.x = v1; 
                else if (axis == AxisEnum.yAxis) vp.y = p1.y = v1;
                else if (axis == AxisEnum.zAxis) vp.z = p1.z = v1;
                icolor = coords[0].icolor;
                
            }
            else //保留高值
            {
                p1 = coords[coords.Count - 1];
                if (axis == AxisEnum.xAxis) vp.x = p1.x = v2;
                if (axis == AxisEnum.yAxis) vp.y = p1.y = v2;
                if (axis == AxisEnum.zAxis) vp.z = p1.z = v2;
                icolor = coords[coords.Count-1].icolor;
            }
            pEdgeCoords[id] = vp;
            return p1;
        }
        public override void SetData(C3DGridData pdata)
        {
            if (allGrid3d == null) allGrid3d = pdata.Copy();
            base.SetData(pdata);
            pISOSurfaceExt.pColor.Clear();
            pISOSurfaceExt.AddColor(Color.BlueViolet);
            pISOSurfaceExt.AddColor(Color.LightSeaGreen);
            pISOSurfaceExt.AddColor(Color.DarkOrange);
            for (int i = 0; i < m_ColorScale.Count; i++)
            {
                pISOSurfaceExt.AddColor(m_ColorScale.GetColor(i));
            }
            allGrid3d.InitBlankTable();
        }
        void UpdatePropertyShowTable(C3DGridData data)
        {
            for (int i = 0; i < data.pgridShowTable.Length; i++)
            {
                if (allGrid3d.pgridShowTable[i] > 0 &&
                    data.pgridShowTable[i] > 0)
                {
                    allGrid3d.pgridShowTable[i] = 1;
                }
                else allGrid3d.pgridShowTable[i] = 0;
            }
        }
        void UpdatePropertyShowTables()
        {
            for(int i=0;i<Properties.Count;i++)
            {
                UpdatePropertyShowTable(Properties[i]);
            }
        }
        void UpdatePropertyBlankedPoints()
        {
            allGrid3d.InitBlankTable();
            for (int k = 0; k < Properties.Count; k++)
            {
                C3DGridData data = Properties[k];
                if( data.pBlankedPoints.Count > 0 )
                {
                    allGrid3d.pBlankedPoints = new List<vec3>(data.pBlankedPoints);
                    if (data.pBlankTable != null)
                    {
                        allGrid3d.pBlankTable = new bool[data.pBlankTable.Length];
                        for (int i = 0; i < allGrid3d.pBlankTable.Length; i++)
                            allGrid3d.pBlankTable[i] = data.pBlankTable[i];
                    }
                    if(data.pBlankedPointIndexes != null)
                    {
                        allGrid3d.pBlankedPointIndexes = new int[data.pBlankedPointIndexes.Length];
                        for (int i = 0; i < allGrid3d.pBlankedPointIndexes.Length; i++)
                            allGrid3d.pBlankedPointIndexes[i] = data.pBlankedPointIndexes[i];
                    }
                }
            }
        }
        public bool DoSearchMultipleEdges1()
        {
            if (Properties.Count == 0) return false;
            //     |z
            //     4--------5    
            //    /|       /|
            //  7--------6  |
            //  |  0-----|--1--->y
            //  | /      | /
            //  3--------2
            //  /x
            int iType = 0;
            int ix, iy, iz;
            pISOSurfaceExt.Clear();
            searchedGridNo = 0;
            if (allGrid3d != null) { allGrid3d.Clear(); allGrid3d = null; }
            SetData(Properties[0]);
            CreateEdgeCoords();
            UpdatePropertyShowTables();
            for (iz = 0; iz < zGridNum - 1; iz++)
            {
                for (iy = 0; iy < yGridNum - 1; iy++)
                    for (ix = 0; ix < xGridNum - 1; ix++)
                    {
                        iType = GetGridType(allGrid3d, ix, iy, iz);                        
                        if (iType != 0 && iType != 255)
                        {
                            ExtractTriangleFromGrid(ix, iy, iz, iType);
                            //CreateFromSurface(ix, iy, iz, iType);
                            searchedGridNo++;
                        }
                        if (iType != 0)CreateFromSurface(ix, iy, iz, iType);
                    }//for (iy = 0; iy < yGridNum - 1; iy++)
            }//for( iz = 0; iz < zGridNum - 1; iz++ )
            //CreateSurfaces();
            pISOSurfaceExt.UpdateRange();
            return true;
        }
        public bool DoSearchMultipleEdges()
        {
            if (Properties.Count == 0) return false;            
            //     |z
            //     4--------5    
            //    /|       /|
            //  7--------6  |
            //  |  0-----|--1--->y
            //  | /      | /
            //  3--------2
            //  /x
            int iType = 0;
            int ix, iy, iz;
            EDGE_POINT_INDEX[] pnext;
            pISOSurfaceExt.Clear();            
            searchedGridNo = 0;
            if (allGrid3d != null) { allGrid3d.Clear(); allGrid3d = null; }
            SetData(Properties[0]);
            CreateEdgeCoords();
            UpdatePropertyShowTables();
            UpdatePropertyBlankedPoints();
            InitEdgePointArray();
            pCurEdgePointArray = pEdgePointArray1;
            pPrevEdgePointArray = pEdgePointArray2;
            for (int k = 0; k < Properties.Count; k++)
            {  
                if(k>0)SetData(Properties[k]);               
                currentProperty = k;                
                for (iz = 0; iz < zGridNum - 1; iz++)
                {
                    for (iy = 0; iy < yGridNum - 1; iy++)
                        for (ix = 0; ix < xGridNum - 1; ix++)
                        {
                            iType = GetGridType(allGrid3d, ix, iy, iz);
                            //if (iType == 0 || iType == 255) continue;
                            //iType = GetGridType(ix, iy, iz);
                            if (iType != 0 && iType != 255)
                            {
                                ExtractTriangleFromGrid1(ix, iy, iz, iType);
                                //CreateFromSurface(ix, iy, iz, iType);
                                searchedGridNo++;
                            }
                            //if (iType !=0 )
                            //CreateFromSurface(ix, iy, iz, iType);
                        }//for (iy = 0; iy < yGridNum - 1; iy++)

                    pnext = pPrevEdgePointArray;
                    pPrevEdgePointArray = pCurEdgePointArray;
                    pCurEdgePointArray = pnext;
                    ResetCurEdgePointArray();
                }//for( iz = 0; iz < zGridNum - 1; iz++ )

            }
            //CreateSurfaces();            
             ReleaseEdgePointArray();
            pISOSurfaceExt.UpdateRange();
            return true;
        }
        public int ExtractTriangleFromGrid1(int ix, int iy, int iz, int typeIndex)
        {
            //     |z
            //     4----f---5    
            //    /|       /|
            //  7--------6  |
            //  |  0---a-|--1--->y
            //  |d/      | /b
            //  3--------2
            //  /x  c
            FLOAT_POINT_EXT p;
            List<FLOAT_POINT_EXT> coords = new List<FLOAT_POINT_EXT>();
            int edno, coordIndex, id;
            AxisEnum axis = AxisEnum.xAxis;
            // searching the edge table,till -1 end
            for (int i = 0; i < 24; i++)
            {
                edno = triTable[typeIndex, i];
                if (edno < 0) break;//三角形结束
                id = GetVerticIndexByEdge(ix, iy, iz, edno, out axis);
                if (IsExistEdgeCoordIndex(id, axis))
                {
                    coordIndex = GetEdgeCoordIndex(id, axis);
                    pISOSurfaceExt.AddTriangleIndex(coordIndex);
                }
                else
                {
                    coords = GetEdgeIntersections(ix,iy,iz,edno);
                    p = SetEdgeCoord(id, coords, axis);
                    SetEdgeCoordIndex(id, axis, GetCoordSize());
                    pISOSurfaceExt.AddTriangleIndex(GetCoordSize());
                    pISOSurfaceExt.AddCoord(p);
                    coords.Clear();
                }                

            }//for
            return 1;
        }
        public override int ExtractTriangleFromGrid(int ix, int iy, int iz, int typeIndex)
        {
            //     |z
            //     4----f---5    
            //    /|       /|
            //  7--------6  |
            //  |  0---a-|--1--->y
            //  |d/      | /b
            //  3--------2
            //  /x  c
            FLOAT_POINT_EXT p;
            int edno, coordIndex,id;
            AxisEnum axis = AxisEnum.xAxis;
            // searching the edge table,till -1 end
            for (int i = 0; i < 24; i++)
            {
                edno = triTable[typeIndex, i];
                if (edno < 0) break;//三角形结束
                id = GetVerticIndexByEdge(ix, iy, iz, edno, out axis);
                if (currentProperty == 0)//第一遍,不比交点坐标
                {                    
                    if (IsExistEdgeCoordIndex(id, axis))
                    {
                        coordIndex = GetEdgeCoordIndex(id, axis);
                        pISOSurfaceExt.AddTriangleIndex(coordIndex);
                    }
                    else 
                    {
                        p = GetEdgeIntersection(ix, iy, iz, edno, out bool crossed);
                        if(crossed)SetEdgeCoord(id, p, axis);
                        SetEdgeCoordIndex(id, axis, GetCoordSize());
                        pISOSurfaceExt.AddTriangleIndex(GetCoordSize());
                        pISOSurfaceExt.AddCoord(p);
                    }                       
                }
                else //第2遍开始，修改交点坐标 
                {                    
                    p = GetEdgeIntersection(ix, iy, iz, edno,out bool crossed);
                    if (crossed) 
                    {
                        if( SetEdgeCoord(id, p, axis) )
                        {
                            coordIndex = GetEdgeCoordIndex(id, axis);
                           // if (coordIndex >= 0)
                            {
                                p = pISOSurfaceExt.pCoordArray[coordIndex];
                                float val = GetEdgeCoord(id, axis);
                                if (axis == AxisEnum.xAxis) p.x = val;
                                if (axis == AxisEnum.yAxis) p.y = val;
                                if (axis == AxisEnum.zAxis) p.z = val;
                                pISOSurfaceExt.pCoordArray[coordIndex] = p;
                            }
                        }
                    }                    
                }                
                
            }//for
            return 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ix"></param>
        /// <param name="iy"></param>
        /// <param name="iz"></param>
        /// <param name="edno"></param>
        /// <param name="crossed"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected List<FLOAT_POINT_EXT> GetEdgeIntersections(int ix, int iy, int iz, int edno)
        {
            double v1, v2;
            bool b1, b2,crossed;        
            List<FLOAT_POINT_EXT>coords = new List<FLOAT_POINT_EXT>();            
            int id1 = GetVerticIndex(ix, iy, iz, vertEdgeRelation[edno].x);
            int id2 = GetVerticIndex(ix, iy, iz, vertEdgeRelation[edno].y);
            bool blank1 = allGrid3d.IsBlankedGrid(id1);
            bool blank2 = allGrid3d.IsBlankedGrid(id2);      
            
            for (int k=0;k<Properties.Count;k++)
            {
                p3DData = Properties[k];
                InitClosedValue(p3DData.ColorScale);
                //2 vertices value v1,v2 of this edge
                v1 = p3DData.pGridData[id1];
                v2 = p3DData.pGridData[id2];
                //2 vertices show states
                b1 = GetVerticShowState(p3DData, id1);
                b2 = GetVerticShowState(p3DData, id2);
                if( b1 == !b2) 
                {
                    FLOAT_POINT_EXT p = GetEdgeIntersection(ix, iy, iz, edno, out crossed);
                    p.icolor = (short)k;
                    coords.Add(p);
                }
            }
            if(coords.Count == 0) 
            {
                throw (new Exception("no edge intersection found."));                
            }
            return coords;
        }
        
        protected override FLOAT_POINT_EXT GetEdgeIntersection(int ix, int iy, int iz, int edno,out bool crossed)
        {
            crossed = false;
            FLOAT_POINT_EXT p = new FLOAT_POINT_EXT(float.NaN, float.NaN, float.NaN);
            //2 vertices value v1,v2 of this edge
            int id1 = GetVerticIndex(ix, iy, iz, vertEdgeRelation[edno].x);
            int id2 = GetVerticIndex(ix, iy, iz, vertEdgeRelation[edno].y);
            double v1 = p3DData.pGridData[id1];
            double v2 = p3DData.pGridData[id2];            
            bool blank1 = p3DData.IsBlankedGrid(id1);
            bool blank2 = p3DData.IsBlankedGrid(id2);

            //2 vertices show states
            bool b1 = GetVerticShowState(p3DData, id1);
            bool b2 = GetVerticShowState(p3DData, id2);            
            
            double scale = 0, crossvalue;
            if (v1 != v2)
            {
                crossed = GetCrossValue(v1, v2, b1, b2, out crossvalue);
                if (crossed) 
                { 
                    scale = (crossvalue - v1) / (v2 - v1);
                    //double scale = GetNearestValue(v1, v2, b1, b2);                
                    //GetCrossValue(v1, v2, b1, b2, out crossvalue);                
                    p = GetVerticCoord(ix, iy, iz, vertEdgeRelation[edno].x);
                    p.x += (float)(xGridStep * scale * vertEdgeDirect[edno].x);
                    p.y += (float)(yGridStep * scale * vertEdgeDirect[edno].y);
                    p.z += (float)(zGridStep * scale * vertEdgeDirect[edno].z);
                    p.icolor = (short)currentProperty;
                    p.alpha = 255;
                }
            }

            if (blank1 == !blank2)//get blanked point p1
            {
                FLOAT_POINT_EXT p1 =
                GetBlankEdgeIntersetion(ix, iy, iz, edno, blank1, blank2, (float)scale, crossed);
                p1.icolor = (short)currentProperty;
                p1.alpha = 255;
                p = p1;
            }            

            return p;
        }
        public TriangleObj toTiangleObj()
        {
            return pISOSurfaceExt.toTriangleObj();
        }
    }
    #endregion //Class of MultiPropertiesMarchingCubes
}
