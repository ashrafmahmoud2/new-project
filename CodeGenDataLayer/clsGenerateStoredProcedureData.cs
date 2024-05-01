using CodeGenBusinessLayer;
using GenerateDataAccessLayerLibrary;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenDataLayer
{
    public static class clsGenerateStoredProcedureData
    {
        private static bool _isLogin;
        private static string _dbName;
        private static string _tableName;
        private static string _tableSingleName;
        private static bool _isGenerateAllMode;
        private static StringBuilder _tempText;
        private static StringBuilder _parametersBuilder;
        private static List<List<clsColumnInfoForDataAccess>> _columnsInfo;


        //Generate Stored Procedure
        public static string GenerateGenerateStoredProcedure(List<List<clsColumnInfoForDataAccess>> columnsInfo, string dbName)
        {

            _tempText = new StringBuilder();
            _dbName = dbName;
            _columnsInfo = columnsInfo;
            _tableSingleName = GetTableName();

            if (!_isGenerateAllMode)
            {
                _isLogin = DoesTableHaveUsernameAndPassword();
            }

            if (_isLogin)
            {
                _GenerateGenerateStoredProcedureAsLoginInfo();
            }
            else
            {
                _GenerateGenerateStoredProcedureAsNormal();
            }


            return _tempText.ToString();
        }
        
        private static string _GenerateGenerateStoredProcedureAsLoginInfo()
        {

            _GenerateAddMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");


            _GenerateUpdateMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");


            _GenerateDeleteMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");

            _GenerateGetAllMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");

            _GenerateFindMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");

            _GenerateGetInfoMethodForUsernameMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");

            _GenerateGetInfoMethodForUsernameAndPasswordMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");

            _GenerateExistsMethodForUsernameMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");

            _GenerateExistsMethodForUsernameAndPasswordInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");

            _GenerateTestsFunctionsInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");

            return _tempText.ToString();
        }

        private static string _GenerateGenerateStoredProcedureAsNormal()
        {
            _GenerateAddMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");


            _GenerateUpdateMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");


            _GenerateDeleteMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");

            _GenerateGetAllMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");

            _GenerateFindMethodInGenerateStoredProcedure();
            _tempText.AppendLine("----------------------\n ----------------------");





            return _tempText.ToString();
        }

        private static string _GetParmterForGenerateStoredProcedure(bool addAtSign = true, bool updateParameter = false, bool withDataType = false)
        {
            StringBuilder parameters = new StringBuilder();

            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    if (!updateParameter)
                    {
                        if (addAtSign)
                        {
                            parameters.Append($"@{columnInfo.ColumnName}{(withDataType ? " " + columnInfo.DataType : "")},");
                        }
                        else
                        {
                            parameters.Append($"{columnInfo.ColumnName}{(withDataType ? " " + columnInfo.DataType : "")},");
                        }
                    }
                    else
                    {
                        parameters.Append($"{columnInfo.ColumnName} = @{columnInfo.ColumnName},");
                    }
                }
            }

            int lastIndex = parameters.ToString().LastIndexOf(',');
            parameters.Remove(lastIndex, 1);

            return parameters.ToString();
        }

        private static string _GenerateAddMethodInGenerateStoredProcedure()
        {


            _tempText.AppendLine($"create procedure SP_AddNew{GetTableName()}");
            _tempText.AppendLine($"{_GetParmterForGenerateStoredProcedure(true, false, true)}");
            _tempText.AppendLine($", @New{GetTableName()}ID int output");
            _tempText.AppendLine($"as");
            _tempText.AppendLine($"begin");
            _tempText.AppendLine($"insert into {GetTableName()} ({_GetParmterForGenerateStoredProcedure(false)})" +
                $"\r\nvalues({_GetParmterForGenerateStoredProcedure()})" +
                $"\r\nset @New{GetTableName()}ID = scope_identity();");
            _tempText.AppendLine($"end;");
            _tempText.AppendLine($"go");
            _tempText.AppendLine();

            return _tempText.ToString();
        }

        private static string _GenerateUpdateMethodInGenerateStoredProcedure()
        {


            _tempText.AppendLine($"create procedure SP_Update{GetTableName()}");
            _tempText.AppendLine($"{_GetParmterForGenerateStoredProcedure(true, false, true)}");
            _tempText.AppendLine($"as");
            _tempText.AppendLine($"begin");
            _tempText.AppendLine($"Update {GetTableName()}");

            _tempText.AppendLine($"set {_GetParmterForGenerateStoredProcedure(true, true)}");

            _tempText.AppendLine($"end;");
            _tempText.AppendLine($"go");
            _tempText.AppendLine();

            return _tempText.ToString();
        }

        private static string _GenerateDeleteMethodInGenerateStoredProcedure()
        {

            _tempText.AppendLine($"create procedure SP_Delete{GetTableName()}");
            _tempText.AppendLine($"@{GetTableName()}ID  int");
            _tempText.AppendLine($"as");
            _tempText.AppendLine($"begin");
            _tempText.AppendLine($"delete  from {GetTableName()}   where {GetTableName()}ID = @{GetTableName()}ID");
            _tempText.AppendLine($"end;");
            _tempText.AppendLine($"go");
            _tempText.AppendLine();
            return _tempText.ToString();
        }

        private static string _GenerateFindMethodInGenerateStoredProcedure()
        {
            _tempText.AppendLine($"create procedure SP_Get{GetTableName()}InfoByID");
            _tempText.AppendLine($"{GetTableName()}ID  int");
            _tempText.AppendLine($"as");
            _tempText.AppendLine($"begin");
            _tempText.AppendLine($"select * from  {GetTableName()}  where {GetTableName()}ID = @{GetTableName()}ID");
            _tempText.AppendLine($"end;");
            _tempText.AppendLine($"go");
            _tempText.AppendLine();

            return _tempText.ToString();
        }

        private static string _GenerateGetAllMethodInGenerateStoredProcedure()
        {
            _tempText.AppendLine($"create procedure SP_GetAll{GetTableName()}()");
            _tempText.AppendLine("as");
            _tempText.AppendLine("begin");
            _tempText.AppendLine($"  select * from {GetTableName()};");
            _tempText.AppendLine("end;");
            _tempText.AppendLine("go;");
            return _tempText.ToString();
        }

        private static string _Generate_DoesExistMethodInGenerateStoredProcedure()
        {
            _tempText.AppendLine($"create procedure SP_Does{GetTableName()}Exist");
            _tempText.AppendLine("@{GetTableName()} int ");
            _tempText.AppendLine("as");
            _tempText.AppendLine("begin");
            _tempText.AppendLine($"  if exists(select top 1 found = 1 from  where {GetTableName()}ID = @{GetTableName()}ID);");
            _tempText.AppendLine("return 1 ");
            _tempText.AppendLine("else\r\n    return 0");
            _tempText.AppendLine("end;");
            _tempText.AppendLine("go;");
            return _tempText.ToString();
        }

        private static string _GenerateExistsMethodForUsernameMethodInGenerateStoredProcedure()
        {
            _tempText.AppendLine($"SP_DoesUserExistByUsername");
            _tempText.AppendLine("@Username nvarchar(20) ");
            _tempText.AppendLine("as");
            _tempText.AppendLine("begin");
            _tempText.AppendLine($"if exists(select top 1 found = 1 from  where Username = @Username);");
            _tempText.AppendLine("return 1 ");
            _tempText.AppendLine("else\r\n    return 0");
            _tempText.AppendLine("end;");

            return _tempText.ToString();
        }

        private static string _GenerateExistsMethodForUsernameAndPasswordInGenerateStoredProcedure()
        {
            _tempText.AppendLine($"SP_DoesUserExistByUsername");
            _tempText.AppendLine("@Username nvarchar(20),\r\n@Password nvarchar(20) ");
            _tempText.AppendLine("as");
            _tempText.AppendLine("begin");
            _tempText.AppendLine($"if exists(select top 1 found = 1 from  where Username = @Username and Password = @Password);");
            _tempText.AppendLine("return 1 ");
            _tempText.AppendLine("else\r\n    return 0");
            return _tempText.ToString();
        }

        private static string _GenerateGetInfoMethodForUsernameMethodInGenerateStoredProcedure()
        {
            _tempText.AppendLine($"create procedure SP_GetUserInfoByUsername");
            _tempText.AppendLine($"@Username nvarchar(20)");
            _tempText.AppendLine($"as");
            _tempText.AppendLine($"begin");
            _tempText.AppendLine($"select * from   where Username = @Username");
            _tempText.AppendLine($"end;");
            _tempText.AppendLine($"go");
            _tempText.AppendLine();
            return _tempText.ToString();
        }

        private static string _GenerateGetInfoMethodForUsernameAndPasswordMethodInGenerateStoredProcedure()
        {
            _tempText.AppendLine($"create procedure SP_GetUserInfoByUsername");
            _tempText.AppendLine($"@Username nvarchar(20),\r\n@Password nvarchar(20)");
            _tempText.AppendLine($"as");
            _tempText.AppendLine($"begin");
            _tempText.AppendLine($"select * from where Username = @Username and Password = @Password");
            _tempText.AppendLine($"end;");
            _tempText.AppendLine($"go");
            _tempText.AppendLine();
            return _tempText.ToString();
        }

        private static string _GenerateTestsFunctionsInGenerateStoredProcedure()
        {
            _tableName = GetTableName();
            StringBuilder initializationCode = new StringBuilder();

            // Generate object creation line
            initializationCode.AppendLine($"cls{GetTableName()} {GetTableName().ToLower()} = new cls{GetTableName()}();");

            // Generate property assignments

            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    initializationCode.AppendLine($"{GetTableName().ToLower()}.{columnInfo.ColumnName} = {clsGenerateBusinessLayer_Data.GetDefaultValueForType(columnInfo.DataType)};");
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
            _tempText.AppendLine($"    if ({GetTableName().ToLower()}.Save())");
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



            _tempText.AppendLine($"static void TestFind{GetTableName()}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    int {GetTableName()}IdToFind = 27; // Replace with the actual {GetTableName()} ID to find");
            _tempText.AppendLine();
            _tempText.AppendLine($"    cls{GetTableName()} found{GetTableName()} = cls{GetTableName()}.Find({GetTableName()}IdToFind);");
            _tempText.AppendLine();
            _tempText.AppendLine($"    if (found{GetTableName()} != null)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine($\"Found {GetTableName()}: {GetTableName()}ID={{found{GetTableName()}.{GetTableName()}ID}}, Notes={{found{GetTableName()}.Notes}}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"{GetTableName()} not found.\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");



            _tempText.AppendLine($"static void TestUpdate{GetTableName()}s()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    int {GetTableName()}IdToUpdate = 27; // Replace with the actual {GetTableName()} ID to update");
            _tempText.AppendLine();
            _tempText.AppendLine($"    cls{GetTableName()} {GetTableName()} = cls{GetTableName()}.Find({GetTableName()}IdToUpdate);");
            _tempText.AppendLine();
            _tempText.AppendLine($"    if ({GetTableName()} != null)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine($\"Current Notes: {GetTableName()}.Notes}}\");");
            _tempText.AppendLine();
            _tempText.AppendLine("        // Modify the properties");
            _tempText.AppendLine($"        {GetTableName()}.Notes = \"Updated {GetTableName()}\";");
            _tempText.AppendLine();
            _tempText.AppendLine($"        if ({GetTableName()}.Save())");
            _tempText.AppendLine("        {");
            _tempText.AppendLine($"            Console.WriteLine(\"{GetTableName()} updated successfully!\");");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("        else");
            _tempText.AppendLine("        {");
            _tempText.AppendLine($"            Console.WriteLine(\"Failed to update {GetTableName()}.\");");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"{GetTableName()} not found.\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");



            _tempText.AppendLine($"static void TestDelete{GetTableName()}s()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    int {GetTableName()}IdToDelete = 36; // Replace with the actual {GetTableName()} ID to delete");
            _tempText.AppendLine();
            _tempText.AppendLine($"    if (cls{GetTableName()}.Delete{GetTableName()}({GetTableName()}IdToDelete))");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"{GetTableName()} deleted successfully!\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"Failed to delete {GetTableName()}.\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        public static string GetTableName()
        {
            if (_columnsInfo.Count > 0)
            {
                string firstValue = _columnsInfo[0][0].ColumnName;
                return firstValue.Remove(firstValue.Length - 2);
            }
            return string.Empty;
        }

        public static bool DoesTableHaveColumn(string columnName)
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

        public static bool DoesTableHaveUsernameAndPassword()
        {
            return (DoesTableHaveColumn("username") && DoesTableHaveColumn("password"));
        }


        //Generate Stored Procedures
        public static bool GenerateStoredProceduresToAllTables(string dbName)
        {
            bool isGenerated = false;

            // Get table names for the specified database
            List<string> tablesNames = clsSQLDate.GetTablesNameByDBByList(dbName);
            List<List<clsColumnInfoForDataAccess>> ColumnsInfro = clsSQLDate.GetTableInfoInList(tablesNames.ToString(), dbName);
            string TextFile;

            if (tablesNames != null && tablesNames.Count > 0)
            {
                foreach (var table in tablesNames)
                {

                    TextFile = GenerateGenerateStoredProcedure(clsSQLDate.GetTableInfoInList(table.ToString(), dbName), dbName);
                    GenerateStoredProceduresToSelectedTable(dbName, table.ToString());
                }

                isGenerated = true;

            }
            else
                isGenerated = false;



            return isGenerated;

        }

        public static bool GenerateStoredProceduresToSelectedTable(string dbName, string selectTable)
        {
            bool isGenerated = false;

            if (!string.IsNullOrWhiteSpace(selectTable))
            {
                try
                {
                    string storedProcedureText =
                            GenerateGenerateStoredProcedure(clsSQLDate.GetTableInfoInList(selectTable, dbName), dbName);

                    //SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                    //builder.DataSource = "SQL Server";
                    //builder.UserID = "your_username";
                    //builder.Password = "your_password";
                    //builder.InitialCatalog = dbName;

                    using (SqlConnection connection = new SqlConnection(clsConnectionString.ConnectionString))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand(storedProcedureText, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    isGenerated = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                isGenerated = false;
            }

            return isGenerated;
        }





    }
}
