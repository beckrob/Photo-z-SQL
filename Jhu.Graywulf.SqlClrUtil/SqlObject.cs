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
    abstract class SqlObject : IComparable
    {
        private string assemblyName;
        private string className;
        private string methodName;
        private string schema;
        private string name;
        private Type returnType;
        private List<Parameter> parameters;

        public abstract SqlObjectRank Rank { get; }

        public string Schema
        {
            get { return schema; }
        }

        public string Name
        {
            get { return name; }
        }

        public string AssemblyName
        {
            get { return assemblyName; }
        }

        public string ClassName
        {
            get { return className; }
        }

        public string MethodName
        {
            get { return methodName; }
        }

        public SqlObject()
        {
            InitializeMembers();
        }

        public SqlObject(SqlObject old)
        {
            CopyMembers(old);
        }

        public SqlObject(Type type)
        {
            ReflectType(type);
        }

        public SqlObject(MethodInfo method)
        {
            ReflectMethod(method);
        }

        private void InitializeMembers()
        {
            this.assemblyName = null;
            this.className = null;
            this.methodName = null;
            this.schema = null;
            this.name = null;
            this.returnType = null;
            this.parameters = null;
        }

        private void CopyMembers(SqlObject old)
        {
            this.assemblyName = old.assemblyName;
            this.className = old.className;
            this.methodName = old.methodName;
            this.schema = old.schema;
            this.name = old.name;
            this.returnType = old.returnType;
            this.parameters = old.parameters;
        }

        public static SqlObject FromType(Type type)
        {
            SqlObject obj = null;
            var atts = type.GetCustomAttributes();

            foreach (var att in atts)
            {
                if (att is SqlUserDefinedAggregateAttribute)
                {
                    obj = new SqlAggregate(type);
                }
                else if (att is SqlUserDefinedTypeAttribute)
                {
                    obj = new SqlUserDefinedType(type);
                }

                if (obj != null)
                {
                    break;
                }
            }

            return obj;
        }

        public static SqlObject FromMethod(MethodInfo method)
        {
            SqlObject obj = null;
            var atts = method.GetCustomAttributes();

            foreach (var att in atts)
            {
                if (att is SqlFunctionAttribute)
                {
                    obj = new SqlFunction(method);
                }

                if (obj != null)
                {
                    break;
                }
            }

            return obj;
        }

        protected virtual void ReflectType(Type type)
        {
            assemblyName = type.Assembly.GetName().Name;
            className = type.FullName;

            ReflectAttributes(type);
        }

        protected virtual void ReflectMethod(MethodInfo method)
        {
            assemblyName = method.DeclaringType.Assembly.GetName().Name;
            className = method.DeclaringType.FullName;
            methodName = method.Name;
            name = method.Name;
            //If the Name property is not set in SqlFunctionAttribute, the ReflectObjectName function will not set this.schema
            //(while this.name is set here), resulting in an erroneous script. Therefore, also giving a default value for the schema here. 
            schema = "dbo";

            ReflectAttributes(method);
        }

        protected virtual void ReflectAttributes(Type type)
        {
        }

        protected virtual void ReflectAttributes(MethodInfo method)
        {

        }

        protected void ReflectObjectName(string name)
        {
            if (name != null)
            {
                var parts = name.Split('.');

                if (parts.Length == 1)
                {
                    this.schema = "dbo";
                    this.name = name;
                }
                else if (parts.Length == 2)
                {
                    this.schema = parts[0];
                    this.name = parts[1];
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        protected void ReflectReturnType(MethodInfo method)
        {
            returnType = method.ReturnType;
        }

        protected void ReflectParameters(MethodInfo method)
        {
            foreach (var parameter in method.GetParameters())
            {
                if (parameters == null)
                {
                    parameters = new List<Parameter>();
                }

                parameters.Add(new Parameter(parameter));
            }
        }

        protected string GetReturnTypeSql(SqlClrReflector r)
        {
            return r.Types[returnType];
        }

        protected string GetParametersSql(SqlClrReflector r)
        {
            if (parameters == null)
            {
                return String.Empty;
            }

            var sb = new StringBuilder();

            for (int i = 0; i < parameters.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(parameters[i].GetSql(r));
            }

            return sb.ToString();
        }

        public abstract void ScriptCreate(SqlClrReflector r, TextWriter writer);

        public abstract void ScriptDrop(SqlClrReflector r, TextWriter writer);

        public int CompareTo(object obj)
        {
            return Rank.CompareTo(((SqlObject)obj).Rank);
        }
    }
}
