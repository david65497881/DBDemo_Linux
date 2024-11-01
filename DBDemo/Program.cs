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
            string connectionString = "Data Source=database.db;Version=3;";



            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 插入資料。使用SQLite的INSERT OR REPLACE語法來覆蓋資料
                string insertDataQuery = @"
                INSERT OR REPLACE INTO Employees (id, name, salary, managerId) VALUES (1, 'Joe', 70000, 3);
                INSERT OR REPLACE INTO Employees (id, name, salary, managerId) VALUES (2, 'Henry', 80000, 4);
                INSERT OR REPLACE INTO Employees (id, name, salary, managerId) VALUES (3, 'Sam', 60000, NULL);
                INSERT OR REPLACE INTO Employees (id, name, salary, managerId) VALUES (4, 'Max', 90000, NULL);
            ";

                connection.Execute(insertDataQuery);

                Console.WriteLine("資料插入成功。");
                Console.ReadLine();
            }

        }
    }

}
