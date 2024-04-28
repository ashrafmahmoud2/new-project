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
using System.Runtime.InteropServices;

namespace CodeGenDataLayer
{
    public class clsSQLDate
    {
        private static string _tableName;
        private static string _dbName;
        private static  string _tableSingleName;
        private static bool _isLogin;
        private static  bool _isGenerateAllMode;
        private static StringBuilder _tempText;
        private static List<List<clsColumnInfoForDataAccess>> _columnsInfo;

        public clsSQLDate()
        {
            _tableName = string.Empty;
            _dbName = string.Empty;
            _tableSingleName = string.Empty;
            _isLogin = false;
            _isGenerateAllMode = false;
            _tempText = new StringBuilder();
            _columnsInfo = new List<List<clsColumnInfoForDataAccess>>();
        }

        private static bool _DoesTableHaveColumn(string columnName)
        {
            foreach (var row in _columnsInfo)
            {
                if (row.Count > 0 && row[0].ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool _DoesTableHaveUsernameAndPassword()
        {
            return (_DoesTableHaveColumn("username") && _DoesTableHaveColumn("password"));
        }

        private static string _GetTableName()
        {
            if (_columnsInfo.Count > 0)
            {
                string firstValue = _columnsInfo[0][0].ColumnName;
                return firstValue.Remove(firstValue.Length - 2);
            }
            return string.Empty;
        }

        private static string _GetParametersByTableColumns()
        {
            StringBuilder parametersBuilder = new StringBuilder();

            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    string columnName = columnInfo.ColumnName;
                    string dataType = columnInfo.DataType;

                    if (parametersBuilder.Length != 0)
                    {
                        parametersBuilder.Append(", ");
                    }

                    parametersBuilder.Append($"{dataType} {columnName}");
                }
            }

            return parametersBuilder.ToString();
        }

        private static string _GetConnectionString()
        {
            return "SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);";
        }

        private static string _GetParametersExecuteReaderInMode()
        {
            StringBuilder parametersBuilder = new StringBuilder();

            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    parametersBuilder.Append($"{columnInfo.ColumnName} = " +
                        $"({columnInfo.DataType})reader[\"{columnInfo.ColumnName}\"], \n");
                }
            }

            if (parametersBuilder.Length > 0)
            {
                parametersBuilder.Length -= 2;
            }

            return parametersBuilder.ToString();
        }

        private  static string _GenerateGetInfoMethodByID()
        {
            //add name spacd , ref ,delete the first line;
            _tempText.Clear();
            _tempText.AppendLine($"public static bool Get{_GetTableName()}InfoByID({_GetParametersByTableColumns()})");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    bool IsFound = false;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"   {_GetConnectionString()}");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = @\"select * from {_GetTableName()}" +
                $" where {_GetTableName()}ID = @{_GetTableName()}ID\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    command.Parameters.AddWithValue(\"@{_GetTableName()}ID\"," +
                $" {_GetTableName()}ID);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    try");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Open();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        SqlDataReader reader = command.ExecuteReader();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        if (reader.Read())");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            // The record was found");
            _tempText.AppendLine("            IsFound = true;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"            {_GetParametersExecuteReaderInMode()}");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("        else");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            // The record was not found");
            _tempText.AppendLine("            IsFound = false;");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("");
            _tempText.AppendLine("        reader.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    catch (Exception ex)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        IsFound = false;");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    finally");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("");
            _tempText.AppendLine("    return IsFound;");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static string _DataAccessAsLoginInfo()
        {
           _GenerateGetInfoMethodByID();
        //_CreateGetInfoMethodForUsername();
        //_CreateGetInfoMethodForUsernameAndPassword();
        //_CreateAddMethod();
        //_CreateUpdateMethod();
        //_CreateDeleteMethod();
        //_CreateExistsMethod();
        //_CreateExistsMethodForUsername();
        //_CreateExistsMethodForUsernameAndPassword();
        //_CreateAllMethod();

            return _tempText.ToString();
        }

        public static string GenerateDataLayer(List<List<clsColumnInfoForDataAccess>> columnsInfo, string dbName)
        {
            // Initialize StringBuilder and other variables
            _tempText = new StringBuilder();
            _dbName = dbName;
            _columnsInfo = columnsInfo;
            _tableSingleName = _GetTableName();

            // Append using directives
            _tempText.AppendLine("using System;");
            _tempText.AppendLine("using System.Data;");
            _tempText.AppendLine("using System.Data.SqlClient;\r\n");

            // Check if dbName is valid for namespace declaration
           
                // Append namespace declaration
                _tempText.AppendLine($"namespace {_dbName}DataAccessLayer");
                _tempText.AppendLine("{");
            

            _tempText.AppendLine($"    public class cls{_tableSingleName}Data");
            _tempText.AppendLine("    {");

            // Check for additional conditions
            if (!_isGenerateAllMode)
            {
                _isLogin = _DoesTableHaveUsernameAndPassword();
            }

            // Call method to generate GetInfo method
            _GenerateGetInfoMethodByID();

            // Close class and namespace declarations if applicable
            _tempText.AppendLine("    }");
            if (!string.IsNullOrEmpty(_dbName))
            {
                _tempText.AppendLine("}");
            }

            // Return generated code as string
            return _tempText.ToString();
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

