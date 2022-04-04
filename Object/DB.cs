using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebservicesSage.Object
{
    class DB
    {
        private static SqlConnection cnn;
        private static void Connect()
        {
            string connetionString;
            connetionString = @"Data Source=" + ConfigurationManager.AppSettings["SERVER"].ToString() + ";Initial Catalog=" + ConfigurationManager.AppSettings["DBNAME"].ToString() + ";User ID=" + ConfigurationManager.AppSettings["SQLUSER"].ToString() + ";Password=" + ConfigurationManager.AppSettings["SQLPWD"].ToString();
            cnn = new SqlConnection(connetionString);
            cnn.Open();
        }

        public static void Disconnect()
        {
            cnn.Close();
        }

        public static SqlDataReader Select(string sql)
        {
            Connect();

            SqlCommand command = new SqlCommand(sql, cnn);
            SqlDataReader dataReader = command.ExecuteReader();

            //Disconnect();
            return dataReader;
        }
        

    }
}
