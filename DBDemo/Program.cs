using System;
using System.Data.SQLite;
using Dapper;
using System.IO;
using FastReport;
using FastReport.Export.PdfSimple;
using System.Text;
using System.Linq;

namespace DBDemo
{
    public class Program
    {
        static void Main(string[] args)
        {
            //SQLite資料庫連線
            string connectionString = "Data Source=database.db;Version=3;";
            //報表模板的路徑
            string reportPath = "report.frx";
            //PDF檔案存放路徑
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "ReportOutput.pdf");

            if (!File.Exists(reportPath))
            {
                Console.WriteLine("報表文件不存在。請確認 report.frx 存在於專案目錄中。");
                return;
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                var managerIdEmployees = connection.Query("SELECT name FROM Employees WHERE managerId IS NOT NULL").ToList();
                var higherSalaryEmployees = connection.Query(@"
                    SELECT e1.name 
                    FROM Employees e1
                    JOIN Employees e2 ON e1.managerId = e2.id
                    WHERE e1.salary > e2.salary").ToList();


                //使用stringbuilder是因為它在處理大量字串連接時效率較高
                //ApendLine代表會自動換行
                StringBuilder nonManagerEmployeesText = new StringBuilder();
                foreach (var employee in managerIdEmployees)
                {
                    nonManagerEmployeesText.AppendLine(employee.name);
                }

                StringBuilder higherSalaryEmployeesText = new StringBuilder();
                foreach (var employee in higherSalaryEmployees)
                {
                    higherSalaryEmployeesText.AppendLine(employee.name);
                }

                //using => 確保使用後釋放資源
                using (Report report = new Report())
                {
                    //加載report.frx
                    report.Load(reportPath);

                    // 找到 TextObject 並設置資料。使用FindObject尋找名為Text3的物件，由於傳回來的物件會是通用物件，因此加上FastReport.TextObject進行轉換
                    var nonManagerTextObject = report.FindObject("Text3") as FastReport.TextObject;
                    //確保物件確實存在於report.frx
                    if (nonManagerTextObject != null)
                    {
                        nonManagerTextObject.Text = nonManagerEmployeesText.ToString();
                    }

                    var higherSalaryTextObject = report.FindObject("Text4") as FastReport.TextObject;
                    if (higherSalaryTextObject != null)
                    {
                        higherSalaryTextObject.Text = higherSalaryEmployeesText.ToString();
                    }

                    //準備報表，這個方法會計算所有表達式和數據綁定，並將報表的頁面渲染到內存中。
                    report.Prepare();

                    //using 確保試用後釋放資源
                    //PDFSimpleExport是FastReport提供的一個類別，用於將報表輸出為PDF格式
                    using (PDFSimpleExport pdfExport = new PDFSimpleExport())
                    {
                        //將PDF存放到指定的路徑 => outputpath
                        report.Export(pdfExport, outputPath);
                        Console.WriteLine($"報表已匯出至 {outputPath}");
                    }
                }
            }
            Console.ReadLine();
        }
    }
}
