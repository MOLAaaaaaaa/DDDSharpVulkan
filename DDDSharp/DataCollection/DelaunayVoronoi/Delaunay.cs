using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
namespace DataCollection.DelaunayVoronoi
{
    public class DelaunayTriangulator
    {        
        private IEnumerable<Triangle> border;
        protected List<Point3D> Points = new List<Point3D>();
        public double minx, maxx, miny, maxy, minz, maxz;
        private bool boderCreated = false;
        double minxZ = 0;
        double minyZ = 0;
        double minzZ = 0;
        double maxxZ = 0;
        double maxyZ = 0;
        double maxzZ = 0;
        private HashSet<Triangle> _Triangles;
        public IEnumerable<Triangle> Triangles 
        {
            get { return _Triangles; }
        }

        public void AddPoint(Point3D p)
        {
            Points.Add(p);
        }
        public void AddPoint(double x,double y,double z)
        {
            Points.Add(new Point3D(x,y,z));
        }
        public int Count { get { return Points.Count; } }
        public void Clear()
        {
            Points.Clear();
            _Triangles.Clear();
        }
        public void UpdateRange()
        {
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                {
                    minx = maxx = Points[i].X;
                    miny = maxy = Points[i].Y;
                    minz = maxz = Points[i].Z;
                }
                else
                {
                    if (minx > Points[i].X) { minx = Points[i].X; minxZ = Points[i].Z; }
                    if (miny > Points[i].Y) { miny = Points[i].Y; minyZ = Points[i].Z; }
                    if (minz > Points[i].Z) { minz = Points[i].Z; minzZ = Points[i].Z; }
                    if (maxx < Points[i].X) { maxx = Points[i].X; maxxZ = Points[i].Z; }
                    if (maxy < Points[i].Y) { maxy = Points[i].Y; maxyZ = Points[i].Z; }
                    if (maxz < Points[i].Z) { maxz = Points[i].Z; maxzZ = Points[i].Z; }
                }
            }
        }
        int leftTop = -1;
        int rightTop = -1;
        int leftBottom = -1;
        int rightBottom = -1;
        /// <summary>
        /// 查找4个角点的在Points里面的索引
        /// </summary>
        /// <returns></returns>
        int GetBoderCornersIndex()
        {
            leftTop = rightTop = -1;
            leftBottom = rightBottom = -1;
            Point3D p;
            int n = 0;
            for (int i = 4; i < Points.Count; i++)
            {
                p = Points[i];
                if (leftTop < 0 && p.X == (float)minx && p.Y == (float)miny)
                {
                    leftTop = i; n++;
                }
                if (leftBottom < 0 && p.X == (float)minx && p.Y == (float)maxy)
                {
                    leftBottom = i; n++;
                }
                if (rightTop < 0 && p.X == (float)maxx && p.Y == (float)miny)
                {
                    rightTop = i; n++;
                }
                if (rightBottom < 0 && p.X == (float)maxx && p.Y == (float)maxy)
                {
                    rightBottom = i; n++;
                }
            }
            return n;
        }
        public bool CreateBorder()
        {
            if (boderCreated) return boderCreated;

            // TODO make more beautiful
            UpdateRange();
            
            var point0 = new Point3D(minx, miny, (minz + maxz) / 2);
            var point1 = new Point3D(minx, maxy, (minz + maxz) / 2);
            var point2 = new Point3D(maxx, maxy, (minz + maxz) / 2);
            var point3 = new Point3D(maxx, miny, (minz + maxz) / 2);
            
            notPolyVertices.Clear();
            if (!IsPolyVertice(point0)) notPolyVertices.Add(point0);
            if (!IsPolyVertice(point1)) notPolyVertices.Add(point1);
            if (!IsPolyVertice(point2)) notPolyVertices.Add(point2);
            if (!IsPolyVertice(point3)) notPolyVertices.Add(point3);

           // Points.Insert(0, point3);
           // Points.Insert(0, point2);
           // Points.Insert(0, point1);
           // Points.Insert(0, point0);
            
            var tri1 = new Triangle(point0, point1, point2);
            var tri2 = new Triangle(point0, point2, point3);
            border = new List<Triangle>() { tri1, tri2 };
            boderCreated = true;

            //GetBoderCornersIndex();

            return boderCreated;
        }

