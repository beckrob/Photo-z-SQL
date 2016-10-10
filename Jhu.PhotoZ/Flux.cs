using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public class Flux : ValueWithErrorConvolveableFromFilterAndSpectrum, IEquatable<Flux>
    {

        public Flux()
        {
            Value = Constants.missingDouble;
            Error = Constants.missingDouble;
        }

        public Flux(Flux aFlux)
        {
            Value = aFlux.Value;
            Error = aFlux.Error;
        }

        public override Object Clone()
        {
            return new Flux(this);
        }

        public override void SetupFromUncorrectedFlux(double uncorrectedFlux, double filterZeroPoint, bool filterZeroPointInFlux)
        {

            Value = GetCorrectedFlux(uncorrectedFlux, filterZeroPoint, filterZeroPointInFlux);
            Error = 0.0;
        }

        public override bool ApplySchlegelExtinctionCorrection(Filter filt, double mapValue, double rVParam)
        {
            double mapFactor;
            if (filt.GetSchlegelMapFactor(rVParam, out mapFactor))
            {
                //mapFactor is \Delta Mag for E(B-V)=1
                //We need it for E(B-V)=mapValue
                Value *= Math.Pow(10, (mapValue * mapFactor) / 2.5);

                return true;
            }
            else
            {
                return false;
            }
        }

        public Magnitude ConvertToMagnitude(MagnitudeSystem.Type aTargetMagSystem)
        {
            return new Magnitude()
            {
                Value = MagnitudeSystem.GetMagnitudeFromCGSFlux(this.Value, aTargetMagSystem),
                Error = MagnitudeSystem.GetMagnitudeErrorFromCGSFluxAndError(this.Error, this.Value, aTargetMagSystem),
                MagSystem = aTargetMagSystem
            };
        }

        public override bool Equals(object aOther)
        {
            return aOther is Flux && Equals((Flux)aOther);
        }

        public bool Equals(Flux aOther)
        {
            return Value == aOther.Value && Error == aOther.Error;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Value.GetHashCode();
            return hash = hash * 23 + Error.GetHashCode();
        }

    }
}
