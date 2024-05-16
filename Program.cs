using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TestReadDB;
using System;

string connectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.6.1.46)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=TCDN)));User Id=mic_di;Password=Micdev@2023;Pooling=true;Min Pool Size=1;Connection Lifetime=180;Max Pool Size=100;Incr Pool Size=5";

using (OracleConnection connection = new OracleConnection(connectionString))
{
    connection.Open();
    string jsonPath = "D:\\M\\Project\\TestReadDB\\TestReadDB\\ConfigurationTemplate.json";
    string jsonString = File.ReadAllText(jsonPath);
    JObject json = JObject.Parse(jsonString);
    // Query to retrieve all table names
    string pathFolderConfiguration = "D:\\M\\Project\\TestReadDB\\TestReadDB\\Configuration";
    string tableQuery = "SELECT table_name FROM user_tables";



    string entityFilePath = "D:\\M\\Project\\TestReadDB\\TestReadDB\\Entities";
   
    if (!Directory.Exists(pathFolderConfiguration)) {
        Directory.CreateDirectory(pathFolderConfiguration);
    }
    if (!Directory.Exists(entityFilePath))
    {
        Directory.CreateDirectory(entityFilePath);
    }
    using (OracleCommand tableCommand = new OracleCommand(tableQuery, connection))
    {
        using (OracleDataReader tableReader = tableCommand.ExecuteReader())
        {
            while (tableReader.Read())
            {
                List<string> primaryKey = new List<string>();
                List<DefineColumn> DefineColumns = new List<DefineColumn>();
                string tableName = tableReader.GetString(0);
                Console.WriteLine("Table Name: " + tableName);
                // Query to retrieve column details for the current table
                string columnQuery = @"
                    SELECT column_name, data_type, data_length, nullable
                    FROM user_tab_columns
                    WHERE table_name = :tableName";

                using (OracleCommand columnCommand = new OracleCommand(columnQuery, connection))
                {
                    columnCommand.Parameters.Add(new OracleParameter("tableName", tableName));

                    using (OracleDataReader columnReader = columnCommand.ExecuteReader())
                    {
                        Console.WriteLine("Column Details:");
                        while (columnReader.Read())
                        {
                            string columnName = columnReader.GetString(0);
                            string dataType = columnReader.GetString(1);
                            int dataLength = columnReader.GetInt32(2); // For VARCHAR2 columns, this represents the maximum length
                            string nullable = columnReader.GetString(3);

                            string nullability = (nullable == "Y") ? "Allows null" : "Does not allow null";
                            DefineColumn itemDefineColumn = new DefineColumn()
                            {
                                ColumnName = columnName,
                                ColumnType = dataType,
                                IsAllowNull = (nullable == "Y") ? true : false,
                            };
                            DefineColumns.Add(itemDefineColumn);
                            Console.WriteLine($"    Name: {columnName}, Type: {dataType}, Max Length: {dataLength}, Nullability: {nullability}");
                        }
                    }
                }

                // Query to retrieve primary key columns for the current table
                string primaryKeyQuery = @"
                    SELECT acc.column_name
                    FROM user_constraints uc
                    JOIN user_cons_columns acc ON uc.constraint_name = acc.constraint_name
                    WHERE uc.table_name = :tableName AND uc.constraint_type = 'P'";

                using (OracleCommand primaryKeyCommand = new OracleCommand(primaryKeyQuery, connection))
                {
                    primaryKeyCommand.Parameters.Add(new OracleParameter("tableName", tableName));

                    using (OracleDataReader primaryKeyReader = primaryKeyCommand.ExecuteReader())
                    {
                        Console.Write("Primary Key(s): ");
                        while (primaryKeyReader.Read())
                        {
                            string primaryKeyColumnName = primaryKeyReader.GetString(0);
                            primaryKey.Add(primaryKeyColumnName);
                            Console.Write(primaryKeyColumnName + " ");
                        }
                        Console.WriteLine(); // Add newline after printing primary key column(s)
                    }
                }
                GenConfiguration.GenConfigTemplate(tableName, DefineColumns, primaryKey, pathFolderConfiguration);
                GenEntities.GenEntity(tableName, DefineColumns, entityFilePath);
                Console.WriteLine(); // Add newline between tables
            }
        }
    }
}

