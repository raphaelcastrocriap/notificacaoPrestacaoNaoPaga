using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SalasZoomNotificationFormadores.connects
{
    class connectHT
    {
        private string DBhost;
        private string DBname;
        private string DBuser;
        private string DBpass;
        public static SqlConnection Connection;
        public static string ConnectionString = null;

        public connectHT(string host, string dbname, string user, string pass)
        {
            DBhost = host;
            DBname = dbname;
            DBuser = user;
            DBpass = pass;

            ConnectionString = "Data Source='" + DBhost + "'; Initial Catalog='" + DBname + "'; User Id='" + DBuser + "'; Password='" + DBpass + "'; Trusted_Connection=False";

            Connection = new SqlConnection();
            Connection.ConnectionString = ConnectionString;
        }

        public Boolean TestConnect()
        {
            try
            {
                Connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                Connection.Close();
            }
        }

        public void ConnInit()
        {
            Connection.Close();
            Connection.Open();
        }

        public void ConnEnd()
        {
            Connection.Close();
        }

        public SqlConnection Conn { get { return Connection; } }
    }

    class connectHT2
    {
        private string DBhost;
        private string DBname;
        private string DBuser;
        private string DBpass;
        public static SqlConnection Connection;
        public static string ConnectionString = null;

        public connectHT2(string host, string dbname, string user, string pass)
        {
            DBhost = host;
            DBname = dbname;
            DBuser = user;
            DBpass = pass;

            ConnectionString = "Data Source='" + DBhost + "'; Initial Catalog='" + DBname + "'; User Id='" + DBuser + "'; Password='" + DBpass + "'; Trusted_Connection=False";

            Connection = new SqlConnection();
            Connection.ConnectionString = ConnectionString;
        }

        public Boolean TestConnect()
        {
            try
            {
                Connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                Connection.Close();
            }
        }

        public void ConnInit()
        {
            Connection.Close();
            Connection.Open();
        }

        public void ConnEnd()
        {
            Connection.Close();
        }

        public SqlConnection Conn { get { return Connection; } }
    }
}

