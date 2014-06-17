using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace CheckBannedIP
{
    class DataClass
    {
        public DataClass()
        {
            _mdbPath = Application.StartupPath + "\\Data\\Data.mdb";
            _conn = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + _mdbPath + ";Jet OLEDB:Database Password=Gong#731;");
            _conn.Open();
        }

        private string _mdbPath;
        private OleDbConnection _conn;

        public List<TAccount> GetAccountlist()
        {
            List<TAccount> accountlist = new List<TAccount>();

            OleDbCommand sql = _conn.CreateCommand();
            sql.CommandText = "SELECT * FROM Account";
            OleDbDataReader ret = sql.ExecuteReader();

            while (ret.Read())
            {
                TAccount account = new TAccount();
                account.code = ret["code"].ToString();
                account.host = ret["host"].ToString();
                account.refer = ret["refer"].ToString();
                account.thread = (int)ret["thread"];
                accountlist.Add(account);
            }

            ret.Close();
            return accountlist;
        }

        public void AddTestData(int thread, String code)
        {
            OleDbCommand sql = _conn.CreateCommand();
            sql.CommandText = "INSERT INTO Test (thread, code) VALUES (" + thread.ToString() + ",'"
                                + code.Substring(0, 200)
                                + "')";
            sql.ExecuteNonQuery();
        }
    }
}
