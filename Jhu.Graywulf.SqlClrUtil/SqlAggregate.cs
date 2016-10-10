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
    class SqlAggregate : SqlObject
    {
        private List<Parameter> parameters;

        public override SqlObjectRank Rank
        {
            get { return SqlObjectRank.Aggregate; }
        }

        public SqlAggregate()
        {
            InitializeMembers();
        }

        public SqlAggregate(SqlAggregate old)
            :base(old)
        {
            CopyMembers(old);
        }

        public SqlAggregate(Type type)
            :base(type)
        {
        }

        private void InitializeMembers()
        {
            this.parameters = null;
        }

        private void CopyMembers(SqlAggregate old)
        {
            this.parameters = old.parameters;
        }

        protected override void ReflectType(Type type)
        {
            base.ReflectType(type);
            
            ReflectReturnType(type.GetMethod("Terminate"));
        }

        protected override void ReflectAttributes(Type type)
        {
            var att = (SqlUserDefinedAggregateAttribute)type.GetCustomAttribute(typeof(SqlUserDefinedAggregateAttribute));

            ReflectObjectName(att.Name);
            ReflectParameters(type.GetMethod("Accumulate"));
        }

        public override void ScriptCreate(SqlClrReflector r, TextWriter writer)
        {
            ScriptDrop(r, writer);

            writer.Write(@"
CREATE AGGREGATE [{0}].[{1}]
({2})
RETURNS {3}
 EXTERNAL NAME [{4}].[{5}]

GO

",
                Schema,
                Name,
                GetParametersSql(r),
                GetReturnTypeSql(r),
                AssemblyName,
                ClassName);
        }

        public override void ScriptDrop(SqlClrReflector r, TextWriter writer)
        {
            writer.Write(@"
IF (OBJECT_ID('{0}.{1}') IS NOT NULL)
BEGIN
    DROP AGGREGATE [{0}].[{1}]
END

GO

",
                Schema,
                Name);
        }
    }
}
