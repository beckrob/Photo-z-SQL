using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public static class MagnitudeSystem
    {
        public enum Type { SDSS_u, SDSS_g, SDSS_r, SDSS_i, SDSS_z, AB };

        public static double GetMagnitudeFromCGSFlux(double aFluxInCGS, Type sys)
        { //Flux is expected to be in cgs
            double x;

            if (aFluxInCGS == 0.0)
            {
                //TODO replace with something more reasonable than a lower bound, or make the lower bound configurable
                return 36.0;
            }

            switch (sys)
            {
                
                case Type.AB:
                    return -2.5 * Math.Log10(aFluxInCGS) - 48.6;

                case Type.SDSS_u:
                    //Applying -0.04 mag correction from SDSS to AB magnitude
                    //f_0=3767.04 Jy
                    x = 0.5 * aFluxInCGS / (3.76704e-20 * 1.4e-10);
                    return -2.5 / Math.Log(10) * (Math.Log(x + Math.Sqrt(x * x + 1)) + Math.Log(1.4e-10));

                case Type.SDSS_g:
                    //f_0=3631 Jy
                    x = 0.5 * aFluxInCGS / (3.631e-20 * 0.9e-10);
                    return -2.5 / Math.Log(10) * (Math.Log(x + Math.Sqrt(x * x + 1)) + Math.Log(0.9e-10));

                case Type.SDSS_r:
                    //f_0=3631 Jy
                    x = 0.5 * aFluxInCGS / (3.631e-20 * 1.2e-10);
                    return -2.5 / Math.Log(10) * (Math.Log(x + Math.Sqrt(x * x + 1)) + Math.Log(1.2e-10));

                case Type.SDSS_i:
                    //f_0=3631 Jy
                    x = 0.5 * aFluxInCGS / (3.631e-20 * 1.8e-10);
                    return -2.5 / Math.Log(10) * (Math.Log(x + Math.Sqrt(x * x + 1)) + Math.Log(1.8e-10));

                case Type.SDSS_z:
                    //Applying +0.02 mag correction from SDSS to AB magnitude
                    //f_0=3564.51 Jy
                    x = 0.5 * aFluxInCGS / (3.56451e-20 * 7.4e-10);
                    return -2.5 / Math.Log(10) * (Math.Log(x + Math.Sqrt(x * x + 1)) + Math.Log(7.4e-10));


                default:
                    return Constants.missingDouble;
            }
        }

        public static double GetMagnitudeErrorFromCGSFluxAndError(double aFluxErrorInCGS, double aFluxInCGS, Type sys)
        { //Flux is expected to be in cgs
            double x;

            if (aFluxInCGS == 0.0)
            {
                //TODO replace with something more reasonable
                return 3.0;
            }

            switch (sys)
            {

                case Type.AB:
                    return Math.Abs(-2.5 / Math.Log(10) / aFluxInCGS * aFluxErrorInCGS);

                case Type.SDSS_u:
                    //Applying -0.04 mag correction from SDSS to AB magnitude
                    //f_0=3767.04 Jy
                    x = 0.5 * aFluxInCGS / (3.76704e-20 * 1.4e-10);
                    return Math.Abs(-2.5 / Math.Log(10) / Math.Sqrt(x * x + 1) * 0.5 / 3.76704e-20 / 1.4e-10 * aFluxErrorInCGS);

                case Type.SDSS_g:
                    //f_0=3631 Jy
                    x = 0.5 * aFluxInCGS / (3.631e-20 * 0.9e-10);
                    return Math.Abs(-2.5 / Math.Log(10) / Math.Sqrt(x * x + 1) * 0.5 / 3.631e-20 / 0.9e-10 * aFluxErrorInCGS);

                case Type.SDSS_r:
                    //f_0=3631 Jy
                    x = 0.5 * aFluxInCGS / (3.631e-20 * 1.2e-10);
                    return Math.Abs(-2.5 / Math.Log(10) / Math.Sqrt(x * x + 1) * 0.5 / 3.631e-20 / 1.2e-10 * aFluxErrorInCGS);

                case Type.SDSS_i:
                    //f_0=3631 Jy
                    x = 0.5 * aFluxInCGS / (3.631e-20 * 1.8e-10);
                    return Math.Abs(-2.5 / Math.Log(10) / Math.Sqrt(x * x + 1) * 0.5 / 3.631e-20 / 1.8e-10 * aFluxErrorInCGS);

                case Type.SDSS_z:
                    //Applying +0.02 mag correction from SDSS to AB magnitude
                    //f_0=3564.51 Jy
                    x = 0.5 * aFluxInCGS / (3.56451e-20 * 7.4e-10);
                    return Math.Abs(-2.5 / Math.Log(10) / Math.Sqrt(x * x + 1) * 0.5 / 3.56451e-20 / 7.4e-10 * aFluxErrorInCGS);

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
                    //Applying -0.04 mag correction from SDSS to AB magnitude
                    //f_0=3767.04 Jy
                    return Math.Sinh(mag * Math.Log(10) / (-2.5) - Math.Log(1.4e-10)) * 2.0 * 1.4e-10 * 3.76704e-20;

                case Type.SDSS_g:
                    //f_0=3631 Jy
                    return Math.Sinh(mag * Math.Log(10) / (-2.5) - Math.Log(0.9e-10)) * 2.0 * 0.9e-10 * 3.631e-20;

                case Type.SDSS_r:
                    //f_0=3631 Jy
                    return Math.Sinh(mag * Math.Log(10) / (-2.5) - Math.Log(1.2e-10)) * 2.0 * 1.2e-10 * 3.631e-20;

                case Type.SDSS_i:
                    //f_0=3631 Jy
                    return Math.Sinh(mag * Math.Log(10) / (-2.5) - Math.Log(1.8e-10)) * 2.0 * 1.8e-10 * 3.631e-20;

                case Type.SDSS_z:
                    //Applying +0.02 mag correction from SDSS to AB magnitude
                    //f_0=3564.51 Jy
                    return Math.Sinh(mag * Math.Log(10) / (-2.5) - Math.Log(7.4e-10)) * 2.0 * 7.4e-10 * 3.56451e-20;


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
                    //Applying -0.04 mag correction from SDSS to AB magnitude
                    //f_0=3767.04 Jy
                    return Math.Abs(Math.Log(10) / (-2.5) * Math.Cosh(mag * Math.Log(10) / (-2.5) - Math.Log(1.4e-10)) * 2.0 * 1.4e-10 * 3.76704e-20 * magError);

                case Type.SDSS_g:
                    //f_0=3631 Jy
                    return Math.Abs(Math.Log(10) / (-2.5) * Math.Cosh(mag * Math.Log(10) / (-2.5) - Math.Log(0.9e-10)) * 2.0 * 0.9e-10 * 3.631e-20 * magError);

                case Type.SDSS_r:
                    //f_0=3631 Jy
                    return Math.Abs(Math.Log(10) / (-2.5) * Math.Cosh(mag * Math.Log(10) / (-2.5) - Math.Log(1.2e-10)) * 2.0 * 1.2e-10 * 3.631e-20 * magError);

                case Type.SDSS_i:
                    //f_0=3631 Jy
                    return Math.Abs(Math.Log(10) / (-2.5) * Math.Cosh(mag * Math.Log(10) / (-2.5) - Math.Log(1.8e-10)) * 2.0 * 1.8e-10 * 3.631e-20 * magError);

                case Type.SDSS_z:
                    //Applying +0.02 mag correction from SDSS to AB magnitude
                    //f_0=3564.51 Jy
                    return Math.Abs(Math.Log(10) / (-2.5) * Math.Cosh(mag * Math.Log(10) / (-2.5) - Math.Log(7.4e-10)) * 2.0 * 7.4e-10 * 3.56451e-20 * magError);


                default:
                    return Constants.missingDouble;
            }
        }


    }
}
