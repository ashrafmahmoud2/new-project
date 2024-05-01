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
using System.Linq;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;
using System.IO;
using System.Configuration.Provider;
using System.Xml;

namespace CodeGenDataLayer
{
    public class clsSQLDate
    {

        //stop in conction sql with vs to make stored prosduers;


        //Ask
        //why you don't add table name in stored prosucers;


        //there are some fun in khaled abouts files  
        //take a copy of dvld db in git hup
        //add stored prosugrues in data layer like khaled by lambad and ()=>  clsGenerateDelegateHelperMethods.delete;
        //you change name missinon in  Log Handler  and log error ;
        //handel out put formating;


        private static bool _isLogin;
        private static string _dbName;
        private static string _tableName;
        private static string _tableSingleName;
        private static bool _isGenerateAllMode;
        private static StringBuilder _tempText;
        private static StringBuilder _parametersBuilder;
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

        //public static bool DoesTableHaveColumn(string columnName)
        //{
        //    foreach (var row in _columnsInfo)
        //    {
        //        if (row.Count > 0 && row[0].ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //public static bool DoesTableHaveUsernameAndPassword()
        //{
        //    return (DoesTableHaveColumn("username") && DoesTableHaveColumn("password"));
        //}

        public static string GetTableName()
        {
            if (_columnsInfo.Count > 0)
            {
                string firstValue = _columnsInfo[0][0].ColumnName;
                return firstValue.Remove(firstValue.Length - 2);
            }
            return string.Empty;
        }

        public static string _GetParametersByTableColumns(bool WithRef = false)
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

