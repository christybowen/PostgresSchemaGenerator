using System;
using System.Collections.Generic;
using Npgsql;
using ATShared;

namespace PostgresSchemaGenerator.src.Library
{
    /// <summary>
    /// This class handles the interpretation of the schema for a given table.
    /// </summary>
    public class SchemaInterpreter : DynamicSchemaInterpreter
    {
        public String modelPrintString;
        public String controllerPrintString;
        public string routesPrintString;
        private string sqlConnString = "host=sand5;Username=cbowen;Database=payledger";

        public SchemaInterpreter(NpgsqlCommand cmd, String tableName, List<ATShared.SchemaEntry> exclusionList, List<ATShared.SchemaEntry> schemaCols)
            :base(cmd, tableName, exclusionList, schemaCols)
        {
            this.prepareSchema();
            this.baseQuery = generateQuery();
        }

        /// <summary>
        /// Creates the Model Class code based on the given schema from PostGres.
        /// </summary>
        public void createModelString()
        {
            var fileString = "using System;\n";
            fileString += "using System.Collections.Generic;\n";
            fileString += "using System.ComponentModel.DataAnnotations;\n";
            fileString += "using LinqToDB.Mapping;\n";
            fileString += "using System.Data.Common;\n\n";

            fileString += "namespace ActionTargetOData.Models\n{\n";
            fileString += "    [Table(Name = \"" + this.schemaName + "." + viewName + "\")]\n";
            fileString += "    public class " + this.schemaName + "_" + viewName + "\n    {\n";
            fileString += "        #region Instance Properties\n\n";

            List<String> primaryKey = new List<String>();

            string instanceProperties = "";
            string readerSwitch = "";
            bool havePrimaryKey = false;

            for(int i = 0; i < this.infoSchemaColumns.Count; i++)
            {
                var col = this.infoSchemaColumns[i];

                var columnType = this.getType(col.ColumnType);

                if (col.Nullable && columnType != "Object" && columnType != "Guid" && columnType != "Byte[]" && columnType != "String")
                {
                    columnType += "?";
                }

                if (!havePrimaryKey && col.PrimaryKey)
                {
                    instanceProperties += "        [PrimaryKey, Identity]\n";

                    havePrimaryKey = true;
                }
                else
                {
                    instanceProperties += "        [Column(Name =\"" + col.ColumnName + "\"), NotNull]\n";
                }

                instanceProperties += "        public " + columnType + " " + col.ColumnName + " { get; set; }\n\n";

                var readerVar = getReader(this.getType(col.ColumnType), i);

                readerSwitch += "                    case " + i + ":\n";
                readerSwitch += "                        this." + col.ColumnName + " = isNull ? " + readerVar + ";\n";
                readerSwitch += "                        break;\n";

                if (col.PrimaryKey)
                {
                    primaryKey.Add(col.ColumnName);
                }
            }

            fileString += "        // ignored columns: ";

            for(int k = 0; k < exclusionList.Count - 1; k++)
            {
                fileString += exclusionList[k].ColumnName + ", ";
            }

            if (exclusionList.Count > 0)
            {
                fileString += exclusionList[exclusionList.Count - 1].ColumnName + "\n";
            }

            fileString += "\n        public List<String> primaryKeys = new List<String>() { ";

            for(int j = 0; j < primaryKey.Count - 1; j++)
            {
                fileString += "\"" + primaryKey[j] + "\", ";
            }

            if (primaryKey.Count == 0)
            {
                fileString += " };\n\n";
            }
            else
            {
                fileString += "\"" + primaryKey[primaryKey.Count - 1] + "\" };\n\n";
            }

            fileString += "        public String baseQuery = \"" + baseQuery + "\";\n\n";

            fileString += instanceProperties;

            fileString += "        #endregion Instance Properties\n\n";
            fileString += "        #region Methods\n\n";
            fileString += "        public " + this.schemaName + "_" + viewName + "() {}\n\n";
            fileString += "        public " + this.schemaName + "_" + viewName + "(DbDataReader reader)\n";
            fileString += "        {\n";
            fileString += "            for(int i = 0; i < reader.FieldCount; i++)\n";
            fileString += "            {\n";
            fileString += "                var isNull = reader.IsDBNull(i);\n\n";
            fileString += "                switch(i)\n                {\n";

            fileString += readerSwitch;
            fileString += "                }\n";
            fileString += "            }\n";
            fileString += "        }\n\n";
            fileString += "        #endregion Methods\n\n";

            fileString += "    }\n";
            fileString += "}\n";

            this.modelPrintString = fileString;
        }

