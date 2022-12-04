using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace DataCollection
{
    public enum ObjectType2D
    {
        point = 0,
        line = 1,
        polygon = 2,
    }

    public class Object2DBase
    {
        public List<PointF> points = new List<PointF>();
        public ObjectType2D Type = ObjectType2D.line;
        
        virtual public void Draw(Graphics g) { }
        public void Add(PointF p) 
        {
            points.Add(p);
        }
        public void Add(double x,double y)
        {
            points.Add(new PointF((float)x, (float)y) );
        }
    }

    public class Point2D:Object2DBase
    {
        public Point2D()
        {
            Type = ObjectType2D.point;
        }
        override public void Draw(Graphics g) 
        {

        }
    }
}
