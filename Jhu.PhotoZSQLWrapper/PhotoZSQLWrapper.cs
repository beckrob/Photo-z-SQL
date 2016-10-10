using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

using Jhu.PhotoZ;
using Jhu.HSCPhotoZ;


namespace Jhu.PhotoZSQL
{
    public sealed class PhotoZSQLWrapper
    {
        private static readonly PhotoZSQLWrapper instance = new PhotoZSQLWrapper();

        static PhotoZSQLWrapper() {}

        public static PhotoZSQLWrapper Instance
        {
            get
            {
                return instance;
            }
        }

        private PhotoZSQLWrapper()
        {
            missingValueSpecifiers = new List<double>();

            filterList = new ConcurrentDictionary<string, Filter>();
            priorInformation = null;
            templateLibrary = null;

            RemoveExtinctionLaw();
        }

        private List<double> missingValueSpecifiers;

        private ConcurrentDictionary<string, Filter> filterList;
        private Prior priorInformation;
        private Template templateLibrary;

        //Extinction parameters
        private Spectrum extinctionReferenceSpectrum;
        private double dustParameterR_V;
        private bool useFitzpatrickExtinction;

        public void RemoveInitialization()
        {
            missingValueSpecifiers = new List<double>();

            filterList = new ConcurrentDictionary<string, Filter>();
            priorInformation = null;
            templateLibrary = null;

            RemoveExtinctionLaw(); 
        }

        public void AddMissingValueSpecifier(double aMissing)
        {
            missingValueSpecifiers.Add(aMissing);
        }

        public void ClearMissingValueSpecifiers()
        {
            missingValueSpecifiers.Clear();
        }


        public int SetupTemplateList(   List<string> aTemplateURLList,
                                        double aFluxMultiplier,
                                        double aRedshiftFrom, 
                                        double aRedshiftTo, 
                                        double aRedshiftStep,
                                        bool aLogarithmicRedshiftSteps = false,
                                        int aLuminosityStepNumber = Constants.missingInt,
                                        double aLuminosityFrom = Constants.missingDouble,
                                        double aLuminosityTo = Constants.missingDouble,
                                        double aLuminosityStep = Constants.missingDouble,
                                        bool aLogarithmicLuminositySteps = true)
        {
            List<Spectrum> templates = new List<Spectrum>(aTemplateURLList.Count);

            int successCount = 0;
            foreach (string URL in aTemplateURLList)
            {
                Spectrum template = null;
                try
                {
                    template = new Spectrum(URL, true, aFluxMultiplier);
                }
                catch
                {
                    template = null;
                }

                if (!ReferenceEquals(template, null))
                {
                    templates.Add(template);
                    ++successCount;
                }
            }

            if (successCount > 0)
            {

                TemplateParameter redshift;
                if (aLogarithmicRedshiftSteps)
                {
                    redshift = new TemplateParameterMultiplicative(aRedshiftFrom, aRedshiftTo, aRedshiftStep) { Name = "Redshift", Value = aRedshiftFrom };
                }
                else
                {
                    redshift = new TemplateParameterAdditive(aRedshiftFrom, aRedshiftTo, aRedshiftStep) { Name = "Redshift", Value = aRedshiftFrom };
                }

                TemplateParameter luminosity;
                if (aLuminosityStepNumber == Constants.missingInt)
                {
                    if (aLogarithmicLuminositySteps)
                    {
                        luminosity = new TemplateParameterMultiplicative(aLuminosityFrom, aLuminosityTo, aLuminosityStep) { Name = "Luminosity", Value = aLuminosityFrom };
                    }
                    else
                    {
                        luminosity = new TemplateParameterAdditive(aLuminosityFrom, aLuminosityTo, aLuminosityStep) { Name = "Luminosity", Value = aLuminosityFrom };
                    }
                }
                else
                {
                    luminosity = null;
                }

                templateLibrary = new TemplateSimpleLibrary(templates, redshift, luminosity)
                {
                    IgnoreLuminosity = (aLuminosityStepNumber != Constants.missingInt),
                    IterationStepsWhenIgnoringLuminosity = aLuminosityStepNumber
                };

                //The prior will depend on the template (except for the flat prior, but that is used by default) 
                //so if the template changes, the prior should be erased
                priorInformation = null;
            }

            return successCount;
        }

