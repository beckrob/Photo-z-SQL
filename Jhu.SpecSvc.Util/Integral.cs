using System;
using System.Collections.Generic;
using System.Text;

namespace Jhu.SpecSvc.Util
{
    public static class Integral
    {
        public delegate double ConvolutionFunction(double dx, double x);

        public static double Integrate(double[] x, double[] y)
        {
            double sum = 0;
            double xa = 0;
            double xb = 0;

            for (int i = 0; i < x.Length; i++)
            {
                if (i == 0)
                {
                    xa = x[i] - (x[i + 1] - x[i]) / 2;
                }
                else
                {
                    //xa = x[i] - (x[i] - x[i - 1]) / 2;
                    xa = xb;
                }

                if (i == x.Length - 1)
                {
                    xb = x[i] + (x[i] - x[i - 1]) / 2;
                }
                else
                {
                    xb = x[i] + (x[i + 1] - x[i]) / 2;
                }

                sum += (xb - xa) * y[i];
            }

            return sum;
        }

        public static double Integrate(double[] xmin, double[] xmax, double[] y, double xa, double xb)
        {
            return Integrate(xmin, xmax, y, xa, xb, null);
        }

        public static double Integrate(double[] xmin, double[] xmax, double[] y, double xa, double xb,
            Func<double,double> mult)
        {
            double[] res;

            Integrate(xmin, xmax, new double[][] { y }, xa, xb, out res, mult);

            return res[0];
        }

        public static void Integrate(double[] xmin, double[] xmax, double[][] y, double xa, double xb, out double[] integral)
        {
            Integrate(xmin, xmax, y, xa, xb, out integral, null);
        }

        public static void Integrate(double[] xmin, double[] xmax, double[][] y, double xa, double xb, out double[] integral,
            Func<double, double> mult)
        {
            integral = new double[y.Length];

            // Check out of the range
            if (xa < xmin[0] || xb >= xmax[xmax.Length - 1])
            {
                for (int i = 0; i < integral.Length; i++)
                {
                    integral[i] = double.NaN;
                }

                return;
            }

            for (int i = 0; i < xmin.Length; i++)
            {
                double a = 0.0;
                double b = 0.0;

                if (xb <= xmin[i] || xa >= xmax[i])
                {
                    continue;
                }
                else if (xa <= xmin[i] && xb >= xmax[i])
                {
                    a = xmin[i];
                    b = xmax[i];
                }
                else if (xa <= xmin[i] && xb < xmax[i] && xb > xmin[i])
                {
                    a = xmin[i];
                    b = xb;
                }
                else if (xa > xmin[i] && xa < xmax[i] && xb >= xmax[i])
                {
                    a = xa;
                    b = xmax[i];
                }
                else if (xa > xmin[i] && xa < xmax[i] && xb > xmin[i] && xb < xmax[i])
                {
                    a = xa;
                    b = xb;
                }
                else
                {
                    throw new Exception();
                }

                double bin = b - a;
                double m = 0.0;

                if (mult != null)
                {
                    m = mult(0.5 * (a + b));
                }

                for (int j = 0; j < y.Length; j++)
                {
                    double t = y[j][i] * bin;

                    if (mult != null)
                    {
                        t *= m;
                    }

                    integral[j] += t;
                }
            }
        }

        public static double Median(double[] x, double[] y, long[] mask, double xa, double xb)
        {
            double[] median;

            Median(x, new double[][] { y }, mask, xa, xb, out median);

            return median[0];
        }

        public static void Median(double[] x, double[][] y, long[] mask, double xa, double xb, out double[] median)
        {
            List<double>[] temp = new List<double>[y.Length];

            for (int j = 0; j < y.Length; j++)
            {
                if (y[j] != null)
                {
                    temp[j] = new List<double>();
                }
            }

            for (int i = 0; i < x.Length; i++)
            {
                if (xa <= x[i] && x[i] <= xb)
                    for (int j = 0; j < y.Length; j++)
                    {
                        if (y[j] != null && (mask == null || mask[j] == 0))
                        {
                            temp[j].Add(y[j][i]);
                        }
                    }
            }

            median = new double[y.Length];
            for (int j = 0; j < y.Length; j++)
            {
                if (y[j] != null && temp[j].Count > 0)
                {
                    temp[j].Sort();
                    median[j] = temp[j][temp[j].Count / 2]; // median
                }
                else
                {
                    median[j] = double.NaN;
                }
            }
        }

