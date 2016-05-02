using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public class Magnitude : ValueWithErrorConvolveableFromFilterAndSpectrum, IEquatable<Magnitude>
    {

        public MagnitudeSystem.Type MagSystem { get; set; }

        public Magnitude()
        {
            Value = Constants.missingDouble;
            Error = Constants.missingDouble;
            MagSystem = MagnitudeSystem.Type.AB;
        }

        public Magnitude(Magnitude aMagnitude)
        {
            Value = aMagnitude.Value;
            Error = aMagnitude.Error;
            MagSystem = aMagnitude.MagSystem;
        }

        public override void SetupFromUncorrectedFlux(double uncorrectedFlux, double filterZeroPoint, bool filterZeroPointInFlux)
        {

            Value = MagnitudeSystem.GetMagnitudeFromCGSFlux(GetCorrectedFlux(uncorrectedFlux, filterZeroPoint, filterZeroPointInFlux), MagSystem);
            Error = 0.0;
        }

        public bool ApplySchlegelExtinctionCorrection(Filter filt, double mapValue, double rVParam)
        {
            double mapFactor;
            if (filt.GetSchlegelMapFactor(rVParam, out mapFactor))
            {
                //mapFactor is \Delta Mag for E(B-V)=1
                //We need it for E(B-V)=mapValue
                Value -= mapValue * mapFactor;

                return true;
            }
            else
            {
                return false;
            }
        }

        public Magnitude ConvertToMagnitudeSystem(MagnitudeSystem.Type aTargetMagSystem)
        {
            if (MagSystem != aTargetMagSystem)
            {
                double flux = MagnitudeSystem.GetCGSFluxFromMagnitude(Value, MagSystem);
                double fluxError = MagnitudeSystem.GetCGSFluxErrorFromMagnitudeAndError(Error, Value, MagSystem);

                Value = MagnitudeSystem.GetMagnitudeFromCGSFlux(flux, aTargetMagSystem);
                Error = MagnitudeSystem.GetMagnitudeErrorFromCGSFluxAndError(fluxError, flux, aTargetMagSystem);
                MagSystem = aTargetMagSystem;
            }
            return this;
        }

        public Flux ConvertToFlux()
        {
            return new Flux() 
            {
                Value = MagnitudeSystem.GetCGSFluxFromMagnitude(this.Value, MagSystem),
                Error = MagnitudeSystem.GetCGSFluxErrorFromMagnitudeAndError(this.Error, this.Value, MagSystem)
            };
        }

        public override bool Equals(object aOther)
        {
            return aOther is Magnitude && Equals((Magnitude)aOther);
        }

        public bool Equals(Magnitude aOther)
        {
            return Value == aOther.Value && Error == aOther.Error && MagSystem == aOther.MagSystem;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Value.GetHashCode();
            hash = hash * 23 + Error.GetHashCode();
            return hash * 23 + MagSystem.GetHashCode();
        }


    }
}
