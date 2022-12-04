using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RegisterAndEncrypt
{
    public class RegisterVerify
    {
        public string userid { get; set; } = "";
        public string encryptKeyWord { get; set; } = "";
        public string Name { get; set; } = "3D Surfer";
        public string License { get; set; } = "";
        public string Version { get; set; } = "3.0";
        public string Date { get; set; } = "2020-01-01";

        public string ErrMsg = "";
        public RegisterVerify(string _userid)
        {
            userid = _userid;
        }

        public bool ReadFromRegister()
        {
            try 
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software");
                if (key == null) return false;
                key = key.OpenSubKey("3DSurferV3");
                if (key == null) return false;
                Name = key.GetValue("Name").ToString();
                License = key.GetValue("License").ToString();

                Version = key.GetValue("Version").ToString();
                Date = key.GetValue("Date").ToString();
                return IsValid();
            }
            catch(Exception e)
            {
                ErrMsg = e.Message;
                return false;
            }
        }
        public bool WriteToRegister()
        {
            try 
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                if (key == null) return false;
                key = key.CreateSubKey("3DSurferV3");                
                if (key == null) return false;

                DateTime t = DateTime.Now;
                string date = t.Year + "-" + t.Month + "-" + t.Day;
                key.SetValue("Name", Name);
                key.SetValue("License", License);
                key.SetValue("Version", Version);
                key.SetValue("Date", date);

                return true;
            }
            catch(Exception e)
            {
                ErrMsg = e.Message;
                return false;
            }            
        }
        
        public bool IsValid()
        {
            if ( Name.Length < 1 || 
                 License.Length < 1 || 
                 Version.Length < 1 || Date.Length<1 ) 
                return false;
            else return true;
        }

        public string GetKeyFromUserID(string _userid)        
        {
            string ss = "";
            if (_userid.Length < 8)
            {
                ss = _userid;
                for (int i = 0; i < 8 - _userid.Length; i++) ss += "*";
            }
            else if (_userid.Length > 8) ss = _userid.Substring(0, 8);
            else if (_userid.Length == 8) ss = _userid;
            return ss;
        }

        public string GetKeyFromEncryptKeyWord(string _encryptKeyWord)
        {
            //密匙解密from _encryptKeyWord
            DESEncrypt des0 = new DESEncrypt(GetKeyFromUserID(userid));
            return des0.Decrypt(_encryptKeyWord);            
        }

        //_encryptKeyWord 已加密密匙
        public bool CreateLicense(string _encryptKeyWord)
        {
            try 
            {
                //密匙解密
                string keyword = GetKeyFromEncryptKeyWord(_encryptKeyWord);
                if( keyword.Length != 8 )
                {
                    ErrMsg = "the Key not correct.";
                    return false;
                }

                DESEncrypt des = new DESEncrypt(keyword);
                License = "";
                License += des.Encrypt(HardWareInfo.GetInfo(HardWareInfo.InfoType.CPUID));
                License += "|";
                License += des.Encrypt(HardWareInfo.GetInfo(HardWareInfo.InfoType.DiskID));
                License += "|";
                License += des.Encrypt(HardWareInfo.GetInfo(HardWareInfo.InfoType.MemoryID));
                License += "|";
                License += _encryptKeyWord;
                return true;
            }
            catch(Exception e)
            {
                ErrMsg = e.Message;
                return false;
            }            
        }

        public bool Verify(HardWareInfo.InfoType type)
        {
            try 
            {
                string[] ss = License.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (ss.Length < 4) return false;

                //最后一项是加密密匙
                string keyword = GetKeyFromEncryptKeyWord(ss[3]);
                if (keyword.Length != 8)
                {
                    ErrMsg = "the Key not correct.";
                    return false;
                }

                if (type == HardWareInfo.InfoType.CPUID)
                {
                    DESEncrypt des = new DESEncrypt(keyword);

                    string str1 = des.Decrypt(ss[0]);
                    string str2 = HardWareInfo.GetInfo(type);
                    return str1.Equals(str2);
                }
                else if (type == HardWareInfo.InfoType.DiskID)
                {
                    DESEncrypt des = new DESEncrypt(keyword);
                    string str1 = des.Decrypt(ss[1]);
                    string str2 = HardWareInfo.GetInfo(type);
                    return str1.Equals(str2);
                }
                else
                {
                    DESEncrypt des = new DESEncrypt(keyword);
                    string str1 = des.Decrypt(ss[2]);
                    string str2 = HardWareInfo.GetInfo(type);
                    return str1.Equals(str2);
                }
            }
            catch(Exception e)
            {
                return false;
            }
        }        
    }
}
