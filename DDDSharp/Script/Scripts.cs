using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextReaderWriter;
using DataCollection;
using DDDSharp;
namespace ScriptInterpreter
{
    enum ValueTypeEnum
    {
        StringType = 0,
        IntType = 1,
        FloatType = 2,
        DoubleType = 3,        
    }
    public enum FunctionEnum
    {
        None = 0,
        //object chang
        LoadObject = 1,        
        SaveObject = 2,
        ImportObject = 3,
        ExportObject = 4,        
        RemoveObject = 5,

        Offset = 10,
        Rotate = 11,//对象旋转
        Scale = 12,
        SetVisible = 13,
        SetColor = 14,
        SetAlpha = 15,
        SetTexture = 16,
        LoadColorScale = 17,
        SetColorRange = 18,
        
        //
        StartRecording = 20,
        EndRecording = 21,

        //
        MoveForward = 25,//视点移动 MoveForward(float step)
        MoveBackward = 26,//视点移动 MoveBackward(float step)
        ScreenRotate = 27,//场景旋转 ScreenRotate(float angle,vec3 axis)

        //Slicer
        SlicerHeight = 30,
        SlicerCreateCounter = 31,
        SlicerShowCounter = 32,        

        //
        UpdateDraw = 100,

        //
        Sleep = 500,
    }    
    public struct FunctionStruct
    {
        public string VarName; //对应的变量名
        public FunctionEnum Key; //关键字
        public ShapeEnum ObjType;
        public List<string> Parameters; //参数
        public string orgLine;      //原始行
        public int lineNo;          //行号
        public FunctionStruct(FunctionEnum _key,string line = "",int no = 0)
        {
            Key = _key;
            ObjType = ShapeEnum.Undefine;
            Parameters = new List<string>();
            orgLine = line;
            lineNo = no;
            VarName = "";
        }       
        public bool IsValid
        {
            get 
            {
                if (Key == FunctionEnum.None)return false;                    
                else return true;
            }            
        }
        public void AddParameter(string para)
        {
            Parameters.Add(para);
        }
        public void ClearPameters()
        {
            Parameters.Clear();
        }

        static public FunctionEnum GetFunctionKey(string str)
        {
            FunctionEnum fs = FunctionEnum.None;
            if (Enum.TryParse(str, out fs))
                return fs;
            else return FunctionEnum.None;
        }

        //解析对象变量
        static public VarObjectStruct ParseVar( List<string>ss )
        {
            VarObjectStruct vs = new VarObjectStruct("");
            
            //声明变量
            if ( ss[0] == "var" || ss[0] == "Object" )
            {
                if (ss.Count < 2) 
                {
                    ss.Clear();
                    return vs;
                }
                ss.RemoveAt(0); //去掉var,Object
                vs.VarName = ss[0];
            }
            
            //变量赋值
            if( ss.Count > 3 && ss[1] == "=" )
            {
                vs.VarName = ss[0];
                ss.RemoveAt(0);
                ss.RemoveAt(0);
            }

            return vs;
        }
        //解析功能键及参数
        public static FunctionStruct ParseFunction( List<string> ss, string line, int lineno = 0)
        {
            if ( ss.Count < 1) return new FunctionStruct();

            FunctionEnum key = GetFunctionKey(ss[0]);
            FunctionStruct fs = new FunctionStruct(key, line, lineno);
            if (key != FunctionEnum.None)
            {
                for (int i = 1; i < ss.Count; i++)
                {
                    fs.AddParameter(ss[i]);
                }
            }
            return fs;
        }
        
    }
    public struct VarObjectStruct
    {
        public string VarName; //变量名        
        public string ObjName; //对象名称 
        public bool IsValid 
        {
            get 
            {
                if (VarName ==null || VarName.Length < 1 ) return false;
                else return true;
            }
        }
        public VarObjectStruct(string vName,string objName ="")
        {
            VarName = vName;            
            ObjName = objName;
        }
    }
    public class ScriptFunction
    {
        public List<string> Lines = new List<string>();
        public string errMessage = "";        
        List<VarObjectStruct> ObjectVars = new List<VarObjectStruct>();
        static public bool IsString(string para)
        {
            int n = para.Length;
            if (n < 1) return false;
            if (para[0] == '"' && para[n - 1] == '"') return true;
            if (para[0] == '\'' && para[n - 1] == '\'') return true;
            return false;
        }
        static public bool IsNumeric(string para)
        {
            if (IsString(para)) return false;
            double val = 0;
            return double.TryParse(para, out val);
        }
        static public string TrimString(string para)
        {
            return para.Trim(new char[] { '"', '\'' });
        }

       
        int GetObjectKeyByName(string name)
        {
            foreach( var item in C3DData.objectsDiction )            
            {
                if (name.ToLower() == item.Value.Name.ToLower() )
                    return item.Key;
            }
            return -1;
        }

