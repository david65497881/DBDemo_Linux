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
                DataTable dataTable = new DataTable("Employees");

                // 修改查詢，將多個人名合併為單一字串，以換行符號分隔
                using (var command = new SQLiteCommand("SELECT name, salary, managerId FROM Employees WHERE managerId IS NOT NULL", connection))

                using (var adapter = new SQLiteDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }

                // 將 DataTable 加入到 DataSet
                dataSet.Tables.Add(dataTable);

                using (Report report = new Report())
                {
                    // 加載 report.frx
                    report.Load(reportPath);

                    // 將資料集註冊到報表
                    report.RegisterData(dataSet, "Employees");

                    // 準備報表
                    report.Prepare();

                    // 匯出報表為 PDF
                    using (PDFSimpleExport pdfExport = new PDFSimpleExport())
                    {
                        report.Export(pdfExport, outputPath);
                        Console.WriteLine($"報表已匯出至 {outputPath}");
                    }
                }
            }
            Console.ReadLine();
        }
    }
}
