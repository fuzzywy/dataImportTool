using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Configuration;



namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();
            this.MainWindow_Load(null,null);
            
            
        }

        private void SelectDate_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime dt = (DateTime)selectDate.SelectedDate;
            dateBox.Text = dt.ToString("yyyy-MM-dd");
        }



        private void MainWindow_Load(Object sender, EventArgs e)
        {
            DateTime date = DateTime.Now;
            dateBox.Text = date.Date.AddDays(-1).ToString("yyyy-MM-dd");
            selectDate.SelectedDatesChanged += SelectDate_SelectedDatesChanged;
            dir_export.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            host_export.Text = ConfigurationManager.AppSettings["export_host"];
            port_export.Text = ConfigurationManager.AppSettings["export_port"];
            user_export.Text = ConfigurationManager.AppSettings["export_user"];
            pwd_export.Text = ConfigurationManager.AppSettings["export_pwd"];
            database_export.Text = ConfigurationManager.AppSettings["export_database"];

            host_import.Text = ConfigurationManager.AppSettings["import_host"];
            port_import.Text = ConfigurationManager.AppSettings["import_port"];
            user_import.Text = ConfigurationManager.AppSettings["import_user"];
            pwd_import.Text = ConfigurationManager.AppSettings["import_pwd"];
            database_import.Text = ConfigurationManager.AppSettings["import_database"];
            provinceBox.Text = ConfigurationManager.AppSettings["import_province"];
        }

        private void ExportDataBtn_Click(object sender, RoutedEventArgs e)
        {
            string host = host_export.Text;
            string port = port_export.Text;
            string user = user_export.Text;
            string pwd = pwd_export.Text;
            string database = database_export.Text;
            string day_id = dateBox.Text;
            string dir = @dir_export.Text;
            logBox.Document.Blocks.Clear();

            //不能选择今天已经之后的日期
            DateTime today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            DateTime day = Convert.ToDateTime(day_id);
            if (day >= today)
            {
                this.WriteExportLog("日期选择异常", "error");
                MessageBox.Show("请选择今天之前的日期！");
                return;
            }
            //判断是否有这天的数据
            string sql = "select * from B_K_LTE_TDD_HOUR where day_id = '"+day_id+"'";
            string sql1 = "select * from B_K_LTE_FDD_HOUR where day_id = '" + day_id + "'";
            MySQLConn mysql = new MySQLConn();
            MySqlConnection conn = mysql.GetMySqlConn(host, port, user, pwd, database);
            conn.Open();
            try {
                DataTable dt = mysql.QuerySql(sql, conn);
                DataTable dt1 = mysql.QuerySql(sql1, conn);
                if (dt.Rows.Count == 0 && dt1.Rows.Count == 0)
                {
                    this.WriteExportLog("该天无数据", "error");
                    MessageBox.Show("选择日期没有数据，请重新选择");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("发生异常: " + ex.Message, "连接异常", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.WriteExportLog("连接异常", "error");
                return;
            }
            //创建文件夹
            dir += @"\"+day_id;
            Directory.CreateDirectory(dir);
            
            //获取全部表名
            sql = "select table_name from information_schema.tables where table_schema = '"+ database + "' and table_name like 'B_K_%' order by table_name;";
            DataTable allTableNames = mysql.QuerySql(sql, conn);
            foreach(DataRow name in allTableNames.Rows)
            {
                string tableName = name[0].ToString();
                this.WriteExportLog("开始导出表"+tableName, "info");
                //获取日期字段是day还是day_id
                sql = "select COLUMN_NAME from information_schema.COLUMNS where table_name = '"+ tableName + "' and table_schema = '"+ database + "' and COLUMN_NAME like 'day%' ;";
                DataTable dt_day = mysql.QuerySql(sql, conn);
                string day_column = dt_day.Rows[0][0].ToString();
                //创建csv文件
                string fileName = tableName + ".csv";
                MyFile mf = new MyFile();
                FileStream fs = mf.CreateFileStream(dir, fileName);
                StreamWriter writer = new StreamWriter(fs);
                //获取数据
                sql = "select * from " + tableName + " where " + day_column + " = '" + day_id + "'";
                DataTable table = mysql.QuerySql(sql, conn);
                int columnCount = table.Columns.Count;

                string filedRow = "";
                DataColumnCollection columns = table.Columns;
                foreach (DataColumn column in columns)
                {
                    filedRow += column.ColumnName + ",";
                }
                filedRow = filedRow.Substring(0, filedRow.Length - 1);
                writer.WriteLine(filedRow);

                foreach (DataRow values in table.Rows)
                {
                    string oneRowStr = "";
                    for (int i = 0;i < columnCount; i++)
                    {
                        oneRowStr += values[i].ToString()+"," ;
                    }
                    oneRowStr = oneRowStr.Substring(0, oneRowStr.Length - 1);
                    writer.WriteLine(oneRowStr);
                }
                
                this.WriteExportLog("表"+tableName+"导出完成", "info");
                writer.Close();
            }
            conn.Close();
            this.WriteExportLog("执行完成", "info");
            MessageBox.Show("导出数据已经完成！");

            //把连接信息写入配置文件，方便之后使用
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["export_host"].Value = host;
            cfa.AppSettings.Settings["export_port"].Value = port;
            cfa.AppSettings.Settings["export_user"].Value = user;
            cfa.AppSettings.Settings["export_pwd"].Value = pwd;
            cfa.AppSettings.Settings["export_database"].Value = database;
            cfa.Save(ConfigurationSaveMode.Modified);

        }

        private void WriteExportLog(string msg,string infoLevel)
        {
            string now = DateTime.Now.ToString();
            Run r = new Run(infoLevel+"-"+now + ":"+msg);
            Paragraph para = new Paragraph();
            para.Inlines.Add(r);
            logBox.Document.Blocks.Add(para);
        }
        private void WriteImportLog(string msg, string infoLevel)
        {
            string now = DateTime.Now.ToString();
            Run r = new Run(infoLevel + "-" + now + ":" + msg);
            Paragraph para = new Paragraph();
            para.Inlines.Add(r);
            logBox_import.Document.Blocks.Add(para);
        }


        private void OpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = "选择导出csv文件的文件夹";
            folderBrowserDialog.ShowNewFolderButton = false;
            //folderBrowserDialog.RootFolder = Environment.SpecialFolder.Personal;
            folderBrowserDialog.ShowDialog();
            if (folderBrowserDialog.SelectedPath == string.Empty)
            {
                return;
            }
            importFilePath.Text = folderBrowserDialog.SelectedPath;;
        }

        private void ImportDataBtn_Click(object sender, RoutedEventArgs e)
        {
            string host = host_import.Text;
            string port = port_import.Text;
            string user = user_import.Text;
            string pwd = pwd_import.Text;
            string database = database_import.Text;
            string dirPath = importFilePath.Text;
            string province = provinceBox.Text;


            MyFile mf = new MyFile();
            List<FileInfo> fileList = mf.GetFileNamesFromDir(dirPath);
            if (fileList.Count == 0)
            {
                this.WriteImportLog("选择的文件夹没有csv文件，请重新选择", "error");
                MessageBox.Show("选择的文件夹没有csv文件，请重新选择");
                return;
            }
            string[] dirArr = dirPath.Split('\\');
            string day_id = dirArr[dirArr.Length - 1];
            //实例化一个Regex对象
            Regex re = new Regex(@"^^\d{4}-\d{2}-\d{2}$");
            //验证数据是否匹配
            if (re.IsMatch(day_id) == true) {
                this.WriteImportLog("选择的日期为："+day_id, "info");
            } else {
                this.WriteImportLog("选择的文件夹为：" + day_id+",应该选择导出时已日期命名的文件夹", "error");
                MessageBox.Show("选择的文件夹有误，请重新选择");
                return;
            }
            MySQLConn mysql = new MySQLConn();
            MySqlConnection conn = mysql.GetMySqlConn(host, port, user, pwd, database);
            conn.Open();
            this.WriteImportLog("开始导入", "info");
            foreach (FileInfo fl in fileList)
            {
                string tableName = fl.Name.Split('.')[0];
                string filePath = fl.FullName;

                //获取日期字段是day还是day_id
                string sql = "select COLUMN_NAME from information_schema.COLUMNS where table_name = '" + tableName + "' and table_schema = '" + database + "' and COLUMN_NAME like 'day%' ;";
                string day_column = "";
                try
                {
                    DataTable dt_day = mysql.QuerySql(sql, conn);
                    day_column = dt_day.Rows[0][0].ToString();

                }
                catch(Exception ex)
                {
                    MessageBox.Show("发生异常: " + ex.Message, "连接异常", MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.WriteImportLog("连接异常", "error");
                    return;
                }

                //先删除所选日期的数据
                string deleteSql = "";
                if (province == "")
                {
                    deleteSql = "delete from `" + tableName + "` where " + day_column + "='" + day_id + "';";
                } else
                {
                    deleteSql = "delete from `" + tableName + "` where " + day_column + "='" + day_id + "' and province = '"+province+"';";
                }
                
                mysql.ExecuteSql(deleteSql, conn);
                //导入数据
                string loadFileSql = "LOAD DATA LOCAL INFILE  '"+filePath+"' INTO TABLE "+tableName+ " FIELDS TERMINATED BY ',' ENCLOSED BY '\"' LINES TERMINATED BY '\r\n' IGNORE 1 LINES; ";
                loadFileSql = loadFileSql.Replace("\\", "/");
                mysql.ExecuteSql(loadFileSql, conn);
                this.WriteImportLog(tableName+" 导入完成", "info");
            }
            conn.Close();
            this.WriteImportLog("导入结束", "info");
            MessageBox.Show("导入数据已经完成！");

            //把连接信息写入配置文件，方便之后使用
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["import_host"].Value = host;
            cfa.AppSettings.Settings["import_port"].Value = port;
            cfa.AppSettings.Settings["import_user"].Value = user;
            cfa.AppSettings.Settings["import_pwd"].Value = pwd;
            cfa.AppSettings.Settings["import_database"].Value = database;
            cfa.AppSettings.Settings["import_province"].Value = province;
            cfa.Save(ConfigurationSaveMode.Modified);
        }
    }
}
