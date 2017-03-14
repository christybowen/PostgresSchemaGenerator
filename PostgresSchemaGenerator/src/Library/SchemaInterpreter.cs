using System;
using System.Collections.Generic;
using Npgsql;
using ATShared;

namespace PostgresSchemaGenerator.src.Library
{
    /// <summary>
    /// This class handles the interpretation of the schema for a given table.
    /// </summary>
    public class SchemaInterpreter : DynamicSchemaInterpreter
    {
        public String modelPrintString;
        public String controllerPrintString;
        public string routesPrintString;
        public string modelTestString;
        public string controllerTestString;
        private string sqlConnString = "host=sand5;Username=cbowen;Database=payledger";

        public SchemaInterpreter(NpgsqlCommand cmd, String tableName, List<ATShared.SchemaEntry> exclusionList, List<ATShared.SchemaEntry> schemaCols)
            :base(cmd, tableName, exclusionList, schemaCols)
        {
            this.prepareSchema();
            this.baseQuery = generateQuery();
        }

        /// <summary>
        /// Creates the Model Class code based on the given schema from PostGres.
        /// </summary>
        public void createModelString()
        {
            var fileString = "using System;\n";
            fileString += "using System.Collections.Generic;\n";
            fileString += "using System.ComponentModel.DataAnnotations;\n";
            fileString += "using LinqToDB.Mapping;\n";
            fileString += "using System.Data.Common;\n\n";

            fileString += "namespace ActionTargetOData.Models\n{\n";
            fileString += "    [Table(Name = \"" + this.schemaName + "." + viewName + "\")]\n";
            fileString += "    public class " + this.schemaName + "_" + viewName + "\n    {\n";
            fileString += "        #region Instance Properties\n\n";

            List<String> primaryKey = new List<String>();

            string instanceProperties = "";
            string readerSwitch = "";
            bool havePrimaryKey = false;

            for(int i = 0; i < this.infoSchemaColumns.Count; i++)
            {
                var col = this.infoSchemaColumns[i];

                var columnType = this.getType(col.ColumnType);

                if (col.Nullable && columnType != "Object" && columnType != "Guid" && columnType != "Byte[]" && columnType != "String")
                {
                    columnType += "?";
                }

                if (!havePrimaryKey && col.PrimaryKey)
                {
                    instanceProperties += "        [Key, PrimaryKey, Identity]\n";

                    havePrimaryKey = true;
                }
                else
                {
                    instanceProperties += "        [Column(Name =\"" + col.ColumnName + "\"), NotNull]\n";
                }

                instanceProperties += "        public " + columnType + " " + col.ColumnName + " { get; set; }\n\n";

                var readerVar = getReader(this.getType(col.ColumnType), i);

                readerSwitch += "                    case " + i + ":\n";
                readerSwitch += "                        this." + col.ColumnName + " = isNull ? " + readerVar + ";\n";
                readerSwitch += "                        break;\n";

                if (col.PrimaryKey)
                {
                    primaryKey.Add(col.ColumnName);
                }
            }

            fileString += "        // ignored columns: ";

            for(int k = 0; k < exclusionList.Count - 1; k++)
            {
                fileString += exclusionList[k].ColumnName + ", ";
            }

            if (exclusionList.Count > 0)
            {
                fileString += exclusionList[exclusionList.Count - 1].ColumnName + "\n";
            }

            fileString += "\n        public List<String> primaryKeys = new List<String>() { ";

            for(int j = 0; j < primaryKey.Count - 1; j++)
            {
                fileString += "\"" + primaryKey[j] + "\", ";
            }

            if (primaryKey.Count == 0)
            {
                fileString += " };\n\n";
            }
            else
            {
                fileString += "\"" + primaryKey[primaryKey.Count - 1] + "\" };\n\n";
            }

            fileString += "        public String baseQuery = \"" + baseQuery + "\";\n\n";

            fileString += instanceProperties;

            fileString += "        #endregion Instance Properties\n\n";
            fileString += "        #region Methods\n\n";
            fileString += "        public " + this.schemaName + "_" + viewName + "() {}\n\n";
            fileString += "        public " + this.schemaName + "_" + viewName + "(DbDataReader reader)\n";
            fileString += "        {\n";
            fileString += "            for(int i = 0; i < reader.FieldCount; i++)\n";
            fileString += "            {\n";
            fileString += "                var isNull = reader.IsDBNull(i);\n\n";
            fileString += "                switch(i)\n                {\n";

            fileString += readerSwitch;
            fileString += "                }\n";
            fileString += "            }\n";
            fileString += "        }\n\n";
            fileString += "        #endregion Methods\n\n";

            fileString += "    }\n";
            fileString += "}\n";

            this.modelPrintString = fileString;
        }

