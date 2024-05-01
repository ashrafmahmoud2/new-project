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
            _tableSingleName = clsSQLDate.GetTableName();

            if (!_isGenerateAllMode)
            {
                _isLogin = clsSQLDate._DoesTableHaveUsernameAndPassword();
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


            _tempText.AppendLine($"create procedure SP_AddNew{clsSQLDate.GetTableName()}");
            _tempText.AppendLine($"{_GetParmterForGenerateStoredProcedure(true, false, true)}");
            _tempText.AppendLine($", @New{clsSQLDate.GetTableName()}ID int output");
            _tempText.AppendLine($"as");
            _tempText.AppendLine($"begin");
            _tempText.AppendLine($"insert into {clsSQLDate.GetTableName()} ({_GetParmterForGenerateStoredProcedure(false)})" +
                $"\r\nvalues({_GetParmterForGenerateStoredProcedure()})" +
                $"\r\nset @New{clsSQLDate.GetTableName()}ID = scope_identity();");
            _tempText.AppendLine($"end;");
            _tempText.AppendLine($"go");
            _tempText.AppendLine();

            return _tempText.ToString();
        }

        private static string _GenerateUpdateMethodInGenerateStoredProcedure()
        {


            _tempText.AppendLine($"create procedure SP_Update{clsSQLDate.GetTableName()}");
            _tempText.AppendLine($"{_GetParmterForGenerateStoredProcedure(true, false, true)}");
            _tempText.AppendLine($"as");
            _tempText.AppendLine($"begin");
            _tempText.AppendLine($"Update {clsSQLDate.GetTableName()}");

            _tempText.AppendLine($"set {_GetParmterForGenerateStoredProcedure(true, true)}");

            _tempText.AppendLine($"end;");
            _tempText.AppendLine($"go");
            _tempText.AppendLine();

            return _tempText.ToString();
        }

        private static string _GenerateDeleteMethodInGenerateStoredProcedure()
        {

            _tempText.AppendLine($"create procedure SP_Delete{clsSQLDate.GetTableName()}");
            _tempText.AppendLine($"@{clsSQLDate.GetTableName()}ID  int");
            _tempText.AppendLine($"as");
            _tempText.AppendLine($"begin");
            _tempText.AppendLine($"delete  from {clsSQLDate.GetTableName()}   where {clsSQLDate.GetTableName()}ID = @{clsSQLDate.GetTableName()}ID");
            _tempText.AppendLine($"end;");
            _tempText.AppendLine($"go");
            _tempText.AppendLine();
            return _tempText.ToString();
        }

        private static string _GenerateFindMethodInGenerateStoredProcedure()
        {
            _tempText.AppendLine($"create procedure SP_Get{clsSQLDate.GetTableName()}InfoByID");
            _tempText.AppendLine($"{clsSQLDate.GetTableName()}ID  int");
            _tempText.AppendLine($"as");
            _tempText.AppendLine($"begin");
            _tempText.AppendLine($"select * from  {clsSQLDate.GetTableName()}  where {clsSQLDate.GetTableName()}ID = @{clsSQLDate.GetTableName()}ID");
            _tempText.AppendLine($"end;");
            _tempText.AppendLine($"go");
            _tempText.AppendLine();

            return _tempText.ToString();
        }

        private static string _GenerateGetAllMethodInGenerateStoredProcedure()
        {
            _tempText.AppendLine($"create procedure SP_GetAll{clsSQLDate.GetTableName()}()");
            _tempText.AppendLine("as");
            _tempText.AppendLine("begin");
            _tempText.AppendLine($"  select * from {clsSQLDate.GetTableName()};");
            _tempText.AppendLine("end;");
            _tempText.AppendLine("go;");
            return _tempText.ToString();
        }

        private static string _Generate_DoesExistMethodInGenerateStoredProcedure()
        {
            _tempText.AppendLine($"create procedure SP_Does{clsSQLDate.GetTableName()}Exist");
            _tempText.AppendLine("@{clsSQLDate.GetTableName()} int ");
            _tempText.AppendLine("as");
            _tempText.AppendLine("begin");
            _tempText.AppendLine($"  if exists(select top 1 found = 1 from  where {clsSQLDate.GetTableName()}ID = @{clsSQLDate.GetTableName()}ID);");
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
            _tableName = clsSQLDate.GetTableName();
            StringBuilder initializationCode = new StringBuilder();

            // Generate object creation line
            initializationCode.AppendLine($"cls{clsSQLDate.GetTableName()} {clsSQLDate.GetTableName().ToLower()} = new cls{clsSQLDate.GetTableName()}();");

            // Generate property assignments

            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    initializationCode.AppendLine($"{clsSQLDate.GetTableName().ToLower()}.{columnInfo.ColumnName} = {clsGenerateBusinessLayer_Data.GetDefaultValueForType(columnInfo.DataType)};");
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
            _tempText.AppendLine($"    if ({clsSQLDate.GetTableName().ToLower()}.Save())");
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



            _tempText.AppendLine($"static void TestFind{clsSQLDate.GetTableName()}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    int {clsSQLDate.GetTableName()}IdToFind = 27; // Replace with the actual {clsSQLDate.GetTableName()} ID to find");
            _tempText.AppendLine();
            _tempText.AppendLine($"    cls{clsSQLDate.GetTableName()} found{clsSQLDate.GetTableName()} = cls{clsSQLDate.GetTableName()}.Find({clsSQLDate.GetTableName()}IdToFind);");
            _tempText.AppendLine();
            _tempText.AppendLine($"    if (found{clsSQLDate.GetTableName()} != null)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine($\"Found {clsSQLDate.GetTableName()}: {clsSQLDate.GetTableName()}ID={{found{clsSQLDate.GetTableName()}.{clsSQLDate.GetTableName()}ID}}, Notes={{found{clsSQLDate.GetTableName()}.Notes}}\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"{clsSQLDate.GetTableName()} not found.\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");



            _tempText.AppendLine($"static void TestUpdate{clsSQLDate.GetTableName()}s()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    int {clsSQLDate.GetTableName()}IdToUpdate = 27; // Replace with the actual {clsSQLDate.GetTableName()} ID to update");
            _tempText.AppendLine();
            _tempText.AppendLine($"    cls{clsSQLDate.GetTableName()} {clsSQLDate.GetTableName()} = cls{clsSQLDate.GetTableName()}.Find({clsSQLDate.GetTableName()}IdToUpdate);");
            _tempText.AppendLine();
            _tempText.AppendLine($"    if ({clsSQLDate.GetTableName()} != null)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine($\"Current Notes: {clsSQLDate.GetTableName()}.Notes}}\");");
            _tempText.AppendLine();
            _tempText.AppendLine("        // Modify the properties");
            _tempText.AppendLine($"        {clsSQLDate.GetTableName()}.Notes = \"Updated {clsSQLDate.GetTableName()}\";");
            _tempText.AppendLine();
            _tempText.AppendLine($"        if ({clsSQLDate.GetTableName()}.Save())");
            _tempText.AppendLine("        {");
            _tempText.AppendLine($"            Console.WriteLine(\"{clsSQLDate.GetTableName()} updated successfully!\");");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("        else");
            _tempText.AppendLine("        {");
            _tempText.AppendLine($"            Console.WriteLine(\"Failed to update {clsSQLDate.GetTableName()}.\");");
            _tempText.AppendLine("        }");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"{clsSQLDate.GetTableName()} not found.\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");



            _tempText.AppendLine($"static void TestDelete{clsSQLDate.GetTableName()}s()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    int {clsSQLDate.GetTableName()}IdToDelete = 36; // Replace with the actual {clsSQLDate.GetTableName()} ID to delete");
            _tempText.AppendLine();
            _tempText.AppendLine($"    if (cls{clsSQLDate.GetTableName()}.Delete{clsSQLDate.GetTableName()}({clsSQLDate.GetTableName()}IdToDelete))");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"{clsSQLDate.GetTableName()} deleted successfully!\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    else");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        Console.WriteLine(\"Failed to delete {clsSQLDate.GetTableName()}.\");");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("}");

            return _tempText.ToString();
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
