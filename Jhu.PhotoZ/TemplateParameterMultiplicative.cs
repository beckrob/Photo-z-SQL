using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public class TemplateParameterMultiplicative : TemplateParameter
    {
        private double paramStart, paramEnd, paramMultiplier;

        public TemplateParameterMultiplicative()
        {
            paramStart = 1.0;
            paramEnd = 1.0;
            paramMultiplier = 2.0;
        }

        public TemplateParameterMultiplicative(double aStart, double aEnd, double aMultiplier)
        {
            paramStart = aStart;
            paramEnd = aEnd;
            paramMultiplier = aMultiplier;
        }

        public TemplateParameterMultiplicative(TemplateParameterMultiplicative aParam) : base(aParam)
        {
            paramStart = aParam.paramStart;
            paramEnd = aParam.paramEnd;
            paramMultiplier = aParam.paramMultiplier;
        }

        public override Object Clone()
        {
            return new TemplateParameterMultiplicative(this);
        }

        //Factory method, so that there aren't two constructors with a very similar signature
        //We can specify the center, and number of steps taken in either direction
        public static TemplateParameterMultiplicative TemplateParameterMultiplicativeAsCentered(double aCenter, int aSteps, double aMultiplier)
        {
            double factor = Math.Pow(aMultiplier, aSteps);

            return new TemplateParameterMultiplicative(aCenter / factor, aCenter * factor, aMultiplier);
        }

        public override bool StepValue()
        {
            Value *= paramMultiplier;
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
                value *= paramMultiplier;
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

            return (int) ( Math.Round( Math.Log( aValue / paramStart, paramMultiplier ) ) );
        }

        public override double GetClosestParameterValueInCoverage(double aValue)
        {
            return paramStart * Math.Pow(paramMultiplier, GetParameterIndexInCoverage(aValue));
        }


    }
}
