using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public class PriorFlat : Prior
    {
        public override bool VerifyParameterlist(List<TemplateParameter> parameters)
        {
            return true;
        }

        public override double Evaluate(List<TemplateParameter> parameters)
        {
            return 1.0;
        }

        public override Prior CloneLightWeight()
        {
            return this;
        }

    }
}
