using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jhu.PhotoZ
{
    class TemplateParameterAdditiveRandomIterator : TemplateParameterAdditive
    {
        private List<double> permutation;
        int where;

        public TemplateParameterAdditiveRandomIterator()
        {
            permutation = new List<double>{0.0};

            where = 0;
        }

        public TemplateParameterAdditiveRandomIterator(double aStart, double aEnd, double aStep):base(aStart,aEnd,aStep)
        {
            InitializeNewPermutation();
        }

        public TemplateParameterAdditiveRandomIterator(TemplateParameterAdditiveRandomIterator aParam) : base(aParam)
        {
            permutation = aParam.permutation;
            where = aParam.where;
        }

        public override Object Clone()
        {
            return new TemplateParameterAdditiveRandomIterator(this);
        }

        public override Object CloneForIteration()
        {
            TemplateParameterAdditiveRandomIterator ret = new TemplateParameterAdditiveRandomIterator(this);

            InitializeNewPermutation();

            return ret;
        }

        private void InitializeNewPermutation()
        {
            permutation = new List<double>(GetParameterCoverage().OrderBy(a => Guid.NewGuid()));

            ResetValue();
        }

        public override bool StepValue()
        {
            if ( (++where) < permutation.Count )
            {
                Value = permutation[where];
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void ResetValue()
        {
            where = 0;
            Value = permutation[0];
        }

    }
}
