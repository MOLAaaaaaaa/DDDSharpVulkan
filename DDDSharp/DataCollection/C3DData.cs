using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Graphics3D;
using GlmNet;
using DDDSharp.DataCollection;
using DDDSharp.Dialogs;

namespace DataCollection
{
    public enum ObjectType
    {
        i3DGrid,
        iSurface,
        iScatterPoint,
        None
    };
    public class C3DData
    {
        //Register information
        static public string UserID = "demo001";
        static public string Password = "12345678";
        static public string UserName = "Chengdu University of Technology";
        static public int CopyNum = 5;
        //

        //for marching cubes test only         
        static public int m_TestValue = 0;
        static public double m_BlankedValue = 1.70141E+038;

        static public List<C3DObjectBase> lastLoadeds = new List<C3DObjectBase>();
        static public C3DObjectBase lastLoaded = null;
        static public C3DObjectBase objSelected = null;
        static public C3DObjectBase objSubSelected = null;
        static public C3DObjectBase objSubSubSelected = null;
        static public StratumDatas Stratums = new StratumDatas();//地层配色方案
        
        static public int objectKeyIndex = 0; //当前对象索引号，保障每个对象不重复

        //对象字典列表，为快速增删对象
        static public Dictionary<int, C3DObjectBase> objectsDiction = new Dictionary<int, C3DObjectBase>();
        static public int objectsCount { get { return objectsDiction.Count; } }
        static public TreeStructData dataTrees = new TreeStructData("Objects", TreeNodeType.Folder);        

        static public List<int> UpdateIndicesList = new List<int>();//需要更新显示列表 

        static public string errMessage = "";
        static public Random globalRandom = new Random(DateTime.Now.Millisecond);

        //menu and bar        
        static public gDrawMode nPolygonMode = gDrawMode.Fill;
        static public bool bShowOuterBox = true;    //是否绘制矩形外框        
        static public bool bShowSelectedOuterBox = true;//是否绘制选中物体的矩形外框
        static public bool bShowDirectionArrow = true;
        static public bool bShowLightPositions = false;


        static public CGraphic3D graphics3D = new CGraphic3D();

        static public bool DemoVersion = false;
        static public int validYear = 2020;
        static public int validMonth = 2;
        static public int validDay = 20;
        static public int expiredDays = 200;

        static public float DataVersion = 1.0f; // 载入数据版本

        //static public float Version = 1.30f;      // 当前版本，Current Version 2024-12-25
        //static public float Version = 1.31f;        // 当前版本，Current Version 2025-8-28
        //static public float Version = 1.32f;        // 当前版本，Current Version 2025-11-14
        static public float Version = 1.33f;        // 当前版本，Current Version 2026-1-9
        /// <summary>
        /// 版本历史：
        /// Version 1.31
        /// ScatteredPoints  ShowSymbol 未写入 2025-8-13
        /// PolygonSlicer Backimage Enable 未写入
        /// TextureStruct TransparentInverse 未写入
        /// TextureStruct TransparentColors 写入/载入未修改
        /// Version 1.32
        /// triangleObje 写入Uniform Color and Wireframe info
        /// </summary>
        static public bool IsDataModified = false;        

        //Current Project
        static public string CurrentProjectFile = "";

        //Recording
        static public bool ControlKeyDown = false;
        static public bool AltKeyDown = false;
        static public bool Recording = false;
        static public int framesPersecond = 4;
        static public int recordingIndex = 0;
        static public List<double>recordList = new List<double>(); //录制时间间隔ms
        static public string aviRecordFile = "";  //录制文件名
        static public string tempRecordPath = ""; //录制临时文件jpg路径
        static public DateTime lastRecording;

        //Mouse Control
        static public float zoomSpeed = 1.0f; //快速
        static public float zoomSlowSpeed = 0.001f;//慢速
        static public float zoomFastSpeed = 10f;//快速

        //textures loaded
        static public List<TextureStruct> pTextures = new List<TextureStruct>();
        
        //Default color scale loaded
        static public List<CColorScale> defaultColorScales = new List<CColorScale>();
        static public ulong CreateRandomKey(int len = 10)
        {
            if( globalRandom == null ) globalRandom = new Random(DateTime.Now.Millisecond);
            string ss = "1";
            for (int i = 0; i < len; i++)
            {
                ss += globalRandom.Next(10);
            }
            return ulong.Parse(ss);
        }
        static public bool toStream(BinaryWriter br, Bitmap bmp)
        {
            try 
            {
                //写入length
                MemoryStream stream = new MemoryStream();
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                br.Write( Convert.ToInt32(stream.Length) );
                br.Write(stream.ToArray(), 0, Convert.ToInt32(stream.Length));
                stream.Close();
                stream.Dispose();
                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }            
        }
        private static double R = 100;
        private static double angle = 30;
        private static double h = R * Math.Cos(angle / 180 * Math.PI);
        private static double r = R * Math.Sin(angle / 180 * Math.PI);
        
