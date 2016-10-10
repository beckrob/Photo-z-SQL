using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Jhu.Graywulf.SqlClrUtil
{
    class Parameter
    {
        private string name;
        private Type type;

        public string Name
        {
            get { return name; }
        }

        public Type Type
        {
            get { return type; }
        }

        public Parameter()
        {
            InitializeMembers();
        }

        public Parameter(ParameterInfo parameter)
        {
            InitializeMembers();

            ReflectParameter(parameter);
        }

        private void InitializeMembers()
        {
            this.name = null;
            this.type = null;
        }

        private void ReflectParameter(ParameterInfo parameter)
        {
            this.name = parameter.Name;
            this.type = parameter.ParameterType;
        }

        public string GetSql(SqlClrReflector r)
        {
            return String.Format("@{0} {1}", name, r.Types[type]);
        }
    }
}
