using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Jhu.PhotoZ;
using Jhu.PhotoZSQL;

namespace Jhu.WrapperTester
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.RemoveInitialization();

            Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupFlatPrior();


            List<int> templateIDs = new List<int>(5);
            for (int i=440; i<511; ++i)
            {
                templateIDs.Add(i);
            }
            List<string> templateURLs = new List<string>(templateIDs.Select(x => "http://voservices.net/spectrum/search_details.aspx?format=ascii&id=ivo%3a%2f%2fjhu%2ftemplates%23" + x.ToString()));

            Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupTemplateList(templateURLs, 1.0, 0.001, 6.001, 0.01, false, 11);

            List<int> filterIDs = new List<int>(5);
            for (int i=144; i<158; ++i)
            {
                filterIDs.Add(i);
            }
            List<string> filterURLList = new List<string>(filterIDs.Select(x => "http://voservices.net/filter/filterascii.aspx?FilterID=" + x.ToString()));

            List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeFluxList = new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(14);

            magnitudeFluxList.Add(new Magnitude() { Value = 25.46196, Error = 0.14894586, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 24.801726, Error = 0.097560635, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 24.958946, Error = 0.094177275, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 24.724696, Error = 0.062858016, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 24.640671, Error = 0.13192112, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 24.518728, Error = 0.15037895, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 25.241, Error = 0.0318, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 24.8141, Error = 0.0191, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 24.8067, Error = 0.0354, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 24.7622, Error = 0.0403, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 24.240552, Error = 0.441836, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 23.361276, Error = 0.345958, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 22.654614, Error = 0.49426395, MagSystem = MagnitudeSystem.Type.AB });
            magnitudeFluxList.Add(new Magnitude() { Value = 24.08153, Error = 1.018636, MagSystem = MagnitudeSystem.Type.AB });


            int fitError;
            List<double> redshifts, redshiftProbabilities;

            Spectrum result = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.CalculatePhotoZBayesian(  magnitudeFluxList,
                                                                                                filterURLList,
                                                                                                0.0,
                                                                                                false,
                                                                                                0.02,
                                                                                                false,
                                                                                                out redshifts,
                                                                                                out redshiftProbabilities,
                                                                                                out fitError);

            Console.Out.WriteLine(fitError);
            

        }
    }
}
