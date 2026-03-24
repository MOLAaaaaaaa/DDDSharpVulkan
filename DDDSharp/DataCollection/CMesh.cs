using GlmNet;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TextReaderWriter;

namespace DataCollection
{
    /// <summary>
    /// 规则网格曲面投影计算工具类（修正版）
    /// </summary>
    public static class MeshSurfaceProjector
    {
        /// <summary>
        /// 计算平面点(x,y)在规则网格曲面上的三维投影坐标（双线性插值）
        /// 修正点：1. 逐行逐列提取网格坐标 2. 修正插值顶点/权重对应 3. 支持递增/递减网格 4. 增强异常处理
        /// </summary>
        /// <param name="grid">规则网格曲面的顶点数组，[行, 列] 对应Row×Col（行=Y维度，列=X维度）</param>
        /// <param name="targetX">目标点平面X坐标</param>
        /// <param name="targetY">目标点平面Y坐标</param>
        /// <returns>投影后的三维坐标Vector64(x,y,z)</returns>
        /// <exception cref="ArgumentNullException">网格数组为空</exception>
        /// <exception cref="ArgumentException">网格行列数不合法</exception>
        public static Vector64 ProjectToSurface(CMesh grid, Vector64 p2d)
        {
            double targetX = p2d.X, targetY = p2d.Y;
            int rowCount = grid.nRow;
            int colCount = grid.nCol;            

            // 2. 提取所有网格顶点的X/Y坐标（逐行逐列）
            double[,] gridX = new double[rowCount, colCount];
            double[,] gridY = new double[rowCount, colCount];
            double[,] gridZ = new double[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    gridX[r, c] = grid[r, c].X;
                    gridY[r, c] = grid[r, c].Y;
                    gridZ[r, c] = grid[r, c].Z;
                }
            }

            // 3. 提取行列的X/Y轴坐标（规则网格：列X单调，行Y单调）
            double[] colXAxis = GetAxisValues(gridX, isColAxis: true);  // 列X轴坐标（取第一行的X，规则网格所有行的列X一致）
            double[] rowYAxis = GetAxisValues(gridY, isColAxis: false); // 行Y轴坐标（取第一列的Y，规则网格所有列的行Y一致）
            if (colXAxis.Length != colCount || rowYAxis.Length != rowCount)
                throw new InvalidOperationException("规则网格的列X/行Y轴坐标提取失败（非规则网格）");

            // 4. 查找目标点所在的网格单元索引（支持递增/递减轴）
            (int colIdx, bool isXIncreasing) = FindAxisIndex(colXAxis, targetX);
            (int rowIdx, bool isYIncreasing) = FindAxisIndex(rowYAxis, targetY);

            // 5. 边界钳位：确保索引在有效区间 [0, count-2]
            colIdx = MyMath.Clamp(colIdx, 0, colCount - 2);
            rowIdx = MyMath.Clamp(rowIdx, 0, rowCount - 2);

            // 6. 获取当前网格单元的四个顶点（核心修正：顶点索引对应）
            // 顶点定义：
            // Q00: (rowIdx, colIdx)     左下/左上（取决于Y轴递增方向）
            // Q01: (rowIdx, colIdx+1)   右下/右上
            // Q10: (rowIdx+1, colIdx)   左上/左下
            // Q11: (rowIdx+1, colIdx+1) 右上/右下
            double x0 = colXAxis[colIdx];
            double x1 = colXAxis[colIdx + 1];
            double y0 = rowYAxis[rowIdx];
            double y1 = rowYAxis[rowIdx + 1];

            double z00 = gridZ[rowIdx, colIdx];
            double z01 = gridZ[rowIdx, colIdx + 1];
            double z10 = gridZ[rowIdx + 1, colIdx];
            double z11 = gridZ[rowIdx + 1, colIdx + 1];

            // 7. 计算双线性插值权重（修正：适配递增/递减轴）
            double tx = CalculateWeight(targetX, x0, x1, isXIncreasing); // X方向权重 [0,1]
            double ty = CalculateWeight(targetY, y0, y1, isYIncreasing); // Y方向权重 [0,1]
            tx = double.IsNaN(tx) ? 0 : MyMath.Clamp(tx, 0, 1); // 限制权重范围，避免外插过度
            ty = double.IsNaN(ty) ? 0 : MyMath.Clamp(ty, 0, 1);

            // 8. 双线性插值计算Z值（核心修正：插值公式）
            double zInterp = BilinearInterpolation(z00, z01, z10, z11, tx, ty);
            double val = grid[rowIdx, colIdx].V;
            // 9. 返回投影坐标（X/Y为目标点，Z为插值结果）
            return new Vector64(targetX, targetY, zInterp,val);
        }

