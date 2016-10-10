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
    class SqlFunction : SqlObject
    {
        private bool isTableValued;
        private string tableDefinition;
        private string fillRowMethodName;

        public override SqlObjectRank Rank
        {
            get { return SqlObjectRank.Function; }
        }

        public SqlFunction()
        {
            InitializeMembers();
        }

        public SqlFunction(SqlObject old)
            : base(old)
        {
            InitializeMembers();
        }

        public SqlFunction(SqlFunction old)
            : base(old)
        {
            CopyMembers(old);
        }

        public SqlFunction(MethodInfo method)
            :base(method)
        {
        }

        private void InitializeMembers()
        {
            this.isTableValued = false;
            this.tableDefinition = null;
            this.fillRowMethodName = null;
        }

        private void CopyMembers(SqlFunction old)
        {
            this.isTableValued = old.isTableValued;
            this.tableDefinition = old.tableDefinition;
            this.fillRowMethodName = old.fillRowMethodName;
        }

        protected override void ReflectMethod(MethodInfo method)
        {
            base.ReflectMethod(method);

            if (method.ReturnType != typeof(System.Collections.IEnumerable))
            {
                ReflectReturnType(method);
            }
        }

        protected override void ReflectAttributes(MethodInfo method)
        {
            base.ReflectAttributes(method);

            var att = (SqlFunctionAttribute)method.GetCustomAttribute(typeof(SqlFunctionAttribute));

            ReflectObjectName(att.Name);
            ReflectParameters(method);

            if (method.ReturnType == typeof(System.Collections.IEnumerable))
            {
                isTableValued = true;
                tableDefinition = att.TableDefinition;
                fillRowMethodName = att.FillRowMethodName;
            }
        }

        public override void ScriptCreate(SqlClrReflector r, TextWriter writer)
        {
            ScriptDrop(r, writer);

            if (isTableValued)
            {
                ScriptCreateTableValued(r, writer);
            }
            else
            {
                ScriptCreateScalar(r, writer);
            }
        }

        private void ScriptCreateScalar(SqlClrReflector r, TextWriter writer)
        {
            writer.Write(@"
CREATE FUNCTION [{0}].[{1}]
({2})
RETURNS {3}
AS
 EXTERNAL NAME [{4}].[{5}].[{6}]

GO

",
                Schema,
                Name,
                GetParametersSql(r),
                GetReturnTypeSql(r),
                AssemblyName,
                ClassName,
                MethodName);
        }

        private void ScriptCreateTableValued(SqlClrReflector r, TextWriter writer)
        {
            writer.Write(@"
CREATE FUNCTION [{0}].[{1}]
({2})
RETURNS TABLE ({3})
AS
 EXTERNAL NAME [{4}].[{5}].[{6}]

GO

",
                Schema,
                Name,
                GetParametersSql(r),
                tableDefinition,
                AssemblyName,
                ClassName,
                MethodName);
        }

        public override void ScriptDrop(SqlClrReflector r, TextWriter writer)
        {
            writer.Write(@"
IF (OBJECT_ID('{0}.{1}') IS NOT NULL)
BEGIN
    DROP FUNCTION [{0}].[{1}]
END

GO

",
                Schema,
                Name);
        }
    }
}
