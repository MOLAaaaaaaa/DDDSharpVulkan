using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCollection
{
    public class AsciiReader
    {
        FileStream fs;
        StreamReader sr;
        public int curline;
        public string errMsg;

        public AsciiReader()
        {
            
        }
        public bool Load(string filename)
        {
            curline = 0;
            errMsg = "";
            fs = null;
            sr = null;
            try
            {
                fs = new FileStream(filename, FileMode.Open);
                sr = new StreamReader(fs);
                return true;
            }
            catch (Exception e)
            {
                errMsg = e.Message;
                return false;
            }
        }
        public bool SeekHeader(string header)
        {
            return SeekSection(header);
        }

        //read 1 line from stream
        public string GetLine()
        {
            string str="";
            char[] cc = new char[] { ' ', '\t' };
            while ( (str = sr.ReadLine()) != null )
            {
                curline++;
                str = str.Trim( cc );
                if (str.Length >0 )break;
            }
            return str;
        }

        //ignoreCaps = false ,ignore the Upper and Lower 
        public bool SeekSection(string section,bool ignoreCaps = false)
        {
            string str;
            string s1 = section;
            if( ignoreCaps) s1 = section.ToLower();

            while( (str = GetLine() ) != null )
            {
                if (str.Length > 0)
                {
                    if (ignoreCaps) str = str.ToLower();
                    if (str == s1) return true;
                }
            }

            return false;
        }
        // name = 1.2
        public string GetItemValue(string str, string name)
        {
            string[] ss = str.Split( new Char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length < 2) return "";

            //check name
            string s1 = ss[0].Trim(new char[] { ' ', '\t'});
            if (s1.ToLower() != name.ToLower()) return "";

            return ss[1];
        }
        //
        public List<double> GetBlockValues(int count)
        {
            string str;
            //trim chars
            char[] cc1 = new char[] { '{','(','}',')',';' };
            char[] cc2 = new char[] { ' ', '\t', ',', ';' };
            string[] ss;

            List<double> values = new List<double>();
            int icount = 0;
            while ((str = GetLine()) != null)
            {
                if (str.Length > 0)
                {
                    str = str.Trim(cc1);
                    ss = str.Split(cc2,StringSplitOptions.RemoveEmptyEntries);
                    for(int i=0;i<ss.Length;i++)
                    {
                        values.Add(Convert.ToDouble(ss[i]));
                        icount++;
                        if (icount >= count) return values;
                    }
                }
            }
            return values;
        }

        public void Close()
        {
            try
            {
                sr.Close();
                fs.Close();
            }
            catch(Exception e)
            {
                errMsg = e.Message;
            }
        }
    }
}
