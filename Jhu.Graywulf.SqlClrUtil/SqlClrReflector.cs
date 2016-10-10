using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace Jhu.Graywulf.SqlClrUtil
{
    class SqlClrReflector : IDisposable
    {
        private SqlAssembly assembly;
        private List<SqlObject> objects;
        private Dictionary<Type, string> types;

        public List<SqlObject> Objects
        {
            get { return objects; }
        }

        public Dictionary<Type, string> Types
        {
            get { return types; }
        }

        public SqlClrReflector(Assembly assembly, AssemblySecurityLevel sec)
        {
            InitializeMembers();
            ReflectAssembly(assembly, sec);
        }

        private void InitializeMembers()
        {
            this.objects = new List<SqlObject>();
            this.types = new Dictionary<Type, string>(Constants.SqlTypes);
        }

        public void Dispose()
        {
        }

        private void ReflectAssembly(Assembly a, AssemblySecurityLevel sec)
        {
            assembly = new SqlAssembly(a, sec);

            foreach (var type in a.GetTypes())
            {
                var obj = SqlObject.FromType(type);

                if (obj != null)
                {
                    objects.Add(obj);

                    if (obj is SqlUserDefinedType)
                    {
                        types.Add(type, ((SqlUserDefinedType)obj).GetSql());
                    }
                }
                else
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        obj = SqlObject.FromMethod(method);
                        if (obj != null)
                        {
                            objects.Add(obj);
                        }
                    }
                }
            }
        }

        public void ScriptCreate(TextWriter writer)
        {
            foreach (var schema in CollectSchemaNames())
            {
                ScriptCreateSchema(writer, schema);
            }

            foreach (var a in assembly.References.Values)
            {
                a.ScriptCreate(writer);
            }

            assembly.ScriptCreate(writer);

            foreach (var obj in objects.OrderBy(i => i.Rank))
            {
                obj.ScriptCreate(this, writer);
            }
        }

        public void ScriptDrop(TextWriter writer)
        {
            foreach (var obj in objects.OrderByDescending(i => i.Rank))
            {
                obj.ScriptDrop(this, writer);
            }

            assembly.ScriptDrop(writer);

            foreach (var a in assembly.References.Values.Reverse())
            {
                a.ScriptDrop(writer);
            }   

            foreach (var schema in CollectSchemaNames())
            {
                ScriptDropSchema(writer, schema);
            }
        }

        private void ScriptCreateSchema(TextWriter writer, string schema)
        {
            if (!Constants.SystemSchemas.Contains(UnquoteIdentifier(schema)))
            {
                writer.Write(
@"
CREATE SCHEMA [{0}]

GO

",
                    schema);
            }
        }

        private void ScriptDropSchema(TextWriter writer, string schema)
        {
            if (!Constants.SystemSchemas.Contains(UnquoteIdentifier(schema)))
            {
                writer.Write(
@"
DROP SCHEMA [{0}]

GO

",
                   schema);
            }
        }
        
        private HashSet<string> CollectSchemaNames()
        {
            var res = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var obj in objects)
            {
                var schema = obj.Schema;

                if (!String.IsNullOrEmpty(schema) &&
                    !Constants.SystemSchemas.Contains(schema) &&
                    !res.Contains(obj.Schema))
                {
                    res.Add(obj.Schema);
                }
            }

            return res;
        }

        protected string UnquoteIdentifier(string identifier)
        {
            return identifier.Trim('[', ']', '"');
        }
    }
}