        public int SetupExtinctionLaw(  string aTemplateURL,
                                        double aR_V,
                                        bool aUseFitzpatrickLaw)
        {
            Spectrum template = null;
            try
            {
                template = new Spectrum(aTemplateURL, true);
            }
            catch
            {
                return -9999;
            }

            if (ReferenceEquals(template, null))
            {
                return -9999;
            }
            else
            {
                extinctionReferenceSpectrum = template;
                dustParameterR_V = aR_V;
                useFitzpatrickExtinction = aUseFitzpatrickLaw;

                if (!ReferenceEquals(filterList, null))
                {
                    foreach (Filter filt in filterList.Values)
                    {
                        filt.SetReferenceSpectrumForExtinctionCorrection(extinctionReferenceSpectrum);
                        filt.UseFitzpatrickExtinction = useFitzpatrickExtinction;
                    }
                }

                return 0;
            }
        }

        public void RemoveExtinctionLaw()
        {
            extinctionReferenceSpectrum = null;
            dustParameterR_V = Constants.missingDouble;
            useFitzpatrickExtinction = true;
            //No need to remove from every filter, since extinctionReferenceSpectrum will be tested for null and the extinction not applied if so
            //Could save some memory if removed, but RemoveInitialization can clean it up
        }

        public void SetupFlatPrior()
        {
            priorInformation = new PriorFlat();
        }

        public int SetupAbsoluteMagnitudeLimitPrior(string aFilterURL,
                                                    double aAbsMagLimit,
                                                    double aH_0,
                                                    double aOmega_m,
                                                    double aOmega_lambda)
        {
            if (ReferenceEquals(templateLibrary, null))
            {
                return -9999;
            }

            Filter filt = null;
            try
            {
                filt = new Filter(aFilterURL, true);
            }
            catch
            {
                return -9999;
            }

            if (ReferenceEquals(filt, null))
            {
                return -9999;
            }
            else
            {
                priorInformation = new PriorAbsMagLimitInFilter(aAbsMagLimit, filt, templateLibrary, aH_0, aOmega_m, aOmega_lambda);
                return 0;
            }
        }


        public int SetupBenitezHDFPrior(string aFilterURL,
                                        bool aUsingLePhareTemplates)
        {
            if (ReferenceEquals(templateLibrary, null))
            {
                return -9999;
            }

            Filter filt = null;
            try
            {
                filt = new Filter(aFilterURL, true);
            }
            catch
            {
                return -9999;
            }

            if (ReferenceEquals(filt, null))
            {
                return -9999;
            }
            else
            {
                try
                {
                    if (aUsingLePhareTemplates)
                    {
                        priorInformation = new PriorBenitezHubbleOnLephare(filt, templateLibrary);
                    }
                    else
                    {
                        priorInformation = new PriorBenitezHubble(filt, templateLibrary);
                    }
                }
                catch
                {
                    return -9999;
                }

                return 0;
            }
        }

        public int SetupTemplateTypePrior(double[] aTemplateProbabilities)
        {
            if (ReferenceEquals(templateLibrary, null))
            {
                return -9999;
            }

            try
            {
                priorInformation = new PriorOnTemplateType(aTemplateProbabilities, templateLibrary);
            }
            catch
            {
                return -9999;
            }

            return 0;
        }



