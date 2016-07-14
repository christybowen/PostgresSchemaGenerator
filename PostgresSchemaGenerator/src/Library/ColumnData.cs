using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgresSchemaGenerator.src.Library
{
    public class ColumnData
    {
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public string IsNullable { get; set; }
    }
}
