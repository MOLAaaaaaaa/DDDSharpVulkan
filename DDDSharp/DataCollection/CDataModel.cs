using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Integration;

/***************************************
 * 
 * Data Models Management
 * update all 3d data size and
 * Convert Logical coords to GL coords
 * 
 * 1.use UpdateModelSize(x1,y1,z1,x2,y2,z2) when add 3D Objects 
 * to List, and then 
 * 
 * 2.use ToModelVector64(p) to convert Logical point to 
 * Graphical Coords(GL)
 * 
 * **************************************/
namespace DataCollection
{
    public enum CoordinateSys
    {
      //    | Y
      //    |_____X
      //   /
      //  Z
        OPENGL_XYZ = 0,
        //    ____east (x)
        //   /|
        // Z/ |Y
        VULKAN_XYZ = 1,
        //  top(z)
        //  | 
        //  | / north(y)
        //  |/____east (x)
        GEO_XYZ = 2,
    };
    static class CDataModel
    {
        static public float BlankValue = 1.123456789E20F;
        static public CubeModel64 m_ModelOrg = new CubeModel64(-1, -1, -1, 1, 1, 1);
        static public CubeModel64 m_Model = new CubeModel64(-1, -1, -1, 1, 1, 1);
        static public bool rangeUpdated = false;
        //模型比例变换因子，统一将模型变换到最大为1的范围
        static public double m_ModelScale = 1.0;
        static private Vector64 m_ModelCenter = new Vector64(0,0,0);
        static private Vector64 m_ModelCorner0 = new Vector64(0, 0, 0);
        static private Vector64 m_ModelCorner1 = new Vector64(0, 0, 0);
        static private CoordinateSys m_CoordSys = CoordinateSys.OPENGL_XYZ;
        static public void SetCoordSys(CoordinateSys coord) { m_CoordSys = coord; }
        static public CoordinateSys GetCoordSys() { return m_CoordSys; }

        static public Axis3DArrow xArrow3D = new Axis3DArrow("x");
        static public Axis3DArrow yArrow3D = new Axis3DArrow("y");
        static public Axis3DArrow zArrow3D = new Axis3DArrow("z");
        static public Axis3DArrow xGeoArrow3D = new Axis3DArrow("North");
        static public Axis3DArrow yGeoArrow3D = new Axis3DArrow("East");
        static public Axis3DArrow zGeoArrow3D = new Axis3DArrow("Depth");
        static public Axis3DArrow xEarthArrow3D = new Axis3DArrow("");
        static public Axis3DArrow yEarthArrow3D = new Axis3DArrow("");
        static public Axis3DArrow zEarthArrow3D = new Axis3DArrow("N");

        static public Axis3DRuler xAxisRuler = new Axis3DRuler(AxisEnum.xAxis);
        static public Axis3DRuler yAxisRuler = new Axis3DRuler(AxisEnum.yAxis);
        static public Axis3DRuler zAxisRuler = new Axis3DRuler(AxisEnum.zAxis);

        //球面坐标
        static public double earthRadius = 6378137;
        static public double MaxEarthRadius = 6378137;
        static public bool IsEarthMapVision = false;    //是否采用球面坐标系统显示
        static public bool IsGeoCoordinateSystem = false;    //是否采用向下坐标系
        //    IsGeoCoordinateSystem = false
        //    Z
        //    |  /Y(North)
        //    | / 
        //    O ------->X(east)
        
        //    IsGeoCoordinateSystem = true
        //    
        //      /X(North)
        //     / 
        //    O ------->Y(east)
        //    |
        //    |
        //    Depth
        
        //static public MapDataBase mapDataBase = new MapDataBase();