        /// <summary>
        /// Creates the Controller class code based on the created model
        /// </summary>
        public void createControllerString()
        {
            string view = this.schemaName + "_" + this.viewName;

            var theString = "using System;\n";
            theString += "using System.Collections.Generic;\n"
                + "using System.Data;\n"
            + "using System.Net;\n"
            + "using System.Web.Http;\n"
            + "using System.Web.OData;\n"
            + "using System.Web.OData.Query;\n"
            + "using ActionTargetOData.Models;\n"
            + "using Npgsql;\n"
            + "using System.Data.Common;\n"
            + "using System.Configuration;\n\n"
            + "namespace ActionTargetOData.Controllers\n{\n"
            + "    public class " + view + "sController : ODataController\n"
            + "    {\n        private static ODataValidationSettings _validationSettings = new ODataValidationSettings();\n"
            + "        private static ConnectionStringSettingsCollection settings = ConfigurationManager.ConnectionStrings;\n"
            + "        string hostName = settings[\"sqlHost\"].ConnectionString;\n"
            + "        string dbName = settings[\"sqlDb\"].ConnectionString;\n"
            + "        string userName = settings[\"sqlUser\"].ConnectionString;\n\n"
            + "        // GET: odata/" + view + "s\n"
            + "        public IHttpActionResult Get" + view + "s(ODataQueryOptions<" + view + "> queryOptions)\n"
            + "        {\n            using (var connection = new NpgsqlConnection(\"host=\" + hostName + \";Username=\" + userName + \";Database=\" + dbName))\n"
            + "            {\n                return Connect(connection, \"SELECT * from "
            + this.schemaName + "." + this.viewName + "\");\n"
            + "            }\n        }\n\n"
            + "        // GET: odata/" + view + "(5)\n"
            + "        public IHttpActionResult Get" + view + "([FromODataUri] " + this.getType(this.infoSchemaColumns[0].ColumnType)
            + " key, ODataQueryOptions<" + view + "> queryOptions)\n"
            + "        {\n            using (var connection = new NpgsqlConnection(\"host=\" + hostName + \";Username=\" + userName + \";Database=\" + dbName))\n"
            + "            {\n                return Connect(connection, \"SELECT * from "
            + this.schemaName + "." + this.viewName + " WHERE " + this.infoSchemaColumns[0].ColumnName + " = '\" + key + \"'\");\n"
            + "            }\n        }\n\n"
            + "        public IHttpActionResult Connect(DbConnection connection, string query)\n"
            + "        {\n            DbConnection conn;\n\n"
            + "            if (string.IsNullOrWhiteSpace(connection.ConnectionString))\n"
            + "            {\n                conn = connection;\n            }\n"
            + "            else {\n                conn = new NpgsqlConnection(connection.ConnectionString);\n            }\n\n"

            + "            // validate the query.\n            try\n            {\n                conn.Open();\n            }\n"
            + "            catch (Exception ex)\n            {\n                System.Diagnostics.Debug.WriteLine(\"ERROR::\");\n"
            + "                System.Diagnostics.Debug.Write(ex.Message);\n            }\n\n"
            + "            // Make sure connection is open\n            if (conn.State == ConnectionState.Closed)\n"
            + "            {\n                return StatusCode(HttpStatusCode.InternalServerError);\n            }\n\n"

            + "            using (var command = new NpgsqlCommand())\n            {\n"
            + "                if (string.IsNullOrWhiteSpace(conn.ConnectionString))\n                {\n"
            + "                    command.Connection = new NpgsqlConnection();\n                }\n"
            + "                else\n                {\n                    command.Connection = (NpgsqlConnection)conn;\n"
            + "                }\n\n"

            + "                List<" + view + "> modelList = new List<" + view + ">();\n\n"
            + "                // Start SQL command\n                command.CommandText = query;\n\n"

            + "                try\n                {\n"
            + "                    using (var reader = command.ExecuteReader())\n                    {\n"
            + "                        // Per Row\n                        while (reader.Read())\n"
            + "                        {\n                            " + view + " temp = new " + view + "(reader);\n"

            + "                            modelList.Add(temp);\n                        }\n"
            + "                    }\n"
            + "                }\n                catch (Exception e)\n                {\n"

            + "                    System.Diagnostics.Debug.WriteLine(e.Message);\n"

            + "                    return StatusCode(HttpStatusCode.InternalServerError);\n"
            + "                }\n\n                var result = Ok<IEnumerable<" + view + ">>(modelList);\n\n"
            + "                command.Connection.Close();\n                conn.Close();\n\n"
            + "                return result;\n            }\n        }\n    }\n}";

            this.controllerPrintString = theString;

            /*#region Using statements

            controllerPrintString += "using System;\n";
                    controllerPrintString += "using System.Collections.Generic;\n";
                    controllerPrintString += "using System.Data;\n";
                    controllerPrintString += "using System.Linq;\n";
                    controllerPrintString += "using System.Net;\n";
                    controllerPrintString += "using System.Net.Http;\n";
                    controllerPrintString += "using System.Web.Http;\n";
                    controllerPrintString += "using System.Web.ModelBinding;\n";
                    controllerPrintString += "using System.Web.OData;\n";
                    controllerPrintString += "using System.Web.OData.Query;\n";
                    controllerPrintString += "using System.Web.OData.Routing;\n";
                    controllerPrintString += "using ActionTargetOData.Models;\n";
                    controllerPrintString += "using Microsoft.OData.Core;\n";
                    controllerPrintString += "using Npgsql;\n\n";

                    #endregion Using statements

                    controllerPrintString += "namespace ActionTargetOData.Controllers\n{\n";

                    controllerPrintString += "    public class " + view + "Controller : ODataController\n";
                    controllerPrintString += "    {\n";

                    controllerPrintString += "        private static ODataValidationSettings _validationSettings = new ODataValidationSettings();\n\n";

                    #region GetAllEntries

                    controllerPrintString += "        public IHttpActionResult Get" + view + "s(ODataQueryOptions<" + view + "> queryOptions)\n";
                    controllerPrintString += "        {\n            using (var connection = new NpgsqlConnection(\"host=sand5;Username=cbowen;Database=payledger\"))\n"
                        + "            {\n                return Connect(connection, \"SELECT * from " + this.schemaName + "." + this.viewName + "\");\n            }\n        }\n\n";

                    #endregion GetAllEntries

                    #region GetSpecificEntry

                    controllerPrintString += "        public IHttpActionResult Get" + view + "([FromODataUri] "
                                + this.getType(this.infoSchemaColumns[0].ColumnType) + " key, ODataQueryOptions<"
                                + view + "> queryOptions)\n";

                    controllerPrintString += "        {\n            using (var connection = new NpgsqlConnection(\"host=sand5;Username=cbowen;Database=payledger\"))\n"
                        + "            {\n                return Connect(connection, \"SELECT * from " + this.schemaName + "." + this.viewName + " WHERE "
                                                                    + this.infoSchemaColumns[0].ColumnName + " = \" + key\");\n            }\n        }\n\n";

                    #endregion GetSpecificEntry

                    controllerPrintString += "    }\n}\n"; // end of controller class and namespace*/
        }

