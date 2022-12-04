using System;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
namespace DataCollection
{    
    /// <summary>
    /// Reads/writes Log ASCII files version 2.0
    /// ASCII file containing:
    /// 1. Header Information
    /// 2. Log Curve Data
    /// http://cwls.org/las
    /// 
    /// </summary>
    public class CLasFile
    {
        private void ProcessLine(char sectionId, string line, ref LasFileData lasFileData, ref List<string> tokens)
        {
            switch (sectionId)
            {
                case 'V':
                    {
                        lasFileData.VersionSection.VersionInformation.Add(SectionEntry.Parse(line));
                    }
                    break;

                case 'W':
                    {
                        lasFileData.WellInformation.Add(SectionEntry.Parse(line));
                    }
                    break;

                case 'P':
                    {
                        lasFileData.ParameterInformation.Add(SectionEntry.Parse(line));
                    }
                    break;

                case 'C':
                    {
                        lasFileData.CurveInformation.Add(SectionEntry.Parse(line));
                    }
                    break;


                case 'O':
                    {
                        lasFileData.Comments.Add(line);
                    }
                    break;

                case 'A':
                    {
                        var lineTokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        tokens.AddRange(lineTokens);

                        if (lasFileData.VersionSection.Wrap == "YES")
                        {
                            // gather tokens
                            if ((tokens.Count <= 0) && (lineTokens.Length != 1))
                                throw new LogAsciiFileException("Wrap Mode: Index channel should be on its own line.");
                        }
                        else if (tokens.Count != lasFileData.CurveInformation.Count)
                        {
                            throw new LogAsciiFileException("Number of curves doesn't match number of values");
                        }

                        if (tokens.Count >= lasFileData.CurveInformation.Count)
                        {
                            var values = tokens.Select(x => float.Parse(x)).ToArray();
                            lasFileData.AddValues(values);

                            tokens.Clear();
                        }
                    }
                    break;

                default:
                    throw new LogAsciiFileException("Invalid section header: " + sectionId);
            };
        }


        /// <summary>
        /// Reads a LAS file and loads the data into memory
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public LasFileData Read(string filename)
        {
            if (filename == null) return null;
            if (Path.GetExtension(filename).ToLower() != ".las")
                throw new LogAsciiFileException("Expected a LAS file with the las extension");

            List<string> tokens = new List<string>();

            try
            {
                LasFileData lasData = new LasFileData();

                // read all lines except empty or comment lines
                var lines = File.ReadAllLines(filename).Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line) && (line[0] != '#'));

                // validate sections - start with version and end with log data
                var sectionFlags = lines.Where(line => (line[0] == '~') && (line.Length >= 2)).ToArray();
                if (sectionFlags.Length < 4 || sectionFlags.Length > 6)
                    throw new LogAsciiFileException("Invalid number of sections");

                if (!sectionFlags[0].StartsWith("~V"))
                    throw new LogAsciiFileException("VersionInformation should be the first section");

                if (!sectionFlags[sectionFlags.Length - 1].StartsWith("~A"))
                    throw new LogAsciiFileException("LogData should be the last section");

                char sectionId = ' ';

                List<char> sectionsFound = new List<char>();

                bool versionInfoValidated = false;

                foreach (var line in lines)
                {
                    if (line[0] == '~')
                    {
                        sectionId = line[1];

                        if (sectionsFound.Contains(sectionId))
                        {
                            throw new LogAsciiFileException("Already processed this section flag: " + sectionId);
                        }

                        if ((sectionId != 'V') && !versionInfoValidated)
                        {
                            lasData.VersionSection.Validate();

                            versionInfoValidated = true;
                        }

                        sectionsFound.Add(sectionId);
                    }
                    else
                    {
                        ProcessLine(sectionId, line, ref lasData, ref tokens);
                    }
                }

                lasData.Validate();