        //球面坐标投影到半径为1的球面
        static public Vector64 toEarthModelVector(Vector64 p)
        {
            return p / MaxEarthRadius*0.5;
        }
        static public Vector64 toEarthModelVector(Vector32 p)
        {
            return toEarthModelVector(p.toVector64());
        }
        static public Vector64 toEarthModelVector(EarthVector p)
        {
            return p.toXYZVector(earthRadius) / MaxEarthRadius;
        }
        public static bool ChooseXCoordinate(string s)
        {
            char[] chars = new char[] { ' ', '\t', '"', '\'' };
            string s1 = s.Trim(chars).ToLower();
            if ( IsGeoCoordinateSystem )
            {
                if(s1=="y" || s1.Contains("north"))
                {
                    return true;
                }
            }
            else
            {
                if (s1=="x" || s1.Contains("east"))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool ChooseYCoordinate(string s)
        {
            char[] chars = new char[] { ' ', '\t', '"', '\'' };
            string s1 = s.Trim(chars).ToLower();
            if (IsGeoCoordinateSystem)
            {
                if ( s1== "x" || s1.Contains("east"))
                {
                    return true;
                }
            }
            else
            {
                if (s1 =="y" || s1.Contains("north"))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool ChooseZCoordinate(string s)
        {
            char[] chars = new char[] { ' ', '\t', '"', '\'' };
            string s1 = s.Trim(chars).ToLower();
            if ( s1=="z" || 
                 s1.Contains("depth")|| 
                 s1.Contains("eleva") ||
                 s1.Contains("高程")||
                 s1.Contains("深度") )
            {
                return true;
            }
            return false;
        }
        public static bool ChooseVCoordinate(string s)
        {
            char[] chars = new char[]{' ', '\t', '"','\'' };
            string s1 = s.Trim(chars).ToLower();
            if ( s1 == "v" ||
                 s1.Contains("val") ||
                 s1.Contains("value") ||
                 s1.Contains("值") ||
                 s1.Contains("电阻率") ||
                 s1.Contains("密度")||
                 s1.Contains("磁化率")||
                 s1.Contains("速度")||
                 s1.Contains("波阻抗")||
                 s1.Contains("波速") )
            {
                return true;
            }
            return false;
        }
        static public bool IsInitialized()
        {
            if (m_ModelOrg.X1 == -1 && m_ModelOrg.X2 == 1 &&
                m_ModelOrg.Y1 == -1 && m_ModelOrg.Y2 == 1 &&
                m_ModelOrg.Z1 == -1 && m_ModelOrg.Z2 == 1) return false;
            return true;
        }

        //version 1.23
        static public bool SaveDataModel(BinaryWriter br)
        {
            br.Write(m_ModelOrg.X1);
            br.Write(m_ModelOrg.X2);
            br.Write(m_ModelOrg.Y1);
            br.Write(m_ModelOrg.Y2);
            br.Write(m_ModelOrg.Z1);
            br.Write(m_ModelOrg.Z2);

            br.Write(m_Model.X1);
            br.Write(m_Model.X2);
            br.Write(m_Model.Y1);
            br.Write(m_Model.Y2);
            br.Write(m_Model.Z1);
            br.Write(m_Model.Z2);

            br.Write(m_ModelScale);
            br.Write(m_ModelCenter.x);
            br.Write(m_ModelCenter.y);
            br.Write(m_ModelCenter.z);

            br.Write(IsEarthMapVision);
            br.Write(IsGeoCoordinateSystem);//向下坐标系

            xArrow3D.Save(br);
            yArrow3D.Save(br);
            zArrow3D.Save(br);
            xGeoArrow3D.Save(br);
            yGeoArrow3D.Save(br);
            zGeoArrow3D.Save(br);
            xEarthArrow3D.Save(br);
            yEarthArrow3D.Save(br);
            zEarthArrow3D.Save(br);

            xAxisRuler.Save(br);
            yAxisRuler.Save(br);
            zAxisRuler.Save(br);

            return true;
        }
        static public bool SaveDataModel_122(BinaryWriter br)
        {
            br.Write(m_ModelOrg.X1);
            br.Write(m_ModelOrg.X2);
            br.Write(m_ModelOrg.Y1);
            br.Write(m_ModelOrg.Y2);
            br.Write(m_ModelOrg.Z1);
            br.Write(m_ModelOrg.Z2);

            br.Write(m_Model.X1);
            br.Write(m_Model.X2);
            br.Write(m_Model.Y1);
            br.Write(m_Model.Y2);
            br.Write(m_Model.Z1);
            br.Write(m_Model.Z2);

            br.Write(m_ModelScale);
            br.Write(m_ModelCenter.x);
            br.Write(m_ModelCenter.y);
            br.Write(m_ModelCenter.z);

            float fontscale = xArrow3D.labelSize;
            string xText = xArrow3D.AxisName;//笛卡尔坐标系
            string yText = yArrow3D.AxisName;
            string zText = zArrow3D.AxisName;
            if (IsEarthMapVision)
            {
                fontscale = xEarthArrow3D.labelSize;
                xText = xEarthArrow3D.AxisName;
                yText = yEarthArrow3D.AxisName;
                zText = zEarthArrow3D.AxisName;
            }
            else
            {
                if (!IsGeoCoordinateSystem)
                {
                    fontscale = xGeoArrow3D.labelSize;
                    xText = xGeoArrow3D.AxisName;
                    yText = xGeoArrow3D.AxisName;
                    zText = xGeoArrow3D.AxisName;
                }
            }

            br.Write(IsEarthMapVision);
            br.Write(fontscale);
            br.Write(IsGeoCoordinateSystem);//向下坐标系
            C3DData.SaveString(br, xText);
            C3DData.SaveString(br, yText);
            C3DData.SaveString(br, zText);

            return true;
        }       
         
        static public bool LoadDataModel_122(BinaryReader br)
        {
            m_ModelOrg = new CubeModel64();
            m_ModelOrg.X1 = br.ReadDouble();
            m_ModelOrg.X2 = br.ReadDouble();
            m_ModelOrg.Y1 = br.ReadDouble();
            m_ModelOrg.Y2 = br.ReadDouble();
            m_ModelOrg.Z1 = br.ReadDouble();
            m_ModelOrg.Z2 = br.ReadDouble();

            m_Model = new CubeModel64();
            m_Model.X1 = br.ReadDouble();
            m_Model.X2 = br.ReadDouble();
            m_Model.Y1 = br.ReadDouble();
            m_Model.Y2 = br.ReadDouble();
            m_Model.Z1 = br.ReadDouble();
            m_Model.Z2 = br.ReadDouble();
            m_ModelScale = br.ReadDouble();

            m_ModelCenter = new Vector64();
            m_ModelCenter.x = br.ReadDouble();
            m_ModelCenter.y = br.ReadDouble();
            m_ModelCenter.z = br.ReadDouble();

            IsEarthMapVision = br.ReadBoolean();
            float fontscale = br.ReadSingle();
            IsGeoCoordinateSystem = br.ReadBoolean();
            string xtext = C3DData.LoadString(br);
            string ytext = C3DData.LoadString(br);
            string ztext = C3DData.LoadString(br);

            if (IsEarthMapVision)//球坐标系，与原来版本兼容
            {
                xEarthArrow3D.labelSize = fontscale;
                xEarthArrow3D.AxisName = xtext;
                yEarthArrow3D.AxisName = ytext;
                zEarthArrow3D.AxisName = ztext;
            }
            else
            {
                if (!IsGeoCoordinateSystem)
                {
                    xArrow3D.labelSize = fontscale;
                    xArrow3D.AxisName = xtext;
                    xArrow3D.AxisName = ytext;
                    xArrow3D.AxisName = ztext;
                }
                else
                {
                    xGeoArrow3D.labelSize = fontscale;
                    xGeoArrow3D.AxisName = xtext;
                    xGeoArrow3D.AxisName = ytext;
                    xGeoArrow3D.AxisName = ztext;
                }
            }
            CalculateModelSize();
            return true;
        }
        static public bool LoadDataModel(BinaryReader br)
        {
            if(C3DData.DataVersion >= 1.23f)
                return LoadDataModel_123(br);
            else return LoadDataModel_122(br);
        }
        static public bool LoadDataModel_123(BinaryReader br)
        {
            m_ModelOrg = new CubeModel64();
            m_ModelOrg.X1 = br.ReadDouble();
            m_ModelOrg.X2 = br.ReadDouble();
            m_ModelOrg.Y1 = br.ReadDouble();
            m_ModelOrg.Y2 = br.ReadDouble();
            m_ModelOrg.Z1 = br.ReadDouble();
            m_ModelOrg.Z2 = br.ReadDouble();

            m_Model = new CubeModel64();
            m_Model.X1 = br.ReadDouble();
            m_Model.X2 = br.ReadDouble();
            m_Model.Y1 = br.ReadDouble();
            m_Model.Y2 = br.ReadDouble();
            m_Model.Z1 = br.ReadDouble();
            m_Model.Z2 = br.ReadDouble();
            m_ModelScale = br.ReadDouble();

            m_ModelCenter = new Vector64();
            m_ModelCenter.x = br.ReadDouble();
            m_ModelCenter.y = br.ReadDouble();
            m_ModelCenter.z = br.ReadDouble();

            IsEarthMapVision = br.ReadBoolean();           
            IsGeoCoordinateSystem = br.ReadBoolean();
            //坐标轴箭头
            xArrow3D.Load(br);
            yArrow3D.Load(br);
            zArrow3D.Load(br);
            xGeoArrow3D.Load(br);
            yGeoArrow3D.Load(br);
            zGeoArrow3D.Load(br);
            xEarthArrow3D.Load(br);
            yEarthArrow3D.Load(br);
            zEarthArrow3D.Load(br);
            //坐标轴刻度
            xAxisRuler.Load(br);
            yAxisRuler.Load(br);
            zAxisRuler.Load(br);

            CalculateModelSize();
            return true;
        }

        //-------convert logical coords to drawing coords----------------
        static public Vector32 CovertCoordSys(Vector32 p,
                                CoordinateSys source = CoordinateSys.GEO_XYZ,
                                CoordinateSys dest = CoordinateSys.OPENGL_XYZ)
        {
            if (source == CoordinateSys.GEO_XYZ && dest == CoordinateSys.OPENGL_XYZ)
                return CovertCoordGeoToGL(p);
            //else if(..)   //need to expanded

            return p;
        }
        static public Vector64 CovertCoordSys(Vector64 p,
                                CoordinateSys source = CoordinateSys.GEO_XYZ,
                                CoordinateSys dest = CoordinateSys.OPENGL_XYZ)
        {
            if (source == CoordinateSys.GEO_XYZ && dest == CoordinateSys.OPENGL_XYZ)
                return CovertCoordGeoToGL(p);
            //else if(..)   //need to expanded

            return p;
        }
        static public Vector32 CovertCoordGeoToGL(Vector32 p)
        {
            return new Vector32(p.X, p.Y, p.Z);
        }
        static public Vector64 CovertCoordGeoToGL(Vector64 p)
        {
            return new Vector64(p.X, p.Y, p.Z);
        }

        // ------Convert logical 3D point to Drawing point------
        static public double ToModelVector(double length)
        {
            return length / m_ModelScale;
        }
        static public Vector64 ToModelVector(Vector64 p)
        {
            return ToModelVector64(p);
        }
        static public Vector32 ToModelVector(Vector32 p)
        {
            return ToModelVector64(p.toVector64());
        }        
        static public Vector32 ToModelVector32(Vector32 p)
        {
            return ToModelVector64(p.toVector64());
        }
        /// <summary>
        /// 对象坐标转换成世界模型坐标
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        static public Vector64 ToModelVector64(Vector64 p)
        {
            if (IsEarthMapVision) return toEarthModelVector(p);
            else
            {
                Vector64 p1 = p - m_ModelCenter;
                p1 = 1.0f * p1 / m_ModelScale;
                return p1;
            }
        }
        // ------Convert Model 3D point to world point------
        static public Vector32 FromModelVector(Vector32 p)
        {   
            Vector32 p1 = p * (float)m_ModelScale;
            p1 = p1 + (Vector32)m_ModelCenter;
            return p1;
        }        
        static public Vector64 FromModelVector(Vector64 p)
        {
            Vector64 p1 = p * m_ModelScale;
            p1 = p1 + m_ModelCenter;
            return p1;
        }
        static public double FromModelVector(double length)
        {
            return length * m_ModelScale;
        }
        static public void ToModelTriangle(CTriangle3f p)
        {
            if (!p.bIsConverted)
            {
                p.p1 = ToModelVector64(p.p1);
                p.p2 = ToModelVector64(p.p2);
                p.p3 = ToModelVector64(p.p3);
                p.bIsConverted = true;
            }
        }
        
        //检查图形比例是否合适，小于超过0.01显示效果不好，提示
        static public bool IsModelScaleAcceptable(double minvalue)
        {
            double minscale = m_ModelOrg.GetWidth(0) / m_ModelScale;
            if (minscale > (m_ModelOrg.GetWidth(1) / m_ModelScale))
                minscale = m_ModelOrg.GetWidth(1) / m_ModelScale;
            if (minscale > (m_ModelOrg.GetWidth(2) / m_ModelScale))
                minscale = m_ModelOrg.GetWidth(2) / m_ModelScale;

            if (minscale > minvalue)return true;
            else return false;
        }
        static public void CalculateModelSize()
        {
            //计算m_ModelScale Based on m_ModelOrg            
            m_Model = m_ModelOrg.Copy();

            m_ModelCenter = m_Model.GetCenter();
            
            m_ModelCorner0 = new Vector64(m_Model.X1, m_Model.Y1, m_Model.Z1);
            m_ModelCorner1 = new Vector64(m_Model.X2, m_Model.Y2, m_Model.Z2);
            
            m_ModelScale = m_ModelOrg.MaxLength;
        }
        static public bool UpdateModelSize(double x1,double y1,double z1,double x2,double y2,double z2)
        {
            Vector64 p1 = new Vector64(x1, y1, z1);
            Vector64 p2 = new Vector64(x2, y2, z2);
            p1 = CDataModel.CovertCoordGeoToGL(p1);
            p2 = CDataModel.CovertCoordGeoToGL(p2);
            
            rangeUpdated = false;

            CubeModel64 cube = new CubeModel64(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z);
            //两个物体比例差别太大，重置，以当前物体比例为准
            if (m_ModelOrg.IsIdentityCube ||
                 m_ModelOrg.MaxLength <= 0.0 ||
                 cube.MaxLength / m_ModelOrg.MaxLength > 50.0 ||
                 cube.MaxLength / m_ModelOrg.MaxLength < 0.02)
            {
                m_ModelOrg = cube.toCentricCube();
                rangeUpdated = true;
            }
            else
            {   
                if( m_ModelOrg.X1 > p1.X) { m_ModelOrg.X1 = p1.X; rangeUpdated = true; }
                if (m_ModelOrg.Y1 > p1.Y) { m_ModelOrg.Y1 = p1.Y; rangeUpdated = true; }
                if (m_ModelOrg.Z1 > p1.Z) { m_ModelOrg.Z1 = p1.Z; rangeUpdated = true; }
                if (m_ModelOrg.X2 < p2.X) { m_ModelOrg.X2 = p2.X; rangeUpdated = true; }
                if (m_ModelOrg.Y2 < p2.Y) { m_ModelOrg.Y2 = p2.Y; rangeUpdated = true; }
                if (m_ModelOrg.Z2 < p2.Z) { m_ModelOrg.Z2 = p2.Z; rangeUpdated = true; }
            }
            if( rangeUpdated )CalculateModelSize();
            return rangeUpdated;
        }
        
        static public bool UpdateModelSize(C3DObjectBase obj)
        {
            return UpdateModelSize(obj.Minx,obj.Miny,obj.Minz, obj.Maxx, obj.Maxy, obj.Maxz);
        }
        
        static public void ResetModelSize(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            Vector64 p1 = new Vector64(x1, y1, z1);
            Vector64 p2 = new Vector64(x2, y2, z2);
            p1 = CDataModel.CovertCoordGeoToGL(p1);
            p2 = CDataModel.CovertCoordGeoToGL(p2);
            m_ModelOrg = new CubeModel64(x1,y1,z1,x2,y2,z2);
            CalculateModelSize();
        }

        static public Vector64 GetCenterPoint()
        {
            return m_Model.GetCenterPoint();
        }
        static public Vector32 GetCenterPoint32()
        {
            return m_Model.GetCenterPoint();
        }
        static public bool IsInModelRange(double x1,double y1,double z1,double x2,double y2,double z2)
        {
            if (x1 > m_Model.X2 || x2 < m_Model.X1 ||
                y1 > m_Model.Y2 || y2 < m_Model.Y1 ||
                z1 > m_Model.Z2 || z2 < m_Model.Z1) return false;
            return true;
        }
    }// class CDataModel
}
