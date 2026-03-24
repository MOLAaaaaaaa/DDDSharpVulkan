using GlmNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextReaderWriter;

namespace DataCollection
{    
    /// <summary>
    /// 地质地层（不含断层）
    /// </summary>
    public class GeoLayerMeshes: C3DObjectBase
    {
        public int[,] pBlankedPointIndexes = null;   //白化点坐标随意
        public List<vec3> pBlankedPoints = new List<vec3>();
        public List<Vector64>Boundaries = new List<Vector64>();

        //范围一致,网格一致
        public List<GeoMesh> Meshes = new List<GeoMesh>();
        public int nRow 
        {  
            get
            {
                if (Meshes.Count < 1) return 0;
                return Meshes[0].nRow;
            }
        }
        public int nCol
        {
            get
            {
                if (Meshes.Count < 1) return 0;
                return Meshes[0].nCol;
            }
        }
        bool _IsFlatBottom = false;
        [CategoryAttribute("Layer"), DisplayNameAttribute("Flat Bottom")]//底界面为平底        
        public bool IsFlatBottom 
        { 
            get { return _IsFlatBottom; }
            set 
            {
                _IsFlatBottom = value;
                UpdateNeeded = true;
                RenderMode = RenderingUpdateMode.Redraw;
            } 
        }

