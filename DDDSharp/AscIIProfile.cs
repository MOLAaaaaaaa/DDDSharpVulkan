using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextReaderWriter
{
    static public class AscIIProfile
    {
        /// <summary>
        /// 从ini文件中查找指定数据段
        /// </summary>
        /// <param name="section">段名称</param>
        /// <param name="sr">流文件句柄</param>
        /// <param name="CapsOn">区分大小写</param>
        /// <param name="fromBegin">从头开始查找</param>
        /// <param name="accurateMatch">是否精确匹配</param>
        /// <returns>是否找到标记</returns>
        public static bool SeekSection(string section, ref StreamReader sr,bool CapsOn = false, bool accurateMatch = false,bool fromBegin = true)
        {
            string str;
            char[] cc = new char[] { ' ', '\t' };
            string s1, s2;
            
            s2 = section;
            if (!CapsOn) s2 = s2.ToLower(); //不区分大小写

            if ( fromBegin ) 
            { 
                sr.BaseStream.Seek(0L, SeekOrigin.Begin);
                sr.DiscardBufferedData();
            }

            while ((str = sr.ReadLine()) != null)
            {
                str = str.Trim(cc);
                if ( str.Length < 1 ) continue;
                s1 = str;
                
                if (!CapsOn) //不区分大小写
                    s1 = s1.ToLower();
                
                if (accurateMatch) //精确匹配
                {
                    if (s1.Equals(s2)) return true;
                }
                else //模糊匹配
                {
                    if (s1.Contains(s2)) return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 读取一行
        /// </summary>
        /// <param name="sr"> 文件流句柄</param>
        /// <param name="filterEmpty"> 是否过滤空行</param>
        /// <returns> 字符串值，null文件末
        /// </returns>
        public static string ReadLine(StreamReader sr, bool filterEmpty = true )
        {
            string line = sr.ReadLine();
            if (line == null) return null;

            if ( filterEmpty ) 
            {
                while ( line.Length < 1)
                {
                    line = sr.ReadLine();
                    if (line == null) return null;
                }
            }
            return line;
        }

        public static string GetLineValue( string str, string name )
        {
            char[] cc = new char[] { ' ', '\t' };
            string[] ss = str.Split(new Char[] {'='}, 2);
            if ( ss.Length < 2 ) return "";

            string s1 = ss[0].Trim(cc);
            if (s1.ToLower() != name.ToLower()) return "";

            return ss[1].Trim(cc);
        }
        public static bool ReadDoubleValue(StreamReader sr,string name,out double val)
        {
            val = 0;
            string line = ReadLine(sr);
            if (line == null) return false;
            return double.TryParse( GetLineValue(line, name), out val );
        }
        public static double ReadDoubleValue(StreamReader sr, string name)
        {
            string line = ReadLine(sr);
            if (line == null)
            {
                throw (new Exception("at the end of file."));
                //return null; 
            }
            return double.Parse(GetLineValue(line, name));
        }
        public static bool ReadFloatValue(StreamReader sr, string name, out float val)
        {
            val = 0;
            string line = ReadLine(sr);
            if (line == null) return false;
            return float.TryParse(GetLineValue(line, name), out val);
        }
        public static float ReadFloatValue(StreamReader sr, string name)
        {
            string line = ReadLine(sr);
            if (line == null)
            {
                throw (new Exception("at the end of file."));
                //return null; 
            }
            return float.Parse(GetLineValue(line, name));
        }
        public static bool ReadIntValue(StreamReader sr, string name,out int val)
        {
            val = 0;
            string line = ReadLine(sr);
            if (line == null) return false;            
            return int.TryParse(GetLineValue(line, name), out val);            
        }
        public static int ReadIntValue(StreamReader sr, string name)
        {
            string line = ReadLine(sr);
            if ( line == null )
            {
                throw (new Exception("at the end of file."));
                //return null; 
            }
            return int.Parse(GetLineValue(line, name));
        }
        public static bool ReadBoolValue(StreamReader sr, string name, out bool val)
        {
            val = false;
            string line = ReadLine(sr);
            if (line == null) return false;
            return bool.TryParse(GetLineValue(line, name), out val);
        }
        public static bool ReadBoolValue(StreamReader sr, string name)
        {
            return bool.Parse( GetLineValue( ReadLine(sr), name));
        }

        public static bool ReadStringValue(StreamReader sr, string name,out string val)
        {
            val = "";
            string line = ReadLine(sr);
            if (line == null) return false;
            val = GetLineValue(line, name);
            return true;
        }
        public static string ReadStringValue(StreamReader sr, string name)
        {
            string val = "";
            string line = ReadLine(sr);
            if (line == null) 
            {
                throw ( new Exception("at the end of file.") );
                //return null; 
            }
            val = GetLineValue(line, name);
            return val;
        }
    }
}
