using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace WpfApp1
{
    class MySQLConn
    {

        public MySqlConnection GetMySqlConn(string host,string port,string user,string pwd,string database)
        {
            string mySqlConn = "Max Pool Size = 512;Database=" + database+";Data Source="+host+";User Id="+user+";Password="+pwd+";pooling=false;CharSet=utf8;port="+port;
            MySqlConnection conn = new MySqlConnection(mySqlConn);
            return conn;
            
        }

        public DataTable QuerySql(string sql, MySqlConnection conn)
        {
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.CommandType = System.Data.CommandType.Text;
            DataTable dt = new DataTable();
            MySqlDataAdapter msda = new MySqlDataAdapter(cmd);
            msda.Fill(dt);
            conn.Close();
            return dt;
        }

        //public void QuerySqlFile(string filePath, MySqlConnection conn)
        //{
        //    FileInfo file = new FileInfo(filePath);  //filename是sql脚本文件路径。
        //    string sql = file.OpenText().ReadToEnd();
        //    conn.Open();
        //    MySqlScript script = new MySqlScript(conn);
        //    script.Query = sql;
        //    script.Execute();
        //
        //    conn.Close();
        //}
        public void QuerySqlFile(string filePath, MySqlConnection conn)
        {
            conn.Open();
            MySqlScript script = new MySqlScript(conn);
            foreach (string sql in File.ReadAllLines(filePath))
            {
                script.Query = sql;
                script.Execute();
            }
            conn.Close();
        }

        public void ExecuteSql(string sql, MySqlConnection conn)
        {
            conn.Open();
            MySqlScript script = new MySqlScript(conn);
            script.Query = sql;
            script.Execute();
            conn.Close();
        }

    }
}
