using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Design;
using GlmNet;
namespace DataCollection
{
    
    public class ScatteredPoints:C3DObjectBase
    {
        public List<Vector32> points = new List<Vector32>();
        public string FilePath = "";
        public int Interval = 1;
        public long TotalRows = 0;
        public int Demension = 4; //x,y,z,v    
        
        public int XGridNum = 0; //grids on x direction
        public int YGridNum = 0; //grids on y direction
        public int ZGridNum = 0; //grids on z direction

        protected char[] remarkChars = new char[] { '/', '#', '!' };
        protected char[] splitChars = new char[] { ' ', ',', '\t' };
        protected char[] trimChars = new char[] { ' ', '\t','"' };
        
        [CategoryAttribute("Display"), DisplayNameAttribute("Count")]
        public int Count { get { return points.Count; } }

        public double _VisibleX1 = 0f;
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible X1")]
        public double VisibleX1 
        {   get { return _VisibleX1;} 
            set { _VisibleX1 = value;
                if (VisibleX2 > VisibleX1) RenderMode = RenderingUpdateMode.Redraw; } 
        }

        public double _VisibleX2 = 0f;
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible X2")]
        public double VisibleX2
        {
            get { return _VisibleX2; }
            set
            {
                _VisibleX2 = value;
                if (VisibleX2 > VisibleX1) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public double _VisibleY1 = 0f;
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible Y1")]
        public double VisibleY1
        {
            get { return _VisibleY1; }
            set
            {
                _VisibleY1 = value;
                if (VisibleY2 > VisibleY1) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public double _VisibleY2 = 0f;
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible Y2")]
        public double VisibleY2
        {
            get { return _VisibleY2; }
            set
            {
                _VisibleY2 = value;
                if (VisibleY2 > VisibleY1) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public double _VisibleZ1 = 0f;
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible Z1")]
        public double VisibleZ1
        {
            get { return _VisibleZ1; }
            set
            {
                _VisibleZ1 = value;
                if (VisibleZ2 > VisibleZ1) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public double _VisibleZ2 = 0f;
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible Z2")]
        public double VisibleZ2
        {
            get { return _VisibleZ2; }
            set
            {
                _VisibleZ2 = value;
                if (VisibleZ2 > VisibleZ1) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public double _VisibleValue1 = 0f;
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible Value1")]        
        public double VisibleValue1
        {
            get { return _VisibleValue1; }
            set
            {
                _VisibleValue1 = value;
                if (VisibleValue2 > VisibleValue1) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public double _VisibleValue2 = 0f;
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible Value2")]
        public double VisibleValue2
        {
            get { return _VisibleValue2; }
            set
            {
                _VisibleValue2 = value;
                if (VisibleValue2 > VisibleValue1) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public CColorScale _ColorScale = new CColorScale();
        [CategoryAttribute("Color"), DisplayNameAttribute("Color Map")]
        [Editor(typeof(ColorEditor), typeof(UITypeEditor)), TypeConverter(typeof(ColorScaleConverter))]
        public CColorScale ColorScale 
        {
            get { return _ColorScale; }
            set
            {
                _ColorScale = value;
                if (!IsUniformColor) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public bool _IsUniformColor = false;
        [CategoryAttribute("Color"), DisplayNameAttribute("Uniform Color")]
        public bool IsUniformColor
        {
            get { return _IsUniformColor; }
            set
            {
                _IsUniformColor = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public Color _ObjColor = Color.Blue;
        [CategoryAttribute("Color"), DisplayNameAttribute("Color")]
        public Color ObjColor
        {
            get { return _ObjColor; }
            set
            {
                _ObjColor = value;
                if(IsUniformColor) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public bool _ShowLines = false;
        [CategoryAttribute("Lines"), DisplayNameAttribute("Enabled")]
        public bool ShowLines
        {
            get { return _ShowLines; }
            set
            {
                _ShowLines = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public float _lineWidth = 1.0f;
        [CategoryAttribute("Lines"), DisplayNameAttribute("Width")]
        public float lineWidth
        {
            get { return _lineWidth; }
            set
            {
                _lineWidth = value;
                if( ShowLines )RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public bool _lineClosed = false;
        [CategoryAttribute("Lines"), DisplayNameAttribute("Closed")]
        public bool lineClosed
        {
            get { return _lineClosed; }
            set
            {
                _lineClosed = value;
                if (ShowLines) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public bool _ShowSymbol = true;
        [CategoryAttribute("Symbol"), DisplayNameAttribute("Enable")]
        public bool ShowSymbol
        {
            get { return _ShowSymbol; }
            set
            {
                _ShowSymbol = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        Cone coneStyle = new Cone();
        Box3D boxStyle = new Box3D(new Vector32(),1,1,1);
        CCylinderExt cylinderStyle = new CCylinderExt();
        Arrow2D arrow2DStyle = new Arrow2D();        

        public SymbolEnum _Symbol = SymbolEnum.Box;
        [CategoryAttribute("Symbol"), DisplayNameAttribute("Type")]
        public SymbolEnum Symbol
        {
            get { return _Symbol; }
            set
            {
                _Symbol = value;
                if (_Symbol == SymbolEnum.Box)
                {
                    UniformStyle = boxStyle;
                }
                else if (_Symbol == SymbolEnum.Cone)
                {
                    UniformStyle = coneStyle;
                }
                else if (_Symbol == SymbolEnum.Arrow)
                    UniformStyle = arrow2DStyle;
                else if (_Symbol == SymbolEnum.Cylinder)
                    UniformStyle = cylinderStyle;
                
                if (ShowSymbol) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        [CategoryAttribute("Symbol"), DisplayNameAttribute("Style"), TypeConverter(typeof(PropertySymbolStyleConverter))]
        [Editor(typeof(PropertyStyleEditor), typeof(UITypeEditor))]
        public Symbol3D UniformStyle { get; set; } = new Symbol3D();


        [CategoryAttribute("Symbol"), DisplayNameAttribute("Interval")]
        public int SymbolInterval 
        { 
            get { return Interval; } 
            set { Interval = value; if (ShowSymbol) RenderMode = RenderingUpdateMode.Redraw; } 
        } 
        
        private float _xWidth = 0.01f;
        [CategoryAttribute("Symbol"), DisplayNameAttribute("Size X")]
        public float xWidth 
        {   get { return _xWidth; } 
            set { _xWidth = value; 
                  UpdateLabelTextSize();
                  if (ShowSymbol) RenderMode = RenderingUpdateMode.Redraw;
                } 
        }

        private float _yWidth = 0.01f;
        [CategoryAttribute("Symbol"), DisplayNameAttribute("Size Y")]
        public float yWidth
        {
            get { return _yWidth; }
            set
            {
                _yWidth = value;
                UpdateLabelTextSize();
                if (ShowSymbol) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        private float _zWidth = 0.01f;
        [CategoryAttribute("Symbol"), DisplayNameAttribute("Size Z")]
        public float zWidth
        {
            get { return _zWidth; }
            set
            {
                _zWidth = value;
                UpdateLabelTextSize();
                if (ShowSymbol) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public int _horizontalRounds = 10;
        [CategoryAttribute("Symbol"), DisplayNameAttribute("Horizontal Rounds")]
        public int horizontalRounds
        {
            get { return _horizontalRounds; }
            set
            {
                _horizontalRounds = value;
                if (ShowSymbol && Symbol == SymbolEnum.Cone) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public int _verticalRounds = 4;
        [CategoryAttribute("Symbol"), DisplayNameAttribute("Vertical Rounds")]
        public int verticalRounds
        {
            get { return _verticalRounds; }
            set
            {
                _verticalRounds = value;
                if (ShowSymbol && Symbol == SymbolEnum.Cone) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public vec3 symbolOffset = new vec3(0,0,0);
        [CategoryAttribute("Symbol"), DisplayNameAttribute("Translation")]
        public string symbolOffsetString
        {
            get { return symbolOffset.x + "," + symbolOffset.y + "," + symbolOffset.z; }
            set
            {
                Vector32 p;
                if (Vector32.TryParse(value, out p, 3))
                {
                    symbolOffset = new vec3(p.X, p.Y, p.Z);                   
                    RenderMode = RenderingUpdateMode.Redraw;
                }
            }
        }
        
        public override float GetOrderedAlpha()
        {
            if (ShowLabel)
            {
                if ( Labels.Count > 0 && textStyle.BackgroundTransparent )
                    return 0.5f; 
            }
            return 1;
        }

        public bool _ShowLabel = false;
        [CategoryAttribute("Labels"), DisplayNameAttribute("Visible")]
        public bool ShowLabel
        {
            get { return _ShowLabel; }
            set
            {
                _ShowLabel = value;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }
        public bool _IsUniformStyle = true;
        [CategoryAttribute("Labels"), DisplayNameAttribute("Is Unifrom Style")]
        public bool IsUniformStyle
        {
            get { return _IsUniformStyle; }
            set
            {
                _IsUniformStyle = value;
                if(ShowLabel) RenderMode = RenderingUpdateMode.Redraw;
            }
        }
        public bool IsTextStyleChanged()
        {
            if(ShowLabel)
            {
                if (_textStyle.DifferentFrom(_oldertextStyle))
                    return true;
            }
            return false;
        }

        public TexturedTextStyle _oldertextStyle = new TexturedTextStyle();
        public TexturedTextStyle _textStyle = new TexturedTextStyle();
        [CategoryAttribute("Labels"), DisplayNameAttribute("Unifrom Style")]
        [Editor(typeof(TexturedTextStyleEditor), typeof(UITypeEditor)), TypeConverter(typeof(TexturedTextStyleConverter))]
        public TexturedTextStyle textStyle
        {
            get { return _textStyle; }
            set
            {
                _oldertextStyle = _textStyle;
                _textStyle = value;
                if (ShowLabel && IsUniformStyle) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public List<TexturedText> _Labels = new List<TexturedText>();
        [CategoryAttribute("Labels"), DisplayNameAttribute("Labels")]
        public List<TexturedText> Labels
        {
            get { return _Labels; }
            set
            {
                _Labels = value;
                if (ShowLabel) RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        public int _LabelInterval = 1;
        [CategoryAttribute("Labels"), DisplayNameAttribute("Interval")]
        public int LabelInterval
        {
            get { return _LabelInterval; }
            set
            {
                _LabelInterval = value;
                if (ShowLabel) RenderMode = RenderingUpdateMode.Redraw;
            }
        }        
       
        public ScatteredPoints()
        {
            type = ShapeEnum.Points;            
        }
        public ScatteredPoints(string _name)
        {
            type = ShapeEnum.Points;
            Name = _name;
        }
        public ScatteredPoints( List<Vector32> _points )
        {
            type = ShapeEnum.Points;
            foreach(Vector32 p in _points)
            {
                points.Add(p);
            }
            UpdateRange();
        }
        public ScatteredPoints(List<Vector64> _points)
        {
            type = ShapeEnum.Points;
            foreach (Vector64 p in _points)
            {
                points.Add(p);
            }
            UpdateRange();            
        }        

        public override void Clear()
        {
            points.Clear();
            Labels.Clear();
        }
        public void AddLabel(string text,Vector64 p)
        {
            TexturedText tt = new TexturedText(text,p,10);
            tt.textStyle.HorizontalAlignment = TextHorizontalAlignment.Center;
            tt.textStyle.VerticalAlignment = TextVerticalAlignment.Center;
            Labels.Add(tt);
        }
        public void AddLabel(string text, double x,double y,double z,double v = 0)
        {
            Vector64 p = new Vector64(x,y,z,v);
            AddLabel(text,p);
        }
        public void UpdateLabelTextSize()
        {
            double xsize = xWidth * MaxWidth;
            double zsize = zWidth * MaxWidth;
            
            textStyle.LabelLength = (float)xsize;
            textStyle.Rotate = rotate;
            textStyle.Scale = scale;
            textStyle.Offset = offset;
            textStyle.Offset.z += (float)(1.1 * zsize / 2);

            TexturedText tt;
            for (int i = 0; i < Labels.Count; i++)
            {
                tt = Labels[i];
                tt.textStyle = textStyle.Copy();
                Labels[i] = tt;
            }
        }
        public void UpdateLabelTextSize(float size,float offsetx = 0,float offsety=0, float offsetz = 0)
        {
            TexturedText tt;
            for ( int i = 0; i < Labels.Count; i++ )
            {
                tt = Labels[i];
                tt.textStyle.LabelLength = size;
                tt.Start = tt.Start + new Vector64(offsetx,offsety,0.01);
                Labels[i] = tt;
            }
        }
        //是否需要隐藏显示该点
        public bool IsHidePoint(Vector32 p)
        {
            return IsHidePoint(p.x,p.y,p.z,p.v);
        }
        public bool IsHidePoint(double x,double y,double z,double val)
        {
            if (  (VisibleValue1 < VisibleValue2 ) &&
                  (val < VisibleValue1 || val > VisibleValue2) )
            {
                return true;                
            }
            if (  ( VisibleX1 < VisibleX2 ) &&
                  ( x < VisibleX1 || x > VisibleX2 ) )
            {
                return true;
            }
            if ((VisibleY1 < VisibleY2) &&
                  (y < VisibleY1 || y > VisibleY2))
            {
                return true;
            }
            if ((VisibleZ1 < VisibleZ2) &&
                  (z < VisibleZ1 || z > VisibleZ2))
            {
                return true;
            }
            return false;
        }
        public override bool SaveAs(BinaryWriter br)
        {
            if ( !SaveObjHeader(br) ) return false;
            try 
            {
                br.Write(points.Count);
                foreach(Vector32 p in points)
                {
                    br.Write(p.x);
                    br.Write(p.y);
                    br.Write(p.z);
                    br.Write(p.v);
                }
                br.Write(Interval);
                br.Write((Int64)TotalRows);
                br.Write(Demension);

                //Symbols
                br.Write((int)Symbol);
                br.Write(SymbolInterval);
                br.Write(horizontalRounds);
                br.Write(verticalRounds);
                br.Write(xWidth);
                br.Write(yWidth);
                br.Write(zWidth);
                br.Write(VisibleX1);
                br.Write(VisibleX2);
                br.Write(VisibleY1);
                br.Write(VisibleY2);
                br.Write(VisibleZ1);
                br.Write(VisibleZ2);
                br.Write(VisibleValue1);
                br.Write(VisibleValue2);
                br.Write(IsUniformColor);

                br.Write(ShowSymbol);//2025-8-28

                br.Write(ObjColor.ToArgb());
                ColorScale.WriteBinary(br);
                
                //br.Write(symbolOffset.x);
                //br.Write(symbolOffset.y);
                //br.Write(symbolOffset.z);
                //br.Write(symbolRotate.x);
                //br.Write(symbolRotate.y);
                //br.Write(symbolRotate.z);

                //DrawLines
                br.Write(ShowLines);
                br.Write(lineWidth);
                br.Write(lineClosed);

                //Labels
                br.Write(ShowLabel);
                br.Write(LabelInterval);
                br.Write(IsUniformStyle);
                textStyle.SaveBinary(br);
                br.Write(Labels.Count);
                foreach (TexturedText tt in Labels)
                {
                    tt.SaveAs(br);
                }
                return true;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }

        public override bool LoadFrom(BinaryReader br)
        {
            if (!LoadObjHeader(br)) return false;
            try
            {
                points.Clear();
                float x, y, z, v;
                int n = br.ReadInt32();
                for(int i = 0; i < n; i++ )
                {
                    x = br.ReadSingle();
                    y = br.ReadSingle();
                    z = br.ReadSingle();
                    v = br.ReadSingle();
                    points.Add(new Vector32(x, y, z, v));
                }

                Interval = br.ReadInt32();
                TotalRows = br.ReadInt64();
                Demension = br.ReadInt32();

                //Symbols
                Symbol = (SymbolEnum) br.ReadInt32();
                SymbolInterval = br.ReadInt32();
                horizontalRounds = br.ReadInt32();
                verticalRounds = br.ReadInt32();
                xWidth = br.ReadSingle();
                yWidth = br.ReadSingle();
                zWidth = br.ReadSingle();
                VisibleX1 = br.ReadDouble();
                VisibleX2 = br.ReadDouble();
                VisibleY1 = br.ReadDouble();
                VisibleY2 = br.ReadDouble();
                VisibleZ1 = br.ReadDouble();
                VisibleZ2 = br.ReadDouble();
                VisibleValue1 = br.ReadDouble();
                VisibleValue2 = br.ReadDouble();
                IsUniformColor = br.ReadBoolean();

                if(C3DData.DataVersion >=1.31f)ShowSymbol = br.ReadBoolean();//2025-8-28

                ObjColor = Color.FromArgb(br.ReadInt32());
                ColorScale.LoadBinary(br);

                //symbolOffset.x = br.ReadSingle();
                //symbolOffset.y = br.ReadSingle();
                //symbolOffset.z = br.ReadSingle();
                //symbolRotate.x = br.ReadSingle();
                //symbolRotate.y = br.ReadSingle();
                //symbolRotate.z = br.ReadSingle();

                //DrawLines
                ShowLines = br.ReadBoolean();
                lineWidth = br.ReadSingle();
                lineClosed = br.ReadBoolean();
                
                //Labels
                ShowLabel = br.ReadBoolean();
                LabelInterval = br.ReadInt32();
                IsUniformStyle = br.ReadBoolean();
                textStyle.LoadFrom(br);
                Labels.Clear();
                int nlabel = br.ReadInt32();                
                for(int i=0;i<nlabel;i++)
                {
                    TexturedText tt = new TexturedText("");
                    tt.LoadFrom(br);
                    Labels.Add(tt);
                }

                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public void AddPoint(double x,double y,double z,double value)
        {
            /*
            if(points.Count == 0)
            {
                minx = maxx = x;
                miny = maxy = y;
                minz = maxz = z;
                minv = maxv = value;
            }
            else
            {
                if (x < minx) minx = x;
                if (y < miny) miny = y;
                if (z < minz) minz = z;
                if (value < minv) minv = value;
                if (x > maxx) maxx = x;
                if (y > maxy) maxy = y;
                if (z > maxz) maxz = z;
                if (value > maxv) maxv = value;
            }
            */
            points.Add(new Vector32((float)x, (float)y, (float)z, (float)value));
            //VisibleValue1 = minv;
            //VisibleValue2 = maxv;
        }
        public void AddPoint(Vector32 p)
        {
            AddPoint(p.x, p.y, p.z, p.v);
        }
        public override void Normalize()
        {
            UpdateRange();

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = TransformedPoint(points[i]);
            }
            scale = new  GlmNet.vec3(1, 1, 1);
            offset = new GlmNet.vec3(0, 0, 0);
            rotate = new GlmNet.vec3(0, 0, 0);

            UpdateRange();
        }
        int GetGridFromArray(double[] xx)
        {
            int k = 0;
            for (int i = 0; i < xx.Length; i++)
            {
                if (!double.IsNaN(xx[i]))
                    k++;
            }
            return k;
        }
        double GetMinStepFromArray(double[] xx)
        {
            int k = 0;
            double last = 0, cur = 0, minstep = 1.0E20;
            for (int i = 0; i < xx.Length; i++)
            {
                if ( !double.IsNaN(xx[i]) )
                {
                    cur = xx[i];
                    if (k == 0) 
                    {
                        last = cur = xx[i];
                    }
                    else
                    {
                        cur = xx[i];
                        if (cur - last < minstep)
                            minstep = cur - last;
                    }
                    last = cur;
                    k++;
                }
            }
            return 0;
        }
        public bool GetCubeSizeFromPoints(int n = 10001)
        {
            try
            {
                double[] xx = new double[n];
                double[] yy = new double[n];
                double[] zz = new double[n];
                for (int i = 0; i < n; i++)
                {
                    xx[i] = double.NaN;
                    yy[i] = double.NaN;
                    zz[i] = double.NaN;
                }
                double xs = (maxx - minx) / (n - 1);
                double ys = (maxy - miny) / (n - 1);
                double zs = (maxz - minz) / (n - 1);
                int ix, iy, iz;
                foreach (Vector32 p in points)
                {
                    ix = (int)((p.x - minx) / xs);
                    xx[ix] = p.x;
                    iy = (int)((p.y - miny) / ys);
                    yy[iy] = p.y;
                    iz = (int)((p.z - minz) / zs);
                    zz[iz] = p.z;
                }
                XGridNum = GetGridFromArray(xx);
                YGridNum = GetGridFromArray(yy);
                ZGridNum = GetGridFromArray(zz);
                /* 
                 double xsize = GetMinStepFromArray(xx);
                 double ysize = GetMinStepFromArray(yy);
                 double zsize = GetMinStepFromArray(zz);
                 double xsize1 = (maxx - minx) / (nx0 - 1);
                 double ysize1 = (maxy - miny) / (ny0 - 1);
                 double zsize1 = (maxz - minz) / (nz0 - 1);
                 xsize = Math.Max(xsize, xsize1);
                 ysize = Math.Max(ysize, ysize1);
                 zsize = Math.Max(zsize, zsize1);
                */
                
                xWidth = (float)(1.0 / (XGridNum - 1) * (maxx - minx) / MaxWidth);
                yWidth = (float)(1.0 / (YGridNum - 1) * (maxy - miny) / MaxWidth);
                zWidth = (float)(1.0 / (ZGridNum - 1) * (maxz - minz) / MaxWidth);
                xx = null;
                yy = null;
                zz = null;
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }

        public override float[] toValuesArray()
        {
            int n = points.Count;
            if (n < 1) return null;
            float[] data = new float[n];
            int k = 0;
            foreach(Vector32 p in points)
            {
                data[k++] = p.v;
            }
            return data;
        }
        public override void SetColorRange()
        {
            ColorScale.SetValueRange(minv,maxv);
        }
        public override void SetColorRange(double v1,double v2)
        {
            ColorScale.SetValueRange(v1, v2);
        }
        public override void UpdateRange()
        {
            Vector32 p;
            for(int i=0;i<points.Count;i++)
            {
                p = points[i];
                if (i == 0)
                {
                    minx = maxx = p.x;
                    miny = maxy = p.y;
                    minz = maxz = p.z;
                    minv = maxv = p.v;
                }
                else
                {
                    if (p.x < minx) minx = p.x;
                    if (p.y < miny) miny = p.y;
                    if (p.z < minz) minz = p.z;
                    if (p.v < minv) minv = p.v;
                    if (p.x > maxx) maxx = p.x;
                    if (p.y > maxy) maxy = p.y;
                    if (p.z > maxz) maxz = p.z;
                    if (p.v > maxv) maxv = p.v;
                }
            }// for(int i=0;i<points.Count;i++)

            VisibleValue1 = minv;
            VisibleValue2 = maxv;
        }//public override void UpdateRange()
        protected bool IsRemarkedLine(string line)
        {
            if (line.Length < 1) return true;
            char c = line[0];
            for (int i = 0; i < remarkChars.Length; i++)
            {
                if (remarkChars[i] == c) return true;
            }
            return false;
        }
        public long PreLoad(string filename)
        {
            long i = 0;
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string ss;
                while ((ss = sr.ReadLine()) != null)
                {
                    ss = ss.Trim(trimChars);
                    if (ss.Length < 3) continue;
                    if (ss[0] == '/' || ss[0] == '#' || ss[0] == '!') continue;
                    if (char.IsLetter(ss[0])) continue;
                    i++;
                }
                sr.Close();
                fs.Close();
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return -1;
            }
            return i;
        }
        public override bool ImportData(string filename)
        {
            FilePath = filename;
            long MB = 1024 * 1024;
            TotalRows = PreLoad(filename);
            double available = PhysicalMemory.GetAvailableMemoryMB() * 0.5;
            double required = TotalRows * 16 / MB;
            Interval = 1;
            if ( required > available )
            {
                //errMsg = "no enough memory. Available:" + available + ";required:" + required;
                //return false;
                Interval = (int)( required / available + 1);
            }
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string ss;
                long i = 0;
                double x = 0, y = 0, z = 0, v = 0;
                string[] str;
                while ((ss = sr.ReadLine()) != null)
                {
                    ss = ss.Trim(trimChars);
                    if (ss.Length < 3) continue;
                    if (ss.Length < 3) continue;
                    if (ss[0] == '/' || ss[0] == '#') continue;
                    str = ss.Split(new Char[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (str == null || str.Length < 3) continue;

                    if (i == 0)//首行是否有注释
                    {
                        if (str.Length > 0) if (!double.TryParse(str[0], out x)) continue;
                        if (str.Length > 1) if (!double.TryParse(str[1], out y)) continue;
                        if (str.Length > 2) if (!double.TryParse(str[2], out z)) continue;
                        if (str.Length > 3) if (!double.TryParse(str[3], out v)) continue;
                    }

                    if ( i % Interval == 0 )
                    {
                        if (str.Length > 0) x = Convert.ToDouble(str[0]);    //x
                        if (str.Length > 1) y = Convert.ToDouble(str[1]);    //y
                        if (str.Length > 2) z = Convert.ToDouble(str[2]);    //z
                        if (str.Length > 3) v = Convert.ToDouble(str[3]);    //v

                        if ( !C3DData.IsBlankValue(x) && !C3DData.IsBlankValue(y) &&
                             !C3DData.IsBlankValue(z) && !C3DData.IsBlankValue(v) )
                        {
                            Demension = str.Length > 4 ? 4 : str.Length;
                            if (Demension == 1) { y = z = v = x; }
                            else if (Demension == 2) { z = v = y; }
                            else if (Demension == 3) { v = z; }
                            AddPoint(x, y, z, v);
                        }
                       
                    }
                    //检查内存是否可用
                    if (points.Count>= 20000)
                    {
                        //break;
                    }
                    str = null;
                    i++;
                }

                sr.Close();
                fs.Close();

                UpdateRange();
                GetCubeSizeFromPoints();
                ColorScale.SetValueRange(minv, maxv);

                if (points.Count > 0) return true;
                else return false;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return false;
            }            
        }
        public virtual bool IsTitleLine(string line,int xcol,int ycol,int zcol,int vcol)
        {
            double v;           
            string[] ss = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length > xcol) { if (!double.TryParse(ss[xcol], out v)) { ss = null; return true; } }
            if (ss.Length > ycol) { if (!double.TryParse(ss[ycol], out v)) { ss = null; return true; } }
            if (ss.Length > zcol) { if (!double.TryParse(ss[zcol], out v)) { ss = null; return true; } }
            if (ss.Length > vcol) { if (!double.TryParse(ss[vcol], out v)) { ss = null; return true; } }           
            return false;
        }
        public virtual bool LoadFrom(string path, int xcol, int ycol, int zcol, int vcol, int label = -1, int interval = 1,bool ignoreFirstrow = false)
        {
            StreamReader sr = null;
            try
            {
                Clear();
                sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.Default);
                
                long i = 0;
                string line = "";
                float x = 0, y = 0, z = 0, v = 0;
                string text = "";
                bool istitled = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length < 1) continue;//空行
                    line = line.Trim(trimChars);
                    if (line.Length < 1) continue;
                    if (IsRemarkedLine(line)) continue; //注释行

                    //首行，检测是否标题行
                    if (i == 0 )
                    {           
                        if(ignoreFirstrow) istitled = true;
                        else 
                        {
                            if ( IsTitleLine(line, xcol, ycol, zcol, vcol) )//ignore
                                istitled = true;
                        }
                        if (istitled)//ignore it
                        {
                            i++;
                            continue;
                        }
                    }

                    //非首行，
                    if (i % Interval == 0)
                    {
                        string[] ss = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                        //string[] ss = line.Split(splitChars);
                        if (xcol >= 0 && xcol < ss.Length) x = float.Parse(ss[xcol]);
                        if (ycol >= 0 && ycol < ss.Length) y = float.Parse(ss[ycol]);
                        if (zcol >= 0 && zcol < ss.Length) z = float.Parse(ss[zcol]);
                        if (vcol >= 0 && vcol < ss.Length) v = float.Parse(ss[vcol]);
                        if ( !C3DData.IsBlankValue(v) )
                        {
                            if (label >= 0 && label < ss.Length) text = ss[label].Trim(trimChars);
                            AddPoint(new Vector32(x, y, z, v));
                            if (label >= 0) AddLabel(text, x, y, z, v);
                        }
                    }

                    i++;
                }
                sr.Close();
                UpdateRange();
                GetCubeSizeFromPoints();
                SetColorRange();
                return true;
            }
            catch (Exception e)
            {
                if (sr != null) sr.Close();
                errMessage = e.Message;
                return false;
            }
        }

        public virtual bool MatchedFrom(List<Vector32> coordinates)
        {
            int n = coordinates.Count;
            if (n < 1) return false;

            double dist1 = coordinates[0].V;
            double dist2 = coordinates[n - 1].V;
            
            Vector32 p, p1, p2,pp;
            int n1, n2;            
            for (int i = 0; i < points.Count; i++ )
            {
                p = points[i];                
                if( Vector32.SearchBoder(p.X, coordinates, out n1, out n2, dist1, dist2) )
                {
                    p1 = coordinates[n1];
                    p2 = coordinates[n2];
                    
                    if (n1 == n2) 
                    { 
                        p.X = p1.X; 
                        p.Y = p1.Y;
                        p.Z = p.Z + p1.Z;
                    }
                    else
                    {
                        pp = p1 + (p2 - p1) * (p.X - p1.V) / (p2.V - p1.V);
                        p.X = pp.X;
                        p.Y = pp.Y;
                        p.Z = p.Z + pp.Z;
                    }
                    points[i] = p;
                }
            }
            return true;
        }

        public virtual bool LoadMatchedFrom(string path, List<Vector32> coordinates, int xcol, int ycol, int zcol, int vcol, int label = -1, int interval = 1, bool ignoreFirstrow = false)
        {
            try
            {
                Clear();
                StreamReader sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read), Encoding.Default);

                long i = 0;
                string line = "";
                float x = 0, y = 0, z = 0, v = 0;
                string text = "";
                bool istitled = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length < 1) continue;//空行
                    line = line.Trim(trimChars);
                    if (line.Length < 1) continue;
                    if (IsRemarkedLine(line)) continue; //注释行

                    //首行，检测是否标题行
                    if (i == 0)
                    {
                        if (ignoreFirstrow) istitled = true;
                        else
                        {
                            if (IsTitleLine(line, xcol, ycol, zcol, vcol))//ignore
                                istitled = true;
                        }
                        if (istitled)//ignore it
                        {
                            i++;
                            continue;
                        }
                    }

                    //非首行，
                    if (i % Interval == 0)
                    {
                        string[] ss = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                        if (xcol >= 0 && xcol < ss.Length) x = float.Parse(ss[xcol]);
                        if (zcol >= 0 && zcol < ss.Length) z = float.Parse(ss[zcol]);
                        if (vcol >= 0 && vcol < ss.Length) v = float.Parse(ss[vcol]);
                        y = x;
                        if (!C3DData.IsBlankValue(v))
                        {
                            if (label >= 0 && label < ss.Length) text = ss[label].Trim(trimChars);
                            AddPoint(new Vector32(x, y, z, v));
                            if (label >= 0) AddLabel(text, x, y, z, v);
                        }
                    }

                    i++;
                }
                sr.Close();

                MatchedFrom(coordinates);

                UpdateRange();
                GetCubeSizeFromPoints();
                SetColorRange();
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public override bool ExportData(string path)
        {
            try
            {
                string line = "";

                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                StreamWriter wr = new StreamWriter(fs);

                Name = Path.GetFileName(path);
                if( Demension == 1) line = "X";
                else if (Demension == 2) line = "X,Y";
                else if (Demension == 3) line = "X,Y,Z";
                else if (Demension == 4) line = "X,Y,Z,Value";
                
                wr.WriteLine(line);

                Vector32 p;
                for (int i = 0; i < points.Count; i++)
                {
                    p = points[i];
                    if (Demension == 1) line = p.x.ToString();
                    else if (Demension == 2) line = p.x + "," + p.y;
                    else if (Demension == 3) line = p.x + "," + p.y + "," + p.z;
                    else if (Demension == 4) line = p.x + "," + p.y + "," + p.z + "," + p.v;
                    wr.WriteLine(line);
                }
                wr.Close();
                fs.Close();
                return true;
            }
            catch(Exception e)
            {
                errMessage = "Export failed.\n" + e.Message;
                return false;
            }
        }
        public Color GetColor(double v)
        {
            if (IsUniformColor) return ObjColor;
            else return ColorScale.GetColor(v);
        }

    }//class
}
