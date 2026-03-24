using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
namespace DataCollection.DSI3DTriangulation
{
    #region 核心数据结构（修订：顶点全三维化）
    /// <summary>
    /// 三维顶点（带索引）
    /// </summary>
    public struct Vertex3D : IEquatable<Vertex3D>
    {
        public double X;
        public double Y;
        public double Z;
        public int Index; // 全局索引

        public Vertex3D(double x, double y, double z, int index = -1)
        {
            X = x;
            Y = y;
            Z = z;
            Index = index;
        }

        /// <summary>
        /// 计算两点欧氏距离
        /// </summary>
        public double DistanceTo(Vertex3D other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            double dz = Z - other.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public bool Equals(Vertex3D other) => Math.Abs(X - other.X) < 1e-6 &&
                                              Math.Abs(Y - other.Y) < 1e-6 &&
                                              Math.Abs(Z - other.Z) < 1e-6;
        public override bool Equals(object obj) => obj is Vertex3D other && Equals(other);
        public override int GetHashCode() 
        { 
           return GetHashCode()^X.GetHashCode()^Y.GetHashCode()^Z.GetHashCode(); 
        }
        public override string ToString() => $"({X:F6}, {Y:F6}, {Z:F6})[{Index}]";
    }

    /// <summary>
    /// 三角面片（由三个顶点索引组成）
    /// </summary>
    public struct Triangle
    {
        public int V1;
        public int V2;
        public int V3;

        public Triangle(int v1, int v2, int v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        public override string ToString() => $"({V1}, {V2}, {V3})";
    }

    /// <summary>
    /// 三维多边形轮廓（每个顶点独立X/Y/Z坐标）
    /// </summary>
    public class ContourPolygon3D
    {
        public List<Vertex3D> Vertices; // 轮廓顶点（三维坐标，按顺时针/逆时针排序）
        public string Name; // 轮廓名称（可选）

        public ContourPolygon3D(string name = "")
        {
            Name = name;
            Vertices = new List<Vertex3D>();
        }

        /// <summary>
        /// 添加三维顶点到轮廓
        /// </summary>
        public void AddVertex(double x, double y, double z) => Vertices.Add(new Vertex3D(x, y, z));

        /// <summary>
        /// 轮廓顶点数量
        /// </summary>
        public int VertexCount => Vertices.Count;

        /// <summary>
        /// 检查轮廓是否闭合（首尾顶点是否重合）
        /// </summary>
        public bool IsClosed => VertexCount > 0 && Vertices.First().DistanceTo(Vertices.Last()) < 1e-6;

        /// <summary>
        /// 闭合轮廓（首尾顶点不重合时补充）
        /// </summary>
        public void Close()
        {
            if (!IsClosed && VertexCount > 0)
            {
                Vertices.Add(Vertices.First());
            }
        }

        /// <summary>
        /// 获取轮廓的平均Z坐标（用于排序）
        /// </summary>
        public double GetAverageZ() => VertexCount == 0 ? 0 : Vertices.Average(v => v.Z);
    }
    #endregion

    #region DSI三角网构建核心类（适配三维轮廓）
    public class Dsi3DTriangulator
    {
        #region 配置参数
        private const double _tolerance = 1e-6; // 数值容差
        private const int _maxSubdivisionLevel = 3; // 最大细分层级（用于顶点对齐）
        #endregion

        // 全局顶点列表（去重）
        private readonly List<Vertex3D> _globalVertices = new List<Vertex3D>();
        // 三角面片列表
        private readonly List<Triangle> _triangles = new List<Triangle>();
        // 三维轮廓列表（按平均Z排序）
        private List<ContourPolygon3D> _sortedContours = new List<ContourPolygon3D>();

        /// <summary>
        /// 构建三维三角网模型（支持任意三维轮廓输入）
        /// </summary>
        /// <param name="contours">输入的三维多边形轮廓列表</param>
        /// <returns>（全局顶点列表，三角面片列表）</returns>
        public (List<Vertex3D> Vertices, List<Triangle> Triangles) BuildModel(List<ContourPolygon3D> contours)
        {
            // 1. 预处理：验证轮廓、排序、闭合、顶点对齐
            PreprocessContours(contours);

            // 2. 构建层内三角网（Delaunay三角化，投影到XY平面）
            //BuildIntraLayerTriangulation();

            // 3. 构建层间三角网（DSI纵向连接，保留三维坐标）
            BuildInterLayerTriangulation();

            // 4. 分配全局顶点索引
            AssignGlobalVertexIndices();

            return (_globalVertices, _triangles);
        }