        bool _IsFlatTop = false;
        [CategoryAttribute("Layer"), DisplayNameAttribute("Flat Top")]//顶界面为平底
        public bool IsFlatTop
        {
            get { return _IsFlatTop; }
            set
            {
                _IsFlatTop = value;
                UpdateNeeded = true;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }

        bool _IsTop = false;//地层由顶界面组成，否则为底界面
        [CategoryAttribute("Layer"), DisplayNameAttribute("Top Faces")]
        public virtual bool IsTop 
        {
            get { return _IsTop; }
            set
            {
                _IsTop = value;
                UpdateNeeded = true;
                RenderMode = RenderingUpdateMode.Redraw;
            }
        }
        
        public int Count { get { return Meshes.Count; } }
        
        public GeoLayerMeshes()
        {
            type = ShapeEnum.GeoLayerMeshes;
        }
        public GeoLayerMeshes(string _name)
        {
            type = ShapeEnum.GeoLayerMeshes;
            Name = _name;
        }

        public GeoMesh this[int index] 
        {
            get { return Meshes[index]; }
            set { Meshes[index] = value; }
        }
        public void Add(GeoMesh mesh) 
        {
            Meshes.Add(mesh);
        }
        /// <summary>
        /// 地层从上到下排列
        /// </summary>
        /// <param name="meshes"></param>
        public void AddRange(List<GeoMesh> meshes)
        {
            Meshes.AddRange(meshes);
        }

        public void AddRange(GeoMesh[]meshes)
        {
            Meshes.AddRange(meshes);
        }

        public override bool Remove(C3DObjectBase obj)
        {
            return Meshes.Remove((GeoMesh)obj);
        }

        public bool TrimWith(Polygon2D poly, bool keepOuter)
        {
            for(int i=0;i<Meshes.Count;i++)
            {
                GeoMesh mesh = (GeoMesh)Meshes[i];
                mesh.TrimWith(poly, keepOuter);
                Meshes[i] = mesh;
            }            
            return true;
        }
        /// <summary>
        /// 判断点是否在地层曲面之间
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsPointOnMeshes(Vector64 p)
        {
            GeoMesh top, bottom;
            if (Meshes.Count < 1 ) return false;
            else if(Meshes.Count ==1 )
            {
                top = Meshes[0];
                top.GetVerticIndex(p.X, p.Y, out int ix, out int iy);
                if (top.IsBlankedGrid(iy, iy)) return false;
                else return true;
            }
            else if (Meshes.Count > 1)
            {
                for (int i = 0; i < Meshes.Count-1; i++)
                {
                    top = Meshes[i];
                    bottom = Meshes[i+1];
                    top.GetVerticIndex(p.X, p.Y, out int ix, out int iy);
                    if (top.IsBlankedGrid(iy, ix)) return false;
                    if (bottom.IsBlankedGrid(iy, ix)) return false;
                    Vector64 p2 = top[iy, ix];
                    Vector64 p1 = bottom[iy, ix];                    
                    if (p.Z >= p1.Z && p.Z <= p2.Z) return true;                   
                }
                return false;
            }
            return false;
        }
        public void UpdateBlanked(GeoMesh top, GeoMesh bottom)
        {
            for(int i=0;i< top.nRow;i++)
            {
                for (int j = 0; j < top.nRow; j++)
                {
                    Vector64 p1 = top[i, j];
                    Vector64 p2 = bottom[i, j];
                    if (p1.Z < p2.Z) 
                    { 
                        p1.V = p2.V = double.NaN;
                        top[i, j] = p1;
                        bottom[i, j] = p2;
                    }                    
                }
            }
        }
        public void UpdateBlanked()
        {
            GeoMesh top, bottom;            
            for(int i=0;i<Meshes.Count-1; i++)//按z升序排列
            {
                top = (GeoMesh)Meshes[i];
                bottom = (GeoMesh)Meshes[i+1];                
                UpdateBlanked(top,bottom);
            }
        }
        
        int GetBlankedPointIndex(int irow, int icol)
        {
            if(pBlankedPointIndexes == null) return -1;
            return pBlankedPointIndexes[irow, icol];
        }

        public double GetPolarAngle(Point p,Point center)
        {
            double dx = p.X - center.X;
            double dy = p.Y - center.Y;
            return Math.Atan2(dy, dx); // 范围：-π ~ π
        }
        /// <summary>
        /// 提取网格中值为1的边界点，并按极角排序组成闭合多边形
        /// 边界点定义：值为1，且至少一个四邻域（上下左右）节点值为0（或越界）
        /// </summary>
        /// <param name="points">网格节点坐标，[ix, iy] 对应x/y方向索引</param>
        /// <param name="values">网格节点值，[ix, iy] 与points索引一一对应</param>
        /// <returns>闭合多边形的边界点列表（首尾点重合，保证闭合）</returns>
        /// <exception cref="ArgumentNullException">输入数组为空</exception>
        /// <exception cref="ArgumentException">points与values维度不匹配/无有效边界点</exception>
        public List<Vector64> ExtractBoundaryPolygon()
        {   
            // 2. 定义四邻域偏移（上、下、左、右）
            (int dix, int diy)[] neighbors = new (int, int)[]
            {
                (-1, 0), // 上（x-1）
                (1, 0),  // 下（x+1）
                (0, -1), // 左（y-1）
                (0, 1)   // 右（y+1）
            };

            // 3. 筛选边界点：值为1，且至少一个邻域值为0（或邻域越界）
            List<Point> boundaryPoints = new List<Point>();
            for (int ix = 0; ix < nCol; ix++)
            {
                for (int iy = 0; iy < nRow; iy++)
                {
                    // 当前节点值为1才可能是边界点
                    if ( IsBlanked(iy,ix) )continue;

                    // 检查四邻域是否有值为0的节点（或越界，越界视为值为0）
                    bool isBoundary = false;
                    foreach (var (dix, diy) in neighbors)
                    {
                        int nx = ix + dix;
                        int ny = iy + diy;
                        // 邻域越界 → 视为外部（值为0）
                        if (nx < 0 || nx >= nCol || ny < 0 || ny >= nRow)
                        {
                            isBoundary = true;
                            break;
                        }
                        // 邻域值为0 → 是边界点
                        if (IsBlanked(iy, ix))
                        {
                            isBoundary = true;
                            break;
                        }
                    }
                    if (isBoundary)boundaryPoints.Add(new Point(ix, iy));
                }
            }

            // 校验：是否找到边界点
            if (boundaryPoints.Count == 0)
                throw new ArgumentException("未找到值为1的边界点，请检查网格数据");

            // 4. 对边界点排序：按极角排序（以1区域的中心为原点，逆时针排序）
            // 第一步：计算1区域的中心（所有值为1的节点的坐标均值）
            Point center = CalculateValue1Center(nCol, nRow);

            // 第二步：按极角逆时针排序
            boundaryPoints = boundaryPoints
                .OrderBy(p => GetPolarAngle(p,center))
                .ToList();

            // 5. 保证多边形闭合：首尾点重合
            if (boundaryPoints.Count > 0 && !IsPointEqual(boundaryPoints.First(), boundaryPoints.Last()))
                boundaryPoints.Add(boundaryPoints.First());

            List<Vector64> points = new List<Vector64>();
            for(int i=0;i<boundaryPoints.Count;i++)
            {
                Point p = boundaryPoints[i];
                CMesh mesh = Meshes[0];
                points.Add(mesh.GetPoint(p.Y, p.X));
            }
            boundaryPoints.Clear();
            return points;
        }

        /// <summary>
        /// 计算所有值为1的网格节点的中心坐标（均值）
        /// </summary>
        private Point CalculateValue1Center(int ixCount, int iyCount)
        {
            int sumX = 0, sumY = 0;
            int count = 0;

            for (int ix = 0; ix < ixCount; ix++)
            {
                for (int iy = 0; iy < iyCount; iy++)
                {
                    if ( !IsBlanked(iy, ix) )
                    {
                        sumX += ix;
                        sumY += iy;
                        count++;
                    }
                }
            }

            return new Point(sumX / count, sumY / count);
        }

        /// <summary>
        /// 比较两个点是否相等（浮点精度兼容）
        /// </summary>
        private static bool IsPointEqual(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) < 1e-8 && Math.Abs(p1.Y - p2.Y) < 1e-8;
        }
        
