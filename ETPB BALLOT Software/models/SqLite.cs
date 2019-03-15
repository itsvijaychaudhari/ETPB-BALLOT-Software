using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETPB_BALLOT_Software.models
{
    public class SqLite
    {
        public static SQLiteConnection OpenSQLLiteConnection(SQLiteConnection sqlite_conn)
        {
            sqlite_conn = new SQLiteConnection("Data Source=" + Directory.GetCurrentDirectory() + "\\DataBase\\RVOTE.db;Version=3;foreign keys=true;New=false;Compress=True;");
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }

            return sqlite_conn;
        }

        public static void CloseSQLLiteConnection(SQLiteConnection sqlite_conn)
        {
            sqlite_conn.Close();
            GC.Collect();
        }
    }
}
