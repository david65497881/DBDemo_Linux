using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;
using System.IO;

namespace DBDemo
{
    public class Program
    {
        static void Main(string[] args)
        {
            string databasePath = @"C:\Users\user\Desktop\DBDemo\DBDemo\bin\Debug\net6.0\database.db";
            //Version=3表示SQLite版本為3
            string connectionString = "Data Source=database.db;Version=3;";

            // 檢查資料庫檔案是否存在；如果不存在，則建立
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
                Console.WriteLine("資料庫已建立");
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 建立 Employees 資料表
                //SQLite使用動態欄位型態。SQLite的動態欄位型態可以向後相容多數的靜態欄位型態
                //INTEGER會根據數值的大小儲存在1 2 3 4 6 8(byte)裡。跟INT具有親和性
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Employees (
                    id INTEGER PRIMARY KEY,
                    name VARCHAR(100),
                    salary INTEGER,
                    managerId INTEGER
                );";

                connection.Execute(createTableQuery);
            }

            Console.WriteLine("資料庫和資料表建立成功。");
            Console.ReadLine();

        }
    }

}
