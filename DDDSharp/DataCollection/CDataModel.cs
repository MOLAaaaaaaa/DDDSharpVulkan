using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        static public string xAxisText1 = "x";//笛卡尔坐标系
        static public string yAxisText1 = "y";
        static public string zAxisText1 = "z";
        static public string xAxisText2 = "North";//地质坐标系
        static public string yAxisText2 = "East";
        static public string zAxisText2 = "Depth";
        static public string xEarthAxisText = "";
        static public string yEarthAxisText = "";
        static public string zEarthAxisText = "N";
        static public float axisFontScale1 = 1.0f;
        static public float axisFontScale2 = 1.0f;
        static public float axisFontScale3 = 1.0f;
        //static public bool zAxisInversed = false;
        //球面坐标
        static public double earthRadius = 6378137;
        static public double MaxEarthRadius = 6378137;
        static public bool IsEarthMapVision = false;    //是否采用球面坐标系统显示
        static public bool IsGeoCoordinateSystem = false;    //是否采用向下坐标系

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

        static public bool IsInitialized()
        {
            if (m_ModelOrg.X1 == -1 && m_ModelOrg.X2 == 1 &&
                m_ModelOrg.Y1 == -1 && m_ModelOrg.Y2 == 1 &&
                m_ModelOrg.Z1 == -1 && m_ModelOrg.Z2 == 1) return false;
            return true;
        }
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
            br.Write(axisFontScale1);
            br.Write(IsGeoCoordinateSystem);//向下坐标系

            if ( !IsGeoCoordinateSystem)//笛卡尔坐标系
            {
                C3DData.SaveString(br, xAxisText1);
                C3DData.SaveString(br, yAxisText1);
                C3DData.SaveString(br, zAxisText1);
            }
            else//地质坐标系
            {
                C3DData.SaveString(br, xAxisText2);
                C3DData.SaveString(br, yAxisText2);
                C3DData.SaveString(br, zAxisText2);
                br.Write(axisFontScale2);
            }            
            if (IsEarthMapVision)
            {
                C3DData.SaveString(br, xEarthAxisText);
                C3DData.SaveString(br, yEarthAxisText);
                C3DData.SaveString(br, zEarthAxisText);
                br.Write(axisFontScale3);
            }

            return true;
        }
         
        static public bool LoadDataModel(BinaryReader br)
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
            axisFontScale1 = br.ReadSingle();
            IsGeoCoordinateSystem = br.ReadBoolean();
            if ( !IsGeoCoordinateSystem )
            {
                xAxisText1 = C3DData.LoadString(br);
                yAxisText1 = C3DData.LoadString(br);
                zAxisText1 = C3DData.LoadString(br);
                //xAxisText2 = C3DData.LoadString(br); //old version need load this 3
                //yAxisText2 = C3DData.LoadString(br);
                //zAxisText2 = C3DData.LoadString(br);
            }
            else
            {
                xAxisText2 = C3DData.LoadString(br);
                yAxisText2 = C3DData.LoadString(br);
                zAxisText2 = C3DData.LoadString(br);
                axisFontScale2 = br.ReadSingle();
            }
            if (IsEarthMapVision)//与原来版本兼容
            {
                xEarthAxisText = C3DData.LoadString(br);
                yEarthAxisText = C3DData.LoadString(br);
                zEarthAxisText = C3DData.LoadString(br);
                axisFontScale3 = br.ReadSingle();
            }

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
                //CubeModel64 newrange = C3DData.GetObjectsRange();
                //if ( newrange != m_ModelOrg )
                //{
                //    rangeUpdated = true;
                //    m_ModelOrg = newrange;
                //    m_ModelOrg = m_ModelOrg.toCentricCube();
                //}
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
        
        static public Vector64 GetCenterPoint()
        {
            return m_Model.GetCenterPoint();
        }
        static public Vector32 GetCenterPoint32()
        {
            return m_Model.GetCenterPoint();
        }
    }// class CDataModel
}
