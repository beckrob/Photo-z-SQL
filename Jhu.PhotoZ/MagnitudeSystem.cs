using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public static class MagnitudeSystem
    {
        public enum Type { SDSS_u, SDSS_g, SDSS_r, SDSS_i, SDSS_z, AB };

        private const double ABFluxZeroPoint = 3.631e-20;
        //Applying -0.04 mag correction from SDSS to AB magnitude
        //f_0=3767.04 Jy
        private const double uSDSSFluxZeroPoint = 3.76704e-20;
        //Applying +0.02 mag correction from SDSS to AB magnitude
        //f_0=3564.51 Jy
        private const double zSDSSFluxZeroPoint = 3.56451e-20;


        public static double GetMagnitudeFromCGSFlux(double aFluxInCGS, Type sys)
        { //Flux is expected to be in cgs
            double x;

            switch (sys)
            {
                
                case Type.AB:
                    return -2.5 * Math.Log10(aFluxInCGS) - 48.6;

                case Type.SDSS_u:
                    x = 0.5 * aFluxInCGS / (uSDSSFluxZeroPoint * 1.4e-10);
                    return -2.5 / Math.Log(10) * (Math.Log(x + Math.Sqrt(x * x + 1)) + Math.Log(1.4e-10));

                case Type.SDSS_g:
                    x = 0.5 * aFluxInCGS / (ABFluxZeroPoint * 0.9e-10);
                    return -2.5 / Math.Log(10) * (Math.Log(x + Math.Sqrt(x * x + 1)) + Math.Log(0.9e-10));

                case Type.SDSS_r:
                    x = 0.5 * aFluxInCGS / (ABFluxZeroPoint * 1.2e-10);
                    return -2.5 / Math.Log(10) * (Math.Log(x + Math.Sqrt(x * x + 1)) + Math.Log(1.2e-10));

                case Type.SDSS_i:
                    x = 0.5 * aFluxInCGS / (ABFluxZeroPoint * 1.8e-10);
                    return -2.5 / Math.Log(10) * (Math.Log(x + Math.Sqrt(x * x + 1)) + Math.Log(1.8e-10));

                case Type.SDSS_z:
                    x = 0.5 * aFluxInCGS / (zSDSSFluxZeroPoint * 7.4e-10);
                    return -2.5 / Math.Log(10) * (Math.Log(x + Math.Sqrt(x * x + 1)) + Math.Log(7.4e-10));


                default:
                    return Constants.missingDouble;
            }
        }

        public static double GetMagnitudeErrorFromCGSFluxAndError(double aFluxErrorInCGS, double aFluxInCGS, Type sys)
        { //Flux is expected to be in cgs
            double x;

            switch (sys)
            {

                case Type.AB:
                    return Math.Abs(-2.5 / Math.Log(10) / aFluxInCGS * aFluxErrorInCGS);

                case Type.SDSS_u:
                    x = 0.5 * aFluxInCGS / (uSDSSFluxZeroPoint * 1.4e-10);
                    return Math.Abs(-2.5 / Math.Log(10) / Math.Sqrt(x * x + 1) * 0.5 / uSDSSFluxZeroPoint / 1.4e-10 * aFluxErrorInCGS);

                case Type.SDSS_g:
                    x = 0.5 * aFluxInCGS / (ABFluxZeroPoint * 0.9e-10);
                    return Math.Abs(-2.5 / Math.Log(10) / Math.Sqrt(x * x + 1) * 0.5 / ABFluxZeroPoint / 0.9e-10 * aFluxErrorInCGS);

                case Type.SDSS_r:
                    x = 0.5 * aFluxInCGS / (ABFluxZeroPoint * 1.2e-10);
                    return Math.Abs(-2.5 / Math.Log(10) / Math.Sqrt(x * x + 1) * 0.5 / ABFluxZeroPoint / 1.2e-10 * aFluxErrorInCGS);

                case Type.SDSS_i:
                    x = 0.5 * aFluxInCGS / (ABFluxZeroPoint * 1.8e-10);
                    return Math.Abs(-2.5 / Math.Log(10) / Math.Sqrt(x * x + 1) * 0.5 / ABFluxZeroPoint / 1.8e-10 * aFluxErrorInCGS);

                case Type.SDSS_z:
                    x = 0.5 * aFluxInCGS / (zSDSSFluxZeroPoint * 7.4e-10);
                    return Math.Abs(-2.5 / Math.Log(10) / Math.Sqrt(x * x + 1) * 0.5 / zSDSSFluxZeroPoint / 7.4e-10 * aFluxErrorInCGS);

                default:
                    return Constants.missingDouble;
            }
        }

        public static double GetSystemFluxZeroPoint(Type sys)
        {
            switch (sys)
            {
                case Type.AB:
                    return ABFluxZeroPoint;

                case Type.SDSS_u:
                    return uSDSSFluxZeroPoint;

                case Type.SDSS_g:
                    return ABFluxZeroPoint;

                case Type.SDSS_r:
                    return ABFluxZeroPoint;

                case Type.SDSS_i:
                    return ABFluxZeroPoint;

                case Type.SDSS_z:
                    return zSDSSFluxZeroPoint;

                default:
                    return Constants.missingDouble;
            }
        }


        public static double GetCGSFluxFromMagnitude(double mag, Type sys)
        { //Flux will be in cgs - except for the simple instrumental mag
            switch (sys)
            {
                case Type.AB:
                    return Math.Pow(10, (mag + 48.6) / (-2.5));

                case Type.SDSS_u:
                    return Math.Sinh(mag * Math.Log(10) / (-2.5) - Math.Log(1.4e-10)) * 2.0 * 1.4e-10 * uSDSSFluxZeroPoint;

                case Type.SDSS_g:
                    return Math.Sinh(mag * Math.Log(10) / (-2.5) - Math.Log(0.9e-10)) * 2.0 * 0.9e-10 * ABFluxZeroPoint;

                case Type.SDSS_r:
                    return Math.Sinh(mag * Math.Log(10) / (-2.5) - Math.Log(1.2e-10)) * 2.0 * 1.2e-10 * ABFluxZeroPoint;

                case Type.SDSS_i:
                    return Math.Sinh(mag * Math.Log(10) / (-2.5) - Math.Log(1.8e-10)) * 2.0 * 1.8e-10 * ABFluxZeroPoint;

                case Type.SDSS_z:
                    return Math.Sinh(mag * Math.Log(10) / (-2.5) - Math.Log(7.4e-10)) * 2.0 * 7.4e-10 * zSDSSFluxZeroPoint;


                default:
                    return Constants.missingDouble;
            }
        }

        public static double GetCGSFluxErrorFromMagnitudeAndError(double magError, double mag, Type sys)
        { //Flux will be in cgs - except for the simple instrumental mag
            switch (sys)
            {
                case Type.AB:
                    return Math.Abs(Math.Log(10) / (-2.5) * Math.Pow(10, (mag + 48.6) / (-2.5)) * magError);

                case Type.SDSS_u:
                    return Math.Abs(Math.Log(10) / (-2.5) * Math.Cosh(mag * Math.Log(10) / (-2.5) - Math.Log(1.4e-10)) * 2.0 * 1.4e-10 * uSDSSFluxZeroPoint * magError);

                case Type.SDSS_g:
                    return Math.Abs(Math.Log(10) / (-2.5) * Math.Cosh(mag * Math.Log(10) / (-2.5) - Math.Log(0.9e-10)) * 2.0 * 0.9e-10 * ABFluxZeroPoint * magError);

                case Type.SDSS_r:
                    return Math.Abs(Math.Log(10) / (-2.5) * Math.Cosh(mag * Math.Log(10) / (-2.5) - Math.Log(1.2e-10)) * 2.0 * 1.2e-10 * ABFluxZeroPoint * magError);

                case Type.SDSS_i:
                    return Math.Abs(Math.Log(10) / (-2.5) * Math.Cosh(mag * Math.Log(10) / (-2.5) - Math.Log(1.8e-10)) * 2.0 * 1.8e-10 * ABFluxZeroPoint * magError);

                case Type.SDSS_z:
                    return Math.Abs(Math.Log(10) / (-2.5) * Math.Cosh(mag * Math.Log(10) / (-2.5) - Math.Log(7.4e-10)) * 2.0 * 7.4e-10 * zSDSSFluxZeroPoint * magError);


                default:
                    return Constants.missingDouble;
            }
        }


    }
}
