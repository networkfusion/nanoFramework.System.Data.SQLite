using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Data.Sqlite;

namespace System.Data.SQLite.Test
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("SQLite Tests!");

            Debug.WriteLine("SQLite RAM DB");
            RunTests();

            //TODO: run tests for file system DB!


            Thread.Sleep(Timeout.Infinite);
        }


        private static void RunTests()
        {

            using (var db = new SqliteCommand())
            {
                Debug.WriteLine("Test 1: Creating Table ...");
                db.ExecuteNonQuery(@"CREATE Table TestData (Var1 TEXT, Var2 INTEGER, Var3 DOUBLE); "); //or should it be FLOAT for Var3 (since nF doesnt support DOUBLE out the box)???

                Debug.WriteLine("Test 2: Insert 1...");
                db.ExecuteNonQuery(@"INSERT INTO TestData (Var1, Var2, Var3) VALUES('Hello, nanoFramework!', 95, 2.34); ");

                Debug.WriteLine("Test 3: Insert2...");
                db.ExecuteNonQuery(@"INSERT INTO TestData (Var1, Var2, Var3) VALUES('Goodbye, nanoFramework!', 25, 9.99); ");

                Debug.WriteLine("Test 4: Query...");
                var result = db.ExecuteQuery(@"SELECT Var1, Var2, Var3 FROM TestData WHERE Var2 > 10;");

                Debug.WriteLine("Test 5: Return column and row count");
                Debug.WriteLine($"columns: {result.ColumnCount} | rows: {result.RowCount}");

                Debug.WriteLine("Test 6: Return column names");
                Debug.Write("| ");
                foreach (var columnName in result.ColumnNames)
                {
                    Debug.Write($"{columnName} |");
                }

                Debug.WriteLine("Test 7: Return column values");
                foreach (ArrayList row in result.Data)
                {
                    Debug.Write("| ");

                    foreach (object value in row)
                    {
                        Debug.Write($"{value} |");
                    }
                    Debug.WriteLine("");
                }
            }
        }
    }
}
