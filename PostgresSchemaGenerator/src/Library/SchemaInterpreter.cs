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

        public void pullView(String viewName)
        {
            List<String> output = new List<String>();
            try
            {
                this.sqlHandle.CommandText = "\\d+ " + viewName;
                using (var reader = this.sqlHandle.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        String currentLine = "";
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            currentLine += reader.GetString(i) + " ";
                        }
                        output.Add(currentLine);
                    }
                }
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

        }
    }
}
