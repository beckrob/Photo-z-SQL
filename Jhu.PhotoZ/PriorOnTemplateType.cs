using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public class PriorOnTemplateType : Prior
    {
        private int templateIDIndex;
        private double[] probabilityOfTemplateID;

        public PriorOnTemplateType(double[] templateProbabilities, Template aTemp)
        {
            probabilityOfTemplateID = new double[templateProbabilities.Length];

            for (int i = 0; i < templateProbabilities.Length; ++i)
            {
                probabilityOfTemplateID[i] = templateProbabilities[i];
            }


            bool found = false;
            List<TemplateParameter> paramList = aTemp.GetParameterList();

            for (int i = 0; i < paramList.Count; ++i)
            {
                if (paramList[i].Name == "TypeID" && paramList[i].GetParameterCoverageSize() == probabilityOfTemplateID.Length)
                {
                    templateIDIndex = i;

                    found = true;
                }
            }

            if (!found)
            {
                throw new ArgumentException("");
            }
        }

        public override bool VerifyParameterlist(List<TemplateParameter> parameters)
        {
            if (parameters.Count > templateIDIndex && parameters[templateIDIndex].Name == "TypeID" && parameters[templateIDIndex].GetParameterCoverageSize() == probabilityOfTemplateID.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override double Evaluate(List<TemplateParameter> parameters)
        {
            int index = (int) Math.Round(parameters[templateIDIndex].Value);

            if (index > 0 && index < probabilityOfTemplateID.Length)
            {
                return probabilityOfTemplateID[index];
            }
            else
            {
                return 0.0;
            }
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
