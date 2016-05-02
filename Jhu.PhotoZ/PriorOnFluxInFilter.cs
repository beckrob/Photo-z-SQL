using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Jhu.PhotoZ
{
    public class PriorOnFluxInFilter : Prior
    {
        protected Filter filt;
        private List<TemplateParameter> paramList;
        protected ConcurrentDictionary<EquatableArray<double>, double> fluxesInFilter;

        //The prior has to be created once the filter zeropoint is already calibrated
        public PriorOnFluxInFilter(Filter aFilt, Template aTemp) : this(aFilt, aTemp, true) {}

        protected PriorOnFluxInFilter(Filter aFilt, Template aTemp, bool calculateAndStoreFluxes)
        {
            filt = aFilt;
            paramList = new List<TemplateParameter>(aTemp.GetParameterList().Select(x => (TemplateParameter)x.Clone()));
            fluxesInFilter = new ConcurrentDictionary<EquatableArray<double>, double>();

            if (calculateAndStoreFluxes)
            {
                DoPreCalculationsFromTemplate(aTemp);
            }
        }

        protected virtual void DoPreCalculationsFromTemplate(Template aTemp)
        {
            aTemp.IgnoreLuminosity = true;
            aTemp.InitializeIteration();

            Flux tmpFlux = new Flux();

            do
            {
                Spectrum currentSpectrum = aTemp.GenerateIteratedSpectrum();

                tmpFlux.ConvolveFromFilterAndSpectrum(currentSpectrum, filt);

                fluxesInFilter[GetParameterDoubleArray(aTemp.GetParameterList())] = tmpFlux.Value;
            }
            while (aTemp.IterateParameters());
        }


        public override bool VerifyParameterlist(List<TemplateParameter> aParams)
        {
            if (!ReferenceEquals(paramList, null) && 
                !ReferenceEquals(aParams, null) &&
                paramList.Count == aParams.Count)
            {
                for (int i = 0; i < paramList.Count; ++i)
                {
                    if (paramList[i].Name != aParams[i].Name) { return false; }
                }

                return true;
            }
            return false;
        }

        protected EquatableArray<double> GetParameterDoubleArray(List<TemplateParameter> aParams)
        {
            double[] parameterArr = new double[aParams.Count - 1];
            for (int i = 1; i < aParams.Count; ++i)
            {
                parameterArr[i-1] = aParams[i].Value;
            }

            return new EquatableArray<double>(parameterArr);
        }

        //This function should be overridden in subclasses with the actual function
        public override double Evaluate(List<TemplateParameter> parameters)
        {

            double dummy;
            if (fluxesInFilter.TryGetValue(GetParameterDoubleArray(parameters), out dummy))
            {
                return 1.0;
            }

            return 0.0;
        }

        public override Prior CloneLightWeight()
        {
            return this;
        }

    }
}
