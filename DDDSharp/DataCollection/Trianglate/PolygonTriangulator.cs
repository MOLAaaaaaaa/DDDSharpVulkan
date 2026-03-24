using DataCollection;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.Polygon;
using NetTopologySuite.Triangulate.Tri;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDSharp.DataCollection.Trianglate
{
    public class PolygonTriangulator
    {
        public string errMessage = "";
        IList<Tri>Triangles = new List<Tri>();
        private const double Epsilon = 1e-6;
        private static double CrossProduct(Vector64 a, Vector64 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        private List<Vector64> RemoveDuplicateVertices(List<Vector64> vertices)
        {
            var unique = new List<Vector64>();
            foreach (var point in vertices)
            {
                // 检查当前点是否已存在（允许微小误差）
                if (!unique.Any(p => p.Equals(point)))
                {
                    unique.Add(point);
                }
            }
            return unique;
        }
        private List<Vector64> RemoveCollinearVertices(List<Vector64> vertices)
        {
            if (vertices.Count < 3)
            {
                return vertices;
            }

            var filtered = new List<Vector64>();
            int n = vertices.Count;

            for (int i = 0; i < n; i++)
            {
                Vector64 prev = vertices[(i - 1 + n) % n];
                Vector64 curr = vertices[i];
                Vector64 next = vertices[(i + 1) % n];

                // 计算三个连续顶点的叉积，叉积为0说明共线，跳过当前顶点
                double cross = CrossProduct(next - prev,curr - prev);
                if (Math.Abs(cross) > Epsilon)
                {
                    filtered.Add(curr);
                }
            }

            // 过滤后如果不足3个顶点，保留原始顶点（避免过度过滤）
            return filtered.Count >= 3 ? filtered : vertices;
        }
        /// 
        /// <param name="vertices">多边形顶点列表</param>
        /// <returns>true=自交，false=不自交</returns>
        private bool IsSelfIntersecting(List<Vector64> vertices)
        {
            int n = vertices.Count;
            // 遍历所有边对（i, i+1）和（j, j+1），判断是否相交（排除相邻边、首尾边）
            for (int i = 0; i < n; i++)
            {
                Vector64 a1 = vertices[i];
                Vector64 a2 = vertices[(i + 1) % n];

                for (int j = i + 2; j < n; j++)
                {
                    // 跳过相邻边（i和i+1，j和j+1），以及首尾边（i=0和j=n-1）
                    if (i == 0 && j == n - 1)
                    {
                        continue;
                    }

                    Vector64 b1 = vertices[j];
                    Vector64 b2 = vertices[(j + 1) % n];

                    // 判断两条线段是否相交（包含端点接触不算自交）
                    if (DoSegmentsIntersect(a1, a2, b1, b2,true))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// 
        /// <param name="a1">线段A起点</param>
        /// <param name="a2">线段A终点</param>
        ///<param name="b1">线段B起点</param>
        /// <param name="b2">线段B终点</param>
        /// <param name="includeEndpoints">是否包含端点接触</param>
        /// <returns>true=相交，false=不相交</returns>
        private bool DoSegmentsIntersect(Vector64 a1, Vector64 a2, Vector64 b1, Vector64 b2, bool includeEndpoints)
        {
            // 计算叉积，判断线段相对位置
            double cross1 = CrossProduct(a2 - a1, b1 - a1);
            double cross2 = CrossProduct(a2 - a1, b2 - a1);
            double cross3 = CrossProduct(b2 - b1, a1 - b1);
            double cross4 = CrossProduct(b2 - b1, a2 - b1);

            // 跨立实验：两条线段互相跨立（叉积异号），则相交
            bool crossIntersect = (cross1 * cross2 < -Epsilon) && (cross3 * cross4 < -Epsilon);

            if (crossIntersect)
            {
                return true;
            }

            // 处理端点接触的情况（如果需要包含端点）
            if (includeEndpoints)
            {
                // 检查线段A的端点是否在线段B上
                bool a1OnB = IsPointOnSegment(a1, b1, b2);
                bool a2OnB = IsPointOnSegment(a2, b1, b2);
                // 检查线段B的端点是否在线段A上
                bool b1OnA = IsPointOnSegment(b1, a1, a2);
                bool b2OnA = IsPointOnSegment(b2, a1, a2);

                return a1OnB || a2OnB || b1OnA || b2OnA;
            }

            return false;
        }

        /// 
        private bool IsPointOnSegment(Vector64 point, Vector64 segmentStart, Vector64 segmentEnd)
        {
            // 1. 点与线段共线（叉积为0）
            if (Math.Abs(CrossProduct(segmentEnd - segmentStart, point - segmentStart)) > Epsilon)
            {
                return false;
            }

            // 2. 点的坐标在segmentsStart和segmentEnd之间（x、y均在范围内）
            double minX = Math.Min(segmentStart.X, segmentEnd.X) - Epsilon;
            double maxX = Math.Max(segmentStart.X, segmentEnd.X) + Epsilon;
            double minY = Math.Min(segmentStart.Y, segmentEnd.Y) - Epsilon;
            double maxY = Math.Max(segmentStart.Y, segmentEnd.Y) + Epsilon;

            return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
        }

        List<Vector64> Preprocess(Polygon2D poly)
        {
            // 1. 基础校验：顶点数量至少为3个
            if (poly == null || poly.Count < 3)
            {
                errMessage = "多边形顶点数量不能少于3个！";
                return null;                
            }

            // 2. 去重：移除重复顶点（连续或非连续重复）
            var uniqueVertices = RemoveDuplicateVertices(poly.points);
            if (uniqueVertices.Count < 3)
            {
                errMessage = "多边形顶点去重后不足3个，无法构成有效多边形！";
                return null;
            }

            // 3. 过滤连续共线顶点（避免退化三角形）
            var filteredVertices = RemoveCollinearVertices(uniqueVertices);
            if (filteredVertices.Count < 3)
            {
                errMessage = "多边形过滤共线顶点后不足3个，无法构成有效多边形！！";
                return null;
            }

            // 4. 检查多边形是否自交（自交多边形无法剖分）
            if (IsSelfIntersecting(filteredVertices))
            {
                errMessage = "多边形存在自交，无法进行三角剖分！";
                return null;
            }
            return filteredVertices;
        }

        public bool CreateTriangles( Polygon2D poly)
        {
            try
            {
                Triangles.Clear();
                List<Vector64> filtred = Preprocess(poly);
                if (filtred == null) { return false; }

                List<NetTopologySuite.Geometries.Coordinate> coordinates = new List<NetTopologySuite.Geometries.Coordinate>();
                for (int i = 0; i < filtred.Count; i++)
                {
                    NetTopologySuite.Geometries.Coordinate p = new NetTopologySuite.Geometries.Coordinate(filtred[i].X, filtred[i].Y);
                    coordinates.Add(p);
                }
                coordinates.Add(coordinates[0]);

                // 1. 构建NTS 2.6.0 闭合环（LinearRing）
                var linearRing = new LinearRing(coordinates.ToArray());
                coordinates.Clear();

                // 2. 构建Polygon对象（无孔洞，2.6.0支持直接传入LinearRing，无需额外处理）
                var polygon = new Polygon(linearRing);
                // 3. 调用TriangulatePolygon方法            
                NetTopologySuite.Triangulate.Polygon.PolygonTriangulator trianglator = new NetTopologySuite.Triangulate.Polygon.PolygonTriangulator(polygon);

                //ConstrainedDelaunayTriangulator trianglator = new ConstrainedDelaunayTriangulator(polygon);
                Triangles = trianglator.GetTriangles();
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
            
        }
        public List<Vector32> toTriangleCoords()
        {
            List<Vector32> points = new List<Vector32>();
            for (int i = 0; i < Triangles.Count; i++)
            {
                var tri = Triangles[i];
                NetTopologySuite.Geometries.Coordinate p1 = tri.GetCoordinate(0);
                NetTopologySuite.Geometries.Coordinate p2 = tri.GetCoordinate(1);
                NetTopologySuite.Geometries.Coordinate p3 = tri.GetCoordinate(2);
                points.Add(new Vector32(p1.X, p1.Y, 0));
                points.Add(new Vector32(p2.X, p2.Y, 0));
                points.Add(new Vector32(p3.X, p3.Y, 0));
            }
            return points;
        }
    }
}
