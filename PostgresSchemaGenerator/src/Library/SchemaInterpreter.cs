using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace PostgresSchemaGenerator.src.Library
{
    class SchemaInterpreter
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

            // Do what you need to with the schema that was pulled back.
        }
    }
}