        public string createModelTestConstructorString()
        {
            string view = this.schemaName + "_" + this.viewName;

            var testConstructorString = "        [TestMethod]\n        public void TestModelConstructor()\n        {\n"
                + "            Mock<DbDataReader> reader = MockReader.CreateMockedReaderRandom();\n"
                + "            " + view + " model = new " + view + "(reader.Object);\n\n"
                + "            Assert.IsNotNull(model);\n";

            for(int i = 0; i < this.infoSchemaColumns.Count; i++)
            {
                var col = this.infoSchemaColumns[i];

                testConstructorString += "            Assert.IsNotNull(model." + col.ColumnName + ");\n"
                    + "            Assert.IsInstanceOfType(model." + col.ColumnName + ", typeof(" + this.getType(col.ColumnType) + "));\n\n";
            }

            testConstructorString += "        }\n\n";

            return testConstructorString;
        }

        public string createModelTestNullString()
        {
            string view = this.schemaName + "_" + this.viewName;

            var testNullString = "        [TestMethod]\n        public void TestModelConstructorNull()\n        {\n"
                + "            Mock<DbDataReader> reader = MockReader.CreateMockedReaderNull();\n"
                + "            " + view + " model = new " + view + "(reader.Object);\n\n"
                + "            Assert.IsNotNull(model);\n";

            for (int i = 0; i < this.infoSchemaColumns.Count; i++)
            {
                var col = this.infoSchemaColumns[i];

                string type = this.getType(col.ColumnType);

                testNullString += "            Assert.IsNotNull(model." + col.ColumnName + ");\n"
                    + "            Assert.IsInstanceOfType(model." + col.ColumnName + ", typeof(" + type + "));\n"
                    + "            Assert.AreEqual(" + this.getDefault(type) + ", model." + col.ColumnName + ");\n\n";
            }

            testNullString += "        }\n\n";

            return testNullString;
        }

