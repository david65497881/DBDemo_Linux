using System;
using System.Data.SQLite;

namespace SQLiteViewCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            // SQLite 資料庫連線字串
            string connectionString = "Data Source=database.db;Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 建立 NonManagerEmployees 視圖的 SQL 語句
                string createNonManagerView = @"
                    CREATE VIEW IF NOT EXISTS NonManagerEmployees AS
                    SELECT name, salary, managerId FROM Employees WHERE managerId IS NOT NULL;
                ";

                // 建立 HigherSalaryEmployees 視圖的 SQL 語句
                string createHigherSalaryView = @"
                    CREATE VIEW IF NOT EXISTS HigherSalaryEmployees AS
                    SELECT e1.name, e1.salary, e1.managerId 
                    FROM Employees e1
                    JOIN Employees e2 ON e1.managerId = e2.id
                    WHERE e1.salary > e2.salary;
                ";

                // 使用 SQLiteCommand 執行 SQL 語句來創建視圖
                using (var command = new SQLiteCommand(createNonManagerView, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("視圖 NonManagerEmployees 已建立或已存在。");
                }

                using (var command = new SQLiteCommand(createHigherSalaryView, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("視圖 HigherSalaryEmployees 已建立或已存在。");
                }

                connection.Close();
            }

            Console.WriteLine("所有視圖已創建完成。");
        }
    }
}
