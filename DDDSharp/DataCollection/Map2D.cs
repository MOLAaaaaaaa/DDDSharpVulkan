using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphics3D;
/// <summary>
/// 2D 地图类
/// </summary>
namespace DataCollection
{
    public enum MapPointEnum
    {
        Box = 0,
        Ball = 1,
        Cross = 2,
        Arrow = 3,
    }
    public enum MapTypeEnum
    {
        Point = 0,
        Line = 1,
        Polygon = 2,        
    }
    public class MapBase
    {
        //z is the elevation here
#pragma warning disable CS0169 // 从不使用字段“MapBase.Minz”
#pragma warning disable CS0169 // 从不使用字段“MapBase.Maxy”
#pragma warning disable CS0169 // 从不使用字段“MapBase.Maxz”
#pragma warning disable CS0169 // 从不使用字段“MapBase.Minx”
#pragma warning disable CS0169 // 从不使用字段“MapBase.Miny”
#pragma warning disable CS0169 // 从不使用字段“MapBase.Maxx”
        double Minx, Maxx, Miny, Maxy, Minz, Maxz;
#pragma warning restore CS0169 // 从不使用字段“MapBase.Maxx”
#pragma warning restore CS0169 // 从不使用字段“MapBase.Miny”
#pragma warning restore CS0169 // 从不使用字段“MapBase.Minx”
#pragma warning restore CS0169 // 从不使用字段“MapBase.Maxz”
#pragma warning restore CS0169 // 从不使用字段“MapBase.Maxy”
#pragma warning restore CS0169 // 从不使用字段“MapBase.Minz”
#pragma warning disable CS0169 // 从不使用字段“MapBase.Elevation1”
#pragma warning disable CS0169 // 从不使用字段“MapBase.Elevation2”
        double Elevation1, Elevation2;
#pragma warning restore CS0169 // 从不使用字段“MapBase.Elevation2”
#pragma warning restore CS0169 // 从不使用字段“MapBase.Elevation1”
        public MapTypeEnum MapType = MapTypeEnum.Line;
        public String Name { get; set; } = "Untitled";
        public List<Vector64> Points = new List<Vector64>();
    }
    public class MapPoint: MapBase
    {
        public Color PointColor { get; set; } = Color.Black;
        public MapPointEnum PointType { get; set; } = MapPointEnum.Cross;
        public double Size { get; set; } = 0.001;
        public MapPoint() 
        {
            MapType = MapTypeEnum.Point;
        }
    }

    public class MapLine : MapBase
    {
        public float LineWidth { get; set; } = 0.1f;
        public gLineStyle Style { get; set; } = gLineStyle.Solid;
        public Color LineColor { get; set; } = Color.Black;
        
        public MapLine()
        {
            MapType = MapTypeEnum.Line;
        }
    }

    public class MapPolygon : MapBase
    {
        public float LineWidth { get; set; } = 0.1f;
        public gLineStyle Style { get; set; } = gLineStyle.Solid;
        public Color LineColor { get; set; } = Color.Black;
        public Color FillColor { get; set; } = Color.Blue;
        public bool IsFilled { get; set; } = true;
        public bool IsDrawBoder { get; set; } = true;
        public MapPolygon()
        {
            MapType = MapTypeEnum.Polygon;
        }
    }

    public class Map2D: C3DObjectBase
    {
        public List<MapBase> MapObjects = new List<MapBase>();

        //  |z
        //  |  /y 
        //  | /
        //  o ----------->x
        public bool IsLocated 
        {
            get
            { return false; }
        }
    }
}