        /*
        public void InitializeForSDSSFromDB(double aRedshiftFrom, 
                                            double aRedshiftTo, 
                                            double aRedshiftStep, 
                                            string aConnectionStringTemplates, 
                                            string aConnectionStringFilters)
        {

            FilterExtractorDB filterExtractor = new FilterExtractorDB(aConnectionStringFilters);

            filterList = new List<Filter>(35);
            for (int i = 0; i < 5; ++i)
            {
                bool error;
                Filter tmpFilter = filterExtractor.ExtractFilterFromDB(14 + i, out error);
                if (!error)
                {
                    tmpFilter.SetReferenceSpectrumForExtinctionCorrection(templates[0]);
                    double dummy;
                    tmpFilter.GetSchlegelMapFactor(3.1, out dummy);
                    filterList.Add(tmpFilter);
                }
            }

            for (int camCol = 0; camCol < 6; ++camCol)
            {
                for (int i = 0; i < 5; ++i)
                {

                    int batch = 0; //There is an unnamed second batch of filters in the SpectrumPortal DB, it can be reached with batch=1              

                    int filterID = 131 + batch * 30 + i * 6 + camCol;
                    
                    /*
                    if (filterID > 168)
                    {
                        ++filterID;
                    }
                    if (filterID > 171)
                    {
                        ++filterID;
                    }
                    */
        /*

                    bool error;
                    Filter tmpFilter = filterExtractor.ExtractFilterFromDB(filterID, out error);
                    if (!error)
                    {
                        tmpFilter.SetReferenceSpectrumForExtinctionCorrection(templates[0]);
                        double dummy;
                        tmpFilter.GetSchlegelMapFactor(3.1, out dummy);
                        filterList.Add(tmpFilter);
                    }

                }
            }

            priorInformation = new PriorFlat();


            List<Magnitude> dummyMagnitudes = new List<Magnitude>(35);
            for (int i = 0; i < filterList.Count; ++i)
            {
                dummyMagnitudes.Add(new Magnitude() { Value = 21.0, Error = 0.2 });
            }


            //Populating syntheticMagnitudeCache with cached synthetic magnitude results
            //Calls from the same filter set will not populate
            List<double> redshifts, redshiftProbabilities;
            bool fitError;
            TemplateFit.GetBayesianBestTemplateFit( templateLibrary,
                                                    priorInformation,
                                                    filterList,
                                                    dummyMagnitudes,
                                                    false,
                                                    0.0,
                                                    false,
                                                    out redshifts,
                                                    out redshiftProbabilities,
                                                    out fitError);
        }
        */

        private List<Filter> SetupFilterListFromURLList(List<string> aFilterURLList)
        {
            if (ReferenceEquals(filterList, null))
            {
                return null;
            }

            List<Filter> filterLocalCopy = new List<Filter>(aFilterURLList.Count);
            foreach (string filterURL in aFilterURLList)
            {

                Filter filt = null;
                if (filterList.TryGetValue(filterURL, out filt))
                {
                    filterLocalCopy.Add(filt);
                }
                else
                {
                    try
                    {
                        filt = new Filter(filterURL, true);
                    }
                    catch
                    {
                        filt = null;
                    }

                    if (ReferenceEquals(filt, null))
                    {
                        return null;
                    }
                    else
                    {
                        filt.SetReferenceSpectrumForExtinctionCorrection(extinctionReferenceSpectrum);
                        filterList.TryAdd(filterURL, filt);
                        filterLocalCopy.Add(filt);
                    }
                }

            }

            return filterLocalCopy;
        }

        private void ApplyExtinctionCorrectionToMeasurements(   List<ValueWithErrorConvolveableFromFilterAndSpectrum> aMagnitudeFluxList,
                                                                double aExtinctionMapValue,
                                                                List<Filter> aFilterLocalCopy)
        {
            if (!ReferenceEquals(extinctionReferenceSpectrum, null) && aExtinctionMapValue != 0.0 && aExtinctionMapValue != Constants.missingDouble)
            {
                for (int i = 0; i < aMagnitudeFluxList.Count; ++i)
                {
                    aMagnitudeFluxList[i].ApplySchlegelExtinctionCorrection(aFilterLocalCopy[i], aExtinctionMapValue, dustParameterR_V);
                }
            }
        }