        /// <summary>
        /// Creates the Controller class code based on the created model
        /// </summary>
        public void createControllerString()
        {
            #region Using statements

            controllerPrintString += "using System;\n";
            controllerPrintString += "using System.Collections.Generic;\n";
            controllerPrintString += "using System.Data;\n";
            controllerPrintString += "using System.Linq;\n";
            controllerPrintString += "using System.Net;\n";
            controllerPrintString += "using System.Net.Http;\n";
            controllerPrintString += "using System.Web.Http;\n";
            controllerPrintString += "using System.Web.ModelBinding;\n";
            controllerPrintString += "using System.Web.OData;\n";
            controllerPrintString += "using System.Web.OData.Query;\n";
            controllerPrintString += "using System.Web.OData.Routing;\n";
            controllerPrintString += "using ActionTargetOData.Models;\n";
            controllerPrintString += "using Microsoft.OData.Core;\n";
            controllerPrintString += "using Npgsql;\n\n";

            #endregion Using statements

            controllerPrintString += "namespace ActionTargetOData.Controllers\n{\n";

            controllerPrintString += "    public class " + this.schemaName + "_" + this.viewName + "Controller : ODataController\n";
            controllerPrintString += "    {\n";

            controllerPrintString += "        private static ODataValidationSettings _validationSettings = new ODataValidationSettings();\n\n";

            #region GetAllEntries

            controllerPrintString += "        public IHttpActionResult Get" + this.schemaName + "_" + this.viewName + "s(ODataQueryOptions<"
                                    + this.schemaName + "_" + this.viewName + "> queryOptions)\n";
            controllerPrintString += GetEntryString("\"SELECT * FROM " + this.schemaName + "." + this.viewName + "\";");

            #endregion GetAllEntries

            #region GetSpecificEntry

            controllerPrintString += "        public IHttpActionResult Get" + this.schemaName + "_" + this.viewName + "([FromODataUri] "
                        + this.getType(this.infoSchemaColumns[0].ColumnType) + " key, ODataQueryOptions<"
                        + this.schemaName + "_" + this.viewName + "> queryOptions)\n";

            controllerPrintString += GetEntryString("\"SELECT * FROM " + this.schemaName + "." + this.viewName + " WHERE "
                                                            + this.infoSchemaColumns[0].ColumnName + " = \" + key;");

            #endregion GetSpecificEntry

            controllerPrintString += "    }\n}\n"; // end of controller class and namespace
        }

        private string GetEntryString(string command)
        {
            string text = "";

            text += "        {\n";

            text += "            List<" + this.schemaName + "_" + this.viewName + "> modelList = new List<" 
                + this.schemaName + "_" + this.viewName + ">();\n\n";

            text += "            using (var conn = new NpgsqlConnection(\"" + sqlConnString + "\"))\n";
            text += "            {\n";
            text += "                try {\n                    conn.Open();\n";
            text += "                } catch (Exception ex)\n                {\n";
            text += "                    System.Diagnostics.Debug.WriteLine(\"ERROR::\");\n";
            text += "                    System.Diagnostics.Debug.Write(ex.Message);\n                }\n\n";

            text += "                if (conn.State == ConnectionState.Closed)\n";
            text += "                {\n";
            text += "                    return StatusCode(HttpStatusCode.InternalServerError);\n";
            text += "                }\n\n";

            text += "                using (var cmd = new NpgsqlCommand())\n";
            text += "                {\n";
            text += "                    cmd.Connection = conn;\n";
            text += "                    cmd.CommandText = " + command + "\n\n";

            text += "                    try {\n";
            text += "                        using (var reader = cmd.ExecuteReader())\n";
            text += "                        {\n";

            text += "                            while (reader.Read())\n";
            text += "                            {\n";
            text += "                                " + this.schemaName + "_" + this.viewName + " temp = new " + this.schemaName + "_" + this.viewName + "(reader);\n";

            text += "                                modelList.Add(temp);\n";
            text += "                            }\n";
            text += "                        }\n";
            text += "                    } catch (Exception e)\n";
            text += "                    {\n";
            text += "                        System.Diagnostics.Debug.WriteLine(e.Message);\n\n";
            text += "                        return StatusCode(HttpStatusCode.InternalServerError);\n";
            text += "                    }\n";
            text += "                }\n\n";

            text += "                conn.Close();\n";
            text += "                return Ok<IEnumerable<" + this.schemaName + "_" + this.viewName + ">>(modelList);\n";
            text += "            }\n";
            text += "        }\n\n"; // end of method

            return text;
        }

