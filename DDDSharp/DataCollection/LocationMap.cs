using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//空间位置映射
namespace DataCollection
{
    public struct LocationMapPoint
    {
        public Vector64 point2D;
        public Vector64 point3D;        
        public LocationMapPoint(Vector64 p2d, Vector64 p3d)
        {
            point2D = p2d;
            point3D = p3d;
        }
    }
    public class LocationMap
    {
        public double minx2D, maxx2D, miny2D, maxy2D;
        public double minx3D, maxx3D, miny3D, maxy3D, minz3D, maxz3D;

        List<LocationMapPoint> Points = new List<LocationMapPoint>();
        //添加定位点
        public void AddMapPoint(Vector64 p2D, Vector64 p3D)
        {
            Points.Add(new LocationMapPoint(p2D,p3D));
        }
        public void UpdateRange()
        {
            LocationMapPoint p;
            for (int i=0;i< Points.Count;i++)
            {
                p = Points[i];
                if( i==0) 
                {
                    minx2D = maxx2D = p.point2D.x;
                    miny2D = maxy2D = p.point2D.y;
                    minx3D = maxx3D = p.point3D.x;
                    miny3D = maxy3D = p.point3D.y;
                    minz3D = maxz3D = p.point3D.z;
                }
                else
                {
                    if (p.point2D.x < minx2D) minx2D = p.point2D.x;
                    if (p.point2D.x > maxx2D) maxx2D = p.point2D.x;
                    if (p.point2D.y < miny2D) miny2D = p.point2D.y;
                    if (p.point2D.y > maxy2D) maxy2D = p.point2D.y;

                    if (p.point3D.x < minx3D) minx3D = p.point3D.x;
                    if (p.point3D.x > maxx3D) maxx3D = p.point3D.x;
                    if (p.point3D.y < miny3D) miny3D = p.point3D.y;
                    if (p.point3D.y > maxy3D) maxy3D = p.point3D.y;
                    if (p.point3D.z < minz3D) minz3D = p.point3D.z;
                    if (p.point3D.z > maxz3D) maxz3D = p.point3D.z;
                }
            }
        }

        public bool IsMapped { get { return Points.Count > 2; } }
        public double xWidth2D
        {
            get { return maxx2D - minx2D; }
        }
        public double yWidth2D
        {
            get { return maxy2D - miny2D; }
        }
        public double xWidth3D
        {
            get { return maxx3D - minx3D; }
        }
        public double yWidth3D
        {
            get { return maxy3D - miny3D; }
        }
        public double zWidth3D
        {
            get { return maxz3D - minz3D; }
        }

        //查找最近的三个点作为定位点
        bool GetTrianglePoints(double x,double y,out Vector64 p1, out Vector64 p2, out Vector64 p3)
        {
            if( Points.Count < 4 )
            {
                p1 = Points[0].point3D;
                p2 = Points[1].point3D;
                p3 = Points[2].point3D;
                return true;
            }
            p1 = new Vector64();
            p2 = new Vector64();
            p3 = new Vector64();
            //查找左右点
            return false;
        }

    }
}
