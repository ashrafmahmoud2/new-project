using GenerateDataAccessLayerLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenDataLayer
{
    public static class clsGenerateDataLayer_Data
    {


        private static bool _isLogin;
        private static string _dbName;
        private static string _tableName;
        private static string _tableSingleName;
        private static bool _isGenerateAllMode;
        private static StringBuilder _tempText;
        private static StringBuilder _parametersBuilder;
        private static List<List<clsColumnInfoForDataAccess>> _columnsInfo;

    


        //_Generate Methods & DataLayer
        public static string GenerateDataLayer(List<List<clsColumnInfoForDataAccess>> columnsInfo, string dbName)
        {

            _tempText = new StringBuilder();
            _dbName = dbName;
            _columnsInfo = columnsInfo;
            
            _tableSingleName = clsSQLDate.GetTableName();

            _tempText.AppendLine($"using System;\r\n" +
               $"using System.Data;\r\nusing " +
               $"System.Data.SqlClient;\r\n\r\nnamespace {_dbName}DataAccess\r\n{{");

            _tempText.Append($"public class cls{_tableSingleName}Data");
            _tempText.AppendLine();
            _tempText.AppendLine("{");

            // Check for additional conditions
            if (!_isGenerateAllMode)
            {
                _isLogin = clsSQLDate._DoesTableHaveUsernameAndPassword();
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
           
            _tempText.AppendLine($"public static bool Get{clsSQLDate.GetTableName()}InfoByID({_GetParametersByTableColumns(true)})");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    bool IsFound = false;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"   {clsSQLDate.GetConnectionString()}");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = @\"select * from {clsSQLDate.GetTableName()}" +
                $" where {clsSQLDate.GetTableName()}ID = @{clsSQLDate.GetTableName()}ID\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    command.Parameters.AddWithValue(\"@{clsSQLDate.GetTableName()}ID\"," +
                $" {clsSQLDate.GetTableName()}ID);");
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


            _tempText.AppendLine($"public static int AddNew{clsSQLDate.GetTableName()}({_GetParametersByTableColumns()}) ");
            _tempText.AppendLine($"{{    int {clsSQLDate.GetTableName()}ID = -1;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {clsSQLDate.GetConnectionString()};");
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
            _tempText.AppendLine($"            {clsSQLDate.GetTableName()}ID = InsertID;");
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
            _tempText.AppendLine($"    return {clsSQLDate.GetTableName()}ID;");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static string _GenerateUpdateMethod()
        {

            _tempText.AppendLine($"public static bool Update{clsSQLDate.GetTableName()}( {_GetParametersByTableColumns()})");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    int RowAffected = 0;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {clsSQLDate.GetConnectionString()};");
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
            _tempText.AppendLine($"public static bool Delete{clsSQLDate.GetTableName()}(int {clsSQLDate.GetTableName()}ID)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    int RowAffected = 0;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {clsSQLDate.GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"DELETE FROM {clsSQLDate.GetTableName()} WHERE {clsSQLDate.GetTableName()}ID = @{clsSQLDate.GetTableName()}ID\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    command.Parameters.AddWithValue(\"@{clsSQLDate.GetTableName()}ID\", {clsSQLDate.GetTableName()}ID);");
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
            _tempText.AppendLine($"public static bool IS{clsSQLDate.GetTableName()}Exists(int {clsSQLDate.GetTableName()}ID)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    bool exists = false;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {clsSQLDate.GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT found=1 FROM {clsSQLDate.GetTableName()} WHERE {clsSQLDate.GetTableName()}ID = @{clsSQLDate.GetTableName()}ID\";");
            _tempText.AppendLine("");
            _tempText.AppendLine("    SqlCommand command = new SqlCommand(query, connection);");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    command.Parameters.AddWithValue(\"@{clsSQLDate.GetTableName()}ID\", {clsSQLDate.GetTableName()}ID);");
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
            _tempText.AppendLine($"public static DataTable GetAll{clsSQLDate.GetTableName()}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    DataTable dt = new DataTable();");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {clsSQLDate.GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT * FROM {clsSQLDate.GetTableName()}_view\";");
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
            _tempText.AppendLine($"public static DataTable GetInfo{clsSQLDate.GetTableName()}ForUsername(string username)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    DataTable dt = new DataTable();");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {clsSQLDate.GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT * FROM {clsSQLDate.GetTableName()}_view WHERE Username = @Username\";");
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
            _tempText.AppendLine($"public static DataTable GetInfo{clsSQLDate.GetTableName()}ForUsernameAndPassword(string username, string password)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    DataTable dt = new DataTable();");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {clsSQLDate.GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT * FROM {clsSQLDate.GetTableName()}_view WHERE Username = @Username AND Password = @Password\";");
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
            _tempText.AppendLine($"public static bool Exists{clsSQLDate.GetTableName()}ForUsername(string username)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    bool exists = false;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {clsSQLDate.GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT Found=1 FROM {clsSQLDate.GetTableName()}_view WHERE Username = @Username\";");
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
            _tempText.AppendLine($"public static bool Exists{clsSQLDate.GetTableName()}ForUsernameAndPassword(string username, string password)");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    bool exists = false;");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    {clsSQLDate.GetConnectionString()};");
            _tempText.AppendLine("");
            _tempText.AppendLine($"    string query = \"SELECT Found=1 FROM {clsSQLDate.GetTableName()}_view WHERE Username = @Username AND Password = @Password\";");
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

        public static string _GetParametersExecuteReader()
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

        public static string _GetAddWithValueParameters()
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

        public static string _GetQueryOfInsert()
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

            string insertQuery = $"INSERT INTO {clsSQLDate.GetTableName()} " +
                $"({columnNames}) VALUES ({parameters}); SELECT SCOPE_IDENTITY();";

            return insertQuery;
        }

        public static string _GetQueryOfUpdate()
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
            string updateQuery = $"UPDATE {clsSQLDate.GetTableName()} SET {setClause} WHERE {clsSQLDate.GetTableName()}" +
                $"ID = @{clsSQLDate.GetTableName()}ID;";

            return updateQuery;
        }




        // Generate Data Access In File Path
        public static bool GenerateDataAccessInFilePath(string dbName, string filePath)
        {
            bool isGenerated = false;

            // Get table names for the specified database
            List<string> tablesNames =clsSQLDate .GetTablesNameByDBByList(dbName);
            List<List<clsColumnInfoForDataAccess>> ColumnsInfro = clsSQLDate.GetTableInfoInList(tablesNames.ToString(), dbName);
            string TextFile;

            if (tablesNames != null && tablesNames.Count > 0)
            {
                foreach (var table in tablesNames)
                {

                    TextFile = GenerateDataLayer(clsSQLDate.GetTableInfoInList(table.ToString(), dbName), dbName);
                    clsSQLDate.GenerateFile(TextFile, filePath, $"cls{table.ToString()}Data");
                }

                isGenerated = true;

            }
            else
                isGenerated = false;



            return isGenerated;
        }



    }
}
