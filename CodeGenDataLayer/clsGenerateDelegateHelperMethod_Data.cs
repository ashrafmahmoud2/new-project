using GenerateDataAccessLayerLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenDataLayer
{
    public static class clsGenerateDelegateHelperMethod_Data
    {

        private static bool _isLogin;
        private static string _dbName;
        private static string _tableName;
        private static string _tableSingleName;
        private static bool _isGenerateAllMode;
        private static StringBuilder _tempText;
        private static StringBuilder _parametersBuilder;
        private static List<List<clsColumnInfoForDataAccess>> _columnsInfo;

        public static string GenerateDelegateHelperMethods(string databaseName)
        {
            _tempText = new StringBuilder();

            // Add necessary using directives
            _tempText.AppendLine("using System;");
            _tempText.AppendLine("using System.Data;");
            _tempText.AppendLine("using System.Collections.Generic;");
            _tempText.AppendLine("using System.Data.SqlClient;");
            _tempText.AppendLine();

            // Add namespace and class declaration
            _tempText.AppendLine($"namespace {databaseName}DataAccess");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    public static class clsDelegateHelperMethods");
            _tempText.AppendLine("    {");

            // Add methods
            _CreateHandleExceptionMethodHelper();
            _tempText.AppendLine();
            _CreateDeleteMethodHelper();
            _tempText.AppendLine();
            _CreateExistsMethodWithOneParameterHelper();
            _tempText.AppendLine();
            _CreateExistsMethodWithTwoParametersHelper();
            _tempText.AppendLine();
            _CreateCountMethodHelper();
            _tempText.AppendLine();
            _CreateRetrieveAllMethodHelper();
            _tempText.AppendLine();
            _CreateRetrieveAllWithParameterMethodHelper();
            _tempText.AppendLine();
            _CreateRetrieveAllWithTwoParametersMethodHelper();
            _tempText.AppendLine();
            _CreateRetrieveAllWithDictionaryParameterMethodHelper();
            _tempText.AppendLine();
            _CreateRetrieveAllInPagesMethodHelper();

            // Close class and namespace
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static void _CreateHandleExceptionMethodHelper()
        {
            _tempText.AppendLine("        public static void HandleDatabaseException(Exception ex)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            if (ex is SqlException sqlEx)");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                var errorLogger = new ErrorLogger(LogHandler.LogToEventViewer);");
            _tempText.AppendLine("                errorLogger.LogDatabaseError(sqlEx);");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            else");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                var errorLogger = new ErrorLogger(LogHandler.LogToEventViewer);");
            _tempText.AppendLine("                errorLogger.LogGeneralError(ex);");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("        }");
        }

        private static void _CreateDeleteMethodHelper()
        {
            _tempText.AppendLine("        public static bool DeleteRecord<T>(string storedProcedureName, string parameterName, T value)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            int rowsAffected = 0;");
            _tempText.AppendLine();
            _tempText.AppendLine("            try");
            _tempText.AppendLine("            {");
            _tempText.AppendLine($"                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString))");
            _tempText.AppendLine("                {");
            _tempText.AppendLine("                    connection.Open();");
            _tempText.AppendLine();
            _tempText.AppendLine("                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))");
            _tempText.AppendLine("                    {");
            _tempText.AppendLine("                        command.CommandType = CommandType.StoredProcedure;");
            _tempText.AppendLine($"                        command.Parameters.AddWithValue(\"@parameterName\", (object)value ?? DBNull.Value);");
            _tempText.AppendLine();
            _tempText.AppendLine("                        rowsAffected = command.ExecuteNonQuery();");
            _tempText.AppendLine("                    }");
            _tempText.AppendLine("                }");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            catch (Exception ex)");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                HandleDatabaseException(ex);");
            _tempText.AppendLine("            }");
            _tempText.AppendLine();
            _tempText.AppendLine("            return rowsAffected > 0;");
            _tempText.AppendLine("        }");
        }

        private static void _CreateExistsMethodWithOneParameterHelper()
        {
            _tempText.AppendLine("        public static bool Exists<T>(string storedProcedureName, string parameterName, T value)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            bool isFound = false;");
            _tempText.AppendLine();
            _tempText.AppendLine("            try");
            _tempText.AppendLine("            {");
            _tempText.AppendLine($"                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            _tempText.AppendLine("                {");
            _tempText.AppendLine("                    connection.Open();");
            _tempText.AppendLine();
            _tempText.AppendLine("                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))");
            _tempText.AppendLine("                    {");
            _tempText.AppendLine("                        command.CommandType = CommandType.StoredProcedure;");
            _tempText.AppendLine($"                        command.Parameters.AddWithValue(\"@parameterName\", (object)value ?? DBNull.Value);");
            _tempText.AppendLine();
            _tempText.AppendLine("                        var returnParameter = new SqlParameter(\"@ReturnVal\", SqlDbType.Int)");
            _tempText.AppendLine("                        {");
            _tempText.AppendLine("                            Direction = ParameterDirection.ReturnValue");
            _tempText.AppendLine("                        };");
            _tempText.AppendLine("                        command.Parameters.Add(returnParameter);");
            _tempText.AppendLine();
            _tempText.AppendLine("                        command.ExecuteNonQuery();");
            _tempText.AppendLine();
            _tempText.AppendLine("                        isFound = (int)returnParameter.Value == 1;");
            _tempText.AppendLine("                    }");
            _tempText.AppendLine("                }");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            catch (Exception ex)");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                isFound = false;");
            _tempText.AppendLine("                clsDataAccessHelper.HandleException(ex);");
            _tempText.AppendLine("            }");
            _tempText.AppendLine();
            _tempText.AppendLine("            return isFound;");
            _tempText.AppendLine("        }");
        }

        private static void _CreateExistsMethodWithTwoParametersHelper()
        {
            _tempText.AppendLine("        public static bool Exists<T1, T2>(string storedProcedureName, string parameterName1, T1 value1, string parameterName2, T2 value2)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            bool isFound = false;");
            _tempText.AppendLine();
            _tempText.AppendLine("            try");
            _tempText.AppendLine("            {");
            _tempText.AppendLine($"                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            _tempText.AppendLine("                {");
            _tempText.AppendLine("                    connection.Open();");
            _tempText.AppendLine();
            _tempText.AppendLine("                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))");
            _tempText.AppendLine("                    {");
            _tempText.AppendLine("                        command.CommandType = CommandType.StoredProcedure;");
            _tempText.AppendLine($"                        command.Parameters.AddWithValue(\"@parameterName1\", (object)value1 ?? DBNull.Value);");
            _tempText.AppendLine($"                        command.Parameters.AddWithValue(\"@parameterName2\", (object)value2 ?? DBNull.Value);");
            _tempText.AppendLine();
            _tempText.AppendLine("                        var returnParameter = new SqlParameter(\"@ReturnVal\", SqlDbType.Int)");
            _tempText.AppendLine("                        {");
            _tempText.AppendLine("                            Direction = ParameterDirection.ReturnValue");
            _tempText.AppendLine("                        };");
            _tempText.AppendLine("                        command.Parameters.Add(returnParameter);");
            _tempText.AppendLine();
            _tempText.AppendLine("                        command.ExecuteNonQuery();");
            _tempText.AppendLine();
            _tempText.AppendLine("                        isFound = (int)returnParameter.Value == 1;");
            _tempText.AppendLine("                    }");
            _tempText.AppendLine("                }");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            catch (Exception ex)");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                isFound = false;");
            _tempText.AppendLine("                clsDataAccessHelper.HandleException(ex);");
            _tempText.AppendLine("            }");
            _tempText.AppendLine();
            _tempText.AppendLine("            return isFound;");
            _tempText.AppendLine("        }");
        }

        private static void _CreateCountMethodHelper()
        {
            _tempText.AppendLine("        public static int Count(string storedProcedureName)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            int count = 0;");
            _tempText.AppendLine();
            _tempText.AppendLine("            try");
            _tempText.AppendLine("            {");
            _tempText.AppendLine($"                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            _tempText.AppendLine("                {");
            _tempText.AppendLine("                    connection.Open();");
            _tempText.AppendLine();
            _tempText.AppendLine("                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))");
            _tempText.AppendLine("                    {");
            _tempText.AppendLine("                        command.CommandType = CommandType.StoredProcedure;");
            _tempText.AppendLine();
            _tempText.AppendLine("                        object result = command.ExecuteScalar();");
            _tempText.AppendLine();
            _tempText.AppendLine("                        if (result != null && int.TryParse(result.ToString(), out int value))");
            _tempText.AppendLine("                        {");
            _tempText.AppendLine("                            count = value;");
            _tempText.AppendLine("                        }");
            _tempText.AppendLine("                    }");
            _tempText.AppendLine("                }");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            catch (Exception ex)");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                clsDataAccessHelper.HandleException(ex);");
            _tempText.AppendLine("            }");
            _tempText.AppendLine();
            _tempText.AppendLine("            return count;");
            _tempText.AppendLine("        }");
        }

        private static void _CreateRetrieveAllMethodHelper()
        {
            _tempText.AppendLine("        public static DataTable All(string storedProcedureName)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            DataTable dt = new DataTable();");
            _tempText.AppendLine();
            _tempText.AppendLine("            try");
            _tempText.AppendLine("            {");
            _tempText.AppendLine($"                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            _tempText.AppendLine("                {");
            _tempText.AppendLine("                    connection.Open();");
            _tempText.AppendLine();
            _tempText.AppendLine("                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))");
            _tempText.AppendLine("                    {");
            _tempText.AppendLine("                        command.CommandType = CommandType.StoredProcedure;");
            _tempText.AppendLine();
            _tempText.AppendLine("                        using (SqlDataReader reader = command.ExecuteReader())");
            _tempText.AppendLine("                        {");
            _tempText.AppendLine("                            if (reader.HasRows)");
            _tempText.AppendLine("                            {");
            _tempText.AppendLine("                                dt.Load(reader);");
            _tempText.AppendLine("                            }");
            _tempText.AppendLine("                        }");
            _tempText.AppendLine("                    }");
            _tempText.AppendLine("                }");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            catch (Exception ex)");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                clsDataAccessHelper.HandleException(ex);");
            _tempText.AppendLine("            }");
            _tempText.AppendLine();
            _tempText.AppendLine("            return dt;");
            _tempText.AppendLine("        }");
        }

        private static void _CreateRetrieveAllWithParameterMethodHelper()
        {
            _tempText.AppendLine("        public static DataTable All<T>(string storedProcedureName, string parameterName, T value)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            DataTable dt = new DataTable();");
            _tempText.AppendLine();
            _tempText.AppendLine("            try");
            _tempText.AppendLine("            {");
            _tempText.AppendLine($"                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            _tempText.AppendLine("                {");
            _tempText.AppendLine("                    connection.Open();");
            _tempText.AppendLine();
            _tempText.AppendLine("                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))");
            _tempText.AppendLine("                    {");
            _tempText.AppendLine("                        command.CommandType = CommandType.StoredProcedure;");
            _tempText.AppendLine($"                        command.Parameters.AddWithValue(\"@parameterName\", (object)value ?? DBNull.Value);");
            _tempText.AppendLine();
            _tempText.AppendLine("                        using (SqlDataReader reader = command.ExecuteReader())");
            _tempText.AppendLine("                        {");
            _tempText.AppendLine("                            if (reader.HasRows)");
            _tempText.AppendLine("                            {");
            _tempText.AppendLine("                                dt.Load(reader);");
            _tempText.AppendLine("                            }");
            _tempText.AppendLine("                        }");
            _tempText.AppendLine("                    }");
            _tempText.AppendLine("                }");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            catch (Exception ex)");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                clsDataAccessHelper.HandleException(ex);");
            _tempText.AppendLine("            }");
            _tempText.AppendLine();
            _tempText.AppendLine("            return dt;");
            _tempText.AppendLine("        }");
        }

        private static void _CreateRetrieveAllWithTwoParametersMethodHelper()
        {
            _tempText.AppendLine("        public static DataTable All<T1, T2>(string storedProcedureName, string parameterName1, T1 value1, string parameterName2, T2 value2)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            DataTable dt = new DataTable();");
            _tempText.AppendLine();
            _tempText.AppendLine("            try");
            _tempText.AppendLine("            {");
            _tempText.AppendLine($"                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            _tempText.AppendLine("                {");
            _tempText.AppendLine("                    connection.Open();");
            _tempText.AppendLine();
            _tempText.AppendLine("                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))");
            _tempText.AppendLine("                    {");
            _tempText.AppendLine("                        command.CommandType = CommandType.StoredProcedure;");
            _tempText.AppendLine($"                        command.Parameters.AddWithValue(\"@parameterName1\", (object)value1 ?? DBNull.Value);");
            _tempText.AppendLine($"                        command.Parameters.AddWithValue(\"@parameterName2\", (object)value2 ?? DBNull.Value);");
            _tempText.AppendLine();
            _tempText.AppendLine("                        using (SqlDataReader reader = command.ExecuteReader())");
            _tempText.AppendLine("                        {");
            _tempText.AppendLine("                            if (reader.HasRows)");
            _tempText.AppendLine("                            {");
            _tempText.AppendLine("                                dt.Load(reader);");
            _tempText.AppendLine("                            }");
            _tempText.AppendLine("                        }");
            _tempText.AppendLine("                    }");
            _tempText.AppendLine("                }");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            catch (Exception ex)");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                clsDataAccessHelper.HandleException(ex);");
            _tempText.AppendLine("            }");
            _tempText.AppendLine();
            _tempText.AppendLine("            return dt;");
            _tempText.AppendLine("        }");
        }

        private static void _CreateRetrieveAllWithDictionaryParameterMethodHelper()
        {
            _tempText.AppendLine("        public static DataTable All<T>(string storedProcedureName, Dictionary<string, T> parameters)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            DataTable dt = new DataTable();");
            _tempText.AppendLine();
            _tempText.AppendLine("            try");
            _tempText.AppendLine("            {");
            _tempText.AppendLine($"                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            _tempText.AppendLine("                {");
            _tempText.AppendLine("                    connection.Open();");
            _tempText.AppendLine();
            _tempText.AppendLine("                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))");
            _tempText.AppendLine("                    {");
            _tempText.AppendLine("                        command.CommandType = CommandType.StoredProcedure;");
            _tempText.AppendLine();
            _tempText.AppendLine("                        // Add parameters from the dictionary");
            _tempText.AppendLine("                        if (parameters != null)");
            _tempText.AppendLine("                        {");
            _tempText.AppendLine("                            foreach (var parameter in parameters)");
            _tempText.AppendLine("                            {");
            _tempText.AppendLine("                                command.Parameters.AddWithValue($\"@{parameter.Key}\", (object)parameter.Value ?? DBNull.Value);");
            _tempText.AppendLine("                            }");
            _tempText.AppendLine("                        }");
            _tempText.AppendLine();
            _tempText.AppendLine("                        using (SqlDataReader reader = command.ExecuteReader())");
            _tempText.AppendLine("                        {");
            _tempText.AppendLine("                            if (reader.HasRows)");
            _tempText.AppendLine("                            {");
            _tempText.AppendLine("                                dt.Load(reader);");
            _tempText.AppendLine("                            }");
            _tempText.AppendLine("                        }");
            _tempText.AppendLine("                    }");
            _tempText.AppendLine("                }");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            catch (Exception ex)");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                clsDataAccessHelper.HandleException(ex);");
            _tempText.AppendLine("            }");
            _tempText.AppendLine();
            _tempText.AppendLine("            return dt;");
            _tempText.AppendLine("        }");
        }

        private static void _CreateRetrieveAllInPagesMethodHelper()
        {
            _tempText.AppendLine("        public static DataTable AllInPages(short pageNumber, int rowsPerPage, string storedProcedureName)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            DataTable dt = new DataTable();");
            _tempText.AppendLine();
            _tempText.AppendLine("            try");
            _tempText.AppendLine("            {");
            _tempText.AppendLine($"                using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))");
            _tempText.AppendLine("                {");
            _tempText.AppendLine("                    connection.Open();");
            _tempText.AppendLine();
            _tempText.AppendLine("                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))");
            _tempText.AppendLine("                    {");
            _tempText.AppendLine("                        command.CommandType = CommandType.StoredProcedure;");
            _tempText.AppendLine();
            _tempText.AppendLine("                        // Add paging parameters to the command");
            _tempText.AppendLine("                        command.Parameters.AddWithValue(\"@PageNumber\", pageNumber);");
            _tempText.AppendLine("                        command.Parameters.AddWithValue(\"@RowsPerPage\", rowsPerPage);");
            _tempText.AppendLine();
            _tempText.AppendLine("                        using (SqlDataReader reader = command.ExecuteReader())");
            _tempText.AppendLine("                        {");
            _tempText.AppendLine("                            if (reader.HasRows)");
            _tempText.AppendLine("                            {");
            _tempText.AppendLine("                                dt.Load(reader);");
            _tempText.AppendLine("                            }");
            _tempText.AppendLine("                        }");
            _tempText.AppendLine("                    }");
            _tempText.AppendLine("                }");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            catch (Exception ex)");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                clsDataAccessHelper.HandleException(ex);");
            _tempText.AppendLine("            }");
            _tempText.AppendLine();
            _tempText.AppendLine("            return dt;");
            _tempText.AppendLine("        }");
        }


    }
}
