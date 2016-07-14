using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace PostgresSchemaGenerator.src.Library
{
    public class SchemaInterpreter
    {
        private NpgsqlCommand sqlHandle;

        public SchemaInterpreter(NpgsqlCommand cmd)
        {
            this.sqlHandle = cmd;
        }

        public void pullSchema(String viewName)
        {
            List<List<String>> output = new List<List<String>>();
            try
            {

                this.sqlHandle.CommandText = "select column_name, data_type, character_maximum_length from INFORMATION_SCHEMA.COLUMNS where table_name = '" + viewName + "'";

                using (var reader = this.sqlHandle.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        List<String> currentLine = new List<String>();
                        currentLine.Add(reader.GetString(0));
                        currentLine.Add(reader.GetString(1));

                        output.Add(currentLine);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            createModel(viewName, new List<ColumnData>());
        }

        public void pullMaterializedView()
        {

        }

        public void pullTable()
        {


            // Do what you need to with the schema that was pulled back.
        }

        public void createModel(string viewName, List<ColumnData> columns)
        {
            var fileString = "using System;\n \n";

            fileString += "namespace PostgresTables \n { \n";
            fileString += "public class " + viewName + "\n { \n";
            fileString += "#region Instance Properties \n";

            foreach(var col in columns)
            {
                var columnType = "";

                switch (col.ColumnType)
                {
                    case "bigint":
                        columnType = "Int64";
                        break;
                    case "binary":
                    case "image":
                    case "varbinary":
                        columnType = "Byte[]";
                        break;
                    case "bit":
                        columnType = "Boolean";
                        break;
                    case "char":
                    case "nchar":
                    case "ntext":
                    case "nvarchar":
                    case "text":
                    case "varchar":
                        columnType = "String";
                        break;
                    case "date":
                    case "datetime":
                    case "datetime2":
                    case "smalldatetime":
                    case "timestamp":
                        columnType = "DateTime";
                        break;
                    case "datetimeoffset":
                        columnType = "DateTimeOffset";
                        break;
                    case "decimal":
                    case "money":
                    case "numeric":
                    case "smallmoney":
                        columnType = "Decimal";
                        break;
                    case "float":
                        columnType = "Single";
                        break;
                    case "int":
                        columnType = "Int32";
                        break;
                    case "real":
                        columnType = "Double";
                        break;
                    case "smallint":
                        columnType = "Int16";
                        break;
                    case "time":
                        columnType = "TimeSpan";
                        break;
                    case "tinyint":
                        columnType = "Byte";
                        break;
                    case "uniqueidentifier":
                        columnType = "Guid";
                        break;
                    default:
                        columnType = "Object";
                        break;

                }

                if (col.IsNullable == "YES")
                {
                    columnType += "?";
                }

                fileString += "public " + columnType + " " + col.ColumnName + " { get; set; } \n";
            }

            fileString += "#endregion Instance Properties \n";
            fileString += "} \n";
            fileString += "} \n";

            // feed fileString to the print function
        }
    }
}
