using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Jhu.PhotoZ
{
    public class PriorAbsMagLimitInFilter : PriorOnFluxInFilter
    {
        private double absMagLimit;
        private ConcurrentDictionary<double, double> redshiftDistanceModuluses;

        // This class implements an absolute magnitude limit in a filter, not allowing galaxies brighter than it

        // Hildebrandt (2010) used a limit of M(B)>-24
        // The B filter could be the the WFC3 F438W (ID 92 in HSCFilterPortal)

        public PriorAbsMagLimitInFilter(double aAbsMagLimit, Filter aFilt, Template aTemp)
            : base(aFilt, aTemp)
        {
            absMagLimit = aAbsMagLimit;
        }

        protected override void DoPreCalculationsFromTemplate(Template aTemp)
        {
            redshiftDistanceModuluses = new ConcurrentDictionary<double, double>();

            List<double> redshifts = aTemp.GetParameterCoverage("Redshift");
            foreach (double z in redshifts)
            {
                redshiftDistanceModuluses[z] = Cosmology.DistanceModulus(z);
            }

            base.DoPreCalculationsFromTemplate(aTemp);
        }

        public override bool VerifyParameterlist(List<TemplateParameter> aParams)
        {
            if (base.VerifyParameterlist(aParams))
            {
                if (aParams.Count > 1)
                {
                    if (aParams[1].Name == "Redshift" && aParams[0].Name == "Luminosity")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override double Evaluate(List<TemplateParameter> parameters)
        {
            double currentLum = parameters[0].Value;

            double fluxAtUnitLum, distanceModulus;
            if (fluxesInFilter.TryGetValue(GetParameterDoubleArray(parameters), out fluxAtUnitLum) && redshiftDistanceModuluses.TryGetValue(parameters[1].Value, out distanceModulus))
            {

                double apparentMag = MagnitudeSystem.GetMagnitudeFromCGSFlux(fluxAtUnitLum * currentLum, MagnitudeSystem.Type.AB);

                double absMag = apparentMag - distanceModulus;
                if (absMag > absMagLimit)
                {
                    return 1.0;
                }
                else
                {
                    return 0.0;
                }

            }

            return 0.0;
        }

    }
}
