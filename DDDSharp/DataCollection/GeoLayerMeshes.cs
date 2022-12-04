using System;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Design;
using System.Collections.Generic;
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
        //范围一致,网格一致
        public List<GeoMesh> Meshes = new List<GeoMesh>();

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
            if (!SaveObjHeader(br)) return false;
            try
            {
                br.Write(nRow);
                br.Write(nCol);
                if (nRow > 0 && nCol > 0)
                {
                    foreach (Vector64 p in pData)
                    {
                        br.Write(p.x);
                        br.Write(p.y);
                        br.Write(p.z);
                        br.Write(p.v);
                    }
                }

                br.Write(IsTop);
                br.Write(Depth);
                br.Write(ShowSurface);
                br.Write(IsFilled);
                //C3DData.SaveString(br, textureImgFile);
                //C3DData.SaveString(br, SurroundingTexturFile);
                SurfaceTexture.Save(br);
                SurroundingTexture.Save(br);

                br.Write(ShowMesh);
                br.Write(ShowContour);
                br.Write(LineWidth);
                br.Write(LineColor.ToArgb());
                br.Write(ObjColor.ToArgb());
                br.Write(EnableColorLevel);
                ColorScale.WriteBinary(br);
                marchingCube.SaveBinary(br);
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
            double x, y, z, v;
            try
            {
                nRow = br.ReadInt32();
                nCol = br.ReadInt32();
                if (nRow > 0 && nCol > 0)
                {
                    pData = null;
                    pData = new Vector64[nRow * nCol];
                    for (int i = 0; i < pData.Length; i++)
                    {
                        x = br.ReadDouble();
                        y = br.ReadDouble();
                        z = br.ReadDouble();
                        v = br.ReadDouble();
                        pData[i] = new Vector64(x, y, z, v);
                    }
                }

                IsTop = br.ReadBoolean();
                Depth = br.ReadSingle();
                ShowSurface = br.ReadBoolean();
                IsFilled =br.ReadBoolean();
                //textureImgFile = C3DData.LoadString(br);
                //SurroundingTexturFile = C3DData.LoadString(br);
                SurfaceTexture.Load(br);
                SurroundingTexture.Load(br);

                ShowMesh = br.ReadBoolean();
                ShowContour = br.ReadBoolean();
                LineWidth = br.ReadSingle();
                LineColor = Color.FromArgb(br.ReadInt32());
                ObjColor = Color.FromArgb(br.ReadInt32());
                EnableColorLevel = br.ReadBoolean();

                ColorScale.LoadBinary(br);
                marchingCube.LoadBinary(br);
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
