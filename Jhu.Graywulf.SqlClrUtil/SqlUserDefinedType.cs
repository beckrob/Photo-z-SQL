using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Microsoft.SqlServer.Server;

namespace Jhu.Graywulf.SqlClrUtil
{
    class SqlUserDefinedType : SqlObject
    {
        public override SqlObjectRank Rank
        {
            get { return SqlObjectRank.UserDefinedType; }
        }

        public SqlUserDefinedType()
        {
            InitializeMembers();
        }

        public SqlUserDefinedType(SqlUserDefinedType old)
            :base(old)
        {
            CopyMembers(old);
        }

        public SqlUserDefinedType(Type type)
            :base(type)
        {
        }

        private void InitializeMembers()
        {
        }

        private void CopyMembers(SqlUserDefinedType old)
        {
        }

        protected override void ReflectAttributes(Type type)
        {
            var att = (SqlUserDefinedTypeAttribute)type.GetCustomAttribute(typeof(SqlUserDefinedTypeAttribute));

            ReflectObjectName(att.Name);
        }

        public string GetSql()
        {
            return String.Format("[{0}].[{1}]", Schema, Name);
        }

        public override void ScriptCreate(SqlClrReflector r, TextWriter writer)
        {
            ScriptDrop(r, writer);

            writer.Write(@"
CREATE TYPE [{0}].[{1}]
EXTERNAL NAME [{2}].[{3}]

GO

",
                Schema,
                Name,
                AssemblyName,
                ClassName);
        }

        public override void ScriptDrop(SqlClrReflector r, TextWriter writer)
        {
            writer.Write(@"
IF (TYPE_ID('{0}.{1}') IS NOT NULL)
BEGIN
    DROP TYPE [{0}].[{1}]
END

GO

",
                Schema,
                Name);
        }
    }
}
