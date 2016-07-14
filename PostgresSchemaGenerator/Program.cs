using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
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
            "rp_v_ppa",
            "rp_v_ppp"
        };

        static void Main(string[] args)
        {
            using (var conn = new NpgsqlConnection("host=sand5;Username=jbennett;Database=payledger"))
            {
                conn.Open();

                // Make sure connection is open
                System.Diagnostics.Debug.WriteLine(conn.State);
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
                        SchemaInterpreter schema = new SchemaInterpreter(cmd);
                        
                        schema.pullSchema(resourceList[i]);
                        schema.createModelString();
                        schema.saveToFile("C:\\Users\\jason\\Desktop\\");
                    }
                }

                conn.Close();
            }
            Console.ReadKey();
        }
    }
}