        int GetObjectVarIndex(string varName)
        {
            for(int i = 0;i< ObjectVars.Count;i++)
            {
                if (ObjectVars[i].VarName == varName) 
                    return i;
            }
            return -1;
        }

        void AddToVarList(string varName,string objName)
        {
            VarObjectStruct vs = new VarObjectStruct(varName,objName);
            int id = GetObjectVarIndex(varName);
            if (id < 0) ObjectVars.Add(vs);
            else ObjectVars[id] = vs;
        }

        /// <summary>
        /// 添加对象到列表
        /// LoadObject(string path,string type)
        /// </summary>
        /// <param name="fs">
        /// </param>
        /// <returns></returns>
        bool DoLoadObject(FunctionStruct fs)
        {
            if (fs.Parameters.Count < 2) 
            {
                AddMessage("error parameters on line " + fs.lineNo);
                AddMessage( fs.orgLine );
                return false; 
            }
            
            string filename = TrimString( fs.Parameters[0] ); //文件名

            ShapeEnum objtype = ShapeEnum.Undefine; //类型说明
            if (!Enum.TryParse( TrimString( fs.Parameters[1] ), out objtype) ) 
            {
                AddMessage("parameter 2 not matched on line " + fs.lineNo);
                AddMessage(fs.orgLine);
                return false; 
            }

            string name = ""; //数据重命名            
            if( fs.Parameters.Count > 2 ) name = TrimString(fs.Parameters[2]);

            switch(objtype)
            {
                case ShapeEnum.Grid3D:
                    {
                        C3DGridData grid3d = new C3DGridData();
                        if (grid3d.LoadFrom(filename))
                        {
                            if (name.Length > 0) grid3d.Name = name;
                            else grid3d.Name = filename;

                            if( fs.VarName.Length > 0 ) AddToVarList(fs.VarName, grid3d.Name);//变量赋值
                            
                            grid3d.InitTables();
                            AddObject(grid3d);
                            return true;
                        }
                        else 
                        {
                            AddMessage("Load object failed:" + grid3d.errMessage);
                            AddMessage("Line " + fs.lineNo + " : " + fs.orgLine);
                        }
                        break;
                    }
                case ShapeEnum.Mesh:
                    TriangleObj obj = C3DData.Load3DObjectFile(filename);
                    if(obj == null )
                    {
                        AddMessage("Load object failed:" + C3DData.errMessage);
                        AddMessage("Line " + fs.lineNo + " : " + fs.orgLine);
                        break;
                    }

                    if (name.Length > 0) obj.Name = name;
                    else obj.Name = filename;

                    //AddObject(obj); 已经添加了
                    UpdateTree();
                    if (fs.VarName.Length > 0) AddToVarList(fs.VarName, obj.Name);//变量赋值                        

                    return true;                   
                   
                case ShapeEnum.Slicer:
                    CSlicer slicer = new CSlicer(100,100);
                    if (slicer.LoadFrom(filename))
                    {
                        if (name.Length > 0) slicer.Name = name;
                        else slicer.Name = filename;
                        AddObject(slicer);
                        if (fs.VarName.Length > 0) AddToVarList(fs.VarName, slicer.Name);//变量赋值
                        return true;
                    }
                    else 
                    {
                        AddMessage("Load object failed:" + slicer.errMessage);
                        AddMessage("Line " + fs.lineNo + " : " + fs.orgLine);
                    }
                    break;
                case ShapeEnum.PolygonSlicer:
                    PolygonSlicer polyslicer = new PolygonSlicer();
                    if (polyslicer.LoadFrom(filename))
                    {
                        if (name.Length > 0) polyslicer.Name = name;
                        else polyslicer.Name = filename;
                        AddObject(polyslicer);
                        if (fs.VarName.Length > 0) AddToVarList(fs.VarName, polyslicer.Name);//变量赋值
                        return true;
                    }
                    else
                    {
                        AddMessage("Load object failed on line " + fs.lineNo);
                        AddMessage(fs.orgLine);
                    }
                    break;
            }

            return false;
        }
        /// <summary>
        /// 获取对象的key值
        /// </summary>
        /// <param name="para"> 名称或key值</param>
        /// <returns></returns>
        int GetObjectKeyFromParameter(string para)
        {
            int pos = -1;
            if ( IsNumeric(para) )
            {
                if ( !int.TryParse( para, out pos ) ) 
                    return -1;
            }
            else
            {
                return GetObjectKeyByName( TrimString( para ) );
            }
            return pos;
        }