        static public bool IsSimilarColorHSV(Color c1, Color c2, int maxDiff = 5)
        {
            ColorHSV h1 = new ColorHSV(c1);
            ColorHSV h2 = new ColorHSV(c2);
            if(h1.Distance(h2) < maxDiff) return true;
            else return false;
        }
        static public bool IsSimilarColor(Color c1,Color c2,float difference = 5)
        {
            if (Math.Abs(c1.R - c2.R) > difference) return false;
            if (Math.Abs(c1.G - c2.G) > difference) return false;
            if (Math.Abs(c1.B - c2.B) > difference) return false;
            return true;
        }
        static public bool IsColorInList(Color c,List<Color>colors, float difference)
        {
            for (int i = 0; i < colors.Count; i++)
            {                
                if (C3DData.IsSimilarColor(c, colors[i], difference)) 
                    return true;
            }
            return false;            
        }
        static public Bitmap fromStream(BinaryReader br, int maxImageSize = 0 )
        {
            try 
            {
                int size = br.ReadInt32();
                if (size < 1) { errMessage = "Invalid bitmap stream length."; return null; }
                byte[] data = new byte[size];
                data = br.ReadBytes(size);
                MemoryStream stream = new MemoryStream();
                stream.Write(data, 0, size);
                Image img = Image.FromStream(stream);
                
                stream.Close();
                stream.Dispose();
                data = null;
                double width = img.Width;
                double height = img.Height;
                if (maxImageSize == 0 || Math.Max(width, height) <= maxImageSize)
                {
                    Bitmap bmp = new Bitmap(img);
                    img.Dispose();
                    return bmp;
                }
                else
                {
                    double scale = height / width;
                    width = maxImageSize;
                    height = width * scale;
                    if (width < height)
                    {
                        scale = width / height;
                        height = maxImageSize;
                        width = height * scale;
                    }
                    return ResizeImage(img, (int)width, (int)height);                    
                }
            }
            catch(Exception e) 
            {
                errMessage = e.Message;
                return null;
            }
        }
        static public Bitmap ResizeImage(Image img, int newWidth, int newHeight)
        {
            // 获取这个图片的宽和高
            float width = img.Width;
            float height = img.Height;
            Bitmap newbmp = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(newbmp);
            g.DrawImage(img, 0, 0, newWidth, newHeight);
            g.Dispose();
            return newbmp;
        }
        static public Bitmap ResizeImage(Bitmap bmp, int newWidth, int newHeight)
        {
            // 获取这个图片的宽和高
            float width = bmp.Width;
            float height = bmp.Height;
            Bitmap newbmp = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(newbmp);
            g.DrawImage(bmp, 0, 0, newWidth, newHeight);
            g.Dispose();
            return newbmp;
        }