        public static double Average(double[] x, double[] y, bool[] mask, double xa, double xb)
        {
            double[] averagea;

            Average(x, new double[][] { y }, mask, xa, xb, out averagea);

            return averagea[0];
        }

        public static void Average(double[] x, double[][] y, bool[] mask, double xa, double xb, out double[] average)
        {
            average = new double[y.Length];
            int[] count = new int[y.Length];

            for (int i = 0; i < x.Length; i++)
            {
                if (xa <= x[i] && x[i] <= xb)
                {
                    for (int j = 0; j < y.Length; j++)
                    {
                        if (y[j] != null && (mask == null || !mask[j]))
                        {
                            average[j] += y[j][i];
                            count[j]++;
                        }
                    }
                }
            }

            for (int j = 0; j < average.Length; j++)
            {
                if (y[j] != null)
                {
                    average[j] /= count[j];
                }
            }
        }

        public static void AverageStdev(double[] x, double[] y, bool[] mask, double xa, double xb, out double average, out double stdev)
        {
            double[] averagea;
            double[] stdeva;

            AverageStdev(x, new double[][] { y }, mask, xa, xb, out averagea, out stdeva);

            average = averagea[0];
            stdev = stdeva[0];
        }

        public static void AverageStdev(double[] x, double[][] y, bool[] mask, double xa, double xb, out double[] average, out double[] stdev)
        {
            average = new double[y.Length];
            stdev = new double[y.Length];
            int[] count = new int[y.Length];

            for (int i = 0; i < x.Length; i++)
            {
                if (xa <= x[i] && x[i] <= xb)
                {
                    for (int j = 0; j < y.Length; j++)
                    {
                        if (y[j] != null && (mask == null || !mask[j]))
                        {
                            average[j] += y[j][i];
                            stdev[j] += y[j][i] * y[j][i];
                            count[j]++;
                        }
                    }
                }
            }

            for (int j = 0; j < average.Length; j++)
            {
                if (y[j] != null)
                {
                    average[j] /= count[j];
                    stdev[j] /= count[j];
                    stdev[j] -= average[j] * average[j];
                    stdev[j] = Math.Sqrt(stdev[j]);
                }
            }
        }

        public static void ConvolveSymmetricKernel(double[] y, double[] kernel, out double[] ny)
        {
            double[][] nny;
            ConvolveSymmetricKernel(new double[][] { y }, kernel, out nny);
            ny = nny[0];
        }

        public static void ConvolveSymmetricKernel(double[][] y, double[] kernel, out double[][] ny)
        {
            // Normalize kernel
            double norm = 0;
            for (int k = 0; k < kernel.Length; k++)
            {
                if (k == 0)
                {
                    norm += kernel[k];
                }
                else
                {
                    norm += 2 * kernel[k];
                }
            }

            for (int k = 0; k < kernel.Length; k++)
            {
                kernel[k] /= norm;
            }


            // Do the convolution

            ny = new double[y.Length][];

            for (int i = 0; i < y.Length; i ++)
            {
                if (y[i] != null)
                {
                    double[] yi = y[i];
                    ny[i] = new double[y[i].Length];

                    for (int j = kernel.Length; j < y[i].Length - kernel.Length; j++)
                    {
                        double nyij = 0;
                        
                        for (int k = 0; k < kernel.Length; k++)
                        {
                            if (k == 0)
                            {
                                nyij += kernel[k] * yi[j];
                            }
                            else
                            {
                                nyij += kernel[k] * (yi[j - k] + yi[j + k]);
                            }
                        }

                        ny[i][j] = nyij;
                    }
                }
                else
                {
                    ny[i] = null;
                }
            }
        }

