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
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string databasePath = Path.Combine(baseDirectory, "database.db");
            string connectionString = $"Data Source={databasePath};Version=3;";
            string reportPath = Path.Combine(baseDirectory, "report.frx");
            string outputPath = Path.Combine(baseDirectory, "ReportOutput.pdf");

            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
                Console.WriteLine("資料庫已建立");
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Employees (
                    id INTEGER PRIMARY KEY,
                    name VARCHAR(100),
                    salary INTEGER,
                    managerId INTEGER
                );";
                connection.Execute(createTableQuery);

                string checkDataQuery = "SELECT COUNT(*) FROM Employees;";
                int rowCount = connection.ExecuteScalar<int>(checkDataQuery);

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


                // 查詢所有managerId != null的員工
                DataTable nonManagersTable = new DataTable("NonManagers");
                nonManagersTable.Columns.Add("name", typeof(string));

                using (var command = new SQLiteCommand(@"
                    SELECT name
                    FROM Employees
                    WHERE managerId IS NOT NULL", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nonManagersTable.Rows.Add(reader["name"]);
                    }
                }
                


                // 查詢⾮管理職且薪資⾼於該主管⼈員
                DataTable higherThanManagersTable = new DataTable("HigherThanManagers");
                higherThanManagersTable.Columns.Add("name", typeof(string));

                using (var command = new SQLiteCommand(@"
                    SELECT e1.name
                    FROM Employees e1
                    WHERE managerId IS NOT NULL
                    AND salary > (
                        SELECT salary FROM Employees e2 WHERE e2.id = e1.managerId
                    )", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        higherThanManagersTable.Rows.Add(reader["name"]);
                    }
                }

                //確認報表文件是否存在
                if (!File.Exists(reportPath))
                {
                    Console.WriteLine("報表文件不存在。請確認 report.frx 存在於專案目錄中。");
                    return;
                }

                using (Report report = new Report())
                {
                    report.Load(reportPath);

                    // 註冊資料表並啟用資料來源
                    report.RegisterData(nonManagersTable, "NonManagers");
                    report.RegisterData(higherThanManagersTable, "HigherThanManagers");


                    report.GetDataSource("NonManagers").Enabled = true;
                    report.GetDataSource("HigherThanManagers").Enabled = true;


                    try
                    {
                        report.Prepare();
                        using (PDFSimpleExport pdfExport = new PDFSimpleExport())
                        {
                            report.Export(pdfExport, outputPath);
                            Console.WriteLine($"報表已匯出至 {outputPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("報表生成或匯出時發生錯誤：" + ex.Message);
                        Console.WriteLine("詳細錯誤：" + ex.StackTrace);
                    }
                }
            }

            Console.WriteLine("程式執行結束。");
            Console.ReadLine();
        }
    }
}
