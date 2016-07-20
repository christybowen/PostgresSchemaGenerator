using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostgresSchemaGenerator.src.Library
{
    public class SchemaEntry
    {
        public String ColumnName;

        public String ColumnType;

        public Boolean PrimaryKey;

        public Boolean Nullable;
    }
}
