using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public abstract class TemplateParameter : ICloneable, IEquatable<TemplateParameter>
    {
  
        public double Value { get; set; }
        public string Name { get; set; }

        public TemplateParameter()
        {
            Value = 0.0;
            Name = "UnnamedParameter";
        }

        public TemplateParameter(TemplateParameter aParam)
        {
            Value = aParam.Value;
            Name = aParam.Name;
        }

        public override bool Equals(object aOther)
        {
            TemplateParameter castedOther = aOther as TemplateParameter;

            return !ReferenceEquals(castedOther, null) && Equals(castedOther);
        }

        //Template parameters compare equal when their name and value match, regardless of iteration details
        public bool Equals(TemplateParameter aOther)
        {
            return Value == aOther.Value && Name == aOther.Name;
        }


        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + Value.GetHashCode();
            hash = hash * 23 + Name.GetHashCode();

            return hash;
        }

        public abstract Object Clone();

        public virtual Object CloneForIteration()
        {
            return this.Clone();
        }


        public abstract void ResetValue();
        public abstract bool StepValue();

        public abstract int GetParameterCoverageSize();
        public abstract List<double> GetParameterCoverage();
        public abstract int GetParameterIndexInCoverage(double aValue = Constants.missingDouble);

        public abstract double GetClosestParameterValueInCoverage(double aValue);
    }
}
