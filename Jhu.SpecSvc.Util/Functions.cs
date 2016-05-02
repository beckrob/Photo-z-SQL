using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.SpecSvc.Util
{
    public static class Functions
    {
        public static readonly double OneOverSqrt2Pi = 1 / Math.Sqrt(2 * Math.PI);
        public static readonly double OneOverSqrtPi = 1 / Math.Sqrt(2 * Math.PI);

        public static double Gauss(double x, double a, double m, double s)
        {
            return a * OneOverSqrt2Pi / s * Math.Exp(-(x - m) * (x - m) / (2 * s * s));
        }

        public static double SkewGauss(double x, double a, double m, double s, double sk)
        {
            return a * OneOverSqrtPi / s * Math.Exp(-(x - m) * (x - m) / (2 * s * s)) *
                (1 + Erf(sk / Math.Sqrt(2) * (x - m) / s));
        }

        public static double Erf(double x)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0) sign = -1;

            x = Math.Abs(x);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }

        //Evaluates a polynomial, i.e. sum_{i=0}^{coefficients.Length-1} x^i*coefficients[i] 
        public static double Polynomial(double x, double[] coefficients)
        {
            double sum = 0.0;
            double variableToPower = 1.0;

            for (int i = 0; i < coefficients.Length; ++i)
            {
                sum += variableToPower * coefficients[i];

                variableToPower *= x;
            }

            return sum;
        }

    }
}