        public string createModelTestStaticString()
        {
            string view = this.schemaName + "_" + this.viewName;

            var testStaticString = "        [TestMethod]\n        public void TestModelConstructorStatic()\n        {\n"
                + "            Mock<DbDataReader> reader = MockReader.CreateMockedReaderStatic();\n"
                + "            " + view + " model = new " + view + "(reader.Object);\n\n"
                + "            Assert.IsNotNull(model);\n";

            for (int i = 0; i < this.infoSchemaColumns.Count; i++)
            {
                var col = this.infoSchemaColumns[i];

                string type = this.getType(col.ColumnType);

                testStaticString += "            Assert.IsNotNull(model." + col.ColumnName + ");\n"
                    + "            Assert.IsInstanceOfType(model." + col.ColumnName + ", typeof(" + type + "));\n"
                    + "            Assert.AreEqual(" + this.getConstant(type) + ", model." + col.ColumnName + ");\n\n";
            }

            testStaticString += "        }\n\n";

            return testStaticString;
        }

        public void createModelTestString()
        {
            this.modelTestString = "using Microsoft.VisualStudio.TestTools.UnitTesting;\n"
               + "using ActionTargetOData.Models;\nusing Moq;\n"
               + "using System.Data.Common;\nusing System;\n\n"
               + "namespace ODataUnitTests\n{\n    [TestClass]\n"
               + "    public class " + this.schemaName + "_" + this.viewName + "ModelTests\n    {\n"
               + createModelTestConstructorString()
               + createModelTestNullString()
               + createModelTestStaticString()
               + "    }\n}\n";
        }