        static public void ClearObjects()
        {
            objSelected = null;
            objectKeyIndex = 0;
            objectsDiction.Clear();            
            IsDataModified = false;
        }
        static public void AddDictionary(List<C3DObjectBase>objests)
        {
            objectKeyIndex = 0;
            objectsDiction.Clear();
            foreach (C3DObjectBase obj in objests)
            { 
                objectsDiction.Add(objectKeyIndex++, obj); 
            }
        }       
        /// <summary>
        /// 返回当前所有图形对象
        /// </summary>
        /// <returns></returns>
        static public List<C3DObjectBase>GetObjects()
        {
            return objectsDiction.Values.ToList<C3DObjectBase>();
        }
        /// <summary>
        /// 返回当前指定类型(type)的所有图形对象
        /// </summary>
        /// <returns></returns>
        static public List<C3DObjectBase> GetObjects(ShapeEnum type)
        {
            return objectsDiction.Values.Where(p=>p.type == type).ToList();
        }
        static public bool UpdateObjectInDiction(C3DObjectBase obj)
        {
            int key = GetObjectKeyFromDiction(obj);
            if( key >= 0)
            {
                objectsDiction[key] = obj;
                return true;
            }
            return false;
        }
        static public int GetObjectKeyFromDiction(C3DObjectBase obj)
        {
            foreach (var item in objectsDiction)
            {
                if (obj == item.Value)
                {
                    return item.Key;
                }
            }
            return -1;
        }
        static public C3DObjectBase GetObjectByKey(int key)
        {
            C3DObjectBase obj = null;
            if (objectsDiction.TryGetValue(key, out obj)) return obj;
            return null; 
        }
        static public void SetObjectByKey(int key, C3DObjectBase obj)
        {
            objectsDiction[key] = obj;
        }
        static public void RemoveObjectByKey(int key)
        {
            objectsDiction.Remove(key);
            IsDataModified = true;
        }
        static public void RemoveObjectFromDictionary(C3DObjectBase obj)
        {
            foreach (var item in objectsDiction)
            {
                if (obj == item.Value)
                { 
                    objectsDiction.Remove(item.Key);
                    IsDataModified = true;
                    break;
                }
            }            
        }
        static public void ReplaceObjectInDictionary(C3DObjectBase obj, C3DObjectBase newobj)
        {
            foreach (var item in objectsDiction)
            {
                if (item.Value == obj)
                {                    
                    objectsDiction.Remove(item.Key);
                    objectsDiction.Add(item.Key, newobj);
                    IsDataModified = true;
                    break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>true - range updated</returns>
        static public bool UpdateRange()
        {
            CubeModel64 range_org = CDataModel.m_Model;

            CDataModel.m_ModelOrg = GetObjectsRange();
            if( CDataModel.m_ModelOrg.IsValid() )
               CDataModel.CalculateModelSize();

            if (range_org != CDataModel.m_Model) return true;
            else return false;
        }
        //得到当前对象的值范围
        static public CubeModel64 GetObjectsRange()
        {
            double x1 = 0, y1 = 0, z1 = 0, x2 = 0, y2 = 0, z2 = 0;

            List<C3DObjectBase> objects = GetObjects();
            C3DObjectBase obj;

            for (int i = 0; i < objects.Count; i++)
            {
                obj = objects[i];
                if (i == 0)
                {
                    x1 = obj.Minx;
                    y1 = obj.Miny;
                    z1 = obj.Minz;
                    x2 = obj.Maxx;
                    y2 = obj.Maxy;
                    z2 = obj.Maxz;
                }
                else
                {
                    if (x1 > obj.Minx) x1 = obj.Minx;
                    if (y1 > obj.Miny) y1 = obj.Miny;
                    if (z1 > obj.Minz) z1 = obj.Minz;
                    if (x2 < obj.Maxx) x2 = obj.Maxx;
                    if (y2 < obj.Maxy) y2 = obj.Maxy;
                    if (z2 < obj.Maxz) z2 = obj.Maxz;
                }
            }
            return new CubeModel64(x1, y1, z1, x2, y2, z2);
        }        
        static public int GetTexIndexFromName(string texName)
        {
            if (texName.Length < 1) return -1;
            if (texName.ToLower() == "none") return -1;

            for (int i = 0; i < pTextures.Count; i++)
            {
                if (texName == pTextures[i].Name)
                    return i;
            }
            return -1;
        }
        static public bool SeekSection(string section, ref StreamReader sr)
        {
            string str;
            while ((str = sr.ReadLine()) != null)
            {
                str = str.Trim(' ');
                if (str.Length < 1) continue;
                if (str == section) return true;
            }
            return false;
        }
        static public string GetLineValue(string str, string name)
        {
            string[] ss = str.Split(new Char[] { '=', '=' }, 2);
            if (ss.Length < 2) return "";

            string s1 = ss[0].Trim(' ');
            if (s1.ToLower() != name.ToLower()) return "";

            return ss[1].Trim(' ');
        }
        
        static public C3DObjectBase GetSelectedObj(ShapeEnum type)
        {
            if (objSelected == null) return null;
            if (type == objSelected.type)return objSelected;
            return null;
        }
        /// <summary>
        /// add object to List
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="update_range">is update current model range</param>
        /// <returns>range updated or not</returns>
        static public bool AddObject(C3DObjectBase obj, bool update_range = true)
        {
            obj.RenderMode = RenderingUpdateMode.Redraw;            
            lastLoaded = obj;
            lastLoadeds.Add(lastLoaded);
            UpdateIndicesList.Add(objectKeyIndex);
            objectsDiction.Add(objectKeyIndex++, obj);
            IsDataModified = true;
            if (update_range) return CDataModel.UpdateModelSize(obj);
            return false;
        }
        static public void AddObjects(List<C3DObjectBase> objects, bool update_range = true)
        {
            foreach(C3DObjectBase obj in objects )
            {
                AddObject(obj, update_range);
            }
            IsDataModified = true;
        }
        static int GetCurrentDictionaryKey()
        {
            int key = 0;
            foreach (var item in objectsDiction)
            {
                if ( item.Key > key )key = item.Key;                
            }
            return key;
        }
        static public void AddDictionary(int key, C3DObjectBase obj )
        {
            objectsDiction.Add(key, obj);
            if (objectKeyIndex < key) objectKeyIndex = key;
        }
        static public bool IsBlankValue(double v, double zero = 0.001)
        {
            if (double.IsNaN(v)) return true;
            if (v == m_BlankedValue) return true;
            if (Math.Abs(v - m_BlankedValue) / Math.Abs(m_BlankedValue) <= zero) return true;
            else return false;
        }
        static public bool IsZero(double v, double error)
        {
            if (Math.Abs(v) < error) return true;
            else return false;
        }
        static public bool LoadTexture(string path)
        {
            Bitmap bmp = new Bitmap(path);
            if (bmp == null) return false;
            TextureStruct tex = new TextureStruct(bmp);
            tex.Name = Path.GetFileNameWithoutExtension(path);
            pTextures.Add(tex);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static public bool Load3DGridData(string path, ref bool range_updated,double sampleScale = 1.0)
        {            
            bool ret = false;
            range_updated = false;

            C3DGridData data = new C3DGridData();
            if (sampleScale <= 1.0) ret = data.LoadFrom(path);
            else ret = data.LoadFromBigGrid(path, sampleScale);

            if (ret)
            {
                data.Name = Path.GetFileName(path);
                range_updated = AddObject(data,true);
                return true;
            }
            else
            {
                errMessage = data.errMessage;
                data.Clear();
                return false;
            }
        }

        static public bool Update3DGridTest(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string path = Path.GetDirectoryName(filename);
            string ss;
            int i = 0;
            string gridfile;
            while ((ss = sr.ReadLine()) != null)
            {
                if (ss.Length < 3) continue;
                if (i > 0)
                {
                    string[] str = ss.Split(new Char[] { ',', ',' }, 4);
                    gridfile = path + "\\" + str[3] + "\\3D-1.3DGrid";
                    C3DGridData data = new C3DGridData();
                    data.LoadFrom(gridfile);
                    data.SaveAs(gridfile);
                }
                i++;
            }

            sr.Close();
            fs.Close();

            return true;
        }
        //load las data from formated file
        // x,y,elevation,filename
        static public CBoreholes LoadLasDataFromControlFile(string filename)
        {           

            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);

                string path = Path.GetDirectoryName(filename);
                string ss, ss1, ss2, ss3, ss4, ss5;
                double xpos, ypos, zpos;
                string lasfile;
                CBoreholes boreholes = new CBoreholes();
                string texture = "";
                bool ret;
                int len;
                string ext;
                while ((ss = sr.ReadLine()) != null)
                {
                    ss = ss.Trim();
                    if (ss.Length < 3) continue;
                    if (ss[0] == '#' || ss[0] == '/') continue;

                    ss = ss.Replace('\t', ' ');
                    string[] str = ss.Split(new Char[] { ',', ',' }, 5);
                    if (str.Length < 4) continue;
                    ss1 = str[0].Trim();    //x
                    ss2 = str[1].Trim();    //y
                    ss3 = str[2].Trim();    //z
                    ss4 = str[3].Trim();    //lasfile

                    ss5 = "";
                    if (str.Length > 4) ss5 = str[4].Trim();    //texture file

                    ret = double.TryParse(ss1, out xpos);
                    if (!ret) continue;
                    ret = double.TryParse(ss2, out ypos);
                    if (!ret) continue;
                    ret = double.TryParse(ss3, out zpos);
                    if (!ret) continue;

                    //las file name ss4
                    len = ss4.Length;
                    if (len < 1) continue;

                    ret = false;
                    if (len > 4)
                    {
                        ext = ss4.Substring(len - 4, 4);
                        ext = ext.ToLower();
                        if (ext == ".las") ret = false;
                    }
                    lasfile = path + "\\" + ss4;
                    if (ret) lasfile += ".las";

                    //texture file
                    texture = "";
                    if (ss5.Length > 0)
                    {
                        texture = path + "\\" + ss5;
                    }
                    CLasFile las = new CLasFile();
                    LasFileData data = las.Read(lasfile);
                    if (data.LogData == null) continue;

                    data.m_pos.X = xpos;
                    data.m_pos.Y = ypos;
                    data.m_pos.Z = zpos;
                    
                    data.Name = ss4;
                    CBorehole bh = new CBorehole(data);                    
                    boreholes.AddBorehole(bh);

                }
                sr.Close();
                fs.Close();
                return boreholes;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return null;
            }

        }

        //read from a data file, ,x,y,z
        static public bool LoadLineFile(string filename)
        {
            C3DLine line = new C3DLine();
            if ( line.ImportData(filename) )
            {
                if(line.Name.Length < 1 || line.Name == "untitled")
                    line.Name = Path.GetFileName(filename);   
                
               AddObject(line);

                return true;
            }
            return false;
        }
        //load and create MeshLines
        static public bool LoadMeshLines(string[] filenames)
        {
            LineMesh meshLines = new LineMesh();

            for (int i = 0; i < filenames.Length; i++)
            {
                C3DLine line = new C3DLine();
                if (line.ImportData(filenames[i]))
                {
                    line.Name = Path.GetFileName(filenames[i]);
                    meshLines.Add(line);
                }
            }
            if (meshLines.lineNum > 0)
            {
                meshLines.UpdateRange();
                AddObject(meshLines);
                return true;
            }
            else
            {
                errMessage = "Invalid lines.";
                return false;
            }
        }

        static public string GetNameWithoutExt(string filename)
        {
            string forname = Path.GetFileName(filename);
            string ext = Path.GetExtension(forname);
            int n = ext.Length;
            if (n < 1) return forname;
            //L01.dxf
            return forname.Substring(0, forname.Length - n);
        }       

        static public List<Polygon2D> LoadDxfFile(string filename)
        {
            DxfFileObj dxf = new DxfFileObj();
            List<Polygon2D> polygons = new List<Polygon2D>();
            if (dxf.Load(filename))
            {
                if (dxf.ExtractObjects() > 0)
                {
                    string name = GetNameWithoutExt(filename);

                    for (int i = 0; i < dxf.polygons.Count; i++)
                    {
                        Polygon2D poly = dxf.polygons[i];
                        poly.Name = name + "-" + (i + 1).ToString();
                        poly.Simplify();
                        poly.UpdateRange();
                        polygons.Add(poly);
                    }   
                }
            }
            return polygons;
        }
        static public List<Polygon2D> LoadShpFile(string filename)
        {
            Shapefile.Shapefile shapefile = new Shapefile.Shapefile();
            if (!shapefile.LoadShapefile(filename))
            {
                errMessage = shapefile.errMessage;
                return null;
            }

            int n = shapefile.Count;

            List<Polygon2D> polygons = new List<Polygon2D>();
            string surname = GetNameWithoutExt(filename);

            // enumerate all shapes
            int count = 1;
            foreach (Shapefile.Shape shape in shapefile)
            {
                string[] metadataNames = shape.GetMetadataNames();
                Polygon2D poly = new Polygon2D();

                if (metadataNames != null)
                {
                    poly.Name = shape.GetMetadata(metadataNames[0]);
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(poly.Name);
                    poly.Name = surname + "-" + System.Text.Encoding.UTF8.GetString(bytes);
                }
                else poly.Name = surname + count++;

                // cast shape based on the type
                switch (shape.Type)
                {
                    case Shapefile.ShapeType.PolyLine:
                        Shapefile.ShapePolyLine shapePolyLine = shape as Shapefile.ShapePolyLine;
                        poly.IsClosed = false;
                        foreach (Shapefile.PointD[] part in shapePolyLine.Parts)
                        {
                            //Console.WriteLine("Polygon part:");
                            foreach (Shapefile.PointD point in part)
                            {
                                poly.Add(point.X, point.Y);
                                //Console.WriteLine("{0}, {1}", point.X, point.Y);
                            }                            
                            // Console.WriteLine();
                        }
                        poly.UpdateRange();
                        polygons.Add(poly);
                        break;
                    case Shapefile.ShapeType.Polygon:
                        // a polygon contains one or more parts - each part is a list of points which
                        // are clockwise for boundaries and anti-clockwise for holes 
                        // see http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf

                        poly.IsClosed = true;

                        Shapefile.ShapePolygon shapePolygon = shape as Shapefile.ShapePolygon;
                        foreach (Shapefile.PointD[] part in shapePolygon.Parts)
                        {
                            //Console.WriteLine("Polygon part:");
                            foreach (Shapefile.PointD point in part)
                            {
                                poly.Add(point.X, point.Y);
                                //Console.WriteLine("{0}, {1}", point.X, point.Y);
                            }                            
                        }
                        poly.UpdateRange();
                        polygons.Add(poly);
                        break;
                    default: break;// and so on for other types...       
                }//switch (shape.Type)
            }//foreach (Shape shape in shapefile)

            return polygons;
        }
       
        static public bool LoadPolygonSlicer(string filename)
        {
            PolygonSlicer slicer = new PolygonSlicer();
            slicer.Name = GetNameWithoutExt(filename);
            if (slicer.LoadFrom(filename))
            {
                AddObject(slicer, true);
                return true;
            }
            return false;
        }

        //read from a data file, ,x,y,z
        static public bool LoadScatteredPointsFile(string filename)
        {
            ScatteredPoints obj = new ScatteredPoints();
            if (obj.ImportData(filename))
            {
                obj.Name = Path.GetFileName(filename);
                AddObject(obj);
                return true;
            }
            else
            {
                errMessage = obj.errMessage;
                return false;
            }
        }

        //Load Slicer from a data file
        static public bool LoadSlicerFile(string filename)
        {
            CSlicer slicer = new CSlicer(100, 100);
            bool ret;
            ret = slicer.LoadSlicer(filename);
            if (ret) AddObject(slicer);
            errMessage = slicer.errMessage;
            return ret;
        }

        //Load Surfer Grid Slicer from a data file
        static public bool LoadSurferGridFile(string filename)
        {
            CSurferGrid cs = new CSurferGrid();
            if (!cs.Read(filename))
            {
                errMessage = cs.errMessage;
                return false;
            }
            CSlicer slicer = new CSlicer(100, 100);
            if (slicer.FromSurferGrid(cs))
            {
                AddObject(slicer);
                return true;
            }
            else
            {
                errMessage = slicer.errMessage;
                return false;
            }
        }
        //Load Cylinder from a data file
        static public bool LoadCylinderFile(string filename)
        {
            CCylinderExt cy = new CCylinderExt();
            bool ret;
            ret = cy.ImportData(filename);
            if (ret) AddObject(cy);
            return ret;
        }
        //Load Slicer from a data file
        static public bool LoadMeshFile(string filename)
        {
            CMesh mesh = new CMesh(100, 100);
            bool ret;
            ret = mesh.LoadFrom(filename);
            if (ret) AddObject(mesh);
            errMessage = mesh.errMessage;
            return ret;
        }
        static public TriangleObj Load3DObjectFile(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            if (ext == ".ply") return LoadPlyFile(path);
            if (ext == ".stl") return LoadStlFile(path);
            if (ext == ".obj") return LoadObjFile(path);
            return null;
        }
        static public TriangleObj LoadStlFile(string path)
        {
            STLObject obj = new STLObject();

            if ( !obj.Read(path) )
            {
                errMessage = obj.errMessage;
                return null;
            }

            string name = Path.GetFileName(path);
            name = name.Replace(".stl", "");
            obj.Name = name;           

            return obj;
        }
        static public TriangleObj LoadPlyFile(string path)
        {
            PlyFile obj = new PlyFile();
            if (!obj.LoadFrom(path))
            {
                errMessage = obj.errMessage;
                return null;
            }

            //get name and texture file
            //if (obj.name.Length < 1)
            {
                string name = Path.GetFileName(path);
                name = name.Replace(".ply", "");
                obj.Name = name;
            }
            if (obj.textureStruct.TextureFile.Length < 1)
            {
                //default img file extension is jpg
                obj.textureStruct.TextureFile = path.Replace(".ply", ".jpg");
            }
            else //set the full path of the texture file
            {
                obj.textureStruct.TextureFile = Path.GetDirectoryName(path) + "\\" + obj.textureStruct.TextureFile;
            }

            TriangleObj tri = obj.toTriangleObj();
            obj.Clear();           

            return tri;
        }
        static public TriangleObj LoadObjFile(string path)
        {
            Object3DFile obj = new Object3DFile();

            if (!obj.Read(path))
            {
                errMessage = obj.errMessage;
                return null;
            }

            string name = Path.GetFileName(path);
            name = name.Replace(".obj", "");
            obj.Name = name;

            return obj;
        }
        static public string LoadString(BinaryReader br)
        {
            try
            {
                string ss = "";
                Int16 n = br.ReadInt16();
                if (n > 0) 
                {
                    char[] chars = br.ReadChars(n);
                    ss = new string(chars); 
                }
                return ss;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return null;
            }
        }
        static public bool SaveFont(BinaryWriter br, Font font)
        {
            try
            {
                SaveString(br, font.Name);
                br.Write(font.Size);
                br.Write((int)font.Style);
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
        static public Font LoadFont(BinaryReader br)
        {
            try
            {
                string name = LoadString(br);
                float emSize = br.ReadSingle();
                FontStyle style = (FontStyle)br.ReadInt32();
                return new Font(name,emSize,style);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return null;
            }
        }
        static public bool SaveString(BinaryWriter br, string ss)
        {
            try
            {
                Int16 n = (Int16)ss.Length;
                br.Write(n);
                if (n > 0) 
                {
                    char[] chars = ss.ToCharArray(0,n);
                    br.Write(chars,0,n); 
                }
                return true;
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
        static public Color LoadColor(BinaryReader br)
        {
            int argb = br.ReadInt32();
            return Color.FromArgb(argb);
        }

        static public void SaveColor(BinaryWriter br, Color color)
        {
            int argb = color.ToArgb();
            br.Write(argb);
        }

        static public bool SaveG3DFile(BinaryWriter br)
        {
            try 
            {
                IsDataModified = false;

                //file flag 
                string header = "GEO3D";
                SaveString(br,header);

                //version               
                br.Write(Version);
                if (Version > 1.2f) return SaveG3DData121(br);
                else return SaveG3DData120(br);
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return false;
            }            
        }
        static private bool SaveG3DData120(BinaryWriter br)
        {
            try
            {
                br.Write((Int32)objectsCount);
                foreach (C3DObjectBase obj in objectsDiction.Values)
                {
                    br.Write((Int32)obj.type);
                    if (!obj.SaveAs(br))
                    {
                        errMessage = "Saving " + obj.Name + " failed.\n";
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }
        static private bool SaveG3DData121(BinaryWriter br)
        {
            try
            {
                //存储数据字典
                br.Write( (Int32) objectsDiction.Count);
                foreach (var item in objectsDiction )
                {
                    br.Write(item.Key);
                    C3DObjectBase obj = item.Value;
                    br.Write((Int32)obj.type);
                    if (!obj.SaveAs(br))
                    {
                        errMessage = "Saving " + obj.Name + " failed.\n";
                        return false;
                    }
                }
                
                //存储目录树
                return TreeStructData.Save(br, dataTrees);
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }

        static public bool ImportFromG3DFile(BinaryReader br,string g3dname)
        {
            string header = "GEO3D";
            string tag = LoadString(br);
            if (tag == null || tag != header)
            {
                errMessage = "not a valid Geo3D file format.";
                return false;
            }
            //version
            DataVersion = br.ReadSingle();
            if (DataVersion < 1f || DataVersion > Version)
            {
                errMessage = "This version is not supported , please load it with a higher version.";
                return false;
            }

            //ClearObjects();

            int key0 = GetCurrentDictionaryKey() + 1;
            bool ret = ImportG3DFile122(br,key0);

            TreeStructData data = TreeStructData.Load(br, key0);
            data.Name = g3dname;
            dataTrees.Items.Add(data);            

            objectKeyIndex++;
            UpdateRange();            
            IsDataModified = true;
            return ret;
        }

        static public bool LoadG3DFile(BinaryReader br)
        {
            string header = "GEO3D";
            string tag = LoadString(br);
            if ( tag ==null || tag != header )
            {
                errMessage = "not a valid Geo3D file format.";
                return false;
            }
            //version
            DataVersion = br.ReadSingle();
            if (DataVersion < 1f || DataVersion > Version )
            {
                errMessage = "This version is not supported , please load it with a higher version.";
                return false;
            }
            
            ClearObjects();
            
            bool ret;

            if (DataVersion > 1.21f) ret = LoadG3DFile122(br);
            else ret = LoadG3dData121(br);
            
            objectKeyIndex++;

            UpdateRange();

            dataTrees = TreeStructData.Load(br);
            IsDataModified = false;

            return ret;
        }
        static private bool LoadG3dData121(BinaryReader br)
        {
            int nObj = br.ReadInt32();
            ShapeEnum type;
            C3DObjectBase obj;            
            List<C3DObjectBase> objects = new List<C3DObjectBase>();
            for (int i = 0; i < nObj; i++)
            {
                bool ret = false;
                type = (ShapeEnum)br.ReadInt32();
                switch (type)
                {
                    case ShapeEnum.Grid3D:
                        obj = new C3DGridData();
                        ret = obj.LoadFrom(br);                        
                        break;
                    case ShapeEnum.Mesh:
                        obj = new CMesh();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.GeoMesh:
                        obj = new GeoMesh();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.GeoLayerMeshes:
                        obj = new GeoLayerMeshes();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Triangles:
                        obj = new TriangleObj();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Slicer:
                        obj = new CSlicer(100, 100);
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.GeoProfile:
                        obj = new GeoProfile();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.PolygonSlicer:
                        obj = new PolygonSlicer();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Boreholes:
                        obj = new CBoreholes();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Borehole:
                        obj = new CBorehole();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Points:
                        obj = new ScatteredPoints();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Text:
                        obj = new TexturedText("");
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Line:
                        obj = new C3DLine();
                        ret = obj.LoadFrom(br);
                        break;
                    default:
                        errMessage = "Uncognized data format.";
                        return false;                        
                }//switch(type)                

                if(ret) objects.Add(obj);
                else
                {
                    errMessage = "Load data failed.\n" + obj.errMessage;
                    return false;
                }
            }//for (int i = 0; i < nObj; i++)

            AddDictionary(objects);
            IsDataModified = false;
            objects.Clear();            
            return true;
        }        
        static public bool ImportG3DFile122(BinaryReader br,int key0)
        {
            int n = br.ReadInt32();
            ShapeEnum type;
            int key = 0;            
            C3DObjectBase obj;
            for (int i = 0; i < n; i++)
            {
                key = br.ReadInt32();
                type = (ShapeEnum)br.ReadInt32();
                bool ret = false;
                switch (type)
                {
                    case ShapeEnum.Grid3D:
                        obj = new C3DGridData();
                        ret = obj.LoadFrom(br);                        
                        break;
                    case ShapeEnum.Mesh:
                        obj = new CMesh();
                        ret = obj.LoadFrom(br);                        
                        break;
                    case ShapeEnum.GeoMesh:
                        obj = new GeoMesh(); 
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.GeoLayerMeshes:
                        obj = new GeoLayerMeshes();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Triangles:
                        obj = new TriangleObj();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Slicer:
                        obj = new CSlicer(100, 100);
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.GeoProfile:
                        obj = new GeoProfile();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.PolygonSlicer:
                        obj = new PolygonSlicer();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Polygon2D:
                        obj = new Polygon2D();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Polygon2Ds:
                        obj = new C2DPolygons();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Borehole:
                        obj = new CBorehole();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Boreholes:
                        obj = new CBoreholes();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Points:
                        obj = new ScatteredPoints();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Text:
                        obj = new TexturedText("");
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Line:
                        obj = new C3DLine();
                        ret = obj.LoadFrom(br);
                        break;
                    default:
                        errMessage = "Uncognized data format.";
                        return false;
                        break;
                }//switch(type)                

                if (ret) AddDictionary(key + key0, obj);
                else
                {
                    errMessage = "Load data failed.\n" + obj.errMessage;
                    return false;
                }
            }//for (int i = 0; i < nObj; i++)

            return true;
        }
        static public bool LoadG3DFile122(BinaryReader br)
        {    
            int n = br.ReadInt32();
            ShapeEnum type;
            int key;
            C3DObjectBase obj;
            for (int i = 0; i < n; i++)
            {
                bool ret = false;
                key = br.ReadInt32();
                type = (ShapeEnum)br.ReadInt32();                
                switch (type)
                {
                    case ShapeEnum.Grid3D:
                        obj = new C3DGridData();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Mesh:
                        obj = new CMesh();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.GeoMesh:
                        obj = new GeoMesh();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.GeoLayerMeshes:
                        obj = new GeoLayerMeshes();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Triangles:
                        obj = new TriangleObj();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Slicer:
                        obj = new CSlicer(100, 100);
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.GeoProfile:
                        obj = new GeoProfile();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.PolygonSlicer:
                        obj = new PolygonSlicer();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Polygon2D:
                        obj = new Polygon2D();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Polygon2Ds:
                        obj = new C2DPolygons();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Borehole:
                        obj = new CBorehole();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Boreholes:
                        obj = new CBoreholes();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Points:
                        obj = new ScatteredPoints();
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Text:
                        obj = new TexturedText("");
                        ret = obj.LoadFrom(br);
                        break;
                    case ShapeEnum.Line:
                        obj = new C3DLine();
                        ret = obj.LoadFrom(br);
                        break;
                    default:
                        errMessage = "Uncognized data format.";
                        return false;                        
                }//switch(type)                

                if (ret) AddDictionary(key, obj);
                else
                {
                    errMessage = "Load data failed.\n" + obj.errMessage;
                    return false;
                }

            }//for (int i = 0; i < nObj; i++)
            
            return true;
        } 
       
    }

}
