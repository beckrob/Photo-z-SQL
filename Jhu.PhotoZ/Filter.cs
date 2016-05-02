using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Jhu.PhotoZ
{
    public class Filter
    {
        private double[] binCenters;
        private double[] responses;

        public double EffectiveWavelength { get; set; }

        //Stores zeropoint calibration result
        public double ZeroPointCorrection { get; set; }
        //Stores the RMS of the zeropoint calibration, which may be added to magnitude/flux errors to distinguish problematic filters  
        public double ZeroPointErrorCalibration { get; set; }
        public bool ZeroPointCorrectionInFlux { get; set; }



        //Stored map factors for given R_V gas parameters
        //Make sure to reset the map factors whenever the filter response curve is modified,
        //or when the reference spectrum is modified
        private Dictionary<double, double> RVschlegelMapFactorCache;
        private Spectrum referenceSpectrum;

        private const double smallMValueToExtrapolate = 0.001;

        private bool useFitzPatrickExtinction;
        public bool UseFitzpatrickExtinction
        {
            get
            {
                return useFitzPatrickExtinction;
            }

            set
            {
                useFitzPatrickExtinction = value;
                RVschlegelMapFactorCache = null;
            }
        }


        public double[] GetBinCenters()
        {
            return binCenters;
        }

        public double[] GetResponses()
        {
            return responses;
        }

        public Filter()
        {
            binCenters = null;
            responses = null;
            RVschlegelMapFactorCache = null;
            referenceSpectrum = null;
            useFitzPatrickExtinction = true;
            EffectiveWavelength = Constants.missingDouble;
            ZeroPointCorrection = 0.0;
            ZeroPointCorrectionInFlux = false;
            ZeroPointErrorCalibration = 1.0;
        }

        public Filter(double[] aBinCenter, double[] aResponses, double aEffectiveWavelength, double aZeroPointCorrection = 0.0, bool aZeroPointCorrectionInFlux = false)
        {
            binCenters = aBinCenter;
            responses = aResponses;
            RVschlegelMapFactorCache = null;
            referenceSpectrum = null;
            useFitzPatrickExtinction = true;
            EffectiveWavelength = aEffectiveWavelength;
            ZeroPointCorrection = aZeroPointCorrection;
            ZeroPointCorrectionInFlux = aZeroPointCorrectionInFlux;
            ZeroPointErrorCalibration = 1.0;
        }

        //Filter read in from file, expected to be in angstroms
        public Filter(string aFilePath)
        {
            binCenters = null;
            responses = null;
            RVschlegelMapFactorCache = null;
            referenceSpectrum = null;
            useFitzPatrickExtinction = true;
            EffectiveWavelength = Constants.missingDouble;
            ZeroPointCorrection = 0.0;
            ZeroPointCorrectionInFlux = false;
            ZeroPointErrorCalibration = 1.0;

            using (StreamReader reader = new StreamReader(aFilePath))
            {
                List<double> lambdaList = new List<double>();
                List<double> throughputList = new List<double>();

                string line;
                while ( (line=reader.ReadLine()) != null)
                {
                    string[] fields=line.Split( (char[])null, StringSplitOptions.RemoveEmptyEntries);

                    //Ignore empty, short or comment lines
                    if (fields.Length > 1 && fields[0].Length > 0 && fields[0][0] != '#')
                    {

                        //First two coloumns will be parsed into doubles
                        double value1, value2;
                        if (double.TryParse(fields[0], out value1) && double.TryParse(fields[1], out value2))
                        {
                            lambdaList.Add(value1);
                            throughputList.Add(value2);
                        }

                    }

                }

                if (lambdaList.Count > 0){
                    binCenters = lambdaList.ToArray();
                    responses = throughputList.ToArray();

                    //Make sure that the ordering of the filter is correct
                    Array.Sort(binCenters, responses);
                }
            }       
        }

        public override string ToString()
        {
            string res = "";

            for (int i = 0; i < binCenters.Length; ++i)
            {
                res += binCenters[i].ToString() + '\t' + responses[i].ToString() + Environment.NewLine;
            }

            return res;
        }


        public void SetReferenceSpectrumForExtinctionCorrection(Spectrum aRefSpec)
        {
            if (!ReferenceEquals(referenceSpectrum, aRefSpec))
            {
                referenceSpectrum = aRefSpec;
                RVschlegelMapFactorCache = null;
            }
        }

        public bool GetSchlegelMapFactor(double rVParam, out double factor)
        {

            if (!ReferenceEquals(RVschlegelMapFactorCache, null))
            {

                if (RVschlegelMapFactorCache.ContainsKey(rVParam))
                {
                    factor = RVschlegelMapFactorCache[rVParam];
                    return true;
                }
                else
                {
                    if (ComputeSchlegelMapFactor(rVParam, out factor))
                    {
                        RVschlegelMapFactorCache[rVParam] = factor;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            else
            {
                if (ComputeSchlegelMapFactor(rVParam, out factor))
                {
                    RVschlegelMapFactorCache = new Dictionary<double, double>(1);
                    RVschlegelMapFactorCache[rVParam] = factor;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool ComputeSchlegelMapFactor(double rVParam, out double factor)
        {
            if (!ReferenceEquals(referenceSpectrum, null) && !ReferenceEquals(binCenters, null) && !ReferenceEquals(responses, null))
            {
                bool error = false;
                //Computes Eq. B2 in Schlegel et al. (1998) 
                //A(V) is assumed to be 1, \Delta m_V is assumed to be small and then rescaled
                double integral;
                double scaling = 1.0;


                if (useFitzPatrickExtinction)
                {

                    double extinctionNormAt1Um;
                    TestMySpline.CubicSpline splineInterp = ComputeFitzpatrickSpline(rVParam, out extinctionNormAt1Um);

                    double mAt1UmODonnell = 1.32; //ComputeALambdaODonnell(10000.0, 3.1, 3.1);

                    integral = Jhu.SpecSvc.Util.Integral.IntegrateFilterSpectrumFunction(referenceSpectrum.GetBinCenters(),
                                                                                                referenceSpectrum.GetFluxes(),
                                                                                                binCenters,
                                                                                                responses,
                                                                                                (x) => ComputeWavelengthFactorFitzpatrick(x, rVParam, mAt1UmODonnell, splineInterp, extinctionNormAt1Um),
                                                                                                out error);
                }
                else
                {
                    //The integral calculates it for A(V)=1, but we need to normalize for E(B-V)=1
                    //R_V=A(V)/E(B-V)
                    scaling = rVParam / smallMValueToExtrapolate;

                    integral = Jhu.SpecSvc.Util.Integral.IntegrateFilterSpectrumFunction(referenceSpectrum.GetBinCenters(),
                                                                                                referenceSpectrum.GetFluxes(),
                                                                                                binCenters,
                                                                                                responses,
                                                                                                (x) => ComputeWavelengthFactorODonnell(x, rVParam, smallMValueToExtrapolate, 1.0),
                                                                                                out error);
                }


                if (error)
                {
                    factor = Constants.missingDouble;
                    return false;
                }
                else
                {
                    factor = -2.5 * Math.Log10(integral) * scaling;
                    return true;
                }
            }
            else
            {
                factor = Constants.missingDouble;
                return false;
            }
        }

        //Computes 10^(- A_\lambda * \delta m_v / 2.5) in Schlegel et al. (1998) Eq. B2  
        //Using O'Donnell (1994) and Cardelli (1989) extionction law
        private double ComputeWavelengthFactorODonnell(double aWavelength, double rVParam, double deltaMVParam, double AVParam)
        {
            double a_lambda = ComputeALambdaODonnell(aWavelength, rVParam, AVParam);

            return Math.Pow(10.0, -a_lambda * deltaMVParam / 2.5);
        }

        //Computes A_\lambda in Schlegel et al. (1998) using O'Donnell (1994) and Cardelli (1989) extionction law
        private double ComputeALambdaODonnell(double aWavelength, double rVParam, double AVParam)
        {
            //Wavelength is assumed to be in angstrom
            double waveNumberInMicroMeter = 1.0 / (aWavelength * 1e-4);

            double a_x = Constants.missingDouble;
            double b_x = Constants.missingDouble;

            if (waveNumberInMicroMeter <= 1.1)
            {
                a_x = 0.574 * Math.Pow(waveNumberInMicroMeter, 1.61);
                b_x = -0.527 * Math.Pow(waveNumberInMicroMeter, 1.61);
            }
            else if (waveNumberInMicroMeter > 1.1 && waveNumberInMicroMeter <= 3.3)
            {
                a_x = Jhu.SpecSvc.Util.Functions.Polynomial(waveNumberInMicroMeter - 1.82, new double[] { 1.0, 0.104, -0.609, 0.701, 1.137, -1.718, -0.827, 1.647, -0.505 });
                b_x = Jhu.SpecSvc.Util.Functions.Polynomial(waveNumberInMicroMeter - 1.82, new double[] { 0.0, 1.952, 2.908, -3.989, -7.985, 11.102, 5.491, -10.805, 3.347 });
            }
            else if (waveNumberInMicroMeter > 3.3 && waveNumberInMicroMeter <= 8.0)
            {
                double F_a_x;
                double F_b_x;

                if (waveNumberInMicroMeter > 5.9)
                {
                    double xSqr = (waveNumberInMicroMeter - 5.9) * (waveNumberInMicroMeter - 5.9);
                    double xCube = xSqr * (waveNumberInMicroMeter - 5.9);
                    F_a_x = -0.04473 * xSqr - 0.009779 * xCube;
                    F_b_x = 0.2130 * xSqr + 0.1207 * xCube;
                }
                else
                {
                    F_a_x = 0.0;
                    F_b_x = 0.0;
                }

                a_x = 1.752 - 0.316 * waveNumberInMicroMeter - 0.104 / ((waveNumberInMicroMeter - 4.67) * (waveNumberInMicroMeter - 4.67) + 0.341) + F_a_x;
                b_x = -3.090 + 1.825 * waveNumberInMicroMeter + 1.206 / ((waveNumberInMicroMeter - 4.62) * (waveNumberInMicroMeter - 4.62) + 0.263) + F_b_x;
            }
            else if (waveNumberInMicroMeter > 8.0)
            {
                a_x = Jhu.SpecSvc.Util.Functions.Polynomial(waveNumberInMicroMeter - 8.0, new double[] { -1.073, -0.628, 0.137, -0.070 });
                b_x = Jhu.SpecSvc.Util.Functions.Polynomial(waveNumberInMicroMeter - 8.0, new double[] { 13.670, 4.257, -0.420, 0.374 });
            }
            //One branch must have been entered and a_x, b_x calculated

            return (a_x + b_x / rVParam) * AVParam;
        }


        //Computes 10^(- A_\lambda * N * \delta m_1um / 2.5) in Schlafly & Finkbeiner (2011) Eq. A1  
        //Using Fitzpatrick (1999) extinction law
        private double ComputeWavelengthFactorFitzpatrick(double aWavelength, double rVParam, double deltaM1umParam, TestMySpline.CubicSpline interpolator, double extinctionNormAt1Um)
        {
            //Extinction law normalized at 1 um
            double a_lambda = ComputeALambdaFitzpatrick(aWavelength, rVParam, interpolator) / extinctionNormAt1Um;

            //0.78 - normalization factor of Schlafly et al. 2010
            return Math.Pow(10.0, -a_lambda * 0.78 * deltaM1umParam / 2.5);
        }


        //Computes A_\lambda using Fitzpatrick (1999) extinction law
        private double ComputeALambdaFitzpatrick(double aWavelength, double rVParam, TestMySpline.CubicSpline interpolator)
        {
            //Wavelength is assumed to be in angstrom
            double waveNumberInMicroMeter = 1.0 / (aWavelength * 1e-4);

            double a_lambda = Constants.missingDouble;

            if (waveNumberInMicroMeter > 3.7)
            {
                //Use functional form for UV region from Fitzpatrick & Massa (1990), parameters from Fitzpatrick (1999)

                double c2 = -0.824 + 4.717 / rVParam;
                double c1 = 2.030 - 3.007 * c2;


                double x0 = 4.596;
                double gamma = 0.99;
                double c3 = 3.23;
                double c4 = 0.41;

                double sqrWaveNumber = waveNumberInMicroMeter * waveNumberInMicroMeter;

                double Dx = sqrWaveNumber / ((sqrWaveNumber - x0 * x0) * (sqrWaveNumber - x0 * x0) + sqrWaveNumber * gamma * gamma);

                double Fx = 0.0;

                if (waveNumberInMicroMeter > 5.9)
                {
                    double xMinus = waveNumberInMicroMeter - 5.9;

                    Fx = 0.5392 * xMinus * xMinus + 0.05644 * xMinus * xMinus * xMinus;
                }


                a_lambda = c1 + c2 * waveNumberInMicroMeter + c3 * Dx + c4 * Fx + rVParam;

            }
            else
            {
                double[] xValue = new double[1];
                xValue[0] = waveNumberInMicroMeter;

                double[] result = interpolator.Eval(xValue);

                a_lambda = result[0];
            }

            return a_lambda;
        }

        private TestMySpline.CubicSpline ComputeFitzpatrickSpline(double rVParam, out double normAt1Um)
        {
            TestMySpline.CubicSpline spline = new TestMySpline.CubicSpline();
            //Spline code created by Ryan Seghers, downloaded from http://www.codeproject.com/Articles/560163/Csharp-Cubic-Spline-Interpolation
            //See copyright in TestMySpline/CubicSpline.cs

            double[] xValues = new double[9] {0.000, 0.377, 0.820, 1.667, 1.828, 2.141, 2.433, 3.704, 3.846};

            double[] yValues = new double[9] {0.000, 0.265, 0.829, 2.688, 3.055, 3.806, 4.315, 6.265, 6.591};

            if (rVParam!=3.1)
            {
                yValues[1]*=rVParam/3.1;
                yValues[2]*=rVParam/3.1;

                yValues[3]=-0.426 + 1.0044*rVParam;
                yValues[4]=-0.050 + 1.0016*rVParam;
                yValues[5]=0.701 + 1.0016*rVParam;
                yValues[6]=1.208 + 1.0032*rVParam - 0.00033*rVParam*rVParam;
            }

            spline.Fit(xValues, yValues);

            double[] x = new double[1] { 1.000 };
            double[] y = spline.Eval(x);

            normAt1Um = y[0];

            return spline;
        }

    }
}