        public void CreateControllerTestString()
        {
            var view = this.schemaName + "_" + this.viewName;

            var expected = "using System.Web.Http;\n"
               + "using Microsoft.VisualStudio.TestTools.UnitTesting;\n"
               + "using ActionTargetOData.Controllers;\n"
               + "using Npgsql;\nusing Moq;\n"
               + "using System.Data.Common;\n"
               + "using System.Threading.Tasks;\n"
               + "using System.Net.Http;\n"
               + "using System.Web.OData;\n"
               + "using System.Web.OData.Builder;\n"
               + "using ActionTargetOData.Models;\n\n"
               + "namespace ODataUnitTests\n{\n"
               + "    /// <summary>\n    /// Summary description for " + view + "ControllerTests\n"
               + "    /// </summary>\n    [TestClass]\n"
               + "    public class " + view + "ControllerTests\n    {\n"
               + "        public " + view + "ControllerTests()\n        {\n"
               + "            controller = new " + view + "sController();\n        }\n\n"
               + "        private " + view + "sController controller;\n\n"
               + "        [TestMethod]\n"
               + "        public async Task TestGet" + view + "s()\n        {\n"
               + "            var request = new HttpRequestMessage(HttpMethod.Get, \"http://localhost:64680/odata/" + view + "s\");\n"
               + "            var builder = new ODataConventionModelBuilder();\n"
               + "            builder.EntitySet<" + view + ">(\"" + view + "s\");\n"
               + "            var model = builder.GetEdmModel();\n"
               + "            var path = new System.Web.OData.Routing.ODataPath();\n"
               + "            var context = new ODataQueryContext(model, typeof(" + view + "), path);\n"
               + "            var options = new System.Web.OData.Query.ODataQueryOptions<ActionTargetOData.Models." + view + ">(context, request);\n"
               + "            controller.Request = request;\n"
               + "            controller.ControllerContext.Configuration = new HttpConfiguration();\n\n"
               + "            var results = controller.Get" + view + "s(options);\n\n"
               + "            var total = await results.ExecuteAsync(new System.Threading.CancellationToken());\n\n"
               + "            Assert.AreEqual(System.Net.HttpStatusCode.OK, total.StatusCode);\n"
               + "            Assert.IsNotNull(total.Content);\n        }\n\n"
               + "        [TestMethod]\n"
               + "        public async Task TestGetSpecific" + view + "()\n        {\n"
               + "            var request = new HttpRequestMessage(HttpMethod.Get, \"http://localhost:64680/odata/" + view + "s\");\n"
               + "            var builder = new ODataConventionModelBuilder();\n"
               + "            builder.EntitySet<" + view + ">(\"" + view + "s\");\n"
               + "            var model = builder.GetEdmModel();\n"
               + "            var path = new System.Web.OData.Routing.ODataPath();\n"
               + "            var context = new ODataQueryContext(model, typeof(" + view + "), path);\n"
               + "            var options = new System.Web.OData.Query.ODataQueryOptions<ActionTargetOData.Models." + view + ">(context, request);\n"
               + "            controller.Request = request;\n"
               + "            controller.ControllerContext.Configuration = new HttpConfiguration();\n\n"
               + "            var results = controller.Get" + view + "(" + getConstant(this.getType(this.infoSchemaColumns[0].ColumnType)) + ", options);\n\n"
               + "            var total = await results.ExecuteAsync(new System.Threading.CancellationToken());\n\n"
               + "            Assert.AreEqual(System.Net.HttpStatusCode.OK, total.StatusCode);\n"
               + "            Assert.IsNotNull(total.Content);\n        }\n\n"
               + "        [TestMethod]\n"
               + "        public async Task TestConnectionClosed()\n        {\n"
               + "            var request = new HttpRequestMessage(HttpMethod.Get, \"http://localhost:64680/odata/" + view + "s\");\n\n"
               + "            controller.Request = request;\n\n"
               + "            Mock<DbConnection> conn = new Mock<DbConnection>();\n\n"
               + "            conn.Setup(c => c.Open()).Callback(() => conn.Setup(co => co.State).Returns(System.Data.ConnectionState.Closed));\n\n"
               + "            var results = controller.Connect(conn.Object, \"SELECT * from " + this.schemaName + "." + this.viewName + "\");\n\n"
               + "            var total = await results.ExecuteAsync(new System.Threading.CancellationToken());\n\n"
               + "            Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, total.StatusCode);\n        }\n\n"
               + "        [TestMethod]\n"
               + "        public async Task TestConnectionOpenBadReader()\n        {\n"
               + "            var request = new HttpRequestMessage(HttpMethod.Get, \"http://localhost:64680/odata/" + view + "s\");\n\n"
               + "            controller.Request = request;\n\n"
               + "            Mock<DbConnection> conn = new Mock<DbConnection>();\n\n"
               + "            conn.Setup(c => c.Open()).Callback(() => conn.Setup(co => co.State).Returns(System.Data.ConnectionState.Open));\n\n"
               + "            var results = controller.Connect(conn.Object, \"SELECT * from " + this.schemaName + "." + this.viewName + "\");\n\n"
               + "            var total = await results.ExecuteAsync(new System.Threading.CancellationToken());\n\n"
               + "            Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, total.StatusCode);\n        }\n\n"
               + "        [TestMethod]\n"
               + "        public async Task TestConnectSuccess()\n        {\n"
               + "            var request = new HttpRequestMessage(HttpMethod.Get, \"http://localhost:64680/odata/" + view + "s\");\n\n"
               + "            controller.ControllerContext.Configuration = new HttpConfiguration();\n"
               + "            controller.Request = request;\n\n"
               + "            NpgsqlConnection conn = new NpgsqlConnection(\"host=\" + MockReader.hostName + \";Username=\" + MockReader.userName + \";Database=\" + MockReader.dbName);\n\n"
               + "            var results = controller.Connect(conn, \"SELECT * from " + this.schemaName + "." + this.viewName + "\");\n\n"
               + "            var total = await results.ExecuteAsync(new System.Threading.CancellationToken());\n\n"
               + "            Assert.AreEqual(System.Net.HttpStatusCode.OK, total.StatusCode);\n"
               + "            Assert.IsNotNull(total.Content);\n        }\n    }\n}";

            this.controllerTestString = expected;
        }