        public TriangleObj toTriangleObj()
        {
            TriangleObj obj = new TriangleObj();
            // 2. 写入顶点数据（完整三维坐标）
            foreach (var vertex in _globalVertices)
            {
                obj.AddPoint(new Vector32(vertex.X, vertex.Z, vertex.Y));
            }

            // 3. 写入面片数据（PLY格式：顶点数 + 索引列表，索引从0开始）
            foreach (var tri in _triangles)
            {
                obj.AddTriangleIndex(tri.V1, tri.V2, tri.V3);
            }
            obj.UpdateRange();
            return obj;
        }


        #region PLY格式导出（完整支持三维坐标）
        /// <summary>
        /// 导出模型为PLY格式（ASCII）
        /// </summary>
        /// <param name="filePath">保存路径（如"model.ply"）</param>
        public void ExportToPlyAscii(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (StreamWriter sw = new StreamWriter(filePath))
            {
                // 1. 写入PLY文件头
                sw.WriteLine("ply");
                sw.WriteLine("format ascii 1.0");
                sw.WriteLine($"element vertex {_globalVertices.Count}");
                sw.WriteLine("property double x");
                sw.WriteLine("property double y");
                sw.WriteLine("property double z");
                sw.WriteLine($"element face {_triangles.Count}");
                sw.WriteLine("property list uchar int vertex_indices");
                sw.WriteLine("end_header");

                // 2. 写入顶点数据（完整三维坐标）
                foreach (var vertex in _globalVertices)
                {
                    sw.WriteLine($"{vertex.X:F6} {vertex.Y:F6} {vertex.Z:F6}");
                }

                // 3. 写入面片数据（PLY格式：顶点数 + 索引列表，索引从0开始）
                foreach (var tri in _triangles)
                {
                    sw.WriteLine($"3 {tri.V1} {tri.V2} {tri.V3}");
                }
            }

            Console.WriteLine($"PLY模型（ASCII）已导出至：{filePath}");
        }

        /// <summary>
        /// 导出模型为PLY格式（二进制，体积更小）
        /// </summary>
        /// <param name="filePath">保存路径</param>
        public void ExportToPlyBinary(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (BinaryWriter bw = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                // 1. 写入PLY文件头（ASCII）
                string header = $"ply\nformat binary_little_endian 1.0\nelement vertex {_globalVertices.Count}\nproperty double x\nproperty double y\nproperty double z\nelement face {_triangles.Count}\nproperty list uchar int vertex_indices\nend_header\n";
                bw.Write(System.Text.Encoding.ASCII.GetBytes(header));

                // 2. 写入顶点数据（双精度浮点数，完整三维坐标）
                foreach (var vertex in _globalVertices)
                {
                    bw.Write((double)vertex.X);
                    bw.Write((double)vertex.Y);
                    bw.Write((double)vertex.Z);
                }

                // 3. 写入面片数据：uchar（顶点数3） + int[3]（索引）
                foreach (var tri in _triangles)
                {
                    bw.Write((byte)3); // 每个三角面片3个顶点
                    bw.Write(tri.V1);
                    bw.Write(tri.V2);
                    bw.Write(tri.V3);
                }
            }

            Console.WriteLine($"PLY模型（二进制）已导出至：{filePath}");
        }
        #endregion

        #region 核心预处理逻辑（适配三维轮廓）
        /// <summary>
        /// 轮廓预处理（验证、按平均Z排序、闭合、顶点对齐）
        /// </summary>
        private void PreprocessContours(List<ContourPolygon3D> contours)
        {
            // 验证输入
            if (contours == null || contours.Count < 2)
                throw new ArgumentException("至少需要2个三维轮廓才能构建模型");
            if (contours.Any(c => c.VertexCount < 3))
                throw new ArgumentException("每个轮廓至少需要3个三维顶点");

            // 按轮廓平均Z坐标排序（升序）
            _sortedContours = contours.OrderBy(c => c.GetAverageZ()).ToList();
            
            // 闭合所有轮廓
            foreach (var contour in _sortedContours)
            {
                contour.Close();
            }

            // 顶点对齐：确保所有轮廓顶点数量一致（DSI核心要求）
            AlignContourVertices();

            // 收集所有顶点到全局列表（去重，基于三维坐标）
            foreach (var contour in _sortedContours)
            {
                foreach (var vertex in contour.Vertices)
                {
                    if (!_globalVertices.Any(v => v.Equals(vertex)))
                    {
                        _globalVertices.Add(vertex);
                    }
                }
            }
        }

        /// <summary>
        /// 三维轮廓顶点对齐：细分顶点少的轮廓，使所有轮廓顶点数量一致
        /// </summary>
        private void AlignContourVertices()
        {
            // 找到顶点数量最多的轮廓
            int maxVertexCount = _sortedContours.Max(c => c.VertexCount);

            // 对每个轮廓进行细分，直到顶点数量匹配
            foreach (var contour in _sortedContours)
            {
                int currentCount = contour.VertexCount;
                if (currentCount == maxVertexCount) continue;

                // 递归细分三维顶点
                SubdivideContour3D(contour, maxVertexCount, 0);
            }
        }

