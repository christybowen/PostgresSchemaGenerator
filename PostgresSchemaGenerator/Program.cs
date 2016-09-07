using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using PostgresSchemaGenerator.src.Library;

namespace PostgresSchemaGenerator
{
    class Program
    {
        /*
        * The list of tables/views to generate models for.
        */
        static String[] resourceList = new String[] {
            "mat_work"
        };

        static void Main(string[] args)
        {
            using (var conn = new NpgsqlConnection("host=sand5;Username=cbowen;Database=payledger"))
            {
                Console.WriteLine("Connecting to database");

                conn.Open();

                // Make sure connection is open
                Console.WriteLine(conn.State);
                if (conn.State == ConnectionState.Closed)
                {
                    System.Diagnostics.Debug.WriteLine("Connection not open");
                    return;
                }

                List<string> tableNames = new List<string>();

                // Start SQL command
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;

                    for (int i = 0; i < resourceList.Length; i++)
                    {
                        SchemaInterpreter schema = new SchemaInterpreter(cmd, resourceList[i], new List<ATShared.SchemaEntry>(), new List<ATShared.SchemaEntry>());

                        schema.prepareSchema();
                        schema.createModelString();
                        schema.createControllerString();
                        schema.saveModelToFile("C:\\Users\\chris\\Desktop\\");
                        schema.saveControllerToFile("C:\\Users\\chris\\Desktop\\");
                    }
                }

                conn.Close();
            }
            Console.ReadKey();
        }
    }
}
