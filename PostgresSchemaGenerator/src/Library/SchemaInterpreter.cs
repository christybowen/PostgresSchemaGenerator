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
        public String printString;

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
            fileString += "using System.Collection.Generic;\n";
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
                fileString += exclusionList[k] + ", ";
            }

            if (exclusionList.Count > 0)
            {
                fileString += exclusionList[exclusionList.Count - 1] + "\n";
            }

            fileString += "\n        public List<String> primaryKeys = new List<String>() {";

            for(int j = 0; j < primaryKey.Count - 1; j++)
            {
                fileString += "\"" + primaryKey[j] + "\", ";
            }

            fileString += "\"" + primaryKey[primaryKey.Count - 1] + "\"};\n\n";

            fileString += "        public String baseQuery = \"" + baseQuery + "\"\n\n";

            fileString += instanceProperties;

            fileString += "        #endregion Instance Properties\n";
            fileString += "    }\n";
            fileString += "}\n";

            this.printString = fileString;
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