                    if (WithRef)
                    {
                        parametersBuilder.Append($"ref {dataType} {columnName}");
                    }
                    else
                    {
                        parametersBuilder.Append($"{dataType} {columnName}");
                    }
                }
            }

            if (WithRef)
            {
                // Remove the first word from the parameters
                int index = parametersBuilder.ToString().IndexOf(' ');
                if (index != -1)
                {
                    parametersBuilder.Remove(0, index + 1);
                }
            }

            return parametersBuilder.ToString();
        }



        //General Mathods
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
        public static List<string> GetTablesNameByDBByList(string databaseName)
        {
            List<string> tableNames = new List<string>();

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
                        // Create a DataSet and a DataTable
                        DataSet dataSet = new DataSet();
                        DataTable dataTable = new DataTable();

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            // Fill the DataTable with the data retrieved from the database
                            adapter.Fill(dataTable);
                        }

                        // Extract table names from the DataTable and add them to the list
                        foreach (DataRow row in dataTable.Rows)
                        {
                            tableNames.Add(row["name"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    Console.WriteLine("Error: " + ex.Message);
                    return new List<string>(); // Return an empty list if an exception occurs
                }
            }

            return tableNames;
        }

        public static List<List<clsColumnInfoForDataAccess>> GetTableInfoInList(string tableName, string dbName)
        {
            List<List<clsColumnInfoForDataAccess>> _columnsInfoForDataAccess = new List<List<clsColumnInfoForDataAccess>>();

            try
            {
                using (SqlConnection connection = new SqlConnection(clsConnectionString.ConnectionString))
                {
                    connection.Open();

                    string query = $@"
                        USE {dbName};
                        SELECT 
                            COLUMN_NAME AS [ColumnName],
                            DATA_TYPE AS [DataType],
                            CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS [IsNullable]
                        FROM 
                            INFORMATION_SCHEMA.COLUMNS
                        WHERE 
                            TABLE_NAME = '{tableName}';";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Loop through the result set
                            while (reader.Read())
                            {
                                // Create an instance of clsColumnInfoForDataAccess
                                clsColumnInfoForDataAccess columnInfo = new clsColumnInfoForDataAccess
                                {
                                    ColumnName = reader["ColumnName"].ToString(),
                                    DataType = reader["DataType"].ToString(),
                                    IsNullable = Convert.ToBoolean(reader["IsNullable"])
                                };

                                // Add clsColumnInfoForDataAccess instance to the list
                                List<clsColumnInfoForDataAccess> columnInfos = new List<clsColumnInfoForDataAccess> { columnInfo };
                                _columnsInfoForDataAccess.Add(columnInfos);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or display the exception message for debugging
                Console.WriteLine("Error: " + ex.Message);
            }

            return _columnsInfoForDataAccess;
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

        public static bool GenerateFile(string TextFile, string filePath, string tableName)
        {
            try
            {
                // Generate file path
                string fileFullPath = Path.Combine(filePath, tableName + ".cs");

                // Write code text to file
                File.WriteAllText(fileFullPath, TextFile);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating file for table {tableName}: {ex.Message}");
                return false;
            }
        }



        // Generate Error Logger -  Log Handler
        public static string GenerateErrorLogger(string DBName)
        {

            _tempText = new StringBuilder();
            _tempText.AppendLine("using System;");
            _tempText.AppendLine();
            _tempText.AppendLine($"namespace {DBName}DataAccess");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    public class clsErrorLogger");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        private Action<string, Exception> _logAction;");
            _tempText.AppendLine();
            _tempText.AppendLine("        public clsErrorLogger(Action<string, Exception> logAction)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            _logAction = logAction;");
            _tempText.AppendLine("        }");
            _tempText.AppendLine();
            _tempText.AppendLine("        public void LogError(string errorType, Exception ex)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            _logAction?.Invoke(errorType, ex);");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        public static string GenerateLogHandler(string DBName)
        {
            _tempText = new StringBuilder();

            _tempText.AppendLine("using System;");
            _tempText.AppendLine("using System.Configuration;");
            _tempText.AppendLine("using System.Diagnostics;");
            _tempText.AppendLine();
            _tempText.AppendLine($"namespace {DBName}DataAccess");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    public class clsLogHandler");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        public static void LogToEventViewer(string errorType, Exception ex)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            string sourceName = ConfigurationManager.AppSettings[\"ProjectName\"];");
            _tempText.AppendLine("            if (!EventLog.SourceExists(sourceName))");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                EventLog.CreateEventSource(sourceName, \"Application\");");
            _tempText.AppendLine("            }");
            _tempText.AppendLine();
            _tempText.AppendLine("            string errorMessage = $\"{errorType} in {ex.Source}\\n\\nException Message: {ex.Message}\\n\\nException Type: {ex.GetType().Name}\\n\\nStack Trace: {ex.StackTrace}\\n\\nException Location: {ex.TargetSite}\";");
            _tempText.AppendLine();
            _tempText.AppendLine("            EventLog.WriteEntry(sourceName, errorMessage, EventLogEntryType.Error);");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");

            // Append the generated log handler text to the existing _tempText
            return _tempText.ToString();


        }



        //Generate Appconfig InFilePath    
        public static bool GenerateAppconfigInFilePath(string filePath, string dbName)
        {

            string text = AppconfigText(dbName);
            string[] lines = File.ReadAllLines(filePath);
            bool updated = false;

            using (StreamWriter writer = new StreamWriter(filePath.Trim()))
            {
                writer.Write(text);
                updated = true;
            }



            return updated;
        }

        private static string AppconfigText(string DbName)
        {
            StringBuilder _tempText = new StringBuilder();

            _tempText.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            _tempText.AppendLine("<configuration>");
            _tempText.AppendLine();
            _tempText.AppendLine("\t<startup>");
            _tempText.AppendLine("\t\t<supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v4.8\" />");
            _tempText.AppendLine("\t</startup>");
            _tempText.AppendLine();
            _tempText.AppendLine("\t<appSettings>");
            _tempText.AppendLine($"\t\t<add key=\"ProjectName\" value=\"{DbName}\" />");
            _tempText.AppendLine("\t</appSettings>");
            _tempText.AppendLine();
            _tempText.AppendLine("\t<connectionStrings>");
            _tempText.AppendLine($"\t\t<add name=\"ConnectionString\" connectionString=\"Server=.;Database={DbName};Integrated Security=True;\" providerName=\"System.Data.SqlClient\" />");
            _tempText.AppendLine("\t</connectionStrings>");
            _tempText.AppendLine();
            _tempText.AppendLine("</configuration>");

            return _tempText.ToString();
        }


    }
}