         private void RemoveMissingValues(List<ValueWithErrorConvolveableFromFilterAndSpectrum> aMagnitudeFluxList, 
                                          List<string> aFilterURLList)
         {
             for (int i = 0; i < aMagnitudeFluxList.Count; ++i)
             {
                 if (missingValueSpecifiers.Contains(aMagnitudeFluxList[i].Value) || missingValueSpecifiers.Contains(aMagnitudeFluxList[i].Error))
                 {
                     aMagnitudeFluxList.RemoveAt(i);
                     aFilterURLList.RemoveAt(i--);
                 }
             }
         }


        public Spectrum CalculatePhotoZBayesian(List<ValueWithErrorConvolveableFromFilterAndSpectrum> aMagnitudeFluxList, 
                                                List<string> aFilterURLList,
                                                double aExtinctionMapValue,
                                                bool aFitInFluxSpace,
                                                double aErrorSmoothening,
                                                bool aFilterErrorCorrection,
                                                out List<double> redshifts, 
                                                out List<double> redshiftProbabilities,
                                                out int fitError)
        {

            if (!ReferenceEquals(aMagnitudeFluxList, null) &&
                !ReferenceEquals(aFilterURLList, null) &&
                aMagnitudeFluxList.Count == aFilterURLList.Count &&
                !ReferenceEquals(templateLibrary, null))
            {
                RemoveMissingValues(aMagnitudeFluxList, aFilterURLList);

                List<Filter> filterLocalCopy = SetupFilterListFromURLList(aFilterURLList);

                if (ReferenceEquals(filterLocalCopy, null))
                {
                    redshifts = null;
                    redshiftProbabilities = null;
                    fitError = -9998;
                    return null;
                }

                ApplyExtinctionCorrectionToMeasurements(aMagnitudeFluxList, aExtinctionMapValue, filterLocalCopy);

                Template templateLocalCopy = templateLibrary.CloneLightWeight();

                Prior priorLocalCopy;
                if (ReferenceEquals(priorInformation, null))
                {
                    priorLocalCopy = new PriorFlat();
                }
                else 
                {
                    priorLocalCopy = priorInformation.CloneLightWeight();
                }

                return TemplateFit.GetBayesianBestTemplateFit(  templateLocalCopy,
                                                                priorLocalCopy,
                                                                filterLocalCopy,
                                                                aMagnitudeFluxList,
                                                                aFitInFluxSpace,
                                                                aErrorSmoothening,
                                                                aFilterErrorCorrection,
                                                                out redshifts,
                                                                out redshiftProbabilities,
                                                                out fitError);

            }
            else
            {
                redshifts = null;
                redshiftProbabilities = null;
                fitError = -9997;
                return null;
            }
        }

        public Spectrum CalculatePhotoZBestChiSquare(   List<ValueWithErrorConvolveableFromFilterAndSpectrum> aMagnitudeFluxList,
                                                        List<string> aFilterURLList,
                                                        double aExtinctionMapValue,
                                                        bool aFitInFluxSpace,
                                                        double aErrorSmoothening,
                                                        bool aFilterErrorCorrection,
                                                        out int fitError)
        {

            if (!ReferenceEquals(aMagnitudeFluxList, null) &&
                !ReferenceEquals(aFilterURLList, null) &&
                aMagnitudeFluxList.Count == aFilterURLList.Count &&
                !ReferenceEquals(templateLibrary, null))
            {
                RemoveMissingValues(aMagnitudeFluxList, aFilterURLList);

                List<Filter> filterLocalCopy = SetupFilterListFromURLList(aFilterURLList);

                if (ReferenceEquals(filterLocalCopy, null))
                {
                    fitError = -9998;
                    return null;
                }

                ApplyExtinctionCorrectionToMeasurements(aMagnitudeFluxList, aExtinctionMapValue, filterLocalCopy);

                Template templateLocalCopy = templateLibrary.CloneLightWeight();

                return TemplateFit.GetBestChiSquareFit(templateLocalCopy,
                                                        filterLocalCopy,
                                                        aMagnitudeFluxList,
                                                        aFitInFluxSpace,
                                                        aErrorSmoothening,
                                                        aFilterErrorCorrection,
                                                        out fitError);

            }
            else
            {
                fitError = -9997;
                return null;
            }
        }

    }
}
