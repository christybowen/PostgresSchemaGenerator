using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PostgresSchemaGenerator.src.Library;
using ATShared;
using System.Collections.Generic;
using Npgsql;
using System.Text.RegularExpressions;

namespace SchemaInterpreterTests
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// Method to generate test included entries
        /// </summary>
        /// <returns>List of schema entries to include</returns>
        public List<ATShared.SchemaEntry> GetIncluded()
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

            List<ATShared.SchemaEntry> includedList = new List<ATShared.SchemaEntry>();
            includedList.Add(entry1);
            includedList.Add(entry2);
            includedList.Add(entry3);
            includedList.Add(entry4);

            return includedList;
        }

        /// <summary>
        /// Method to generate test excluded data
        /// </summary>
        /// <returns>List of excluded schedma entries</returns>
        public List<ATShared.SchemaEntry> GetExcluded()
        {
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

            List<ATShared.SchemaEntry> excludedList = new List<ATShared.SchemaEntry>();
            excludedList.Add(entry5);
            excludedList.Add(entry6);
            excludedList.Add(entry7);
            excludedList.Add(entry8);

            return excludedList;
        }

        [TestMethod]
        public void GenerateQueryTest()
        {
            List<ATShared.SchemaEntry> includedList = GetIncluded();

            List<ATShared.SchemaEntry> excludedList = GetExcluded();

            var cmd = new NpgsqlCommand();

            SchemaInterpreter mockInterpreter = new SchemaInterpreter(cmd, "mat_work", excludedList, includedList);

            var baseQuery = mockInterpreter.generateQuery();

            Assert.AreEqual("SELECT oper, seq, display, descr FROM public.mat_work", baseQuery);
        }

        [TestMethod]
        public void CreateStringTest()
        {
            List<ATShared.SchemaEntry> includedList = GetIncluded();
            List<ATShared.SchemaEntry> excludedList = GetExcluded();
            var cmd = new NpgsqlCommand();

            SchemaInterpreter mockInterpreter = new SchemaInterpreter(cmd, "mat_work", excludedList, includedList);

            mockInterpreter.createModelString();

            var printString = "using System;\nusing System.Collections.Generic;\n"
                            + "using System.ComponentModel.DataAnnotations;\nusing LinqToDB.Mapping;\n"
                            + "using System.Data.Common;\n\n"
                            + "namespace ActionTargetOData.Models\n{\n    [Table(Name = \"public.mat_work\")]\n"
                            + "    public class public_mat_work\n    {\n        #region Instance Properties\n\n"
                            + "        // ignored columns: xmin, tableoid, ctid, oid\n\n"
                            + "        public List<String> primaryKeys = new List<String>() { \"oper\", \"seq\" };\n\n"
                            + "        public String baseQuery = \"SELECT oper, seq, display, descr FROM public.mat_work\";\n\n"
                            + "        [PrimaryKey, Identity]\n        public String oper { get; set; }\n\n"
                            + "        [Column(Name =\"seq\"), NotNull]\n        public Int32? seq { get; set; }\n\n"
                            + "        [Column(Name =\"display\"), NotNull]\n        public String display { get; set; }\n\n"
                            + "        [Column(Name =\"descr\"), NotNull]\n        public String descr { get; set; }\n\n"
                            + "        #endregion Instance Properties\n\n"
                            + "        #region Methods\n\n        public public_mat_work() {}\n\n"
                            + "        public public_mat_work(DbDataReader reader)\n        {\n"
                            + "            for(int i = 0; i < reader.FieldCount; i++)\n            {\n"
                            + "                var isNull = reader.IsDBNull(i);\n\n                switch(i)\n                {\n"
                            + "                    case 0:\n                        this.oper = isNull ? \"\" : reader.GetString(0);\n"
                            + "                        break;\n"
                            + "                    case 1:\n                        this.seq = isNull ? 0 : reader.GetInt32(1);\n"
                            + "                        break;\n"
                            + "                    case 2:\n                        this.display = isNull ? \"\" : reader.GetString(2);\n"
                            + "                        break;\n"
                            + "                    case 3:\n                        this.descr = isNull ? \"\" : reader.GetString(3);\n"
                            + "                        break;\n"
                            + "                }\n" + "            }\n" + "        }\n\n" + "        #endregion Methods\n\n"
                            + "    }\n}\n";

            Assert.AreEqual(printString, mockInterpreter.modelPrintString);
        }

        [TestMethod]
        public void CreateControllerStringTest()
        {
            List<ATShared.SchemaEntry> includedList = GetIncluded();
            List<ATShared.SchemaEntry> excludedList = GetExcluded();
            var cmd = new NpgsqlCommand();

            SchemaInterpreter mockInterpreter = new SchemaInterpreter(cmd, "mat_work", excludedList, includedList);

            mockInterpreter.createControllerString();

            /*var printString = "using System;\nusing System.Collections.Generic;\nusing System.Data;\n"
                + "using System.Linq;\nusing System.Net;\nusing System.Net.Http;\nusing System.Web.Http;\n"
                + "using System.Web.ModelBinding;\nusing System.Web.OData;\nusing System.Web.OData.Query;\n"
                + "using System.Web.OData.Routing;\nusing ActionTargetOData.Models;\nusing Microsoft.OData.Core;\nusing Npgsql;\n\n"
                + "namespace ActionTargetOData.Controllers\n{\n    public class public_mat_workController : ODataController\n"
                + "    {\n        private static ODataValidationSettings _validationSettings = new ODataValidationSettings();\n\n"
                + "        public IHttpActionResult Getpublic_mat_works(ODataQueryOptions<public_mat_work> queryOptions)\n"
                + "        {\n            List<public_mat_work> modelList = new List<public_mat_work>();\n\n"
                + "            using (var conn = new NpgsqlConnection(\"host=sand5;Username=cbowen;Database=payledger\"))\n"
                + "            {\n                try {\n                    conn.Open();\n"
                + "                } catch (Exception ex)\n                {\n"
                + "                    System.Diagnostics.Debug.WriteLine(\"ERROR::\");\n"
                + "                    System.Diagnostics.Debug.Write(ex.Message);\n                }\n\n"
                + "                if (conn.State == ConnectionState.Closed)\n                {\n"
                + "                    return StatusCode(HttpStatusCode.InternalServerError);\n                }\n\n"
                + "                using (var cmd = new NpgsqlCommand())\n                {\n"
                + "                    cmd.Connection = conn;\n                    cmd.CommandText = \"SELECT * FROM public.mat_work\";\n\n"
                + "                    try {\n                        using (var reader = cmd.ExecuteReader())\n"
                + "                        {\n                            while (reader.Read())\n                            {\n"
                + "                                public_mat_work temp = new public_mat_work(reader);\n"
                + "                                modelList.Add(temp);\n                            }\n"
                + "                        }\n                    } catch (Exception e)\n"
                + "                    {\n                        System.Diagnostics.Debug.WriteLine(e.Message);\n\n"
                + "                        return StatusCode(HttpStatusCode.InternalServerError);\n                   }\n"
                + "                }\n\n                conn.Close();\n"
                + "                return Ok<IEnumerable<public_mat_work>>(modelList);\n            }\n        }\n\n"
                + "        public IHttpActionResult Getpublic_mat_work([FromODataUri] String key, ODataQueryOptions<public_mat_work> queryOptions)\n"
                + "        {\n            List<public_mat_work> modelList = new List<public_mat_work>();\n\n"
                + "            using (var conn = new NpgsqlConnection(\"host=sand5;Username=cbowen;Database=payledger\"))\n"
                + "            {\n                try {\n                    conn.Open();\n                } catch (Exception ex)\n"
                + "                {\n                    System.Diagnostics.Debug.WriteLine(\"ERROR::\");\n"
                + "                    System.Diagnostics.Debug.Write(ex.Message);\n                }\n\n"
                + "                if (conn.State == ConnectionState.Closed)\n                {\n"
                + "                    return StatusCode(HttpStatusCode.InternalServerError);\n                }\n\n"
                + "                using (var cmd = new NpgsqlCommand())\n                {\n                    cmd.Connection = conn;\n"
                + "                    cmd.CommandText = \"SELECT * FROM public.mat_work WHERE oper = \" + key;\n\n"
                + "                    try {\n                        using (var reader = cmd.ExecuteReader())\n"
                + "                        {\n                            while (reader.Read())\n                            {\n"
                + "                                public_mat_work temp = new public_mat_work(reader);\n"
                + "                                modelList.Add(temp);\n                            }\n"
                + "                        }\n                    } catch (Exception e)\n"
                + "                    {\n                        System.Diagnostics.Debug.WriteLine(e.Message);\n\n"
                + "                        return StatusCode(HttpStatusCode.InternalServerError);\n                    }\n"
                + "                }\n\n                conn.Close();\n                return Ok<IEnumerable<public_mat_work>>(modelList);\n"
                + "            }\n        }\n\n    }\n}\n";*/

            var printString = "using System;\nusing System.Collections.Generic;\nusing System.Data;\nusing System.Net;\n"
                + "using System.Web.Http;\nusing System.Web.OData;\nusing System.Web.OData.Query;\nusing ActionTargetOData.Models;\n"
                + "using Npgsql;\nusing System.Data.Common;\n\nnamespace ActionTargetOData.Controllers\n{\n"
                + "    public class public_mat_workController : ODataController\n    {\n"
                + "        private static ODataValidationSettings _validationSettings = new ODataValidationSettings();\n\n"
                + "        // GET: odata/public_mat_works\n        public IHttpActionResult Getpublic_mat_works(ODataQueryOptions<public_mat_work> queryOptions)\n"
                + "        {\n            using (var connection = new NpgsqlConnection(\"host=sand5;Username=cbowen;Database=payledger\"))\n"
                + "            {\n                return Connect(connection, \"SELECT * from public.mat_work\");\n            }\n        }\n\n"
                + "        // GET: odata/public_mat_work(5)\n        public IHttpActionResult Getpublic_mat_work([FromODataUri] String key, ODataQueryOptions<public_mat_work> queryOptions)\n"
                + "        {\n            using (var connection = new NpgsqlConnection(\"host=sand5;Username=cbowen;Database=payledger\"))\n"
                + "            {\n                return Connect(connection, \"SELECT * from public.mat_work WHERE oper = \" + key);\n            }\n        }\n\n"
                + "        public IHttpActionResult Connect(DbConnection connection, string query)\n        {\n            DbConnection conn;\n\n"
                + "            if (string.IsNullOrWhiteSpace(connection.ConnectionString))\n            {\n                conn = connection;\n            }\n"
                + "            else {\n                conn = new NpgsqlConnection(connection.ConnectionString);\n            }\n\n"
                + "            // validate the query.\n            try\n            {\n                conn.Open();\n            }\n"
                + "            catch (Exception ex)\n            {\n                System.Diagnostics.Debug.WriteLine(\"ERROR::\");\n"
                + "                System.Diagnostics.Debug.Write(ex.Message);\n            }\n\n"
                + "            // Make sure connection is open\n            if (conn.State == ConnectionState.Closed)\n            {\n"
                + "                return StatusCode(HttpStatusCode.InternalServerError);\n            }\n\n"
                + "            using (var command = new NpgsqlCommand())\n            {\n                if (string.IsNullOrWhiteSpace(conn.ConnectionString))\n"
                + "                {\n                    command.Connection = new NpgsqlConnection();\n                }\n"
                + "                else\n                {\n                    command.Connection = (NpgsqlConnection)conn;\n                }\n\n"
                + "                List<public_mat_work> modelList = new List<public_mat_work>();\n\n"
                + "                // Start SQL command\n                command.CommandText = query;\n\n"
                + "                try\n                {\n                    using (var reader = command.ExecuteReader())\n                    {\n"
                + "                        // Per Row\n                        while (reader.Read())\n                        {\n"
                + "                            public_mat_work temp = new public_mat_work(reader);\n                            modelList.Add(temp);\n"
                + "                        }\n                    }\n                }\n                catch (Exception e)\n"
                + "                {\n                    System.Diagnostics.Debug.WriteLine(e.Message);\n"
                + "                    return StatusCode(HttpStatusCode.InternalServerError);\n                }\n\n"
                + "                var result = Ok<IEnumerable<public_mat_work>>(modelList);\n\n                command.Connection.Close();\n"
                + "                conn.Close();\n\n                return result;\n            }\n        }\n    }\n}";

            var nowhiteSpace = Regex.Replace(printString, @"\s+", "");
            var interpreterNoWhiteSpace = Regex.Replace(mockInterpreter.controllerPrintString, @"\s+", "");

            Assert.AreEqual(printString, mockInterpreter.controllerPrintString);
            Assert.AreEqual(nowhiteSpace, interpreterNoWhiteSpace);
        }


        [TestMethod]
        public void CreateRouteStringTest()
        {
            List<ATShared.SchemaEntry> includedList = GetIncluded();
            List<ATShared.SchemaEntry> excludedList = GetExcluded();
            var cmd = new NpgsqlCommand();

            SchemaInterpreter mockInterpreter = new SchemaInterpreter(cmd, "mat_work", excludedList, includedList);

            mockInterpreter.GetRoutesString();

            var printString = "            // view-start: public_mat_work\n\n"
                + "            EntityTypeConfiguration<public_mat_work> public_mat_workType = builder.EntityType<public_mat_work>();\n"
                + "            public_mat_workType.HasKey(a => a.oper);\n"
                + "            public_mat_workType.Property(a => a.seq);\n"
                + "            public_mat_workType.Property(a => a.display);\n"
                + "            public_mat_workType.Property(a => a.descr);\n"
                + "            builder.EntitySet<public_mat_work>(\"public_mat_work\");\n\n"
                + "            // view-end: public_mat_work\n\n";

            Assert.AreEqual(printString, mockInterpreter.routesPrintString);
        }

        [TestMethod]
        public void CreateTestModelConstructorTest()
        {
            List<ATShared.SchemaEntry> includedList = GetIncluded();
            List<ATShared.SchemaEntry> excludedList = GetExcluded();
            var cmd = new NpgsqlCommand();

            SchemaInterpreter mockInterpreter = new SchemaInterpreter(cmd, "mat_work", excludedList, includedList);

            string actualString = mockInterpreter.createTestConstructorString();

            string printString = "        [TestMethod]\n        public void TestModelConstructor()\n        {\n"
                + "            Mock<DbDataReader> reader = MockReader.CreateMockedReaderRandom();\n"
                + "            public_mat_work model = new public_mat_work(reader.Object);\n\n"
                + "            Assert.IsNotNull(model);\n            Assert.IsNotNull(model.oper);\n"
                + "            Assert.IsInstanceOfType(model.oper, typeof(String));\n\n"
                + "            Assert.IsNotNull(model.seq);\n            Assert.IsInstanceOfType(model.seq, typeof(Int32));\n\n"
                + "            Assert.IsNotNull(model.display);\n            Assert.IsInstanceOfType(model.display, typeof(String));\n\n"
                + "            Assert.IsNotNull(model.descr);\n            Assert.IsInstanceOfType(model.descr, typeof(String));\n\n"
                + "        }\n\n";

            Assert.AreEqual(printString, actualString);
        }

        [TestMethod]
        public void CreateTestModelNullTest()
        {
            List<ATShared.SchemaEntry> includedList = GetIncluded();
            List<ATShared.SchemaEntry> excludedList = GetExcluded();
            var cmd = new NpgsqlCommand();

            SchemaInterpreter mockInterpreter = new SchemaInterpreter(cmd, "mat_work", excludedList, includedList);

            string actualString = mockInterpreter.createTestNullString();

            string printString = "        [TestMethod]\n        public void TestModelConstructor()\n        {\n"
                + "            Mock<DbDataReader> reader = MockReader.CreateMockedReaderRandom();\n"
                + "            public_mat_work model = new public_mat_work(reader.Object);\n\n"
                + "            Assert.IsNotNull(model);\n            Assert.IsNotNull(model.oper);\n"
                + "            Assert.IsInstanceOfType(model.oper, typeof(String));\n"
                + "            Assert.AreEqual(\"\", model.oper);\n\n"
                + "            Assert.IsNotNull(model.seq);\n            Assert.IsInstanceOfType(model.seq, typeof(Int32));\n"
                + "            Assert.AreEqual(0, model.seq);\n\n"
                + "            Assert.IsNotNull(model.display);\n            Assert.IsInstanceOfType(model.display, typeof(String));\n"
                + "            Assert.AreEqual(\"\", model.display);\n\n"
                + "            Assert.IsNotNull(model.descr);\n            Assert.IsInstanceOfType(model.descr, typeof(String));\n"
                + "            Assert.AreEqual(\"\", model.descr);\n\n"
                + "        }\n\n";

            Assert.AreEqual(printString, actualString);
        }

        [TestMethod]
        public void CreateTestModelStaticTest()
        {
            List<ATShared.SchemaEntry> includedList = GetIncluded();
            List<ATShared.SchemaEntry> excludedList = GetExcluded();
            var cmd = new NpgsqlCommand();

            SchemaInterpreter mockInterpreter = new SchemaInterpreter(cmd, "mat_work", excludedList, includedList);

            string actualString = mockInterpreter.createTestStaticString();

            string printString = "        [TestMethod]\n        public void TestModelConstructor()\n        {\n"
                + "            Mock<DbDataReader> reader = MockReader.CreateMockedReaderRandom();\n"
                + "            public_mat_work model = new public_mat_work(reader.Object);\n\n"
                + "            Assert.IsNotNull(model);\n            Assert.IsNotNull(model.oper);\n"
                + "            Assert.IsInstanceOfType(model.oper, typeof(String));\n"
                + "            Assert.AreEqual(\"String Placeholder\", model.oper);\n\n"
                + "            Assert.IsNotNull(model.seq);\n            Assert.IsInstanceOfType(model.seq, typeof(Int32));\n"
                + "            Assert.AreEqual(83726, model.seq);\n\n"
                + "            Assert.IsNotNull(model.display);\n            Assert.IsInstanceOfType(model.display, typeof(String));\n"
                + "            Assert.AreEqual(\"String Placeholder\", model.display);\n\n"
                + "            Assert.IsNotNull(model.descr);\n            Assert.IsInstanceOfType(model.descr, typeof(String));\n"
                + "            Assert.AreEqual(\"String Placeholder\", model.descr);\n\n"
                + "        }\n\n";

            Assert.AreEqual(printString, actualString);
        }

        [TestMethod]
        public void CreateModelTestClassTest()
        {
            List<ATShared.SchemaEntry> includedList = GetIncluded();
            List<ATShared.SchemaEntry> excludedList = GetExcluded();
            var cmd = new NpgsqlCommand();

            SchemaInterpreter mockInterpreter = new SchemaInterpreter(cmd, "mat_work", excludedList, includedList);

            mockInterpreter.createModelTestString();

            string printString = "using System.ComponentModel;\n"
                + "using System.Collections.Generic;\n"
                + "using Moq;\n"
                + "using System.Data.Common;\n\n"
                + "namespace ODataUnitTests\n{\n    [TestClass]\n"
                + "    public class public_mat_workModelTests\n    {\n"
                + "        [TestMethod]\n        public void TestModelConstructor()\n        {\n"
                + "            Mock<DbDataReader> reader = MockReader.CreateMockedReaderRandom();\n"
                + "            public_mat_work model = new public_mat_work(reader.Object);\n\n"
                + "            Assert.IsNotNull(model);\n            Assert.IsNotNull(model.oper);\n"
                + "            Assert.IsInstanceOfType(model.oper, typeof(String));\n\n"
                + "            Assert.IsNotNull(model.seq);\n            Assert.IsInstanceOfType(model.seq, typeof(Int32));\n\n"
                + "            Assert.IsNotNull(model.display);\n            Assert.IsInstanceOfType(model.display, typeof(String));\n\n"
                + "            Assert.IsNotNull(model.descr);\n            Assert.IsInstanceOfType(model.descr, typeof(String));\n\n"
                + "        }\n\n"
                + "        [TestMethod]\n        public void TestModelConstructor()\n        {\n"
                + "            Mock<DbDataReader> reader = MockReader.CreateMockedReaderRandom();\n"
                + "            public_mat_work model = new public_mat_work(reader.Object);\n\n"
                + "            Assert.IsNotNull(model);\n            Assert.IsNotNull(model.oper);\n"
                + "            Assert.IsInstanceOfType(model.oper, typeof(String));\n"
                + "            Assert.AreEqual(\"\", model.oper);\n\n"
                + "            Assert.IsNotNull(model.seq);\n            Assert.IsInstanceOfType(model.seq, typeof(Int32));\n"
                + "            Assert.AreEqual(0, model.seq);\n\n"
                + "            Assert.IsNotNull(model.display);\n            Assert.IsInstanceOfType(model.display, typeof(String));\n"
                + "            Assert.AreEqual(\"\", model.display);\n\n"
                + "            Assert.IsNotNull(model.descr);\n            Assert.IsInstanceOfType(model.descr, typeof(String));\n"
                + "            Assert.AreEqual(\"\", model.descr);\n\n"
                + "        }\n\n"
                + "        [TestMethod]\n        public void TestModelConstructor()\n        {\n"
                + "            Mock<DbDataReader> reader = MockReader.CreateMockedReaderRandom();\n"
                + "            public_mat_work model = new public_mat_work(reader.Object);\n\n"
                + "            Assert.IsNotNull(model);\n            Assert.IsNotNull(model.oper);\n"
                + "            Assert.IsInstanceOfType(model.oper, typeof(String));\n"
                + "            Assert.AreEqual(\"String Placeholder\", model.oper);\n\n"
                + "            Assert.IsNotNull(model.seq);\n            Assert.IsInstanceOfType(model.seq, typeof(Int32));\n"
                + "            Assert.AreEqual(83726, model.seq);\n\n"
                + "            Assert.IsNotNull(model.display);\n            Assert.IsInstanceOfType(model.display, typeof(String));\n"
                + "            Assert.AreEqual(\"String Placeholder\", model.display);\n\n"
                + "            Assert.IsNotNull(model.descr);\n            Assert.IsInstanceOfType(model.descr, typeof(String));\n"
                + "            Assert.AreEqual(\"String Placeholder\", model.descr);\n\n"
                + "        }\n\n    }\n}\n";

            Assert.AreEqual(printString, mockInterpreter.modelTestString);
        }
    }
}