        public override void UpdateRange()
        {
            CMesh mesh;
            for ( int i = 0; i < Meshes.Count; i++ )
            {
                mesh = Meshes[i];
                if(i == 0) 
                {
                    minx = mesh.minx;
                    miny = mesh.miny;
                    minz = mesh.minz;
                    minv = mesh.minv;
                }
                else
                {
                    if (mesh.minx < minx) minx = mesh.minx;
                    if (mesh.miny < miny) miny = mesh.miny;
                    if (mesh.minz < minz) minz = mesh.minz;
                    if (mesh.minv < minv) minv = mesh.minv;

                    if (mesh.maxx > maxx) maxx = mesh.maxx;
                    if (mesh.maxy > maxy) maxy = mesh.maxy;
                    if (mesh.maxz > maxz) maxz = mesh.maxz;
                    if (mesh.maxv > maxv) maxv = mesh.maxv;
                }
            }
            if ( Meshes.Count > 0 )
            {
                if ( !IsTop )//底界面
                {
                    maxz = maxz + Meshes[0].Depth;
                }
                else //顶界面
                {
                    minz = minz - Meshes[Count - 1].Depth;
                }
            }
        }
        public override bool SaveAs(BinaryWriter br)
        {
            if (!SaveObjHeader(br)) return false;
            try
            {
                br.Write(IsTop);
                br.Write(IsFlatTop);
                br.Write(IsFlatBottom);
                br.Write(Count);
                for ( int i = 0; i < Count; i++ )
                {
                    Meshes[i].SaveAs(br);
                }
                return true;
            }
            catch (Exception e)
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
                IsTop = br.ReadBoolean();
                IsFlatTop = br.ReadBoolean();
                IsFlatBottom = br.ReadBoolean();
                Meshes.Clear();
                int n = br.ReadInt32();
                for (int i = 0; i < n; i++)
                {
                    GeoMesh mesh = new GeoMesh();
                    if (mesh.LoadFrom(br)) Meshes.Add(mesh);
                    else
                    {
                        errMessage = mesh.errMessage;
                        return false;
                    }
                }

                UpdateRange();
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }

    }
    public class GeoMesh : CMesh
    {        
        [CategoryAttribute("Display"), DisplayNameAttribute("Visible")]
        public override bool Visible 
        {
            get { return _Visible; } 
            set { _Visible = value; RenderMode = RenderingUpdateMode.Redraw; } 
        }

        [CategoryAttribute("Display"), DisplayNameAttribute("Show Surface")]
        public bool ShowSurface { get; set; } = true;
        
        [CategoryAttribute("Display"), DisplayNameAttribute("Filled")]
        public bool IsFilled { get; set; } = true;

        [CategoryAttribute("Display"), DisplayNameAttribute("Color")]
        public Color Color { get { return ObjColor; } set { ObjColor = value; } }

        [CategoryAttribute("Texture"), DisplayNameAttribute("Enable"), Browsable(false)]
        public override bool enbaleTexture { get; set; }

        [CategoryAttribute("Texture"), DisplayNameAttribute("TexureFile"), Browsable(false)]
        public override TextureStruct textureStruct { get; set; } = new TextureStruct();

        [CategoryAttribute("Texture"), DisplayNameAttribute("Surface")]//表面贴图
        [Editor(typeof(PropertyStyleEditor), typeof(UITypeEditor)), TypeConverter(typeof(PropertyStyleConverter))]
        public TextureStruct SurfaceTexture { get { return textureStruct; } set { textureStruct = value; } }

