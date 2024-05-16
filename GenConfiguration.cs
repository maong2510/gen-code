using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace TestReadDB
{
    public static class GenConfiguration
    {
        private const string paramTableConfig = "{{TableConfig}}";
        private const string paramTableName = "{{TableName}}";
        private const string paramPrimaryKey = "{{primaryKey}}";
        private const string paramConfigKey = "{{ConfigKey}}";
        private const string paraProjectName = "{{ProjectName}}";
        private const string NoKey = @" 
            if (env !=  ""Development"")
                builder.ToTable(""{{TableName}}"", SystemConstant.SchemaAppSaleProduct).HasNoKey();
            else
                builder.ToTable(""{{TableName}}"", SystemConstant.SchemaAppSale).HasNoKey();";
        private const string KeyConfig = @"
            if (env != ""Development"")
                builder.ToTable(""{{TableName}}"", SystemConstant.SchemaAppSaleProduct).HasKey(x => new { {{primaryKey}} });
            else
                builder.ToTable(""{{TableName}}"", SystemConstant.SchemaAppSale).HasKey(x => new { {{primaryKey}} });";
        private const string template = @"
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using {{ProjectName}};

namespace Infrastructure.Configurations
{
    public class {{TableName}}Configuration : IEntityTypeConfiguration<{{TableName}}>
    {
        public void Configure(EntityTypeBuilder<{{TableName}}> builder)
        {
            {{TableConfig}}
            var env = Environment.GetEnvironmentVariable(""ASPNETCORE_ENVIRONMENT"");
            {{ConfigKey}}
        }
    }
}";

        public static void GenConfigTemplate(string TableName, List<DefineColumn> DefineColumns, List<string> Primarykey, string pathFolderConfig)
        {
            string result = template.Replace(paramTableName,TableName);
            string contentTableConfig = "";
            string baseDirectory = pathFolderConfig;

            // Specify the file name you want to create
            string fileName = $"{TableName}Configuration.cs";

            // Combine the base directory with the file name to get the full file path
            string filePath = Path.Combine(baseDirectory, fileName);
            foreach (var column in DefineColumns)
            {
                string fieldConfig = $"builder.Property(x => x.{column.ColumnName}).HasColumnName(\"{column.ColumnName}\");" + Environment.NewLine;
                contentTableConfig += fieldConfig;
            }
            var temKey = KeyConfig.Replace(paramTableName, TableName);
            if (Primarykey != null && Primarykey.Count > 0)
            {
                string listKey = "";
                int count = Primarykey.Count();
                for (int i = 0; i <= count - 1; i++)
                {
                    if (i == count - 1)
                        listKey += $"x.{Primarykey[i]}";
                    else
                        listKey += $"x.{Primarykey[i]},";
                }
                temKey = temKey.Replace(paramPrimaryKey, listKey);
            }
            else
            {
                temKey = NoKey.Replace(paramTableName, TableName);
            }
            result = result.Replace(paramTableConfig, contentTableConfig).Replace(paramConfigKey, temKey).Replace(paraProjectName,Enum.ProjectName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllText(filePath, result);

        }


    }
}
