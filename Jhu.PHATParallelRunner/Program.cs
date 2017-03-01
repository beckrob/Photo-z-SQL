using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jhu.PhotoZ;
using Jhu.HSCPhotoZ;

namespace Jhu.PHATParallelRunner
{
    class Program
    {

        private const string aConnectionStringTemplates = @"data source=future1;initial catalog=beckrob23;multipleactiveresultsets=true;Integrated Security=true";


        private static readonly string[] aFilterPaths = new string[14] 
        {
            "filters\\U_kpno.res",
            "filters\\B_subaru.res",
            "filters\\V_subaru.res",
            "filters\\R_subaru.res",
            "filters\\I_subaru.res",
            "filters\\z_subaru.res",
            "filters\\HST_ACS_WFC_F435W.res",
            "filters\\HST_ACS_WFC_F606W.res",
            "filters\\HST_ACS_WFC_F775W.res",
            "filters\\HST_ACS_WFC_F850LP.res",
            "filters\\WFCAM_J.res",
            "filters\\WFCAM_H.res",
            "filters\\HK_88in.res",
            "filters\\K_flaming_resam.res"
        };

        private static readonly string[] aFilterTags = new string[14] 
        {
            "U", "B", "V", "R", "I", "Z", "F435W", "F606W", "F775W", "F850LP", "J", "H", "HK", "K"
        };

        private static readonly double[] aFilterEffWavelengths = new double[14] 
        {
            3552, 4478, 5493, 6550, 7996, 9054, 4327.3, 5921.8, 7693.7, 9033.6, 12483, 16313, 18947.38, 21900
        };


        private static readonly Object consoleLock = new Object();
        private static readonly Object taskNumberLock = new Object();

        private const int maxNumberOfExistingTasks = 100;
        private const int waitTimeBetweenTaskNumberCheckInms = 50;

