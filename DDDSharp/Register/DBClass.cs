using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection;
using System.Data;

namespace ADODatabase
{
    public class DBClass
    {
        string connectString;
        public string ErrMsg = "";
        //定义SQL Server连接对象
        SqlConnection connection = null;
        //数据库命令和数据库连接
        SqlDataAdapter dataAdapter = null;
        SqlCommand command = null;
        public SqlDataReader reader = null;
        public DBClass()
        {

        }
        public int Count 
        {
            get 
            {
                if (reader == null) return 0;
                if (!reader.HasRows) return 0;
                return reader.FieldCount;
            }
        }
        public bool Connect(string _connectString = "")
        {
            if(_connectString.Length > 1 ) connectString = _connectString;
            if (connectString.Length < 1) 
            {
                ErrMsg = "connecting String is empty.";
                return false; 
            }
            try 
            {
                if (connection == null) 
                { 
                    connection = new SqlConnection(connectString);
                    connection.Open();                    
                }
                
                command = new SqlCommand();
                command.Connection = connection;

                if ( reader != null ) reader.Close();
                
                return true;
            }
            catch(Exception e)
            {
                Close();
                ErrMsg = e.Message;
                return false;
            }            
        }
        public bool SqlDataReader(string sql)
        {
            try
            {
                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                reader = command.ExecuteReader();
                return true;
            }
            catch (Exception e)
            {
                ErrMsg = e.Message;
                return false;
            }
        }
        public bool ExcuteSQL(string sql)
        {
            try
            {
                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                command.ExecuteNonQuery();
                
                return true;
            }
            catch (Exception e)
            {
                ErrMsg = e.Message;
                return false;
            }
        }
        public bool Close()
        {
            if (reader != null) reader.Close();
            if (command != null) command.Dispose();
            if (connection != null) connection.Close();
            reader = null;
            command = null;
            connection = null;            
            return true;            
        }
    }
}