        List<Point3D> notPolyVertices = new List<Point3D>();
        bool IsPolyVertice(Point3D p)
        {
            foreach(Point3D v in Points)
            {
                if (p.X == v.X && p.Y == v.Y) 
                    return true;
            }
            return false;
        }
        bool IsRemoveNeeded(Point3D p)
        {
            foreach (Point3D v in notPolyVertices)
            {
                if (p.X == v.X && p.Y == v.Y)
                    return true;
            }
            return false;
        }
        public bool IsCornerPoint(Point3D v)
        {
            Point3D p;
            //前4个点是角点
            for(int i = 0; i < 4; i++ )
            {
                p = Points[i];
                if (v.X == p.X && v.Y == p.Y) return true;
            }
            return false;            
        }

        public virtual TriangleObj toTriangleObj()
        {
            TriangleObj obj = new TriangleObj();
            CColorScale colorscale = new CColorScale(minz,maxz);
            Point3D p1,p2,p3;            
            int n = 0;
            foreach( Triangle tri in Triangles)
            {
                p1 = tri.Vertices[0];
                p2 = tri.Vertices[1];
                p3 = tri.Vertices[2];
                //if (IsRemoveNeeded(p1)) continue;
                //if (IsRemoveNeeded(p2)) continue;
                //if (IsRemoveNeeded(p3)) continue;
                obj.AddPoint(p1.X, p1.Y, p1.Z, p1.Z);
                obj.AddPoint(p2.X, p2.Y, p2.Z, p2.Z);
                obj.AddPoint(p3.X, p3.Y, p3.Z, p3.Z);
                obj.AddPointColor(colorscale.GetColor(p1.Z));
                obj.AddPointColor(colorscale.GetColor(p2.Z));
                obj.AddPointColor(colorscale.GetColor(p3.Z));

                n += 3;

                obj.AddTriangleIndex(n - 3, n - 2, n - 1);
            }            
            return obj;
        }
        public IEnumerable<Triangle> BowyerWatson()
        {
            CreateBorder();//构建2个超级三角形

            //var supraTriangle = GenerateSupraTriangle();
           
            _Triangles = new HashSet<Triangle>(border);

            if ( Points.Count < 4 ) return _Triangles;
            
            Point3D point;
            for (int i = 0; i < Points.Count; i++ )
            {
                point = Points[i];
                var badTriangles = FindBadTriangles(point, _Triangles);
                var polygon = FindHoleBoundaries(badTriangles);

                foreach (var triangle in badTriangles)
                {
                    foreach (var vertex in triangle.Vertices)
                    {
                        vertex.AdjacentTriangles.Remove(triangle);
                    }
                }
                _Triangles.RemoveWhere(o => badTriangles.Contains(o));

                foreach (var edge in polygon.Where(possibleEdge => possibleEdge.Point1 != point && possibleEdge.Point2 != point))
                {
                    var triangle = new Triangle(point, edge.Point1, edge.Point2);
                    if(!triangle.Invalid)
                    _Triangles.Add(triangle);
                }
            }

            //triangulation.RemoveWhere(o => o.Vertices.Any(v => supraTriangle.Vertices.Contains(v)));
            return _Triangles;
        }

        private List<Edge> FindHoleBoundaries(ISet<Triangle> badTriangles)
        {
            var edges = new List<Edge>();
            foreach (var triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }
            var grouped = edges.GroupBy(o => o);
            var boundaryEdges = edges.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First());
            return boundaryEdges.ToList();
        }      

        private ISet<Triangle> FindBadTriangles(Point3D point, HashSet<Triangle> triangles)
        {
            var badTriangles = triangles.Where(o => o.IsPointInsideCircumcircle(point));
            return new HashSet<Triangle>(badTriangles);
        }
    }
}