        #region 辅助方法（核心修正）
        /// <summary>
        /// 提取规则网格的轴坐标（列X轴/行Y轴）
        /// </summary>
        private static double[] GetAxisValues(double[,] gridVals, bool isColAxis)
        {
            int rowCount = gridVals.GetLength(0);
            int colCount = gridVals.GetLength(1);
            double[] axis = new double[isColAxis ? colCount : rowCount];

            if (isColAxis)
            {
                // 列轴（X）：取第一行的所有列值（规则网格所有行的列值一致）
                for (int c = 0; c < colCount; c++) axis[c] = gridVals[0, c];
                // 验证所有行的列值一致（规则网格校验）
                for (int r = 1; r < rowCount; r++)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        if (Math.Abs(gridVals[r, c] - axis[c]) > 1e-8)
                            throw new ArgumentException("输入网格非规则网格（列轴值不统一）");
                    }
                }
            }
            else
            {
                // 行轴（Y）：取第一列的所有行值（规则网格所有列的行值一致）
                for (int r = 0; r < rowCount; r++) axis[r] = gridVals[r, 0];
                // 验证所有列的行值一致（规则网格校验）
                for (int c = 1; c < colCount; c++)
                {
                    for (int r = 0; r < rowCount; r++)
                    {
                        if (Math.Abs(gridVals[r, c] - axis[r]) > 1e-8)
                            throw new ArgumentException("输入网格非规则网格（行轴值不统一）");
                    }
                }
            }
            return axis;
        }

        /// <summary>
        /// 查找目标值在轴坐标中的区间索引（支持递增/递减）
        /// </summary>
        private static (int index, bool isIncreasing) FindAxisIndex(double[] axis, double target)
        {
            int count = axis.Length;
            if (count < 2) return (0, true);

            // 判断轴递增/递减
            bool isIncreasing = axis[1] > axis[0] + 1e-8;
            bool isDecreasing = axis[1] < axis[0] - 1e-8;

            // 线性查找区间（二分查找可优化效率）
            int idx = 0;
            if (isIncreasing)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    if (target >= axis[i] && target <= axis[i + 1])
                    {
                        idx = i;
                        break;
                    }
                    else if (target < axis[0]) { idx = 0; break; }
                    else if (target > axis[count - 1]) { idx = count - 2; break; }
                }
            }
            else if (isDecreasing)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    if (target <= axis[i] && target >= axis[i + 1])
                    {
                        idx = i;
                        break;
                    }
                    else if (target > axis[0]) { idx = 0; break; }
                    else if (target < axis[count - 1]) { idx = count - 2; break; }
                }
            }
            return (idx, isIncreasing);
        }

        /// <summary>
        /// 计算插值权重（适配递增/递减轴）
        /// </summary>
        private static double CalculateWeight(double target, double val0, double val1, bool isIncreasing)
        {
            double delta = val1 - val0;
            if (Math.Abs(delta) < 1e-10) return 0; // 轴值无变化，权重为0

            double weight = isIncreasing
                ? (target - val0) / delta
                : (val0 - target) / delta;
            return weight;
        }

        /// <summary>
        /// 双线性插值核心公式（标准公式）
        /// </summary>
        private static double BilinearInterpolation(double z00, double z01, double z10, double z11, double tx, double ty)
        {
            // 步骤1：X方向插值（两行）
            double zRow0 = z00 * (1 - tx) + z01 * tx; // 上/下行X插值
            double zRow1 = z10 * (1 - tx) + z11 * tx; // 下/上行X插值
            // 步骤2：Y方向插值（最终Z）
            double zFinal = zRow0 * (1 - ty) + zRow1 * ty;
            return zFinal;
        }
        #endregion
    }
    //object based on triangles    
    public class CMesh : C3DObjectBase
    {
        public int nRow = 0;
        public int nCol = 0;
        public Vector64[] pData = null;
        public double xStep = 0;
        public double yStep = 0;

        public struct IntersectionStruct 
        {
            public Vector64 P1;
            public Vector64 P2;
            public bool IsValid(int id) 
            {
                if (id == 1) 
                { 
                    if (double.IsNaN(P1.X) || double.IsNaN(P1.Y)|| double.IsNaN(P1.Z)) return false;
                }
                else //if (id == 1)
                {
                    if (double.IsNaN(P2.X) || double.IsNaN(P2.Y) || double.IsNaN(P2.Z)) return false;
                }
                return true;
            }
            public IntersectionStruct(Vector64 _p1, Vector64 _p2)
            {
                P1 = _p1;
                P2 = _p2;
            }
        }
        int[,] IntersectionIndices = null;
        public List<IntersectionStruct> CoordIntersections =new List<IntersectionStruct>();
        public List<Vector64> Boundaries = new List<Vector64>();
        public ClockDirection Clockwise = ClockDirection.None;
        public List<int>[] EdgeTables = null;     
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

        public override bool TopographyBlank(CMesh mesh)
        {
            for(int i=0;i<pData.Length;i++)            
            {
                Vector64 p = pData[i];
                double z = mesh.GetValue(p.X, p.Y);
                if (p.Z > mesh.GetValue(p.X, p.Y))
                {
                    p.V = double.NaN;
                    pData[i] = p;                   
                }
            }
            return true;
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
            InitEdgeTable();
        }
        public CMesh(int row, int col)
        {
            nRow = row;
            nCol = col;
            type = ShapeEnum.Mesh;           
            pData = new Vector64[row * col];
            Version = 1.3f; //2022-9-27 updated
            InitEdgeTable();
        }

        /// <summary>
        /// 2----6----3
        /// |         |
        /// 5         7
        /// |         |
        /// 0----4----1
        /// </summary>
        void InitEdgeTable()
        {
            EdgeTables = new List<int>[16];
            EdgeTables[0] = new List<int> { };
            EdgeTables[1] = new List<int> { 0, 4, 5 };
            EdgeTables[2] = new List<int> { 1, 7, 4 };
            EdgeTables[3] = new List<int> { 0, 1, 7, 5 };
            EdgeTables[4] = new List<int> { 2, 5, 6 };
            EdgeTables[5] = new List<int> { 2,0, 4, 6 };
            EdgeTables[6] = new List<int> { 1, 7, 6, 2, 5, 4 };
            EdgeTables[7] = new List<int> { 0, 1, 7, 6, 2 };
            EdgeTables[8] = new List<int> { 3, 6, 7 };
            EdgeTables[9] = new List<int> { 0, 4, 7, 3, 6, 5 };
            EdgeTables[10] = new List<int> { 1, 3, 6, 4 };
            EdgeTables[11] = new List<int> { 1, 3, 6, 5, 0 };
            EdgeTables[12] = new List<int> { 3, 2, 5, 7 };
            EdgeTables[13] = new List<int> { 2, 0, 4, 7, 3 };
            EdgeTables[14] = new List<int> { 3, 2, 5, 4, 1 };
            EdgeTables[15] = new List<int> { 0, 1, 3, 2 };
        }
        /// <summary>
        /// 2----6----3
        /// |         |
        /// 5         7
        /// |         |
        /// 0----4----1
        /// <summary>
        /// 获取节点或边的坐标
        /// </summary>
        /// <param name="irow"></param>
        /// <param name="icol"></param>
        /// <param name="id">0---7</param>
        /// <returns></returns>
        public Vector64 GetEdgePoint(int irow,int icol,int id)
        {
            Vector64 p = new Vector64(double.NaN, double.NaN, double.NaN, double.NaN);
            Vector64 p1, p2;
            switch (id)
            {
                case 0: return this[irow, icol];
                case 1: return this[irow, icol+1];
                case 2: return this[irow+1, icol];
                case 3: return this[irow+1, icol+1];
                case 4:
                    if (IsHaveIntersection(irow, icol,AxisEnum.xAxis)) p = CoordIntersections[IntersectionIndices[irow, icol]].P1;
                    else //没有找到交点就取中点
                    {
                        p1 = this[irow, icol];
                        p2 = this[irow, icol + 1];
                        p = (p1 + p2) / 2;
                        if (!IsBlanked(p1)) p.V = p1.V;
                        else p.V = p2.V;
                    }
                    break;
                case 5:
                    if (IsHaveIntersection(irow, icol, AxisEnum.yAxis)) p = CoordIntersections[IntersectionIndices[irow, icol]].P2;
                    else 
                    {
                        p1 = this[irow, icol];
                        p2 = this[irow+1, icol];
                        p = (p1 + p2) / 2;
                        if (!IsBlanked(p1)) p.V = p1.V;
                        else p.V = p2.V;
                    }
                    break;
                case 6:
                    if (IsHaveIntersection(irow + 1, icol, AxisEnum.xAxis)) p = CoordIntersections[IntersectionIndices[irow + 1, icol]].P1;
                    else 
                    {
                        p1 = this[irow+1, icol];
                        p2 = this[irow+1, icol + 1];
                        p = (p1 + p2) / 2;
                        if (!IsBlanked(p1)) p.V = p1.V;
                        else p.V = p2.V;
                    }
                    break;
                case 7:
                    if (IsHaveIntersection(irow, icol+1, AxisEnum.yAxis ))p = CoordIntersections[IntersectionIndices[irow, icol+1]].P2;
                    else
                    {
                        p1 = this[irow, icol+1];
                        p2 = this[irow + 1, icol + 1];
                        p = (p1 + p2) / 2;
                        if (!IsBlanked(p1)) p.V = p1.V;
                        else p.V = p2.V;
                    }
                    break;
                default: break;
            }
            if ( !IsValidPoint(p,4) && !IsInRange(p.X, p.Y) )
                throw new Exception("no valid points");
            
            return p;
        }
        /// <summary>
        /// 获取网格单元Mesh水平连接三角形点
        /// </summary>
        /// <param name="irow"></param>
        /// <param name="icol"></param>
        /// <param name="itype"></param>
        /// <returns></returns>
        public List<Vector64> GetCellHorizEdgeTablePoints(int irow,int icol,int itype)
        {
            List<Vector64> edgepoints = new List<Vector64>();
            List<int> indices = EdgeTables[itype];
            for (int i = 0; i < indices.Count; i++ )
            {
                edgepoints.Add(GetEdgePoint(irow, icol, indices[i]));
            }
            return edgepoints;
        }
        /// <summary>
        /// 获取网格单元Mesh垂向连接三角形点
        /// </summary>
        /// <param name="irow"></param>
        /// <param name="icol"></param>
        /// <param name="itype"></param>
        /// <returns></returns>
        public List<Vector64> GetCellVerticalEdgeTablePoints(int irow, int icol, int itype)
        {
            List<Vector64> edgepoints = new List<Vector64>();
            List<int> indices = EdgeTables[itype];
            for (int i = 0; i < indices.Count; i++)
            {
                edgepoints.Add(GetEdgePoint(irow, icol, indices[i]));

            }
            return edgepoints;
        }
        bool GetGridState(int irow, int icol,int id)
        {
            if (id == 0) return IsBlanked(this[irow, icol]);
            else if (id == 1) return IsBlanked(this[irow, icol+1]);
            else if (id == 2) return IsBlanked(this[irow+1, icol]);
            else if (id == 3) return IsBlanked(this[irow+1, icol+1]);
            return false;
        }
        public int GetGridType(int irow, int icol)
        {
            int iType = 0;
            for (int k = 0; k < 4; k++)
            {
                if ( !GetGridState(irow,icol,k) )
                {
                    iType |= (1 << k);
                }
            }
            return iType;
        }
        /// <summary>
        /// 直线与平面求交,得到直线在曲面上的投影
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public C3DLine CreateIntersectionLine(CLine line)
        {
            double step = Math.Min(xStep, yStep) * 0.5;
            int nstep = (int)(line.Length / step + 0.1);
            step = line.Length / nstep;
            C3DLine line3d = new C3DLine();
            for ( int i = 0; i < nstep; i++ )
            {
                Vector64 p = line.p1 + (line.p2 - line.p1) * (double) i / (nstep - 1);
                double z = GetValue(p.X, p.Y);
                p.Z = z;
                line3d.AddPoint(p);
            }
            return line3d;
        }
        /// <summary>
        /// 转换成Mesh的三角形连接
        /// </summary>
        /// <returns></returns>
        public TriangleObj toBlankedTriangleObj()
        {
            TriangleObj obj = new TriangleObj(Name);
            obj.CopyHeaderFrom(this);
            int count = 0;
            if (EdgeTables == null) InitEdgeTable();
            for (int i = 0; i < nRow-1; i++)
            {
                for (int j = 0; j < nCol-1; j++)
                {
                    int itype = GetGridType(i, j);
                    if (itype == 0) continue;
                    List<Vector64> edgepoints = GetCellHorizEdgeTablePoints(i, j, itype);
                    if (edgepoints.Count == 0) continue;
                    count = obj.points.Count;

                    if (edgepoints.Count == 3) 
                    {                        
                        obj.AddTriangleIndex(count, count+1, count+2);
                    }
                    else if (edgepoints.Count == 4)
                    {                        
                        obj.AddTriangleIndex(count, count + 1, count + 2);
                        obj.AddTriangleIndex(count, count + 2, count + 3);
                    }
                    else if (edgepoints.Count == 5)
                    {
                        obj.AddTriangleIndex(count, count + 1, count + 2);
                        obj.AddTriangleIndex(count, count + 2, count + 3);
                        obj.AddTriangleIndex(count, count + 3, count + 4);
                    }
                    else if (edgepoints.Count == 6)
                    {
                        obj.AddTriangleIndex(count, count + 1, count + 5);
                        obj.AddTriangleIndex(count + 1, count + 2, count + 5);
                        obj.AddTriangleIndex(count + 2, count + 4, count + 5);
                        obj.AddTriangleIndex(count + 2, count + 3, count + 4);
                    }
                    obj.AddPoints(edgepoints);
                    if (EnableColorLevel)
                    {
                        for (int k = 0; k < edgepoints.Count; k++)
                        {
                            obj.AddPointColor(GetColor(edgepoints[k].V));
                        }
                    }
                    edgepoints.Clear();
                }
            }
            
            obj.IsUniformColor = !EnableColorLevel;
            obj.color = ConvertColor.Convert(ObjColor);
            return obj;
        }
        /// <summary>
        /// 2----6----3
        /// |         |
        /// 5         7
        /// |         |
        /// 0----4----1
        bool IsGridPointOnEdge(int p,int irow,int icol)
        {
            if (p == 0 && (irow == 0 || icol == 0)) return true;
            if (p == 1 && (irow == 0 || icol == nCol-2)) return true;
            if (p == 2 && (irow == nRow-2 || icol == 0)) return true;
            if (p == 3 && (irow == nRow-2 || icol == nCol - 2)) return true;
            return false;
        }
        bool KeepGridPoint(int p, int irow, int icol)
        {
            if (p <= 3) return IsGridPointOnEdge(p, irow, icol);
            else return true;
        }
        /// <summary>
        /// 转换成垂向两层侧边三角网连接
        /// </summary>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public TriangleObj toBlankedTriangleObj(CMesh bottom)
        {
            TriangleObj obj = new TriangleObj(Name);
            obj.CopyHeaderFrom(this);
            int count = 0;
            if (EdgeTables == null) return obj;
            for (int i = 0; i < nRow - 1; i++)
            {
                for (int j = 0; j < nCol - 1; j++)
                {
                    int itype = GetGridType(i, j);
                    if (itype == 0) continue;
                    List<Vector64> edgepoints1 = GetCellVerticalEdgeTablePoints(i, j, itype);
                    List<Vector64> edgepoints2 = bottom.GetCellVerticalEdgeTablePoints(i, j, itype);
                    if (edgepoints1.Count == 0 || edgepoints2.Count == 0) continue;
                    count = obj.points.Count;
                    if(itype == 15)
                    {
                        if (i == 0 || j == 0 || i == nRow - 2 || j == nCol - 2)
                        {
                            obj.AddPoints(edgepoints1);
                            obj.AddPoints(edgepoints2);
                            if (EnableColorLevel)
                            {
                                for (int k = 0; k < edgepoints1.Count; k++)
                                    obj.AddPointColor(GetColor(edgepoints1[k].V));
                                for (int k = 0; k < edgepoints1.Count; k++)
                                    obj.AddPointColor(GetColor(edgepoints2[k].V));
                            }

                            if (i == 0)
                            {
                                obj.AddTriangleIndex(count, count + 4, count + 1);
                                obj.AddTriangleIndex(count + 1, count + 4, count + 5);
                            }
                            if (j == nCol - 2)
                            {
                                obj.AddTriangleIndex(count + 1, count + 5, count + 2);
                                obj.AddTriangleIndex(count + 2, count + 5, count + 6);
                            }
                            if (i == nRow - 2)
                            {
                                obj.AddTriangleIndex(count + 2, count + 6, count + 3);
                                obj.AddTriangleIndex(count + 3, count + 6, count + 7);
                            }
                            if (j == 0)
                            {
                                obj.AddTriangleIndex(count + 3, count + 7, count + 0);
                                obj.AddTriangleIndex(count + 0, count + 7, count + 4);
                            }
                        }                        
                    }
                    else 
                    {                        
                        int c = 0;
                        for (int k = 0; k < edgepoints1.Count; k++)
                        {
                            if (KeepGridPoint(EdgeTables[itype][k], i, j))
                            {
                                obj.AddPoint(edgepoints1[k]);
                                obj.AddPoint(edgepoints2[k]);
                                if (EnableColorLevel) obj.AddPointColor(GetColor(edgepoints1[k].V));
                                if (EnableColorLevel) obj.AddPointColor(GetColor(edgepoints2[k].V));
                                c++;
                            }
                        }
                        if (c >= 2 && c < edgepoints1.Count )
                        {
                            obj.AddTriangleIndex(count, count + 1, count + 2);
                            obj.AddTriangleIndex(count + 2, count + 1, count + 3);
                        }
                        if (c >= 3 && c < edgepoints1.Count)
                        {
                            obj.AddTriangleIndex(count + 2, count + 3, count + 4);
                            obj.AddTriangleIndex(count + 4, count + 3, count + 5);
                        }
                        if (c >= 4 && c < edgepoints1.Count)
                        {
                            obj.AddTriangleIndex(count + 4, count + 5, count + 6);
                            obj.AddTriangleIndex(count + 6, count + 5, count + 7);
                        }
                        if (c >= 5 && c < edgepoints1.Count)
                        {
                            obj.AddTriangleIndex(count + 6, count + 7, count + 8);
                            obj.AddTriangleIndex(count + 8, count + 7, count + 9);
                        }
                        if (c >= 6 && c < edgepoints1.Count)
                        {
                            obj.AddTriangleIndex(count + 8, count + 9, count + 10);
                            obj.AddTriangleIndex(count + 10, count + 9, count + 11);
                        }
                        if (c == edgepoints1.Count) //最后的封闭曲面
                        {
                            obj.AddTriangleIndex(count + 2 * (c - 1), count + 2 * (c - 1) + 1, count + 0);
                            obj.AddTriangleIndex(count + 0, count + 2 * (c - 1) + 1, count + 1);
                        }
                    }          
                    
                    edgepoints1.Clear();
                    edgepoints2.Clear();
                }
            }

            obj.IsUniformColor = !EnableColorLevel;
            obj.color = ConvertColor.Convert(ObjColor);
            return obj;
        }
        public Vector64 this[int row,int col]
        {
            get { return pData[row * nCol + col]; }
            set { pData[row * nCol + col] = value; }
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
        public Color GetColor(int irow,int icol) 
        {
            if (!EnableColorLevel) return ObjColor;
            else 
            {
                int i = irow, j = icol;
                MyMath.Clamp(i, 0, nRow-1);
                MyMath.Clamp(j, 0, nCol - 1);
                Vector64 p = this[i, j];
                return GetColor(p.V);
            }
        }
        public bool TrimWith(Polygon2D poly,bool keepOuter)
        {
            //Set Blanked
            for (int i = 0; i < nRow; i++)
            {
                for (int j = 0; j < nCol; j++)
                {
                    Vector64 p = this[i, j];
                    bool blank = false;
                    if (poly.IsPointInsidePoly(p))
                    {
                        if (keepOuter) blank = true;
                    }
                    else
                    {
                        if (!keepOuter) blank = true;
                    }
                    if (blank) { p.V = double.NaN; this[i, j] = p; }
                }
            }
            CreateBoundaries(poly);
            return true;
        }
        bool IsValidPoint(Vector64 p,int num = 2)
        {
            bool valid = true;
            if (num >= 1) valid &= !double.IsNaN(p.X);
            if (num >= 2) valid &= !double.IsNaN(p.Y);
            if (num >= 3) valid &= !double.IsNaN(p.Z);
            if (num >= 4) valid &= !double.IsNaN(p.V);
            return valid;
        }
        public bool IsHaveIntersection(int irow,int icol, AxisEnum axis)
        {
            if (IntersectionIndices == null) return false;
            int id = IntersectionIndices[irow,icol];
            if (id < 0) return false;
            if (CoordIntersections.Count <= id) return false;
            if (axis == AxisEnum.xAxis && IsValidPoint(CoordIntersections[id].P1)) return true;
            else if(axis == AxisEnum.yAxis && IsValidPoint(CoordIntersections[id].P2)) return true;
            return false;            
        }
        public IntersectionStruct GetIntersection(int irow, int icol)
        {
            int id = IntersectionIndices[irow, icol];
            return CoordIntersections[id];
        }
        /// <summary>
        /// 求线段与网格线的交点
        /// </summary>
        /// <param name="line"></param>
        /// <param name="intersection"></param>
        /// <returns></returns>
        public List<Vector64> GetIntersection(CLine line)
        {
            List<Vector64> points = new List<Vector64>();
            Vector64 p, p1, p2,s1,s2;
            CLine line1 = new CLine();
            CLine line2 = new CLine();
            for (int i = 0; i < nRow-1; i++)
            {                
                for (int j = 0; j < nCol-1; j++)
                {
                    p = this[i, j];
                    p1 = this[i, j + 1];
                    p2 = this[i + 1, j];
                    line1.p1 = p;   line1.p2 = p1;
                    line2.p1 = p;   line2.p2 = p2;
                    // p2
                    // |
                    // p------->p1
                    bool section1 = false;
                    bool section2 = false;
                    if (CLine.CalculateIntersection(line, line1,out s1)) //X axis
                    {
                        s1.Z = p.Z + (p1.Z - p.Z) * s1.Distance2D(p) / p1.Distance2D(p);
                        if(!IsBlanked(p))s1.V = p.V;
                        else s1.V = p1.V;
                        section1 = true;
                        points.Add(s1); 
                    }
                    if (CLine.CalculateIntersection(line, line2, out s2))  //Y axis
                    {
                        s2.Z = p.Z + (p2.Z - p.Z) * s2.Distance2D(p) / p2.Distance2D(p);
                        if (!IsBlanked(p)) s2.V = p.V;
                        else s2.V = p2.V;                        
                        section2 = true;
                        points.Add(s2); 
                    }
                    if(section1 || section2)
                    {
                        IntersectionIndices[i, j] = CoordIntersections.Count;                        
                        CoordIntersections.Add(new IntersectionStruct(s1, s2));
                    }
                }
            }

            //points.OrderBy(s => s.Distance2D(line.p1));
            points = SortPoints(points,line.p1);
            return points;
        }
        List<Vector64>SortPoints(List<Vector64>list1, Vector64 p0)
        {
            for(int i=0; i<list1.Count; i++)
            {
                Vector64 s1 = list1[i];
                s1.V = s1.Distance2D(p0);
                list1[i] = s1;
            }
            list1.Sort((a, b) => { return a.V.CompareTo(b.V); });
            return list1;
        }

        /// <summary>
        /// 计算平面点(x,y)在规则网格曲面上的三维投影坐标
        /// 核心：双线性插值（内插优先，外插基于边缘单元扩展）
        /// </summary>
        /// <param name="grid">规则网格曲面的顶点数组，[行, 列] 对应Row×Col</param>
        /// <param name="targetX">目标点平面X坐标</param>
        /// <param name="targetY">目标点平面Y坐标</param>
        /// <returns>投影后的三维坐标Vector64(x,y,z)</returns>
        /// <exception cref="ArgumentNullException">网格数组为空</exception>
        /// <exception cref="ArgumentException">网格行列数不合法</exception>
        public Vector64 GetProjectCoord(Vector64 p2d)
        {
            double targetX = p2d.X, targetY = p2d.Y;
            int rowCount = nRow;
            int colCount = nCol;            
            // 2. 提取网格的X/Y范围，确定目标点所在的网格单元索引
            // 步骤2.1：获取网格的行列X/Y坐标（规则网格假设行列方向X/Y单调）
            double[] colXs = new double[colCount]; // 每列的X坐标（取第一行的X值，规则网格行列对齐）
            double[] rowYs = new double[rowCount]; // 每行的Y坐标（取第一列的Y值，规则网格行列对齐）
            for (int col = 0; col < colCount; col++) colXs[col] = this[0, col].X;
            for (int row = 0; row < rowCount; row++) rowYs[row] = this[row, 0].Y;

            // 步骤2.2：找到目标点所在的列区间 [colIdx, colIdx+1]
            int colIdx = FindGridIndex(colXs, targetX);
            // 步骤2.3：找到目标点所在的行区间 [rowIdx, rowIdx+1]
            int rowIdx = FindGridIndex(rowYs, targetY);

            // 3. 处理边界：若目标点在网格外，取边缘单元（外插）
            colIdx = MyMath.Clamp(colIdx, 0, colCount - 2);
            rowIdx = MyMath.Clamp(rowIdx, 0, rowCount - 2);

            // 4. 获取当前网格单元的四个顶点（Q11:左下, Q12:左上, Q21:右下, Q22:右上）
            Vector64 Q11 = this[rowIdx, colIdx];     // (rowIdx, colIdx)
            Vector64 Q12 = this[rowIdx, colIdx + 1]; // (rowIdx, colIdx+1)
            Vector64 Q21 = this[rowIdx + 1, colIdx]; // (rowIdx+1, colIdx)
            Vector64 Q22 = this[rowIdx + 1, colIdx + 1]; // (rowIdx+1, colIdx+1)

            // 5. 双线性插值计算Z值
            // 步骤5.1：计算X方向的插值权重（相对colIdx的X偏移）
            double x1 = Q11.X;
            double x2 = Q12.X;
            double tx = (targetX - x1) / (x2 - x1); // 0≤tx≤1（内插），tx<0或tx>1（外插）
            tx = double.IsNaN(tx) ? 0 : tx; // 处理x1=x2的极端情况

            // 步骤5.2：计算Y方向的插值权重（相对rowIdx的Y偏移）
            double y1 = Q11.Y;
            double y2 = Q21.Y;
            double ty = (targetY - y1) / (y2 - y1); // 0≤ty≤1（内插），ty<0或ty>1（外插）
            ty = double.IsNaN(ty) ? 0 : ty; // 处理y1=y2的极端情况

            // 步骤5.3：双线性插值公式
            // 先在X方向插值两次，再在Y方向插值
            double z1 = Q11.Z * (1 - tx) + Q12.Z * tx; // 下边缘（rowIdx行）的X插值
            double z2 = Q21.Z * (1 - tx) + Q22.Z * tx; // 上边缘（rowIdx+1行）的X插值
            double targetZ = z1 * (1 - ty) + z2 * ty;  // Y方向插值得到最终Z
            double targetV = this[rowIdx, colIdx].V;
            // 6. 返回投影后的三维坐标
            return new Vector64(targetX, targetY, targetZ, targetV);
        }

        /// <summary>
        /// 查找目标值在单调数组中的区间索引（返回左边界索引）
        /// 例如：数组[1,3,5]，目标值4 → 返回1（区间[3,5]）
        /// </summary>
        private int FindGridIndex(double[] sortedArray, double target)
        {
            // 假设数组是单调递增的（规则网格默认）
            for (int i = 0; i < sortedArray.Length - 1; i++)
            {
                if (target >= sortedArray[i] && target <= sortedArray[i + 1])
                    return i;
            }
            // 目标值在数组外：小于最小值返回0，大于最大值返回最后一个区间左边界
            return target < sortedArray[0] ? 0 : sortedArray.Length - 2;
        }

        public List<Vector64>CreateBoundaries(Polygon2D poly)
        {
            Boundaries.Clear();
            CoordIntersections.Clear();
            IntersectionIndices = new int[nRow,nCol];
            InitEdgeTable();
            for(int i=0;i<nRow;i++)
            { 
                for (int j = 0; j < nCol; j++) 
                    IntersectionIndices[i, j] = -1; 
            }
            Clockwise = poly.GetClockDirection();
            List<Vector64>poly1 = new List<Vector64>();
            for(int i=0;i<poly.Count;i++)
            {
                poly1.Add(MeshSurfaceProjector.ProjectToSurface(this,poly[i]));
                //poly1.Add(poly[i]);
            }
            CLine line = new CLine();
            for(int i=0;i<poly1.Count;i++)
            {
                line.p1 = poly1[i];
                if(i==poly1.Count-1) line.p2 = poly1[0];
                else line.p2 = poly1[i + 1];
                
                Boundaries.Add(line.p1);

                List<Vector64> points = GetIntersection(line);
                if (points.Count > 0)
                {                    
                    for (int k = 0; k < points.Count; k++)
                    {
                        if (points[k].Distance(line.p1) > 1e-8)
                            Boundaries.Add(points[k]);
                    }
                    //Boundaries.AddRange(points);
                    points.Clear();
                }
            }
            
            //Boundaries.Add(poly1[poly1.Count-1]);
            poly1.Clear();
            
            //for(int i = 0;i < nCol-1;i++)
            //{
            //    if( GetGridType(0,i)==15 )
            //    {
            //        Boundaries.Add(this[0,i]);
            //    }
            //    if (GetGridType(nRow-2, i) == 15)
            //    {
            //        Boundaries.Add(this[nRow-1, i]);
            //    }
            //}
            //for (int i = 1; i < nRow-2; i++)
            //{
            //    if (GetGridType(i, 0) == 15)
            //    {
            //        Boundaries.Add(this[i, 0]);
            //    }
            //    if (GetGridType(i, nCol-2) == 15)
            //    {
            //        Boundaries.Add(this[i,nCol-1]);
            //    }
            //}
            //StreamWriter wr = new StreamWriter("d:\\jian\\boundaries.csv");
            //wr.WriteLine("X,Y,Z,V");
            //for(int i=0;i<Boundaries.Count;i++)
            //{
            //    wr.WriteLine(Boundaries[i].ToString());
            //}
            //wr.Close();
            //List<Vector64> lists = Vector64.SortBoundaryPoints( Boundaries );
            //List<Vector64> lists = PolygonSorter.SortPolygonPoints( Boundaries );            
            //Boundaries.Clear();
            //Boundaries = lists;
            return Boundaries;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override vec2 GetTextureCoord(Vector32 p, planEnum plan = planEnum.XOY)
        {
            double x1 = Minx;
            double x2 = Maxx;
            double y1 = Miny;
            double y2 = Maxy;
            double x0 = p.X, y0 = p.Y;
            float x = -1, y = -1;
            //Z
            //|
            //o--->Y
            if (plan == planEnum.YOZ)
            {
                x0 = p.Y;y0 = p.Z;
                x1 = Miny; x2 = Maxy;
                y1 = Minz; y2 = Maxz;
            }
            else if (plan == planEnum.XOZ)
            {
                x0 = p.X; y0 = p.Z;
                x1 = Minx; x2 = Maxx;
                y1 = Minz; y2 = Maxz;
            }
            if (textureStruct != null)
            {
                if (textureStruct.textureRect.Width > 0 &&
                    textureStruct.textureRect.Height > 0)
                {
                    x1 = textureStruct.textureRect.X1;
                    x2 = textureStruct.textureRect.X2;
                    y1 = textureStruct.textureRect.Y1;
                    y2 = textureStruct.textureRect.Y2;
                }
            }
            if (textureStruct.textureRect.Contains(x0, y0))
            {
                x = (float)((x0 - x1) / (x2 - x1));
                y = (float)((y0 - y1) / (y2 - y1));
                if (textureStruct.FlipVertical) y = 1 - y;
                if (textureStruct.FlipHorizontal) x = 1 - x;
            }
            return new vec2((float)x, (float)y);
        }

        public CMesh VerticalDerivating(int level = 1)
        {
            CMesh mesh = new CMesh(nRow-1,nCol-1);           
            Vector64 p1,p2,p3,p;
            //for(int iy = 0; iy < nRow-1;iy++)
            //{
            //    for (int ix = 0; ix < nCol; ix++)
            //    {
            //        p1 = GetPoint(iy, ix);
            //        p2 = GetPoint(iy+1, ix);
            //        p = p1;
            //        p.v = p2.v - p1.v;
            //        mesh[iy, ix] = p;
            //    }
            //}
            for (int iy = 0; iy < nRow - 1; iy++)
            {
                for (int ix = 0; ix < nCol-1; ix++)
                {
                    p1 = GetPoint(iy, ix);
                    p2 = GetPoint(iy + 1, ix);
                    p3 = GetPoint(iy , ix+1);
                    p = p1;
                    Color c1 = ColorScale.GetColor(p1.v);
                    Color c2 = ColorScale.GetColor(p2.v);
                    Color c3 = ColorScale.GetColor(p3.v);
                    int v1 = ConvertColor.toGray(c1);
                    int v2 = ConvertColor.toGray(c2);
                    int v3 = ConvertColor.toGray(c3);
                    p.v = Math.Abs(v2 - v1) + Math.Abs(v3 - v1);
                    mesh[iy, ix] = p;
                }
            }

            mesh.UpdateRange();
            return mesh;
        }
        public CMesh HorizontalDerivating(int level = 1)
        {
            CMesh mesh = new CMesh(nRow, nCol-1);
            Vector64 p1, p2, p;
            for (int iy = 0; iy < nRow; iy++)
            {
                for (int ix = 0; ix < nCol-1; ix++)
                {
                    p1 = GetPoint(iy, ix);
                    p2 = GetPoint(iy, ix+1);
                    p = p1;
                    p.v = p2.v - p1.v;
                    mesh[iy, ix] = p;
                }
            }
            mesh.UpdateRange();
            return mesh;
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
        public void GetVerticIndex(double x,double y,out int ix,out int iy)
        {
            double stepx = (Maxx - Minx) / (nCol-1);
            double stepy = (Maxy - Miny) / (nRow-1);
            ix = (int)((x - Minx) / stepx + 0.1);
            iy = (int)((y - Miny) / stepy + 0.1);
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
        public override bool SaveAs(string path,int version=0)
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

                //added on 2026-1-9                
                bool blanked = false;
                if (IntersectionIndices != null) blanked = true;
                br.Write(blanked);                
                if (blanked)//写入裁剪
                {
                    for (int i = 0; i < nRow; i++)
                        for (int j = 0; j < nCol; j++)
                            br.Write(IntersectionIndices[i, j]);
                }

                br.Write(CoordIntersections.Count);
                for(int i=0;i<CoordIntersections.Count;i++)
                {
                    IntersectionStruct sections = CoordIntersections[i];
                    sections.P1.Write(br);
                    sections.P2.Write(br);
                }
                br.Write(Boundaries.Count);
                for (int i = 0; i < Boundaries.Count; i++)
                {
                    Boundaries[i].Write(br);
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
                
                if (C3DData.DataVersion >= 1.33f)//added on 2026-1-9
                {
                    IntersectionIndices = null;
                    bool blanked = br.ReadBoolean();                    
                    if (blanked)
                    {
                        IntersectionIndices = new int[nRow, nCol];
                        for (int i = 0; i < nRow; i++)
                            for (int j = 0; j < nCol; j++)
                                IntersectionIndices[i, j] = br.ReadInt32();
                    }
                    CoordIntersections.Clear();
                    int n = br.ReadInt32();
                    for (int i = 0; i < n; i++)
                    {
                        Vector64 p1 = new Vector64(); p1.Load(br);
                        Vector64 p2 = new Vector64(); p2.Load(br);
                        CoordIntersections.Add(new IntersectionStruct(p1, p2));
                    }
                    Boundaries.Clear();
                    n = br.ReadInt32();
                    for (int i = 0; i < n; i++)
                    {
                        Vector64 p1 = new Vector64(); p1.Load(br);
                        Boundaries.Add(p1);
                    }
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
            
            bool exportz = false;
            if (minv >= maxv) //V值无效输出Z值
            { 
                exportz = true;
                cs.minv = minz;
                cs.maxv = maxz;
            }

            cs.pData = new float[nRow * nCol];
            for (int i = 0; i < nRow; i++)
            {
                for (int j = 0; j < nCol; j++)
                {
                    if(exportz) val = GetPoint(i, j).Z;
                    else val = GetPoint(i, j).V;
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
            CoordIntersections.Clear();
            Boundaries.Clear();
            IntersectionIndices = null;
        }
        public bool IsBlanked(Vector64 p)
        {
            return IsBlanked(p.v);
        }
        public bool IsBlankedGrid(int irow,int icol)
        {
            if (irow < 0 || irow >= nRow) return true;
            if (icol < 0 || icol >= nCol) return true;            
            return IsBlanked(this[irow, icol].v);
        }
        public void ResetDataRange(double x1, double x2, double y1, double y2,double z1,double z2)
        {
            bool mx = false, my = false, mz = false;
            if (minx != x1 || maxx != x2) mx = true;
            if (miny != y1 || maxy != y2) my = true;
            if (minz != z1 || maxz != z2) mz = true;
            for (int i = 0; i < pData.Length; i++) 
            {
                Vector64 p = pData[i];
                if(mx)p.X = x1 + (p.X - minx) / (maxx - minx) * (x2 - x1);
                if(my)p.Y = y1 + (p.Y - miny) / (maxy - miny) * (y2 - y1);
                if(mz)p.Z = p.V = z1 + (p.Z - minz) / (maxz - minz) * (z2 - z1);
                pData[i] = p;
            }
            minx = x1;
            miny = y1;
            maxx = x2;
            maxy = y2;
            minz = minv = z1;
            maxz = maxv = z2;
            xStep = (maxx - minx) / (nCol - 1);
            yStep = (maxy - miny) / (nRow - 1);
            UpdateTextureRect();
        }
        public void UpdateTextureRect(planEnum plan = planEnum.XOY)
        {
            double x1 = Minx;
            double x2 = Maxx;
            double y1 = Miny;
            double y2 = Maxy;
            //Z
            //|
            //o--->Y
            if (plan == planEnum.YOZ)
            {               
                x1 = Miny; x2 = Maxy;
                y1 = Minz; y2 = Maxz;
            }
            else if (plan == planEnum.XOZ)
            {               
                x1 = Minx; x2 = Maxx;
                y1 = Minz; y2 = Maxz;
            }
            textureStruct.textureRect = new DoubleRect(x1, y1, x2, y2);
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
            UpdateTextureRect();
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
