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

                // Start SQL command
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    // Here is where the SchemaInterpreter is created.

                    SchemaInterpreter schema = new SchemaInterpreter(cmd);
                    schema.pullSchema("rp_v_ppa");
                    //schema.pullView("ap_aging_summary");

                }
            }
            Console.ReadKey();
        }
    }
}
