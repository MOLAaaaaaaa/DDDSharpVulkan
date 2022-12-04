using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Poly2Tri;

namespace DataCollection.DelaunayVoronoi
{
    /// <summary>
    /// 多边形内三角剖分，凹多边形拆分法---该方法效果不好，已放弃2022-7-27
    /// 已采用Poly2Tri方法,Triangulate()
    /// </summary>
    public class PolygonTriangulate
    {
        Polygon2D Polygon;
        
        public PolygonTriangulate(Polygon2D _polygon)
        {
            Polygon = _polygon;
        }        
        
        /// <summary>
        /// 在XOY平面内剖分
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public TriangleObj Triangulate(Polygon2D _Polygon)
        {
            Polygon = _Polygon;
            if (Polygon == null || Polygon.Count < 3) return null;

            TriangleObj triangles = new TriangleObj();

            Poly2Tri.Polygon poly = new Polygon(_Polygon);
            P2T.Triangulate(poly);

            Vector32 p1 = new Vector32();
            Vector32 p2 = new Vector32();
            Vector32 p3 = new Vector32();
            float z = (float)Polygon[0].Z;
            float v = (float)Polygon[0].V;
            int n = 0;
            foreach (DelaunayTriangle tri in poly.Triangles )
            {
                p1 = new Vector32(tri.Points[0].Xf, tri.Points[0].Yf,z,v);
                p2 = new Vector32(tri.Points[1].Xf, tri.Points[1].Yf, z, v);
                p3 = new Vector32(tri.Points[2].Xf, tri.Points[2].Yf, z, v);
                triangles.AddPoint(p1);
                triangles.AddPoint(p2);
                triangles.AddPoint(p3);
                n = triangles.points.Count;
                triangles.AddTriangleIndex(n-3,n-2,n-1);
            }
            poly.Clear();
            return triangles;
        }
        
        /// <summary>
        /// 凸多边形三角剖分，在XOY平面内剖分
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public TriangleObj TriangulateConvexPoly( Polygon2D poly )
        {
            DelaunayTriangulator delaunay = new DelaunayTriangulator();

            Vector64 p;
            for (int i = 0; i < poly.Count; i++)
            {
                p = poly[i];
                delaunay.AddPoint(p.X, p.Y, p.Z);
            }

            if ( delaunay.Count > 3 )
            {
                delaunay.BowyerWatson();
                TriangleObj obj = delaunay.toTriangleObj();
                obj.UpdateRange();
                delaunay.Clear();
                return obj;
            }
            else return null;
        }
        
        /// <summary>
        /// 简单凸多边形三角剖分，在XOY平面内剖分
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public TriangleObj TriangulateConvexPolySimple(Polygon2D poly)
        {
            TriangleObj obj = new TriangleObj();
            int n = poly.Count;
            Vector64 p0 = poly.GetCenter64();
            Vector64 p;
            obj.AddPoint(p0);
            for(int i=0;i<n;i++)
            {
                p = poly[i];
                obj.AddPoint(p);
            }
            for (int i = 0; i < n; i++)
            {
                if(i < n-1 )obj.AddTriangleIndex(0, i + 1, i + 2 );
                else obj.AddTriangleIndex(0, i + 1, 1);
            }
            return obj;
        }
        

        bool IsInPolygon(double x,double y)
        {
            return Polygon.IsPointInsidePoly(x, y);
        }

        bool IsTriangleInPolygon(Point3D p1, Point3D p2, Point3D p3)
        {
            //if ( !Polygon.IsPointInsidePoly(p1.X, p1.Y) ) return false;
            //if ( !Polygon.IsPointInsidePoly(p2.X, p2.Y) ) return false;
            //if ( !Polygon.IsPointInsidePoly(p3.X, p3.Y) ) return false;
            double x0 = (p1.X + p2.X + p3.X) / 3.0;
            double y0 = (p1.Y + p2.Y + p3.Y) / 3.0;
            if ( !Polygon.IsPointInsidePoly(x0, y0) ) return false;
            
            int n = 5;
            double L = Math.Sqrt( (p1.X - x0)* (p1.X - x0) + (p1.Y - y0)* (p1.Y - y0) );
            double step = L / n;
            double x, y;
            
            //线段P0-P1上取n-1个点，判断是否在多边形内
            for(int i = 1; i < n; i++ )
            {
                x = x0 + i * (p1.X - x0) / n;
                y = y0 + i * (p1.Y - y0) / n;
                if (!Polygon.IsPointInsidePoly(x, y)) return false;
            }

            //线段P0-P2上取n-1个点，判断是否在多边形内
            for (int i = 1; i < n; i++)
            {
                x = x0 + i * (p2.X - x0) / n;
                y = y0 + i * (p2.Y - y0) / n;
                if (!Polygon.IsPointInsidePoly(x, y)) return false;
            }

            //线段P0-P3上取n-1个点，判断是否在多边形内
            for (int i = 1; i < n; i++)
            {
                x = x0 + i * (p3.X - x0) / n;
                y = y0 + i * (p3.Y - y0) / n;
                if (!Polygon.IsPointInsidePoly(x, y)) return false;
            }
            return true;
        }
    }
}