        /// <summary>
        /// Writes the Schema currently stored by the object to a file.
        /// </summary>
        /// <param name="fileFolder">The file location.</param>
        public void saveModelToFile(String fileFolder)
        {
            fileFolder += this.schemaName + "_" + this.viewName + ".cs";
            System.IO.File.WriteAllText(fileFolder, this.modelPrintString);
        }

        /// <summary>
        /// Writes the Schema currently stored by the object to a file.
        /// </summary>
        /// <param name="fileFolder">The file location.</param>
        public void saveControllerToFile(String fileFolder)
        {
            fileFolder += this.schemaName + "_" + this.viewName + "Controller.cs";
            System.IO.File.WriteAllText(fileFolder, this.controllerPrintString);
        }

        /// <summary>
        /// Creates the Routes code based on the created model
        /// </summary>
        public void GetRoutesString()
        {
            routesPrintString = "            // view-start: " + this.viewName + "\n\n"
                + "            EntityTypeConfiguration<" + this.schemaName + "_" + this.viewName + "> "
                + this.schemaName + "_" + this.viewName + "Type = builder.EntityType<" + this.schemaName + "_" + this.viewName + ">();\n";

            bool havePrimaryKey = false;

            for (int i = 0; i < this.infoSchemaColumns.Count; i++)
            {
                var col = this.infoSchemaColumns[i];

                if (!havePrimaryKey && col.PrimaryKey)
                {
                    routesPrintString += "            " + this.schemaName + "_" + this.viewName + "Type.HasKey(a => a." + col.ColumnName + ");\n";

                    havePrimaryKey = true;
                }
                else
                {
                    routesPrintString += "            " + this.schemaName + "_" + this.viewName + "Type.Property(a => a." + col.ColumnName + ");\n";
                }
            }

            routesPrintString += "            builder.EntitySet<" + this.schemaName + "_" + this.viewName + ">(\"" + this.schemaName + "_" + this.viewName + "\");\n\n";
            routesPrintString += "            // view-end: " + this.viewName + "\n\n";
        }

        private String getReader(String typeOf, int indexNum)
        {
            // Determine what command to read.
            switch (typeOf)
            {
                case "Int64":
                    return " 0 : reader.GetInt64(" + indexNum + ")";
                case "Byte[]":
                    return "null : ((byte[])reader[" +indexNum + "])";
                case "Boolean":
                    return "false : reader.GetBoolean(" + indexNum + ")";
                case "String":
                case "IPAddress":
                case "varchar":
                    return "\"\" : reader.GetString(" + indexNum + ")";
                case "DateTime":
                    return "null : reader.GetDateTime(" + indexNum + ")";
                case "Decimal":
                case "Single":
                    return "0.0 : reader.GetFloat(" + indexNum + ")";
                case "Int32":
                    return "0 : reader.GetInt32(" + indexNum + ")";
                case "Double":
                    return "0.0 : reader.GetDouble(" + indexNum + ")";
                case "Int16":
                    return "0 : reader.GetInt16(" + indexNum + ")";
                case "TimeSpan":
                case "DateTimeOffset":
                    return "null : reader.GetTimeSpan(" + indexNum + ")";
                case "Byte":
                    return "null : reader.GetByte(" + indexNum + ")";
                case "Guid":
                    return "null : reader.GetGuid(" + indexNum + ")";
                default:
                    return null;
            }
        }
    }
}