        public static void Convolve(double[] x, double[] xmin, double[] xmax, double[][] y, double supMin, double supMax, int oversampling, ConvolutionFunction kernel, out double[][] ny)
        {
            // Initialize output arrays
            ny = new double[y.Length][];
            for (int i = 0; i < y.Length; i++)
            {
                ny[i] = (y[i] == null) ? null : new double[x.Length];
            }

            // Loop over new bins
            for (int np = 0; np < x.Length; np++)
            {

                // This will sum the contribution from every other bins
                // for every function we are convolving
                double[] acc = new double[y.Length];
                double norm = 0;

                // Loop over all old bins
                for (int op = 0; op < x.Length; op++)
                {
                    // check if old bin is entirely within the support
                    if (x[np] + supMin <= xmin[op] && xmax[op] <= x[np] + supMax)
                    {

#if false
                        NumericalRecipes.IntegrationOfFunctions.Qsimp intg = new NumericalRecipes.IntegrationOfFunctions.Qsimp();

                        double f = intg.qsimp(
                            delegate(double xx)
                            {
                                return kernel(x[np] - xx, x[np]);
                            },
                            xmin[op],
                            xmax[op]);

                        norm += f;

                        for (int l = 0; l < acc.Length; l++)
                        {
                            if (y[l] != null)
                            {
                                acc[l] += y[l][op] * f;
                            }
                        }
#else 
                        // Old code, replaced with Qsimp above
                        double binsize = xmax[op] - xmin[op];
                        double xinc = binsize / oversampling;

                        // Loop to do the oversampling
                        double a, b;
                        double fa, fb;
                        b = xmin[op];
                        fb = kernel(b - x[np], x[np]);
                        for (int i = 0; i < oversampling; i++)
                        {
                            a = b;
                            b = a + xinc;

                            fa = fb;
                            fb = kernel(b - x[np], x[np]);

                            norm += 0.5 * (fa + fb) * xinc;

                            // Accumulate from old bins
                            for (int l = 0; l < acc.Length; l++)
                            {
                                if (y[l] != null)
                                {
                                    acc[l] += 0.5 * (fa + fb) * y[l][op] * xinc;
                                }
                            }
                        }
#endif
                    }
                }

                // Copy to the results array
                for (int l = 0; l < acc.Length; l++)
                {
                    if (ny[l] != null)
                    {
                        if (norm == 0.0)
                        {
                            ny[l][np] = y[l][np];
                        }
                        else
                        {
                            ny[l][np] = acc[l] / norm;
                        }
                    }
                }
            }
        }

        /*public static void Convolve(double[] x, double[] xmin, double[] xmax, double[][] y, double supMin, double supMax, int oversampling, ConvolutionFunction kernel, out double[][] ny)
        {
            ny = new double[y.Length][];
            for (int i = 0; i < y.Length; i++)
            {
                ny[i] = (y[i] == null) ? null : new double[x.Length];
            }

            for (int np = 0; np < x.Length; np++)
            {
                double[] acc = new double[y.Length];

                // && (double.IsInfinity(supMax) || this.Spectral_Acc[k] - this.Spectral_Value[i] <= supMax)
                //while (k < x.Length)

                for (int op = 0; op < x.Length; op++)
                {
                    double xbinsize = (xmax[op] - xmin[op]);
                    double xinc = xbinsize / oversampling;

                    for (int dx = 0; dx < oversampling; dx++)
                    {
                        double xc = xmin[op] + dx * xinc;

                        double min = xc - xmax[np];
                        double max = xc - xmin[np];

                        if (max > supMin && min < supMax)
                        {
                            double binsize = (max - min);

                            double inc = binsize / oversampling;
                            double a = min;
                            double b = a + inc;
                            for (int q = 0; q < oversampling; q++)
                            {
                                double fa = kernel(a, x[np]);
                                double fb = kernel(b, x[np]);

                                for (int l = 0; l < y.Length; l++)
                                {
                                    if (y[l] != null)
                                    {
                                        acc[l] += (fa + fb) / 2 * inc * xinc * y[l][op];
                                    }
                                }

                                a = b;
                                fa = fb;
                            }
                        }
                    }
                }

                for (int l = 0; l < y.Length; l++)
                {
                    if (ny[l] != null)
                    {
                        ny[l][np] = acc[l] / (xmax[np] - xmin[np]);
                    }
                }
            }
        }*/

