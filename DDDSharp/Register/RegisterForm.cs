using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;
using RegisterAndEncrypt;
using ADODatabase;

namespace DDDSharp
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
            textBoxID.Text = C3DData.UserID;
            textBoxName.Text = C3DData.UserName;
            textBoxPass.Text = C3DData.Password;
        }
        /*
        private void Register_Click(object sender, EventArgs e)
        {
            if (textBoxID.Text.Length < 1 || textBoxPass.Text.Length < 1 || textBoxName.Text.Length < 1)
            {
                MessageBox.Show("please fill all user informations.");
                return;
            }

            RegisterVerify reg = new RegisterVerify();

            //get userid and encryptkey from remote db
            DBClass db = new DBClass();

            Cursor = Cursors.WaitCursor;

            if (!db.Connect("Data Source = registration.cdtracer.cn; Initial Catalog = DDDSURFER; user id = sa; password = giT26vJLR957QU; Network Library = DBMSSOCN; "))
            {
                MessageBox.Show("Connect remote server failed.\r\n" + db.ErrMsg);
                return;
            }

            string sql = "select * from users where UserID = '" + textBoxID.Text + "';";
            if (!db.SqlDataReader(sql))
            {
                Cursor = Cursors.Default;

                MessageBox.Show("error occurred while reading from remote database.");

                db.Close();

                return;
            }

            if (db.Count < 1 || !db.reader.HasRows)
            {
                Cursor = Cursors.Default;

                MessageBox.Show("error occurred while reading from remote database.");

                db.Close();

                return;
            }

            //读取数据库信息
            List<UserStruct> users = new List<UserStruct>();
            try
            {
                while (db.reader.Read())
                {
                    UserStruct us = new UserStruct();
                    us.UserID = db.reader["UserID"].ToString();
                    us.Password = db.reader["Password"].ToString();
                    us.Keyword = db.reader["Keyword"].ToString();
                    us.RegisterDay = db.reader["RegisterDay"].ToString();
                    us.Group = db.reader["Group"].ToString();
                    us.devices = db.reader["DeviceInfo"].ToString();
                    users.Add(us);
                }
                db.reader.Close();
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;

                MessageBox.Show("error occurred while reading from remote database.");

                db.Close();

                return;
            }

            //验证密码
            UserStruct us0 = users[0];
            RegisterVerify verify = new RegisterVerify();
            string encrptkey = us0.Keyword;
            //加密健解密
            string key0 = verify.GetKeyFromUserID(us0.UserID);
            DESEncrypt des = new DESEncrypt(key0);
            string key1 = des.Decrypt(us0.Keyword);
            DESEncrypt des1 = new DESEncrypt(key1);
            if (des1.Decrypt(us0.Password) != textBoxPass.Text)
            {
                Cursor = Cursors.Default;
                MessageBox.Show("password not correct.");
                db.Close();
                return;
            }

            int maxcopynum = int.Parse(us0.Group);
            if (maxcopynum > C3DData.CopyNum) maxcopynum = C3DData.CopyNum;
            int regnum = 0;
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].devices.Length > 0) regnum++;
            }

            string CPUID = HardWareInfo.GetInfo(HardWareInfo.InfoType.CPUID);
            string DiskID = HardWareInfo.GetInfo(HardWareInfo.InfoType.DiskID);
            string MemoryID = HardWareInfo.GetInfo(HardWareInfo.InfoType.MemoryID);

            //检查是否已注册
            bool exist = false;
            foreach (UserStruct us in users)
            {
                string[] ss = us.devices.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (ss.Length > 2)
                {
                    if (des1.Decrypt(ss[0]) == CPUID &&
                        des1.Decrypt(ss[1]) == DiskID &&
                        des1.Decrypt(ss[2]) == MemoryID)
                    {
                        exist = true;
                        break;
                    }
                }
            }

            if (!exist && users.Count >= copynum)
            {
                Cursor = Cursors.Default;
                MessageBox.Show("registration have reached maximum numers.");
                db.Close();
                return;
            }

            if (!exist)//未注册，写入数据库
            {
                //设备信息加密
                UserStruct usnew = users[0];
                usnew.CPUID = des1.Encrypt(CPUID);
                usnew.DiskID = des1.Encrypt(DiskID);
                usnew.MemoryID = des1.Encrypt(MemoryID);
                if (!db.ExcuteSQL(usnew.toInsertSQLString()))
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("error occurred while writting to remote server.\n" + db.ErrMsg);
                    db.Close();
                    return;
                }
                db.Close();
            }


            Cursor = Cursors.Default;

            if (!reg.CreateLicense(encrptkey))
            {
                MessageBox.Show("registration failed." + reg.ErrMsg);
                return;
            }

            if (!reg.WriteToRegister())
            {
                MessageBox.Show("registration failed." + reg.ErrMsg);
                return;
            }

            MessageBox.Show("Successfully !!! \r\n\nThis product have been registered to " + C3DData.UserID + ".");

            DialogResult = DialogResult.OK;
            this.Close();
        }
        */
        private void Register_Click(object sender, EventArgs e)
        {
            if( textBoxID.Text.Length < 1 || textBoxPass.Text.Length < 1 || textBoxName.Text.Length < 1)
            {
                MessageBox.Show("please fill all user informations.");
                return;
            }

            DBClass db = new DBClass();

            Cursor = Cursors.WaitCursor;

            if ( !db.Connect("Data Source = registration.cdtracer.cn; Initial Catalog = DDDSURFER; user id = sa; password = giT26vJLR957QU; Network Library = DBMSSOCN; ") )
            {
                MessageBox.Show("Connect remote server failed.\r\n" + db.ErrMsg);
                return;
            }

            string sql = "select * from users where UserID = '" + textBoxID.Text + "';";
            if ( !db.SqlDataReader(sql) )
            {
                Cursor = Cursors.Default;

                MessageBox.Show("error occurred while reading from remote database.");
                
                db.Close();
                
                return;
            }

            if ( db.Count < 1 || !db.reader.HasRows )
            {
                Cursor = Cursors.Default;

                MessageBox.Show("error occurred while reading from remote database.");

                db.Close();

                return;
            }

            if ( !db.reader.Read() )
            {
                Cursor = Cursors.Default;

                MessageBox.Show("error occurred while reading from remote database.");

                db.Close();

                return;
            }
            
            UserStruct us = new UserStruct(C3DData.UserID);
            us.Keyword = us.DecryptKey(db.reader["Keyword"].ToString());
            us.Password = us.Decrypt(db.reader["Password"].ToString());
            us.Group = db.reader["Group"].ToString();
            us.RegisterDay = db.reader["RegisterDay"].ToString();
            us.GetDeviceInfoFromEncryptedString(db.reader["DeviceInfo"].ToString());
            db.reader.Close();

            if( us.Password != textBoxPass.Text )
            {
                Cursor = Cursors.Default;
                MessageBox.Show("password not correct.");
                db.Close();
                return;
            }

            string CPUID = HardWareInfo.GetInfo(HardWareInfo.InfoType.CPUID);
            string DiskID = HardWareInfo.GetInfo(HardWareInfo.InfoType.DiskID);
            string MemoryID = HardWareInfo.GetInfo(HardWareInfo.InfoType.MemoryID);

            //检查是否已注册
            bool registered = false;
            if (us.EncryptedDeviceInfo.Length > 3)
            {
                if (    CPUID == us.CPUID && 
                        DiskID == us.DiskID &&
                        MemoryID == us.MemoryID)
                {
                    registered = true;                    
                }
                else
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("This product had been registered to another device.\n" + db.ErrMsg);
                    db.Close();
                    return;
                }
            }

            us.CPUID = CPUID;
            us.DiskID = DiskID;
            us.MemoryID = MemoryID;

            //写入远程数据库
            if (!registered)
            {
                if (!db.ExcuteSQL(us.toUpdateSQLString()))
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("error occurred while writting to remote server.\n" + db.ErrMsg);
                    db.Close();
                    return;
                }
            }

            //验证数据库是否正确
            bool correct = false;
            sql = "select * from users where UserID = '" + textBoxID.Text + "';";
            if (db.SqlDataReader(sql))
            {
                if ( db.reader.HasRows )
                {
                    if (db.reader.Read())
                    {
                        UserStruct us1 = new UserStruct(C3DData.UserID);
                        us1.Keyword = us.Keyword;
                        us1.GetDeviceInfoFromEncryptedString(db.reader["DeviceInfo"].ToString()); 
                        if( us1.CPUID == CPUID && us1.DiskID == DiskID && us1.MemoryID == MemoryID)                        
                            correct = true;
                    }
                }
            }

            db.Close();
            
            if ( !correct )
            {
                Cursor = Cursors.Default;
                MessageBox.Show("writting to remote server failed.\n");                
                return;
            }

            Cursor = Cursors.Default;

            RegisterVerify reg = new RegisterVerify(C3DData.UserID);
            reg.userid = C3DData.UserID;
            if( !reg.CreateLicense(us.EncryptKey(us.Keyword)) )
            {
                MessageBox.Show("registration failed." + reg.ErrMsg);
                return;
            }

            if( !reg.WriteToRegister() )
            {
                MessageBox.Show("registration failed." + reg.ErrMsg);
                return;
            }

            MessageBox.Show("Successfully !!! \r\n\nThis product have been registered to " + C3DData.UserID + "." );

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

    public struct UserStruct
    {
        public string UserID;
        public string Password;
        public string Keyword;
        public string Name;
        public string Group;
        public string Detail;
        public string RegisterDay;
        public string CPUID;
        public string DiskID;
        public string MemoryID;
        public string Reserved1;
        public string Reserved2;
        public string Reserved3;

        public string GetKeyFromUserID()
        {
            string ss = "";
            if (UserID.Length < 8)
            {
                ss = UserID;
                for (int i = 0; i < 8 - UserID.Length; i++) ss += "*";
            }
            else if (UserID.Length > 8) ss = UserID.Substring(0, 8);
            else if (UserID.Length == 8) ss = UserID;
            return ss;
        }

        public string EncryptKey(string line)
        {
            DESEncrypt des = new DESEncrypt(GetKeyFromUserID());
            return des.Encrypt(line);
        }
        public string DecryptKey(string line)
        {
            DESEncrypt des = new DESEncrypt(GetKeyFromUserID());
            return des.Decrypt(line);
        }
        public string Encrypt(string line)
        {
            DESEncrypt des = new DESEncrypt(Keyword);
            return des.Encrypt(line);
        }
        public string Decrypt(string line)
        {
            DESEncrypt des = new DESEncrypt(Keyword);
            return des.Decrypt(line);
        }
        public UserStruct(string _userid)
        {
            UserID = _userid;
            Password = "";
            Keyword = "";
            Name = "";
            Group = "";
            Detail = "";
            RegisterDay = "";
            CPUID = "";
            DiskID = "";
            MemoryID = "";
            Reserved1 = "";
            Reserved2 = "";
            Reserved3 = "";
        }

        public string EncryptedDeviceInfo
        {
            get
            {
                if (CPUID.Length < 1 || DiskID.Length < 1 || MemoryID.Length < 1)
                    return "";
                else return Encrypt(CPUID) + "|" + Encrypt(DiskID) + "|" + Encrypt(MemoryID);
            }
        }
        public string DeviceInfoString
        {
            get
            {
                return CPUID + "|" + DiskID + "|" + MemoryID;
            }
        }
        public bool GetDeviceInfoFromEncryptedString(string encryptedString)
        {
            CPUID = "";
            DiskID = "";
            MemoryID = "";
            try
            {
                if (encryptedString.Length < 1) return false;
                string[] ss = encryptedString.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                if (ss.Length < 3) return false;
                CPUID = Decrypt(ss[0]);
                DiskID = Decrypt(ss[1]);
                MemoryID = Decrypt(ss[2]);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public string toInsertSQLString()
        {
            string sql = "insert into users(UserID,Password,Keyword,Name,[Group],Detail,RegisterDay,DeviceInfo,Reserved1,Reserved2,Reserved3) values(";
            sql += "'" + UserID + "',";
            sql += "'" + Encrypt(Password) + "',";
            sql += "'" + EncryptKey(Keyword) + "',";
            sql += "'" + Name + "',";
            sql += Group + ",";
            sql += "'" + Detail + "',";
            sql += "'" + RegisterDay + "',";
            sql += "'" + EncryptedDeviceInfo + "',";
            sql += "'" + Reserved1 + "',";
            sql += "'" + Reserved2 + "',";
            sql += "'" + Reserved3 + "'";
            sql += ");";
            return sql;
        }
        public string toUpdateSQLString()
        {
            string sql = "update users set ";
            sql += "Keyword = ";
            sql += "'" + EncryptKey(Keyword) + "',";
            sql += "Password = ";
            sql += "'" + Encrypt(Password) + "',";
            sql += "Name = ";
            sql += "'" + Name + "',";
            sql += "[Group] = ";
            sql += Group + ",";
            sql += "Detail = ";
            sql += "'" + Detail + "',";
            sql += "RegisterDay = ";
            sql += "'" + RegisterDay + "',";
            sql += "DeviceInfo = ";
            sql += "'" + EncryptedDeviceInfo + "',";
            sql += "Reserved1 = ";
            sql += "'" + Reserved1 + "',";
            sql += "Reserved2 = ";
            sql += "'" + Reserved2 + "',";
            sql += "Reserved3 = ";
            sql += "'" + Reserved3 + "'";
            sql += " where UserID = ";
            sql += "'" + UserID + "'";
            sql += ";";
            return sql;
        }
        public string toDeleteSQLString()
        {
            string sql = "Delete from users where UserID = ";
            sql += "'" + UserID + "';";
            return sql;
        }
        public string toFormattedLine()
        {
            string line = "";
            line += UserID;
            line += ",";
            line += Password;
            line += ",";
            line += Keyword;
            line += ",";
            line += Name;
            line += ",";
            line += Group;
            line += ",";
            line += Detail;
            line += ",";
            line += RegisterDay;
            line += ",";
            line += DeviceInfoString;
            line += ",";
            line += Reserved1;
            line += ",";
            line += Reserved2;
            line += ",";
            line += Reserved3;
            return line;
        }
    };
}
