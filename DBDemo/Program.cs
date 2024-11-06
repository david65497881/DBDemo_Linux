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
            // SQLite 資料庫連線
            string connectionString = "Data Source=database.db;Version=3;";
            // 報表模板的路徑
            string reportPath = "report.frx";
            // PDF 檔案存放路徑
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "ReportOutput.pdf");

            try
            {
                if (!File.Exists(reportPath))
                {
                    Console.WriteLine("報表文件不存在。請確認 report.frx 存在於專案目錄中。");
                    return;
                }

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // 創建 DataSet 並查詢資料填入 DataTable
                    DataSet dataSet = new DataSet();

                    // 查詢 NonManagerEmployees 資料表
                    DataTable nonManagerTable = new DataTable("NonManagerEmployees");
                    using (var command = new SQLiteCommand("SELECT name, salary, managerId FROM Employees WHERE managerId IS NOT NULL", connection))
                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(nonManagerTable);
                    }
                    dataSet.Tables.Add(nonManagerTable);

                    // 查詢 HigherSalaryEmployees 資料表
                    DataTable higherSalaryTable = new DataTable("HigherSalaryEmployees");
                    using (var command = new SQLiteCommand(@"SELECT e1.name, e1.salary, e1.managerId
                                                             FROM Employees e1
                                                             JOIN Employees e2 ON e1.managerId = e2.id
                                                             WHERE e1.salary > e2.salary", connection))
                    using (var adapter = new SQLiteDataAdapter(command))
                    {
                        adapter.Fill(higherSalaryTable);
                    }
                    dataSet.Tables.Add(higherSalaryTable);

                    using (Report report = new Report())
                    {
                        try
                        {
                            // 加載 report.frx
                            report.Load(reportPath);

                            // 註冊 DataSet 中的所有 DataTable 到報表中
                            report.RegisterData(dataSet, "EmployeesData");
                            report.GetDataSource("NonManagerEmployees").Enabled = true;
                            report.GetDataSource("HigherSalaryEmployees").Enabled = true;

                            // 準備報表
                            report.Prepare();

                            // 匯出報表為 PDF
                            using (PDFSimpleExport pdfExport = new PDFSimpleExport())
                            {
                                report.Export(pdfExport, outputPath);
                                Console.WriteLine($"報表已匯出至 {outputPath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("報表生成或匯出時發生錯誤：" + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("程式執行時發生錯誤：" + ex.Message);
            }
            finally
            {
                Console.WriteLine("程式執行結束。");
            }
            Console.ReadLine();
        }
    }
}