        bool DoUpdateDraw(FunctionStruct fs)
        {
            bool show = true;
            if (fs.Parameters.Count == 0) //不带参数
            {
                UpdateDraw(show);
                return true;
            }

            //带参数，true or false
            if ( bool.TryParse(fs.Parameters[0], out show) )
            {
                UpdateDraw(show);
                return true;
            }

            //带参数，对象变量或对象名称
            string para = fs.Parameters[0];
            if (para[0] != '"') //变量名称
            {
                int id = GetObjectVarIndex(para);
                if (id >= 0) para = ObjectVars[id].ObjName;
                else
                {
                    AddMessage("variable '" + para + "' not found" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }
            }
            else //对象名称
            {
                para = TrimString(para);
            }            

            int pos = GetObjectKeyFromParameter(para);
            if (pos < 0)
            {
                AddMessage("object not found ! " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }
            
            //重绘对象
            Program.m_MainForm.UpdateDraw(C3DData.GetObjectByKey(pos));
            return true;
        }

        bool DoRemoveObject(FunctionStruct fs)
        {
            if (fs.Parameters.Count < 1 || fs.Parameters[0].Length < 1 ) 
            {
                AddMessage("lack of parameter at " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false; 
            }
            
            string para = fs.Parameters[0];
            if ( para[0] != '"' ) //变量名称
            {
                int id = GetObjectVarIndex(para);
                if (id >= 0) para = ObjectVars[id].ObjName;
                else 
                {
                    AddMessage("variable '" + para + "' not found" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }                
            }
            else //字符串 
            {
                para = TrimString(para);
            }
            int pos = GetObjectKeyFromParameter(para);
            if(pos < 0 )
            {
                AddMessage("object not found ! " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }

            C3DData.RemoveObjectByKey(pos);
            return true;
        }

        bool DoOffset(FunctionStruct fs)
        {
            if ( fs.Parameters.Count < 4 ) return false;
            
            string para = fs.Parameters[0];
            if (para[0] != '"') //变量名称
            {
                int id = GetObjectVarIndex(para);
                if (id >= 0) para = ObjectVars[id].ObjName;
                else
                {
                    AddMessage("variable '" + para + "' not found" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }
            }
            else
            {
                para = TrimString(para);
            }
            int pos = GetObjectKeyFromParameter(para);
            if (pos < 0)
            {
                AddMessage("object not found ! " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }

            try
            {
                float offx = Convert.ToSingle(fs.Parameters[1]);
                float offy = Convert.ToSingle(fs.Parameters[2]);
                float offz = Convert.ToSingle(fs.Parameters[3]);                 
                C3DObjectBase obj = C3DData.GetObjectByKey(pos);
                GlmNet.vec3 offset = obj.offset;
                offset.x += offx;
                offset.y += offy;
                offset.z += offz;
                obj.offset = offset;
                return true;
            }
            catch (Exception ex) 
            {
                AddMessage("parameter error of " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }            
        }

        bool DoRotate(FunctionStruct fs)
        {
            if (fs.Parameters.Count < 4) return false;

            string para = fs.Parameters[0];
            if (para[0] != '"') //变量名称
            {
                int id = GetObjectVarIndex(para);
                if (id >= 0) para = ObjectVars[id].ObjName;
                else
                {
                    AddMessage("variable '" + para + "' not found" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }
            }
            else
            {
                para = TrimString(para);
            }
            int pos = GetObjectKeyFromParameter(para);
            if (pos < 0)
            {
                AddMessage("object not found ! " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }

            try
            {
                float offx = Convert.ToSingle(fs.Parameters[1]);
                float offy = Convert.ToSingle(fs.Parameters[2]);
                float offz = Convert.ToSingle(fs.Parameters[3]);

                C3DObjectBase obj = C3DData.GetObjectByKey(pos);
                GlmNet.vec3 rotate = obj.rotate;
                rotate.x += offx;
                rotate.y += offy;
                rotate.z += offz;
                obj.rotate = rotate;

                return true;
            }
            catch (Exception ex)
            {
                AddMessage("parameter error of " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }            
        }
        bool DoScale(FunctionStruct fs)
        {
            if (fs.Parameters.Count < 4) return false;

            string para = fs.Parameters[0];
            if (para[0] != '"') //变量名称
            {
                int id = GetObjectVarIndex(para);
                if (id >= 0) para = ObjectVars[id].ObjName;
                else
                {
                    AddMessage("variable '" + para + "' not found" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }
            }
            else
            {
                para = TrimString(para);
            }
            //如何获取对象，索引号？名称？
            int pos = GetObjectKeyFromParameter(para);
            if (pos < 0)
            {
                AddMessage("object not found ! " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }
            try
            {
                float offx = Convert.ToSingle(fs.Parameters[1]);
                float offy = Convert.ToSingle(fs.Parameters[2]);
                float offz = Convert.ToSingle(fs.Parameters[3]);
                
                //如何获取对象，索引号？名称？
                C3DObjectBase obj = C3DData.GetObjectByKey(pos);
                GlmNet.vec3 scale = obj.scale;
                scale.x *= offx;
                scale.y *= offy;
                scale.z *= offz;
                obj.scale = scale;
                return true;
            }
            catch (Exception ex)
            {
                AddMessage("parameter error of " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }
            
        }
        bool DoSetVisible(FunctionStruct fs)
        {
            if (fs.Parameters.Count < 2) return false;

            string para = fs.Parameters[0];
            if (para[0] != '"') //变量名称
            {
                int id = GetObjectVarIndex(para);
                if (id >= 0) para = ObjectVars[id].ObjName;
                else
                {
                    AddMessage("variable '" + para + "' not found" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }
            }
            else
            {
                para = TrimString(para);
            }
            int pos = GetObjectKeyFromParameter(para);
            if (pos < 0)
            {
                AddMessage("object not found ! " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }
            try 
            {
                bool visible = Convert.ToBoolean(fs.Parameters[1]);
                C3DObjectBase obj = C3DData.GetObjectByKey(pos);
                obj.Visible = visible;
                return true;
            }
            catch (Exception ex)
            {
                AddMessage("parameter error of " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }            
        }
        bool DoSetAlpha(FunctionStruct fs)
        {
            if (fs.Parameters.Count < 2) return false;
            
            string para = fs.Parameters[0];
            if (para[0] != '"') //变量名称
            {
                int id = GetObjectVarIndex(para);
                if (id >= 0) para = ObjectVars[id].ObjName;
                else
                {
                    AddMessage("variable '" + para + "' not found" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }
            }
            else
            {
                para = TrimString(para);
            }
            int pos = GetObjectKeyFromParameter(para);
            if (pos < 0)
            {
                AddMessage("object not found ! " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }
            try 
            {
                float alpha = Convert.ToSingle(fs.Parameters[1]);
                C3DObjectBase obj = C3DData.GetObjectByKey(pos);
                obj.Alpha = alpha;
                return true;
            }
            catch (Exception ex)
            {
                AddMessage("parameter error of " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }            
        }
        
        bool DoSetTexture(FunctionStruct fs)
        {
            if (fs.Parameters.Count < 2) return false;

            string para = fs.Parameters[0];
            if (para[0] != '"') //变量名称
            {
                int id = GetObjectVarIndex(para);
                if (id >= 0) para = ObjectVars[id].ObjName;
                else
                {
                    AddMessage("variable '" + para + "' not found" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }
            }
            else
            {
                para = TrimString(para);
            }
            int pos = GetObjectKeyFromParameter(para);
            if (pos < 0)
            {
                AddMessage("object not found ! " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }
            try
            {
                bool enabled = Convert.ToBoolean(fs.Parameters[1]);
                C3DObjectBase obj = C3DData.GetObjectByKey(pos);
                obj.enbaleTexture = enabled;
                if (enabled && fs.Parameters.Count < 3) return false;
                obj.textureStruct.TextureFile = fs.Parameters[2];
                if (obj.textureStruct.TextureFile.Length < 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                AddMessage("parameter error of " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }
            
        }
        bool DoSetColor(FunctionStruct fs)
        {
            if (fs.Parameters.Count < 4) return false;

            string para = fs.Parameters[0];
            if (para[0] != '"') //变量名称
            {
                int id = GetObjectVarIndex(para);
                if (id >= 0) para = ObjectVars[id].ObjName;
                else
                {
                    AddMessage("variable '" + para + "' not found" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }
            }
            else
            {
                para = TrimString(para);
            }
            int pos = GetObjectKeyFromParameter(para);
            if (pos < 0)
            {
                AddMessage("object not found ! " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }

            int r = Convert.ToInt32(fs.Parameters[1]);
            int g = Convert.ToInt32(fs.Parameters[2]);
            int b = Convert.ToInt32(fs.Parameters[3]);
            
            C3DObjectBase obj = C3DData.GetObjectByKey(pos);
            if ( obj.type == ShapeEnum.Mesh )
            {
                CMesh mesh = (CMesh)obj;
                mesh.ObjColor = System.Drawing.Color.FromArgb(r, g, b);
                return true;
            }
            else if (obj.type == ShapeEnum.Slicer)
            {
                CSlicer slicer = (CSlicer)obj;
                slicer.ObjColor = System.Drawing.Color.FromArgb(r, g, b);
                return true;
            }
            else if (obj.type == ShapeEnum.Triangles)
            {
                TriangleObj tri = (TriangleObj)obj;
                tri.color = new GlmNet.vec4(r/255f,g/255f,b/255f,1.0f);
                return true;
            }
            return false;
        }
        bool DoLoadColorScale(FunctionStruct fs)
        {
            if (fs.Parameters.Count < 2) return false;

            string para = fs.Parameters[0];
            if (para[0] != '"') //变量名称
            {
                int id = GetObjectVarIndex(para);
                if (id >= 0) para = ObjectVars[id].ObjName;
                else
                {
                    AddMessage("variable '" + para + "' not found" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }
            }
            else
            {
                para = TrimString(para);
            }
            int pos = GetObjectKeyFromParameter(para);
            if (pos < 0)
            {
                AddMessage("object not found ! " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }

            string file  = fs.Parameters[1];

            C3DObjectBase obj = C3DData.GetObjectByKey(pos);
            if (obj.type == ShapeEnum.Grid3D)
            {
                C3DGridData grid3d = (C3DGridData)obj;
                return grid3d.ColorScale.LoadClr(file);
            }
            else if (obj.type == ShapeEnum.Mesh)
            {
                CMesh mesh = (CMesh)obj;
                return mesh.ColorScale.LoadClr(file);
            }
            else if (obj.type == ShapeEnum.Slicer)
            {
                CSlicer slicer = (CSlicer)obj;
                return slicer.ColorScale.LoadClr(file);
            }
            
            return false;
        }
        bool DoSetColorRange(FunctionStruct fs)
        {
            if (fs.Parameters.Count < 3) return false;

            string para = fs.Parameters[0];
            if (para[0] != '"') //变量名称
            {
                int id = GetObjectVarIndex(para);
                if (id >= 0) para = ObjectVars[id].ObjName;
                else
                {
                    AddMessage("variable '" + para + "' not found" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }
            }
            else
            {
                para = TrimString(para);
            }
            int pos = GetObjectKeyFromParameter(para);
            if (pos < 0)
            {
                AddMessage("object not found ! " + "Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }

            double minv = Convert.ToDouble( fs.Parameters[1]);
            double maxv = Convert.ToDouble( fs.Parameters[2]);

            C3DObjectBase obj = C3DData.GetObjectByKey(pos);
            if (obj.type == ShapeEnum.Grid3D)
            {
                C3DGridData grid3d = (C3DGridData)obj;
                grid3d.ColorScale.SetValueRange(minv, maxv);
            }
            else if (obj.type == ShapeEnum.Mesh)
            {
                CMesh mesh = (CMesh)obj;
                mesh.ColorScale.SetValueRange(minv, maxv);
            }
            else if (obj.type == ShapeEnum.Slicer)
            {
                CSlicer slicer = (CSlicer)obj;
                slicer.ColorScale.SetValueRange(minv, maxv);
            }

            return false;
        }
        bool DoSleep(FunctionStruct fs)
        {
            if (fs.Parameters.Count < 1) return false;
            int sec = 0;
            if( int.TryParse(fs.Parameters[0],out sec ) )
            { 
                System.Threading.Thread.Sleep(sec);
                return true; 
            }
            else return false;
        }
        bool DoStartRecording(FunctionStruct fs)
        {
            if ( fs.Parameters.Count < 1 )
            {
                AddMessage("parameters missing" + " at Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }

            string avifile = TrimString( fs.Parameters[0] );
            int frames = 2;
            if( fs.Parameters.Count > 1 )
            {
                if( !int.TryParse(fs.Parameters[1], out frames) )
                {
                    AddMessage("parameters not correct " + " at Line " + fs.lineNo + " : " + fs.orgLine);
                    return false;
                }
            }

            return Program.m_MainForm.m_DDDForm.StartScreenRecord(avifile, frames);
        }
        bool DoEndRecording(FunctionStruct fs)
        {
            return Program.m_MainForm.m_DDDForm.EndScreenRecord();
        }
        /// <summary>
        /// 屏幕场景旋转
        /// ScreenRotate(float angle,float axisx,float axisy,float axisz)
        /// </summary>
        /// <param name="angle">角度（度）</param>
        /// <param name="axisx">x轴分量</param>
        /// <param name="axisy">y轴分量</param>
        /// <param name="axisz">z轴分量</param>
        /// <returns> true or false</returns>
        bool DoScreenRotate(FunctionStruct fs)
        {
            if ( fs.Parameters.Count < 4 )
            {
                AddMessage("parameters not correct " + " at Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }
            try 
            {
                float angle = float.Parse(fs.Parameters[0]);
                float ax = float.Parse(fs.Parameters[1]);
                float ay = float.Parse(fs.Parameters[2]);
                float az = float.Parse(fs.Parameters[3]);

                C3DData.graphics3D.Rotate(angle, new GlmNet.vec3(ax, ay, az));
                Program.m_MainForm.UpdateView();
                return true;
            }
            catch(Exception ex)
            {
                AddMessage("parameters error " + " at Line " + fs.lineNo + " : " + fs.orgLine);
                return false;
            }
        }
        /// <summary>
        /// 视点往前移动 MoveForward( float step = 1.0 )
        /// </summary>
        /// <param name="step"> 移动步长（正负），默认步长</param>
        /// <returns></returns>
        bool DoMoveForward(FunctionStruct fs)
        {
            float angle = 1.0f;
            if (fs.Parameters.Count > 0)
            {
                if( !float.TryParse(fs.Parameters[0],out angle) )
                {
                    angle = 1.0f;
                }                
            }
            C3DData.graphics3D.MoveForward(angle);
            Program.m_MainForm.UpdateView();
            return true;
        }
        /// <summary>
        /// 视点往前移动 MoveForward( float step = 1.0 )
        /// </summary>
        /// <param name="step"> 移动步长（正负），默认步长1</param>
        /// <returns>ture or false</returns>
        bool DoMoveBackward(FunctionStruct fs)
        {
            float angle = 1.0f;
            if (fs.Parameters.Count > 0)
            {
                if (!float.TryParse(fs.Parameters[0], out angle))
                {
                    angle = 1.0f;
                }
            }
            C3DData.graphics3D.MoveBackward(angle);
            Program.m_MainForm.UpdateView();
            return true;
        }

        public int LoadScript(string file)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                Lines.Clear();
                string line;                
                while( (line = sr.ReadLine()) != null )
                {
                    Lines.Add(line);                    
                }
                sr.Close();
                fs.Close();                
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
            }

            return Lines.Count;
        }
       
        /// <summary>
        /// Parse from a string
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public FunctionStruct Parse(string line, int lineno = 0)
        {
            string[] s = line.Split(new char[] { '(', ')', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (s.Length < 1) return new FunctionStruct();

            //参数列表
            List<string> ss = new List<string>();

            //再次拆分
            string str;
            for (int i=0;i<s.Length;i++)
            {
                str = s[i].Trim();
                str = str.Replace("=", " = "); //防止等号前后连在一起

                if (str.Length < 1) continue;
                if (str[0] != '"')
                {
                    //空格拆分
                    string[] s1 = str.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < s1.Length; j++)
                    {
                        string s2 = s1[j].Trim();
                        if (s2.Length > 0) ss.Add(s2);
                    }
                }
                else ss.Add(str);
            }
            
            
            //解析变量声明
            VarObjectStruct vs = FunctionStruct.ParseVar(ss);
            if ( ss.Count < 1 ) return new FunctionStruct(); //变量声明语句

            //解析参数
            FunctionEnum key = FunctionStruct.GetFunctionKey(ss[0]);
            FunctionStruct fs = new FunctionStruct(key, line, lineno);
                        
            if (vs.IsValid) // 变量赋值
            {
                fs.VarName = vs.VarName;
            }

            if (key != FunctionEnum.None)
            {
                for (int i = 1; i < ss.Count; i++)
                {
                    fs.AddParameter(ss[i]);
                }
            }
            

            return fs;
        }
        public void DoScripts()
        {
            string line;
            char[] cc = new char[] { ' ', '\t' };
            for (int k = 0; k<Lines.Count;k++ )
            {                
                line = Lines[k].Trim(cc);
                if ( line.Length > 0 )
                {
                    if (line[0] == '#' || line[0] == '/' || line[0] == '!')
                        continue;

                    FunctionStruct fs = Parse(line, k);
                    if (fs.IsValid)
                    {
                        DoFunction(fs);                        
                    }
                }                
            }            
        }        

        public bool DoFunction(FunctionStruct fs)
        {
           bool ret = false;
           switch( fs.Key)
            {
                case FunctionEnum.UpdateDraw:
                    ret = DoUpdateDraw(fs);                    
                    break;
                case FunctionEnum.LoadObject:
                    ret = DoLoadObject(fs);                   
                    break;
                case FunctionEnum.RemoveObject:
                    ret = DoRemoveObject(fs);                    
                    break;
                case FunctionEnum.SetVisible:
                    ret = DoSetVisible(fs);                   
                    break;
                case FunctionEnum.Offset:
                    ret = DoOffset(fs);                    
                    break;
                case FunctionEnum.Rotate:
                    ret = DoRotate(fs);                    
                    break;
                case FunctionEnum.Scale:
                    ret = DoScale(fs);                    
                    break;
                case FunctionEnum.SetAlpha:
                    ret = DoSetAlpha(fs);                    
                    break;
                case FunctionEnum.SetColor:
                    ret = DoSetColor(fs);                    
                    break;
                case FunctionEnum.SetTexture:
                    ret = DoSetTexture(fs);                    
                    break;
                case FunctionEnum.LoadColorScale:
                    ret = DoLoadColorScale(fs);                    
                    break;
                case FunctionEnum.SetColorRange:
                    ret = DoSetColorRange(fs);                    
                    break;
                case FunctionEnum.Sleep:
                    ret = DoSleep(fs);                    
                    break;
                case FunctionEnum.StartRecording:
                    ret = DoStartRecording(fs);
                    break;
                case FunctionEnum.EndRecording:
                    ret = DoEndRecording(fs);
                    break;
                case FunctionEnum.ScreenRotate:
                    ret = DoScreenRotate(fs);
                    break;
                case FunctionEnum.MoveBackward:
                    ret = DoMoveBackward(fs);
                    break;
                case FunctionEnum.MoveForward:
                    ret = DoMoveForward(fs);
                    break;
                /*                 

        ObjectRotate = 4,
        ObjectTranslation = 5,
        ObjectVisible = 4,
        ObjectColor = 7,
        ObjectAlpha = 8,
        ObjectEnableTexture = 9,
        ObjectDisableTexture = 10,*/
                default:break;
            }
            return ret;
        }
        
        void UpdateTree()
        {
            Program.m_MainForm.m_ObjectForm.UpdateTree();
        }
        void AddObject(C3DObjectBase obj, bool update_ange = true )
        {
            C3DData.AddObject(obj, update_ange);
            UpdateTree();
        }
        bool RemoveObjectByKey(int key)
        {
            C3DData.RemoveObjectByKey(key);
            UpdateTree();
            return true;
        }
        void UpdateDraw(bool refresh = false )
        {
            if( refresh ) Program.m_MainForm.UpdateDraw();
            else Program.m_MainForm.UpdateView();
        }
        void AddMessage(string message)
        {
            Program.m_MainForm.AddtoInfo(message);
        }
    }
}