                lasData.nullValue = lasData.GetNullDataValue();
                lasData.CreateCurvesDictionary();//创建列名称字典
                lasData.UpdateRange();
                return lasData;
            }
            catch (LogAsciiFileException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new LogAsciiFileException("Read Error: " + ex.Message, ex);
            }
        }


        private void AppendLasLine(StringBuilder b, SectionEntry entry)
        {
            string mnemstr = string.Format("{0}.{1}", entry.Mnemonic, entry.Units);
            b.AppendFormat(String.Format("{0, -11}{1, -17}: {2}", mnemstr, entry.Data, entry.Description));
        }



        /// <summary>
        /// Creates a LAS file containing the specified data
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="lasFileData"></param>
        public void Write(String filename, LasFileData lasFileData)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                ////
                // Write Version Information section
                ////
                builder.AppendFormat("~Version Information");
                foreach (var entry in lasFileData.VersionSection.VersionInformation)
                {
                    AppendLasLine(builder, entry);
                }

                ////
                // Write Well Information section
                ////
                builder.AppendFormat("~Well Information Section");
                builder.AppendFormat("#Mnem.Unit Value/Name       : Description");
                builder.AppendFormat("#--------- ----------         -----------------------");
                foreach (var entry in lasFileData.WellInformation)
                {
                    AppendLasLine(builder, entry);
                }

                ////
                // Write Parameter Information section
                ////
                builder.AppendFormat("~Parameter Information Section");
                builder.AppendFormat("#Mnem.Unit Value            : Description");
                builder.AppendFormat("#--------- ----------         -----------------------");

                foreach (var entry in lasFileData.ParameterInformation)
                {
                    AppendLasLine(builder, entry);
                }

                ////
                // Write Curve Information section
                ////
                builder.AppendFormat("~Curve Information Section");
                builder.AppendFormat("#Mnem.Unit API Codes        : Description");
                builder.AppendFormat("#--------- ----------         -----------------------");

                foreach (var entry in lasFileData.CurveInformation)
                {
                    AppendLasLine(builder, entry);
                }

                //
                // Log Data
                //
                builder.AppendFormat("#--------------------------------------------------");
                builder.AppendFormat("~A");

                // Then write on each line, a value from each trace
                for (int i = 0; i < lasFileData.LogData.Length; i++)
                {
                    String valuesStr = string.Empty;
                    for (int j = 0; j < lasFileData.LogData[i].Count; j++)
                    {
                        valuesStr += String.Format("{0,10:F4} ", lasFileData.LogData[i][j]);
                    }

                    builder.AppendFormat(valuesStr);
                }

                File.WriteAllText(filename, builder.ToString());
            }
            catch (Exception ex)
            {
                throw new LogAsciiFileException("Write Error: " + ex.Message, ex);
            }
        }
    }
    public struct LayerStruct
    {
        public string layername;        
        public float start;  //start depth
        public float end;    //end depth
    }
    public class LasDataProperties
    {
        public LasFileData lasdata = null;
        public int Count 
        {
            get 
            { 
                if (lasdata == null) return 0;
                if (lasdata.LogData == null) return 0;
                return lasdata.LogData.Length;
            }
        }
    }
    public interface IValuesProvider
    {
        string[] GetPropertiesString();
    }
    public class LasFileData: C3DObjectBaseHide, IValuesProvider
    {
        /// <summary>
        /// Manadatory - first section in file
        /// </summary>
        [CategoryAttribute("Display"), DisplayNameAttribute("Version"),Browsable(false)]
        public VersionSection VersionSection { get; internal set; }
        /// <summary>
        /// Mandatory - describes the well including the start/stop depths
        /// </summary>
        [CategoryAttribute("Display"), DisplayNameAttribute("WellInformation"), Browsable(false)]
        public List<SectionEntry> WellInformation { get; internal set; }      
        
        /// <summary>
        /// Optional - Describes input values for various parameters relating to this well
        /// </summary>
        [CategoryAttribute("Display"), DisplayNameAttribute("ParameterInformation"), Browsable(false)]
        public List<SectionEntry> ParameterInformation { get; internal set; }
        /// <summary>
        /// Mandatory - describes the curves and their units
        /// First channel is the index for all other channels.
        /// The only valid mnemonics for the index channel are DEPT, DEPTH, TIME or INDEX.
        /// </summary>
        [CategoryAttribute("Display"), DisplayNameAttribute("CurveInformation"), Browsable(false)]
        public List<SectionEntry> CurveInformation { get; internal set; }
        //从字典中快速查找指定列
        private Dictionary<string, int> CurvesInformationDictionary = new Dictionary<string, int>();
        public void CreateCurvesDictionary()
        {
            CurvesInformationDictionary.Clear();
            for (int i = 0; i < CurveInformation.Count; i++)
            {
                string key = CurveInformation[i].Mnemonic.Trim().ToLower();
                CurvesInformationDictionary.Add(key, i);
            }
        }


        /// <summary>
        /// Mandatory last section - Log Data
        /// In wrap mode, the index channel will be on its own line and will be no longer than 80 characters.
        /// This includes a carriage return and line feed.
        /// </summary>
        [CategoryAttribute("Display"), DisplayNameAttribute("Data"), Browsable(false)]
        public List<float>[] LogData { get; internal set; }
        /// <summary>
        /// Optional - remarks or comments
        /// </summary>
        [CategoryAttribute("Display"), DisplayNameAttribute("Comments"), Browsable(false)]
        public List<string> Comments { get; internal set; }
        public List<LayerStruct> pLayers;
        // store the data range of the each column ,min and max
        [CategoryAttribute("Display"), DisplayNameAttribute("DataRanges"), Browsable(false)]
        public List<Vector64> DataRanges { get; private set; } //X -MINV,Y- MAXV
        public double minDepth = 0;
        public double maxDepth = 0;
        //null value of the data
        public double nullValue { get; set; } = -999.25;
        [CategoryAttribute("Display"), DisplayNameAttribute("Name"), Browsable(true)]
        public override  string Name { get; set; } = "las data";

        public Vector64 m_pos = new Vector64(0, 0, 0);
        [CategoryAttribute("Display"), DisplayNameAttribute("Position"), Browsable(true)]
        public string PositionString 
        {
            get 
            {
                return m_pos.toString(3);
            }
            set 
            {
               if( Vector64.TryParse(value, out m_pos, 3))
                {
                    if ( Parent != null )
                    {
                        CBorehole bh = Parent as CBorehole;
                        bh.Position = m_pos;
                    }
                }                
            }
        }
       
        public int depthIndex = 0;          //深度列 序号        
        //深度列名称
        [CategoryAttribute("Display"), DisplayNameAttribute("Depth"),Browsable(true)]
        [TypeConverter(typeof(StringSelectorConverter))]
        public string DepthColumnName
        {
            get 
            {
                if (depthIndex >= 0 && depthIndex < CurveInformation.Count)
                    return CurveInformation[depthIndex].Mnemonic;
                else return "";
            }
            set {   
                  depthIndex = GetColumnIndex(value);
                }
        }
        public string[] GetPropertiesString()
        {
            string[] values = new string[CurveInformation.Count];
            int i = 0;
            foreach (SectionEntry s in CurveInformation)
                values[i++] = s.Mnemonic;
            return values;
        }
        
        public LasFileData Copy()
        {
            
            LasFileData las = new LasFileData(Name);
            las.type = type;

            las.nullValue = nullValue;
            las.m_pos = m_pos;
            las.depthIndex = depthIndex;
            las.WellInformation.AddRange(WellInformation);

            las.CurveInformation.AddRange(CurveInformation);
            las.CreateCurvesDictionary();
            
            las.DataRanges.AddRange(DataRanges);
            

            if ( LogData != null )
            {
                int n = LogData.Length;
                las.LogData = new List<float>[n];
                for (int i=0;i<n;i++)
                {
                    las.LogData[i] = new List<float>(LogData[i]);
                }
            }
            return las;
        }
        public override bool SaveAs(BinaryWriter br)
        {
            C3DData.SaveString(br, Name);

            br.Write(nullValue);

            br.Write(m_pos.x);
            br.Write(m_pos.y);
            br.Write(m_pos.z);
            br.Write(m_pos.v);

            br.Write(depthIndex);

            br.Write(WellInformation.Count);            
            for (int i=0;i< WellInformation.Count;i++)            
                WellInformation[i].SaveAs(ref br);

            br.Write(CurveInformation.Count);
            for (int i = 0; i < CurveInformation.Count; i++)
                CurveInformation[i].SaveAs(ref br);

            br.Write(DataRanges.Count);
            for (int i = 0; i < DataRanges.Count; i++)
            {
                br.Write(DataRanges[i].x);
                br.Write(DataRanges[i].y);
                br.Write(DataRanges[i].z);
            }

            int n = 0;
            if ( LogData != null ) n = LogData.Length;
            br.Write(n);
            for (int i=0;i<n;i++)
            {
                List<float> values = LogData[i];
                br.Write( values.Count );
                for (int j = 0; j < values.Count; j++)
                    br.Write(values[j]);
            }

            return true;
        }
        public override bool LoadFrom(BinaryReader br)
        {
            Name = C3DData.LoadString(br);            
            nullValue = br.ReadDouble();
            double x, y, z,v;
            x = br.ReadDouble();
            y = br.ReadDouble();
            z = br.ReadDouble();
            v = br.ReadDouble();
            m_pos = new Vector64(x, y, z,v);

            depthIndex = br.ReadInt32();

            WellInformation.Clear();
            int n = br.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                SectionEntry en = new SectionEntry("","","","");
                en.LoadFrom(ref br);
                WellInformation.Add(en);
            }
            
            CurveInformation.Clear();
            n = br.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                SectionEntry en = new SectionEntry("", "", "", "");
                en.LoadFrom(ref br);
                CurveInformation.Add(en);
            }

            DataRanges.Clear();
            n = br.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                x = br.ReadDouble();
                y = br.ReadDouble();
                z = br.ReadDouble();
                DataRanges.Add(new Vector64(x, y, z));
            }

            LogData = null;
            n = br.ReadInt32();
            if (n > 0)
            {
                LogData = new List<float>[n];                
                for (int i = 0; i < n; i++)
                {
                    LogData[i] = new List<float>();

                    int count = br.ReadInt32();
                    for (int j = 0; j < count; j++)
                        LogData[i].Add(br.ReadSingle());
                }
            }

            UpdateRange();

            return true;
        }

        public Vector64 GetDataRange(int col)
        {
            Vector64 p = new Vector64(0,0,0);
            if ( LogData == null || LogData.Length <= col ) return p;

            int i = 0;
            foreach(float v in LogData[col])
            {
                if (v == nullValue) continue;
                if (i == 0) p.X = p.Y = v;
                else
                {  
                    if ( v < p.X ) p.X = v;
                    if ( v > p.Y ) p.Y = v;
                }
                i++;
            }
            return p;
        }

        public bool GetDataRange(int col,out double min,out double max)
        {
            min = max = 0;
            if ( DataRanges.Count <= col ) return false;
            min = DataRanges[col].X;
            max = DataRanges[col].Y;
            return true;
        }
        public double GetValue(int index, int col)
        {
            if (LogData == null) return double.NaN;
            double val = LogData[col][index];
            if (val == nullValue) return double.NaN;
            else return val;            
        }
        public double GetValue(int index, string property)
        {
            if (LogData == null) return double.NaN;
            int col = GetColumnIndex(property);
            if (col < 0) return double.NaN;
            double val = LogData[col][index];
            if (val == nullValue) return double.NaN;
            else return val;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double[] GetPropertyValues(int id,List<string>properties)
        {
            if (LogData == null) return null;            
            int col, n = 0;
            double[] values = new double[properties.Count];
            float val ;
            for(int i=0;i<properties.Count;i++)            
            {
                col = GetColumnIndex( properties[i] );
                if ( col < 0 ) return null;
                val = LogData[col][id];
                if (val == nullValue) values[i] = double.NaN;
                else { values[i] = val; n++; }
            }
            if (n > 0) return values; //有效值
            else return null;//无效值
        }
        
        public int GetColumnIndex(string columnName)
        {
            string key = columnName.Trim().ToLower();

            //首先从字典中检索
            if (CurvesInformationDictionary.Count > 0)
            {
                if (CurvesInformationDictionary.TryGetValue(key, out int index))
                    return index;
                else return -1;
            }
            else return GetColumnIndexFromList(key);            
        }
        //从数组中检索
        public int GetColumnIndexFromList(string columnName)
        {
            string _colname;
            for( int i = 0; i < CurveInformation.Count; i++ )
            {
                _colname = CurveInformation[i].Mnemonic.ToLower();
                if ( _colname == columnName )
                    return i;
            }
            return -1;
        }

        //检查指定的列序号和名称是否存在于所有钻孔
        public bool IsColuwnExist (int index, string name)
        {   
            if ( index < 0 || index >= CurveInformation.Count) return false;
            string s1 = CurveInformation[index].Mnemonic.ToLower();
            string s2 = name.ToLower();
            return s1.Equals(s2);
        }
        public bool IsColuwnExist(string name)
        {
            string _colname;
            for (int i = 0; i < CurveInformation.Count; i++)
            {
                _colname = CurveInformation[i].Mnemonic.ToLower();
                if (_colname == name.ToLower())
                    return true;
            }
            return false;
        }
        public List<float> GetData(int index)
        {
            if ( index >= 0 && index < LogData.Length)
            {
                return LogData[index];
            }
            return null;
        }
        public bool GetValuesOrder(List<float>values) 
        {
            if (values[0] < values[1] ) return true;
            else return false;
        }        
        
        //折半查找法（二分法）--升序
        int BinarySearchAsc(List<float> arr, double key)
        {
            int low = 0;
            int high = arr.Count - 1;
            int mid;
            
            if (key < arr[low]) return -1;
            if (key > arr[high]) return -1;

            while (low <= high)
            {
                mid = (low + high) / 2;       //找中间值
                if (key == arr[mid]) return mid;
                else if (key > arr[mid])low = mid + 1;
                else high = mid - 1;
            }
            return (low + high)/2;
        }
        //折半查找法（二分法）--降序
        int BinarySearchDesc(List<float> arr, double key)
        {
            int low = 0;
            int high = arr.Count - 1;
            int mid;
            
            if ( key > arr[low] ) return -1;
            if ( key < arr[high] ) return -1;

            while ( low <= high )
            {
                mid = (low + high) / 2;       //找中间值
                if (key == arr[mid]) return mid;
                else if (key < arr[mid]) low = mid + 1;
                else high = mid - 1;
            }

            return (low + high) / 2;
        }
        int BinarySearch(List<float> arr, double key)
        {
            if ( arr.Count < 2 ) return -1;
            if (arr[0] < arr[1]) return BinarySearchAsc(arr, key);
            else return BinarySearchDesc(arr, key);
        }
        /// <summary>
        /// 深度列中获取指定值的序号
        /// </summary>
        /// <param name="depth">深度值</param>
        /// <returns>序号</returns>
        public int GetIndexFromDepth(double depth)
        {
            if (depth < minDepth) return -1;
            if (depth > maxDepth) return -1;

            List<float> values = GetDepthData();
            return BinarySearch(values, depth);
            //if ( GetValuesOrder(values) )
            //{
            //    //深度升序排列
            //    for (int i = 0; i < values.Count; i++)
            //    {
            //        if ( depth >= values[i] ) return i;
            //    }
            //    return values.Count - 1;
            //}
            //else
            //{
            //    //深度降序排列
            //    for (int i = values.Count - 1; i >= 0; i--)
            //    {
            //        if (depth >= values[i]) return i;
            //    }
            //    return 0;
            //}
        }
        public float GetDepth(int index)
        {
            List<float>depths = GetDepthData();
            if (depths == null) return 0;
            if (index >= 0 && index < depths.Count )
            {
                return depths[index];
            }
            else return 0;
        }
        public int GetDepthIndex(string name = "Depth")
        {
            depthIndex = 0;
            int index = GetColumnIndex(name);
            if (index >= 0) depthIndex = index;
            return depthIndex;
        }
        public List<float> GetDepthData( int iDepth = -1 )
        {
            if (iDepth < 0) depthIndex = GetDepthIndex();
            else depthIndex = iDepth;

            if ( depthIndex >=0 && depthIndex < LogData.Length )
            {
                return LogData[depthIndex];
            }
            else return null;
        }
        
        public Vector64 GetDepthRange(int iDepth = -1)
        {
            int id = depthIndex;
            if (iDepth >= 0) id = iDepth;
            if (id >= 0 && id < DataRanges.Count)
                return DataRanges[id];
            else return new Vector64();
        }

        // Get null data value
        public double GetNullDataValue()
        {
            double v = -999999;
            string strName, strDescription;
            for (int i = 0; i < WellInformation.Count; i++)
            {
                strName = WellInformation[i].Mnemonic.ToLower();
                strDescription = WellInformation[i].Description.ToLower();

                if (strName.Contains("null") || strDescription.Contains("null value"))
                {
                    try
                    {
                        v = double.Parse(WellInformation[i].Data);
                    }
                    catch { }
                    break;
                }
            }
            return v;
        }
        /// <summary>
        /// 根据数据范围
        /// </summary>
        public override void UpdateRange()
        {
            minx = maxx = m_pos.X;
            miny = maxy = m_pos.Y;
            minz = maxz = m_pos.Z;
            if ( LogData == null ) return;
            DataRanges.Clear();
            for(int i=0; i < LogData.Length; i++)
            {
                DataRanges.Add(GetDataRange(i));//列数据范围存放入X,Y
            }

            if( depthIndex >=0 )
            {
                minDepth = DataRanges[depthIndex].X;
                maxDepth = DataRanges[depthIndex].Y;
            }
        }

        internal LasFileData(string _name = "Untitled")
        {
            Name = _name;
            type = ShapeEnum.LasData;
            VersionSection = new VersionSection();
            WellInformation = new List<SectionEntry>();
            ParameterInformation = new List<SectionEntry>();
            CurveInformation = new List<SectionEntry>();
            LogData = new List<float>[0];

            Comments = new List<string>();

            //min and max values of each column
            DataRanges = new List<Vector64>();

            pLayers = new List<LayerStruct>();
        }

        public void AddLayer(LayerStruct layer)
        {
            pLayers.Add(layer);
        }
        public LayerStruct GetLayer(int i)
        {
            return pLayers[i];
        }
        public LayerStruct GetLayer(string layername)
        {
            LayerStruct layer = new LayerStruct();
            for (int i=0;i<pLayers.Count;i++)
            {
                if (pLayers[i].layername == layername)
                    return pLayers[i];
            }
            return layer;
        }
        internal void AddValues(float[] values)
        {
            if (!LogData.Any())
            {
                LogData = new List<float>[CurveInformation.Count];
                for (int i = 0; i < LogData.Length; i++)
                {
                    LogData[i] = new List<float>();
                }
            }

            if (values.Length == LogData.Length)
            {
                for (int i = 0; i < LogData.Length; i++)
                {
                    LogData[i].Add(values[i]);
                }
            }
            else
            {
                throw new LogAsciiFileException("Number of values does not match the number of curves");
            }
        }
        public override void Clear()
        {
            VersionSection.VersionInformation.Clear();
            WellInformation.Clear();
            ParameterInformation.Clear();
            CurveInformation.Clear();
            CurvesInformationDictionary.Clear();
            LogData = null;
            Comments.Clear();
            //min and max values of each column
            DataRanges.Clear();
            pLayers.Clear();
        }

        internal void Validate()
        {
            if (CurveInformation.Count <= 1)
                throw new LogAsciiFileException("There are insufficient intervals.");

            var mnemonic = CurveInformation.First().Mnemonic.ToUpper();

            if ((mnemonic != "DEPT") &&
                (mnemonic != "DEPTH") &&
                (mnemonic != "TIME"))
                throw new LogAsciiFileException("First curve channel must be DEPTH or TIME.");
        }
    }
    public class SectionEntry
    {
        public string Mnemonic { get; private set; }
        public string Units { get; private set; }
        public string Data { get; private set; }
        public string Description { get; private set; }

        public SectionEntry(string mnemonic, string units, string data, string description)
        {
            Mnemonic = mnemonic;
            Units = units;
            Data = data;
            Description = description;
        }
        public bool SaveAs(ref BinaryWriter br)
        {
            C3DData.SaveString(br, Mnemonic);
            C3DData.SaveString(br, Units);
            C3DData.SaveString(br, Data);
            C3DData.SaveString(br, Description);
           
            return true;
        }
        public bool LoadFrom(ref BinaryReader br)
        {
            Mnemonic = C3DData.LoadString(br);
            Units = C3DData.LoadString(br);
            Data = C3DData.LoadString(br);
            Description = C3DData.LoadString(br);
            return true;
        }
        internal static SectionEntry Parse(string line)
        {
            int mnemonicDelimiterIndex = line.IndexOf('.');
            int descriptionDelimiterIndex = line.LastIndexOf(':');

            string mnemonic = line.Substring(0, mnemonicDelimiterIndex).Trim();
            string units = string.Empty;
            string data = string.Empty;
            // are there any units ?
            int startIdx = mnemonicDelimiterIndex + 1;
            if ((line[startIdx] != ':') && (line[startIdx] != ' '))
            {
                // read units field
                int startDataIndex = Math.Min(line.IndexOf(' ', startIdx), line.IndexOf(':', startIdx));
                int length = startDataIndex - startIdx;
                units = line.Substring(startIdx, length).Trim();
                startIdx = startDataIndex;
            }

            // are there any data ?
            if (startIdx != descriptionDelimiterIndex)
            {
                // read data field
                int length = descriptionDelimiterIndex - startIdx;
                data = line.Substring(startIdx, length).Trim();
            }

            string description = line.Substring(descriptionDelimiterIndex + 1).Trim();

            return new SectionEntry(mnemonic, units, data, description);
        }
    }


    /// <summary>
    /// Compulsory section of a LAS file and must be the first section within the file
    /// </summary>
    public class VersionSection
    {
        public List<SectionEntry> VersionInformation { get; internal set; }
        /// <summary>
        /// File version eg. 2.0
        /// </summary>
        public string Version { get; private set; }
        /// <summary>
        /// Indicates Wrapped Mode
        /// YES - Multiple lines per depth step
        /// NO - One line per depth step
        /// </summary>
        public string Wrap { get; private set; }

        internal VersionSection()
        {
            Version = string.Empty;
            Wrap = string.Empty;
            VersionInformation = new List<SectionEntry>();
        }

        public VersionSection(List<SectionEntry> versionInformation)
        {
            VersionInformation = versionInformation;

            Validate();
        }

        internal string GetEntryData(string mnemonic)
        {
            var entry = VersionInformation.FirstOrDefault(x => x.Mnemonic == mnemonic);
            if (entry == null)
                throw new InvalidDataException($"{mnemonic} not found in VersionInformation");
            //   $ seems not allowed in 2010 ???

            return entry.Data;
        }

        internal void Validate()
        {
            Version = GetEntryData("VERS");

            float fVersion = float.Parse(Version);
            if( fVersion > 2.0 )
            //if (Version != "2.0")
                throw new NotSupportedException("Only support LAS v2.0");

            Wrap = GetEntryData("WRAP").ToUpper();
            if ((Wrap != "YES") && (Wrap != "NO"))
                throw new InvalidDataException("Wrap mode should be YES or NO");
        }
    }
    [Serializable]
    public class LogAsciiFileException : Exception
    {
        public LogAsciiFileException()
        {
        }

        public LogAsciiFileException(string message)
            : base(message)
        {
            // Add implementation.
        }

        public LogAsciiFileException(string message, Exception inner)
            : base(message, inner)
        {
            // Add implementation.
        }

        // This constructor is needed for serialization.
        protected LogAsciiFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Add implementation.
        }
    }    
}