        /// <summary>
        /// 递归细分三维轮廓顶点（保留Z坐标插值）
        /// </summary>
        private void SubdivideContour3D(ContourPolygon3D contour, int targetCount, int level)
        {
            if (level >= _maxSubdivisionLevel || contour.VertexCount >= targetCount)
                return;

            List<Vertex3D> newVertices = new List<Vertex3D>();
            for (int i = 0; i < contour.Vertices.Count - 1; i++)
            {
                var v1 = contour.Vertices[i];
                var v2 = contour.Vertices[i + 1];

                // 添加原三维顶点
                newVertices.Add(v1);

                // 插入三维中点（X/Y/Z均插值）
                if (newVertices.Count < targetCount)
                {
                    double midX = (v1.X + v2.X) / 2;
                    double midY = (v1.Y + v2.Y) / 2;
                    double midZ = (v1.Z + v2.Z) / 2; // 三维中点，保留Z坐标插值
                    newVertices.Add(new Vertex3D(midX, midY, midZ));
                }
            }

            // 闭合新轮廓
            contour.Vertices = newVertices;
            contour.Close();

            // 递归细分
            SubdivideContour3D(contour, targetCount, level + 1);
        }
        #endregion

        #region 层内三角化（投影XY平面，保留Z坐标）
        /// <summary>
        /// 构建每层三维轮廓的内部三角网（Delaunay三角化）
        /// </summary>
        private void BuildIntraLayerTriangulation()
        {
            foreach (var contour in _sortedContours)
            {
                // 提取层内顶点的XY投影（用于三角化），保留原始三维顶点映射
                List<Vector2> planarVertices = new List<Vector2>();
                Dictionary<Vector2, Vertex3D> planarTo3DMap = new Dictionary<Vector2, Vertex3D>();

                foreach (var v3d in contour.Vertices.Take(contour.VertexCount - 1)) // 排除闭合顶点
                {
                    Vector2 v2d = new Vector2((float)v3d.X, (float)v3d.Y);
                    planarVertices.Add(v2d);
                    planarTo3DMap[v2d] = v3d;
                }

                // Delaunay三角化（基于XY投影）
                List<Triangle> intraTriangles = DelaunayTriangulate(planarVertices, contour, planarTo3DMap);

                // 添加到全局三角列表
                _triangles.AddRange(intraTriangles);
            }
        }

