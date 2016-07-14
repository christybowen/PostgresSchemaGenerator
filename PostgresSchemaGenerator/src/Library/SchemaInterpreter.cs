﻿using System;
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
        private List<List<String>> infoSchemaColumns;
        private String printString;
        private String viewName;

        public SchemaInterpreter(NpgsqlCommand cmd)
        {
            this.sqlHandle = cmd;
        }

        public void pullSchema(String viewName)
        {
            List<List<String>> output = new List<List<String>>();
            try
            {
                this.sqlHandle.CommandText = "select column_name, data_type, is_nullable from INFORMATION_SCHEMA.COLUMNS where table_name = '" + viewName + "'";
                this.viewName = viewName;

                using (var reader = this.sqlHandle.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        List<String> currentLine = new List<String>();
                        // The column name
                        currentLine.Add(reader.GetString(0));

                        // The data type for the column
                        currentLine.Add(reader.GetString(1));

                        // Whether the column is nullable
                        currentLine.Add(reader.GetString(2));

                        // This is a List of strings that coorelate to the previous items.
                        output.Add(currentLine);

                        //Console.WriteLine(currentLine[0] + " " + currentLine[1] + " " + currentLine[2]);
                    }
                }

                this.infoSchemaColumns = output;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void pullMaterializedView()
        {

        }

        public void pullTable()
        {


            // Do what you need to with the schema that was pulled back.
        }

        public void createModelString()
        {
            var fileString = "using System;\n\n";

            fileString += "namespace PostgresTables\n{\n";
            fileString += "public class " + viewName + "\n{\n";
            fileString += "#region Instance Properties\n";

            foreach(var col in this.infoSchemaColumns)
            {
                var columnType = "";

                switch (col[1])
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
                    case "boolean":
                        columnType = "Boolean";
                        break;
                    case "char":
                    case "nchar":
                    case "ntext":
                    case "nvarchar":
                    case "text":
                    case "varchar":
                    case "character varying":
                    case "character":
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
                    case "integer":
                        columnType = "Int32";
                        break;
                    case "real":
                    case "double precision":
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

                if (col[2] == "YES" && columnType != "Object" && columnType != "Guid" && columnType != "Byte[]" && columnType != "String")
                {
                    columnType += "?";
                }

                fileString += "public " + columnType + " " + col[0] + " { get; set; }\n";
            }

            fileString += "#endregion Instance Properties\n";
            fileString += "}\n";
            fileString += "}\n";

            this.printString = fileString;

            //Console.WriteLine(fileString);
        }

        public void saveToFile(String fileFolder)
        {
            fileFolder += this.viewName + ".cs";
            System.IO.File.WriteAllText(fileFolder, this.printString);
        }
    }
}
