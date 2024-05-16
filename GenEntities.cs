using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestReadDB
{
    public static class GenEntities
    {
        private const string paramDeclareField = "{{DeclareField}}";
        private const string paramTableName = "{{TableName}}";
        public static void GenEntity(string tableName, List<DefineColumn> defineColumns, string entityFilePath)
        {
           
        
            string baseDirectory = entityFilePath;
            string fileName = $"{tableName}.cs";
            string result = $"using {Enum.ProjectName};\r\n" +
                "namespace Domain.Entities\r\n" +
                "{\r\n    public class {{TableName}}\r\n    " +
                "{\r\n    " +
                        "{{DeclareField}}  " +
                "\r\n    " +
                "}\r\n" +
                "}\r\n";

            string filePath = Path.Combine(baseDirectory, fileName);
            var declareFile = "";
            foreach (var itemColumn in defineColumns)
            {
                if (itemColumn.IsAllowNull && itemColumn.CSharpType != "string")
                    declareFile += $"public {itemColumn.CSharpType}? {itemColumn.ColumnName} {{get;set;}} " + Environment.NewLine;
                else
                    declareFile += $"public {itemColumn.CSharpType} {itemColumn.ColumnName} {{get;set;}} " + Environment.NewLine;
            }
            result = result.Replace(paramDeclareField, declareFile).Replace(paramTableName,tableName);
            if (!string.IsNullOrEmpty(result))
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.WriteAllText(filePath, result);
            }


        }
    }
}
