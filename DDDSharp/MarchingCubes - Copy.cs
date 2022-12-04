using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace DDDSharp
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
    }
    public struct FLOAT_POINT_EXT
    {
        public float x, y, z;
        public Int16 icolor;
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
        public int[] pIndex;    // each point index in pCoordArray, -1 no point
        public EDGE_POINT_INDEX()
        {
            pIndex = new int[20];
            for (int i = 0; i < 20; i++)
                pIndex[i] = -1;
        }
        public void Reset()
        {
            for (int i = 0; i < 20; i++)
                pIndex[i] = -1;
        }
    }
    public class CISOSurface
    {
        public float isoVale;
        public List<FLOAT_POINT> pCoordArray;            //triangle coords array
        public List<int> pTriangleIndex;             //
        public List<FLOAT_POINT> pTriangleNormalArray;           //triangle coords array
        public CISOSurface()
        {
            isoVale = 0;
            pCoordArray = new List<FLOAT_POINT>();
            pTriangleIndex = new List<int>();
            pTriangleNormalArray = new List<FLOAT_POINT>();
        }
    }
    public class CISOSurfaceExt: IDisposable
    {
        //triangle coords array
        public List<FLOAT_POINT_EXT> pCoordArray = new List<FLOAT_POINT_EXT>();
        //coord color table
        public List<Color> pColor = new List<Color>();
        //triangle coord index list
        public List<int> pTriangleIndex = new List<int>();             //
        //triangle normal array
        public List<FLOAT_POINT> pTriangleNormalArray = new List<FLOAT_POINT>();
        public CISOSurfaceExt()
        {         
        }
        public void Clear()
        {
            pCoordArray.Clear();
            //pColor.Clear();
            pTriangleIndex.Clear();
            pTriangleNormalArray.Clear();
        }
        public void AddColor(Color p)
        {
            pColor.Add(p);
        }
        public FLOAT_POINT_EXT GetPoint(int id)
        {
            return pCoordArray[id];
        }
        public Color GetColor(int index)
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
        //             y
        //             |-------> y2 
        //             |-------> y1
        //          ---0------->x
        //            / -- z2--
        //           /z -- z1--
        //   OpenGL coord compitable, 
        //	 Z is the elevation in geology
        //   data sort as Z(desend) to YOX

        protected float[] pGridData;   // grid data array, data 
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

            InitMatchTables();
        }
        
        // 256 match table	
        protected int[] edgeTable;     //[256]
        protected int[,] triTable;	 //[256][36]
        private void InitMatchTables()
        {
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

            if (nx < 5 || ny < 5 || nz < 5)
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
            if (xGridNum < 5 || yGridNum < 5 || zGridNum < 5)
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
        private GRID_EDGE_INDEX[] pGrigEdgeIndex;    //       
        public int DoSearchSurface(double isoValue,byte[]pgridshow=null)           // return >0 successs, <0 error
        {
            if (pGridData == null)
            {
                m_ErrInfo = "input data is not ready.";
                return -1;
            }
            if (xGridNum < 5 || yGridNum < 5 || zGridNum < 5)
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
    //Extract the edge of the cubes
    public class MarchingCubesExt : MarchingCubes
    {
        public CISOSurfaceExt pISOSurfaceExt = new CISOSurfaceExt();
        private List<EDGE_POINT_INDEX> pEdgePointArray = new List<EDGE_POINT_INDEX>();
        private C3DGridData p3DData;
        private CColorScale m_ColorScale = new CColorScale();
        private int GetVerticIndex(int ix,int iy,int iz,int verno)
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
        //Get vertics coord according to the index
        private FLOAT_POINT_EXT GetVerticCoord(int ix, int iy, int iz, int ver)
        {
            FLOAT_POINT_EXT p = new FLOAT_POINT_EXT();            
            p.x = (float)(xMin + ix * xGridStep);
            p.y = (float)(yMin + iy * yGridStep);
            p.z = (float)(zMin + iz * zGridStep);
            int icur = ix + iy * xGridNum + iz * xyGrid;            
            switch(ver)
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
            p.icolor = (short)GetColorIndex(p3DData.pGridData[icur]);
            return p;
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
                    p[no++] = p3DData.pGridData[grid[0]];//0                                                           
                    p[no++] = p3DData.pGridData[grid[1]];//1
                    p[no++] = (p3DData.pGridData[grid[4]] + p3DData.pGridData[grid[5]]) / 2.0;                    
                    p[no++] = (p3DData.pGridData[grid[3]] + p3DData.pGridData[grid[2]]) / 2.0;                    
                    if ( iy > 0  )//y--
                    {
                        icur1 = grid[0] - xGridNum;
                        icur2 = grid[1] - xGridNum;
                        p[no++] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;                        
                    }
                    if ( iz > 0 )//z--
                    {
                        icur1 = grid[0] - xyGrid;
                        icur2 = grid[1] - xyGrid;
                        p[no++] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;                        
                    }
                    break;
                case 1:
                    p[no++] = p3DData.pGridData[grid[1]];//1
                    p[no++] = p3DData.pGridData[grid[2]];//2
                    p[no++] = (p3DData.pGridData[grid[5]] + p3DData.pGridData[grid[6]]) / 2.0;
                    p[no++] = (p3DData.pGridData[grid[0]] + p3DData.pGridData[grid[3]]) / 2.0;
                    if (iy > 0)//y--
                    {
                        icur1 = grid[1] - xGridNum;
                        icur2 = grid[2] - xGridNum;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix < xGridNum - 2)
                    {
                        icur1 = grid[1] + 1;
                        icur2 = grid[2] + 1;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 2:
                    p[no++] = p3DData.pGridData[grid[2]];//2
                    p[no++] = p3DData.pGridData[grid[3]];//3
                    p[no++] = (p3DData.pGridData[grid[0]] + p3DData.pGridData[grid[1]]) / 2.0;
                    p[no++] = (p3DData.pGridData[grid[6]] + p3DData.pGridData[grid[7]]) / 2.0;
                    if (iy > 0)//y--
                    {
                        icur1 = grid[2] - xGridNum;
                        icur2 = grid[3] - xGridNum;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }                    
                    if (iz < zGridNum - 2)
                    {
                        icur1 = grid[2] + xyGrid;
                        icur2 = grid[3] + xyGrid;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 3:
                    p[no++] = p3DData.pGridData[grid[0]];//0
                    p[no++] = p3DData.pGridData[grid[3]];//3
                    p[no++] = (p3DData.pGridData[grid[1]] + p3DData.pGridData[grid[2]]) / 2.0;
                    p[no++] = (p3DData.pGridData[grid[4]] + p3DData.pGridData[grid[7]]) / 2.0;
                    if (iy > 0)//y--
                    {
                        icur1 = grid[0] - xGridNum;
                        icur2 = grid[3] - xGridNum;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix  >  0)
                    {
                        icur1 = grid[0] - 1;
                        icur2 = grid[3] - 1;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 4:
                    p[no++] = p3DData.pGridData[grid[4]];//4
                    p[no++] = p3DData.pGridData[grid[5]];//5
                    p[no++] = (p3DData.pGridData[grid[0]] + p3DData.pGridData[grid[1]]) / 2.0;
                    p[no++] = (p3DData.pGridData[grid[6]] + p3DData.pGridData[grid[7]]) / 2.0;
                    if (iz > 0)//z--
                    {
                        icur1 = grid[4] - xyGrid;
                        icur2 = grid[5] - xyGrid;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    if (iy < yGridNum - 2)
                    {
                        icur1 = grid[4] + xGridNum;
                        icur2 = grid[5] + xGridNum;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 5:
                    p[no++] = p3DData.pGridData[grid[5]];//5
                    p[no++] = p3DData.pGridData[grid[6]];//6
                    p[no++] = (p3DData.pGridData[grid[1]] + p3DData.pGridData[grid[2]]) / 2.0;
                    p[no++] = (p3DData.pGridData[grid[4]] + p3DData.pGridData[grid[7]]) / 2.0;
                    if (ix < xGridNum -2)//x++
                    {
                        icur1 = grid[5] + 1;
                        icur2 = grid[6] + 1;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    if (iy < yGridNum - 2)//y++
                    {
                        icur1 = grid[5] + xGridNum;
                        icur2 = grid[6] + xGridNum;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 6:
                    p[no++] = p3DData.pGridData[grid[6]];//6
                    p[no++] = p3DData.pGridData[grid[7]];//7
                    p[no++] = (p3DData.pGridData[grid[2]] + p3DData.pGridData[grid[3]]) / 2.0;
                    p[no++] = (p3DData.pGridData[grid[4]] + p3DData.pGridData[grid[5]]) / 2.0;
                    if (iy < yGridNum - 2)//y++
                    {
                        icur1 = grid[6] + xGridNum;
                        icur2 = grid[7] + xGridNum;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    if (iz < zGridNum - 2)//z++
                    {
                        icur1 = grid[6] + xyGrid;
                        icur2 = grid[7] + xyGrid;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 7:
                    p[no++] = p3DData.pGridData[grid[4]];//4
                    p[no++] = p3DData.pGridData[grid[7]];//7
                    p[no++] = (p3DData.pGridData[grid[0]] + p3DData.pGridData[grid[3]]) / 2.0;
                    p[no++] = (p3DData.pGridData[grid[6]] + p3DData.pGridData[grid[5]]) / 2.0;
                    if (iy < yGridNum - 2)//y++
                    {
                        icur1 = grid[4] + xGridNum;
                        icur2 = grid[7] + xGridNum;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix>0)//x--
                    {
                        icur1 = grid[4] - 1;
                        icur2 = grid[7] - 1;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 8:
                    p[no++] = p3DData.pGridData[grid[4]];//4
                    p[no++] = p3DData.pGridData[grid[7]];//0
                    p[no++] = (p3DData.pGridData[grid[1]] + p3DData.pGridData[grid[5]]) / 2.0;
                    p[no++] = (p3DData.pGridData[grid[3]] + p3DData.pGridData[grid[7]]) / 2.0;
                    if (iz >0)//z--
                    {
                        icur1 = grid[4] - xyGrid;
                        icur2 = grid[0] - xyGrid;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix > 0)//x--
                    {
                        icur1 = grid[4] - 1;
                        icur2 = grid[0] - 1;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 9:
                    p[no++] = p3DData.pGridData[grid[1]];//1
                    p[no++] = p3DData.pGridData[grid[5]];//5
                    p[no++] = (p3DData.pGridData[grid[0]] + p3DData.pGridData[grid[4]]) / 2.0;
                    p[no++] = (p3DData.pGridData[grid[6]] + p3DData.pGridData[grid[2]]) / 2.0;
                    if (iz > 0)//z--
                    {
                        icur1 = grid[1] - xyGrid;
                        icur2 = grid[5] - xyGrid;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix <xGridNum -2)//x++
                    {
                        icur1 = grid[1] + 1;
                        icur2 = grid[5] + 1;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 10:
                    p[no++] = p3DData.pGridData[grid[2]];//2
                    p[no++] = p3DData.pGridData[grid[6]];//6
                    p[no++] = (p3DData.pGridData[grid[1]] + p3DData.pGridData[grid[5]]) / 2.0;
                    p[no++] = (p3DData.pGridData[grid[3]] + p3DData.pGridData[grid[7]]) / 2.0;
                    if (iz <zGridNum-2)//z++
                    {
                        icur1 = grid[2] + xyGrid;
                        icur2 = grid[6] + xyGrid;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix < xGridNum - 2)//x++
                    {
                        icur1 = grid[2] + 1;
                        icur2 = grid[6] + 1;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    break;
                case 11:
                    p[no++] = p3DData.pGridData[grid[3]];//3
                    p[no++] = p3DData.pGridData[grid[7]];//7
                    p[no++] = (p3DData.pGridData[grid[0]] + p3DData.pGridData[grid[4]]) / 2.0;
                    p[no++] = (p3DData.pGridData[grid[6]] + p3DData.pGridData[grid[2]]) / 2.0;
                    if (iz < zGridNum - 2)//z++
                    {
                        icur1 = grid[3] + xyGrid;
                        icur2 = grid[7] + xyGrid;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
                        no++;
                    }
                    if (ix < xGridNum - 2)//x++
                    {
                        icur1 = grid[3] - 1;
                        icur2 = grid[7] - 1;
                        p[no] = (p3DData.pGridData[icur1] + p3DData.pGridData[icur2]) / 2.0;
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
        //Get edge coord according to the index
        private FLOAT_POINT_EXT GetEdgeCoord(int ix, int iy, int iz, int edno)
        {
            FLOAT_POINT_EXT p0 = new FLOAT_POINT_EXT();
            FLOAT_POINT_EXT p1 = new FLOAT_POINT_EXT();
            p0.x = (float)(xMin + ix * xGridStep);
            p0.y = (float)(yMin + iy * yGridStep);
            p0.z = (float)(zMin + iz * zGridStep);
            p1 = p0;            
            int icur = ix + iy * xGridNum + iz * xyGrid;
            int next = 0;
            switch(edno)
            {
                case 0://0-1
                    next = icur + 1;
                    p1.x = p0.x + (float)xDividedStep;
                    break;
                case 1: //1-2
                    icur = icur + 1;
                    next = icur + xyGrid;
                    p1.x = p0.x + (float)xGridStep;
                    p1.z = p0.z + (float)zDividedStep;
                    break;
                case 2: //2-3
                    icur = icur + 1 + xyGrid;
                    next = icur - 1;
                    p1.x = p0.x + (float)xDividedStep;
                    p1.z = p0.z + (float)zGridStep;
                    break;
                case 3: //0-3                    
                    next = icur + xyGrid;                    
                    p1.z = p0.z + (float)zDividedStep;
                    break;
                case 4://4-5
                    icur = icur + xGridNum;
                    next = icur + 1;
                    p1.y = p0.y + (float)yGridStep;
                    p1.x = p0.x + (float)xDividedStep;
                    break;
                case 5: //5-6
                    icur = icur + xGridNum + 1;
                    next = icur + xyGrid;
                    p1.y = p0.y + (float)yGridStep;
                    p1.x = p0.x + (float)xGridStep;
                    p1.z = p0.z + (float)zDividedStep;
                    break;
                case 6: //6-7
                    icur = icur + 1 + xyGrid + xGridNum;
                    next = icur - 1;
                    p1.y = p0.y + (float)yGridStep;
                    p1.x = p0.x + (float)xDividedStep;
                    p1.z = p0.z + (float)zGridStep;
                    break;
                case 7: //4-7
                    icur = icur + xGridNum;
                    next = icur + xyGrid;
                    p1.y = p0.y + (float)yGridStep;                    
                    p1.z = p0.z + (float)zDividedStep;
                    break;
                case 8: //0-4                    
                    next = icur + xGridNum;
                    p1.y = p0.y + (float)yDividedStep;
                    break;
                case 9: //1-5
                    icur = icur + 1;
                    next = icur + xGridNum;
                    p1.y = p0.y + (float)yDividedStep;
                    p1.x = p0.x + (float)xGridStep;
                    break;
                case 10: //2-6
                    icur = icur + 1 + xyGrid;
                    next = icur + xGridNum;
                    p1.y = p0.y + (float)yDividedStep;
                    p1.x = p0.x + (float)xGridStep;
                    p1.z = p0.z + (float)zGridStep;
                    break;
                case 11: //3-7
                    icur = icur + xyGrid;
                    next = icur + xGridNum;
                    p1.y = p0.y + (float)yDividedStep;                    
                    p1.z = p0.z + (float)zGridStep;
                    break;
            }
            p1.icolor = (short)GetColorIndex(GetEdgeValue(ix,iy,iz, edno));
            //p1.icolor = (short)GetColorIndex((p3DData.pGridData[icur] + p3DData.pGridData[next]) / 2);
            return p1;
        }
        private void InitEdgePointArray()
        {
            pEdgePointArray.Clear();
            for (int i = 0; i < 2*xyGrid; i++)
            {
                EDGE_POINT_INDEX p = new EDGE_POINT_INDEX();
                pEdgePointArray.Add(p);
            }
        }
        private void ResetEdgePointArray()
        {
            for (int i = 0; i < pEdgePointArray.Count; i++)
            {
                pEdgePointArray[i].Reset();
            }
        }
        //返回编号>=0,否则没有返回-1，
        private int GetEdgeIndex(int icur,int edge)
        {
            return pEdgePointArray[icur].pIndex[edge];
        }
        private void StoreEdgeIndex(int cur, int edge, int id)
        {
            // store to current cube
            pEdgePointArray[cur].pIndex[edge] = id;
        }
        private void StoreVerticIndex(int cur, int ver, int id)
        {
            // store to current cube ,12-19 
            pEdgePointArray[cur].pIndex[12+ ver] = id;
        }
        private void StoreVerticIndex(int ix, int iy, int iz, int ver, int id)
        {
            //current cube index
            int icur = ix + iy * xGridNum + iz * xyGrid;
            // store to current cube first
            pEdgePointArray[icur].pIndex[12+ver] = id;
            return;
        }
        private void StoreEdgeIndex(int ix, int iy, int iz, int edge,int id)
        {
            //current cube index
            int icur = ix + iy * xGridNum + iz * xyGrid;
            // store to current cube first
            pEdgePointArray[icur].pIndex[edge] = id;           
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
            int icur = ix + iy * xGridNum + iz * xyGrid;
            int iprev;
            if (  iz > 0 )
            {
                //0145 - 3276
                iprev = icur - xyGrid;
                if( ver == 0 )
                {
                    if ( pEdgePointArray[iprev].pIndex[12 + 3] >= 0 )
                        return pEdgePointArray[iprev].pIndex[12 + 3];
                }
                if (ver == 1)
                {
                    if (pEdgePointArray[iprev].pIndex[12 + 2] >= 0)
                        return pEdgePointArray[iprev].pIndex[12 + 2];
                }
                if (ver == 4)
                {
                    if (pEdgePointArray[iprev].pIndex[12 + 7] >= 0)
                        return pEdgePointArray[iprev].pIndex[12 + 7];
                }
                if (ver == 5)
                {
                    if (pEdgePointArray[iprev].pIndex[12 + 6] >= 0)
                        return pEdgePointArray[iprev].pIndex[12 + 6];
                }
            }
            if (iy > 0)
            {
                //0123 - 4567
                iprev = icur - xGridNum;
                if (ver == 0)
                {
                    if (pEdgePointArray[iprev].pIndex[12 + 4] >= 0)
                        return pEdgePointArray[iprev].pIndex[12 + 4];
                }
                if (ver == 1)
                {
                    if (pEdgePointArray[iprev].pIndex[12 + 5] >= 0)
                        return pEdgePointArray[iprev].pIndex[12 + 5];
                }
                if (ver == 2)
                {
                    if (pEdgePointArray[iprev].pIndex[12 + 6] >= 0)
                        return pEdgePointArray[iprev].pIndex[12 + 6];
                }
                if (ver == 3)
                {
                    if (pEdgePointArray[iprev].pIndex[12 + 7] >= 0)
                        return pEdgePointArray[iprev].pIndex[12 + 7];
                }
            }
            if (ix > 0)
            {
                //0374 - 1265
                iprev = icur - 1;
                if (ver == 0)
                {
                    if (pEdgePointArray[iprev].pIndex[12 + 1] >= 0)
                        return pEdgePointArray[iprev].pIndex[12 + 1];
                }
                if (ver == 3)
                {
                    if (pEdgePointArray[iprev].pIndex[12 + 2] >= 0)
                        return pEdgePointArray[iprev].pIndex[12 + 2];
                }
                if (ver == 7)
                {
                    if (pEdgePointArray[iprev].pIndex[12 + 6] >= 0)
                        return pEdgePointArray[iprev].pIndex[12 + 6];
                }
                if (ver == 4)
                {
                    if (pEdgePointArray[iprev].pIndex[12 + 5] >= 0)
                        return pEdgePointArray[iprev].pIndex[12 + 5];
                }
            }
            return -1;
        }
        private int GetStoredEdgeIndex(int ix,int iy,int iz,int edge)
        {
            //     |z
            //     o---4----o    
            //   7/|8      /|
            //  o----6---o  |9
            //11|  o---0-|--o--->y
            //  |3/      | /1
            //  o----2---o
            //  /x  c     
            byte[] xedge= new byte[4] { 3,7,8,11};
            byte[] xedge1 = new byte[4] { 1, 5, 9, 10 };
            byte[] yedge = new byte[4] { 0, 1, 2, 3 };
            byte[] yedge1 = new byte[4] { 4, 5, 6, 7 };
            byte[] zedge = new byte[4] { 0, 4, 8, 9 };
            byte[] zedge1 = new byte[4] { 2, 6, 11, 10 };
            // current cube index
            int icur = ix + iy * xGridNum + iz * xyGrid;
            int iprev;
            if (ix > 0)
            {
                //37811-15910
                iprev = (ix - 1) + iy * xGridNum + iz * xyGrid;
                for (int i = 0; i < 4; i++)
                {
                    if (edge == xedge[i])
                    {
                        if (pEdgePointArray[iprev].pIndex[xedge1[i]] >= 0)
                        return pEdgePointArray[iprev].pIndex[xedge1[i]];
                    }
                }
            }
            if (iy > 0)
            {
                //37811-15910
                iprev = ix + (iy-1) * xGridNum + iz * xyGrid;
                for (int i = 0; i < 4; i++)
                {
                    if (edge == yedge[i])
                    {
                        if (pEdgePointArray[iprev].pIndex[yedge1[i]] >= 0)
                            return pEdgePointArray[iprev].pIndex[yedge1[i]];
                    }
                }
            }
            if (iz > 0)
            {
                //37811-15910
                iprev = ix + iy * xGridNum + (iz-1) * xyGrid;
                for (int i = 0; i < 4; i++)
                {
                    if (edge == zedge[i])
                    {
                        if (pEdgePointArray[iprev].pIndex[zedge1[i]] >= 0)
                            return pEdgePointArray[iprev].pIndex[zedge1[i]];
                    }
                }
            }
            return -1;
            // previous cube index

            switch (edge)
            {
                case 0:
                    if (iy > 0) //0 <-- 4
                    {
                        iprev = icur - xGridNum;
                        if (pEdgePointArray[iprev].pIndex[4] >= 0)
                            return pEdgePointArray[iprev].pIndex[4];
                    }
                    if (iz > 0) //0 <-- 2
                    {
                        iprev = icur - xyGrid;
                        if (pEdgePointArray[iprev].pIndex[2] >= 0)
                            return pEdgePointArray[iprev].pIndex[2];
                    }                    
                    break;               
                case 3:
                    if( ix>0 )  //3-1
                    {
                        iprev = icur - 1;
                        if (pEdgePointArray[iprev].pIndex[1] >= 0)
                            return pEdgePointArray[iprev].pIndex[1];
                    }
                    if (iy > 0) //3-7
                    {
                        iprev = icur - xGridNum;
                        if(pEdgePointArray[iprev].pIndex[7] >=0 )
                            return pEdgePointArray[iprev].pIndex[7];
                    }
                    break;
                case 8:
                    if (ix > 0)  //8  -9
                    {
                        iprev = icur - 1;
                        if (pEdgePointArray[iprev].pIndex[9] >= 0)
                            return pEdgePointArray[iprev].pIndex[9];
                    }
                    if (iz > 0) //8-11
                    {
                        iprev = icur - xyGrid;
                        if (pEdgePointArray[iprev].pIndex[11] >= 0)
                            return pEdgePointArray[iprev].pIndex[11];
                    }
                    break;
                case 7:
                    if (ix > 0)//x:37811,x-1:15910
                    {
                        iprev = icur - 1;
                        if( pEdgePointArray[iprev].pIndex[5] >=0 )
                           return pEdgePointArray[iprev].pIndex[5];
                    }
                    break;
                case 11:                
                    if (ix > 0)//x:37811,x-1:15910
                    {
                        iprev = icur - 1;
                        if (pEdgePointArray[iprev].pIndex[10] >= 0)
                            return pEdgePointArray[iprev].pIndex[10];
                    }
                    break;                
                case 1:
                    if (iy > 0)//y:0123, y-1:4567
                    {
                        iprev = icur - xGridNum;
                        if (pEdgePointArray[iprev].pIndex[5] >= 0)
                            return pEdgePointArray[iprev].pIndex[5];
                    }
                    break;
                case 2:                
                    if (iy > 0)//y:0123, y-1:4567
                    {
                        iprev = icur - xGridNum;
                        if (pEdgePointArray[iprev].pIndex[6] >= 0)
                            return pEdgePointArray[iprev].pIndex[6];
                    }
                    break;
                //z:261011,z-1:0498
                case 4:
                    if (iz > 0)
                    {
                        iprev = icur - xyGrid;
                        if (pEdgePointArray[iprev].pIndex[6] >= 0)
                            return pEdgePointArray[iprev].pIndex[6];                        
                    }
                    break;
                case 9:                
                    if (iz > 0)
                    {
                        iprev = icur - xyGrid;
                        if (pEdgePointArray[iprev].pIndex[10] >= 0)
                            return pEdgePointArray[iprev].pIndex[10];
                    }
                    break;
            }            
            return -1;
        }
        private int GetCoordSize()
        {
            return pISOSurfaceExt.pCoordArray.Count;
        }        
        private void AddCoord(FLOAT_POINT_EXT p)
        {
            pISOSurfaceExt.AddCoord(p);
        }
        private void InitTriangleTable()
        {
            
      //   /*
            triTable = new int[256, 24]
            {
        {-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,8,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,8,3,1,9,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,10,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,3,2,2,10,8,10,1,0,0,8,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,9,0,2,10,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,3,2,8,2,10,9,8,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,3,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,11,2,0,8,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,2,1,1,9,11,9,0,3,3,11,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,2,1,11,1,9,8,11,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,3,11,10,1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,1,0,10,0,8,11,10,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,0,3,9,3,11,10,9,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,10,9,9,8,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,7,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,3,0,4,7,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,8,0,0,1,7,1,9,4,4,7,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,9,4,1,4,7,3,1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,8,2,7,2,10,4,7,10,1,4,10,4,1,8,8,1,2,-1,-1,-1,-1,-1,-1},
{0,4,1,10,1,4,10,4,7,2,10,7,3,2,7,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,2,8,7,8,2,7,2,10,4,7,10,9,4,10,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,3,2,10,9,3,9,7,3,9,4,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,3,8,8,4,2,4,7,11,11,2,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,7,11,4,11,2,0,4,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,9,0,1,11,2,1,7,11,9,4,7,1,9,7,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,7,11,9,4,11,9,11,2,9,2,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,10,7,4,7,10,4,10,1,8,4,1,3,8,1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,10,7,7,10,4,4,10,1,1,0,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,3,8,11,10,7,7,10,4,10,9,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,7,11,9,4,11,9,11,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,9,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,0,9,9,5,3,5,4,8,8,3,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,5,4,0,1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,4,8,5,8,3,1,5,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,9,1,1,2,4,2,10,5,5,4,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,3,0,8,5,4,8,10,5,3,2,10,8,3,10,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,10,5,2,5,4,0,2,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,10,5,3,2,5,3,5,4,3,4,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,11,4,3,4,9,2,3,9,5,2,9,2,5,11,11,5,4,-1,-1,-1,-1,-1,-1},
{8,11,4,5,4,11,5,11,2,9,5,2,0,9,2,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,5,2,11,2,5,11,5,4,3,11,4,0,3,4,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,5,2,2,5,11,11,5,4,4,8,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,3,9,4,9,3,4,3,11,5,4,11,10,5,11,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,0,9,8,11,4,4,11,5,11,10,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,10,5,4,0,10,0,11,10,0,3,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,4,8,10,5,8,10,8,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,7,8,9,5,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,0,9,3,9,5,7,3,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,8,0,7,0,1,5,7,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,1,5,5,7,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,7,10,2,10,7,2,7,8,1,2,8,9,1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,9,1,5,7,10,10,7,2,7,3,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,7,10,10,7,2,2,7,8,8,0,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,10,5,3,2,5,3,5,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,9,3,2,3,9,2,9,5,11,2,5,7,11,5,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,7,11,2,0,7,0,5,7,0,9,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,0,3,1,5,2,2,5,11,5,7,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,2,1,7,11,1,7,1,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,7,11,11,10,5,8,9,1,1,3,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,11,10,11,1,0,7,11,0,7,0,9,7,9,5,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,5,7,5,8,0,10,5,0,10,0,3,10,3,11,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,11,5,11,10,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,5,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,3,6,8,6,5,0,8,5,10,0,5,0,10,3,3,10,6,-1,-1,-1,-1,-1,-1},
{0,1,10,10,6,0,6,5,9,9,0,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,8,5,6,5,8,6,8,3,10,6,3,1,10,3,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,1,2,6,5,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,6,3,8,3,6,8,6,5,0,8,5,1,0,5,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,5,9,6,9,0,2,6,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,8,5,5,8,6,6,8,3,3,2,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,11,6,6,5,3,5,10,2,2,3,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,0,10,5,10,0,5,0,8,6,5,8,11,6,8,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,11,2,3,9,0,3,5,9,11,6,5,3,11,5,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,1,10,9,8,5,5,8,6,8,11,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,11,6,3,6,5,1,3,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,11,6,5,1,11,1,8,11,1,0,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,5,9,11,6,9,11,9,0,11,0,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,5,9,11,6,9,11,9,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,4,5,5,10,8,10,6,7,7,8,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,3,6,10,6,3,10,3,0,5,10,0,4,5,0,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,8,4,7,10,6,7,1,10,8,0,1,7,8,1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,4,5,7,3,6,6,3,10,3,1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,1,4,8,4,1,8,1,2,7,8,2,6,7,2,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,4,5,5,1,0,7,3,2,2,6,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,9,4,0,2,8,8,2,7,2,6,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,2,6,2,5,9,3,2,9,3,9,4,3,4,7,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,10,6,5,8,4,5,3,8,10,2,3,5,10,3,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,11,6,2,0,10,10,0,5,0,4,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,0,3,2,1,10,4,5,9,6,7,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,7,11,2,1,11,1,9,11,9,4,7,9,7,11,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,6,7,5,1,4,4,1,8,1,3,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,0,4,0,7,11,1,0,11,1,11,6,1,6,5,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,5,9,0,3,9,3,11,9,11,6,5,11,5,9,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,7,11,11,9,4,6,5,9,9,11,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,10,6,4,9,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,10,0,3,0,10,3,10,6,8,3,6,4,8,6,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,1,10,0,10,6,4,0,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,1,10,6,4,1,4,3,1,4,8,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,9,1,4,1,2,6,4,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,1,0,2,6,3,3,6,8,6,4,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,6,4,4,0,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,3,2,4,8,2,4,2,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,4,11,3,11,4,3,4,9,2,3,9,10,2,9,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,0,9,9,10,2,8,11,6,6,4,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,10,2,6,4,11,11,4,3,4,0,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,8,11,8,2,1,4,8,1,4,1,10,4,10,6,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,3,9,9,3,4,4,3,11,11,6,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,6,4,6,9,1,11,6,1,11,1,0,11,0,8,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,11,6,0,3,6,0,6,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,8,6,8,11,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,6,7,10,7,8,9,10,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,10,0,0,10,3,3,10,6,6,7,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,1,10,8,0,10,8,10,6,8,6,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,6,7,1,10,7,1,7,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,9,1,2,6,9,6,8,9,6,7,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,7,3,7,0,9,6,7,9,6,9,1,6,1,2,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,8,0,6,7,0,6,0,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,7,2,7,3,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,7,11,8,9,3,3,9,2,9,10,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,9,10,9,6,7,0,9,7,0,7,11,0,11,2,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,1,10,6,7,10,7,8,10,8,0,1,8,1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,6,7,7,1,10,11,2,1,1,7,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,1,3,1,11,6,9,1,6,9,6,7,9,7,8,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,7,11,1,0,11,1,11,6,1,6,9,7,9,6,0,9,7,-1,-1,-1,-1,-1,-1},
{7,8,0,0,6,7,3,11,6,6,0,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,11,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,11,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,8,7,7,6,0,6,11,3,3,0,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,7,0,11,0,1,6,11,1,9,6,1,6,9,7,7,9,0,-1,-1,-1,-1,-1,-1},
{3,1,11,6,11,1,6,1,9,7,6,9,8,7,9,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,6,10,10,1,7,1,2,11,11,7,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,1,2,10,7,6,10,8,7,1,0,8,10,1,8,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,9,6,7,6,9,7,9,0,11,7,0,2,11,0,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,2,11,10,9,6,6,9,7,9,8,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,7,6,2,3,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,8,7,0,7,6,2,0,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,7,0,9,0,7,9,7,6,1,9,6,2,1,6,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,8,7,6,2,8,2,9,8,2,1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,6,10,7,10,1,3,7,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,8,7,1,0,7,1,7,6,1,6,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,9,6,6,9,7,7,9,0,0,3,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,6,10,8,7,10,8,10,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,8,4,6,11,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,11,3,6,3,0,4,6,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,6,9,1,9,6,1,6,11,0,1,11,8,0,11,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,1,11,11,1,6,6,1,9,9,4,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,8,2,1,2,8,1,8,4,10,1,4,6,10,4,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,3,2,0,4,1,1,4,10,4,6,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,6,10,10,9,4,11,8,0,0,2,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,4,6,4,11,3,9,4,3,9,3,2,9,2,10,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,3,8,2,8,4,6,2,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,4,6,6,2,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,8,0,4,6,9,9,6,1,6,2,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,9,4,2,1,4,2,4,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,3,8,4,6,3,6,1,3,6,10,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,1,0,6,10,0,6,0,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,10,9,10,0,3,6,10,3,6,3,8,6,8,4,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,10,4,10,9,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,7,4,4,9,11,9,5,6,6,11,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,5,4,9,3,0,9,11,3,5,6,11,9,5,11,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,0,7,11,7,0,11,0,1,6,11,1,5,6,1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,8,7,3,1,11,11,1,6,1,5,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,7,6,11,1,2,11,9,1,7,4,9,11,7,9,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,4,8,0,9,1,6,10,5,2,11,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,5,6,4,0,7,7,0,11,0,2,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,10,5,4,8,5,8,3,5,3,2,10,3,10,5,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,2,5,9,5,2,9,2,3,4,9,3,7,4,3,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,7,4,6,2,5,5,2,9,2,0,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,2,1,1,5,6,3,7,4,4,0,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,1,5,1,4,8,2,1,8,2,8,7,2,7,6,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,10,5,1,3,9,9,3,4,3,7,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,8,7,6,10,7,10,1,7,1,0,8,1,8,7,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,3,7,3,6,10,0,3,10,0,10,5,0,5,4,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,4,8,8,10,5,7,6,10,10,8,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,5,6,9,6,11,8,9,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,11,3,5,6,3,5,3,0,5,0,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,8,0,1,5,8,5,11,8,5,6,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,11,3,5,6,3,5,3,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,6,10,11,8,2,2,8,1,8,9,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,11,3,0,9,3,9,5,3,5,6,11,5,11,3,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,0,2,0,10,5,8,0,5,8,5,6,8,6,11,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,11,3,3,5,6,2,10,5,5,3,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,9,3,3,9,2,2,9,5,5,6,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,5,6,0,9,6,0,6,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,6,2,6,3,8,5,6,8,5,8,0,5,0,1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,1,6,1,5,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,8,9,8,5,6,3,8,6,3,6,10,3,10,1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,1,0,0,6,10,9,5,6,6,0,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{6,3,8,5,6,8,5,8,0,5,0,10,3,10,0,6,10,3,-1,-1,-1,-1,-1,-1},
{10,5,6,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,11,7,5,10,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,5,8,0,8,5,0,5,10,3,0,10,11,3,10,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,11,1,0,1,11,0,11,7,9,0,7,5,9,7,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,11,3,3,1,10,7,5,9,9,8,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,2,11,1,11,7,5,1,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,11,3,7,5,8,8,5,0,5,1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,5,9,0,2,5,2,7,5,2,11,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,9,8,9,3,2,5,9,2,5,2,11,5,11,7,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,10,2,5,2,3,7,5,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,5,8,8,5,0,0,5,10,10,2,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,2,1,3,7,0,0,7,9,7,5,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,7,5,7,10,2,8,7,2,8,2,1,8,1,9,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,5,1,1,3,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,8,7,1,0,7,1,7,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,0,3,5,9,3,5,3,7,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,7,9,7,5,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,4,5,8,5,10,11,8,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,11,3,0,4,11,4,10,11,4,5,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,5,9,10,11,1,1,11,0,11,8,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,3,1,3,9,4,11,3,4,11,4,5,11,5,10,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,1,4,4,1,8,8,1,2,2,11,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,5,1,5,2,11,4,5,11,4,11,3,4,3,0,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,11,8,11,4,5,2,11,5,2,5,9,2,9,0,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,11,3,9,4,3,9,3,2,9,2,5,11,5,2,4,5,11,-1,-1,-1,-1,-1,-1},
{2,3,8,10,2,8,10,8,4,10,4,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,10,2,4,5,2,4,2,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,3,8,4,5,8,5,10,8,10,2,3,10,3,8,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,9,4,4,2,1,5,10,2,2,4,5,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,4,5,3,8,5,3,5,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,5,0,5,1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,0,3,3,5,9,8,4,5,5,3,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{5,9,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,7,4,11,4,9,10,11,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,4,8,9,10,0,0,10,3,10,11,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{10,11,1,1,11,0,0,11,7,7,4,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{7,10,11,10,7,4,1,10,4,1,4,8,1,8,3,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{4,9,1,7,4,1,7,1,2,7,2,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,9,1,2,11,1,11,7,1,7,4,9,7,9,1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,7,4,2,11,4,2,4,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,3,2,2,4,8,11,7,4,4,2,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,7,4,9,10,7,10,3,7,10,2,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,2,0,2,8,7,10,2,7,10,7,4,10,4,9,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,4,0,4,1,10,7,4,10,7,10,2,7,2,3,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,8,7,10,2,7,10,7,4,10,4,1,8,1,4,2,1,8,-1,-1,-1,-1,-1,-1},
{4,9,1,7,4,1,7,1,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,8,7,7,1,0,4,9,1,1,7,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,3,4,3,7,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,7,4,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{8,9,10,10,11,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,0,9,11,3,9,11,9,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,1,10,8,0,10,8,10,11,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,3,10,3,1,10,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,2,11,9,1,11,9,11,8,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,2,11,11,9,1,3,0,9,9,11,3,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,11,0,11,8,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{11,3,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,3,8,10,2,8,10,8,9,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{0,9,2,9,10,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{2,3,8,8,10,2,0,1,10,10,8,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{1,10,2,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,8,1,8,9,1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{9,1,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{3,8,0,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1},
            };
      
        }
        public int GetColorIndex(double v)
        {
            return m_ColorScale.GetColorIndex(v);
        }
        public void SetData(C3DGridData pdata)
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
            pISOSurfaceExt.pColor.Clear();

            if (pdata.m_ColorScale != null )
                m_ColorScale = pdata.m_ColorScale;
            else
            {
                m_ColorScale = new CColorScale();
                m_ColorScale.SetValueRange(vMin, vMax);
             }            
            for (int i=0;i<pdata.m_ColorScale.nColorNum;i++)
            {
                pISOSurfaceExt.AddColor(m_ColorScale.GetColor(i));
            }
        }
        public MarchingCubesExt()
        {
            InitTriangleTable();
        }
        public override void Clear()
        {
            pISOSurfaceExt.Clear();
            pEdgePointArray.Clear();
            p3DData = null;
        }
        public void ExtractTriangleFromCubeTest(int ix, int iy, int iz, int typeIndex)
        {
            pISOSurfaceExt.Clear();
            pEdgePointArray.Clear();
            for (int i = 0; i < zGridNum * xyGrid; i++)
            {
                EDGE_POINT_INDEX p = new EDGE_POINT_INDEX(i);
                pEdgePointArray.Add(p);
            }
            ExtractTriangleFromGrid(ix, iy, iz, typeIndex);
            CreateFromSurface(ix, iy, iz, typeIndex);
            pEdgePointArray.Clear();
        }        
        private int ExtractTriangleFromGrid(int ix, int iy, int iz, int typeIndex)
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
            FLOAT_POINT_EXT p0;
            p0.x = (float)(xMin + ix * xGridStep);
            p0.y = (float)(yMin + iy * yGridStep);
            p0.z = (float)(zMin + iz * zGridStep);
            p0.icolor = 0;
            int icur = ix + iy * xGridNum + iz * xyGrid;
            //vertexs index number
            int[] pGridIndex = new int[8];
            pGridIndex[0] = icur;
            pGridIndex[1] = (ix+1) + iy * xGridNum + iz * xyGrid;
            pGridIndex[2] = (ix+1) + iy * xGridNum + (iz+1) * xyGrid;
            pGridIndex[3] = ix + iy * xGridNum + (iz+1) * xyGrid;
            pGridIndex[4] = pGridIndex[0] + xGridNum;
            pGridIndex[5] = pGridIndex[1] + xGridNum;
            pGridIndex[6] = pGridIndex[2] + xGridNum;
            pGridIndex[7] = pGridIndex[3] + xGridNum;
            //8 vertexs coords
            Vector32[] Vect = new Vector32[8];
            Vect[0] = new Vector32(p0.x, p0.y, p0.z);
            Vect[1] = new Vector32(p0.x + (float)xGridStep, p0.y, p0.z);
            Vect[2] = new Vector32(p0.x + (float)xGridStep, p0.y, p0.z+ (float)zGridStep);
            Vect[3] = new Vector32(p0.x, p0.y, p0.z+ (float)zGridStep);
            Vect[4] = Vect[0];  Vect[4].Y+= (float)yGridStep;
            Vect[5] = Vect[1]; Vect[5].Y += (float)yGridStep;
            Vect[6] = Vect[2]; Vect[6].Y += (float)yGridStep;
            Vect[7] = Vect[3]; Vect[7].Y += (float)yGridStep;
            //12 edges middle points coords
            Vector32[] Edge = new Vector32[12];
            Edge[0] = (Vect[0] + Vect[1]) / 2;
            Edge[1] = (Vect[1] + Vect[2]) / 2;
            Edge[2] = (Vect[2] + Vect[3]) / 2;
            Edge[3] = (Vect[0] + Vect[3]) / 2;
            Edge[4] = (Vect[4] + Vect[5]) / 2;
            Edge[5] = (Vect[5] + Vect[6]) / 2;
            Edge[6] = (Vect[6] + Vect[7]) / 2;
            Edge[7] = (Vect[4] + Vect[7]) / 2;
            Edge[8] = (Vect[0] + Vect[4]) / 2;
            Edge[9] = (Vect[1] + Vect[5]) / 2;
            Edge[10] = (Vect[2] + Vect[6]) / 2;
            Edge[11] = (Vect[3] + Vect[7]) / 2;
            //12 edges middle points coords color index
            int[] colorIndex = new int[12];
            for (int i = 0; i < 12; i++)
            colorIndex[i] = GetColorIndex(GetEdgeValue(ix, iy, iz, i));
            //processed
            List<EDGE_INDEX_PROCESS> edges = new List<EDGE_INDEX_PROCESS>();
            // searching the edge table,till -1 end
            for (int i = 0; ; i++)
            {
                edno = triTable[typeIndex, i];
                if (edno < 0) break;

                // check if the same edge processed before in the same cube
                bool dealed = false;
                for (int j=0;j< edges.Count; j++)
                {
                    if( edges[j].edno == edno )
                    {
                        pISOSurfaceExt.pTriangleIndex.Add(edges[j].index);
                        dealed = true;
                        break;
                    }
                }
                if (dealed) continue;

                // check if the same edge coords been stored
                edIndex = GetStoredEdgeIndex(ix, iy, iz, edno);
                if (edIndex >= 0)
                {
                    StoreEdgeIndex(ix, iy, iz, edno, edIndex);
                    pISOSurfaceExt.pTriangleIndex.Add(edIndex);
                    EDGE_INDEX_PROCESS ed = new EDGE_INDEX_PROCESS(edno,edIndex);
                    edges.Add(ed);
                    continue;
                }
                
                //else if no stored create new
                FLOAT_POINT_EXT p = new FLOAT_POINT_EXT();
                p.x = Edge[edno].X;
                p.y = Edge[edno].Y;
                p.z = Edge[edno].Z;
                p.icolor = (Int16)colorIndex[edno];
                
                pISOSurfaceExt.pTriangleIndex.Add(GetCoordSize());
                StoreEdgeIndex(ix, iy, iz, edno, GetCoordSize());
                EDGE_INDEX_PROCESS ed1 = new EDGE_INDEX_PROCESS(edno, GetCoordSize());
                edges.Add(ed1);
                AddCoord(p);
            }//for
            return 1;
        }
        //bVertics[] 4 vertics show state,vertics[],4 Edges[](0-11)
        private int ExtractTriangleFromFace(int ix, int iy, int iz, bool[] bVertics, int[]Vertics,int[] Edges)
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
            int i1, i2, i3,i11,i12,i21,i22,i31,i32;
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
                            AddCoord(p);
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
                            p = GetEdgeCoord(ix, iy, iz, Relation[i].edge2);                            
                            StoreEdgeIndex(ix, iy, iz, Relation[i].edge2, GetCoordSize());
                            AddCoord(p);
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
                            p = GetEdgeCoord(ix, iy, iz, Relation[i].edge1);                            
                            StoreEdgeIndex(ix, iy, iz, Relation[i].edge1, GetCoordSize());
                            AddCoord(p);
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
                            AddCoord(p);
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
                            p = GetEdgeCoord(ix, iy, iz, Relation[n1].edge1);                            
                            StoreEdgeIndex(ix, iy, iz, Relation[n1].edge1, GetCoordSize());
                            AddCoord(p);
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
                            AddCoord(p);
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
                            p = GetEdgeCoord(ix, iy, iz, Relation[n2].edge2);                            
                            StoreEdgeIndex(ix, iy, iz, Relation[n2].edge2, GetCoordSize());
                            AddCoord(p);
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
                            AddCoord(p);
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
                            AddCoord(p);
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
                            p = GetEdgeCoord(ix, iy, iz, Relation[n1].edge1);
                            StoreEdgeIndex(ix, iy, iz, Relation[n1].edge1, GetCoordSize());
                            AddCoord(p);
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
                            p = GetEdgeCoord(ix, iy, iz, Relation[n1].edge2);
                            StoreEdgeIndex(ix, iy, iz, Relation[n1].edge2, GetCoordSize());
                            AddCoord(p);
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
                            p = GetEdgeCoord(ix, iy, iz, Relation[n2].edge1);
                            StoreEdgeIndex(ix, iy, iz, Relation[n2].edge1, GetCoordSize());
                            AddCoord(p);
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
                            p = GetEdgeCoord(ix, iy, iz, Relation[n2].edge2);
                            StoreEdgeIndex(ix, iy, iz, Relation[n2].edge2, GetCoordSize());
                            AddCoord(p);
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
                        AddCoord(p);
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
                        AddCoord(p);
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
                        AddCoord(p);
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
                        p = GetEdgeCoord(ix, iy, iz, Relation[n1].edge1);
                        StoreEdgeIndex(ix, iy, iz, Relation[n1].edge1, GetCoordSize());
                        AddCoord(p);
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
                        p = GetEdgeCoord(ix, iy, iz, Relation[n3].edge2);
                        StoreEdgeIndex(ix, iy, iz, Relation[n3].edge2, GetCoordSize());
                        AddCoord(p);
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
                            AddCoord(p);
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

            uint icur = 0, icur1 = 0;
            int iType = 0;
            int ix, iy, iz;
            Int32XYZ[] pGridIndex = new Int32XYZ[8];
            bool[] pShow = new bool[8];

            pISOSurfaceExt.Clear();            
            InitEdgePointArray();
            for ( iz = 0; iz<zGridNum-1;iz++)
            {
                ResetEdgePointArray();
                for (iy = 0; iy < yGridNum - 1; iy++)
                    for (ix = 0; ix < xGridNum - 1; ix++)
                    {
                        icur = (uint)(ix + iy * xGridNum + iz * xGridNum * yGridNum);

                        pGridIndex[0].x = ix; pGridIndex[0].y = iy; pGridIndex[0].z = iz;
                        pGridIndex[1].x = ix + 1; pGridIndex[1].y = iy; pGridIndex[1].z = iz;
                        pGridIndex[2].x = ix + 1; pGridIndex[2].y = iy; pGridIndex[2].z = iz + 1;
                        pGridIndex[3].x = ix; pGridIndex[3].y = iy; pGridIndex[3].z = iz + 1;
                        pGridIndex[4].x = ix; pGridIndex[4].y = iy + 1; pGridIndex[4].z = iz;
                        pGridIndex[5].x = ix + 1; pGridIndex[5].y = iy + 1; pGridIndex[5].z = iz;
                        pGridIndex[6].x = ix + 1; pGridIndex[6].y = iy + 1; pGridIndex[6].z = iz + 1;
                        pGridIndex[7].x = ix; pGridIndex[7].y = iy + 1; pGridIndex[7].z = iz + 1;

                        for (int k = 0; k < 8; k++)
                            pShow[k] = p3DData.GetShowState((uint)pGridIndex[k].x, (uint)pGridIndex[k].y, (uint)pGridIndex[k].z);

                        iType = 0;
                        if (pShow[0]) iType |= 1;
                        if (pShow[1]) iType |= 2;
                        if (pShow[2]) iType |= 4;
                        if (pShow[3]) iType |= 8;
                        if (pShow[4]) iType |= 16;
                        if (pShow[5]) iType |= 32;
                        if (pShow[6]) iType |= 64;
                        if (pShow[7]) iType |= 128;

                        if (edgeTable[iType] != 0)
                        {
                            ExtractTriangleFromGrid(ix, iy, iz, iType);
                        }
                        CreateFromSurface(ix, iy, iz, iType);
                    }//for (iy = 0; iy < yGridNum - 1; iy++)
            }//for( iz = 0; iz < zGridNum - 1; iz++ )
            pEdgePointArray.Clear();            
            return 1;
        }
        private void CreateFromSurface(int ix,int iy,int iz,int iType)
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
            
            if (!bx1)
            {
                bx1 |= p3DData.GetShowState(GetVerticIndex(ix - 1, iy, iz, 3));
                bx1 |= p3DData.GetShowState(GetVerticIndex(ix - 1, iy, iz, 7));
                bx1 |= p3DData.GetShowState(GetVerticIndex(ix - 1, iy, iz, 4));
                bx1 |= p3DData.GetShowState(GetVerticIndex(ix - 1, iy, iz, 0));
                bx1 = !bx1;
            }
            if (!bx2)
            {
                bx2 |= p3DData.GetShowState(GetVerticIndex(ix + 1, iy, iz, 1));
                bx2 |= p3DData.GetShowState(GetVerticIndex(ix + 1, iy, iz, 5));
                bx2 |= p3DData.GetShowState(GetVerticIndex(ix + 1, iy, iz, 6));
                bx2 |= p3DData.GetShowState(GetVerticIndex(ix + 1, iy, iz, 2));
                bx2 = !bx2;
            }

            if (!by1)
            {
                by1 |= p3DData.GetShowState(GetVerticIndex(ix, iy - 1, iz, 0));
                by1 |= p3DData.GetShowState(GetVerticIndex(ix, iy - 1, iz, 1));
                by1 |= p3DData.GetShowState(GetVerticIndex(ix, iy - 1, iz, 2));
                by1 |= p3DData.GetShowState(GetVerticIndex(ix, iy - 1, iz, 3));
                by1 = !by1;
            }
            if (!by2)
            {
                by2 |= p3DData.GetShowState(GetVerticIndex(ix, iy + 1, iz, 4));
                by2 |= p3DData.GetShowState(GetVerticIndex(ix, iy + 1, iz, 5));
                by2 |= p3DData.GetShowState(GetVerticIndex(ix, iy + 1, iz, 6));
                by2 |= p3DData.GetShowState(GetVerticIndex(ix, iy + 1, iz, 7));
                by2 = !by2;
            }

            if (!bz1)
            {
                bz1 |= p3DData.GetShowState(GetVerticIndex(ix, iy, iz - 1, 0));
                bz1 |= p3DData.GetShowState(GetVerticIndex(ix, iy, iz - 1, 4));
                bz1 |= p3DData.GetShowState(GetVerticIndex(ix, iy, iz - 1, 5));
                bz1 |= p3DData.GetShowState(GetVerticIndex(ix, iy, iz - 1, 1));
                bz1 = !bz1;
            }
            if (!bz2)
            {
                bz2 |= p3DData.GetShowState(GetVerticIndex(ix, iy, iz + 1, 3));
                bz2 |= p3DData.GetShowState(GetVerticIndex(ix, iy, iz + 1, 7));
                bz2 |= p3DData.GetShowState(GetVerticIndex(ix, iy, iz + 1, 6));
                bz2 |= p3DData.GetShowState(GetVerticIndex(ix, iy, iz + 1, 2));
                bz2 = !bz2;
            }

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
}