        static void Main(string[] args)
        {
            //First argument: path to file containing the query objects, in CSV format
            //Second argument: tag of spectrum and probability distribution files
            //Third argument: integer code of photometry setup to use
            //  0 - BPZ templates with flat prior
            //  1 - BPZ templates with BPZ prior
            //  2 - LePhare templates with flat prior
            //  3 - LePhare templates with absolute magnitude prior
            //  4 - LePhare templates with adapted BPZ prior
            //Fourth argument: integer code of whether to do zero-point calibration (0 no, 1 yes, 2 yes, and also apply filter error calibration)
            //Fifth argument: number of galaxies to skip/use for calibration
            //Sixth argument: amount of extra magnitude error to add

            //The output will be on the standard output

            int photoSetup;
            int doCalibration;
            int skippedGalaxyNum;
            double errorSoftening;
            if (args.Length == 6 && int.TryParse(args[2], out photoSetup) && int.TryParse(args[3], out doCalibration) && int.TryParse(args[4], out skippedGalaxyNum) && double.TryParse(args[5], out errorSoftening))
            {

                HSCTemplateExtractorDB templateExtractor = new HSCTemplateExtractorDB(aConnectionStringTemplates);

                List<Spectrum> templates = null;

                if (photoSetup == 0 || photoSetup == 1)
                {
                    templates = new List<Spectrum>(71);
                    for (int i = 0; i < 71; ++i)
                    {
                        bool error;
                        Spectrum template = templateExtractor.ExtractTemplateFromDB(i, 0, out error);
                        if (!error)
                        {
                            templates.Add(template);
                        }
                    }
                }

                if (photoSetup == 2 || photoSetup == 3 || photoSetup == 4)
                {
                    templates = new List<Spectrum>(641);
                    for (int i = 0; i < 641; ++i)
                    {
                        bool error;
                        Spectrum template = templateExtractor.ExtractTemplateFromDB(i, 2, out error);
                        if (!error)
                        {
                            templates.Add(template);
                        }
                    }
                }

                bool applyFilterErrorCorrection = (doCalibration == 2);

                TemplateSimpleLibrary templateLib = new TemplateSimpleLibrary(templates, new TemplateParameterAdditive(1e-3, 6.001, 0.01))
                {
                    IgnoreLuminosity = true,
                    IterationStepsWhenIgnoringLuminosity = 11
                };


                string globalFilePath = (args[0].LastIndexOf('/') < args[0].LastIndexOf('\\'))
                                            ? args[0].Remove(args[0].LastIndexOf('\\')) + "\\"
                                            : args[0].Remove(args[0].LastIndexOf('/')) + "/";

                List<Filter> filterList = new List<Filter>(14);
                for (int i = 0; i < 14; ++i)
                {

                    Filter tmpFilter = new Filter(globalFilePath+aFilterPaths[i]);

                    tmpFilter.EffectiveWavelength = aFilterEffWavelengths[i];
                    tmpFilter.SetReferenceSpectrumForExtinctionCorrection(templates[0]);
                    double dummy;
                    tmpFilter.GetSchlegelMapFactor(3.1, out dummy);
                    filterList.Add(tmpFilter);

                }


                List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeList = new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(14);
                for (int i = 0; i < 14; ++i)
                {
                    magnitudeList.Add(new Magnitude() { Value = 21.0, Error = 0.2, MagSystem = MagnitudeSystem.Type.AB });
                }

                Console.Error.WriteLine("Starting at: " + DateTime.Now.ToString("HH:mm:ss.fff"));


                //Populating syntheticFluxCache with cached synthetic magnitude results
                //Calls from the same filter set will not populate
                /*int dummyFitError;
                TemplateFit.GetBestChiSquareFit(templateLib,
                                                filterList,
                                                magnitudeList,
                                                false,
                                                0.0,
                                                false,
                                                out dummyFitError);*/


                Prior priorInfo = null;
                /*double[] typeProbabilites = {   10431.0,61237.0,481.0,726.0,901.0,
                                                577.0,86690.0,2095.0,19246.0,1533.0,
                                                1067.0,327.0,942.0,26613.0,312.0,26550.0,
                                                14304.0,2725.0,215.0,1157.0,40504.0,12838.0,
                                                107030.0,16318.0,4518.0,1443.0,2676.0,153810.0,
                                                17913.0,22934.0,46470.0,17546.0,2469.0,301.0,
                                                2862.0,7073.0,4436.0,1948.0};
                Prior priorInfo = new PriorOnTemplateType(Enumerable.Range(0, 38).ToArray(), typeProbabilites);*/
                if (photoSetup == 0 || photoSetup == 2)
                {
                    priorInfo = new PriorFlat();
                }
                if (photoSetup == 1)
                {
                    bool error;
                    HSCFilterExtractorDB priorFilterExtractor = new HSCFilterExtractorDB(aConnectionStringTemplates);
                    Filter filtI814 = priorFilterExtractor.ExtractFilterFromDB(265, 53000, out error);

                    if (!error)
                    {
                        priorInfo = new PriorBenitezHubble(filtI814, templateLib);
                    }
                }
                if (photoSetup == 3)
                {
                    bool error;
                    HSCFilterExtractorDB priorFilterExtractor = new HSCFilterExtractorDB(aConnectionStringTemplates);
                    Filter filtB = priorFilterExtractor.ExtractFilterFromDB(92, 0, out error);

                    if (!error)
                    {
                        priorInfo = new PriorAbsMagLimitInFilter(-24.0, filtB, templateLib);
                    }
                }
                if (photoSetup == 4)
                {
                    bool error;
                    HSCFilterExtractorDB priorFilterExtractor = new HSCFilterExtractorDB(aConnectionStringTemplates);
                    Filter filtI814 = priorFilterExtractor.ExtractFilterFromDB(265, 53000, out error);

                    if (!error)
                    {
                        priorInfo = new PriorBenitezHubbleOnLephare(filtI814, templateLib);
                    }
                }



                int galaxyCount = 0;
                int taskCount = 0;

                List<List<ValueWithErrorConvolveableFromFilterAndSpectrum>> calibrationMagnitudeLists = new List<List<ValueWithErrorConvolveableFromFilterAndSpectrum>>();
                List<List<Filter>> calibrationFilterLists = new List<List<Filter>>();
                List<double> calibrationRedshiftList = new List<double>();

                using (StreamReader reader = new StreamReader(args[0]))
                {

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] fields = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        //Ignore empty, short or comment lines
                        if (fields.Length > 30 && fields[0].Length > 0 && fields[0][0] != '#')
                        {
                            long matchID;

                            if (long.TryParse(fields[0], out matchID))
                            {

                                double redshift;
                                if (!double.TryParse(fields[1], out redshift))
                                {
                                    redshift = Constants.missingDouble;
                                }

                                bool readError = false;

                                for (int i = 0; i < 14; ++i)
                                {
                                    double tmp;
                                    if (!double.TryParse(fields[i + 2], out tmp))
                                    {
                                        readError = true;
                                    }
                                    magnitudeList[i].Value = tmp;

                                    if (!double.TryParse(fields[i + 20], out tmp))
                                    {
                                        readError = true;
                                    }
                                    magnitudeList[i].Error = tmp;
                                }

                                if (!readError)
                                {

                                    List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeLocalCopy = new List<ValueWithErrorConvolveableFromFilterAndSpectrum>();
                                    List<Filter> filterLocalCopy = new List<Filter>();

                                    for (int i = 0; i < 14; ++i)
                                    {
                                        if (magnitudeList[i].Value != 99 && magnitudeList[i].Value != -99
                                            && magnitudeList[i].Error != 99 && magnitudeList[i].Error > 0.0)
                                        {
                                            magnitudeLocalCopy.Add((ValueWithErrorConvolveableFromFilterAndSpectrum)magnitudeList[i].Clone());
                                            filterLocalCopy.Add(filterList[i]);
                                        }
                                    }

                                    double closestRedshift = templateLib.GetClosestParameterValueInCoverage("Redshift", redshift);

                                    ++galaxyCount;


                                    if (galaxyCount <= skippedGalaxyNum)
                                    {
                                        if (doCalibration == 1 || doCalibration == 2)
                                        {

                                            calibrationMagnitudeLists.Add(new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(magnitudeLocalCopy));
                                            calibrationFilterLists.Add(new List<Filter>(filterLocalCopy));
                                            calibrationRedshiftList.Add(closestRedshift);


                                            if (galaxyCount == skippedGalaxyNum)
                                            {

                                                TemplateFit.CalibrateFilterZeroPoints(0.001,
                                                                                        100,
                                                                                        templateLib,
                                                                                        calibrationFilterLists,
                                                                                        calibrationMagnitudeLists,
                                                                                        calibrationRedshiftList,
                                                                                        false,
                                                                                        errorSoftening);
                                            }

                                        }
                                    }

                                }



                            }
                        }
                    }
                }

