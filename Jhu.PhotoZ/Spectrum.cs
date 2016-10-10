using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Globalization;

namespace Jhu.PhotoZ
{
    public class Spectrum
    {
        private double[] binCenters;
        private double[] fluxes;
        //The fluxes are assumed to be in cgs units when loaded into this object

        public double[] GetBinCenters()
        {
            return binCenters;
        }

        public double[] GetFluxes()
        {
            return fluxes;
        }

        //The spectrum knows its luminosity and redshift, and can rescale itself given a new value
        private double luminosity;
        public double Luminosity 
        {
            get
            {
                return luminosity;
            }

            set
            {
                if (luminosity != value)
                {

                    if (!ReferenceEquals(fluxes, null))
                    {
                        double factor = value / luminosity;

                        for (int i = 0; i < fluxes.Length; ++i)
                        {
                            fluxes[i] *= factor;
                        }
                    }

                    luminosity = value;

                }

            }
        }

        private double redshift;
        public double Redshift
        {
            get
            {
                return redshift;
            }

            set
            {
                if (redshift != value)
                {

                    if (!ReferenceEquals(binCenters, null))
                    {
                        for (int i = 0; i < binCenters.Length; ++i)
                        {
                            binCenters[i] *= (1 + value) / (1 + redshift);
                        }
                    }

                    redshift = value;
                }

            }

        }

        public Spectrum(Spectrum otherSpec)
        {
            luminosity = otherSpec.Luminosity;
            redshift = otherSpec.Redshift;

            binCenters = new double[otherSpec.GetBinCenters().Length];
            fluxes = new double[otherSpec.GetFluxes().Length];
            Array.Copy(otherSpec.GetBinCenters(), binCenters, otherSpec.GetBinCenters().Length);
            Array.Copy(otherSpec.GetFluxes(), fluxes, otherSpec.GetFluxes().Length);
        }

        public Spectrum()
        {
            binCenters = null;
            fluxes = null;
            luminosity = 1.0;
            redshift = 0.0;       
        }

        public Spectrum(double[] aBinCenter, double[] aFluxes, double aRedshift = 0.0, double aLuminosity = 1.0, bool aPadInIR = false)
        {
            //The fluxes are assumed to be normalized to the given luminosity
            luminosity = aLuminosity;
            redshift = aRedshift;

            binCenters = aBinCenter;
            fluxes = aFluxes;

            if (aPadInIR)
            {
                AddIRPadding();
            }
           
        }

        public override string ToString()
        {
            string res = "";

            for (int i = 0; i < binCenters.Length; ++i)
            {
                res += binCenters[i].ToString() + '\t' + fluxes[i].ToString() + Environment.NewLine;
            }

            return res;
        }


        //Spectrum read in from file, expected to be in angstroms
        //aCGSmultiplier scales the vertical axis to get to CGS units
        public Spectrum(string aFilePath, bool aIsURL = false, double aCGSmultiplier = 1.0, double aRedshift = 0.0, double aLuminosity = 1.0, bool aPadInIR = false)
        {
            //The fluxes are assumed to be normalized to the given luminosity
            binCenters = null;
            fluxes = null;
            luminosity = aLuminosity;
            redshift = aRedshift;

            using (WebClient client = new WebClient())
            using (Stream dataStream = aIsURL ? client.OpenRead(aFilePath) : new FileStream(aFilePath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(dataStream))
            {

                List<double> lambdaList = new List<double>();
                List<double> fluxList = new List<double>();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                    //Ignore empty, short or comment lines
                    if (fields.Length > 1 && fields[0].Length > 0 && fields[0][0] != '#')
                    {

                        //First two coloumns will be parsed into doubles
                        double value1, value2;
                        if (double.TryParse(fields[0], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value1) &&
                            double.TryParse(fields[1], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value2))
                        {
                            lambdaList.Add(value1);
                            fluxList.Add(value2 * aCGSmultiplier);
                        }

                    }

                }

                if (lambdaList.Count > 0)
                {
                    binCenters = lambdaList.ToArray();
                    fluxes = fluxList.ToArray();

                    //Make sure that the ordering of the spectrum is correct
                    Array.Sort(binCenters, fluxes);
                    if (aPadInIR)
                    {
                        AddIRPadding();
                    }
                }
                
            }

            if (ReferenceEquals(binCenters, null) || ReferenceEquals(fluxes, null))
            {
                throw new ArgumentException("Could not read spectrum properly at the specified path", "aFilePath");
            }

        }

        public Spectrum(string aSpectrumServiceURL, double aCGSmultiplier = 1.0)
        {

        }


        private void AddIRPadding()
        {

            if (!ReferenceEquals(binCenters, null) && binCenters[binCenters.Length-1]<150000.0)
            {
                int currentSize = binCenters.Length;
                int paddedSize = currentSize + (int)((150000.0 - binCenters[currentSize - 1]) / 2000.0) + 1;

                double verticalStep = fluxes[currentSize - 1]/(paddedSize-currentSize);

                Array.Resize(ref binCenters, paddedSize);
                Array.Resize(ref fluxes, paddedSize);

                for (int i = currentSize; i < paddedSize; ++i)
                {
                    binCenters[i] = binCenters[i - 1] + 2000.0;
                    fluxes[i] = fluxes[i - 1] - verticalStep;
                }

            }

        }

    }

}
