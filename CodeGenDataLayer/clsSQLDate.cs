using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using CodeGenBusinessLayer;
using System.Data.Common;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using System.Configuration;
using System.Text;
using GenerateDataAccessLayerLibrary;

namespace CodeGenDataLayer
{
    public class clsSQLDate
    {
        private static string _TableName;
        private static string _DBName;
        private static string _TableSingleName;

        private static bool _IsLogin;
        private static bool _IsGenerateAllMode;
        private static StringBuilder _TempText;
        private static List<List<clsColumnInfoForDataAccess>> _ColumnsInfo;


        static clsSQLDate()
        {
            _TableName = string.Empty;
            _DBName=string.Empty;
            _TableSingleName = string.Empty;

            _IsLogin = false;
            _IsGenerateAllMode = false;
            _TempText = new StringBuilder();
            _ColumnsInfo=new List<List<clsColumnInfoForDataAccess>>();




        }

        private static bool _DoesTableHaveColumn(string ColumnName)
        {
            foreach (var row in _ColumnsInfo)
            {
                // Check if the row is not empty
                if (row.Count > 0)
                {
                    // Access the first item in the row and compare its ColumnName (case-insensitive)
                    if (row[0].ColumnName.Equals(ColumnName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool _DoesTableHaveUsernameAndPassword()
        {
            return (_DoesTableHaveColumn("username") && _DoesTableHaveColumn("password"));
        }

        private static String _GetTableName()
        {
            
            if(_ColumnsInfo.Count > 0)
            {
                String FirstValue = _ColumnsInfo[0][0].ColumnName;
                return FirstValue.Remove(FirstValue.Length - 2);
            }
            return "";
        }       

        public static string GenerateDataLayer(List<List<clsColumnInfoForDataAccess>> ColumnsInfo, string DBName)
        {
          _TempText.Clear();
          _DBName = DBName;
          _ColumnsInfo = ColumnsInfo;          
          _TableSingleName = _GetTableName();
            //Get Table Name;

            if (!_IsGenerateAllMode)
            {
                _IsLogin=_DoesTableHaveUsernameAndPassword();   
            }
    _TempText.AppendLine($"using System;\r\n" +
                $"using System.Data;\r\nusing " +
                $"System.Data.SqlClient;\r\n\r\nnamespace {_DBName}DataAccessLayer\r\n{{");

            _TempText.Append($"public class cls{_TableSingleName}Data");

            _TempText.AppendLine();
            _TempText.AppendLine("{");





             

            //if(_IsLogin)
            //{

            //}


            _TempText.Append("}");
            _TempText.Append("\n}");

            return _TempText.ToString();
        }
       
        public static DataTable GetAllDatabaseNames()
        {
            DataTable databaseNames = new DataTable();

            // Create a SqlConnection object with the connection string
            using (SqlConnection connection = new SqlConnection(clsConnectionString.ConnectionString))
            {
                try
                {
                    connection.Open();

                    // Query to retrieve database names
                    string query = "SELECT name AS DatabaseName FROM " +
                        "sys.databases WHERE database_id > 4"; // Exclude system databases

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(databaseNames);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            return databaseNames;
        }
      
        public static DataTable GetTablesNameByDB(string databaseName)
        {
            DataTable tableNames = new DataTable();

            // Create a SqlConnection object with the connection string
            using (SqlConnection connection = new SqlConnection(clsConnectionString.ConnectionString))
            {
                try
                {
                    connection.Open();

                    // Query to retrieve table names for the specified database
                    string query = "SELECT name FROM " + databaseName + ".sys.tables"; // Query tables in the specified database

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(tableNames);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    Console.WriteLine("Error: " + ex.Message);
                }
            }

            return tableNames;
        }
       
        public static DataTable GetTableInfo(string tableName, string dbName)
        {
            DataTable tableInfo = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsConnectionString.ConnectionString))
                {
                    connection.Open();

                    string query = $@"
    USE {dbName};

    SELECT 
        COLUMN_NAME AS [Column Name],
        DATA_TYPE AS [Data Type],
        IS_NULLABLE AS [Is Null]
    FROM 
        INFORMATION_SCHEMA.COLUMNS
    WHERE 
        TABLE_NAME = '{tableName}';";


                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(tableInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or display the exception message for debugging
                Console.WriteLine("Error: " + ex.Message);
            }

            return tableInfo;
        }
       
        public static string GenerateDataAccessSettings(string DBName)
        {
            return $@"using System.Configuration;

namespace {DBName}DataAccess
{{
    static class clsDataAccessSettings
    {{
        public static string ConnectionString =
            ConfigurationManager.ConnectionStrings[""ConnectionString""].ConnectionString;
    }}
}}";
        }


    }
}

