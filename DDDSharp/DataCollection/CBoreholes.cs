using DataCollection;
using GlmNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static DataCollection.CBorehole;

namespace DataCollection
{
    public enum CylinderTypeEnum
    {
        Cylinder = 0,
        SquareColumn = 1,
    }   

    /// <summary>
    /// 井曲线 Well Curves
    /// </summary>
    public class BoreholeCurves : C3DObjectBaseHide
    {
        //测井曲线
        public LasFileData lasData = null; //LAS data
        public int depthIndex = 0;      //深度列

        [CategoryAttribute("Curves"), DisplayNameAttribute("Curves"), Browsable(true)]
        public List<BoreholeCurve> Curves { get; set; } = new List<BoreholeCurve>();        

        [CategoryAttribute("Display"), DisplayNameAttribute("Visible"), Browsable(true)]
        public override bool Visible { get; set; } = true;
        [CategoryAttribute("Display"), DisplayNameAttribute("Name"), Browsable(true)]
        public override string Name { get; set; } = "Untitled";
        
        [CategoryAttribute("Display"), DisplayNameAttribute("UniformStyle")]
        public bool UniformCurveStyle { get; set; } = true;

        bool _EnableCurveColorMap = true;
        [CategoryAttribute("Color Map"), DisplayNameAttribute("Enable")]
        public bool EnableCurveColorMap
        {
            get { return _EnableCurveColorMap; }
            set
            {
                _EnableCurveColorMap = value;
                if (UniformCurveStyle)
                {
                    foreach (BoreholeCurve cv in Curves)
                        cv.EnableColorScale = value;                        
                }
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public CColorScale _OlderColorScale = new CColorScale();
        public CColorScale _ColorScale = new CColorScale();
        [CategoryAttribute("Color Map"), DisplayNameAttribute("Color Map")]
        [Editor(typeof(ColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorScaleConverter))]
        public CColorScale ColorScale
        {
            get { return _ColorScale; }
            set
            {
                _OlderColorScale = _ColorScale;
                _ColorScale = value;
                if (UniformCurveStyle)
                {
                    foreach (BoreholeCurve cv in Curves)
                        cv.ColorScale = _ColorScale.Copy();
                }
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        Color _CurveColor = Color.Black;
        [CategoryAttribute("Curves"), DisplayNameAttribute("Color")]
        public Color CurveColor
        {
            get { return _CurveColor; }
            set
            {
                _CurveColor = value;
                if (UniformCurveStyle)
                {
                    foreach (BoreholeCurve cv in Curves)
                        cv.Color = value;
                }
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }        

        float _LineWidth = 1.0f;
        [CategoryAttribute("Curves"), DisplayNameAttribute("LineWidth")]
        public float LineWidth
        {
            get { return _LineWidth; }
            set
            {
                _LineWidth = value;                
                if (UniformCurveStyle)
                {
                    foreach (BoreholeCurve cv in Curves)
                        cv.LineWidth = value;
                }
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        float _Radius = 1.0f;
        [CategoryAttribute("Curves"), DisplayNameAttribute("Radius")]
        public float Radius
        {
            get { return _Radius; }
            set
            {
                _Radius = value;
                if (UniformCurveStyle)
                {
                    foreach (BoreholeCurve cv in Curves)
                        cv.Radius = value;
                }
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public bool IsColuwnExist(int index, string name)
        {
            if (lasData == null) return false;
            return lasData.IsColuwnExist(index, name);
        }
        public BoreholeCurves(string name = "Logging Curves")
        {
            Name = name;
            type = ShapeEnum.BoreholeCurves;
        }
        public int Count { get { return Curves.Count; } }
        public BoreholeCurve this[int index]
        {
            get { return Curves[index]; }
            set { Curves[index] = value; }
        }
        
        public void Add(BoreholeCurve d) { Curves.Add(d); }
        public override void Clear() { Curves.Clear(); }
        public BoreholeCurves Copy()
        {
            BoreholeCurves s = new BoreholeCurves(Name);
            s.CopyHeaderFrom(this);
            s.lasData = lasData;
            s.depthIndex = depthIndex;
            s.UniformCurveStyle = UniformCurveStyle;
            s._Radius = _Radius;
            s._LineWidth = _LineWidth;
            s._CurveColor = _CurveColor;
            s._ColorScale = _ColorScale.Copy();
            s._EnableCurveColorMap = _EnableCurveColorMap;

            foreach (BoreholeCurve d in Curves)
                s.Curves.Add(d.Copy());
            
            return s;
        }

        public override bool SaveAs(BinaryWriter br)
        {
            if ( !SaveObjHeader(br) ) return false;
            try 
            {
                br.Write(depthIndex);
                br.Write(UniformCurveStyle);
                br.Write(_Radius);
                br.Write(_LineWidth);
                br.Write(_CurveColor.ToArgb());
                br.Write(_EnableCurveColorMap);
                _ColorScale.WriteBinary(br);
                br.Write(Curves.Count);

                foreach (BoreholeCurve d in Curves)
                    d.SaveAs(br);

                if (lasData == null) br.Write(false);
                else
                {
                    br.Write(true);
                    lasData.SaveAs(br);
                }
                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }            
        }

        public override bool LoadFrom(BinaryReader br)
        {
            if ( !LoadObjHeader(br) ) return false;
            try
            {
                depthIndex = br.ReadInt32();
                UniformCurveStyle = br.ReadBoolean();
                _Radius = br.ReadSingle();
                _LineWidth = br.ReadSingle();
                _CurveColor = Color.FromArgb(br.ReadInt32());
                _EnableCurveColorMap = br.ReadBoolean();
                _ColorScale.LoadBinary(br);

                Curves.Clear();
                int n = br.ReadInt32();
                for(int i = 0; i < n; i++ )
                {
                    BoreholeCurve cv = new BoreholeCurve();
                    cv.LoadFrom(br);
                    Curves.Add(cv);
                }                
                if( br.ReadBoolean() )
                {
                    lasData = new LasFileData();
                    lasData.LoadFrom(br);
                }
                UpdateRange();
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }

    }
    /// <summary>
    /// 井曲线 Well Curve
    /// </summary>
    public class BoreholeCurve: C3DObjectBaseHide
    {
        public int xIndex = 0; //曲线X数据来源列序号
        public int yIndex = 0; //曲线y数据来源列序号
        public string xName = "";  //曲线X数据来源列名称
        public string yName = "";  //曲线y数据来源列名称
        
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible"), Browsable(true)]
        public override bool Visible { get; set; } = true;
        [CategoryAttribute("Curve"), DisplayNameAttribute("Name"), Browsable(true)]
        public override string Name { get; set; } = "Untitled";
        [CategoryAttribute("Curve"), DisplayNameAttribute("Overview"), Browsable(true)]
        public override string Information 
        {
            get 
            {
                string info = "Depth: " + minDepth + " to " + maxDepth + "\r\n";
                info += "Values: " + minValue + " to " + maxValue;
                return info;
            }
        }
        Color _Color = Color.Black;
        [CategoryAttribute("Curve"), DisplayNameAttribute("Color"), Browsable(true)]
        public Color Color 
        {
            get { return _Color; }
            set { _Color = value; if(!EnableColorScale)RenderMode = RenderingUpdateMode.Redraw; }
        }

        float _LineWidth = 1.0f;
        [CategoryAttribute("Curve"), DisplayNameAttribute("Line Width"), Browsable(true)]
        public float LineWidth
        {
            get { return _LineWidth; }
            set { _LineWidth = value; RenderMode = RenderingUpdateMode.Redraw; }
        }

        float _Radius = 1.0f;
        [CategoryAttribute("Curve"), DisplayNameAttribute("Radius"), Browsable(true)]
        public float Radius
        {
            get { return _Radius; }
            set { _Radius = value; RenderMode = RenderingUpdateMode.Redraw; }
        }

        bool _EnableColorScale = true;
        [CategoryAttribute("Color Map"), DisplayNameAttribute("Enable"), Browsable(true)]
        public bool EnableColorScale
        {
            get { return _EnableColorScale; }
            set { _EnableColorScale = value; RenderMode = RenderingUpdateMode.Redraw; }
        }
        [CategoryAttribute("Color Map"), DisplayNameAttribute("Color Map")]
        [Editor(typeof(ColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorScaleConverter))]
        public CColorScale ColorScale { get; set; } = new CColorScale();
        
        public float nullValue = -999.25f;
        //曲线数据: 深度，值
        public List<PointF> Points = new List<PointF>();
        public double maxValue, minValue;//数据最大最小值
        public double maxDepth, minDepth;//数据最大最小值
        
        public BoreholeCurve Copy()
        {
            BoreholeCurve cv = new BoreholeCurve();
            cv.CopyHeaderFrom(this);
            cv.xIndex = xIndex;
            cv.yIndex = yIndex;
            cv.xName = xName;
            cv.yName = yName;            
            cv.Color = Color;
            cv.EnableColorScale = EnableColorScale;
            cv.LineWidth = LineWidth;
            cv.Radius = Radius;
            cv.nullValue = nullValue;
            cv.Points.AddRange(Points);
            cv.maxValue = maxValue;
            cv.minValue = minValue;
            cv.maxDepth = maxDepth;
            cv.minDepth = minDepth;
            cv.ColorScale = ColorScale.Copy();
            return cv;
        }
        public override bool SaveAs(BinaryWriter br)
        {
            if ( !SaveObjHeader(br) ) return false;

            br.Write(xIndex);
            br.Write(yIndex);
            C3DData.SaveString(br,xName);
            C3DData.SaveString(br,yName);            
            br.Write(Color.ToArgb());
            br.Write(EnableColorScale);
            br.Write(LineWidth);
            br.Write(Radius);
            br.Write(nullValue);
            br.Write(maxValue);
            br.Write(minValue);
            br.Write(maxDepth);
            br.Write(minDepth);
            ColorScale.WriteBinary(br);
            br.Write(Points.Count);
            foreach(PointF p in Points)
            {
                br.Write(p.X);
                br.Write(p.Y);                
            }
            return true;
        }
        public override bool LoadFrom(BinaryReader br)
        {           
            if (!LoadObjHeader(br)) return false;

            xIndex = br.ReadInt32();
            yIndex = br.ReadInt32();
            xName = C3DData.LoadString(br);
            yName = C3DData.LoadString(br);
            Color = Color.FromArgb(br.ReadInt32());
            EnableColorScale = br.ReadBoolean();
            LineWidth = br.ReadSingle();
            Radius = br.ReadSingle();
            nullValue = br.ReadSingle();
            maxValue = br.ReadDouble();
            minValue = br.ReadDouble();
            maxDepth = br.ReadDouble();
            minDepth = br.ReadDouble();
            ColorScale.LoadBinary(br);
            int n = br.ReadInt32();
            Points.Clear();
            float x, y;
            for(int i=0;i<n;i++)
            {
                x = br.ReadSingle();
                y = br.ReadSingle();
                Points.Add(new PointF(x,y));
            }
            UpdateRange();
            return true;
        }
        public BoreholeCurve(string name = "Untitled")
        {
            Name = name;
            type = ShapeEnum.BoreholeCurve;
        }
        public override void UpdateRange()
        {
            if ( Points.Count < 1 ) return;
            int k = 0;            
            for ( int i = 0; i < Points.Count; i++ )
            {
                if ( Points[i].Y == nullValue) continue;

                if( k == 0) 
                {
                    minDepth = maxDepth = Points[i].X;
                    minValue = maxValue = Points[i].Y;                    
                }
                else
                {
                    if (Points[i].X < minDepth) minDepth = Points[i].X;
                    if (Points[i].X > maxDepth) maxDepth = Points[i].X;
                    if (Points[i].Y < minValue) minValue = Points[i].Y;
                    if (Points[i].Y > maxValue) maxValue = Points[i].Y;
                }
                
                minv = minValue;
                maxv = maxValue;

                k++;
            }
        }
        public void CreateFromLAS( LasFileData data,int xcol,int ycol, float _nullValue )
        {
            Points.Clear();
            xIndex = xcol;
            yIndex = ycol;            
            xName = data.CurveInformation[xIndex].Mnemonic;
            yName = data.CurveInformation[yIndex].Mnemonic;
            nullValue = _nullValue;            
            int n = data.LogData[xIndex].Count;
            for(int i = 0; i < n; i++ )
            {
                PointF p = new PointF();
                p.X = data.LogData[xIndex][i];
                p.Y = data.LogData[yIndex][i];
                Points.Add(p);
            }

            if( Points.Count > 1 )
            {
                if (Points[0].X > Points[1].X) 
                    Points.Reverse();
                UpdateRange();
                ColorScale.SetValueRange(minValue,maxValue);
            }
        }

        public void CreateFromLAS(LasFileData data, float _nullValue)
        {
            CreateFromLAS(data, xIndex,yIndex, _nullValue);
        }

        public void Draw(Graphics g, Rectangle rect, double h1, double h2, bool selected = false)
        {
            if (Points.Count < 2) return;
            PointF[] pp = new PointF[Points.Count];
            double x, y;
            for(int i=0;i<Points.Count;i++)
            {
                PointF p = Points[i];
                //y = rect.Top + rect.Height * (p.X - minDepth) / (maxDepth - minDepth);
                y = rect.Top + rect.Height * (p.X - h1) / (h2-h1);
                if (p.Y == nullValue) x = (rect.Left + rect.Right) / 2;
                else x = rect.Left + rect.Width * (p.Y - minValue) / (maxValue - minValue);

                pp[i] = new PointF((float)x, (float)y);
            }
            if( EnableColorScale ) 
            {
                PointF p1, p2;
                for (int i = 0; i < Points.Count-1; i++)
                {
                    p1 = pp[i];
                    p2 = pp[i+1];
                    Pen pen = new Pen( ColorScale.GetColor(Points[i].Y),LineWidth );
                    g.DrawLine(pen, p1, p2);
                    pen.Dispose();
                }
            }
            else
            {
                Pen pen = new Pen(Color, LineWidth);
                g.DrawLines(pen, pp);
                pen.Dispose();
            }
            pp = null;
        }
    }
    /// <summary>
    /// 钻孔测斜数据结构
    /// </summary>
    public class BoreholeAnglesStruct: C3DObjectBaseHide
    {
        [CategoryAttribute("Display"), DisplayNameAttribute("Name"), Browsable(true),ReadOnly(true)]
        public override string Name { get; set; } = "Untitled";

        public Vector64 Start = new Vector64(); //开始位置
        [CategoryAttribute("Inclines"), DisplayNameAttribute("Length")]
        public double Depth { get; set; } = 0;   //孔深     
        [CategoryAttribute("Inclines"), DisplayNameAttribute("方位角(Azimuth)")]
        public double Azimuth { get; set; } = 0;    //方位角--地面投影与北向夹角
        [CategoryAttribute("Inclines"), DisplayNameAttribute("顶角(Zenith)")]
        public double Zenith { get; set; } = 90;     //顶角、倾角、天顶角--与地表夹角 = 90 - DipAngle
        public BoreholeAnglesStruct()
        {
            type = ShapeEnum.BoreholeAngle;
        }
        public BoreholeAnglesStruct(string name)
        {
            type = ShapeEnum.BoreholeAngle;
            Name = name;
        }
        public override string ToString()
        {
            string ss = "Length = " + Depth;
            ss += ", Azimuth = " + Azimuth;
            ss += ", Zenith = " + Zenith;
            return ss;
        }
        public bool IsValid()
        {
            if (Depth <= 0) return false;
            else return true;
        }
        public Vector64 toTracedPoint(Vector64 top)
        {
            return top + toTracedPoint();
        }
        public Vector64 toTracedPoint()
        {
            double dx=0, dy=0, dz = 0, dxy = 0;
            dz = -Math.Sin(Vector64.toRad(Zenith)) * Depth;//垂向投影高度
            dxy = Math.Cos(Vector64.toRad(Zenith)) * Depth; //地面投影长度
            if( Zenith == 90 )return new Vector64(0, 0, dz);
            if ( Azimuth > 0 && Azimuth <= 90 ) // 0 - 360
            {
                dx = dxy * Math.Sin(Vector64.toRad(Azimuth));//east
                dy = dxy * Math.Cos(Vector64.toRad(Azimuth));//north                
            }
            else if (Azimuth > 90 && Azimuth < 180 ) 
            {
                dx = dxy * Math.Cos(Vector64.toRad(Azimuth-90));
                dy = -dxy * Math.Sin(Vector64.toRad(Azimuth-90));
            }
            else if (Azimuth >= 180 && Azimuth < 270 )
            {
                dx = -dxy * Math.Sin(Vector64.toRad(Azimuth - 180));
                dy = -dxy * Math.Cos(Vector64.toRad(Azimuth - 180));
            }
            else if (Azimuth >= 270 && Azimuth < 360)
            {
                dx = -dxy * Math.Sin(Vector64.toRad(Azimuth - 270));
                dy = dxy * Math.Cos(Vector64.toRad(Azimuth - 270));
            }
            return new Vector64(dx, dy, dz);
        }

        public BoreholeAnglesStruct Copy()
        {
            BoreholeAnglesStruct an = new BoreholeAnglesStruct();
            an.Start = Start;
            an.Azimuth = Azimuth;
            an.Depth = Depth;
            an.Zenith = Zenith;
            return an;
        }
        public override bool SaveAs(BinaryWriter br)
        {
            C3DData.SaveString(br, Name);
            br.Write(Start.X);
            br.Write(Start.Y);
            br.Write(Start.Z);
            br.Write(Depth);
            br.Write(Azimuth);
            br.Write(Zenith); 
            return true;
        }
        public override bool LoadFrom(BinaryReader br)
        {
            Name = C3DData.LoadString(br);
            Start.X = br.ReadDouble();
            Start.Y = br.ReadDouble();
            Start.Z = br.ReadDouble();
            Depth = br.ReadDouble();
            Azimuth = br.ReadDouble();
            Zenith = br.ReadDouble();
            return true;
        }
    }
    /// <summary>
    /// 井地层数据
    /// </summary>
    public class StratumData: C3DObjectBaseHide
    {
        [CategoryAttribute("Stratum"), DisplayNameAttribute("Visible"), Browsable(true)]
        public override bool Visible { get; set; } = true;
        [CategoryAttribute("Stratum"), DisplayNameAttribute("Name"), Browsable(true)]
        public override string Name { get; set; } = "Untitled";        

        [CategoryAttribute("Stratum"), DisplayNameAttribute("Code"), Browsable(true)]
        public string Code { get; set; } = "";//地层代码       
        [CategoryAttribute("Stratum"), DisplayNameAttribute("TopDepth"), Browsable(true)]
        public double TopDepth { get; set; } = 0.0;       //层顶埋深-从井口起算

        [CategoryAttribute("Stratum"), DisplayNameAttribute("BottomDepth"), Browsable(true)]
        public double BottomDepth
        {
            get { return TopDepth + Thickness; }
            set { Thickness = value - TopDepth; }
        }
        public double Value { get; set; } = 0;

        [CategoryAttribute("Stratum"), DisplayNameAttribute("Thickness"), Browsable(true)]
        public double Thickness { get; set; } = 0.0;      //厚度
        [CategoryAttribute("Display"), DisplayNameAttribute("Color"), Browsable(true)]
        public Color Color { get; set; } = Color.Gray;

        public bool IsGeocoordinate = false; //是否向下坐标系  
        public Vector64 Top = new Vector64();//计算顶界面位置
        public Vector64 Bottom = new Vector64();//计算底界面位置
        public bool IsValid()
        {
            if ( Name.Trim().Length < 1 || Thickness <= 0 )
                return false;
            else return true;
        }        
       
        [CategoryAttribute("Display"), DisplayNameAttribute("Alpha"), Browsable(true)]
        public override float Alpha { get; set; } = 1.0f;

        [CategoryAttribute("Display"), DisplayNameAttribute("IsWireFrameMode"), Browsable(true)]
        public override bool IsWireFrameMode { get; set; }
        [CategoryAttribute("Display"), DisplayNameAttribute("textureStruct"), Browsable(true)]
        public override TextureStruct textureStruct
        {
            get { return _textStruct; }
            set { _textStruct = value; }
        }

        public double GetNextTopDepth()
        {
            return TopDepth + Thickness;
        }
        public StratumData(string name = "Untitiled") 
        {
            Name = name;
            type = ShapeEnum.BoreholeStratum;            
        }
        public StratumData(string name,double top,double thickness)
        {
            Name = name;
            TopDepth = top;
            Thickness = thickness;
        }       
       
        public StratumData Copy()
        {
            StratumData s = new StratumData(Name);
            s.CopyHeaderFrom(this);
            s.Code = Code;
            s.Color = Color;            
            s.Top = Top;
            s.Bottom = Bottom;
            s.Thickness = Thickness;
            s.TopDepth = TopDepth; 
            s.IsGeocoordinate = IsGeocoordinate;
            s.textureStruct = textureStruct.Copy();
            s.Parent = Parent;
            return s;
        }
        public override bool SaveAs(BinaryWriter br)
        {
            br.Write(Visible);
            C3DData.SaveString(br, Name);
            C3DData.SaveString(br, Code);
            br.Write(TopDepth);
            br.Write(Thickness);
            br.Write(Color.ToArgb());
            br.Write(Alpha);
            br.Write(IsWireFrameMode);
            br.Write(Top.X);
            br.Write(Top.Y);
            br.Write(Top.Z);
            br.Write(Bottom.X);
            br.Write(Bottom.Y);
            br.Write(Bottom.Z);
            textureStruct.Save(br);
            return true;
        }
        public override bool LoadFrom(BinaryReader br)
        {
            Visible = br.ReadBoolean();
            Name = C3DData.LoadString(br);
            Code = C3DData.LoadString(br);
            TopDepth = br.ReadDouble();
            Thickness = br.ReadDouble();
            Color = Color.FromArgb(br.ReadInt32());
            Alpha = br.ReadSingle();
            IsWireFrameMode = br.ReadBoolean();
            Top.X = br.ReadDouble();
            Top.Y = br.ReadDouble();
            Top.Z = br.ReadDouble();
            Bottom.X = br.ReadDouble();
            Bottom.Y = br.ReadDouble();
            Bottom.Z = br.ReadDouble();
            textureStruct.Load(br);
            UpdateRange();
            return true;
        }
    }
    public enum SamplingMethodEnum
    {
        Importance =0,
        Average =1,
        Squared = 2,
    }
    /// <summary>
    /// 井地层数据
    /// </summary>
    public class StratumDatas : C3DObjectBaseHide
    {        
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible"), Browsable(true)]
        public override bool Visible { get; set; } = true;
        [CategoryAttribute("Display"), DisplayNameAttribute("Name"), Browsable(true)]
        public override string Name { get; set; } = "Strata";

        public List<StratumData> Stratums = new List<StratumData>();
        public int Count { get { return Stratums.Count; } }        

        Dictionary<string, int> stratumDictionary = new Dictionary<string, int>();

        public StratumData this[int index] 
        {
            get { return Stratums[index]; }
            set { Stratums[index] = value; }
        }
        public StratumDatas(string name = "Strata")
        {
            Name = name;
            type = ShapeEnum.BoreholeStratums;
        }
        public CColorScale CreateColorScale()
        {
            CColorScale colorScale = new CColorScale();
            colorScale.Levels.Clear();

            ColorLevel lvl = new ColorLevel(0, 1, 1, 1); //背景，无地层
            colorScale.Levels.Add(lvl);

            float step = 100f / Stratums.Count;

            for (int i = 0; i < Stratums.Count; i++)
            {
                float percent = (i + 1) * step;
                float r = Stratums[i].Color.R / 255f;
                float g = Stratums[i].Color.G / 255f;
                float b = Stratums[i].Color.B / 255f;
                lvl = new ColorLevel(percent, r, g, b);
                colorScale.Levels.Add(lvl);
            }
            colorScale.SetValueRange(0, Stratums.Count);
            return colorScale;
        }
        public void CreateDictionary()
        {
            stratumDictionary.Clear();
            for (int i = 0; i < Stratums.Count; i++)
            {
                StratumData s = Stratums[i];
                stratumDictionary.Add(s.Name.ToLower(), i);
            }
        }
        public int GetStrataIdByName(string name)
        {
            if (Stratums.Count < 1) return -1;
            if (stratumDictionary.Count < 1) CreateDictionary();
            string s1 = name.ToLower();
            if (stratumDictionary.ContainsKey(s1))
            {
                return stratumDictionary[s1];
            }
            return -1;
        }

        public StratumData GetStrataByName(string name)
        {
            int id = GetStrataIdByName(name);
            if( id < 0 ) return null;
            return Stratums[id];            
        }
        public List<string>toStratumNames()
        {
            List<string>names = new List<string>();
            for(int i=0;i<Count;i++)
            {
                names.Add(Stratums[i].Name);
            }
            return names;
        }
        int SampleStratumByRect(Rectangle rect,int[] intBytes, int bmpWidth, int diff) 
        {
            int id = 0, count = -1, iStratum;
            int length = intBytes.Length;            
            int[] Indices = new int[Stratums.Count];            
            for (int i = 0; i < Stratums.Count; i++) Indices[i] = -1;
            for (int iy = rect.Top; iy < rect.Bottom; iy++ )
            {
                for (int ix = rect.Left; ix < rect.Right; ix++)
                {
                    id = iy * bmpWidth + ix;
                    if ( id >= length ) continue;
                    Color c = Color.FromArgb(intBytes[id]);
                    iStratum = GetStratumIdByColor(c, diff);
                    if (iStratum >= 0) Indices[iStratum]++;                    
                }
            }
            iStratum = -1;           
            for (int i = 0; i < Stratums.Count; i++) 
            {
                if (Indices[i] > count) 
                { 
                    count = Indices[i];
                    iStratum = i; 
                }                
            }
            Indices = null;            
            return iStratum;
        }

        int SeekTop(short[,] grid, int width, int height)
        {
            int top = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if ( grid[j, i] >= 0 )
                    {
                        if (i > top) top = i;
                        return top;
                    }
                }
            }
            return top;
        }
        int SeekBottom(short[,] grid, int width, int height)
        {
            int bottom = height - 1;
            for (int i = height-1; i >= 0; i--)
            {
                for (int j = 0; j < width; j++)
                {
                    if (grid[j, i] >= 0)
                    {
                        if (i < bottom) bottom = i;
                        return bottom;
                    }
                }
            }
            return bottom;
        }
        int SeekLeft(short[,] grid, int width, int height)
        {
            int left = 0;
            for (int j = 0; j < width; j++)
            {
                for (int i = 0; i < height; i++)
                {
                    if (grid[j, i] >= 0)
                    {
                        if (j > left) left = j;
                        return left;
                    }
                }
            }
            return left;
        }
        int SeekRight(short[,] grid, int width, int height)
        {
            int right = width - 1;
            for (int j = width-1; j >=0; j--)
            {
                for (int i = 0; i < height; i++)
                {
                    if (grid[j, i] >= 0)
                    {
                        if (j < right) right = j;
                        return right;
                    }
                }
            }
            return right;
        }
        /// <summary>
        /// 获取有效地层区域范围
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="diff"></param>
        /// <returns></returns>
        public Rectangle CheckImageValidateArea(Bitmap bmp, int diff = 5)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            short[,] grid = new short[width, height];
            for (int i = 0; i < height; i++)
            {              
                for (int j = 0; j < width; j++)
                {                    
                    grid[j, i] = (short)GetStratumIdByColor(bmp.GetPixel(j, i), diff);
                }
            }
            int top = SeekTop(grid, width, height);
            int bottom = SeekBottom(grid, width, height);
            int left = SeekLeft(grid, width, height);
            int right = SeekRight(grid, width, height);
            grid = null;
            return new Rectangle(left, top, right - left, bottom - top);
        }
        /// <summary>
        /// 按颜色色标对地层进行采样（随机重要点法）
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="nx"></param>
        /// <param name="ny"></param>
        /// <param name="diff"></param>
        /// <returns></returns>
        int[,] SamplingFromImageByColorOnImportance(Bitmap bmp,int nx,int ny,int diff =5)
        {
            double dx = (double)bmp.Width / nx;
            double dy = (double)bmp.Height / ny;
            int[,] grid = new int[nx, ny];
            int ix, iy;
            for (int i = 0; i < ny; i++)
            {
                iy = (int)(dy * i);
                for (int j = 0; j < nx; j++)
                {
                    ix = (int)(dx * j);
                    Color c = bmp.GetPixel(ix, iy);
                    grid[j, ny - 1 - i] = GetStratumIdByColor(c,diff);
                }
            }
            return grid;
        }

        Object lock1 = new object();

        /// <summary>
        /// 按颜色色标对地层进行采样（面积法统计）
        /// </summary>
        /// <param name="bmp">地图图片</param>
        /// <param name="nx">网格单元数（非节点数）</param>
        /// <param name="ny">网格单元数（非节点数）</param>
        /// <param name="diff">允许的颜色误差</param>
        /// <returns></returns>
        int[,] SamplingFromImageByColorOnSqured(Bitmap bmp, int nx, int ny, int diff = 5)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int[] intBytes = new int[bmp.Width * bmp.Height];
            Marshal.Copy(bd.Scan0, intBytes, 0, intBytes.Length);
            bmp.UnlockBits(bd);

            //网格单元数目nx,ny
            int width = bmp.Width;
            int height = bmp.Height;
            double dx = (double)width / nx;
            double dy = (double)height / ny;
            int[,] grid = new int[nx, ny];
            int ix, iy;
            //Parallel.For(0, ny, i => //不能并行？？？采样错误？？？
            for (int i = 0; i < ny; i++)
            {
                iy = (int)(dy * i);
                for (int j = 0; j < nx; j++)
                {
                    ix = (int)(dx * j);
                    rect = new Rectangle(ix, iy, (int)dx, (int)dy);
                    grid[j, ny - 1 - i] = SampleStratumByRect(rect, intBytes, width, diff);
                }
            }//);
            intBytes = null;
            return grid;
        }
        /// <summary>
        /// 根据图例颜色从图像中进行地层采样
        /// </summary>
        /// <param name="bmp">图像</param>
        /// <param name="nx">采样横向间隔</param>
        /// <param name="ny">采样纵向间隔</param>
        /// <param name="method">采样方法</param>
        /// <param name="diff">颜色误差</param>
        /// <returns></returns>
        public int[,] SamplingFromImageByColor(Bitmap bmp, int nx, int ny, SamplingMethodEnum method = SamplingMethodEnum.Squared, int diff = 5)
        {
            if (method == SamplingMethodEnum.Squared) return SamplingFromImageByColorOnSqured(bmp, nx, ny, diff);
            else if (method == SamplingMethodEnum.Importance) return SamplingFromImageByColorOnImportance(bmp, nx, ny, diff);
            else return null;
        }
        /// <summary>
        /// 获取颜色在色标中的位置
        /// </summary>
        /// <param name="color"></param>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
        public float GetColorBetween(Color color, ref int id1,ref int id2, double err = 1E-6)
        {
            Color c = color;
            float s1 = -1, s2 = -1, s3 = -1;
            id1 = id2 = -1;
            for (int i = 0; i < Stratums.Count - 1; i++ )
            {
                Color c1 = Stratums[i].Color;
                Color c2 = Stratums[i+1].Color;
                vec3 v1 = new vec3(c1.R, c1.G, c1.B);
                vec3 v2 = new vec3(c2.R, c2.G, c2.B);
                vec3 v = new vec3(c.R, c.G, c.B);

                s1 = s2 = s3 = 500;//初始大于255
                //判别标志，三个方向一致（符号），0 - 1之间
                if ( Math.Abs(v2.x - v1.x) == 0 )
                {
                    if(Math.Abs(v.x - v1.x) == 0) s1 = 0;
                }                
                else s1 = (v.x - v1.x) / (v2.x - v1.x);

                if (Math.Abs(v2.y - v1.y) == 0)
                {
                    if (Math.Abs(v.y - v1.y) == 0) s2 = 0;
                }
                else s2 = (v.y - v1.y) / (v2.y - v1.y);

                if (Math.Abs(v2.z - v1.z) == 0)
                {
                    if (Math.Abs(v.z - v1.z) == 0) s3 = 0;
                }
                else s3 = (v.z - v1.z) / (v2.z - v1.z);

                if (s1 < 0 || s2 < 0 || s3 < 0) continue;
                if (s1 > 1 || s2 > 1 || s3 > 1) continue;
                if (s1 * s2 < 0 || s2 * s3 < 0 || s1 * s3 < 0) continue;//不同向
                id1 = i;
                id2 = i + 1;
                return Math.Max(Math.Max(s1, s2), Math.Max(s2, s3));
            }
            return -1;
        }
        
        public int GetStratumIdByColor(Color color, int diff)
        {
            for (int i = 0; i < Stratums.Count; i++)
            {
                if (C3DData.IsSimilarColor(Stratums[i].Color, color, diff))
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 按属性采样，地层名称为属性值
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="nx"></param>
        /// <param name="ny"></param>
        /// <param name="colordiff"></param>
        /// <returns></returns>
        public bool SamplingValuesFromImage(Bitmap bmp, int nx, int ny, float[,] grid,double err = 1E-6)
        {
            try
            {
                double dx = (double)bmp.Width / nx;
                double dy = (double)bmp.Height / ny;                
                int ix, iy, id1 = -1, id2 = -1;
                float scale = 0,v1,v2;
                for (int i = 0; i < ny; i++)
                {
                    iy = (int)(dy * i);
                    for (int j = 0; j < nx; j++)
                    {
                        ix = (int)(dx * j);
                        Color c = bmp.GetPixel(ix, iy);
                        scale = GetColorBetween(c, ref id1, ref id2, err);
                        if(scale >=0 && scale <= 1)
                        {
                            v1 = float.Parse(Stratums[id1].Name);
                            v2 = float.Parse(Stratums[id2].Name);
                            grid[j, ny - 1 - i] = v1 + scale * (v2 - v1);
                        }                         
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }

        }
        /// <summary>
        /// 地层采样
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="nx"></param>
        /// <param name="ny"></param>
        /// <param name="colordiff"></param>
        /// <returns></returns>
        public int[,] SamplingFromImage(Bitmap bmp, int nx, int ny, int colordiff)
        {
            return SamplingFromImageByColor(bmp, nx, ny, SamplingMethodEnum.Squared, colordiff);
        }
        public bool IsExist(string name)
        {
            for(int i=0;i< Stratums.Count;i++)
            {
                if (Stratums[i].Name.ToLower() == name.ToLower())
                    return true;
            }
            return false;
        }

        public void AddLayer(StratumData d, bool autoTopDepth = true) 
        {            
            if(autoTopDepth)
            {
                d.TopDepth = 0;                
                if (Stratums.Count > 0 && autoTopDepth)
                {
                    d.TopDepth = Stratums[Stratums.Count - 1].GetNextTopDepth();
                }
            }            
            Stratums.Add(d); 
        }
        public void RemoveLayer(int index, bool autoTopDepth = true)
        {
            if ( index < 0 || index >= Count) return;
            Stratums.RemoveAt( index );

            if (autoTopDepth && index < Count - 1 )
            {
                UpdateTopDepth(index + 1);
            }            
        }
        public override void Clear() 
        {
            stratumDictionary.Clear();
            Stratums.Clear(); 
        }
        /// <summary>
        /// 得到第id层的层顶埋深
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double GetDepthBythickness(int id)
        {
            if (Stratums.Count < 1) return 0;

            StratumData layer = Stratums[0];
            double depth = layer.TopDepth;
            for ( int i = 0; i < Stratums.Count && i < id; i++ )
            {
                layer = Stratums[i];
                depth += layer.Thickness;
            }
            return depth;
        }
        /// <summary>
        /// 上层界面被改变，修改地层顶埋深
        /// </summary>
        /// <param name="index">开始位置</param>
        /// <param name="firstdepth">开始位置的顶界面</param>
        public void UpdateTopDepth(int index, double firstdepth)
        {   
            if (index < 0 || index >= Count ) return;
            StratumData layer1 = Stratums[index];
            layer1.TopDepth = firstdepth;
            if (index >= Count - 1) return; //最后一层
            StratumData layer2 = Stratums[index+1];
            double dz = firstdepth + layer1.Thickness - layer2.TopDepth;
            for (int i = index + 1; i < Stratums.Count; i++)
            {
                layer2 = Stratums[i];
                layer2.TopDepth += dz;
            }
        }
        public void UpdateTopDepth(int index)
        {
            if (index <= 0 || index >= Count) return;

            double depth = Stratums[index - 1].GetNextTopDepth();
            double dz = depth - Stratums[index].TopDepth;
            if (dz == 0) return;

            Stratums[index].TopDepth = depth;
            
            for (int i = index+1; i < Stratums.Count; i++)
            {
                StratumData layer = Stratums[i];                
                layer.TopDepth += dz;
                Stratums[i] = layer;
            }
        }
        /// <summary>
        /// 将layer地层插入指定位置index
        /// </summary>
        /// <param name="id"></param>
        /// <param name="layer"></param>
        /// <returns>当前顶层的顶界面</returns>
        public double InsertLayer(int index, StratumData layer, bool autoTopdepth = true )
        {
            if (index < 0 || index >= Count) return -1;
            layer.TopDepth = 0;
            if (autoTopdepth) //修改顶界面
            {
                if (index > 0) layer.TopDepth = Stratums[index - 1].GetNextTopDepth();
            }
            if ( index > 0)//修改下层地层界面
            {
                UpdateTopDepth(index + 1, layer.GetNextTopDepth());
            }
            Stratums.Insert(index, layer);
            return layer.TopDepth;
        }

        public StratumDatas Copy()
        {
            StratumDatas s = new StratumDatas(Name);
            s.CopyHeaderFrom(this);
            foreach (StratumData d in Stratums)
                s.Stratums.Add(d.Copy()) ;
            return s;
        }

        public override bool SaveAs(BinaryWriter br)
        {
            try
            {
                //测井基准线，Z按由小到大排列
                SaveObjHeader(br);               
                br.Write(Stratums.Count);
                foreach (StratumData s in Stratums)
                {
                    if ( !s.SaveAs(br) )
                    {
                        errMessage = s.errMessage;
                        return false;
                    }
                }                
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }

        }
        public override bool LoadFrom(BinaryReader br)
        {
            try
            {
                LoadObjHeader(br); 
                Stratums.Clear();
                int n = br.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    StratumData s = new StratumData();
                    if( !s.LoadFrom(br) )
                    {
                        errMessage = s.errMessage;
                        return false;
                    }
                    Stratums.Add(s);
                }                
                UpdateRange();
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 输出地层配色方案
        /// </summary>
        /// <param name="bh"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        static public bool ExportStratumScheme(StratumDatas stratums, string filename)
        {
            try
            {
                StreamWriter br = new StreamWriter(new FileStream(filename, FileMode.Create));
                string line = "// Stratums Color File";
                br.WriteLine(line);
                line = "// Created by " + C3DData.UserID + " on " + DateTime.Now.ToShortDateString();
                br.WriteLine(line);
                line = "Stratums Schemes " + C3DData.Version * 100;
                br.WriteLine(line);
                for (int i = 0; i < stratums.Count; i++)
                {
                    StratumData layer = stratums[i];
                    line = layer.Name + ",  ";
                    line += layer.Code + ",  ";
                    line += layer.Color.R.ToString() + ",  "; ;
                    line += layer.Color.G.ToString() + ",  "; ;
                    line += layer.Color.B.ToString();
                    br.WriteLine(line);
                }
                br.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 载入地层配色方案
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public StratumDatas LoadFromStratumScheme(string filename)
        {           
            try
            {
                StreamReader br = new StreamReader(new FileStream(filename, FileMode.Open));
                string line;
                int i = 0;
                while ((line = br.ReadLine()) != null)
                {
                    if (line.Length < 1) continue;
                    if (line.Length > 1 && line[0] == '/' && line[1] == '/') continue; //双斜杠备注                    
                    if (line[0] == '!' || line[0] == '#') continue;
                    if (i == 0) //第一行
                    {
                        if (!line.Contains("Stratums Schemes"))
                        {
                            errMessage = "Not a valid Stratums Schemes file.";
                            br.Close();
                            return this;
                        }
                        i++;
                    }
                    else
                    {
                        string[] ss = line.Split(new char[] { ',' });
                        if (ss.Length < 4) continue;
                        StratumData layer = new StratumData(ss[0].Trim());
                        layer.Code = ss[1].Trim();
                        layer.Color = Color.FromArgb(byte.Parse(ss[2]), byte.Parse(ss[3]), byte.Parse(ss[4]));
                        AddLayer(layer);
                    }
                    i++;
                }
                br.Close();

            }
            catch (Exception ex)
            {
               errMessage = ex.Message;
            }
            return this;
        }

        public void AddStratums(CBoreholes boreholes)
        {
            for (int i = 0; i < boreholes.Count; i++)
            {
                AddStratums(boreholes[i]);
            }
        }

        public void AddStratums(CBorehole bh)
        {
            for (int j = 0; j < bh.Stratums.Count; j++)
            {
                if (IsExist(bh.Stratums[j].Name)) continue;
                AddLayer(bh.Stratums[j],false);
            }
        }


    }
    /// <summary>
    /// 井斜数据
    /// </summary>
    public class BoreholeAngles : C3DObjectBaseHide
    {
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible"),Browsable(true)]
        public override bool Visible { get; set; } = true;
        [CategoryAttribute("Display"), DisplayNameAttribute("Name"), Browsable(true)]
        public override string Name { get; set; } = "Inclines";

        public List<BoreholeAnglesStruct> Angles = new List<BoreholeAnglesStruct>();//测斜数据
        public int Count { get { return Angles.Count; } }
        public BoreholeAnglesStruct this[int index]
        {
            get { return Angles[index]; }
            set { Angles[index] = value; }
        }
        public BoreholeAngles(string name = "Incline angles")
        {
            Name = name;
            type = ShapeEnum.BoreholeAngles;
        }
        public void Add(BoreholeAnglesStruct d) { Angles.Add(d); }
        public override void Clear() { Angles.Clear(); }
        public BoreholeAngles Copy()
        {
            BoreholeAngles s = new BoreholeAngles(Name);
            s.CopyHeaderFrom(this);
            foreach (BoreholeAnglesStruct d in Angles)
                s.Add(d.Copy());
            return s;
        }
        /// <summary>
        /// 根据测斜数据创建基准线
        /// 测斜数据排列顺序：由井口向下排列
        /// </summary>
        /// <param name="start">井口标高</param>
        /// <param name="geocoord">
        /// true, 向下坐标系--X指北，y指向东
        /// false,向上坐标系--X指东，y指向北
        /// </param>
        /// <returns></returns>
        public List<Vector64> CreateTracesLine(Vector64 start, bool geocoord = false)
        {
            List<Vector64> traces = new List<Vector64>();
            Vector64 p1 = start, p2;
            traces.Add(start); //地面点
            BoreholeAnglesStruct angle;
            for (int i = 0; i < Angles.Count; i++)
            {
                angle = Angles[i];
                if ( !angle.IsValid() ) continue;
                p2 = angle.toTracedPoint(p1);
                traces.Add(p2);
                p1 = p2;
            }
            return traces;
        }

        public override bool SaveAs(BinaryWriter br)
        {
            try
            {
                //测井基准线，Z按由小到大排列
                SaveObjHeader(br);                
                br.Write(Angles.Count);
                foreach (BoreholeAnglesStruct s in Angles)
                {
                    if( !s.SaveAs(br) )
                    {
                        errMessage = s.errMessage;
                        return false;
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }

        }
        public override bool LoadFrom(BinaryReader br)
        {
            try
            {
                LoadObjHeader(br);
                Angles.Clear();
                int n = br.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    BoreholeAnglesStruct s = new BoreholeAnglesStruct();
                    if( !s.LoadFrom(br) )
                    {
                        errMessage = s.errMessage;
                        return false;
                    }
                    Angles.Add(s);
                }
                
                UpdateRange();
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }       

    }
    public class CBorehole: C3DObjectBase
    {
        public struct BoreholeSamplePoint 
        {
            public Vector64 Point;
            public short boreholeID;
            public BoreholeSamplePoint(Vector64 p,int id)
            {
                Point = p;
                boreholeID = (short)id;
            }
        }
        //测井基准线，Z按由小到大排列
        public Vector64 Position = new Vector64();               //井口坐标
        public List<Vector64> Baseline = new List<Vector64>(); //基准线
        public BoreholeAngles boreholeAngles = new BoreholeAngles();//测斜数据
        public StratumDatas Stratums = new StratumDatas(); //地层数据
        public bool IsBaselineAscOrder
        {
            get 
            { 
                if ( Baseline.Count < 2 ) return false;
                else 
                {
                    double z1 = Baseline[0].z;
                    double z2 = Baseline[Baseline.Count-1].z;
                    if (z1 < z2) return true;
                    else return false;
                }
            }
        }
        /// <summary>
        /// 根据高程获得对应的插值位置xy
        /// </summary>
        /// <param name="z">高程位置</param>
        /// <param name="points">应该按升序排列</param>
        /// <returns></returns>
        public Vector64 GetPositionFromBaseline(double Z, List<Vector64>points = null)
        {
            if (points == null || points.Count < 2) return new Vector64();
            
            int n1 = -1, n2 = -1;
            Vector64 pos = new Vector64(0, 0, 0);
            double err = 1E-6;//误差
            for (int i = 0; i < points.Count; i++)
            {
                if ( Math.Abs(Z - points[i].Z) <= err ) return points[i];
                if (IsBaselineAscOrder)//升序排列
                {
                    if ( Math.Round(Z,6) < Math.Round(points[i].Z, 6) )
                    {
                        n2 = i;
                        break;
                    }
                    else n1 = i;
                }
                else //降序排列
                {
                    if (Math.Round(Z, 6) > Math.Round(points[i].Z, 6))
                    {
                        n1 = i;
                        break;
                    }
                    else n2 = i;
                }
            }
            Vector64 p1 = new Vector64(0, 0, 0);
            Vector64 p2 = new Vector64(0, 0, 0);
            if (n1 < 0) // 最低以下
            {
                if (IsBaselineAscOrder) { p1 = points[1]; p2 = points[0]; }
                else { p2 = points[points.Count - 2]; p2 = points[points.Count - 1];}
            }
            if (n2 < 0) // 最高以上
            {
                if (IsBaselineAscOrder) { p2 = points[points.Count - 1]; p1 = points[points.Count - 2]; }
                else { p1 = points[1]; p2 = points[0]; }
            }

            if( n1 >= 0 && n2 >= 0 )
            {
                p1 = points[n1];
                p2 = points[n2];                
            }
            
            if (p1.Z == p2.Z) pos = p1;
            else pos = p1 + (p2 - p1) * (Z - p1.Z) / (p2.Z - p1.Z);

            pos.Z = Z;
            return pos;
        }

        /// <summary>
        /// 根据深度高程值获取基准线上的位置
        /// </summary>
        /// <param name="z">高程值</param>
        /// <returns></returns>
        public Vector64 GetPositionFromBaseline(double z)
        {
            Vector64 p = new Vector64();
            int n = Baseline.Count;
            if (n < 1) return p;

            return GetPositionFromBaseline(z, Baseline);
        }
        /// <summary>
        /// 根据孔深位置获得对应的点坐标xyz
        /// </summary>
        /// <param name="p0">井口坐标</param>
        /// <param name="top">离井口深度</param>
        /// <param name="points">基准线坐标，按升序排列</param>
        /// <returns></returns>
        public Vector64 GetPositionFromBaseline(Vector64 p0, double top, List<Vector64> points = null)
        {
            if (points == null || points.Count < 2) return new Vector64();
            
            double len = 0;
            double err = 1E-6;//误差
            int n1 = -1, n2 = -1;
            Vector64 p, p1 = p0, p2 = p0;
            
            if (IsBaselineAscOrder)//Z升序排列
            {
                for (int i = points.Count-1; i >= 0; i--)
                {
                    p = points[i];
                    len += p1.Distance(p); //井口距离
                    if (Math.Abs(top - len) <= err) return p;
                    if (Math.Round(len, 6) > Math.Round(top, 6))
                    {  n2 = i;  p2 = p; break; }
                    else { n1 = i; p1 = p; }
                }
            }
            else //Z降序排列
            {   
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    len += p1.Distance(p); //井口距离
                    if (Math.Abs(top - len) <= err) return p;
                    if (Math.Round(len, 6) > Math.Round(top, 6))
                    { n2 = i; p2 = p; break; }
                    else { n1 = i; p1 = p; }
                }
            }

            if (n1 < 0) // 最低以下
            {
                if (IsBaselineAscOrder) { p1 = points[1]; p2 = points[0]; }
                else { p2 = points[points.Count - 2]; p2 = points[points.Count - 1]; }
            }
            if (n2 < 0) // 最高以上
            {
                if (IsBaselineAscOrder) { p2 = points[points.Count - 1]; p1 = points[points.Count - 2]; }
                else { p1 = points[1]; p2 = points[0]; }
            }

            double len1 = len - p1.Distance(p2);
            Vector64 pos = p1 + (p2 - p1) * (top - len1) / p1.Distance(p2);
            return pos;
        }


        void StrataSampling(ref List<BoreholeSamplePoint> points,int boreholeId, double step, double bkstep, StratumData s1, StratumData s2,bool target )
        {
            double h1 = s1.TopDepth;
            double h2 = s2.BottomDepth;
            double min = step * 0.001;
            Vector64 p0 = Position;
            if ( h2 - h1 <= step )
            {
                points.Add(new BoreholeSamplePoint(new Vector64(p0.X, p0.Y, p0.Z - h1 - min, s1.Value),boreholeId));
                points.Add(new BoreholeSamplePoint(new Vector64(p0.X, p0.Y, p0.Z - h2 + min, s2.Value),boreholeId));
            }
            else
            {
                double samplestep = bkstep; //实际采样间隔
                if (target) samplestep = step;

                int n =(int)( ( h2 - h1 ) / samplestep) + 1;
                samplestep = (h2 - h1) / n;
                double h = h1 - min;
                for(int i=0; i < n; i++)
                {
                    if (i == 0) h = h1 - min; //顶
                    else if (i == n - 1) h = h1 + min;//底
                    else h = h1 + samplestep * i;
                    points.Add(new BoreholeSamplePoint(new Vector64(p0.X, p0.Y, p0.Z - h, s1.Value),boreholeId));
                }                
            }
        }
        /// <summary>
        /// 钻孔地层采样，采样成点文件
        /// </summary>
        /// <param name="points">点坐标数组</param>
        /// <param name="step">采样间距</param>
        /// <param name="step">采样间距</param>
        /// <param name="bkstep">背景采样间距</param>
        /// <param name="targets">目标地层的名称，可能有多个名称</param>
        /// <param name="targetvalue">目标地层的值</param>
        /// <param name="targetvalue">背景值</param>
        /// <returns></returns>
       /* public void StrataSampling(ref List<Vector64> points,StratumDatas stratums, double step, double bkstep, List<int>targets, double targetvalue = 1, double bkvalue = 0 )
        {
            if ( Stratums.Count < 1 ) return;                        
            
            StratumData s, s1, s2;
            s1 = s2 = Stratums[0];
            s1.Value = bkvalue;

            s = stratums.GetStrataByName(s1.Name);
            if ( s != null ) s1.Value = s.Value;
            
            int i = 1;
            while (true)
            {
                if (i == Stratums.Count) //最后一层
                {
                    if (s1.Value == targetvalue) StrataSampling(ref points, step, bkstep, s1, s2, true);
                    else StrataSampling(ref points, step, bkstep, s1, s2, false);
                    break;
                }
                else
                {
                    s = Stratums[i];
                    if (targets.Contains(s.Name)) s.Value = targetvalue;
                    else s.Value = bkvalue;
                    if (s.Value == s1.Value)
                    {
                        i++;
                        s2 = s;
                        continue;
                    }
                    else //a different layer
                    {
                        if (s1.Value == targetvalue) StrataSampling(ref points, step, bkstep, s1, s2, true);
                        else StrataSampling(ref points, step, bkstep, s1, s2, false);
                        s1 = s2 = s;
                        i++;
                    }
                }
            }
           
        } */
        /// <summary>
        /// 地层采样
        /// </summary>
        /// <param name="points">数组</param>
        /// <param name="step">采样步长</param>
        /// <param name="resetValue">是否重设地层值</param>
        /// <param name="value">//重设目标地层值</param>
        //
        void StrataSampling(List<BoreholeSamplePoint> points,int boreholeid,List<StratumData>stratums, double step,bool resetValue = false,double value = 1)                                 
        {
            if (stratums.Count < 1) return; //地层为空                       
            Vector64 p1, p2;
            double val = 0;
            for(int i=0;i< stratums.Count;i++)
            {
                StratumData s = stratums[i];
                val = s.Value;  //地层值
                if (resetValue) val = value;
                p1 = s.Top;
                p2 = s.Bottom;
                p1.V = p2.V = val;                
                if ( (p1.Z - p2.Z) <= step ) 
                {   
                    p1.Z -= step * 0.1;
                    p2.Z += step * 0.1;
                    points.Add(new BoreholeSamplePoint(p1,boreholeid));
                    points.Add(new BoreholeSamplePoint(p2, boreholeid));
                }
                else
                {                    
                    for (double z = p2.Z + step*0.1; z <= p1.Z- step*0.1; z += step)
                    { 
                        Vector64 p = p2 +(p1 - p2) * (z - p2.Z) / (p1.Z - p2.Z);
                        p.V = val;
                        points.Add(new BoreholeSamplePoint(p, boreholeid));
                    }
                }
            }
        }
        

        List<StratumData> ResortStratums(List<StratumData>stratums,bool istarget,bool reset)
        {            
            List<StratumData> list1 = new List<StratumData>();//target list
            if (stratums.Count == 0 ) return list1;
            if (stratums.Count ==1) { list1.Add(stratums[0]);return list1; }
            StratumData s1 = stratums[0].Copy();
            for (int i = 1; i < stratums.Count; i++)
            {
                StratumData s = stratums[i];
                if ( Math.Abs(s1.Bottom.Z - s.Top.Z ) <= 1E-6 && 
                    (!istarget || reset || s1.Value == s.Value) )
                {
                    s1.Bottom = s.Bottom;
                }
                else
                {
                    list1.Add(s1);
                    s1 = s.Copy();
                }
            }
            list1.Add(s1);
            return list1;
        }
        public void StrataSampling(List<BoreholeSamplePoint> points, 
                                   int boreholeId,  //钻孔编号
                                   List<StratumData>targets,//目标地层,null表示采样所有地层
                                             double targetStep,   //地层采样步长
                                             double bkStep,       //背景地层采样步长（非目标地层）                                             
                                             bool samplebk = true,//是否采样背景地层
                                             bool resetValue = false,//是否重设地层值
                                             double targetvalue = 1, //重设目标地层值
                                             double bkvalue = 0)     //重设背景地层值
        {
            if ( Stratums.Count < 1 ) return; //地层为空                       
            if (targets == null || targets.Count < 1)
            {
                //全部地层采样
                StrataSampling(points, boreholeId, Stratums.Stratums,targetStep, resetValue, targetvalue);
                return;
            }

            //地层组织与合并
            List<StratumData> stratums1 = new List<StratumData>();//target list
            List<StratumData> stratums2 = new List<StratumData>();//none target list
            for (int i = 0; i < Stratums.Count; i++)
            {
                StratumData s = Stratums[i];
                bool istargrt = false;
                foreach(StratumData s1 in targets)
                {
                    if( s1.Name.ToLower() == s.Name.ToLower() )
                    {
                        istargrt = true;
                        s.Value = s1.Value;
                        break;
                    }
                }
                if (istargrt) stratums1.Add(s);
                else stratums2.Add(s);
            }

            List<StratumData> list1 = ResortStratums(stratums1, true, resetValue);
            StrataSampling(points, boreholeId,list1, targetStep, resetValue, targetvalue);
            list1.Clear();
            if (samplebk) //是否采样背景地层
            {
                List<StratumData> list2 = ResortStratums(stratums2, false, resetValue);
                StrataSampling(points, boreholeId,list2, bkStep, resetValue, bkvalue);
                list2.Clear();
            }  
            stratums1.Clear();
            stratums2.Clear();
        }

        [CategoryAttribute("Inclines"), DisplayNameAttribute("Visible")]
        public bool ShowBaseLine 
        {
            get { return boreholeAngles.Visible; }
            set { boreholeAngles.Visible = value; } 
        }
        [CategoryAttribute("Inclines"), DisplayNameAttribute("LineWidth")]
        public float BaseLineWidth { get; set; } = 1.0f;
        [CategoryAttribute("Inclines"), DisplayNameAttribute("LineColor")]
        public Color BaseLineColor { get; set; } = Color.AliceBlue;

        public override void Clear()
        {
            Baseline.Clear();
            Stratums.Clear();
            Curves.Clear();
            cylinderTriangleObj.Clear();
        }
        public CBorehole Copy()
        {
            CBorehole bh = new CBorehole();
            bh.CopyHeaderFrom(this);
            bh.Position = Position;
            bh.Baseline.AddRange(Baseline);
            bh.boreholeAngles = boreholeAngles.Copy();
            bh._Radius = _Radius;
            bh.Curves = Curves.Copy();
            bh.ShowCylinder = ShowCylinder;
            bh.CylinderColor = CylinderColor;
            bh.Stratums = Stratums.Copy();
            return bh;
        }
        public void ApplyStrtumsColorScheme(StratumDatas layers)
        {            
            for(int i=0;i< Stratums.Count;i++)
            {
                StratumData s1 = Stratums[i];
                for(int j=0;j< layers.Count;j++)
                {
                    StratumData s2 = layers[j];
                    if(s1.Name == s2.Name)
                    {
                        s1.Code = s2.Code;
                        s1.Color = s2.Color;
                        Stratums[i] = s1;
                        break;
                    }
                }
            }
        }
        public Vector64 GetPosFromBaseline(int id)
        {
            if (id >= Baseline.Count) return new Vector64();
            return Baseline[id];
        }
        public void CreateBaseLine(bool geocoord = false )
        {
            Baseline.Clear();

            if (boreholeAngles.Count > 0)
            {
                Baseline = boreholeAngles.CreateTracesLine(Position, geocoord);
            }            
            else if (Curves.Count > 0)
            {
                Baseline = CreateBaselineFromLasDepth(Curves.depthIndex, Curves.lasData, geocoord); 
            }
            else if (Stratums.Count > 0)
            {
                Baseline = CreateBaselineFromStratums(Position, geocoord);
            }
            
          //  if (geocoord)//按Z升序排列
          //      Baseline.Sort((a, b) => { return a.Z.CompareTo(b.Z); });
          //  else//按Z降序排列 
          //      Baseline.Sort((a, b) => { return b.Z.CompareTo(a.Z); });

            UpdateRange();
        }
        /// <summary>
        /// 从测斜数据创建
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="geoCoordinate"></param>
        /// <returns> </returns>
        public List<Vector64> CreateBaselineFromAngles(Vector64 p0, bool geoCoordinate = false)
        {
            return boreholeAngles.CreateTracesLine(Position);            
        }

        /// <summary>
        /// 从地层创建轨迹线
        /// </summary>
        /// <param name="p0">地面点坐标</param>
        /// <param name="geoCoordinate">暂不考虑</param>
        /// <returns></returns>
        public List<Vector64> CreateBaselineFromStratums(Vector64 p0, bool geoCoordinate = false)
        { 
            List<Vector64> traces = new List<Vector64>();
            traces.Add(p0); //地面坐标,第一层顶界面
            Vector64 p = p0; 
            for (int i = 0; i < Stratums.Count; i++)
            {
                if (i == Stratums.Count - 1)
                {
                    StratumData layer = Stratums[i];
                    p.Z = p0.Z - layer.TopDepth - layer.Thickness;
                    traces.Add(p);
                }
            }
                //for (int i = 0; i < Stratums.Count; i++)
                //{
                //    layer = Stratums[i];                
                //    p.Z = p0.Z - layer.TopDepth;
                //    if(p.Z < z)
                //    { 
                //        traces.Add(p);
                //        z= p.Z;
                //    }
                //    if(i==Stratums.Count-1)
                //    {
                //        p.Z = p.Z - layer.Thickness;
                //        traces.Add(p);
                //    }                
                //} 

                return traces;
        }

        public List<Vector64> CreateBaselineFromLasDepth(int iDepth, LasFileData lasData, bool geoCoordinate = false)
        {
            List<Vector64> traces = new List<Vector64>();
            if (lasData == null) return traces;

            List<float> depths = lasData.GetDepthData(iDepth);

            Vector64 p0 = Position;
            for (int i = 0; i < depths.Count; i++)
            {
                if (geoCoordinate) traces.Add(new Vector64(p0.X, p0.Y, p0.Z - depths[i]));
                else traces.Add(new Vector64(p0.X, p0.Y, p0.Z - depths[i]));
            }
            traces.Add(p0);
            return traces;
        }
        /// <summary>
        /// 根据Z位置计算基准线上的投影位置
        /// </summary>
        /// <param name="Z">从地面起算埋深</param>
        /// <param name="pos">返回位置</param>
        /// <returns></returns>
        int GetInterpolatedPosition( List<Vector64> traces, double Z, out Vector64 pos )
        {
            int n1 = -1, n2 = -1;
            pos = new Vector64();           
            for (int i = 0; i < traces.Count; i++)
            {
                if (Z == traces[i].Z)
                {
                    n1 = n2 = i;
                    pos = traces[i];                   
                    return i;
                }
                else if (Z < traces[i].Z)
                {
                    n2 = i;
                    break;
                }
                else if (Z > traces[i].Z) n1 = i;
            }

            if (n1 < 0)// 最低以下
            { 
                pos = traces[0]; 
                pos.Z = Z;               
                return 0; 
            }
            else if (n2 < 0)//最高以上
            { 
                pos = traces[traces.Count - 1]; 
                pos.Z = Z;               
                return traces.Count - 1; 
            }
            else
            {
                Vector64 p1 = traces[n1];
                Vector64 p2 = traces[n2];
                if (p1.Z == p2.Z) pos = p1;
                else pos = p1 + (p2 - p1) * (Z - p1.Z) / (p2.Z - p1.Z);                
                return n1;
            }
        }
        /// <summary>
        /// 根据基准线计算地层在基准线上的投影轨迹
        /// </summary>
        /// <param name="layer">地层</param>       
        /// <param name="geocoord">坐标系</param>
        public List<Vector64> CreateLayerTraces( StratumData layer, bool geocoord = false )
        {
            List<Vector64> traces = new List<Vector64>();
            double l1 = Position.Z - layer.TopDepth;
            double l2 = l1 - layer.Thickness;
            Vector64 top = GetPositionFromBaseline(Position, layer.TopDepth, Baseline);
            Vector64 bottom = GetPositionFromBaseline(Position, layer.TopDepth + layer.Thickness, Baseline);
            layer.Top = top;layer.Bottom = bottom;
            traces.Add(top);
            traces.Add(bottom);
            return traces;
            /*            
            List<Vector64> baselines = new List<Vector64>(Baseline);
            if( IsBaselineAscOrder )baselines.Reverse(); //升序按反序排列            

            

            double lensum=0, len = 0,len1,len2;
            Vector64 p, p2, p1 = baselines[0];//地面点            
            for (int i = 1; i < baselines.Count; i++ )//降序排列
            {
                p2 = baselines[i];
                len = p1.Distance(p2);
                lensum += len;
                if (layer.TopDepth == lensum) //between [p1, p]
                {
                    traces.Add(p2);

                }
                else if ( layer.TopDepth < lensum ) //between [p1, p]
                {
                    len1 = lensum - len;
                    len2 = lensum;
                    len = layer.TopDepth - len1;
                    p = p1 + (p2 - p1)*(len - len1) / (len2 - len1);
                    traces.Add(p);
                    len = layer.TopDepth + layer.Thickness;
                    p = p1 + (p2 - p1) * (len - len1) / (len2 - len1);
                    traces.Add(p);
                    break;                    
                }
                p1 = p2;
            }
            baselines.Clear();
            */
            return traces;
        }

        /// <summary>
        /// 从地层曲面中创建层数据
        /// </summary>
        /// <param name="meshes">地层界面</param>
        /// <param name="isdepth">是否是深度 or 标高Z</param>        
        public void CreateStrataFromMeshes( List <CMesh> meshes, bool topface = true,  bool isdepth = true )
        {
            if ( meshes.Count < 2 ) return;

            double x0 = Position.X;
            double y0 = Position.Y;
            double z0 = Position.Z;
            double depth = 0, z1 = 0, z = 0;
            double thickness = 0;
            
            z1 = meshes[0].GetValue(x0, y0);
            Stratums.Clear();
            for ( int i = 1; i < meshes.Count; i++ )
            {
                z = meshes[i].GetValue(x0, y0);
                if (z == CSurferGrid.blankValue ||
                    z1 == CSurferGrid.blankValue) { z1 = z; continue; }

                if (isdepth)//深度值
                {
                    depth = z1;
                    thickness = z - z1;
                }
                else//标高值
                {
                    depth = z0 - z1;
                    thickness = z1 - z;
                }
                StratumData layer = new StratumData(meshes[i].Name);
                layer.TopDepth = depth;
                layer.Thickness = thickness;
                
                if(topface) layer.Color = meshes[i-1].ObjColor;
                else layer.Color = meshes[i].ObjColor;

                Stratums.AddLayer(layer,false);
                z1 = z;
            }
        }

        [CategoryAttribute("Borehole"), DisplayNameAttribute("Position")]
        public string PositionString
        {
            get { return Position.toString(3); }
            set { Position = Vector64.Parse(value, 3); UpdateCylinder = true; RenderMode = RenderingUpdateMode.Redraw; }
        }
        float _Radius = 1.0f;
        [CategoryAttribute("Borehole"), DisplayNameAttribute("Radius")]
        public float Radius
        {
            get { return _Radius; }
            set
            {
                _Radius = value;
                BoreholeCurve cv;
                if ( Curves.UniformCurveStyle )
                {
                    for (int i = 0; i < Curves.Count; i++)
                    {
                        cv = Curves[i];
                        cv.Radius = _Radius;
                        Curves[i] = cv;
                    }                    
                    RenderMode = RenderingUpdateMode.Redraw;
                }
                UpdateCylinder = true;
                UpdateRange();
            }
        }

        public BoreholeCurves Curves { get; set; } = new BoreholeCurves();

        //测井柱体-沿基准线绘制的柱体
        public TriangleObj cylinderTriangleObj = new TriangleObj();
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Visible")]
        public bool ShowCylinder { get; set; } = true;

        public Color _CylinderColor  = Color.White; 
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Color")]
        public Color CylinderColor 
        {
            get { return _CylinderColor; }
            set
            {
                _CylinderColor = value;
                UpdateCylinder = true;
            }
        }
        public int _cylinderCircles = 10;
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("circles")]
        public int cylinderCircles
        {
            get
            {
                return _cylinderCircles;
            }
            set
            {
                _cylinderCircles = value;
                UpdateCylinder = true;
            }
        }
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("WireframeMode")]
        public override bool IsWireFrameMode
        {
            get { return _IsWireFrameMode; }
            set { _IsWireFrameMode = value; cylinderTriangleObj.IsWireFrameMode = value; }
        }
        public bool UpdateCylinder = false;
        public CylinderTypeEnum _cylinderShape = CylinderTypeEnum.Cylinder;
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Shape")]
        public CylinderTypeEnum cylinderShape 
        {
            get { return _cylinderShape; }
            set 
            {
                if( _cylinderShape != value )
                {
                    UpdateCylinder = true;
                    RenderMode = RenderingUpdateMode.Redraw;
                }
                _cylinderShape = value;               
            } 
        }

        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Enable"), Browsable(false)]
        public override bool enbaleTexture { get; set; } = false;
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Texture"), Browsable(true)]
        public override TextureStruct textureStruct { get; set; } = new TextureStruct();

        CubeModel64 GetBaseLineRange(List<Vector64>points)
        {
            CubeModel64 cube = new CubeModel64();
            Vector64 p;
            for (int i = 0; i < points.Count; i++)
            {
                p = points[i];
                if (i == 0)
                {
                    cube.X1 = cube.X2 = p.X;
                    cube.Y1 = cube.Y2 = p.Y;
                    cube.Z1 = cube.Z2 = p.Z;
                }
                else
                {
                    if (p.X < cube.X1) cube.X1 = p.X;
                    if (p.Y < cube.Y1) cube.Y1 = p.Y;
                    if (p.Z < cube.Z1) cube.Z1 = p.Z;
                    if (p.X > cube.X2) cube.X2 = p.X;
                    if (p.Y > cube.Y2) cube.Y2 = p.Y;
                    if (p.Z > cube.Z2) cube.Z2 = p.Z;
                }
            }
            return cube;
        }

        public TriangleObj CreateCylinder(bool geocoord = false)
        {
            int circle = _cylinderCircles;
            UpdateCylinder = false;
            double rad = Radius;
            
            if (Stratums.Visible || Curves.Visible) rad = Radius * 1.001;

            Vector64[] pp = CreateCircleArray(rad, circle, geocoord);
            if (pp == null) return new TriangleObj();
            int circles = pp.Length + 1;

            float u = 0, v = 0;
            cylinderTriangleObj.Clear();
            cylinderTriangleObj.CopyHeaderFrom(this);

            Vector64 p;
            CubeModel64 range = GetBaseLineRange(Baseline);
            for (int i = 0; i < Baseline.Count; i++)//基准线-z从小到大排序
            {
                p = Baseline[i];
                for (int j = 0; j < circles; j++)
                {
                    if (j == circles - 1)
                        cylinderTriangleObj.AddPoint(pp[0].X + p.X, pp[0].Y + p.Y, p.Z);
                    else
                        cylinderTriangleObj.AddPoint(pp[j].X + p.X, pp[j].Y + p.Y, p.Z);

                    u = (float)j / (circles - 1);
                    v = (float)((p.Z - range.Z1) / range.ZWidth);
                    if (geocoord) cylinderTriangleObj.AddTexture(u, 1-v);
                    else cylinderTriangleObj.AddTexture(u, v);
                }//for (int j = 0; j <= circle; j++)
            }//for (int i = 0; i < Baseline.Count; i++)
            pp = null;

            int id1, id2, id3, id4;
            for (int i = 0; i < Baseline.Count - 1; i++)
            {
                for (int j = 0; j < circles - 1; j++)
                {
                    id1 = i * circles + j;
                    id2 = id1 + 1;
                    id3 = id1 + circles;
                    id4 = id2 + circles;
                    if (geocoord)
                    {
                        cylinderTriangleObj.AddTriangleIndex(id3, id2, id1);
                        cylinderTriangleObj.AddTriangleIndex(id2, id3, id4);
                    }
                    else
                    {
                        cylinderTriangleObj.AddTriangleIndex(id1, id2, id3);
                        cylinderTriangleObj.AddTriangleIndex(id4, id3, id2);
                    }
                }
            }
            return cylinderTriangleObj;
        }
       
        Vector64[]CreateCircleArray(double rad, int circle = 10, bool geocoord = false)
        {
            Vector64[] pp = null;           
            if ( cylinderShape == CylinderTypeEnum.Cylinder )
            {
                double a, x, y;
                double angle = 2 * Math.PI / circle;                
                pp = new Vector64[circle];
                for (int i = 0; i < circle; i++)
                {
                    a = i * angle;
                    x = rad * Math.Cos(a);
                    y = rad * Math.Sin(a);
                    pp[i] = new Vector64(x, y, 0, 0);
                }
            }
            else
            {                
                pp = new Vector64[4];
                pp[0] = new Vector64(rad, 0, 0, 0);
                pp[1] = new Vector64(0, rad, 0, 0);
                pp[2] = new Vector64(-rad, 0, 0, 0);
                pp[3] = new Vector64(0, -rad, 0, 0);
            }
            return pp;
        }
        /// <summary>
        /// 从地层数据创建三角网对象
        /// 需投影到钻孔基准线上
        /// </summary>
        /// <param name="layer">地层数据</param>
        /// <param name="circle">圆柱剖分环数</param>
        /// <param name="geocoord">是否地质向下坐标系</param>
        /// <returns></returns>
        public TriangleObj CreateStratumTriangles(StratumData layer,bool geocoord = false)
        {
            Vector64[] pp = CreateCircleArray(Radius, cylinderCircles, geocoord);
            if ( pp == null ) return new TriangleObj();

            int circles = pp.Length;

            float u = 0, v;
            TriangleObj tri = new TriangleObj();
            tri.CopyHeaderFrom(this);
            tri.textureStruct = layer.textureStruct;
            tri.uniformColor = layer.Color;
            tri.Alpha = layer.Alpha;
            tri.Blend = layer.Blend;
            tri.IsWireFrameMode = layer.IsWireFrameMode;
            tri.IsUniformColor = true;

            Vector64 p;
            List<Vector64> traces = CreateLayerTraces(layer, geocoord);
            CubeModel64 range = GetBaseLineRange(traces);

            for (int k = 0; k < traces.Count; k++)
            {
                p = traces[k];
                v = (float)((p.Z - range.Z1) / range.ZWidth);//计算贴图坐标
                for (int j = 0; j < circles; j++)
                {   
                    tri.AddPoint(pp[j] + p);
                    u = (float)j / (circles - 1);
                    if (geocoord) tri.AddTexture(u, 1-v);
                    else tri.AddTexture(u, v);
                }//for (int j = 0; j <= circle; j++)
            }

            pp = null;

            // 3 4
            // 1 2
            int id1, id2, id3, id4;
            for (int k = 0; k < traces.Count - 1; k++)
            {
                for (int j = 0; j < circles; j++)
                {
                    id1 = k * circles + j;
                    id2 = id1 + 1;
                    if ( id2 >= circles ) id2 = k * circles;
                    id3 = id1 + circles;
                    id4 = id2 + circles;
                    tri.AddTriangleIndex(id1, id2, id3);
                    tri.AddTriangleIndex(id4, id3, id2);
                }
            }
            traces.Clear();
            return tri;
        }
        /// <summary>
        /// 将参数应用到所有曲线
        /// <param name="minValues">所有曲线色标最小值，null默认采用曲线数据最小值</param>
        /// <param name="maxValues">所有曲线色标最大值，null默认采用曲线数据最小值</param>
        /// <param name="updateColorScaleRange">是否改变色标范围</param>
        /// </summary>
        public void ApplyToAll(bool updateColorScaleRange = false,double[] minValues = null, double[] maxValues = null)
        {
            if (Curves.UniformCurveStyle)
            {
                BoreholeCurve cv;
                for (int i = 0; i < Curves.Count; i++)
                {
                    cv = Curves[i];
                    cv.Visible = Curves.Visible;
                    cv.Radius = Radius;
                    cv.LineWidth = Curves.LineWidth;
                    cv.Color = Curves.CurveColor;
                    cv.EnableColorScale = Curves.EnableCurveColorMap;
                    if (updateColorScaleRange)
                    {
                        if( minValues != null && maxValues != null )
                             cv.ColorScale.SetValueRange(minValues[i], maxValues[i]);
                        else cv.ColorScale.SetValueRange(cv.minValue, cv.maxValue); 
                    }
                    Curves[i] = cv;
                }
            }
            StratumData strata;
            for (int i = 0; i < Stratums.Count; i++)
            {
                strata = Stratums[i];
                strata.Alpha = Alpha;
                Stratums[i] = strata;
            }
        }
        
        public override bool SaveAs(BinaryWriter br)
        {
            try
            {
                //测井基准线，Z按由小到大排列
                SaveObjHeader(br);
                br.Write(Position.X);
                br.Write(Position.Y);
                br.Write(Position.Z);
                br.Write(Baseline.Count);
                foreach (Vector64 p in Baseline)
                {
                    br.Write(p.X); 
                    br.Write(p.Y); 
                    br.Write(p.Z);
                }
                br.Write(BaseLineWidth);
                br.Write(BaseLineColor.ToArgb());
                br.Write(_Radius);
                br.Write(ShowCylinder);
                br.Write(_CylinderColor.ToArgb());
                br.Write(_cylinderCircles);
                br.Write((int)_cylinderShape);
                if (!boreholeAngles.SaveAs(br))
                {
                    errMessage = boreholeAngles.errMessage;
                    return false;
                }
                if (!Stratums.SaveAs(br))
                {
                    errMessage = Stratums.errMessage;
                    return false;
                }
                if ( !Curves.SaveAs(br) )
                {
                    errMessage = Curves.errMessage;
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
            
        }
        public override bool LoadFrom(BinaryReader br)
        {
            try
            {
                double x, y, z;
                LoadObjHeader(br);
                x = br.ReadDouble();
                y = br.ReadDouble();
                z = br.ReadDouble();
                Position = new Vector64(x,y,z);

                Baseline.Clear();
                int n = br.ReadInt32();
                for(int i=0;i<n;i++)
                {
                    x = br.ReadDouble();
                    y = br.ReadDouble();
                    z = br.ReadDouble();
                    Baseline.Add(new Vector64(x,y,z));
                }
                BaseLineWidth = br.ReadSingle();
                BaseLineColor = Color.FromArgb(br.ReadInt32());
                Radius = br.ReadSingle();
                ShowCylinder = br.ReadBoolean();
                CylinderColor = Color.FromArgb(br.ReadInt32());
                cylinderCircles = br.ReadInt32();
                cylinderShape = (CylinderTypeEnum) br.ReadInt32();

                if (!boreholeAngles.LoadFrom(br))
                {
                    errMessage = boreholeAngles.errMessage;
                    return false;
                }                
                if( !Stratums.LoadFrom(br) )
                {
                    errMessage = Stratums.errMessage;
                    return false;
                }
                if (!Curves.LoadFrom(br))
                {
                    errMessage = Curves.errMessage;
                    return false;
                }
                UpdateRange();
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
        /// <summary>
        /// 根据基准线更新范围
        /// </summary>
        public override void UpdateRange()
        {
            minx = Position.X - Radius;
            maxx = Position.X + Radius;
            miny = Position.Y - Radius;
            maxy = Position.Y + Radius;
            minz = maxz = Position.Z;

            if ( Baseline.Count > 1)
            {
                minx = maxx = Baseline[0].X;
                miny = maxy = Baseline[0].Y;
                minz = maxz = Baseline[0].Z;
                for (int i = 0; i < Baseline.Count; i++)
                {
                    if (Baseline[i].X < minx) minx = Baseline[i].X;
                    if (Baseline[i].Y < miny) miny = Baseline[i].Y;
                    if (Baseline[i].Z < minz) minz = Baseline[i].Z;
                    if (Baseline[i].X > maxx) maxx = Baseline[i].X;
                    if (Baseline[i].Y > maxy) maxy = Baseline[i].Y;
                    if (Baseline[i].Z > maxz) maxz = Baseline[i].Z;
                }
                minx -= Radius;
                maxx += Radius;
                miny -= Radius;
                maxy += Radius;
            }   
            else if( Stratums.Count > 0 )
            {
                StratumData layer = Stratums[Stratums.Count - 1];
                minz = Position.Z -(layer.TopDepth+layer.Thickness);
            }
        }

        /// <summary>
        /// 根据钻孔井曲线更新范围
        /// </summary>
        public void UpdateRangeByLasData()
        {
            minx = Position.X - Radius;
            maxx = Position.X + Radius;
            miny = Position.Y - Radius;
            maxy = Position.Y + Radius;
            minz = maxz = Position.Z;
            if ( Curves.lasData == null ) return;
            if ( Curves.lasData.depthIndex < 0) return;
            Vector64 p = Curves.lasData.GetDepthRange();            
            minz = Position.Z - p.Y;
            maxz = Position.Z - p.X;            
        }
        /// <summary>
        /// 根据钻孔井曲线更新范围
        /// </summary>
        public CubeModel64 GetRangesByLasData()
        {
            CubeModel64 cube = new CubeModel64();
            cube.X1 = Position.X - Radius;
            cube.X2 = Position.X + Radius;
            cube.Y1 = Position.Y - Radius;
            cube.Y2 = Position.Y + Radius;
            cube.Z1 = cube.Z2 = Position.Z;
            if (Curves.lasData == null) return cube;            
            Vector64 p = Curves.lasData.GetDepthRange();
            cube.Z1 = Position.Z - p.Y;
            cube.Z2 = Position.Z - p.X;
            return cube;
        }
        public CBorehole(string name = "Untitled")
        {
            type = ShapeEnum.Borehole;
            Name = name;
            Curves.Parent = this;
            Stratums.Parent = this;
            boreholeAngles.Parent = this;
        }
        public CBorehole(LasFileData data)
        {
            type = ShapeEnum.Borehole;
            Name = data.Name;
            Position = data.m_pos;            
            Curves.lasData = data;
            Curves.Parent = this;
            Stratums.Parent = this;
            boreholeAngles.Parent = this;
            data.Parent = this;

            CreateBaseLine();

            UpdateRange();
        }   
        
    }//end class CBorehole
    public class CBoreholes: C3DObjectBaseHide
    {  
        public CBoreholes(string name = "Untitled")
        { 
            type = ShapeEnum.Boreholes;
            Name = name;
        }
        public CBorehole this[int index] 
        {
            get { return pData[index]; }
            set { pData[index] = value; }
        }
        public override bool Remove(C3DObjectBase obj)
        {
            return pData.Remove((CBorehole)obj);
        }
        public CBoreholes Copy()
        {
            CBoreholes b = new CBoreholes(Name);
            b.CopyHeaderFrom(this);            
            foreach( CBorehole bh in pData )
            {
                b.AddBorehole(bh.Copy());
            }
            b.Radius = Radius;
            b.ShowCurve = ShowCurve;
            b.ShowCylinder = ShowCylinder;
            b.ShowStratum = ShowStratum;
            b.IsUniformStyle = IsUniformStyle;
            return b;
        }
        /// <summary>
        /// 清除图像显示缓冲区ID（同时清除子对象）
        /// </summary>
        public override void ClearRenderingBuffers()
        {
            foreach(CBorehole bh in pData)
            {
                bh.ClearRenderingBuffers();
            }
            RenderingBuffers.Clear();
        }

        public override void DoOffset(double offx, double offy, double offz)
        {
            base.DoOffset(offx, offy, offz);
            CBorehole bh;
            for ( int i = 0 ; i < pData.Count; i++ )
            {
                bh = pData[i];
                bh.DoOffset(offx,offy,offz);
            }            
        }
        public override void DoRotate(double x, double y, double z)
        {
            base.DoRotate(x,y,z);
            CBorehole bh;
            for (int i = 0; i < pData.Count; i++)
            {
                bh = pData[i];
                bh.DoOffset(x,y,z);
            }            
        }
        public override void DoScale(double x, double y, double z)
        {
            base.DoScale(x, y, z);
            CBorehole bh;
            for (int i = 0; i < pData.Count; i++)
            {
                bh = pData[i];
                bh.DoScale(x, y, z);
            }            
        }
        
        public List<string>GetLasDataColumns()
        {
            List<string> columns = new List<string>();
            foreach(CBorehole bh in pData)
            {
                LasFileData las = bh.Curves.lasData;
                if (las == null) continue;
                foreach (SectionEntry s in las.CurveInformation)
                {
                    if (s.Mnemonic == las.DepthColumnName) continue;
                    if (!columns.Contains(s.Mnemonic))
                        columns.Add(s.Mnemonic);
                }
            }
            return columns;
        }
        //public LasDataProperties lasProperties = new LasDataProperties();

        [CategoryAttribute("Boreholes"), DisplayNameAttribute("Visible"), Browsable(true)]
        public override bool Visible { get; set; } = true;

        [CategoryAttribute("Boreholes"), DisplayNameAttribute("Boreholes"), Browsable(true)]
        public List<CBorehole> pData { get; set; } = new List<CBorehole>();
        
        [CategoryAttribute("Boreholes"), DisplayNameAttribute("Count"), Browsable(true)]
        public int Count { get { return pData.Count; } }

        public bool _IsUniformStyle = true;
        [CategoryAttribute("Boreholes"), DisplayNameAttribute("UniformStyle"), Browsable(true)]
        public bool IsUniformStyle
        {
            get { return _IsUniformStyle; }
            set
            {
                _IsUniformStyle = value;
                if(_IsUniformStyle)
                {
                    ApplyToAll();
                }                
            }
        }

        public float _Radius = 0.1f;
        [CategoryAttribute("Boreholes"), DisplayNameAttribute("Radius"), Browsable(true)]
        public float Radius 
        {
            get { return _Radius; }
            set 
            {
                _Radius = value;
                if (IsUniformStyle)
                {
                    foreach (CBorehole bh in pData)
                        bh.Radius = _Radius;
                }
            }
        }

        public bool _ShowCylinder = true;
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Visible"), Browsable(true)]
        public bool ShowCylinder 
        {
            get { return _ShowCylinder; }
            set { 
                   _ShowCylinder = value;
                    if (IsUniformStyle)
                    {
                      foreach (CBorehole bh in pData)
                        bh.ShowCylinder = _ShowCylinder;
                    }
            } 
        }

        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Blend"), Browsable(false)]
        public override bool Blend
        {
            get { return _Blend; }
            set
            {
                _Blend = value;
                if (IsUniformStyle)
                {
                    foreach (CBorehole bh in pData)
                        bh._Blend = _Blend;
                }
            }
        }

        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Alpha (0 ~ 1)"), Browsable(true)]
        public override float Alpha
        {
            get { return _Alpha; }
            set 
            {   _Alpha = value;
                if (IsUniformStyle)
                {
                    foreach (CBorehole bh in pData)
                        bh.Alpha = _Alpha;
                }
            }
        }

        public Color _CylinderColor  = Color.White;
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Color"), Browsable(true)]
        public Color CylinderColor
        {
            get { return _CylinderColor; }
            set {
                  _CylinderColor = value;
                if (IsUniformStyle)
                {
                    foreach (CBorehole bh in pData)
                        bh.CylinderColor = _CylinderColor;
                }
            } 
        }

        public CylinderTypeEnum _cylinderShape = CylinderTypeEnum.Cylinder;
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Shape"), Browsable(true)]
        public CylinderTypeEnum cylinderShape
        {
            get { return _cylinderShape; }
            set
            {
                _cylinderShape = value;
                if ( IsUniformStyle )
                {
                    foreach (CBorehole bh in pData)
                        bh.cylinderShape = _cylinderShape;
                }
            }
        }
       
        [CategoryAttribute("Cylinder"), DisplayNameAttribute("Texture"),Browsable(true)]
        public override TextureStruct textureStruct
        {
            get { return _textStruct; }
            set {
                 _textStruct = value;
                if (IsUniformStyle)
                {
                    foreach (CBorehole bh in pData)
                    {
                        bh.textureStruct = _textStruct.Copy();
                        bh.cylinderTriangleObj.textureStruct = bh.textureStruct;
                    }
                }
            }
        }
        public bool _ShowBaseLine = false;
        [CategoryAttribute("Inclines"), DisplayNameAttribute("Visible")]
        public bool ShowBaseLine 
        {   
            get 
            { 
                return _ShowBaseLine; 
            }
            set 
            {
                _ShowBaseLine = value;
                if (IsUniformStyle)
                {
                    foreach (CBorehole bh in pData)
                        bh.ShowBaseLine = _ShowBaseLine;
                }
            }
        }

        public float _BaseLineWidth = 1.0f;
        [CategoryAttribute("Inclines"), DisplayNameAttribute("LineWidth")]        
        public float BaseLineWidth
        {
            get { return _BaseLineWidth; }
            set
            {
                _BaseLineWidth = value;
                if (IsUniformStyle)
                {
                    foreach (CBorehole bh in pData)
                        bh.BaseLineWidth = _BaseLineWidth;
                }
            }
        }

        public Color _BaseLineColor = Color.AliceBlue;
        [CategoryAttribute("Inclines"), DisplayNameAttribute("LineColor")]        
        public Color BaseLineColor
        {
            get { return _BaseLineColor; }
            set
            {
                _BaseLineColor = value;
                if (IsUniformStyle)
                {
                    foreach (CBorehole bh in pData)
                        bh.BaseLineColor = _BaseLineColor;
                }
            }
        }

        public bool _ShowCurve = true;
        [CategoryAttribute("Curves"), DisplayNameAttribute("Visible"), Browsable(true)]
        public bool ShowCurve
        {
            get { return _ShowCurve; }
            set
            {
                _ShowCurve = value;
                if (IsUniformStyle)
                {
                    foreach (CBorehole bh in pData)
                        bh.Curves.Visible = value;
                }
            }
        }

        public Color _CurveColor = Color.Black;
        [CategoryAttribute("Curves"), DisplayNameAttribute("Color"), Browsable(true)]
        public Color CurveColor
        {
            get { return _CurveColor; }
            set
            {
                _CurveColor = value;
                if (IsUniformStyle)
                {
                    foreach (CBorehole bh in pData)
                        bh.Curves.CurveColor = value;
                }
            }
        }

        public bool _EnableCurveColorMap = true;
        [CategoryAttribute("Curves"), DisplayNameAttribute("ColorMap"), Browsable(true)]
        public bool EnableCurveColorMap 
        {
            get { return _EnableCurveColorMap; }
            set 
            {
                _EnableCurveColorMap = value;
                if (IsUniformStyle)                    
                {
                    foreach (CBorehole bh in pData)
                        bh.Curves.EnableCurveColorMap = value;                   
                }
            }
        }

        public float _CurveLineWidth = 1.0f;
        [CategoryAttribute("Curves"), DisplayNameAttribute("LineWidth"), Browsable(true)]
        public float CurveLineWidth
        {
            get { return _CurveLineWidth; }
            set
            {
                _CurveLineWidth = value;
                if (IsUniformStyle)                    
                {
                    foreach (CBorehole bh in pData)
                        bh.Curves.LineWidth = value;
                }
            }
        }
        public bool _ShowStratum = true;
        [CategoryAttribute("Strata"), DisplayNameAttribute("Visible"), Browsable(true)]
        public bool ShowStratum 
        {
            get { return _ShowStratum; }
            set
            {
                _ShowStratum = value;
                if (IsUniformStyle)
                {
                    foreach (CBorehole bh in pData)
                        bh.Stratums.Visible = value;
                }
            }
        }

        public override void UpdateRange()
        {
            if ( Count < 1 ) return;
            minx = pData[0].minx;
            miny = pData[0].miny;
            minz = pData[0].minz;
            maxx = pData[0].maxx;
            maxy = pData[0].maxy;
            maxz = pData[0].maxz;
            CBorehole borehole;
            for ( int i = 1; i < Count; i++ )
            {
                borehole = pData[i];
                if (borehole.minx < minx) minx = borehole.minx;
                if (borehole.miny < miny) miny = borehole.miny;
                if (borehole.minz < minz) minz = borehole.minz;
                if (borehole.maxx > maxx) maxx = borehole.maxx;
                if (borehole.maxy > maxy) maxy = borehole.maxy;
                if (borehole.maxz > maxz) maxz = borehole.maxz;
            }
        }

        public void AddBorehole(CBorehole bh)
        {
            pData.Add(bh);
        }
        public void ApplyStrtumsColorScheme(StratumDatas layers)
        {
            for(int i=0; i < Count;i++)
            {
                CBorehole bh = pData[i];
                bh.ApplyStrtumsColorScheme(layers);
                pData[i] = bh;
            }
        }
        public List<string>GetAllProperties()
        {
            List<string> properties = new List<string>();
            LasFileData las;
            string name;
            foreach(CBorehole bh in pData)
            {
                las = bh.Curves.lasData;
                if (las == null) continue;
                for (int i = 0; i < las.CurveInformation.Count; i++)
                { 
                    name = las.CurveInformation[i].Mnemonic;
                    if (properties.IndexOf(name) < 0)
                        properties.Add(name);
                }
            }
            return properties;
        }
        /// <summary>
        /// 参数应用到所有boreholes
        /// </summary>
        /// <param name="minValues">所有曲线色标最小值，null默认采用曲线数据最小值</param>
        /// <param name="maxValues">所有曲线色标最大值，null默认采用曲线数据最小值</param>
        /// <param name="updateColorScaleRange">是否改变色标范围</param>
        public void ApplyToAll(bool updateColorScaleRange = false, double []minValues = null,double []maxValues = null)
        {
            if (IsUniformStyle)
            {
                CBorehole bh;
                for (int i = 0; i < Count; i++)
                {
                    bh = pData[i];
                    bh.Radius = Radius;
                    bh.Curves.Visible = ShowCurve;
                    bh.Curves.CurveColor = CurveColor;
                    bh.Curves.LineWidth = CurveLineWidth;
                    bh.Curves.EnableCurveColorMap = EnableCurveColorMap;
                    bh.Curves.UniformCurveStyle = IsUniformStyle;
                    bh.IsWireFrameMode = IsWireFrameMode;
                    bh.ShowCylinder = ShowCylinder;
                    bh.CylinderColor = CylinderColor;
                    bh.textureStruct = textureStruct.Copy();
                    bh.Alpha = Alpha;

                    bh.Stratums.Visible = ShowStratum;

                    bh.offset = offset;
                    bh.scale = scale;
                    bh.rotate = rotate;

                    bh.ApplyToAll(updateColorScaleRange,minValues, maxValues);

                    pData[i] = bh;
                }
            }
        }

        public StratumDatas GetStratums()
        {
            StratumDatas stratums = new StratumDatas();
            for(int i=0;i<pData.Count;i++)
            {
                CBorehole bh = pData[i];
                for (int j = 0; j < bh.Stratums.Count; j++)
                {
                    StratumData s = bh.Stratums[j];
                    if (!stratums.IsExist(s.Name)) stratums.AddLayer(s);
                }
            }
            return stratums;
        }
        public override void Clear()
        {
            pData.Clear();            
        }
        public List<Vector64> StrataSampling(double sample_step, 
                                             double bksample_step, 
                                             StratumDatas stratums,
                                             List<int> targets, //目标地层索引 
                                             bool resetBkvalue)
        {
            List<Vector64> points = new List<Vector64>();
            for(int i=0;i < pData.Count; i++) 
            {
                CBorehole bh = pData[i];
                //bh.StrataSampling(ref points, sample_step, bksample_step,)

            }
            return null;
        }
        public override bool SaveAs(BinaryWriter br)
        {
            try
            {
                //测井基准线，Z按由小到大排列
                SaveObjHeader(br);
                br.Write(pData.Count);
                foreach (CBorehole bh in pData)
                {
                    if (!bh.SaveAs(br))
                    {
                        errMessage = bh.errMessage;
                        return false;
                    }
                }
                br.Write(_IsUniformStyle);
                br.Write(_Radius);
                br.Write(_ShowCylinder);
                br.Write(Blend);
                br.Write(Alpha);
                br.Write(_CylinderColor.ToArgb());
                br.Write((int)_cylinderShape);
                br.Write(_ShowCurve);
                br.Write(_CurveColor.ToArgb());
                br.Write(_EnableCurveColorMap);
                br.Write(_CurveLineWidth);
                br.Write(_ShowStratum);                

                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
        public override bool LoadFrom(BinaryReader br)
        {
            try
            {
                //测井基准线，Z按由小到大排列
                LoadObjHeader(br);
                int n = br.ReadInt32();
                pData.Clear();
                for (int i = 0; i < n; i++)
                {
                    CBorehole bh = new CBorehole();
                    if (!bh.LoadFrom(br))
                    {
                        errMessage = bh.errMessage;
                        return false;
                    }
                    pData.Add(bh);
                }
                _IsUniformStyle = br.ReadBoolean();
                _Radius = br.ReadSingle();
                _ShowCylinder = br.ReadBoolean();
                _Blend = br.ReadBoolean();
                _Alpha = br.ReadSingle();
                _CylinderColor = Color.FromArgb(br.ReadInt32());
                _cylinderShape = (CylinderTypeEnum)br.ReadInt32();
                _ShowCurve = br.ReadBoolean();
                _CurveColor = Color.FromArgb(br.ReadInt32());
                _EnableCurveColorMap = br.ReadBoolean();
                _CurveLineWidth = br.ReadSingle();
                _ShowStratum = br.ReadBoolean();
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
        public override bool ImportData(string path)
        {
            return base.ImportData(path);
        }
        public override bool ExportData(string path)
        {
            return base.ExportData(path);
        }        
        public bool ImportStratumsData(string path)
        {
            try 
            {
                StreamReader sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read));
                Name = Path.GetFileName(path);

                AscIIColumn asc = new AscIIColumn();
                if (!asc.Load(path)) 
                {
                    errMessage = asc.errMessage;
                    return false; 
                }
                if (asc.pData.Count < 1) 
                {
                    errMessage = "no valid data.";
                    return false; 
                }
                
                pData.Clear();
                CBorehole bh = new CBorehole("~~~");
                int start = 0;
                if (asc.IsTitleLine(asc.pData[0])) start = 1;
                stringRow row;                
                for( int i = start; i < asc.pData.Count; i++ )
                {
                    row = asc.pData[i];
                    if ( row.Count < 4 ) continue;
                    if( row[0] != bh.Name )
                    {
                        if (bh.Stratums.Count > 0) 
                        { 
                            AddBorehole(bh); 
                        }
                        bh = new CBorehole(row[0]);
                        bh.Position.X = double.Parse(row[1]);
                        bh.Position.Y = double.Parse(row[2]);
                        bh.Position.Z = double.Parse(row[3]);
                    }
                    StratumData layer = new StratumData(row[4]);
                    layer.Code = row[5];
                    layer.TopDepth = double.Parse(row[6]);
                    layer.Thickness = double.Parse(row[7]);
                    bh.Stratums.AddLayer(layer);
                }
                if (bh.Stratums.Count > 0)
                {
                    AddBorehole(bh);
                }
                sr.Close();
                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
    }


    public class BoreholesInterpolation2D
    {
        struct StrataNameStruct
        {
            public string Name;
            public int count;
            public StrataNameStruct(string name)
            {
                Name = name;
                count = 0;
            }
        }
        public string InterpolatePropertyName = ""; //插值属性
        public string errorMessage = "";
        List<StrataNameStruct> strataNames = new List<StrataNameStruct>();
        public List<CBorehole> Boreholes = new List<CBorehole>();
        List<PLine> Lines = new List<PLine>();
        double slicerLength = 0;
        double minx,miny,minz, maxx,maxy,maxz;

        public BoreholesInterpolation2D(List<CBorehole> boreholes)
        {
            Boreholes = boreholes;
        }
        
        int GetStrataStructByName(string name)
        {
            for (int k = 0; k < strataNames.Count; k++)
            {
                if (strataNames[k].Name == name)
                    return k;                
            }
            return -1;
        }

        public bool GetStrataNames()
        {
            strataNames.Clear();            
            for (int i=0;i<Boreholes.Count;i++)
            {
                CBorehole bh = Boreholes[i];
                for (int j = 0; j < bh.Stratums.Stratums.Count; j++)
                {
                    string name = bh.Stratums.Stratums[j].Name;
                    int pos = GetStrataStructByName(name);
                    if( pos < 0 ) strataNames.Add(new StrataNameStruct(name));
                    else 
                    {
                        StrataNameStruct st = strataNames[pos];
                        st.count++;
                        strataNames[pos] = st;
                    }
                }
            }
            //clear count < 2
            for( int i = strataNames.Count-1; i >= 0; i--)
            {
                if ( strataNames[i].count < 2) strataNames.RemoveAt(i);
            }

            if (strataNames.Count > 2) return true;
            else { errorMessage = "no enough strata parameters."; return false; }
        }

        void GetBoreholesRange()
        {
            double depth = 0;
            for(int i=0;i<Boreholes.Count;i++)
            {
                CBorehole bh = Boreholes[i];
                depth = bh.Curves.lasData.maxDepth;
                if (i == 0) 
                {
                    minx = maxx = bh.Position.X;
                    miny = maxy = bh.Position.Y;
                    minz = bh.Position.Z - depth;
                    maxz = bh.Position.Z;
                }
                else
                {
                    if (bh.Position.X < minx) minx = bh.Position.X;
                    if (bh.Position.Y < miny) miny = bh.Position.Y;
                    if (bh.Position.Z-depth < minz) minz = bh.Position.Z - depth;
                    if (bh.Position.X > maxx) maxx = bh.Position.X;
                    if (bh.Position.Y > maxy) maxy = bh.Position.Y;
                    if (bh.Position.Z > maxz) maxz = bh.Position.Z;
                }
            }
        }
        /// <summary>
        /// 创建地层界面平滑曲线
        /// </summary>
        /// <param name="name">地层名称</param>
        /// <returns>曲线</returns>
        List<Vector64> CreateSplineByStratum(string name)
        {
            CubicSpline spline = new CubicSpline();
            StratumData strata = null;
            List<Vector64> controlPoints = new List<Vector64>();
            double x = 0, y,len;
            Vector64 p1 = Boreholes[0].Position;
            for (int i = 0; i < Boreholes.Count; i++ )
            {
                CBorehole bh = Boreholes[i];
                strata = bh.Stratums.GetStrataByName(name);
                if (strata == null) continue;
                len = bh.Position.Distance(p1);
                x += len;
                y = bh.Position.Z - strata.TopDepth;
                controlPoints.Add(new Vector64(x, y, 0));
            }           

            if (controlPoints.Count < 2) return null;
            else return spline.CreateSpline(controlPoints);
        }
        /// <summary>
        /// 创建地层界面曲线Lines
        /// </summary>
        /// <returns></returns>
        public bool CreateStrataBorderLines()
        {
            if ( !GetStrataNames() ) return false;
            Lines.Clear();

            //地层顺序：从上到下（浅到深）
            for (int i = 0; i < strataNames.Count; i++)
            {
                List<Vector64> line = CreateSplineByStratum(strataNames[i].Name);
                if (line == null) continue;
                Lines.Add(new PLine(line));
            }
            if (Lines.Count < 1)
            {
                errorMessage = "no valid border lines created.";
                return false;
            }
            //地层变为逆序：Z从小到大（深到浅）
            Lines.Reverse();
            return true;
        }
        enum StrataInterfaceEnum 
        {
            Upper = 0, //上部
            Between = 1,//下部
            Lower = 2,//中间
            Boder = 3,//界面上
        }
        /// <summary>
        /// 剖面上任意点x,z所在的地层界限
        /// 地层为逆序：Z从小到大（深到浅）
        /// </summary>
        /// <param name="x0">横向距离0开始</param>
        /// <param name="z0">纵向高程</param>
        /// <param name="n1">z1地层序号</param>
        /// <param name="n2">z2地层序号</param>
        /// <returns></returns>
        StrataInterfaceEnum GetFromBorderLines(double x0, double z0, out double z1, out double z2, out int n1,out int n2)
        {
            n1 = n2 = -1;
            z1 = z2 = 0;
            int n = Lines.Count;        
            double z; 
            //将所有的交点的值存入数组并排序-升序
            List<double> zvalues = new List<double>();
            for( int i = 0; i < Lines.Count; i++ )
            {
                PLine line = Lines[i];
                z = line.GetInterpolatedPositionByDistance(x0).Y;
                zvalues.Add(z);
            }

            //找出z0所在的地层
            if (z0 < zvalues[0]) 
            { 
                n1 = -1; 
                n2 = 0;
                z2 = zvalues[0];
                return StrataInterfaceEnum.Lower; 
            }
            else if (z0 > zvalues[n - 1]) 
            { 
                n1 = n - 1; 
                n2 = - 1;
                z1 = zvalues[n - 1];
                return StrataInterfaceEnum.Upper; 
            }
            else 
            {
                for (int i = 0; i < n; i++)
                {
                    if (z0 == zvalues[i]) 
                    { 
                        n1 = n2 = i;
                        z1 = z2 = zvalues[i];
                        return StrataInterfaceEnum.Boder; 
                    }
                    else if (z0 > zvalues[i]) n1 = i;
                    else if (z0 < zvalues[i]) 
                    { 
                        n2 = i;
                        z1 = zvalues[n1];
                        z2 = zvalues[n2];
                        break;
                    }
                }
                return StrataInterfaceEnum.Between;
            }            
        }

        double GetInterpolatedUpper(double x0, double z0, double z1)
        {
            InversePower ip = new InversePower();
            LasFileData las;

            double dh = z0 - z1;
            
            //曲线经过了取反操作Reverse()
            string upperName = strataNames[0].Name;
            int id;
            double h, val;
            StratumData strata;
            foreach (CBorehole bh in Boreholes)
            {
                las = bh.Curves.lasData;
                if (las == null) continue;
                strata = bh.Stratums.GetStrataByName(upperName);
                if (strata == null) continue;
                h = strata.TopDepth - dh;
                id = las.GetIndexFromDepth(h);//对应深度
                if (id < 0) continue;
                //对应深度对应属性值
                val = las.GetValue(id, InterpolatePropertyName);
                if (!double.IsNaN(val)) ip.AddPoint(x0, 0, bh.Position.Z - h, val);
            }
            return ip.GetInterpolatedValue(x0, 0, z0);
        }

        double GetInterpolatedLower(double x0, double z0, double z2)
        {
            InversePower ip = new InversePower();
            LasFileData las;
            double dh = z2 - z0;
            //曲线经过了取反操作Reverse()
            string lowerName = strataNames[strataNames.Count-1].Name;
            int id;
            double h, val;
            StratumData strata;
            foreach (CBorehole bh in Boreholes)
            {
                las = bh.Curves.lasData;
                if (las == null) continue;
                strata = bh.Stratums.GetStrataByName(lowerName);
                if (strata == null) continue;
                h = strata.TopDepth + dh;
                id = las.GetIndexFromDepth(h);//对应深度
                if (id < 0) continue;

                //对应深度对应属性值
                val = las.GetValue(id, InterpolatePropertyName);
                if (!double.IsNaN(val)) ip.AddPoint(x0, 0, bh.Position.Z - h, val);
            }
            return ip.GetInterpolatedValue(x0, 0, z0);
        }
        double GetInterpolatedBorder(double x0, double z0, double z1,int n1)
        {
            InversePower ip = new InversePower();
            LasFileData las;

            //曲线经过了取反操作Reverse()
            int nlayer = strataNames.Count - 1;
            string lowerName = strataNames[nlayer - n1].Name;
            int id;
            double h, val;
            StratumData strata;
            foreach (CBorehole bh in Boreholes)
            {
                las = bh.Curves.lasData;
                if (las == null) continue;
                strata = bh.Stratums.GetStrataByName(lowerName);
                if (strata == null) continue;
                h = strata.TopDepth;
                id = las.GetIndexFromDepth(h);//对应深度
                if (id < 0) continue;
                //对应深度对应属性值
                val = las.GetValue(id, InterpolatePropertyName);
                if (!double.IsNaN(val)) ip.AddPoint(x0, 0, bh.Position.Z - h, val);
            }
            return ip.GetInterpolatedValue(x0, 0, z0);
        }
        double GetInterpolatedBetween(double x0, double z0, double z1, double z2, int n1,int n2)
        {
            InversePower ip = new InversePower();
            LasFileData las;
            double percent = (z0 - z1) / (z2 - z1) * 100;
            int nLayer = strataNames.Count -1;            
            //曲线经过了取反操作Reverse()
            string upperName = strataNames[nLayer - n2].Name;
            string lowerName = strataNames[nLayer - n1].Name;
            int id;
            double h, h1, h2,val;            
            StratumData strata;
            foreach (CBorehole bh in Boreholes)
            {
                las = bh.Curves.lasData;
                if (las == null) continue;
                strata = bh.Stratums.GetStrataByName(upperName);
                if (strata == null) continue;
                h1 = strata.TopDepth;
                strata = bh.Stratums.GetStrataByName(lowerName);
                if (strata == null) continue;
                h2 = strata.TopDepth;
                h = h1 + (h2 - h1) * (100 - percent) / 100;
                id = las.GetIndexFromDepth(h);//对应深度        
                if (id < 0) continue;
                //对应深度对应属性值
                val = las.GetValue(id, InterpolatePropertyName);
                if( !double.IsNaN(val) )ip.AddPoint(x0, 0, bh.Position.Z-h, val);
            }
            return ip.GetInterpolatedValue(x0, 0, z0);            
        }
        
        public CSlicer CreateLasSlicer()
        {
            if ( Boreholes.Count < 1 ) return null;
            if ( !CreateStrataBorderLines() ) return null;
            
            GetBoreholesRange();

            //创建基准线
            Vector64 p1 = new Vector64(), p;
            CSlicer slicer = new CSlicer();
            slicerLength = 0;
            
            for (int i = 0; i < Boreholes.Count; i++)
            {
                CBorehole bh = Boreholes[i];
                p = bh.Position;                
                if ( i > 0) slicerLength += p.Distance(p1);
                slicer.pBaseLine.Add(p);
                p1 = p;
            }
            //创建插值网格
            slicer.nRow = 101;
            double dd = (maxz - minz) / (slicer.nRow - 1);
            slicer.CreateInterpolatedBaseline(4*dd);
            slicer.nCol = slicer.pInterpolatedBaseLine.Count;

            slicer.Plan = planEnum.XOY;
            slicer.minHeight = minz;
            slicer.maxHeight = maxz;

            slicer.pBlankTable = new bool[slicer.nRow * slicer.nCol];
            for (int i = 0; i < slicer.pBlankTable.Length; i++)
                slicer.pBlankTable[i] = false;            

            double x, z,v = 0, z1, z2;
            double dx = slicerLength / (slicer.nCol - 1);
            double dz = (maxz - minz) / (slicer.nRow - 1);
            int n1, n2;
            StrataInterfaceEnum ret;
            slicer.pData = new Vector64[slicer.nRow* slicer.nCol];
            for (int i = 0; i < slicer.nRow; i++)
            {
                z = minz + dz * i;
                for (int j = 0; j < slicer.nCol; j++)
                {
                    x = j * dx;                                       
                    ret = GetFromBorderLines(x, z, out z1, out z2, out n1, out n2);
                    if ( ret == StrataInterfaceEnum.Between )
                        v = GetInterpolatedBetween(x, z, z1, z2, n1, n2);
                    else if(ret == StrataInterfaceEnum.Upper)
                        v = GetInterpolatedUpper(x, z, z1);
                    else if (ret == StrataInterfaceEnum.Lower)
                        v = GetInterpolatedLower(x, z, z2);
                    else if (ret == StrataInterfaceEnum.Boder)
                        v = GetInterpolatedBorder(x, z, z1, n1);

                    if (double.IsNaN(v)) 
                    { 
                        v = C3DData.m_BlankedValue;
                        slicer.pBlankTable[i * slicer.nCol + j] = true;
                    }
                    p = slicer.pInterpolatedBaseLine[j];
                    p.Z = z;
                    p.V = v;
                    slicer.pData[ i * slicer.nCol + j ] = p;
                }
            }
            slicer.UpdateRange();
            slicer.ColorScale.SetValueRange(slicer.minv,slicer.maxv);
            return slicer;
        }

    }//public class BoreholesInterpolation2D
    
}
