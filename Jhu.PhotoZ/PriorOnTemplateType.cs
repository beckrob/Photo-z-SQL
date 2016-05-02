using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public class PriorOnTemplateType : Prior
    {
        private int templateIDIndex;
        private Dictionary<int, double> probabilityOfTemplateID;

        public PriorOnTemplateType(int[] templateIDs, double[] templateProbabilities, int templateIDParameterIndex=2)
        {
            probabilityOfTemplateID = new Dictionary<int, double>();

            for (int i = 0; i < templateIDs.Length; ++i)
            {
                probabilityOfTemplateID[templateIDs[i]] = templateProbabilities[i];
            }

            //This parameter will be set in VerifyParameterlist(), but can be set as default
            templateIDIndex = templateIDParameterIndex;
        }

        public override bool VerifyParameterlist(List<TemplateParameter> parameters)
        {
            for (int i = 0; i < parameters.Count; ++i)
            {
                if (parameters[i].Name == "TypeID")
                {
                    templateIDIndex = i;
                    return true;
                }
            }

            return false;
        }

        public override double Evaluate(List<TemplateParameter> parameters)
        {
            return probabilityOfTemplateID[ (int) Math.Round(parameters[templateIDIndex].Value) ];
        }

        public override Prior CloneLightWeight()
        {
            PriorOnTemplateType copy = new PriorOnTemplateType();

            copy.templateIDIndex = templateIDIndex;
            copy.probabilityOfTemplateID = probabilityOfTemplateID;

            return copy;
        }

        protected PriorOnTemplateType() { }

    }
}
