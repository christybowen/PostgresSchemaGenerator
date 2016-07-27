using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace PostgresSchemaGenerator.src.Library
{
    /// <summary>
    /// This class handles the interpretation of the schema for a given table.
    /// </summary>
    public class SchemaInterpreter
    {
        /// <summary>
        /// The handle to the PostGres connection.
        /// </summary>
        private NpgsqlCommand sqlHandle;
        private List<SchemaEntry> infoSchemaColumns;
        private String printString;
        private String viewName;

        private List<String> exclusionList;

        /// <summary>
        /// The name of the table to get the schema for.
        /// </summary>
        private String tableName = null;

        /// <summary>
        /// The iteratable list of strings that make up the c sharp class.
        /// </summary>
        private List<String> cFile;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cmd">The connection handle.</param>
        public SchemaInterpreter(NpgsqlCommand cmd)
        {
            this.sqlHandle = cmd;
        }

        /// <summary>
        /// Pulls the information about the schema from the database for the specified table/view.
        /// </summary>
        /// <param name="viewName">The table or view to pull information on.</param>
        public void pullSchema(String viewName)
        {
            try
            {
                this.sqlHandle.CommandText = "select cdt_col, type, is_pkey, nonull from wm.column_data where cdt_tab = '" + viewName + "'";
                this.viewName = viewName;

                using (var reader = this.sqlHandle.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SchemaEntry currentLine = new SchemaEntry();
                        // The column name
                        currentLine.ColumnName = reader.GetString(0);

                        // The data type for the column
                        currentLine.ColumnType = reader.GetString(1);

                        // Whether the column is a pkey
                        currentLine.PrimaryKey = reader.GetBoolean(2);

                        // Whether the column is nullable
                        currentLine.Nullable = reader.GetBoolean(3);

                        // This is a List of strings that coorelate to the previous items.
                        this.infoSchemaColumns.Add(currentLine);

                        //Console.WriteLine(currentLine[0] + " " + currentLine[1] + " " + currentLine[2]);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (this.infoSchemaColumns == null)
            {
                throw new Exception("No data was returned for this request.");
            }
        }

        /// <summary>
        /// Creates the Model Class code based on the given schema from PostGres.
        /// </summary>
        public void createModelString()
        {
            var fileString = "using System;\n";
            fileString += "using System.ComponentModel.DataAnnotations;\n";
            fileString += "using LinqToDB.Mapping;\n\n";

            fileString += "namespace ActionTargetOData.Models\n{\n";
            fileString += "    [Table(Name = \"" + viewName + "\")]\n";
            fileString += "    public class " + viewName + "\n    {\n";
            fileString += "        #region Instance Properties\n\n";

            List<String> primaryKey = new List<String>();

            string instanceProperties = "";

            for(int i = 0; i < this.infoSchemaColumns.Count; i++)
            {
                var col = this.infoSchemaColumns[i];

                if(i == 0)
                {
                    instanceProperties += "        [PrimaryKey, Identity]\n";
                }
                else
                {
                    instanceProperties += "        [Column(Name =\"" + col.ColumnName + "\"), NotNull]\n";
                }

                var columnType = "";

                switch (col.ColumnType)
                {
                    case "bigint":
                    case "int8":
                        columnType = "Int64";
                        break;
                    case "binary":
                    case "image":
                    case "varbinary":
                    case "bytea":
                        columnType = "Byte[]";
                        break;
                    case "bit":
                    case "boolean":
                    case "bool":
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
                    case "audit_type":
                    case "bpchar":
                    case "yes_or_no":
                        columnType = "String";
                        break;
                    case "date":
                    case "datetime":
                    case "datetime2":
                    case "smalldatetime":
                    case "timestamp":
                    case "timetz":
                    case "timestamptz":
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
                    case "float4":
                        columnType = "Single";
                        break;
                    case "int":
                    case "integer":
                    case "int4":
                    case "cardinal":
                        columnType = "Int32";
                        break;
                    case "real":
                    case "double precision":
                    case "float8":
                        columnType = "Double";
                        break;
                    case "smallint":
                    case "int2":
                        columnType = "Int16";
                        break;
                    case "interval":
                    case "time":
                        columnType = "TimeSpan";
                        break;
                    case "tinyint":
                        columnType = "Byte";
                        break;
                    case "uniqueidentifier":
                    case "uuid":
                        columnType = "Guid";
                        break;
                    case "inet":
                        columnType = "IPAddress";
                        break;
                    default:
                        if(exclusionList == null)
                        {
                            exclusionList = new List<String>();
                        }
                        addToExclusion(col.ColumnName);
                        break;
                }

                if (col.Nullable && columnType != "Object" && columnType != "Guid" && columnType != "Byte[]" && columnType != "String")
                {
                    columnType += "?";
                }

                instanceProperties += "        public " + columnType + " " + col.ColumnName + " { get; set; }\n\n";

                if(col.PrimaryKey)
                {
                    primaryKey.Add(col.ColumnName);
                }
            }

            fileString += "// ignored columns: ";

            for(int k = 0; k < exclusionList.Count - 1; k++)
            {
                fileString += exclusionList[k] + ", ";
            }

            fileString += exclusionList[exclusionList.Count] + "\n\n";

            fileString += "\n public List<String> primaryKeys = new List<String>() {";

            for(int j = 0; j < primaryKey.Count - 1; j++)
            {
                fileString += primaryKey[j] + ", ";
            }

            fileString += primaryKey[primaryKey.Count] + "};\n\n";

            fileString += instanceProperties;

            fileString += "        #endregion Instance Properties\n";
            fileString += "    }\n";
            fileString += "}\n";

            this.printString = fileString;
        }

        /// <summary>
        /// Adds a column name to an exclusion list so they don't get pulled when performing queries or parsing data.
        /// </summary>
        /// <param name="ColumnName">The name of the column to exclude from queries.</param>
        private void addToExclusion(String ColumnName)
        {
            exclusionList.Add(ColumnName);
        }

        /// <summary>
        /// Generates a query for the given table to pull back all entries.
        /// </summary>
        public void generateQuery()
        {
            String query = "SELECT";

            for (int i = 0; i < this.infoSchemaColumns.Count(); i++)
            {
                query += this.infoSchemaColumns.ElementAt(i);
                if (i < this.infoSchemaColumns.Count())
                {
                    query += ",";
                }
                query += " ";
            }

            query += " FROM ";
            query += tableName;
        }

        /// <summary>
        /// Writes the Schema currently stored by the object to a file.
        /// </summary>
        /// <param name="fileFolder">The file location.</param>
        public void saveToFile(String fileFolder)
        {
            fileFolder += this.viewName + ".cs";
            System.IO.File.WriteAllText(fileFolder, this.printString);
        }
    }
}
