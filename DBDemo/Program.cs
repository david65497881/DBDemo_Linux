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
            //Version=3 => SQLite版本為3
            string connectionString = "Data Source=database.db;Version=3;";

            //使用using確保SQLiteConnection在使用後會自動關閉並釋放資源
            //var connection = new SQLiteConnection(connectionString) =>與SQLite資料庫建立連線
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 查詢非管理職員工(managerId != nulla)
                //使用 Dapper 的 Query 方法執行 SQL 查詢並返回結果。Query<string>代表傳回的質會用string格式呈現
                var managerIdEmployees = connection.Query<string>("SELECT name FROM Employees WHERE managerId IS NOT NULL");

                Console.WriteLine("非管理職員工(managerId != NULL)：");
                foreach (var employee in managerIdEmployees)
                {
                    Console.WriteLine(employee);
                }
                Console.ReadLine();
            }

            //重新使用using，每個查詢都需要新的資料庫連接
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 查詢非管理職且薪資高於其主管的員工
                //
                var result = connection.Query<string>(@"
                SELECT e1.name 
                FROM Employees e1
                JOIN Employees e2 ON e1.managerId = e2.id
                WHERE e1.salary > e2.salary
            ");

                Console.WriteLine("非管理職且薪資高於其主管的員工：");
                foreach (var employee in result)
                {
                    Console.WriteLine(employee);
                }
                Console.ReadLine();
            }

        }
    }

}
