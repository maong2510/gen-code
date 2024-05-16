using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestReadDB
{
    public class DefineColumn
    {
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAllowNull { get; set; }
        public string CSharpType
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ColumnType)){
                    switch (ColumnType)
                    {
                        case "VARCHAR2":
                        case "VARCHAR":
                        case "CHAR":
                        case "NCHAR":
                        case "NCLOB":
                        case "CLOB":
                            return "string";

                        case "NUMBER":
                        case "FLOAT":
                            return "decimal";
                        case "DATE":
                        case "TIMESTAMP":
                            return "DateTime";
                        case "BOOLEAN":
                            return "bool";
                    }
                }
                return "string";
            }
        }
    }
}