        public static double Integrate(double[] x, double[] y, double[] fx, double[] fy, out bool error)
        {
            // make sure that filter covers spectrum
            error = (fx[0] < x[0] || fx[fx.Length - 1] > x[x.Length - 1]);

            double flux = 0.0;
            double filt = 0.0;
            int fp = 0;		// filter pointer
            int sp = 0;		// spectrum pointer

            int fpa = -1, fpb = -1;

            double fla = 0.0, flb = 0.0;
            double wla = 0.0, wlb = 0.0;
            double ra = 0.0, rb = 0.0;

            // filter is entirely outsite the spectral coverage
            if (fx[0] > x[x.Length - 1] ||
                fx[fx.Length - 1] < x[0])
            {
                error = true;
                return double.NaN;
            }

            while (sp < y.Length)
            {

                // it runs only in the first iteration and steps the spectrum pointer right after the first existing
                // filter point
                if (fx[fp] > x[sp] && fpa == -1)
                {
                    while (fx[fp] > x[sp])
                    {
                        sp++;
                    }
                }

                while (fx[fp] < x[sp])
                {
                    fp++;
                    if (fp == fx.Length)
                        return flux / filt;
                }

                if (fpa == -1)
                {
                    fpa = (fp == 0) ? 1 : fp;	//if filter first point exactly matches a spectrum point
                    fla = y[sp];
                    wla = x[sp];
                    ra = fy[fpa - 1] + (fy[fpa] - fy[fpa - 1]) / (fx[fpa] - fx[fpa - 1]) * (wla - fx[fpa - 1]);
                    sp++;
                    continue;
                }

                fpb = fp;
                flb = y[sp];
                wlb = x[sp];
                rb = fy[fpb - 1] + (fy[fpb] - fy[fpb - 1]) / (fx[fpb] - fx[fpb - 1]) * (wlb - fx[fpb - 1]);


                flux += (fla * ra * wla + flb * rb * wlb) / 2 * (wlb - wla);
                filt += (ra / wla + rb / wlb) / 2 * (wlb - wla);

                fla = flb;
                fpa = fpb;
                wla = wlb;
                ra = rb;

                sp++;
            }

            return flux / filt;

        }

        //This function integrates over a filter multiplied by a spectrum multiplied by a function that is evaluated at every wavelength
        //Also, it normalizes with the integral of the filter multiplied by the spectrum
        public static double IntegrateFilterSpectrumFunction(double[] x, double[] y, double[] fx, double[] fy, Func<double, double> function, out bool error)
        {
            // make sure that filter covers spectrum
            error = (fx[0] < x[0] || fx[fx.Length - 1] > x[x.Length - 1]);

            double flux = 0.0;
            double filt = 0.0;
            int fp = 0;		// filter pointer
            int sp = 0;		// spectrum pointer

            int fpa = -1, fpb = -1;

            double fla = 0.0, flb = 0.0;
            double wla = 0.0, wlb = 0.0;
            double ra = 0.0, rb = 0.0;

            // filter is entirely outsite the spectral coverage
            if (fx[0] > x[x.Length - 1] ||
                fx[fx.Length - 1] < x[0])
            {
                error = true;
                return double.NaN;
            }

            while (sp < y.Length)
            {

                // it runs only in the first iteration and steps the spectrum pointer right after the first existing
                // filter point
                if (fx[fp] > x[sp] && fpa == -1)
                {
                    while (fx[fp] > x[sp])
                    {
                        sp++;
                    }
                }

                while (fx[fp] < x[sp])
                {
                    fp++;
                    if (fp == fx.Length)
                        return flux / filt;
                }

                if (fpa == -1)
                {
                    fpa = (fp == 0) ? 1 : fp;	//if filter first point exactly matches a spectrum point
                    fla = y[sp];
                    wla = x[sp];
                    ra = fy[fpa - 1] + (fy[fpa] - fy[fpa - 1]) / (fx[fpa] - fx[fpa - 1]) * (wla - fx[fpa - 1]);
                    sp++;
                    continue;
                }

                fpb = fp;
                flb = y[sp];
                wlb = x[sp];
                rb = fy[fpb - 1] + (fy[fpb] - fy[fpb - 1]) / (fx[fpb] - fx[fpb - 1]) * (wlb - fx[fpb - 1]);


                flux += (fla * ra * function(wla) + flb * rb * function(wlb)) / 2 * (wlb - wla);
                filt += (fla * ra + flb * rb) / 2 * (wlb - wla);

                fla = flb;
                fpa = fpb;
                wla = wlb;
                ra = rb;

                sp++;
            }

            return flux / filt;

        }


    }
}
