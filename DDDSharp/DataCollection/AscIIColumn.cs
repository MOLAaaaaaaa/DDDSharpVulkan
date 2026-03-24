using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace DataCollection
{
    public class stringRow
    {
        public List<string> pData;
        public stringRow()
        {
            pData = new List<string>();
        }
        public string this[int id]
        {
            get { return pData[id]; }
            set {  pData[id] = value;}
        }
        public void Clear() { pData.Clear(); }
        public void Add(string val) { pData.Add(val); }
        public int Count { get { return pData.Count; } }
        public string GetColumn(int index)
        {
            if (index < 0 || index >= pData.Count) return null;
            else return pData[index];
        }
    }

    //分页读取数据
    public class FilePage: AscIIColumn
    {
        public int PageIndex = 0; //页面序号       
        public long StartStreamPosition = 0;//数据开始定位（准确）
        public long EndStreamPosition = 0;//数据结束定位（不准确）
        public int LineNum = 0; //数据行数
        public FilePage(int index,int pagenum)
        {
            PageIndex = index;
            PageNum = pagenum;
            LineNum = 0;
            StartStreamPosition = 0;
            EndStreamPosition = 0;
            Titles.Clear();
            pData.Clear();
        }
        public void AddTitles(List<string>titles)
        {
            Titles.AddRange(titles);
        }
        /// <summary>
        /// 按文本行方式分页读取？？
        /// 由于存在缓存，该方法读取行定位不准确，不建议用
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public override bool Load(StreamReader sr)
        {
            try 
            {
                pData.Clear();

                //分页时候已经生成了标题行？
                //if( PageIndex == 0) Titles.Clear();

                int i = 0;
                string line = "";
                sr.BaseStream.Position = StartStreamPosition;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length < 1) continue;//空行
                    line = line.Trim(trimChars);
                    if (line.Length < 1) continue;
                    if (IsRemarkedLine(line)) continue; //注释行

                    if (i % Interval == 0) ProcessLine(line);
                    
                    i++;
                    
                    if ( Row >= PageNum ) break;
                }
                return true;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }//public override bool Load(StreamReader sr)        

        /// <summary>
        ///按二进制，字节方式读取 
        ///由于StreamReader读取采用了缓存方式，BaseStream.Position定位不准确
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public bool LoadFrom(BinaryReader sr)
        {
            try
            {
                int i = 0;

                pData.Clear();             
                
                //创建pages已经创建标题行？？
                //if (PageIndex == 0) Titles.Clear();

                string line = "";
                sr.BaseStream.Position = StartStreamPosition;
                while ((line = ReadLine(sr)) != null)
                {
                    if (line.Length < 1) continue;//空行
                    line = line.Trim(trimChars);
                    if (line.Length < 1) continue;
                    if (IsRemarkedLine(line)) continue; //注释行

                    if (i % Interval == 0) ProcessLine(line);

                    i++;

                    if (Row >= PageNum) break;
                }
                return true;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }//public override bool Load(StreamReader sr)     
    }


    public class AscIIColumn
    {
        public List<string> Titles = new List<string>();
        
        public int Interval = 1;

        public string errMessage = "";
        public double NullValue = CSurferGrid.blankValue;

        protected char[] remarkChars = new char[] { '/', '#', '!' };
        protected char[] splitChars = new char[] { ' ', ',', '\t' };
        protected char[] trimChars = new char[] { ' ', '\t','"' };

        public List<stringRow> pData = new List<stringRow>();

        public int PageNum = 500;
        public int Pages = 0;
        public long TotalRows = 0;
        public long MaxLinesCount = 1000000; 
        public List<FilePage> PageData = new List<FilePage>();

        public Encoding encoding = Encoding.UTF8;
        public int CodePage 
        { 
            get { return encoding.CodePage; }
            set
            {
                encoding = Encoding.GetEncoding(value);
            }
        }
       
        public bool IsNullValue(double value, double nullvalue, double zero = 0.0001)
        {
            if (value == nullvalue) return true;
            if (nullvalue == 0 && value <= zero) return true;
            if (Math.Abs(value - nullvalue) / Math.Abs(nullvalue) <= zero) return true;
            else return false;
        }

        public virtual int Col
        {
            get
            {
                return Titles.Count;
            }
        }
        public stringRow this[int id]
        {
            get { return pData[id]; }
        }
        public virtual int Row { get { return pData.Count; } }

        public AscIIColumn()
        {

        }
        public void Clear()
        {
            Titles.Clear();
            PageData.Clear();
            pData.Clear();
            Pages = 0;
            TotalRows = 0;
            errMessage = "";
        }

       /// <summary>
       /// 预览全文，读取文本总行数
       /// 同时生成分页信息PageData
       /// </summary>
       /// <param name="path"></param>
       /// <returns></returns>
        public long GetFileTotalRows(string path)
        {
            try
            {
                string line;
                long lineNo = 0;

                PageData.Clear();
                Titles.Clear();
                pData.Clear();

                FilePage page = new FilePage(0,PageNum);
                page.StartStreamPosition = 0;
                                
                BinaryReader sr = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read), encoding);
                long filelength = sr.BaseStream.Length;

                long lastPos = 0; //上一行位置
                List<byte> bytes = new List<byte>();
                byte c;
                while ( sr.BaseStream.Position < filelength )
                {
                    c = sr.ReadByte();
                    if ( c != 13 && c != 10 ) //换行符号"\r\n"
                    { 
                        bytes.Add(c);
                        continue;
                    }

                    //换行符号"\r\n"
                    if ( bytes.Count > 0 ) 
                    {
                        line = encoding.GetString(bytes.ToArray());
                        
                        bytes.Clear();

                        if (line.Length < 1) continue;//空行
                        line = line.Trim(trimChars);
                        if (line.Length < 1) continue;
                        if ( IsRemarkedLine(line) ) continue; //注释行

                        if (lineNo == 0)//首行，创建标题
                        {
                            if ( IsTitleLine(line) )
                            {
                                CreateTitlesFromLine(line);
                                page.StartStreamPosition = sr.BaseStream.Position;
                                lineNo++;
                                continue;
                            }
                            else CreateDefaultTitles(line);
                        }

                        //行数据
                        page.LineNum++;
                        if ( page.LineNum >= PageNum )
                        {
                            page.EndStreamPosition = sr.BaseStream.Position;
                            page.AddTitles(Titles);
                            PageData.Add(page);

                            page = new FilePage(PageData.Count,PageNum);
                            page.StartStreamPosition = sr.BaseStream.Position;
                        }

                        lineNo++;
                        lastPos = sr.BaseStream.Position;

                    }//if ( bytes.Count > 0 ) 

                }//while ( sr.BaseStream.Position < filelength )

                //最后一页数据
                if (page.LineNum > 0)
                {
                    page.EndStreamPosition = sr.BaseStream.Position;
                    page.AddTitles(Titles);
                    PageData.Add(page);
                }
                sr.Close();
                return lineNo;
            }
            catch (Exception e)
            {
                errMessage = e.Message;
                return 0;
            }
        }       
        /// <summary>
        /// 从流中读取一行，按字节读取
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public virtual string ReadLine(BinaryReader sr)
        {           
            byte c;
            List<byte> bytes = new List<byte>();
            string text;            

            while(sr.BaseStream.CanRead)
            {
                c = sr.ReadByte();
                if (c == 13 || c == 10)
                {
                    if (bytes.Count > 0)
                    {
                        text = encoding.GetString(bytes.ToArray());
                        bytes.Clear();
                        return text;
                    }
                }
                else bytes.Add(c);
            }
            return null;
        }
        public virtual bool Load(string path, Encoding _encoding = null )
        {
            StreamReader sr;
            
            if (_encoding != null) encoding = _encoding;
            sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read),
                      encoding);

            bool ret = Load(sr);
            sr.Close();
            return ret;
        }
        /// <summary>
        /// 载入数据，一次性载入全部，按文本方式读取
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public virtual bool Load(StreamReader sr)
        {
            try 
            {
                pData.Clear();
                Titles.Clear();
                long i = 0;
                string line = "";
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length < 1) continue;//空行
                    if (line.Length > 50000)  //识别错误
                    {
                        errMessage = "not a recognized format or not a recognized Encoding.";
                        return false;
                    }
                    line = line.Trim(trimChars);
                    if (line.Length < 1) continue;
                    if (IsRemarkedLine(line)) continue; //注释行
                    
                    if (i == 0)//首行，检测是否标题行，创建标题
                    {
                        if (IsTitleLine(line))
                        {
                            CreateTitlesFromLine(line);
                            i++;
                            continue;
                        }
                        else CreateDefaultTitles(line);
                    }

                    if (i % Interval == 0) ProcessLine(line);

                    i++;
                    if (i >= MaxLinesCount) 
                    {
                        errMessage = "Exceed max lines count as " + MaxLinesCount;
                        break; 
                    }
                }
                
                TotalRows = pData.Count;

                return true;
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return false;
            }
        }

        public virtual bool Export(string path)
        {
            if ( TotalRows < 1 ) return false;
            try 
            {
                StreamWriter sr = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write), encoding);
                stringRow rows;
                string line;

                if( Titles.Count > 0 )
                {
                   // if( !IsDefaultTitle() )
                    {
                        line = "";
                        for(int i=0;i<Titles.Count;i++)
                        {
                            line += Titles[i];
                            if (i < Titles.Count - 1) line += ",  ";
                        }
                        sr.WriteLine(line);
                    }
                }

                for (int i = 0; i < TotalRows; i++)
                {
                    rows = GetRow(i);
                    line = "";
                    for (int j = 0; j < rows.Count; j++)
                    {
                        line += rows[j];
                        if (j < rows.Count - 1) line += ",  ";
                    }
                    sr.WriteLine(line);
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
        //set trim chars
        public void SetTrimChars(string charString)
        {
            if (charString.Length > 0 )
                trimChars = charString.ToCharArray();
        }

        //set remark chars
        public void SetRemarkChars(string charString)
        {
            if ( charString.Length > 0)
                remarkChars = charString.ToCharArray();
        }

        //set split chars
        public void SetSplitChars(string charString)
        {            
            if (charString.Length > 0)
                splitChars = charString.ToCharArray();            
        }
        //trim line 
        protected string TrimLine(string line)
        {
            return line.Trim(trimChars);
        }

        //首字符是否注释行，
        protected bool IsRemarkedLine(string line)
        {
            if (line.Length > 0)
            {   
                char c = line[0];
                for (int i = 0; i < remarkChars.Length; i++)
                {
                    if (remarkChars[i] == c) return true;
                }
            }
            return false;
        }
        //是否数字？
        public static bool IsNumeric(string value)
        {
            double v = 0;
            return double.TryParse(value, out v);
            //return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
        public bool IsDefaultTitle()
        {
            if ( Titles.Count < 0 ) return true;
            if (Titles[0].ToLower().Contains("column"))
                return true;
            return false;
        }
        //创建默认标题Column A
        public void CreateDefaultTitles(string line)
        {
            Titles.Clear();
            string[] ss = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

            string colstring = "Column_";
            char c = 'A';
            for (int i = 0; i < ss.Length; i++)
            {
                Titles.Add( colstring + (char)(c+i) );
            }
        }

        //从行数据中创建标题
        public void CreateTitlesFromLine(string line)
        {
            Titles.Clear();
            string[] ss = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);            
            for (int i = 0; i < ss.Length; i++)
            {
                Titles.Add(ss[i]);
            }
            ss = null;
        }

        /// <summary>
        /// 是否标题行，满足：全字符行，无数字
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool IsTitleLine(string line)
        {
            string[] ss = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ss.Length; i++)
            {
                if ( IsNumeric(ss[i]) )
                {
                    ss = null;
                    return false;
                }
            }
            ss = null;
            return true;
        }
        public bool IsTitleLine(stringRow row)
        {
            foreach( string ss in row.pData )
            {
                if ( IsNumeric(ss.Trim()) )
                {
                    return false;
                }
            }            
            return true;
        }
        /// <summary>
        /// 处理行数据并添加到数组
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public int ProcessLine(string line)
        {
            string[] ss = line.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            //string[] ss = line.Split(splitChars);
            stringRow row = new stringRow();
            for (int i = 0; i < ss.Length; i++)
            {
                row.Add(ss[i]);
            }
            pData.Add(row);
            return ss.Length;
        }

        public stringRow GetRow(int irow)
        {
            return pData[irow];
        }
        public string GetValue(int irow, int icol)
        {
            int row = pData.Count;
            if (irow < 0 || icol < 0 || irow >= row) return null;
            stringRow ss = pData[irow];
            int col = ss.Count;
            if (icol >= col) return null;
            return ss.GetColumn(icol);
        }
    }//class 
}
