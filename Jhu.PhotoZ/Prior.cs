using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public abstract class Prior
    {
        public abstract bool VerifyParameterlist(List<TemplateParameter> parameters);

        public abstract double Evaluate(List<TemplateParameter> parameters);

        //Create a lightweight copy so that the prior can be used parallelly
        public abstract Prior CloneLightWeight();

        protected Prior() { }
    }
}
