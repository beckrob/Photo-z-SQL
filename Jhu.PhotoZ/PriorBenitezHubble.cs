using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public class PriorBenitezHubble : PriorOnFluxInFilter
    {
        //This class is very specific, it has to be used with the 71 BPZ templates (HSCFilterPortal.dbo.Templates or TemplatesLogInterp)
        //Also, the filter should be the WFPC2 F814W (ID 265 in HSCFilterPortal, with MJD ~ 53000 as an estimate)

        //The class could be more general by storing which are the redshift and type parameters like the PriorOnTemplateType class,
        //and by allowing other interpolation versions, but with the present choice of template libraries this is not needed


        public PriorBenitezHubble(Filter aFilt, Template aTemp) : this(aFilt, aTemp, true) {}

        protected PriorBenitezHubble(Filter aFilt, Template aTemp, bool performChecksAndCalculation) : base(aFilt, aTemp, false)
        {
            if (performChecksAndCalculation)
            {

                if (aTemp is TemplateSimpleLibrary)
                {
                    if (((TemplateSimpleLibrary)aTemp).GetNumberOfTemplateSpectra() == 71)
                    {
                        DoPreCalculationsFromTemplate(aTemp);
                        return;
                    }

                }

                throw new ArgumentException("");
            }
        }


        public override bool VerifyParameterlist(List<TemplateParameter> aParams)
        {
            if (base.VerifyParameterlist(aParams))
            {
                if (aParams.Count > 2)
                {
                    if (aParams[2].Name == "TypeID" && aParams[1].Name == "Redshift" && aParams[0].Name == "Luminosity")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /*
        Parameters from Benitez (2000):
        E/S0	2.46+/-0.22	0.431+/-0.030	0.091+/-0.017		0.147+/-0.013       ID 0        35% at m0=20
        Sbc,Scd	1.81+/-0.10	0.390+/-0.024	0.0636+/-0.0090		0.450+/-0.036       ID 10,20    50% at m0=20
        Irr		0.91+/-0.05	0.063+/-0.013	0.123+/-0.012		...                 ID 30-70    remaining
        */
        public override double Evaluate(List<TemplateParameter> parameters)
        {
            double currentLum = parameters[0].Value;

            double fluxAtUnitLum;
            if (fluxesInFilter.TryGetValue(GetParameterDoubleArray(parameters), out fluxAtUnitLum))
            {

                double apparentMag = MagnitudeSystem.GetMagnitudeFromCGSFlux(fluxAtUnitLum * currentLum, MagnitudeSystem.Type.AB);

                if (apparentMag < 20.0)
                {
                    apparentMag = 20.0;
                }
                if (apparentMag > 32.0)
                {
                    apparentMag = 32.0;
                }

                return ComputeTypeAndRedshiftProb(apparentMag, parameters[1].Value, parameters[2].Value);
            }

            return 0.0;
        }

        protected virtual double ComputeTypeAndRedshiftProb(double aApparentMag, double aRedshift, double aTypeID)
        {
            double probTIfm0 = 0.0;
            double probZIfTAndm0 = 0.0;

            double typeID = Math.Round(aTypeID);
            if (typeID == 0.0)
            {
                probTIfm0 = ComputeEllipticalTypeProb(aApparentMag);
                probZIfTAndm0 = ComputeEllipticalRedshiftProb(aApparentMag, aRedshift);
            }
            else if (typeID > 0.0 && typeID < 10.0)
            {//Interpolation between types
                double where = typeID / 10.0;

                probTIfm0 = where * ComputeSpiralTypeProb(aApparentMag) + (1 - where) * ComputeEllipticalTypeProb(aApparentMag);

                probZIfTAndm0 = where * ComputeSpiralRedshiftProb(aApparentMag, aRedshift)
                                + (1 - where) * ComputeEllipticalRedshiftProb(aApparentMag, aRedshift);
            }
            else if (typeID >= 10.0 && typeID <= 20.0)
            {
                probTIfm0 = ComputeSpiralTypeProb(aApparentMag);
                probZIfTAndm0 = ComputeSpiralRedshiftProb(aApparentMag, aRedshift);
            }
            else if (typeID > 20.0 && typeID < 30.0)
            {//Interpolation between types
                double where = (typeID - 20.0) / 10.0;

                probTIfm0 = where * ComputeIrregularTypeProb(aApparentMag) + (1 - where) * ComputeSpiralTypeProb(aApparentMag);

                probZIfTAndm0 = where * ComputeIrregularRedshiftProb(aApparentMag, aRedshift)
                                + (1 - where) * ComputeSpiralRedshiftProb(aApparentMag, aRedshift);
            }
            else if (typeID >= 30.0 && typeID <= 70.0)
            {
                probTIfm0 = ComputeIrregularTypeProb(aApparentMag);
                probZIfTAndm0 = ComputeIrregularRedshiftProb(aApparentMag, aRedshift);
            }

            return probTIfm0 * probZIfTAndm0;
        }

        protected static double ComputeEllipticalTypeProb(double aAppMag, double aTypeNorm = 5.0)
        {
            return 0.35 * Math.Exp(-0.147 * (aAppMag - 20.0)) / aTypeNorm;
        }

        protected static double ComputeSpiralTypeProb(double aAppMag, double aTypeNorm = 20.0)
        {
            return 0.50 * Math.Exp(-0.450 * (aAppMag - 20.0)) / aTypeNorm;
        }

        protected static double ComputeIrregularTypeProb(double aAppMag, double aTypeNorm = 45.0)
        {
            return (1.0 - ComputeEllipticalTypeProb(aAppMag, 1.0) - ComputeSpiralTypeProb(aAppMag, 1.0)) / aTypeNorm;
        }

        protected static double ComputeEllipticalRedshiftProb(double aAppMag, double aRedshift)
        {
            return Math.Pow(aRedshift, 2.46) * Math.Exp(-Math.Pow(aRedshift / (0.431 + 0.091 * (aAppMag-20.0)), 2.46));
        }

        protected static double ComputeSpiralRedshiftProb(double aAppMag, double aRedshift)
        {
            return Math.Pow(aRedshift, 1.81) * Math.Exp(-Math.Pow(aRedshift / (0.390 + 0.0636 * (aAppMag-20.0)), 1.81));
        }

        protected static double ComputeIrregularRedshiftProb(double aAppMag, double aRedshift)
        {
            return Math.Pow(aRedshift, 0.91) * Math.Exp(-Math.Pow(aRedshift / (0.063 + 0.123 * (aAppMag-20.0)), 0.91));
        }

    }
}
