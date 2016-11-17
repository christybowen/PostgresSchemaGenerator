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
        static String[] blackList = new String[] {
            "mat_work", "ap_aging_summary"
        };

        static void Main(string[] args)
        {
            using (var conn = new NpgsqlConnection("host=sand6;Username=cbowen;Database=ledger"))
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

                // Start SQL command
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;

                    SaveRoutesToFile("C:\\Users\\chris\\Desktop\\WebApiConfig.cs", "            // schema-start: public\n\n");

                    for (int i = 0; i < blackList.Length; i++)
                    {
                        SchemaInterpreter schema = new SchemaInterpreter(cmd, blackList[i], new List<ATShared.SchemaEntry>(), new List<ATShared.SchemaEntry>());

                        schema.createModelString();
                        schema.createControllerString();
                        schema.GetRoutesString();
                        schema.saveModelToFile("C:\\Users\\chris\\Desktop\\");
                        schema.saveControllerToFile("C:\\Users\\chris\\Desktop\\");
                        SaveRoutesToFile("C:\\Users\\chris\\Desktop\\WebApiConfig.cs", schema.routesPrintString);
                    }

                    SaveRoutesToFile("C:\\Users\\chris\\Desktop\\WebApiConfig.cs", "            // schema-end: public\n\n");
                }

                conn.Close();

                Console.WriteLine("Closed Connection");
            }
            Console.ReadKey();
        }

        /// <summary>
        /// Saves the provided string to the file path provided. This
        /// has specific logic for the current structure of the WebApiConfig.cs
        /// file for the ActionTargetLogicLayer project
        /// </summary>
        /// <param name="filename">path to the file</param>
        /// <param name="newRoute">string to insert into the file</param>
        public static void SaveRoutesToFile(string filename, string newRoute)
        {
            string[] routesFileString = System.IO.File.ReadAllLines(filename);

            for(int i = 0; i < routesFileString.Length; i++)
            {
                if(i == 0)
                {
                    System.IO.File.WriteAllText(filename, "");
                }

                if(i == routesFileString.Length - 4)
                {
                    System.IO.File.AppendAllText(filename, newRoute);
                }

                System.IO.File.AppendAllText(filename, routesFileString[i] + "\n");
            }
        }
    }
}