                galaxyCount = 0;

                using (StreamReader reader = new StreamReader(args[0]))
                {

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] fields = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        //Ignore empty, short or comment lines
                        if (fields.Length > 30 && fields[0].Length > 0 && fields[0][0] != '#')
                        {
                            long matchID;

                            if (long.TryParse(fields[0], out matchID))
                            {

                                double redshift;
                                if (!double.TryParse(fields[1], out redshift))
                                {
                                    redshift = Constants.missingDouble;
                                }

                                bool readError = false;

                                for (int i = 0; i < 14; ++i)
                                {
                                    double tmp;
                                    if (!double.TryParse(fields[i + 2], out tmp))
                                    {
                                        readError = true;
                                    }
                                    magnitudeList[i].Value = tmp;

                                    if (!double.TryParse(fields[i + 20], out tmp))
                                    {
                                        readError = true;
                                    }
                                    magnitudeList[i].Error = tmp;
                                }

                                if (!readError)
                                {

                                    List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeLocalCopy = new List<ValueWithErrorConvolveableFromFilterAndSpectrum>();
                                    List<Filter> filterLocalCopy = new List<Filter>();
                                    List<string> filterStringLocalCopy = new List<string>();

                                    for (int i = 0; i < 14; ++i)
                                    {
                                        if (magnitudeList[i].Value != 99 && magnitudeList[i].Value != -99
                                            && magnitudeList[i].Error != 99 && magnitudeList[i].Error != -99)
                                        {
                                            magnitudeLocalCopy.Add((ValueWithErrorConvolveableFromFilterAndSpectrum)magnitudeList[i].Clone());
                                            filterLocalCopy.Add(filterList[i]);
                                            filterStringLocalCopy.Add(aFilterTags[i]);
                                        }
                                    }

                                    //double closestRedshift = templateLib.GetClosestParameterValueInCoverage("Redshift", redshift);

                                    int localGalaxyCount = ++galaxyCount;

                                    if (galaxyCount > skippedGalaxyNum && galaxyCount <= 515)
                                    {

                                        int localTaskCount;
                                        do
                                        {

                                            lock (taskNumberLock)
                                            {
                                                localTaskCount = taskCount;
                                            }

                                            if (localTaskCount >= maxNumberOfExistingTasks)
                                            {
                                                Thread.Sleep(waitTimeBetweenTaskNumberCheckInms);
                                            }

                                        } while (localTaskCount >= maxNumberOfExistingTasks);

                                        Prior priorLocalCopy = priorInfo.CloneLightWeight();
                                        Template templateLocalCopy = templateLib.CloneLightWeight();
                                        //templateLocalCopy.GetParameterList()[1] = new TemplateParameterAdditive(closestRedshift, closestRedshift, 1.0) { Name = "Redshift", Value = closestRedshift };

                                        lock (taskNumberLock)
                                        {
                                            ++taskCount;
                                        }
                                        Task.Factory.StartNew(() => DoBayesianAnalysisWithOutput(magnitudeLocalCopy,
                                                                                                    templateLocalCopy,
                                                                                                    priorLocalCopy,
                                                                                                    filterLocalCopy,
                                                                                                    localGalaxyCount,
                                                                                                    ref taskCount,
                                                                                                    args[0],
                                                                                                    args[1],
                                                                                                    redshift,
                                                                                                    matchID,
                                                                                                    false,
                                                                                                    errorSoftening,
                                                                                                    applyFilterErrorCorrection,
                                                                                                    filterStringLocalCopy),
                                                                TaskCreationOptions.LongRunning);

                                    }
       
                                }


                            }
                        }
                    }
                }


                int localTaskCount2;
                do
                {
                    lock (taskNumberLock)
                    {
                        localTaskCount2 = taskCount;
                    }

                    if (localTaskCount2 > 0)
                    {
                        Thread.Sleep(500);
                    }

                } while (localTaskCount2 > 0);

                Console.Error.WriteLine("Finished at: " + DateTime.Now.ToString("HH:mm:ss.fff"));
            }
        }


        private static void DoBayesianAnalysisWithOutput(List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeList,
                                                             Template templateLib,
                                                             Prior priorInfo,
                                                             List<Filter> filterList,
                                                             int galaxyCount,
                                                             ref int taskCount,
                                                             string sampleString,
                                                             string tag,
                                                             double redshift,
                                                             long ID,
                                                             bool fitInFluxSpace,
                                                             double errorSoftening,
                                                             bool applyFilterErrorCalibration,
                                                             List<string> filterStringList)
        {
            List<double> redshifts, redshiftProbabilities;
            int fitError;

            Spectrum result = TemplateFit.GetBayesianBestTemplateFit(templateLib, priorInfo, filterList, magnitudeList, fitInFluxSpace, errorSoftening, applyFilterErrorCalibration, out redshifts, out redshiftProbabilities, out fitError);

            double maximumProb = 0.0;
            double maximumProbZ = -9999;
            List<Magnitude> synthMags = new List<Magnitude>(magnitudeList.Count);

            double redshiftResult = -9999;
            if (fitError==0)
            {
                redshiftResult = result.Redshift;

                for (int i = 0; i < magnitudeList.Count; ++i)
                {
                    synthMags.Add(new Magnitude() { MagSystem = ((Magnitude)magnitudeList[i]).MagSystem });
                    synthMags[i].ConvolveFromFilterAndSpectrum(result, filterList[i]);
                }

                for (int i = 0; i < redshifts.Count; ++i)
                {
                    if (redshiftProbabilities[i] > maximumProb)
                    {
                        maximumProb = redshiftProbabilities[i];
                        maximumProbZ = redshifts[i];
                    }
                }
            }

            lock (consoleLock)
            {
                Console.Out.Write(ID.ToString() + '\t');

                if (redshift != Constants.missingDouble)
                {
                    Console.Out.Write(redshift.ToString() + '\t');
                }
                else
                {
                    Console.Out.Write("-9999\t");
                }

                if (fitError==0)
                {

                    Console.Out.Write(redshiftResult.ToString() + '\t');

                    Console.Out.Write(maximumProbZ.ToString() + '\t');
                    Console.Out.Write(templateLib.GetParameterValue("TypeID").ToString() + '\t');

                    if ((int)(redshift * 100.0 + 0.5) < redshiftProbabilities.Count && (int)(redshiftResult * 100.0 + 0.5) < redshiftProbabilities.Count)
                    {
                        Console.Out.Write(redshiftProbabilities[(int)(redshift * 100.0 + 0.5)].ToString() + '\t');
                        Console.Out.Write(redshiftProbabilities[(int)(redshiftResult * 100.0 + 0.5)].ToString() + '\t');
                    }
                    else
                    {
                        Console.Out.Write(redshiftProbabilities[0].ToString() + '\t');
                        Console.Out.Write(redshiftProbabilities[0].ToString() + '\t');
                    }


                    Console.Out.Write(maximumProb.ToString() + '\t');
                }
                else
                {
                    Console.Out.Write("-9999\t");

                    Console.Out.Write("-9999\t");
                    Console.Out.Write("-9999\t");

                    Console.Out.Write("-9999\t");
                    Console.Out.Write("-9999\t");
                    Console.Out.Write("-9999\t");
                }

                Console.Out.Write(filterStringList.Distinct().Count().ToString() + '\t');
                Console.Out.Write(magnitudeList.Count.ToString() + '\t');

                for (int i = 0; i < magnitudeList.Count; ++i)
                {
                    Console.Out.Write(filterStringList[i] + '\t');

                    Console.Out.Write(magnitudeList[i].Value.ToString() + '\t');
                    Console.Out.Write(magnitudeList[i].Error.ToString() + '\t');

                    if (fitError==0)
                    {
                        Console.Out.Write(synthMags[i].Value.ToString() + '\t');
                    }
                    else
                    {
                        Console.Out.Write("-9999\t");
                    }
                }

                Console.Out.Write("\n");
            }

            string probDistFilePath = (sampleString.LastIndexOf('/') < sampleString.LastIndexOf('\\'))
                                        ? sampleString.Remove(sampleString.LastIndexOf('\\')) + "\\probs\\" + tag + '_' + galaxyCount.ToString("D4") + ".txt"
                                        : sampleString.Remove(sampleString.LastIndexOf('/')) + "/probs/" + tag + '_' + galaxyCount.ToString("D4") + ".txt";

            using (StreamWriter outputFile = new StreamWriter(probDistFilePath))
            {
                if (fitError==0)
                {
                    for (int i = 0; i < redshifts.Count; ++i)
                    {
                        if (i == 0)
                        {
                            outputFile.Write(redshifts[i].ToString() + '\t' + redshiftProbabilities[i].ToString()
                                                + '\t' + redshift.ToString() + "\t0.3"
                                                + '\t' + redshiftResult.ToString() + "\t0.3"
                                                + '\t' + maximumProbZ.ToString() + '\t' + maximumProb.ToString() + '\n');
                        }
                        else
                        {
                            outputFile.Write(redshifts[i].ToString() + '\t' + redshiftProbabilities[i].ToString() + '\n');
                        }

                    }
                }
                else
                {
                    outputFile.Write("0.0\t-9999\t-9999\t-9999\t-9999\t-9999\t-9999\t-9999\n\n");
                }
            }

            string specFilePath = (sampleString.LastIndexOf('/') < sampleString.LastIndexOf('\\'))
                                    ? sampleString.Remove(sampleString.LastIndexOf('\\')) + "\\spec\\" + tag + '_' + galaxyCount.ToString("D4") + ".txt"
                                    : sampleString.Remove(sampleString.LastIndexOf('/')) + "/spec/" + tag + '_' + galaxyCount.ToString("D4") + ".txt";

            using (StreamWriter outputFile = new StreamWriter(specFilePath))
            {
                if (fitError==0)
                {

                    outputFile.Write("#SpecZ=" + redshift.ToString() + " BestFitPhotoZ=" + redshiftResult.ToString() + " MaxProbPhotoZ=" + maximumProbZ.ToString() + '\n');

                    double[] binCenters = result.GetBinCenters();
                    double[] fluxes = result.GetFluxes();

                    for (int i = 0; i < binCenters.Length; ++i)
                    {
                        if (i < filterList.Count)
                        {
                            Flux synthFlux = synthMags[i].ConvertToFlux();
                            Flux origFlux = ((Magnitude)magnitudeList[i]).ConvertToFlux();

                            outputFile.Write(binCenters[i].ToString() + '\t' + fluxes[i].ToString()
                                                + '\t' + filterList[i].EffectiveWavelength.ToString()
                                                + '\t' + synthFlux.Value.ToString()
                                                + '\t' + origFlux.Value.ToString()
                                                + '\t' + origFlux.Error.ToString() + '\n');
                        }
                        else
                        {
                            outputFile.Write(binCenters[i].ToString() + '\t' + fluxes[i].ToString() + '\n');
                        }

                    }
                }
                else
                {
                    outputFile.Write("0.0\t-9999\t-9999\t-9999\t-9999\t-9999\t-9999\t-9999\n\n");
                }
            }

            lock (taskNumberLock)
            {
                --taskCount;
            }

        }

    }
}