        /// <summary>
        /// Writes the Schema currently stored by the object to a file.
        /// </summary>
        /// <param name="fileFolder">The file location.</param>
        public void saveModelToFile(String fileFolder)
        {
            fileFolder += this.schemaName + "_" + this.viewName + ".cs";
            System.IO.File.WriteAllText(fileFolder, this.modelPrintString);
        }

        /// <summary>
        /// Writes the Schema currently stored by the object to a file.
        /// </summary>
        /// <param name="fileFolder">The file location.</param>
        public void saveControllerToFile(String fileFolder)
        {
            fileFolder += this.schemaName + "_" + this.viewName + "Controller.cs";
            System.IO.File.WriteAllText(fileFolder, this.controllerPrintString);
        }

        public void saveModelTestsToFile(string fileFolder)
        {
            fileFolder += this.schemaName + "_" + this.viewName + "ModelTests.cs";
            System.IO.File.WriteAllText(fileFolder, this.modelTestString);
        }

        public void saveControllerTestsToFile(string fileFolder)
        {
            fileFolder += this.schemaName + "_" + this.viewName + "ControllerTests.cs";
            System.IO.File.WriteAllText(fileFolder, this.controllerTestString);
        }

        /// <summary>
        /// Creates the Routes code based on the created model
        /// </summary>
        public void GetRoutesString()
        {
            string view = this.schemaName + "_" + this.viewName;

            routesPrintString = "            // view-start: " + view + "\n\n"
                + "            EntityTypeConfiguration<" + view + "> "
                + view + "Type = builder.EntityType<" + view + ">();\n";

            bool havePrimaryKey = false;

            for (int i = 0; i < this.infoSchemaColumns.Count; i++)
            {
                var col = this.infoSchemaColumns[i];

                if (!havePrimaryKey && col.PrimaryKey)
                {
                    routesPrintString += "            " + view + "Type.HasKey(a => a." + col.ColumnName + ");\n";

                    havePrimaryKey = true;
                }
                else
                {
                    routesPrintString += "            " + view + "Type.Property(a => a." + col.ColumnName + ");\n";
                }
            }

            routesPrintString += "            builder.EntitySet<" + view + ">(\"" + view + "s\");\n\n";
            routesPrintString += "            // view-end: " + view + "\n\n";
        }

