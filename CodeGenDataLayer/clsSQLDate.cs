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

namespace CodeGenDataLayer
{
    public class clsSQLDate
    {

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

        private static string _GetParametersByTableColumns(bool WithRef = false)
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

        private static string _GetConnectionString()
        {
            return "SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);";
        }

        private static string _GetParametersExecuteReader()
        {
            _parametersBuilder = new StringBuilder();
            foreach (var columnList in _columnsInfo)
            {

                foreach (var columnInfo in columnList)
                {
                    // Skip the first column

                    _parametersBuilder.Append($"{columnInfo.ColumnName} = " +
                           $"({columnInfo.DataType})reader[\"{columnInfo.ColumnName}\"]; \n");

                }
            }

            if (_parametersBuilder.Length > 0)
            {
                _parametersBuilder.Length -= 2;
            }


            return _parametersBuilder.ToString();
        }

        private static string _GetAddWithValueParameters()
        {
            _parametersBuilder.Clear();
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    _parametersBuilder.Append($"command.Parameters.AddWithValue(\"@{columnInfo.ColumnName}\", " +
                        $"{columnInfo.ColumnName} ?? DBNull.Value);\n");
                }
            }

            return _parametersBuilder.ToString();
        }

        private static string _GetQueryOfInsert()
        {
            string columnNames = "";
            string parameters = "";

            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    // Append column name to columnNames
                    columnNames += $"{columnInfo.ColumnName}, ";

                    // Append parameter placeholder to parameters
                    parameters += $"@{columnInfo.ColumnName}, ";
                }
            }

            columnNames = columnNames.TrimEnd(',', ' ');
            parameters = parameters.TrimEnd(',', ' ');

            string insertQuery = $"INSERT INTO {_GetTableName()} ({columnNames}) VALUES ({parameters}); SELECT SCOPE_IDENTITY();";

            return insertQuery;
        }

        private static string _GetQueryOfUpdate()
        {
            string setClause = "";

            // Iterate over _columnsInfo to build the SET clause
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    // Append column name and parameter placeholder to the SET clause
                    setClause += $"{columnInfo.ColumnName} = @{columnInfo.ColumnName}, ";
                }
            }

            // Remove the trailing comma and space from setClause
            setClause = setClause.TrimEnd(',', ' ');

            // Construct the UPDATE query
            string updateQuery = $"UPDATE {_GetTableName()} SET {setClause} WHERE {_GetTableName()}ID = @{_GetTableName()}ID;";

            return updateQuery;
        }




        //_Generate Methods & DataLayer
        public static string GenerateDataLayer(List<List<clsColumnInfoForDataAccess>> columnsInfo, string dbName)
        {
            _tempText = new StringBuilder();
            _dbName = dbName;
            _columnsInfo = columnsInfo;
            _tableSingleName = _GetTableName();

            _tempText.AppendLine($"using System;\r\n" +
               $"using System.Data;\r\nusing " +
               $"System.Data.SqlClient;\r\n\r\nnamespace {_dbName}DataAccess\r\n{{");

            _tempText.Append($"public class cls{_tableSingleName}Data");
            _tempText.AppendLine();
            _tempText.AppendLine("{");

            // Check for additional conditions
            if (!_isGenerateAllMode)
            {
                _isLogin = _DoesTableHaveUsernameAndPassword();
            }


            if (_isLogin)
            {
                _GenerateDataLayerAsLoginInfo();
            }
            else
                _GenerateDataLayerAsNorumal();




            // Close class and namespace declarations if applicable
            _tempText.AppendLine("    }");
            if (!string.IsNullOrEmpty(_dbName))
            {
                _tempText.AppendLine("}");
            }

            // Return generated code as string
            return _tempText.ToString();
        }

        private static string _GenerateDataLayerAsLoginInfo()
        {
            _GenerateGetInfoMethodByID();
            _GenerateAddMethod();
            _GenerateUpdateMethod();
            _GenerateDeleteMethod();
            _GenerateGetAllMethod();
            _GenerateExistsMethod();
            _GenerateGetInfoMethodForUsername();
            _GenerateGetInfoMethodForUsernameAndPassword();
            _GenerateExistsMethodForUsername();
            _GenerateExistsMethodForUsernameAndPassword();


            return _tempText.ToString();
        }

        private static string _GenerateDataLayerAsNorumal()
        {
            _GenerateGetInfoMethodByID();
            _GenerateAddMethod();
            _GenerateUpdateMethod();
            _GenerateDeleteMethod();
            _GenerateGetAllMethod();
            _GenerateExistsMethod();
            //_GenerateGetInfoMethodForUsername();
            //_GenerateGetInfoMethodForUsernameAndPassword();
            //_GenerateExistsMethodForUsername();
            //_GenerateExistsMethodForUsernameAndPassword();


            return _tempText.ToString();
        }

        private static string _GenerateGetInfoMethodByID()
        {
            _tempText.AppendLine($"public static bool Get{_GetTableName()}InfoByID({_GetParametersByTableColumns(true)})");
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
            _tempText.AppendLine($"            {_GetParametersExecuteReader()}");
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

        private static string _GenerateAddMethod()
        {


            _tempText.AppendLine($"public static int AddNew{_GetTableName()}({_GetParametersByTableColumns()}) ");
            _tempText.AppendLine($"{{    int {_GetTableName()}ID = -1;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {_GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"string query = \"{_GetQueryOfInsert()}\";");

            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine($"  {_GetAddWithValueParameters()};");
            _tempText.AppendLine("");
            _tempText.AppendLine("    try");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Open();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        object result = command.ExecuteScalar();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        if (result != null && int.TryParse(result.ToString(), out int InsertID))");
            _tempText.AppendLine("        {");
            _tempText.AppendLine($"            {_GetTableName()}ID = InsertID;");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    catch (Exception ex)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        Console.WriteLine($\"Error: {ex.Message}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    finally");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    return {_GetTableName()}ID;");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static string _GenerateUpdateMethod()
        {

            _tempText.AppendLine($"public static bool Update{_GetTableName()}( {_GetParametersByTableColumns()})");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    int RowAffected = 0;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {_GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"{_GetQueryOfUpdate()}\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {_GetAddWithValueParameters()};");
            _tempText.AppendLine("");
            _tempText.AppendLine("    try");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Open();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        RowAffected = command.ExecuteNonQuery();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    catch (Exception ex)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        Console.WriteLine($\"Error: {ex.Message}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    finally");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("");
            _tempText.AppendLine("    return (RowAffected > 0);");
            _tempText.AppendLine("}");
            return _tempText.ToString();


            return _tempText.ToString();
        }

        private static string _GenerateDeleteMethod()
        {
            _tempText.AppendLine($"public static bool Delete{_GetTableName()}(int {_GetTableName()}ID)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    int RowAffected = 0;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {_GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"DELETE FROM {_GetTableName()} WHERE {_GetTableName()}ID = @{_GetTableName()}ID\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    command.Parameters.AddWithValue(\"@{_GetTableName()}ID\", {_GetTableName()}ID);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    try");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Open();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        RowAffected = command.ExecuteNonQuery();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    catch (Exception ex)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        Console.WriteLine($\"Error: {ex.Message}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    finally");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("");
            _tempText.AppendLine("    return (RowAffected > 0);");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static string _GenerateExistsMethod()
        {
            _tempText.AppendLine($"public static bool IS{_GetTableName()}Exists(int {_GetTableName()}ID)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    bool exists = false;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {_GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT found=1 FROM {_GetTableName()} WHERE {_GetTableName()}ID = @{_GetTableName()}ID\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    command.Parameters.AddWithValue(\"@{_GetTableName()}ID\", {_GetTableName()}ID);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    try");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Open();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        int count = (int)command.ExecuteScalar();");
            _tempText.AppendLine("        exists = count > 0;");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    catch (Exception ex)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        Console.WriteLine($\"Error: {ex.Message}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    finally");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("");
            _tempText.AppendLine("    return exists;");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static string _GenerateGetAllMethod()
        {
            _tempText.AppendLine($"public static DataTable GetAll{_GetTableName()}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    DataTable dt = new DataTable();");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {_GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT * FROM {_GetTableName()}_view\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    try");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Open();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        SqlDataReader reader = command.ExecuteReader();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        if (reader.HasRows)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            dt.Load(reader);");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("");
            _tempText.AppendLine("        reader.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    catch (Exception ex)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        Console.WriteLine($\"Error: {ex.Message}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    finally");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("");
            _tempText.AppendLine("    return dt;");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static string _GenerateGetInfoMethodForUsername()
        {
            _tempText.AppendLine($"public static DataTable GetInfo{_GetTableName()}ForUsername(string username)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    DataTable dt = new DataTable();");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {_GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT * FROM {_GetTableName()}_view WHERE Username = @Username\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    command.Parameters.AddWithValue(\"@Username\", username);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    try");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Open();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        SqlDataReader reader = command.ExecuteReader();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        if (reader.HasRows)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            dt.Load(reader);");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("");
            _tempText.AppendLine("        reader.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    catch (Exception ex)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        Console.WriteLine($\"Error: {ex.Message}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    finally");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("");
            _tempText.AppendLine("    return dt;");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static string _GenerateGetInfoMethodForUsernameAndPassword()
        {
            _tempText.AppendLine($"public static DataTable GetInfo{_GetTableName()}ForUsernameAndPassword(string username, string password)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    DataTable dt = new DataTable();");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {_GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT * FROM {_GetTableName()}_view WHERE Username = @Username AND Password = @Password\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    command.Parameters.AddWithValue(\"@Username\", username);");
            _tempText.AppendLine("    command.Parameters.AddWithValue(\"@Password\", password);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    try");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Open();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        SqlDataReader reader = command.ExecuteReader();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        if (reader.HasRows)");
            _tempText.AppendLine("        {");
            _tempText.AppendLine("            dt.Load(reader);");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("");
            _tempText.AppendLine("        reader.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    catch (Exception ex)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        Console.WriteLine($\"Error: {ex.Message}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    finally");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("");
            _tempText.AppendLine("    return dt;");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static string _GenerateExistsMethodForUsername()
        {
            _tempText.AppendLine($"public static bool Exists{_GetTableName()}ForUsername(string username)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    bool exists = false;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {_GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT Found=1 FROM {_GetTableName()}_view WHERE Username = @Username\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    command.Parameters.AddWithValue(\"@Username\", username);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    try");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Open();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        int count = (int)command.ExecuteScalar();");
            _tempText.AppendLine("        exists = count > 0;");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    catch (Exception ex)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        Console.WriteLine($\"Error: {ex.Message}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    finally");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("");
            _tempText.AppendLine("    return exists;");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static string _GenerateExistsMethodForUsernameAndPassword()
        {
            _tempText.AppendLine($"public static bool Exists{_GetTableName()}ForUsernameAndPassword(string username, string password)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    bool exists = false;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {_GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT Found=1 FROM {_GetTableName()}_view WHERE Username = @Username AND Password = @Password\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    command.Parameters.AddWithValue(\"@Username\", username);");
            _tempText.AppendLine("    command.Parameters.AddWithValue(\"@Password\", password);");
            _tempText.AppendLine("");
            _tempText.AppendLine("    try");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Open();");
            _tempText.AppendLine("");
            _tempText.AppendLine("        int count = (int)command.ExecuteScalar();");
            _tempText.AppendLine("        exists = count > 0;");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    catch (Exception ex)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        Console.WriteLine($\"Error: {ex.Message}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    finally");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        connection.Close();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("");
            _tempText.AppendLine("    return exists;");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }





        //_Generate Methods & BusinessLayer
        public static string GenerateBusinessLayer(List<List<clsColumnInfoForDataAccess>> columnsInfo, string dbName)
        {
            _tempText = new StringBuilder();
            _dbName = dbName;
            _columnsInfo = columnsInfo;
            _tableSingleName = _GetTableName();

            _tempText.AppendLine($"using System;\r\n" +
               $"using System.Data;\r\nusing " +
               $"System.Data.SqlClient;\r\n\r\nnamespace {_dbName}BusinessLayer\r\n{{");

            _tempText.Append($"public class cls{_tableSingleName}");
            _tempText.AppendLine();
            _tempText.AppendLine("{");

            // Check for additional conditions
            if (!_isGenerateAllMode)
            {
                _isLogin = _DoesTableHaveUsernameAndPassword();
            }

            if (_isLogin)
            {
                _GenerateBusinessLayerAsLoginInfo();
            }
            else
            {
                _GenerateBusinessLayerAsNormal();
            }

            // Close class and namespace declarations if applicable
            _tempText.AppendLine("}");
            if (!string.IsNullOrEmpty(_dbName))
            {
                _tempText.AppendLine("}");
            }

            // Return generated code as string
            return _tempText.ToString();
        }

        private static string _GenerateBusinessLayerAsLoginInfo()
        {
            _GenerateEnumMode();
            _GenerateProperties();
            _GenerateConstructor();
           _GenerateAdd_Update_Save_MethodInBusinessLayer();
           _GenerateDeleteMethodInBusinessLayer();
            _GenerateGetAllMethodInBusinessLayer();
            _GenerateFindMethodInBusinessLayer();
            _GenerateGetInfoMethodForUsernameMethodInBusinessLayer();
            _GenerateGetInfoMethodForUsernameAndPasswordMethodInBusinessLayer();
            _GenerateExistsMethodForUsernameMethodInBusinessLayer();
            _GenerateExistsMethodForUsernameAndPasswordInBusinessLayer();
            _GenerateTestsFunctions();
            return _tempText.ToString();
        }

        private static string _GenerateBusinessLayerAsNormal()
        {
            _GenerateEnumMode();
            _GenerateProperties();
            _GenerateConstructor();
            _GenerateAdd_Update_Save_MethodInBusinessLayer();
            _GenerateDeleteMethodInBusinessLayer();
            _GenerateGetAllMethodInBusinessLayer();
            _GenerateFindMethodInBusinessLayer();
            _GenerateTestsFunctions();
            return _tempText.ToString();


            return _tempText.ToString();
        }

        private static string _GetParmterForAddUpdateBusinesslayer(bool AddMode = true)
        {
            StringBuilder parameters = new StringBuilder();
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    parameters.AppendLine($"this.{columnInfo.ColumnName},");
                   
                }
            
            }
            int lastIndex = parameters.ToString().LastIndexOf(',');
            parameters.Remove(lastIndex, 1);
            // If it's not Add mode, remove the first parameter
            if (!AddMode)
            {
                int startIndex = parameters.ToString().IndexOf("ref");
                if (startIndex != -1)
                {
                    parameters.Remove(startIndex, 3); // Removing "ref" and the following space
                }

            }

            return parameters.ToString();
        }

        private static string _GenerateEnumMode()
        {
            _tempText.AppendLine("public enum enMode { AddNew = 0, Update = 1 };");
            _tempText.AppendLine("public enMode Mode { get; set; } = enMode.AddNew;");

            return _tempText.ToString();
        }

        private static void _GenerateProperties()
        {
            // Generate properties based on column information
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    _tempText.AppendLine($"public {columnInfo.DataType} {columnInfo.ColumnName} {{ get; set; }}");
                }
            }
        }

        private static void _GenerateConstructor()
        {
            StringBuilder parameters = new StringBuilder();
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    parameters.Append($"{columnInfo.DataType} {columnInfo.ColumnName},");

                }

            }
            int lastIndex = parameters.ToString().LastIndexOf(',');
            parameters.Remove(lastIndex, 1);


            // Generate constructor
            _tempText.AppendLine($"public cls{_tableSingleName}()");
            _tempText.AppendLine("{");
            // Initialize properties with default values
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    _tempText.AppendLine($"this.{columnInfo.ColumnName}" +$" = {GetDefaultValueForType(columnInfo.DataType)};");
                }
            }
            _tempText.AppendLine("  this.Mode = enMode.AddNew;");
            _tempText.AppendLine("}");



            _tempText.AppendLine($"private  cls{_tableSingleName}({parameters.ToString()})");
            _tempText.AppendLine("{");
            // Initialize properties with default values
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    _tempText.AppendLine($"this.{columnInfo.ColumnName}={columnInfo.ColumnName};");
                }
            }
            _tempText.AppendLine("  this.Mode = enMode.Update;");
            _tempText.AppendLine("}");
        }

        private static string GetDefaultValueForType(string type)
        {
            // Return default value based on type
            switch (type.ToLower())
            {
                case "string":
                    return "\"\"";
                case "int":
                case "long":
                case "double":
                case "float":
                    return "0";
                case "datetime":
                    return "DateTime.Now";
                case "bool":
                    return "false";
                case "char":
                 return "0";
                default:
                    return "null";
            }
        }

        private static string _GenerateAdd_Update_Save_MethodInBusinessLayer()
        {
            
            string addParameters = _GetParmterForAddUpdateBusinesslayer(true);
            string updateParameters = _GetParmterForAddUpdateBusinesslayer(false);

            _tempText.AppendLine($"public bool _AddNew{_GetTableName()}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    this.{_GetTableName()}ID =" +
                $" cls{_GetTableName()}Data.AddNew{_GetTableName()}({addParameters});");
            _tempText.AppendLine($"    return (this.{_GetTableName()}ID != -1);");
            _tempText.AppendLine("}");
            _tempText.AppendLine();


            _tempText.AppendLine($"private bool _Update{_GetTableName()}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    return cls{_GetTableName()}Data.Update{_GetTableName()}({updateParameters});");
            _tempText.AppendLine("}");
            _tempText.AppendLine();


            _tempText.AppendLine("public bool Save()");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    switch (Mode)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        case enMode.AddNew:");
            _tempText.AppendLine($"            if (_AddNew{_GetTableName()}())");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                Mode = enMode.Update;");
            _tempText.AppendLine("                return true;");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            else");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                return false;");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("        case enMode.Update:");
            _tempText.AppendLine($"            return _Update{_GetTableName()}();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    return false;");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static string _GenerateDeleteMethodInBusinessLayer()
        {
            _tempText.AppendLine($"public static bool Delete{_GetTableName()}(int {_GetTableName()}ID)");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    return cls{_GetTableName()}Data.Delete{_GetTableName()}({_GetTableName()}ID);");
            _tempText.AppendLine("}");
            return _tempText.ToString();
        }          

        private static string _GenerateGetAllMethodInBusinessLayer()
        {
            _tempText.AppendLine($"public static DataTable GetAll{_GetTableName()}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    return cls{_GetTableName()}Data.GetAll{_GetTableName()}();");
            _tempText.AppendLine("}");
            return _tempText.ToString();
        }

        private static string _GenerateExistsMethodForUsernameMethodInBusinessLayer()
        {
            _tempText.AppendLine($"public static bool Does{_GetTableName()}Exist(string Username)");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    return cls{_GetTableName()}Data.Does{_GetTableName()}Exist(Username);");
            _tempText.AppendLine("}");
            return _tempText.ToString();
        }

        private static string _GenerateExistsMethodForUsernameAndPasswordInBusinessLayer()
        {
            _tempText.AppendLine($"public static bool Does{_GetTableName()}Exist(string Username, string Password)");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    return cls{_GetTableName()}Data.Does{_GetTableName()}Exist(Username, Password);");
            _tempText.AppendLine("}");
            return _tempText.ToString();
        }

        private static string _GenerateFindMethodInBusinessLayer()
        {
            StringBuilder m = new StringBuilder();

            // Generate property assignments
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    m.AppendLine($"  {(columnInfo.DataType)} {columnInfo.ColumnName} = {GetDefaultValueForType((columnInfo.DataType))};");
                }
            }

            StringBuilder m2 = new StringBuilder();

            // Generate parameter list with 'ref' keyword only for the first parameter
            bool Addref = true;
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    if (Addref)
                    {
                        m2.AppendLine($"ref {columnInfo.ColumnName},");
                       // Addref = false;
                    }
                    else
                    {
                        m2.AppendLine($" {columnInfo.ColumnName},");
                    }
                }
            }

            // Remove the last comma if it exists
            int lastIndex = m2.ToString().LastIndexOf(',');
            if (lastIndex != -1)
            {
                m2.Remove(lastIndex, 1);
            }

            // Generate method signature
            _tempText.AppendLine($"public static cls{_GetTableName()} Find(int {_GetTableName()}ID)");
            _tempText.AppendLine("{");
            Addref =true ;
            _tempText.AppendLine(m.ToString()); // Append property assignments
            _tempText.AppendLine();

            _tempText.AppendLine($"    bool IsFound = cls{_GetTableName()}Data.Get{_GetTableName()}InfoByID({m2.ToString()});");
            _tempText.AppendLine();
            _tempText.AppendLine("    if (IsFound)");
            _tempText.AppendLine("    {");
            Addref = true;
            _tempText.AppendLine($"        return new cls{_GetTableName()}( {m2.ToString()});");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        return null;");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");
            return _tempText.ToString();
        }

        private static string _GenerateGetInfoMethodForUsernameMethodInBusinessLayer()
        {
            StringBuilder m = new StringBuilder();

            // Generate property assignments
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    m.AppendLine($"this.{columnInfo.ColumnName} = {GetDefaultValueForType(columnInfo.DataType)};");
                }
            }

            StringBuilder parameters = new StringBuilder();
            bool isFirst = true;

            // Generate parameter list with 'ref' keyword only for the first parameter
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    if (isFirst)
                    {
                        parameters.AppendLine($"ref {columnInfo.ColumnName},");
                        isFirst = false;
                    }
                    else
                    {
                        parameters.AppendLine($"{columnInfo.ColumnName},");
                    }
                }
            }

            _tempText.AppendLine($"public static cls{_GetTableName()} FindByUsername(string UserName)");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"   {m.ToString()};");
            //_tempText.AppendLine("    string Password = \"\";");
            //_tempText.AppendLine("    bool IsActive = false;");
            _tempText.AppendLine();
            _tempText.AppendLine($"    bool IsFound = cls{_GetTableName()}Data.GetUserInfoByUserName");
            _tempText.AppendLine($"        ({parameters.ToString()});");
            _tempText.AppendLine();
            _tempText.AppendLine("    if (IsFound)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        return new cls{_GetTableName()}({parameters.ToString()});");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        return null;");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");
            return _tempText.ToString();
        }

        private static string _GenerateGetInfoMethodForUsernameAndPasswordMethodInBusinessLayer()
        {
            StringBuilder m = new StringBuilder();

            // Generate property assignments
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    m.AppendLine($"this.{columnInfo.ColumnName} = {GetDefaultValueForType(columnInfo.DataType)};");
                }
            }

            StringBuilder parameters = new StringBuilder();
            bool isFirst = true;

            // Generate parameter list with 'ref' keyword only for the first parameter
            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    if (isFirst)
                    {
                        parameters.AppendLine($"ref {columnInfo.ColumnName},");
                        isFirst = false;
                    }
                    else
                    {
                        parameters.AppendLine($"{columnInfo.ColumnName},");
                    }
                }
            }
            _tempText.AppendLine($"public static cls{_GetTableName()} FindByUsernameAndPassword(string UserName, string Password)");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"   {m.ToString()};");
            //_tempText.AppendLine("    int userID = -1, personID = -1;");
            //_tempText.AppendLine("    bool IsActive = false;");
            _tempText.AppendLine();
            _tempText.AppendLine($"    bool IsFound = cls{_GetTableName()}.Get{_GetTableName()}InfoByUserNameAndPassword");
            _tempText.AppendLine($"        ({parameters.ToString()});");
            _tempText.AppendLine();
            _tempText.AppendLine("    if (IsFound)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        return new cls{_GetTableName()}({parameters.ToString()});");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        return null;");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");
            return _tempText.ToString();
        }

        private static string _GenerateTestsFunctions()
        {
            _tableName = _GetTableName();
            StringBuilder initializationCode = new StringBuilder();

            // Generate object creation line
            initializationCode.AppendLine($"cls{_GetTableName()} {_GetTableName().ToLower()} = new cls{_GetTableName()}();");

            // Generate property assignments

            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    initializationCode.AppendLine($"{_GetTableName().ToLower()}.{columnInfo.ColumnName} = {GetDefaultValueForType(columnInfo.DataType)};");
                }
            }




            // Test methods...

            _tempText.AppendLine("//static void Main(string[] args)");
            _tempText.AppendLine("//{");
            _tempText.AppendLine(" //   TestAddLicenses();");
            _tempText.AppendLine(" //   TestFindLicenses();");
            _tempText.AppendLine(" //   TestUpdateLicenses();");
            _tempText.AppendLine(" //   TestDeleteLicenses();");
            _tempText.AppendLine(" //   Console.ReadLine();");
            _tempText.AppendLine("//}");

            // TestAdd method
            _tempText.AppendLine($"static void TestAdd{_tableName}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"{initializationCode.ToString()};");
            _tempText.AppendLine();
            _tempText.AppendLine($"    if ({_GetTableName().ToLower()}.Save())");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"{_tableName} added successfully!\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"Failed to add {_tableName}.\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");

            // Append all test methods
            StringBuilder all_tempText = new StringBuilder();
            all_tempText.AppendLine("// Test methods...");
            all_tempText.Append(_tempText.ToString());



            _tempText.AppendLine($"static void TestFind{_GetTableName()}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    int {_GetTableName()}IdToFind = 27; // Replace with the actual {_GetTableName()} ID to find");
            _tempText.AppendLine();
            _tempText.AppendLine($"    cls{_GetTableName()} found{_GetTableName()} = cls{_GetTableName()}.Find({_GetTableName()}IdToFind);");
            _tempText.AppendLine();
            _tempText.AppendLine($"    if (found{_GetTableName()} != null)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine($\"Found {_GetTableName()}: {_GetTableName()}ID={{found{_GetTableName()}.{_GetTableName()}ID}}, Notes={{found{_GetTableName()}.Notes}}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"{_GetTableName()} not found.\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");



            _tempText.AppendLine($"static void TestUpdate{_GetTableName()}s()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    int {_GetTableName()}IdToUpdate = 27; // Replace with the actual {_GetTableName()} ID to update");
            _tempText.AppendLine();
            _tempText.AppendLine($"    cls{_GetTableName()} {_GetTableName()} = cls{_GetTableName()}.Find({_GetTableName()}IdToUpdate);");
            _tempText.AppendLine();
            _tempText.AppendLine($"    if ({_GetTableName()} != null)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine($\"Current Notes: {_GetTableName()}.Notes}}\");");
            _tempText.AppendLine();
            _tempText.AppendLine("        // Modify the properties");
            _tempText.AppendLine($"        {_GetTableName()}.Notes = \"Updated {_GetTableName()}\";");
            _tempText.AppendLine();
            _tempText.AppendLine($"        if ({_GetTableName()}.Save())");
            _tempText.AppendLine("        {");
            _tempText.AppendLine($"            Console.WriteLine(\"{_GetTableName()} updated successfully!\");");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("        else");
            _tempText.AppendLine("        {");
            _tempText.AppendLine($"            Console.WriteLine(\"Failed to update {_GetTableName()}.\");");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"{_GetTableName()} not found.\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");



            _tempText.AppendLine($"static void TestDelete{_GetTableName()}s()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    int {_GetTableName()}IdToDelete = 36; // Replace with the actual {_GetTableName()} ID to delete");
            _tempText.AppendLine();
            _tempText.AppendLine($"    if (cls{_GetTableName()}.Delete{_GetTableName()}({_GetTableName()}IdToDelete))");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"{_GetTableName()} deleted successfully!\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"Failed to delete {_GetTableName()}.\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }




        //generel functions
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

