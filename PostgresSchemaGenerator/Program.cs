using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using Npgsql;

namespace PostgresSchemaGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var conn = new NpgsqlConnection("host=sand5;Username=cbowen;Database=payledger"))
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
                    cmd.CommandText = "select table_name from INFORMATION_SCHEMA.views where table_schema = ANY (current_schemas(false))";

                    try
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) {
                                for (int i = 0; i < reader.FieldCount; i++) {
                                    tableNames.Add(reader.GetString(i));
                                    Console.Write(reader.GetString(i) + ' ');
                                }
                                Console.WriteLine();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);

                        return;
                    }
                }

                using (var cmd = new NpgsqlCommand())
                {
                    for (int i = 0; i < tableNames.Count; i++)
                    {
                        cmd.CommandText = "select column_name from INFORMATION_SCHEMA.columns where table_name = " + tableNames[i];

                        try
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Console.Write(tableNames[i] + "  columns: ");
                                    for (int j = 0; j < reader.FieldCount; j++)
                                    {
                                        Console.Write(reader.GetString(j) + ' ');
                                    }
                                    Console.WriteLine();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine(e.Message);

                            return;
                        }
                    }
                }

                conn.Close();
            }
            Console.ReadKey();
        }
    }
}
