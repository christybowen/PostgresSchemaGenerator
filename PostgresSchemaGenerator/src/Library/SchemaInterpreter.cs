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
    class SchemaInterpreter
    {
        /// <summary>
        /// The handle to the PostGres connection.
        /// </summary>
        private NpgsqlCommand sqlHandle;

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
            List<List<String>> output = new List<List<String>>();
            try
            {
                this.sqlHandle.CommandText = "select column_name, data_type, is_nullable from INFORMATION_SCHEMA.COLUMNS where table_name = '" + viewName + "'";
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
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            this.tableName = viewName;

            // Do what you need to with the schema that was pulled back.
        }

        /// <summary>
        /// Writes the Schema currently stored by the object to a file.
        /// </summary>
        /// <param name="location">The file location.</param>
        public void writeSchema(String location)
        {
            // TODO: Need to make a sanity check for if the file exists and delete it if it does.

            if (this.tableName != null)
            {
                System.IO.File.WriteAllLines(@location + this.tableName, this.cFile);
            }
        }
    }
}
