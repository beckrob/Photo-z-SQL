using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public abstract class ValueWithErrorConvolveableFromFilterAndSpectrum : IValueWithError<double>,ICloneable
    {

        public double Value { get; set; }
        public double Error { get; set; }

        public virtual bool ConvolveFromFilterAndSpectrum(Spectrum spec, Filter filt, out double uncorrectedFlux)
        {
            bool error;
            uncorrectedFlux = Jhu.SpecSvc.Util.Integral.Integrate(spec.GetBinCenters(), spec.GetFluxes(), filt.GetBinCenters(), filt.GetResponses(), out error)
                                / (Constants.speedOfLightSI * 100);
            //Converting c in SI to cgs

            if (!error)
            {
                SetupFromUncorrectedFlux(uncorrectedFlux, filt.ZeroPointCorrection, filt.ZeroPointCorrectionInFlux);
            }
            else
            {
                uncorrectedFlux = Constants.missingDouble;
            }

            return (!error);
        }

        public virtual bool ConvolveFromFilterAndSpectrum(Spectrum spec, Filter filt)
        {
            double dummyFlux;
            return ConvolveFromFilterAndSpectrum(spec, filt, out dummyFlux);
        }

        public abstract void SetupFromUncorrectedFlux(double uncorrectedFlux, double filterZeroPoint, bool filterZeroPointInFlux);

        public abstract bool ApplySchlegelExtinctionCorrection(Filter filt, double mapValue, double rVParam);

        public abstract Object Clone();

        protected double GetCorrectedFlux(double uncorrectedFlux, double filterZeroPoint, bool filterZeroPointInFlux)
        {
            if (filterZeroPointInFlux)
            {
                if (filterZeroPoint > 0.0)
                {
                    return uncorrectedFlux * filterZeroPoint;
                }
                else
                {
                    return uncorrectedFlux;
                }
            }
            else
            {
                if (filterZeroPoint != 0.0)
                {
                    return uncorrectedFlux * Math.Pow(10.0, -filterZeroPoint / 2.5);
                }
                else
                {
                    return uncorrectedFlux;
                }
            }

        }

    }
}