        private String getReader(String typeOf, int indexNum)
        {
            // Determine what command to read.
            switch (typeOf)
            {
                case "Int64":
                    return getDefault(typeOf) + " : reader.GetInt64(" + indexNum + ")";
                case "Byte[]":
                    return getDefault(typeOf) + " : ((byte[])reader[" + indexNum + "])";
                case "Boolean":
                    return getDefault(typeOf) + " : reader.GetBoolean(" + indexNum + ")";
                case "String":
                case "IPAddress":
                case "varchar":
                    return getDefault(typeOf) + " : reader.GetString(" + indexNum + ")";
                case "DateTime":
                    return getDefault(typeOf) + " : reader.GetDateTime(" + indexNum + ")";
                case "Decimal":
                case "Single":
                    return getDefault(typeOf) + " : reader.GetFloat(" + indexNum + ")";
                case "Int32":
                    return getDefault(typeOf) + " : reader.GetInt32(" + indexNum + ")";
                case "Double":
                    return getDefault(typeOf) + " : reader.GetDouble(" + indexNum + ")";
                case "Int16":
                    return getDefault(typeOf) + " : reader.GetInt16(" + indexNum + ")";
                case "TimeSpan":
                case "DateTimeOffset":
                    return getDefault(typeOf) + " : reader.GetTimeSpan(" + indexNum + ")";
                case "Byte":
                    return getDefault(typeOf) + " : reader.GetByte(" + indexNum + ")";
                case "Guid":
                    return getDefault(typeOf) + " : reader.GetGuid(" + indexNum + ")";
                default:
                    return null;
            }
        }

        private string getDefault(string typeOf)
        {
            // Determine what command to read.
            switch (typeOf)
            {
                case "Int64":
                    return "0";
                case "Byte[]":
                    return "null";
                case "Boolean":
                    return "false";
                case "String":
                case "IPAddress":
                case "varchar":
                    return "\"\"";
                case "DateTime":
                    return "null";
                case "Decimal":
                case "Single":
                    return "0.0";
                case "Int32":
                    return "0";
                case "Double":
                    return "0.0";
                case "Int16":
                    return "0";
                case "TimeSpan":
                case "DateTimeOffset":
                    return "null";
                case "Byte":
                    return "null";
                case "Guid":
                    return "null";
                default:
                    return "";
            }
        }

        private string getConstant(string typeOf)
        {
            // Determine what command to read.
            switch (typeOf)
            {
                case "Int64":
                    return "1234";
                case "Byte[]":
                    return "(long)839275";
                case "Boolean":
                    return "true";
                case "String":
                case "IPAddress":
                case "varchar":
                    return "\"String Placeholder\"";
                case "DateTime":
                    return "DateTime.Now";
                case "Decimal":
                case "Single":
                    return "(float)298473.76";
                case "Int32":
                    return "83726";
                case "Double":
                    return "37284.98";
                case "Int16":
                    return "(short)234";
                case "TimeSpan":
                case "DateTimeOffset":
                    return "DateTime.Now";
                case "Byte":
                    return "0";
                case "Guid":
                    return "new Guid()";
                default:
                    return "";
            }
        }
    }
}