        [CategoryAttribute("Texture"), DisplayNameAttribute("Surrounding")]//侧面贴图
        [Editor(typeof(PropertyStyleEditor), typeof(UITypeEditor)), TypeConverter(typeof(PropertyStyleConverter))]
        public TextureStruct SurroundingTexture { get; set; } = new TextureStruct();
        [CategoryAttribute("Layer"), DisplayNameAttribute("IsTop")]
        public bool IsTop { get; set; } = false;
        [CategoryAttribute("Layer"), DisplayNameAttribute("Depth")]
        public float Depth { get; set; } = 0;      
        public GeoMesh()
        {
            type = ShapeEnum.GeoMesh;
        }
        public override bool ExportData(string path)
        {
            try
            {
                string line;
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                StreamWriter wr = new StreamWriter(fs);

                Name = Path.GetFileName(path);

                line = "[GEOMESH]";
                wr.WriteLine(line);

                line = "Row = " + nRow;
                wr.WriteLine(line);

                line = "Column = " + nCol;
                wr.WriteLine(line);

                line = "Color = " + ObjColor.ToArgb();
                wr.WriteLine(line);
                
                for (int i = 0; i < nRow; i++)
                    for (int j = 0; j < nCol; j++)
                    {
                        wr.WriteLine(pData[i * nCol + j].toString(4));
                    }

                line = "[Properties]";
                wr.WriteLine(line);
                
                line = "TopSurface = " + IsTop;
                wr.WriteLine(line);
                line = "Depth = " + Depth;
                wr.WriteLine(line);                
                line = "ShowSurface = " + ShowSurface;
                wr.WriteLine(line);
                line = "Filled = " + IsFilled;
                wr.WriteLine(line);
                line = "SurfaceTexture = " + SurfaceTexture.TextureFile;
                wr.WriteLine(line);
                line = "SurroundingTextur = " + SurroundingTexture.TextureFile;
                wr.WriteLine(line);

                line = "EnableColorLevel = " + EnableColorLevel;
                wr.WriteLine(line);
                line = "ShowMesh = " + ShowMesh;
                wr.WriteLine(line);
                line = "ShowCounterLine = " + ShowContour;
                wr.WriteLine(line);

                //"[TRANSLATIONS]"
                ExportTranslations(wr);

                //"[ColorMap]"
                if (EnableColorLevel)
                {
                    ColorScale.WriteStream(wr);
                }

                wr.Close();
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                errMessage = "export to file failed." + ex.Message;
                return false;
            }
        }
        public override bool ImportData(string path)
        {
            try
            {
                string str;

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                Name = Path.GetFileName(path);

                Clear();

                if (!AscIIProfile.SeekSection("[MESH]", ref sr))
                {
                    errMessage = "not a valid mesh file";
                    sr.Close();
                    fs.Close();
                    return false;
                }

                int color = 0;
                AscIIProfile.ReadIntValue(sr, "Row", out nRow);
                AscIIProfile.ReadIntValue(sr, "Column", out nCol);
                AscIIProfile.ReadIntValue(sr, "Color", out color);
                ObjColor = Color.FromArgb(color);

                if (nRow < 1 || nCol < 1)
                {
                    errMessage = "reading data error.";
                    sr.Close();
                    fs.Close();
                    return false;
                }

                pData = new Vector64[nRow * nCol];

                for (int i = 0; i < nRow; i++)
                    for (int j = 0; j < nCol; j++)
                    {
                        str = AscIIProfile.ReadLine(sr);
                        AddPoint(i, j, Vector64.Parse(str, 4));
                    }

                if (AscIIProfile.SeekSection("[Properties]", ref sr))
                {
                    IsTop = AscIIProfile.ReadBoolValue(sr, "TopSurface");
                    Depth = AscIIProfile.ReadFloatValue(sr, "Depth");
                    ShowSurface = AscIIProfile.ReadBoolValue(sr, "ShowSurface");
                    IsFilled = AscIIProfile.ReadBoolValue(sr, "Filled");
                    SurfaceTexture.TextureFile = AscIIProfile.ReadStringValue(sr, "textureImgFile");
                    SurroundingTexture.TextureFile = AscIIProfile.ReadStringValue(sr, "SurroundingTexturFile");

                    EnableColorLevel = AscIIProfile.ReadBoolValue(sr, "EnableColorLevel");
                    ShowMesh = AscIIProfile.ReadBoolValue(sr, "ShowMesh");
                    ShowContour = AscIIProfile.ReadBoolValue(sr, "ShowCounterLine");
                }

                if (AscIIProfile.SeekSection("[TRANSLATIONS]", ref sr))
                {
                    ImportTranslations(sr);
                }
                else UpdateRange();

                if (AscIIProfile.SeekSection("[ColorMap]", ref sr))
                {
                    ColorScale = new CColorScale(minv, maxv);
                    ColorScale.ReadStream(ref sr);
                    ColorScale.SetValueRange(minv, maxv);
                }

                sr.Close();
                fs.Close();
                return true;
            }
            catch
            {
                errMessage = "open file failed.";
                return false;
            }
        }
        public override bool SaveAs(BinaryWriter br)
        {
            if (!base.SaveAs(br)) return false;
            try
            {
                br.Write(IsTop);
                br.Write(Depth);
                br.Write(ShowSurface);
                br.Write(IsFilled);
                SurfaceTexture.Save(br);
                SurroundingTexture.Save(br);
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        public override bool LoadFrom(BinaryReader br)
        {
            if (!base.LoadFrom(br)) return false;            
            try
            {
                IsTop = br.ReadBoolean();
                Depth = br.ReadSingle();
                ShowSurface = br.ReadBoolean();
                IsFilled = br.ReadBoolean();
                SurfaceTexture.Load(br);
                SurroundingTexture.Load(br);
                UpdateRange();
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        /////////////////////////////////////
    }//Class GeoMesh
}
