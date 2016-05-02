using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public static class Cosmology
    {

        //Using omega_k = 1 - omega_m - omega_lambda
        public static double ComovingDistanceInHubbleDistance(double z, double omega_m = 0.3, double omega_lambda = 0.7, int resolution = 1000)
        {
            if (z > 0.0)
            {
                double omega_k = 1 - omega_m - omega_lambda;

                double[] x = new double[resolution];
                double[] fx = new double[resolution];

                //Integrating in t=sqrt(a)=sqrt(1/(1+z)) from t_0 to 1
                double t_0 = Math.Sqrt(1 / (1 + z));

                double stepSize = (1 - t_0) / (resolution - 1);

                for (int i = 0; i < resolution; ++i)
                {
                    x[i] = t_0 + i * stepSize;

                    double tSqr = x[i] * x[i];
                    double tSixth = tSqr * tSqr * tSqr;

                    fx[i] = 2.0 / Math.Sqrt(omega_m + omega_k * tSqr + omega_lambda * tSixth);
                }

                return SpecSvc.Util.Integral.Integrate(x, fx);
            }
            else
            {
                return 0.0;
            }
        }

        public static double TransverseComovingDistanceInHubbleDistance(double z, double omega_m = 0.3, double omega_lambda = 0.7, int resolution = 1000)
        {
            double omega_k = 1 - omega_m - omega_lambda;

            if (omega_k == 0.0)
            {
                return ComovingDistanceInHubbleDistance(z, omega_m, omega_lambda, resolution);
            }
            else if (omega_k > 0.0)
            {
                double sqrtOmega_k = Math.Sqrt(omega_k);
                return Math.Sinh(sqrtOmega_k * ComovingDistanceInHubbleDistance(z, omega_m, omega_lambda, resolution)) / sqrtOmega_k;
            }
            else
            {
                double sqrtOmega_k = Math.Sqrt(-omega_k);
                return Math.Sin(sqrtOmega_k * ComovingDistanceInHubbleDistance(z, omega_m, omega_lambda, resolution)) / sqrtOmega_k;
            }
        }

        public static double LuminosityDistanceInParsec(double z, double h = 0.7, double omega_m = 0.3, double omega_lambda = 0.7, int resolution = 1000)
        {
            return (1 + z) * TransverseComovingDistanceInHubbleDistance(z, omega_m, omega_lambda, resolution) * 2.9979e9 / h;
        }


        public static double DistanceModulus(double z, double h = 0.7, double omega_m = 0.3, double omega_lambda = 0.7, int resolution = 1000)
        {
            return 5.0 * (Math.Log10(LuminosityDistanceInParsec(z, h, omega_m, omega_lambda, resolution)) - 1.0);
        }

    }
}
