using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCollection;
namespace DDDSharp
{
    //convert sgy to 3DGrid from GeoInterp software
    public class GeoInterpConvert
    {
        public string sgyFile;
        public string geoFile;
        GeoInterpConvert()
        {
        }
    }

    public struct SgyDao
    {
        public int Index;  //Index num of the dao
        public float[] pData;//amp array
    }
    public class SgyData3D
    {   
        //array of each track
        public List<SgyDao> pDao = new List<SgyDao>();
        //coords array of each track
        public List<Vector32> pDaoCoords = new List<Vector32>();
        public int nSampleNum = 0;
        public int nSampleTimeSpace = 0;
        public string ErrMsg = "";
        public int Length
        {
            get { return pDao.Count; }
        }
        public void Clear()
        {
            pDao.Clear();
            pDaoCoords.Clear();
            nSampleNum = 0;
            nSampleTimeSpace = 0;
        }
        public double minx = 0;
        public double maxx = 0;
        public double miny = 0;
        public double maxy = 0;
        public double minz = 0;
        public double maxz = 0;
        public double minv = 0;
        public double maxv = 0;
        
        void AddDao(SgyDao dao)
        {
            pDao.Add(dao);
        }
        public void GetDataRange()
        {
            float val = 0;
            for(int i=0;i<pDao.Count;i++)
            {
                for(int j=0;j<nSampleNum;j++)
                {
                    val = pDao[i].pData[j];
                    
                    if (i==0&&j==0)
                    {
                        minv = maxv = val;
                    }
                    else
                    {
                        if (minv > val ) minv = val;
                        if (maxv < val ) maxv = val;
                    }
                }
            }
        }
        //for sun segy data,ibm float
        public bool LoadFrom(string sgyfile)
        {
            Clear();
            BinaryReader br;
            FileStream fs;
            try
            {
                fs = new FileStream(sgyfile, FileMode.Open);
                br = new BinaryReader(fs);
            }
            catch (IOException e)
            {
                ErrMsg = "Open file failed.\n" + e.Message;
                return false;
            }
            
            byte[] buf = new byte[2];            
            long ret = fs.Seek(3212L, SeekOrigin.Begin);
            ret = fs.Read(buf, 0, 2);

            //this is not correct ,nDao
            uint nDao = ConvertData.BitToUInt(buf);

            fs.Seek(3216L, SeekOrigin.Begin);
            fs.Read(buf, 0, 2);
            nSampleTimeSpace = ConvertData.BitToInt(buf);
            if (nSampleTimeSpace < 0) nSampleTimeSpace = -nSampleTimeSpace;

            fs.Seek(3220L, SeekOrigin.Begin);
            fs.Read(buf, 0, 2);
            nSampleNum = ConvertData.BitToInt(buf);

            double trackLen = nSampleNum * nSampleTimeSpace / 10000.0;

            if (nDao < 1 || nSampleNum < 1 || nSampleTimeSpace < 1)
            {
                ErrMsg = "read track num error";
                br.Close();
                fs.Close();
                return false;
            }

            fs.Seek(3600L, SeekOrigin.Begin);
            byte[] pDaoHeader= new byte[240];
            
#pragma warning disable CS0168 // 声明了变量“x”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“y”，但从未使用过
            double x, y,z;
#pragma warning restore CS0168 // 声明了变量“y”，但从未使用过
#pragma warning restore CS0168 // 声明了变量“x”，但从未使用过
#pragma warning disable CS0168 // 声明了变量“i”，但从未使用过
            int i, j;            
#pragma warning restore CS0168 // 声明了变量“i”，但从未使用过
            minz = maxz = 0;
            byte[] cc = new byte[4];
            for (nDao = 0;; nDao++) //每道的样点数 281
            {
                //header of track
                if (fs.Read(pDaoHeader, 0, 240) <= 0) break;

                SgyDao sd = new SgyDao();                
                //cmp道集号
                sd.Index = (int)ConvertData.MakeLongFromBytes(pDaoHeader[20], pDaoHeader[21], pDaoHeader[22], pDaoHeader[23]);

                //x坐标784705.56875	14066071.555,read x,y from geo file
                //x = ConvertData.MakeLongFromBytes(pDaoHeader[80], pDaoHeader[81], pDaoHeader[82], pDaoHeader[83]);// / 10000.0f;                
                //y = ConvertData.MakeLongFromBytes(pDaoHeader[84], pDaoHeader[85], pDaoHeader[86], pDaoHeader[87]);// / 10000.0f;
                z = ConvertData.MakeLongFromBytes(pDaoHeader[40], pDaoHeader[41], pDaoHeader[42], pDaoHeader[43]) / 10000.0f;

               // samleplenum = MakeIntFromBytes(pDaoHeader[114], pDaoHeader[115]);
              //  sampletimespace = MakeIntFromBytes(pDaoHeader[116], pDaoHeader[117]);
              //  trackLen = nSampleNum * sampletimespace / 10000.0;

                //z is the elevation 
                if (minz == maxz)
                {
                    maxz = z;
                    minz = maxz - trackLen;
                }
               // else
               // {
               //     if (minz > maxz - trackLen) minz = maxz - trackLen;
               //     if (maxz < z) maxz = z;
              //  }

                sd.pData = new float[nSampleNum];
                if (sd.pData == null)
                {
                    ErrMsg = "Allocate memory failed.";
                    br.Close();
                    fs.Close();
                    Clear();
                    return false;
                }
                for (j = 0; j < nSampleNum; j++)
                {
                    fs.Read(cc, 0, 4);
                    //sd.pData[j] = (float)ConvertData.BitToDouble(cc);
                    //sd.pData[j] = ConvertData.IEEEtoIBM(sd.pData[j]);
                    sd.pData[j] = ConvertData.IBMtoIEEE(cc);
                    //sd.pData[j] = ConvertData.toIEEEfloat(cc);
                    //sd.pData[j] = (ulong)(sd.pData[j]);// MakeULongFromBytes(cc[0], cc[1], cc[2], cc[3],true);
                }
                try
                {
                    pDao.Add(sd);
                }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
                catch(Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
                {
                    ErrMsg = "no enough memory.";
                    br.Close();
                    fs.Close();
                    Clear();
                    return false;
                }
               // pDaoCoords.Add(new Vector32((float)x, (float)y, (float)maxz));
            } //for (i = 0; i < nDao; i++)

            br.Close();
            fs.Close();

          //  if (nDao != pDao.Count )
	        {
		    //    ErrMsg = "not all data are loaded correctly.";
		      //  return false;
	        }	        
            ErrMsg = " load SGY data successed.";

            GetDataRange();

            return true;
        }//LoadFrom
    }//class Sgy3D
    
}