        /// <summary>
        /// 二维Delaunay三角化（Bowyer-Watson算法），映射回三维顶点
        /// </summary>
        private List<Triangle> DelaunayTriangulate(List<Vector2> vertices, ContourPolygon3D contour,
                                                   Dictionary<Vector2, Vertex3D> planarTo3DMap)
        {
            List<Triangle> triangles = new List<Triangle>();
            int vertexCount = vertices.Count;
            if (vertexCount < 3) return triangles;

            // 1. 创建超级三角形（包含所有顶点）
            float minX = vertices.Min(v => v.X);
            float maxX = vertices.Max(v => v.X);
            float minY = vertices.Min(v => v.Y);
            float maxY = vertices.Max(v => v.Y);
            float dx = maxX - minX;
            float dy = maxY - minY;
            float maxDist = Math.Max(dx, dy) * 2;

            Vector2 v1 = new Vector2(minX - maxDist, minY - maxDist);
            Vector2 v2 = new Vector2(maxX + maxDist, minY - maxDist);
            Vector2 v3 = new Vector2(minX, maxY + maxDist);

            // 超级三角形顶点添加到临时列表
            List<Vector2> tempVertices = new List<Vector2>(vertices) { v1, v2, v3 };
            triangles.Add(new Triangle(vertexCount, vertexCount + 1, vertexCount + 2));

            // 2. 逐个插入顶点
            for (int i = 0; i < vertexCount; i++)
            {
                Vector2 p = vertices[i];
                List<Triangle> badTriangles = new List<Triangle>();

                // 找到包含当前顶点的外接圆的三角形（坏三角形）
                foreach (var tri in triangles)
                {
                    Vector2 t1 = tempVertices[tri.V1];
                    Vector2 t2 = tempVertices[tri.V2];
                    Vector2 t3 = tempVertices[tri.V3];

                    if (IsPointInCircumcircle(p, t1, t2, t3))
                    {
                        badTriangles.Add(tri);
                    }
                }

                // 收集坏三角形的边界边
                List<(int, int)> edges = new List<(int, int)>();
                foreach (var tri in badTriangles)
                {
                    edges.Add((tri.V1, tri.V2));
                    edges.Add((tri.V2, tri.V3));
                    edges.Add((tri.V3, tri.V1));
                }

                // 移除重复边（边界边只出现一次）
                edges = edges.GroupBy(e => new { Min = Math.Min(e.Item1, e.Item2), Max = Math.Max(e.Item1, e.Item2) })
                    .Where(g => g.Count() == 1)
                    .Select(g => g.First())
                    .ToList();

                // 删除坏三角形
                triangles.RemoveAll(t => badTriangles.Contains(t));

                // 用当前顶点构建新三角形
                foreach (var edge in edges)
                {
                    triangles.Add(new Triangle(edge.Item1, edge.Item2, i));
                }
            }

            // 3. 移除包含超级三角形顶点的三角形
            triangles.RemoveAll(t => t.V1 >= vertexCount || t.V2 >= vertexCount || t.V3 >= vertexCount);

            // 4. 映射回三维顶点，获取全局索引
            List<Triangle> result = new List<Triangle>();
            foreach (var tri in triangles)
            {
                Vector2 v2d_1 = vertices[tri.V1];
                Vector2 v2d_2 = vertices[tri.V2];
                Vector2 v2d_3 = vertices[tri.V3];

                if (planarTo3DMap.TryGetValue(v2d_1, out Vertex3D v3d_1) &&
                    planarTo3DMap.TryGetValue(v2d_2, out Vertex3D v3d_2) &&
                    planarTo3DMap.TryGetValue(v2d_3, out Vertex3D v3d_3))
                {
                    int idx1 = _globalVertices.FindIndex(v => v.Equals(v3d_1));
                    int idx2 = _globalVertices.FindIndex(v => v.Equals(v3d_2));
                    int idx3 = _globalVertices.FindIndex(v => v.Equals(v3d_3));

                    if (idx1 >= 0 && idx2 >= 0 && idx3 >= 0)
                    {
                        result.Add(new Triangle(idx1, idx2, idx3));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 判断点是否在三角形外接圆内
        /// </summary>
        private bool IsPointInCircumcircle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float ax = a.X - p.X;
            float ay = a.Y - p.Y;
            float bx = b.X - p.X;
            float by = b.Y - p.Y;
            float cx = c.X - p.X;
            float cy = c.Y - p.Y;

            float det = ax * (by * (cx * cx + cy * cy) - cy * (bx * bx + by * by))
                      - ay * (bx * (cx * cx + cy * cy) - cx * (bx * bx + by * by))
                      + (ax * ax + ay * ay) * (bx * cy - cx * by);

            return det > _tolerance;
        }
        #endregion

        #region 层间三角化（完整三维坐标连接）
        /// <summary>
        /// 构建层间三角网（相邻三维轮廓顶点一对一连接，形成四边形后拆分）
        /// </summary>
        private void BuildInterLayerTriangulation()
        {
            // 遍历相邻两层轮廓
            for (int i = 0; i < _sortedContours.Count - 1; i++)
            {
                var lowerContour = _sortedContours[i];
                var upperContour = _sortedContours[i + 1];

                int vertexCount = lowerContour.VertexCount - 1; // 排除闭合顶点
                if (upperContour.VertexCount - 1 != vertexCount) continue;

                // 逐个顶点对构建三维纵向三角
                for (int j = 0; j < vertexCount; j++)
                {
                    int jNext = (j + 1) % vertexCount;

                    // 下层三维顶点
                    var v1 = lowerContour.Vertices[j];
                    var v2 = lowerContour.Vertices[jNext];

                    // 上层三维顶点
                    var v3 = upperContour.Vertices[j];
                    var v4 = upperContour.Vertices[jNext];

                    // 查找全局索引（基于三维坐标匹配）
                    int idx1 = _globalVertices.FindIndex(v => v.Equals(v1));
                    int idx2 = _globalVertices.FindIndex(v => v.Equals(v2));
                    int idx3 = _globalVertices.FindIndex(v => v.Equals(v3));
                    int idx4 = _globalVertices.FindIndex(v => v.Equals(v4));

                    if (idx1 < 0 || idx2 < 0 || idx3 < 0 || idx4 < 0) continue;

                    // 拆分四边形为两个三维三角形（DSI定向细分）
                    _triangles.Add(new Triangle(idx1, idx2, idx3));
                    _triangles.Add(new Triangle(idx2, idx4, idx3));
                }
            }
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 为全局顶点分配连续索引
        /// </summary>
        private void AssignGlobalVertexIndices()
        {
            for (int i = 0; i < _globalVertices.Count; i++)
            {
                _globalVertices[i] = new Vertex3D(
                    _globalVertices[i].X,
                    _globalVertices[i].Y,
                    _globalVertices[i].Z,
                    i
                );
            }
        }
        #endregion
    }
    #endregion
    
}