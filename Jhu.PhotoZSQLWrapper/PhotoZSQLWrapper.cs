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
            filterList = null;
            priorInformation = null;
            templateLibrary = null;
        }

        private List<Filter> filterList;
        private Prior priorInformation;
        private Template templateLibrary;


        public void RemoveInitialization()
        {
            filterList = null;
            priorInformation = null;
            templateLibrary = null;
        }


        public void InitializeForSDSSFromDB(double aRedshiftFrom, 
                                            double aRedshiftTo, 
                                            double aRedshiftStep, 
                                            string aConnectionStringTemplates, 
                                            string aConnectionStringFilters)
        {
            HSCTemplateExtractorDB templateExtractor = new HSCTemplateExtractorDB(aConnectionStringTemplates);

            List<Spectrum> templates = new List<Spectrum>(641);
            for (int i = 0; i < 641; ++i)
            {
                bool error;
                Spectrum template = templateExtractor.ExtractTemplateFromDB(i, 2, out error);
                if (!error)
                {
                    templates.Add(template);
                }
            }

            templateLibrary = new TemplateSimpleLibrary(templates, new TemplateParameterAdditive(aRedshiftFrom, aRedshiftTo, aRedshiftStep) { Name = "Redshift", Value = 0.0 })
            {
                IgnoreLuminosity = true,
                IterationStepsWhenIgnoringLuminosity = 21
            };

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


        public Spectrum CalculatePhotoZ(List<Magnitude> aMagnitudeList, 
                                        List<int> aFilterIDList,
                                        double aExtinctionMapValue,
                                        out List<double> redshifts, 
                                        out List<double> redshiftProbabilities,
                                        out bool fitError)
        {

            if (!ReferenceEquals(aMagnitudeList, null) &&
                !ReferenceEquals(aFilterIDList, null) && 
                !ReferenceEquals(filterList, null) &&
                aFilterIDList.Max()<filterList.Count &&
                aMagnitudeList.Count==aFilterIDList.Count &&
                !ReferenceEquals(priorInformation, null) &&
                !ReferenceEquals(templateLibrary, null))
            {


                List<Filter> filterLocalCopy = new List<Filter>(aFilterIDList.Count);
                foreach (int filterID in aFilterIDList)
                {
                    filterLocalCopy.Add(filterList[filterID]);
                }

                if (aExtinctionMapValue != 0)
                {
                    for (int i = 0; i < aMagnitudeList.Count; ++i)
                    {
                        aMagnitudeList[i].ApplySchlegelExtinctionCorrection(filterLocalCopy[i], aExtinctionMapValue, 3.1);
                    }
                }


                Template templateLocalCopy = templateLibrary.CloneLightWeight();
                Prior priorLocalCopy = priorInformation.CloneLightWeight();


                Spectrum result = TemplateFit.GetBayesianBestTemplateFit(   templateLocalCopy,
                                                                            priorLocalCopy,
                                                                            filterLocalCopy,
                                                                            aMagnitudeList,
                                                                            false,
                                                                            0.0,
                                                                            false,
                                                                            out redshifts,
                                                                            out redshiftProbabilities,
                                                                            out fitError);


                return result;
            }
            else
            {
                redshifts = null;
                redshiftProbabilities = null;
                fitError = true;
                return null;
            }
        }




    }
}
