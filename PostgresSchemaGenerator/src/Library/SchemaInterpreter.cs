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
        private string sqlConnString = "host=sand5;Username=cbowen;Database=payledger";

        public SchemaInterpreter(NpgsqlCommand cmd, String tableName, List<ATShared.SchemaEntry> exclusionList, List<ATShared.SchemaEntry> schemaCols)
            :base(cmd, tableName, exclusionList, schemaCols)
        {

        }

        /// <summary>
        /// Creates the Model Class code based on the given schema from PostGres.
        /// </summary>
        public void createModelString()
        {
            var fileString = "using System;\n";
            fileString += "using System.Collections.Generic;\n";
            fileString += "using System.ComponentModel.DataAnnotations;\n";
            fileString += "using LinqToDB.Mapping;\n\n";

            fileString += "namespace ActionTargetOData.Models\n{\n";
            fileString += "    [Table(Name = \"" + viewName + "\")]\n";
            fileString += "    public class " + viewName + "\n    {\n";
            fileString += "        #region Instance Properties\n\n";

            List<String> primaryKey = new List<String>();

            string instanceProperties = "";
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

                if(col.PrimaryKey)
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

            fileString += "\n        public List<String> primaryKeys = new List<String>() {";

            for(int j = 0; j < primaryKey.Count - 1; j++)
            {
                fileString += "\"" + primaryKey[j] + "\", ";
            }

            fileString += "\"" + primaryKey[primaryKey.Count - 1] + "\"};\n\n";

            fileString += "        public String baseQuery = \"" + baseQuery + "\";\n\n";

            fileString += instanceProperties;

            fileString += "        #endregion Instance Properties\n";
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

            controllerPrintString += "    public class " + this.viewName + "Controller : ODataController\n";
            controllerPrintString += "    {\n";

            controllerPrintString += "        private static ODataValidationSettings _validationSettings = new ODataValidationSettings();\n\n";

            #region GetAllEntries

            controllerPrintString += "        public IHttpActionResult Get" + this.viewName + "s(ODataQueryOptions<"
                                    + this.viewName + "> queryOptions)\n";
            controllerPrintString += "        {\n";

            controllerPrintString += "            List<" + this.viewName + "> modelList = new List<" + this.viewName + ">();\n\n";

            controllerPrintString += "            using (var conn = new NpgsqlConnection(\"" + sqlConnString + "\"))\n";
            controllerPrintString += "            {\n";
            controllerPrintString += "                conn.Open();\n\n";

            controllerPrintString += "                if (conn.State == ConnectionState.Closed)\n";
            controllerPrintString += "                {\n";
            controllerPrintString += "                    return StatusCode(HttpStatusCode.InternalServerError);\n";
            controllerPrintString += "                }\n\n";

            controllerPrintString += "                using (var cmd = new NpgsqlCommand())\n";
            controllerPrintString += "                {\n";
            controllerPrintString += "                    cmd.Connection = conn;\n";
            controllerPrintString += "                    cmd.CommandText = \"SELECT * FROM " + this.viewName + "\";\n\n";

            controllerPrintString += "                    try {\n";
            controllerPrintString += "                        using (var reader = cmd.ExecuteReader())\n";
            controllerPrintString += "                        {\n";

            controllerPrintString += "                            while (reader.Read())\n";
            controllerPrintString += "                            {\n";
            controllerPrintString += "                                " + this.viewName + " temp = new " + this.viewName + "();\n";

            for (int i = 0; i < this.infoSchemaColumns.Count; i++)
            {
                if(i == 0)
                {
                    controllerPrintString += "                                var isNull = reader.IsDBNull(" + i + ");\n";
                }
                else
                {
                    controllerPrintString += "                                isNull = reader.IsDBNull(" + i + ");\n";
                }

                controllerPrintString += "                                if(!isNull){\n";

                var col = this.infoSchemaColumns[i];
                var readerVar = getReader(this.getType(col.ColumnType), i);

                controllerPrintString += "                                    temp." + col.ColumnName + " = " + readerVar + ";\n";
                controllerPrintString += "                                }\n";
                controllerPrintString += "                                else {\n";
                controllerPrintString += "                                    temp." + col.ColumnName + " = null;\n";
                controllerPrintString += "                                }\n";
            }

            controllerPrintString += "                                modelList.Add(temp);\n";
            controllerPrintString += "                            }\n";
            controllerPrintString += "                        }\n";
            controllerPrintString += "                    } catch (Exception e)\n";
            controllerPrintString += "                    {\n";
            controllerPrintString += "                        System.Diagnostics.Debug.WriteLine(e.Message);\n\n";
            controllerPrintString += "                        return StatusCode(HttpStatusCode.InternalServerError);\n";
            controllerPrintString += "                    }\n";
            controllerPrintString += "                }\n\n";

            controllerPrintString += "                conn.Close();\n";
            controllerPrintString += "                return Ok<IEnumerable<" + this.viewName + ">>(modelList);\n";
            controllerPrintString += "            }\n";
            controllerPrintString += "        }\n\n"; // end of method

            #endregion GetAllEntries

            #region GetSpecificEntry

            controllerPrintString += "        public IHttpActionResult Get" + this.viewName + "([FromODataUri] "
                                    + this.getType(this.infoSchemaColumns[0].ColumnType) + " key, ODataQueryOptions<"
                                    + this.viewName + "> queryOptions)\n";
            controllerPrintString += "        {\n";

            controllerPrintString += "            List<" + this.viewName + "> modelList = new List<" + this.viewName + ">();\n\n";

            controllerPrintString += "            using (var conn = new NpgsqlConnection(\"" + sqlConnString + "\"))\n";
            controllerPrintString += "            {\n";
            controllerPrintString += "                conn.Open();\n\n";

            controllerPrintString += "                if (conn.State == ConnectionState.Closed)\n";
            controllerPrintString += "                {\n";
            controllerPrintString += "                    return StatusCode(HttpStatusCode.InternalServerError);\n";
            controllerPrintString += "                }\n\n";

            controllerPrintString += "                using (var cmd = new NpgsqlCommand())\n";
            controllerPrintString += "                {\n";
            controllerPrintString += "                    cmd.Connection = conn;\n";
            controllerPrintString += "                    cmd.CommandText = \"SELECT * FROM " + this.viewName + " WHERE " 
                                                            + this.infoSchemaColumns[0].ColumnName + " = \" + key;\n\n";

            controllerPrintString += "                    try {\n";
            controllerPrintString += "                        using (var reader = cmd.ExecuteReader())\n";
            controllerPrintString += "                        {\n";

            controllerPrintString += "                            while (reader.Read())\n";
            controllerPrintString += "                            {\n";
            controllerPrintString += "                                " + this.viewName + " temp = new " + this.viewName + "();\n";

            for (int i = 0; i < this.infoSchemaColumns.Count; i++)
            {
                var col = this.infoSchemaColumns[i];
                var readerVar = getReader(this.getType(col.ColumnType), i);

                controllerPrintString += "                                temp." + col.ColumnName + " = " + readerVar + ";\n";
            }

            controllerPrintString += "                                modelList.Add(temp);\n";
            controllerPrintString += "                            }\n";
            controllerPrintString += "                        }\n";
            controllerPrintString += "                    } catch (Exception e)\n";
            controllerPrintString += "                    {\n";
            controllerPrintString += "                        System.Diagnostics.Debug.WriteLine(e.Message);\n\n";
            controllerPrintString += "                        return StatusCode(HttpStatusCode.InternalServerError);\n";
            controllerPrintString += "                    }\n";
            controllerPrintString += "                }\n\n";

            controllerPrintString += "                conn.Close();\n";
            controllerPrintString += "                return Ok<IEnumerable<" + this.viewName + ">>(modelList);\n";
            controllerPrintString += "            }\n";
            controllerPrintString += "        }\n\n"; // end of method

            #endregion GetSpecificEntry





            controllerPrintString += "    }\n}\n"; // end of controller class and namespace
        }

        /// <summary>
        /// Writes the Schema currently stored by the object to a file.
        /// </summary>
        /// <param name="fileFolder">The file location.</param>
        public void saveModelToFile(String fileFolder)
        {
            fileFolder += this.viewName + ".cs";
            System.IO.File.WriteAllText(fileFolder, this.modelPrintString);
        }

        /// <summary>
        /// Writes the Schema currently stored by the object to a file.
        /// </summary>
        /// <param name="fileFolder">The file location.</param>
        public void saveControllerToFile(String fileFolder)
        {
            fileFolder += this.viewName + "Controller.cs";
            System.IO.File.WriteAllText(fileFolder, this.controllerPrintString);
        }

        private String getReader(String typeOf, int indexNum)
        {
            // Determine what command to read.
            switch (typeOf)
            {
                case "Int64":
                    return "reader.GetInt64(" + indexNum + ")";
                case "Byte[]":
                    return "((byte[])reader[" +indexNum + "])";
                case "Boolean":
                    return "reader.GetBoolean(" + indexNum + ")";
                case "String":
                case "IPAddress":
                case "varchar":
                    return "reader.GetString(" + indexNum + ")";
                case "DateTime":
                    return "reader.GetDateTime(" + indexNum + ")";
                case "Decimal":
                case "Single":
                    return "reader.GetFloat(" + indexNum + ")";
                case "Int32":
                    return "reader.GetInt32(" + indexNum + ")";
                case "Double":
                    return "reader.GetDouble(" + indexNum + ")";
                case "Int16":
                    return "reader.GetInt16(" + indexNum + ")";
                case "TimeSpan":
                case "DateTimeOffset":
                    return "reader.GetTimeSpan(" + indexNum + ")";
                case "Byte":
                    return "reader.GetByte(" + indexNum + ")";
                case "Guid":
                    return "reader.GetGuid(" + indexNum + ")";
                default:
                    return null;
            }
        }
    }
}
