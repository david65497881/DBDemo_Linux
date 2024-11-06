using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using Dapper;
using System.Text;
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

            // 使用 using 確保使用後釋放資源
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 創建 DataSet 並查詢資料填入 DataTable
                DataSet dataSet = new DataSet();
                DataTable dataTable = new DataTable("Employees");

                // 修改查詢，將多個人名合併為單一字串，以換行符號分隔
                using (var command = new SQLiteCommand("SELECT group_concat(name, '\n') as name, salary, managerId FROM Employees GROUP BY managerId", connection))
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
            //PDF檔案存放路徑。Directory.GetCurrentDirectory() =>獲取當前專案運行時的工作目錄路徑
            //Path.Combine => 將Directory.GetCurrentDirectory() 跟 ReportOutput.pdf 結合產生完整路徑
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "ReportOutput.pdf");

            if (!File.Exists(reportPath))
            {
                Console.WriteLine("報表文件不存在。請確認 report.frx 存在於專案目錄中。");
                return;
            }

            //使用using確保使用後釋放資源
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                //使用Dapper的Query()方法查詢資料
                var managerIdEmployees = connection.Query("SELECT name FROM Employees WHERE managerId IS NOT NULL").ToList();
                //使用@(逐字字串字面值)來避免使用換行符號
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
