using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public class TemplateParameterAdditive : TemplateParameter
    {
        private double paramStart, paramEnd, paramStepSize;


        public TemplateParameterAdditive()
        {
            paramStart = 0.0;
            paramEnd = 0.0;
            paramStepSize = 1.0;
        }

        public TemplateParameterAdditive(double aStart, double aEnd, double aStep)
        {
            paramStart = aStart;
            paramEnd = aEnd;
            paramStepSize = aStep;
        }

        public TemplateParameterAdditive(TemplateParameterAdditive aParam) : base(aParam)
        {
            paramStart = aParam.paramStart;
            paramEnd = aParam.paramEnd;
            paramStepSize = aParam.paramStepSize;
        }

        public override Object Clone()
        {
            return new TemplateParameterAdditive(this);
        }


        public override bool StepValue()
        {
            Value += paramStepSize;
            return (Value <= paramEnd);
        }

        public override void ResetValue()
        {
            Value = paramStart;
        }

        public override List<double> GetParameterCoverage()
        {
            List<double> result = new List<double>( GetParameterCoverageSize() );

            double value = paramStart;
            do
            {
                result.Add(value);
                value += paramStepSize;
            } while (value <= paramEnd);
           
            return result;
        }

        public override int GetParameterCoverageSize()
        {
            return GetParameterIndexInCoverage(paramEnd) + 1;
        }

        public override int GetParameterIndexInCoverage(double aValue = Constants.missingDouble)
        {
            if (aValue == Constants.missingDouble)
            {
                aValue = Value;
            }

            return (int)(Math.Round((aValue - paramStart) / paramStepSize));
        }

        public override double GetClosestParameterValueInCoverage(double aValue)
        {
            return paramStart + GetParameterIndexInCoverage(aValue) * paramStepSize;
        }

    }
}
