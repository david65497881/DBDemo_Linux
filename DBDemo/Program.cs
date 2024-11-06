using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Dapper;
using FastReport;
using FastReport.Export.PdfSimple;

namespace DBDemo
{
    public class Program
    {
        static void Main(string[] args)
        {
            //設定動態路徑。AppDomain.CurrentDomain.BaseDirectory獲取程式執行的根目錄
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string databasePath = Path.Combine(baseDirectory, "database.db");
            string connectionString = $"Data Source={databasePath};Version=3;";
            string reportPath = Path.Combine(baseDirectory, "report.frx");
            string outputPath = Path.Combine(baseDirectory, "ReportOutput.pdf");

            //檢查並建立資料庫
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
                Console.WriteLine("資料庫已建立");
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                //建立資料表（若不存在）
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Employees (
                    id INTEGER PRIMARY KEY,
                    name VARCHAR(100),
                    salary INTEGER,
                    managerId INTEGER
                );";
                connection.Execute(createTableQuery);

                //檢查是否已存在資料
                string checkDataQuery = "SELECT COUNT(*) FROM Employees;";
                int rowCount = connection.ExecuteScalar<int>(checkDataQuery);

                // 如果資料表為空，則插入資料
                if (rowCount == 0)
                {
                    string insertDataQuery = @"
                    INSERT INTO Employees (id, name, salary, managerId) VALUES (1, 'Joe', 70000, 3);
                    INSERT INTO Employees (id, name, salary, managerId) VALUES (2, 'Henry', 80000, 4);
                    INSERT INTO Employees (id, name, salary, managerId) VALUES (3, 'Sam', 60000, NULL);
                    INSERT INTO Employees (id, name, salary, managerId) VALUES (4, 'Max', 90000, NULL);
                    ";
                    connection.Execute(insertDataQuery);
                    Console.WriteLine("資料已插入成功。");
                }
                else
                {
                    Console.WriteLine("資料已存在，跳過插入步驟。");
                }

                // 檢查報表
                if (!File.Exists(reportPath))
                {
                    Console.WriteLine("報表文件不存在。請確認 report.frx 存在於專案目錄中。");
                    return;
                }

                //產生報表PDF
                try
                {
                    DataSet dataSet = new DataSet();

                    // 查詢 Employees 資料表
                    DataTable employeesTable = new DataTable("Employees");
                    using (var command = new SQLiteCommand("SELECT * FROM Employees", connection))
                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(employeesTable);
                    }

                    // 檢查是否成功填充
                    Console.WriteLine($"Employees 表行數: {employeesTable.Rows.Count}");

                    dataSet.Tables.Add(employeesTable);

                    using (Report report = new Report())
                    {
                        report.Load(reportPath);

                        // 註冊資料到報表
                        report.RegisterData(dataSet, "EmployeesData");
                        report.GetDataSource("Employees").Enabled = true;

                        report.Prepare();

                        using (PDFSimpleExport pdfExport = new PDFSimpleExport())
                        {
                            report.Export(pdfExport, outputPath);
                            Console.WriteLine($"報表已匯出至 {outputPath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("報表生成或匯出時發生錯誤：" + ex.Message);
                }


            }

            Console.WriteLine("程式執行結束。");
            Console.ReadLine();
        }
    }
}
