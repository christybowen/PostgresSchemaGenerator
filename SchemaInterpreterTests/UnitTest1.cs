using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PostgresSchemaGenerator.src.Library;
using ATShared;
using System.Collections.Generic;
using Npgsql;

namespace SchemaInterpreterTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GenerateQueryTest()
        {
            ATShared.SchemaEntry entry1 = new ATShared.SchemaEntry();
            entry1.ColumnName = "oper";
            entry1.ColumnType = "text";
            entry1.Nullable = true;
            entry1.PrimaryKey = true;

            ATShared.SchemaEntry entry2 = new ATShared.SchemaEntry();
            entry2.ColumnName = "seq";
            entry2.ColumnType = "integer";
            entry2.Nullable = true;
            entry2.PrimaryKey = true;

            ATShared.SchemaEntry entry3 = new ATShared.SchemaEntry();
            entry3.ColumnName = "display";
            entry3.ColumnType = "text";
            entry3.Nullable = true;
            entry3.PrimaryKey = false;

            ATShared.SchemaEntry entry4 = new ATShared.SchemaEntry();
            entry4.ColumnName = "descr";
            entry4.ColumnType = "text";
            entry4.Nullable = true;
            entry4.PrimaryKey = false;

            ATShared.SchemaEntry entry5 = new ATShared.SchemaEntry();
            entry5.ColumnName = "xmin";
            entry5.ColumnType = "oid";
            entry5.Nullable = true;
            entry5.PrimaryKey = false;

            ATShared.SchemaEntry entry6 = new ATShared.SchemaEntry();
            entry6.ColumnName = "tableoid";
            entry6.ColumnType = "oid";
            entry6.Nullable = true;
            entry6.PrimaryKey = false;

            ATShared.SchemaEntry entry7 = new ATShared.SchemaEntry();
            entry7.ColumnName = "ctid";
            entry7.ColumnType = "oid";
            entry7.Nullable = true;
            entry7.PrimaryKey = false;

            ATShared.SchemaEntry entry8 = new ATShared.SchemaEntry();
            entry8.ColumnName = "oid";
            entry8.ColumnType = "oid";
            entry8.Nullable = true;
            entry8.PrimaryKey = false;

            List<ATShared.SchemaEntry> includedList = new List<ATShared.SchemaEntry>();
            includedList.Add(entry1);
            includedList.Add(entry2);
            includedList.Add(entry3);
            includedList.Add(entry4);

            List<ATShared.SchemaEntry> excludedList = new List<ATShared.SchemaEntry>();
            excludedList.Add(entry5);
            excludedList.Add(entry6);
            excludedList.Add(entry7);
            excludedList.Add(entry8);

            var cmd = new NpgsqlCommand();

            Mock<SchemaInterpreter> interpreter = new Mock<SchemaInterpreter>(cmd, "mat_work", excludedList, includedList);

            SchemaInterpreter mockObject = interpreter.Object;

            mockObject.generateQuery();
        }
    }
}
