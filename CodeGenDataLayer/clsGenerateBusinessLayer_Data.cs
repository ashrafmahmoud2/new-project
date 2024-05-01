using GenerateDataAccessLayerLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenDataLayer
{
    public static class clsGenerateBusinessLayer_Data
    {

        private static bool _isLogin;
        private static string _dbName;
        private static string _tableName;
        private static string _tableSingleName;
        private static bool _isGenerateAllMode;
        private static StringBuilder _tempText;
        private static StringBuilder _parametersBuilder;
        private static List<List<clsColumnInfoForDataAccess>> _columnsInfo;

        public static string GenerateBusinessLayer(List<List<clsColumnInfoForDataAccess>> columnsInfo, string dbName)
        {
            _tempText = new StringBuilder();
            _dbName = dbName;
            _columnsInfo = columnsInfo;
            _tableSingleName = clsSQLDate.GetTableName();

            _tempText.AppendLine($"using System;\r\n" +
               $"using System.Data;\r\nusing " +
               $"System.Data.SqlClient;\r\n\r\nnamespace {_dbName}BusinessLayer\r\n{{");

            _tempText.Append($"public class cls{_tableSingleName}");
            _tempText.AppendLine();
            _tempText.AppendLine("{");

            // Check for additional conditions
            if (!_isGenerateAllMode)
            {
                _isLogin =clsSQLDate._DoesTableHaveUsernameAndPassword();
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
                    _tempText.AppendLine($"this.{columnInfo.ColumnName}" + $" = {GetDefaultValueForType(columnInfo.DataType)};");
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

        public static string GetDefaultValueForType(string type)
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

            _tempText.AppendLine($"public bool _AddNew{clsSQLDate.GetTableName()}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    this.{clsSQLDate.GetTableName()}ID =" +
                $" cls{clsSQLDate.GetTableName()}Data.AddNew{clsSQLDate.GetTableName()}({addParameters});");
            _tempText.AppendLine($"    return (this.{clsSQLDate.GetTableName()}ID != -1);");
            _tempText.AppendLine("}");
            _tempText.AppendLine();


            _tempText.AppendLine($"private bool _Update{clsSQLDate.GetTableName()}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    return cls{clsSQLDate.GetTableName()}Data.Update{clsSQLDate.GetTableName()}({updateParameters});");
            _tempText.AppendLine("}");
            _tempText.AppendLine();


            _tempText.AppendLine("public bool Save()");
            _tempText.AppendLine("{");
            _tempText.AppendLine("    switch (Mode)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine("        case enMode.AddNew:");
            _tempText.AppendLine($"            if (_AddNew{clsSQLDate.GetTableName()}())");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                Mode = enMode.Update;");
            _tempText.AppendLine("                return true;");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("            else");
            _tempText.AppendLine("            {");
            _tempText.AppendLine("                return false;");
            _tempText.AppendLine("            }");
            _tempText.AppendLine("        case enMode.Update:");
            _tempText.AppendLine($"            return _Update{clsSQLDate.GetTableName()}();");
            _tempText.AppendLine("    }");
            _tempText.AppendLine("    return false;");
            _tempText.AppendLine("}");

            return _tempText.ToString();
        }

        private static string _GenerateDeleteMethodInBusinessLayer()
        {
            _tempText.AppendLine($"public static bool Delete{clsSQLDate.GetTableName()}(int {clsSQLDate.GetTableName()}ID)");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    return cls{clsSQLDate.GetTableName()}Data.Delete{clsSQLDate.GetTableName()}({clsSQLDate.GetTableName()}ID);");
            _tempText.AppendLine("}");
            return _tempText.ToString();
        }

        private static string _GenerateGetAllMethodInBusinessLayer()
        {
            _tempText.AppendLine($"public static DataTable GetAll{clsSQLDate.GetTableName()}()");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    return cls{clsSQLDate.GetTableName()}Data.GetAll{clsSQLDate.GetTableName()}();");
            _tempText.AppendLine("}");
            return _tempText.ToString();
        }

        private static string _GenerateExistsMethodForUsernameMethodInBusinessLayer()
        {
            _tempText.AppendLine($"public static bool Does{clsSQLDate.GetTableName()}Exist(string Username)");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    return cls{clsSQLDate.GetTableName()}Data.Does{clsSQLDate.GetTableName()}Exist(Username);");
            _tempText.AppendLine("}");
            return _tempText.ToString();
        }

        private static string _GenerateExistsMethodForUsernameAndPasswordInBusinessLayer()
        {
            _tempText.AppendLine($"public static bool Does{clsSQLDate.GetTableName()}Exist(string Username, string Password)");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"    return cls{clsSQLDate.GetTableName()}Data.Does{clsSQLDate.GetTableName()}Exist(Username, Password);");
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
            _tempText.AppendLine($"public static cls{clsSQLDate.GetTableName()} Find(int {clsSQLDate.GetTableName()}ID)");
            _tempText.AppendLine("{");
            Addref = true;
            _tempText.AppendLine(m.ToString()); // Append property assignments
            _tempText.AppendLine();

            _tempText.AppendLine($"    bool IsFound = cls{clsSQLDate.GetTableName()}Data.Get{clsSQLDate.GetTableName()}InfoByID({m2.ToString()});");
            _tempText.AppendLine();
            _tempText.AppendLine("    if (IsFound)");
            _tempText.AppendLine("    {");
            Addref = true;
            _tempText.AppendLine($"        return new cls{clsSQLDate.GetTableName()}( {m2.ToString()});");
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

            _tempText.AppendLine($"public static cls{clsSQLDate.GetTableName()} FindByUsername(string UserName)");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"   {m.ToString()};");
            //_tempText.AppendLine("    string Password = \"\";");
            //_tempText.AppendLine("    bool IsActive = false;");
            _tempText.AppendLine();
            _tempText.AppendLine($"    bool IsFound = cls{clsSQLDate.GetTableName()}Data.GetUserInfoByUserName");
            _tempText.AppendLine($"        ({parameters.ToString()});");
            _tempText.AppendLine();
            _tempText.AppendLine("    if (IsFound)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        return new cls{clsSQLDate.GetTableName()}({parameters.ToString()});");
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
            _tempText.AppendLine($"public static cls{clsSQLDate.GetTableName()} FindByUsernameAndPassword(string UserName, string Password)");
            _tempText.AppendLine("{");
            _tempText.AppendLine($"   {m.ToString()};");
            //_tempText.AppendLine("    int userID = -1, personID = -1;");
            //_tempText.AppendLine("    bool IsActive = false;");
            _tempText.AppendLine();
            _tempText.AppendLine($"    bool IsFound = cls{clsSQLDate.GetTableName()}.Get{clsSQLDate.GetTableName()}InfoByUserNameAndPassword");
            _tempText.AppendLine($"        ({parameters.ToString()});");
            _tempText.AppendLine();
            _tempText.AppendLine("    if (IsFound)");
            _tempText.AppendLine("    {");
            _tempText.AppendLine($"        return new cls{clsSQLDate.GetTableName()}({parameters.ToString()});");
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
            _tableName = clsSQLDate.GetTableName();
            StringBuilder initializationCode = new StringBuilder();

            // Generate object creation line
            initializationCode.AppendLine($"cls{clsSQLDate.GetTableName()} {clsSQLDate.GetTableName().ToLower()} = new cls{clsSQLDate.GetTableName()}();");

            // Generate property assignments

            foreach (var columnList in _columnsInfo)
            {
                foreach (var columnInfo in columnList)
                {
                    initializationCode.AppendLine($"{clsSQLDate.GetTableName().ToLower()}.{columnInfo.ColumnName} = {GetDefaultValueForType(columnInfo.DataType)};");
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





        //Generate Business Clas In FilePath
        public static bool GenerateBusinessClasInFilePath(string DBName, string filePath)
        {
            bool isGenerated = false;

            // Get table names for the specified database
            List<string> tablesNames =
            clsSQLDate.GetTablesNameByDBByList(DBName);
            List<List<clsColumnInfoForDataAccess>> ColumnsInfro =
            clsSQLDate.GetTableInfoInList(tablesNames.ToString(), DBName);
            string TextFile;

            if (tablesNames != null && tablesNames.Count > 0)
            {
                foreach (var table in tablesNames)
                {

                    TextFile = GenerateBusinessLayer(
            clsSQLDate.GetTableInfoInList(table.ToString(), DBName), DBName);

                    clsSQLDate.GenerateFile(TextFile, filePath, $"cls{table.ToString()}");
                }

                isGenerated = true;

            }
            else
                isGenerated = false;



            return isGenerated;
        }
    }
}
