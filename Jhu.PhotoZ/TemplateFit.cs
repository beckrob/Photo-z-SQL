using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//using System.IO;

namespace Jhu.PhotoZ
{
    public static class TemplateFit
    {

        public static Spectrum GetBestChiSquareFit( Template spectrumTemplate, 
                                                    List<Filter> filterList, 
                                                    List<Magnitude> magnitudeList, 
                                                    bool fitInFluxSpace,
                                                    double errorSofteningParameter,
                                                    bool applyFilterErrorCalibration,
                                                    out bool error)
        {

            error = false;

            Spectrum bestFitSpectrum = null;


            //We cannot fit for a single filter, the constant offset is a parameter in the chi-square fit
            if (filterList.Count > 1 && 
                filterList.Count == magnitudeList.Count)
            {
                List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeOrFluxListAB = SetupABMagOrFluxListWithErrorScaling( filterList,
                                                                                                                                    magnitudeList,
                                                                                                                                    fitInFluxSpace,
                                                                                                                                    errorSofteningParameter,
                                                                                                                                    applyFilterErrorCalibration);

                double sigmaSqrReciprocalSum;
                if (GetChiSquareConstantFactor(magnitudeOrFluxListAB, fitInFluxSpace, out sigmaSqrReciprocalSum))
                {

                    double minChiSqr = -Constants.missingDouble;
                    double minConstantOffsetOrMultiplier = Constants.missingDouble;
                    Magnitude minExampleMagnitude = null;
                    List<TemplateParameter> bestFitParameters = null;

                    //Creating container for synthetic magnitudes
                    //Here the magnitude system property is copied
                    List<ValueWithErrorConvolveableFromFilterAndSpectrum> SyntheticMagnitudeOrFluxList;
                    if (fitInFluxSpace)
                    {
                        SyntheticMagnitudeOrFluxList = new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(
                                                        magnitudeOrFluxListAB.Select(x => new Flux( (Flux)x ) ) );
                    }
                    else
                    {
                        SyntheticMagnitudeOrFluxList = new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(
                                                        magnitudeOrFluxListAB.Select(x => new Magnitude( (Magnitude)x )));
                    }

                    //The luminosity (constant magnitude offset) is fitted, not iterated over
                    spectrumTemplate.IgnoreLuminosity = true;
                    spectrumTemplate.InitializeIteration();

                    do
                    {

                        bool fitError = !GetSyntheticMagnitudesOrFluxes(SyntheticMagnitudeOrFluxList,
                                                                        filterList,
                                                                        spectrumTemplate,
                                                                        fitInFluxSpace);


                        if (!fitError)
                        {
                            double constantOffsetOrMultiplier = GetMinimumChiSquareFitParameter(magnitudeOrFluxListAB,
                                                                                                SyntheticMagnitudeOrFluxList,
                                                                                                fitInFluxSpace,
                                                                                                sigmaSqrReciprocalSum);

                            if (fitInFluxSpace)
                            {
                                foreach (ValueWithErrorConvolveableFromFilterAndSpectrum fluxOrMag in SyntheticMagnitudeOrFluxList)
                                {
                                    fluxOrMag.Value *= constantOffsetOrMultiplier;
                                }
                            }
                            else
                            {
                                foreach (ValueWithErrorConvolveableFromFilterAndSpectrum fluxOrMag in SyntheticMagnitudeOrFluxList)
                                {
                                    fluxOrMag.Value += constantOffsetOrMultiplier;
                                }
                            }



                            double chiSqr = 0.0;
                            if (!GetChiSquareValue(magnitudeOrFluxListAB, SyntheticMagnitudeOrFluxList, out chiSqr))
                            {
                                fitError = true;
                            }

                            if (!fitError)
                            {

                                if (chiSqr < minChiSqr)
                                {
                                    minChiSqr = chiSqr;
                                    minConstantOffsetOrMultiplier = constantOffsetOrMultiplier;
                                    if (!fitInFluxSpace)
                                    {
                                        minExampleMagnitude = new Magnitude((Magnitude)SyntheticMagnitudeOrFluxList[0]);
                                    }
                                    bestFitParameters = new List<TemplateParameter>(spectrumTemplate.GetParameterList().Select(x => (TemplateParameter)x.Clone()));
                                }
                            }

                        }

                    }
                    while (spectrumTemplate.IterateParameters());

                    if (!ReferenceEquals(bestFitParameters, null) && spectrumTemplate.SetParameterList(bestFitParameters))
                    {
                        double fluxOrig, fluxScaled;

                        if (fitInFluxSpace)
                        {
                            fluxOrig = 1.0;
                            fluxScaled = minConstantOffsetOrMultiplier;
                        }
                        else
                        {
                            fluxOrig = MagnitudeSystem.GetCGSFluxFromMagnitude(minExampleMagnitude.Value - minConstantOffsetOrMultiplier, minExampleMagnitude.MagSystem);
                            fluxScaled = MagnitudeSystem.GetCGSFluxFromMagnitude(minExampleMagnitude.Value, minExampleMagnitude.MagSystem);
                        }

                        //Only actually rescaling the luminosity of the best-fit spectrum

                        if (spectrumTemplate.SetParameterValue("Luminosity", spectrumTemplate.GetParameterValue("Luminosity") * fluxScaled / fluxOrig))
                        {
                            bestFitSpectrum = spectrumTemplate.GenerateSpectrum();
                        }
                        else
                        {
                            error = true;
                        }                  
                    }
                    else
                    {
                        error = true;
                    }

                } 
                else 
                {
                    error = true;
                }

            }
            else 
            {
                error = true;
            }

            return bestFitSpectrum;
        }


        private static void GetIterationStepSize(List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeOrFluxListAB, int numberOfSteps, bool fitInFluxSpace, out bool positiveFluxInRange, out double iterationStepSize)
        {
            double avgFluxOrMag, avgErr;
            if (magnitudeOrFluxListAB.Count > 0)
            {
                double valueSum = 0.0;
                double errorSum = 0.0;
                foreach (IValueWithError<double> magOrFlux in magnitudeOrFluxListAB)
                {
                    valueSum += magOrFlux.Value;
                    errorSum += Math.Abs(magOrFlux.Error);
                }

                avgFluxOrMag = valueSum / magnitudeOrFluxListAB.Count;
                avgErr = errorSum / magnitudeOrFluxListAB.Count;

            }
            else
            {
                avgFluxOrMag = Constants.missingDouble;
                avgErr = Constants.missingDouble;
            }

            positiveFluxInRange = avgFluxOrMag + 3 * avgErr > 0;

            if (numberOfSteps > 1)
            {
                if (fitInFluxSpace)
                {
                    if (avgFluxOrMag - 3 * avgErr > 0)
                    {
                        iterationStepSize = Math.Pow(1.0 - 3.0 * avgErr / avgFluxOrMag, -2.0 / (numberOfSteps - 1.0));
                    }
                    else
                    {
                        iterationStepSize = Math.Pow(1.0 + 3.0 * avgErr / avgFluxOrMag, 2.0 / (numberOfSteps - 1.0));
                    }
                }
                else
                {
                    iterationStepSize = 3 * avgErr / ((numberOfSteps - 1) / 2);
                }
            }
            else
            {
                iterationStepSize = Constants.missingDouble;
            }

        }


        //Calculate the factors for the best-fitting chi-square constant offset/multiplier
        private static bool GetChiSquareConstantFactor(List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeOrFluxList, 
                                                       bool isFittingFlux,
                                                       out double sigmaSqrReciprocalSum)
        {

            sigmaSqrReciprocalSum = 0.0;

            for (int i = 0; i < magnitudeOrFluxList.Count; ++i)
            {
                if (magnitudeOrFluxList[i].Error == 0.0)
                {
                    return false;
                }         
            }

            if (isFittingFlux)
            {
                sigmaSqrReciprocalSum = 1.0;
            } 
            else 
            {
                for (int i = 0; i < magnitudeOrFluxList.Count; ++i)
                {
                    sigmaSqrReciprocalSum += 1.0 / (magnitudeOrFluxList[i].Error * magnitudeOrFluxList[i].Error);
                }
            }


            return true;
        }

        //Calculate the chi-square value from measured and fitted magnitudes
        private static bool GetChiSquareValue(  List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeOrFluxList,
                                                List<ValueWithErrorConvolveableFromFilterAndSpectrum> synthMagnitudeOrFluxList, 
                                                out double chiSqr)
        {
            chiSqr=0.0;

            if (magnitudeOrFluxList.Count < 2)
            {
                return false;
            }

            for (int i = 0; i < magnitudeOrFluxList.Count; ++i)
            {
                if (magnitudeOrFluxList[i].Error == 0.0)
                {
                    //chiSqr diverges in this case
                    return false;
                }

                chiSqr += (synthMagnitudeOrFluxList[i].Value - magnitudeOrFluxList[i].Value) * (synthMagnitudeOrFluxList[i].Value - magnitudeOrFluxList[i].Value)
                                                / (magnitudeOrFluxList[i].Error * magnitudeOrFluxList[i].Error);

            }
            chiSqr /= (magnitudeOrFluxList.Count - 1);


            return true;
     
        }

        private static bool GetSyntheticMagnitudesOrFluxes( List<ValueWithErrorConvolveableFromFilterAndSpectrum> SyntheticMagnitudeOrFluxList,
                                                            List<Filter> filterList,
                                                            Template spectrumTemplate,
                                                            bool fitInFluxSpace)
        {
            Spectrum currentSpectrum = null;

            for (int i = 0; i < filterList.Count; ++i)
            {

                double cachedFlux;
                if (spectrumTemplate.TryGetFluxFromCache(filterList[i], out cachedFlux))
                {
                    //Assuming 0 error for synthetic magnitude. If errors are implemented, they have to be cached, too
                    SyntheticMagnitudeOrFluxList[i].SetupFromUncorrectedFlux(cachedFlux, filterList[i].ZeroPointCorrection, filterList[i].ZeroPointCorrectionInFlux);
                }
                else
                {
                    //Redshifted, normalized spectrum generation
                    if (ReferenceEquals(currentSpectrum, null))
                    {
                        currentSpectrum = spectrumTemplate.GenerateIteratedSpectrum();
                    }

                    double uncorrectedFlux;
                    if (SyntheticMagnitudeOrFluxList[i].ConvolveFromFilterAndSpectrum(currentSpectrum, filterList[i], out uncorrectedFlux))
                    {
                        spectrumTemplate.TryAddFluxToCache(filterList[i], uncorrectedFlux);
                    }
                    else
                    {
                        return false;
                    }
                }

            }

            return true;
        }

        private static double GetMinimumChiSquareFitParameter(  List<ValueWithErrorConvolveableFromFilterAndSpectrum > magnitudeOrFluxListAB,
                                                                List<ValueWithErrorConvolveableFromFilterAndSpectrum> SyntheticMagnitudeOrFluxList,
                                                                bool fitInFluxSpace,
                                                                double sigmaSqrReciprocalSum)
        {
            double syntheticComponentRHSSum = 0.0;
            double fluxLHSSum = 0.0;

            if (fitInFluxSpace)
            {
                for (int i = 0; i < magnitudeOrFluxListAB.Count; ++i)
                {
                    //MagError cannot be zero at this point (sigmaSqrReducedSum would have been bad already)

                    syntheticComponentRHSSum += (magnitudeOrFluxListAB[i].Value * SyntheticMagnitudeOrFluxList[i].Value) /
                                                (magnitudeOrFluxListAB[i].Error * magnitudeOrFluxListAB[i].Error);
                    fluxLHSSum += (SyntheticMagnitudeOrFluxList[i].Value * SyntheticMagnitudeOrFluxList[i].Value) /
                                    (magnitudeOrFluxListAB[i].Error * magnitudeOrFluxListAB[i].Error);
                }
            }
            else
            {
                for (int i = 0; i < magnitudeOrFluxListAB.Count; ++i)
                {
                    //MagError cannot be zero at this point (sigmaSqrReducedSum would have been bad already)

                    syntheticComponentRHSSum += (magnitudeOrFluxListAB[i].Value - SyntheticMagnitudeOrFluxList[i].Value) /
                                                (magnitudeOrFluxListAB[i].Error * magnitudeOrFluxListAB[i].Error);
                    

                }
                fluxLHSSum = 1.0;
            }

            return syntheticComponentRHSSum / sigmaSqrReciprocalSum / fluxLHSSum;
        }

        private static void EvaluateProbabilityAtCurrentParameters( List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeOrFluxListAB,
                                                                    List<ValueWithErrorConvolveableFromFilterAndSpectrum> SyntheticMagnitudeOrFluxList,
                                                                    double storedLuminosity,
                                                                    double fluxOrig,
                                                                    double DOF,
                                                                    bool fitInFluxSpace,
                                                                    Template spectrumTemplate,
                                                                    Prior priorInformation,
                                                                    ref double redshiftProbabilityAtIndex,
                                                                    ref double maxProbability,
                                                                    ref List<TemplateParameter> bestFitParameters)
        {
            double chiSqr = 0.0;
            if (GetChiSquareValue(magnitudeOrFluxListAB, SyntheticMagnitudeOrFluxList, out chiSqr))
            {

                double fluxScaled;
                if (fitInFluxSpace)
                {
                    fluxScaled = SyntheticMagnitudeOrFluxList[0].Value;
                }
                else
                {
                    fluxScaled = MagnitudeSystem.GetCGSFluxFromMagnitude(SyntheticMagnitudeOrFluxList[0].Value,
                                                                            ((Magnitude)SyntheticMagnitudeOrFluxList[0]).MagSystem);
                }

                //Going to index-based setting to avoid string comparison
                spectrumTemplate.SetParameterValue(0, storedLuminosity * fluxScaled / fluxOrig);

                //Using the likelihood function assuming that the different filter magnitudes are independent of each other, and errors are normally distributed
                //Ignoring pre-factors in the distribution that are not chiSqr dependent, as those are common in all templates
                double priorProb = priorInformation.Evaluate(spectrumTemplate.GetParameterList());
                double exponential = Math.Exp(-chiSqr * DOF / 2.0);

                double probability = exponential * priorProb;

                //Numerical instability sometimes yields inf instead of 0 (?)
                if (Double.IsInfinity(probability) || Double.IsNaN(probability))
                {
                    probability = 0.0;
                }

                redshiftProbabilityAtIndex += probability;

                if (probability > maxProbability)
                {
                    maxProbability = probability;
                    bestFitParameters = new List<TemplateParameter>(spectrumTemplate.GetParameterList().Select(x => (TemplateParameter)x.Clone()));
                }
            }
        }

        private static List<ValueWithErrorConvolveableFromFilterAndSpectrum> SetupABMagOrFluxListWithErrorScaling(  List<Filter> filterList,
                                                                                                                    List<Magnitude> magnitudeList,
                                                                                                                    bool fitInFluxSpace,
                                                                                                                    double errorSofteningParameter,
                                                                                                                    bool applyFilterErrorCalibration)
        {
            List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeOrFluxListAB;

            if (fitInFluxSpace)
            {
                magnitudeOrFluxListAB = new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(
                                                magnitudeList.Select(x => x.ConvertToFlux()));
            }
            else
            {
                magnitudeOrFluxListAB = new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(
                                                magnitudeList.Select(x => (new Magnitude(x)).ConvertToMagnitudeSystem(MagnitudeSystem.Type.AB)));
            }

            if (applyFilterErrorCalibration)
            {
                for (int i = 0; i < filterList.Count; ++i)
                {
                    if (filterList[i].ZeroPointErrorCalibration > 1.0)
                    {
                        magnitudeOrFluxListAB[i].Error *= filterList[i].ZeroPointErrorCalibration;
                    }
                }
            }

            if (errorSofteningParameter != 0.0)
            {
                if (fitInFluxSpace)
                {
                    foreach (ValueWithErrorConvolveableFromFilterAndSpectrum fluxOrMag in magnitudeOrFluxListAB)
                    {
                        fluxOrMag.Error = Math.Sqrt(fluxOrMag.Error * fluxOrMag.Error +
                                                    errorSofteningParameter * errorSofteningParameter * fluxOrMag.Value * fluxOrMag.Value);
                    }
                }
                else
                {
                    foreach (ValueWithErrorConvolveableFromFilterAndSpectrum fluxOrMag in magnitudeOrFluxListAB)
                    {
                        fluxOrMag.Error = Math.Sqrt(fluxOrMag.Error * fluxOrMag.Error + errorSofteningParameter * errorSofteningParameter * 2.5 * 2.5 / Math.Log(10) / Math.Log(10));
                    }
                }
            }

            return magnitudeOrFluxListAB;
        }



        public static Spectrum GetBayesianBestTemplateFit(  Template spectrumTemplate, 
                                                            Prior priorInformation,
                                                            List<Filter> filterList, 
                                                            List<Magnitude> magnitudeList,
                                                            bool fitInFluxSpace,
                                                            double errorSofteningParameter,
                                                            bool applyFilterErrorCalibration,
                                                            out List<double> redshiftValues, 
                                                            out List<double> redshiftProbabilities, 
                                                            out bool error)
        {

            redshiftValues = spectrumTemplate.GetParameterCoverage("Redshift");
            redshiftProbabilities = new List<double>(redshiftValues.Count);
            foreach (double redshift in redshiftValues)
            {
                redshiftProbabilities.Add(0.0);
            }

            error = false;

            List<TemplateParameter> bestFitParameters = null;
            Spectrum bestFitSpectrum = null;

            //We cannot fit for a single filter, the constant offset is a parameter in the chi-square fit
            if (filterList.Count > 1 &&
                filterList.Count == magnitudeList.Count)
            {

                List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeOrFluxListAB = SetupABMagOrFluxListWithErrorScaling( filterList,
                                                                                                                                    magnitudeList,
                                                                                                                                    fitInFluxSpace,
                                                                                                                                    errorSofteningParameter,
                                                                                                                                    applyFilterErrorCalibration);

                double iterationStepSize;
                bool positiveFluxInRange;
                GetIterationStepSize(magnitudeOrFluxListAB, spectrumTemplate.IterationStepsWhenIgnoringLuminosity, fitInFluxSpace, out positiveFluxInRange, out iterationStepSize);

                double sigmaSqrReciprocalSum;
                if ((!fitInFluxSpace || positiveFluxInRange)
                    && priorInformation.VerifyParameterlist(spectrumTemplate.GetParameterList())
                    && GetChiSquareConstantFactor(magnitudeOrFluxListAB, fitInFluxSpace, out sigmaSqrReciprocalSum))
                {

                    double maxProbability = Constants.missingDouble;

                    double DOF = (magnitudeOrFluxListAB.Count - 1);

                    //Creating container for synthetic magnitudes
                    //Here the magnitude system property is copied
                    List<ValueWithErrorConvolveableFromFilterAndSpectrum> SyntheticMagnitudeOrFluxList;
                    if (fitInFluxSpace)
                    {
                        SyntheticMagnitudeOrFluxList = new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(
                                                        magnitudeOrFluxListAB.Select(x => new Flux((Flux)x)));
                    }
                    else
                    {
                        SyntheticMagnitudeOrFluxList = new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(
                                                        magnitudeOrFluxListAB.Select(x => new Magnitude((Magnitude)x)));
                    }

                    //The luminosity (constant magnitude offset) is iterated over once the normalized spectrum is generated
                    spectrumTemplate.IgnoreLuminosity = true;
                    spectrumTemplate.InitializeIteration();

                    do
                    {

                        int redshiftIndex = spectrumTemplate.GetParameterIndexInCoverage("Redshift");
                        double redshiftProbabilityAtIndex = redshiftProbabilities[redshiftIndex];

                        bool fitError = !GetSyntheticMagnitudesOrFluxes(SyntheticMagnitudeOrFluxList,
                                                                        filterList,
                                                                        spectrumTemplate,
                                                                        fitInFluxSpace);


                        if (!fitError)
                        {
                            double constantOffsetOrMultiplier = GetMinimumChiSquareFitParameter(magnitudeOrFluxListAB,
                                                                                                SyntheticMagnitudeOrFluxList,
                                                                                                fitInFluxSpace,
                                                                                                sigmaSqrReciprocalSum);


                            double fluxOrig;
                            if (fitInFluxSpace)
                            {
                                fluxOrig = SyntheticMagnitudeOrFluxList[0].Value;


                                for (int i = 0; i < filterList.Count; ++i)
                                {
                                    SyntheticMagnitudeOrFluxList[i].Value *= constantOffsetOrMultiplier;
                                }

                            }
                            else
                            {
                                fluxOrig = MagnitudeSystem.GetCGSFluxFromMagnitude(SyntheticMagnitudeOrFluxList[0].Value,
                                                                                    ((Magnitude)SyntheticMagnitudeOrFluxList[0]).MagSystem);


                                for (int i = 0; i < filterList.Count; ++i)
                                {
                                    SyntheticMagnitudeOrFluxList[i].Value += constantOffsetOrMultiplier;
                                }
                            }

                            double storedLuminosity = spectrumTemplate.GetParameterValue(0);

                            //Starting iteration in luminosity space in the center, then going from the edges
                            //This way bestFitParameters hopefully does not have to be recopied as many times (depending on prior)
                            EvaluateProbabilityAtCurrentParameters(magnitudeOrFluxListAB,
                                                                    SyntheticMagnitudeOrFluxList,
                                                                    storedLuminosity,
                                                                    fluxOrig,
                                                                    DOF,
                                                                    fitInFluxSpace,
                                                                    spectrumTemplate,
                                                                    priorInformation,
                                                                    ref redshiftProbabilityAtIndex,
                                                                    ref maxProbability,
                                                                    ref bestFitParameters);

                            if (spectrumTemplate.IterationStepsWhenIgnoringLuminosity > 1)
                            {
                                if (fitInFluxSpace)
                                {
                                    for (int i = 0; i < filterList.Count; ++i)
                                    {
                                        SyntheticMagnitudeOrFluxList[i].Value /= Math.Pow(iterationStepSize, (spectrumTemplate.IterationStepsWhenIgnoringLuminosity - 1) / 2);
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < filterList.Count; ++i)
                                    {
                                        SyntheticMagnitudeOrFluxList[i].Value -= ((spectrumTemplate.IterationStepsWhenIgnoringLuminosity - 1) / 2) * iterationStepSize;
                                    }
                                }


                                //Iteration in the luminosity space
                                for (int step = 0; step < spectrumTemplate.IterationStepsWhenIgnoringLuminosity; ++step)
                                {
                                    //Leaving out center point, which was done previously
                                    if (step != ((spectrumTemplate.IterationStepsWhenIgnoringLuminosity - 1) / 2))
                                    {
                                        EvaluateProbabilityAtCurrentParameters(magnitudeOrFluxListAB,
                                                                                SyntheticMagnitudeOrFluxList,
                                                                                storedLuminosity,
                                                                                fluxOrig,
                                                                                DOF,
                                                                                fitInFluxSpace,
                                                                                spectrumTemplate,
                                                                                priorInformation,
                                                                                ref redshiftProbabilityAtIndex,
                                                                                ref maxProbability,
                                                                                ref bestFitParameters);
                                    }

                                    if (fitInFluxSpace)
                                    {
                                        for (int i = 0; i < filterList.Count; ++i)
                                        {
                                            SyntheticMagnitudeOrFluxList[i].Value *= iterationStepSize;
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < filterList.Count; ++i)
                                        {
                                            SyntheticMagnitudeOrFluxList[i].Value += iterationStepSize;
                                        }
                                    }

                                }
                            }

                            spectrumTemplate.SetParameterValue(0, storedLuminosity);

                            redshiftProbabilities[redshiftIndex] = redshiftProbabilityAtIndex;
                        }

                    }
                    while (spectrumTemplate.IterateParameters());

                    if (!ReferenceEquals(bestFitParameters, null) && spectrumTemplate.SetParameterList(bestFitParameters))
                    {
                        bestFitSpectrum = spectrumTemplate.GenerateSpectrum();

                        //Normalizing redshift probability distribution
                        double probSum = 0.0;
                        foreach (double prob in redshiftProbabilities)
                        {
                            probSum += prob;
                        }

                        if (probSum != 0)
                        {
                            for (int i = 0; i < redshiftProbabilities.Count; ++i)
                            {
                                redshiftProbabilities[i] /= probSum;
                            }
                        }
                        else
                        {
                            error = true;
                        }
                    }
                    else
                    {
                        error = true;
                    }

                }
                else
                {
                    error = true;
                }
            }
            else
            {
                error = true;
            }

            return bestFitSpectrum;


        }


        public static bool CalibrateFilterZeroPoints(   double calibrationPrecision, //In magnitudes
                                                        int iterationLimit,
                                                        Template spectrumTemplate,
                                                        List<List<Filter>> filterLists,
                                                        List<List<Magnitude>> magnitudeLists,
                                                        List<double> redshiftList, //Can be null
                                                        bool fitInFluxSpace)
        {

            if (ReferenceEquals(filterLists, null) ||
                ReferenceEquals(magnitudeLists, null) ||
                filterLists.Count!=magnitudeLists.Count ||
                (!ReferenceEquals(redshiftList,null) && filterLists.Count!=redshiftList.Count) )
            {
                return false;
            }

            int iterationNumber = 0;
            double maxChange = 1e10;
            double prevMaxChange = 1e10;
            double prevPrevMaxChange = 1e10;

            List<Filter> distinctFilters = new List<Filter>();
            foreach(List<Filter> filterList in filterLists){
                distinctFilters.AddRange(filterList);
            }
            distinctFilters = distinctFilters.Distinct().ToList();


            //TODO Change this if multiple filters will have the same calibration
            Dictionary<Filter, int> filterIDDictionary = new Dictionary<Filter, int>();
            List<Object> filterLockList = new List<Object>(distinctFilters.Count);
            int count = 0;
            foreach (Filter filt in distinctFilters)
            {
                filterIDDictionary[filt]=count++;
                filterLockList.Add(new Object());

                if (fitInFluxSpace)
                {
                    filt.ZeroPointCorrection = 1.0;
                    filt.ZeroPointCorrectionInFlux = true;
                }
                else
                {
                    filt.ZeroPointCorrection = 0.0;
                    filt.ZeroPointCorrectionInFlux = false;
                }
            }


            do
            {
                //List<List<double>> synthListForFilter = new List<List<double>>(distinctFilters.Count);
                //List<List<double>> measuredListForFilter = new List<List<double>>(distinctFilters.Count);

                List<List<double>> synthMagListForFilter = new List<List<double>>(distinctFilters.Count);
                List<List<double>> measuredMagListForFilter = new List<List<double>>(distinctFilters.Count);
                List<List<double>> measuredMagErrorListForFilter = new List<List<double>>(distinctFilters.Count);
                List<List<double>> synthFluxListForFilter = new List<List<double>>(distinctFilters.Count);
                List<List<double>> measuredFluxListForFilter = new List<List<double>>(distinctFilters.Count);
                List<List<double>> measuredFluxErrorListForFilter = new List<List<double>>(distinctFilters.Count);
                List<List<double>> synthTemplateIDForFilter = new List<List<double>>(distinctFilters.Count);

                //TODO Change this as well if multiple filters will have the same calibration
                foreach (Filter filt in distinctFilters)
                {
                    //synthListForFilter.Add(new List<double>());
                    //measuredListForFilter.Add(new List<double>());
                    synthMagListForFilter.Add(new List<double>());
                    measuredMagListForFilter.Add(new List<double>());
                    measuredMagErrorListForFilter.Add(new List<double>());
                    synthFluxListForFilter.Add(new List<double>());
                    measuredFluxListForFilter.Add(new List<double>());
                    measuredFluxErrorListForFilter.Add(new List<double>());
                    synthTemplateIDForFilter.Add(new List<double>());
                }

                Parallel.For(0, filterLists.Count,
                                index =>
                                {
                                    List<Magnitude> localABMagList = new List<Magnitude>(magnitudeLists[index].Select(x => (new Magnitude(x)).ConvertToMagnitudeSystem(MagnitudeSystem.Type.AB)));
                                    Template templateLocalCopy = spectrumTemplate.CloneLightWeight();

                                    if (!ReferenceEquals(redshiftList,null))
                                    {
                                        templateLocalCopy.LockRedshiftAtFixedValue(redshiftList[index]);                                     
                                    }

                                    bool error;
                                    Spectrum bestFitSpec = GetBestChiSquareFit( templateLocalCopy, 
                                                                                filterLists[index],
                                                                                localABMagList,
                                                                                fitInFluxSpace,
                                                                                0.0,
                                                                                true,
                                                                                out error);

                                    if (!error)
                                    {
                                        
                                        Flux synthFlux = new Flux();

                                        for (int i = 0; i < localABMagList.Count; ++i)
                                        {
                                            synthFlux.ConvolveFromFilterAndSpectrum(bestFitSpec, filterLists[index][i]);

                                            double synthMagValue = synthFlux.ConvertToMagnitude(localABMagList[i].MagSystem).Value;

                                            Flux measuredFlux = localABMagList[i].ConvertToFlux();

                                            double typeID = templateLocalCopy.GetParameterValue("TypeID");

                                            int currentFilterID = filterIDDictionary[filterLists[index][i]];

                                            lock (filterLockList[currentFilterID])
                                            {
                                                synthMagListForFilter[currentFilterID].Add(synthMagValue);
                                                measuredMagListForFilter[currentFilterID].Add(localABMagList[i].Value);
                                                measuredMagErrorListForFilter[currentFilterID].Add(localABMagList[i].Error);
                                                synthFluxListForFilter[currentFilterID].Add(synthFlux.Value);
                                                measuredFluxListForFilter[currentFilterID].Add(measuredFlux.Value);
                                                measuredFluxErrorListForFilter[currentFilterID].Add(measuredFlux.Error);
                                                synthTemplateIDForFilter[currentFilterID].Add(typeID);
                                            }

                                        }

                                    }

                                });

                prevPrevMaxChange = prevMaxChange;
                prevMaxChange = maxChange;

                maxChange=0.0;
                int filterNumber=0;

                //using (StreamWriter logFile = new StreamWriter("calib_iter" + iterationNumber.ToString("D3") + ".txt"))
                //{
                    foreach (Filter filt in distinctFilters)
                    {
                        int filterID = filterIDDictionary[filt];

                        double magCorrection = 0.0;

                        //logFile.Write(filterNumber.ToString() + '\t');

                        if (true)
                        {

                            if (synthFluxListForFilter[filterID].Count > 0.0)
                            {
                                double rhs = 0.0;
                                double lhs = 0.0;
                                for (int i = 0; i < synthFluxListForFilter[filterID].Count; ++i)
                                {
                                    if (measuredFluxErrorListForFilter[filterID][i] > 0.0)
                                    {
                                        rhs += measuredFluxListForFilter[filterID][i] * synthFluxListForFilter[filterID][i] / measuredFluxErrorListForFilter[filterID][i] / measuredFluxErrorListForFilter[filterID][i];
                                        lhs += synthFluxListForFilter[filterID][i] * synthFluxListForFilter[filterID][i] / measuredFluxErrorListForFilter[filterID][i] / measuredFluxErrorListForFilter[filterID][i];
                                    }
                                }

                                if (fitInFluxSpace && lhs != 0.0)
                                {
                                    filt.ZeroPointCorrection *= rhs/lhs;

                                    //For fluxes, (alpha*f-f)/f = (alpha-1) is a percentage error, it has to be divided by ln(10)/(-2.5) to give a magnitude error
                                    magCorrection = (rhs / lhs - 1) / Math.Log(10) * (-2.5);
                                }

                                double stdev = 0.0;
                                int stdevcount = 0;

                                if (lhs != 0.0)
                                {

                                    for (int i = 0; i < synthFluxListForFilter[filterID].Count; ++i)
                                    {
                                        if (measuredFluxErrorListForFilter[filterID][i] > 0.0)
                                        {
                                            stdev += (measuredFluxListForFilter[filterID][i] - synthFluxListForFilter[filterID][i] * rhs / lhs) * (measuredFluxListForFilter[filterID][i] - synthFluxListForFilter[filterID][i] * rhs / lhs) / measuredFluxErrorListForFilter[filterID][i] / measuredFluxErrorListForFilter[filterID][i];
                                            ++stdevcount;
                                        }
                                    }

                                    if (fitInFluxSpace && stdevcount > 1)
                                    {
                                        filt.ZeroPointErrorCalibration = Math.Sqrt(stdev / (stdevcount - 1));
                                    }

                                }


                                //logFile.Write((rhs / lhs).ToString() + '\t' + ((rhs / lhs - 1) / Math.Log(10) * (-2.5)).ToString() + '\t' + Math.Sqrt(stdev / (stdevcount - 1)).ToString() + '\t');

                            }

                        }
                        if (true)
                        {
                            if (synthMagListForFilter[filterID].Count > 0.0)
                            {

                                double rhs = 0.0;
                                double lhs = 0.0;
                                for (int i = 0; i < synthMagListForFilter[filterID].Count; ++i)
                                {
                                    if (measuredMagErrorListForFilter[filterID][i] > 0.0)
                                    {
                                        rhs += (measuredMagListForFilter[filterID][i] - synthMagListForFilter[filterID][i]) / measuredMagErrorListForFilter[filterID][i] / measuredMagErrorListForFilter[filterID][i];
                                        lhs += 1.0 / measuredMagErrorListForFilter[filterID][i] / measuredMagErrorListForFilter[filterID][i];
                                    }
                                }

                                if (!fitInFluxSpace && lhs != 0.0)
                                {
                                    filt.ZeroPointCorrection += rhs / lhs;

                                    magCorrection = rhs / lhs;
                                }

                                double stdev = 0.0;
                                int stdevcount = 0;

                                if (lhs != 0.0)
                                {

                                    for (int i = 0; i < synthMagListForFilter[filterID].Count; ++i)
                                    {
                                        if (measuredMagErrorListForFilter[filterID][i] > 0.0)
                                        {
                                            stdev += (measuredMagListForFilter[filterID][i] - synthMagListForFilter[filterID][i] - rhs / lhs) * (measuredMagListForFilter[filterID][i] - synthMagListForFilter[filterID][i] - rhs / lhs) / measuredMagErrorListForFilter[filterID][i] / measuredMagErrorListForFilter[filterID][i];
                                            ++stdevcount;
                                        }
                                    }

                                    if (!fitInFluxSpace && stdevcount > 1)
                                    {
                                        filt.ZeroPointErrorCalibration = Math.Sqrt(stdev/(stdevcount-1));
                                    }

                                }

                                //logFile.Write((rhs / lhs).ToString() + '\t' + Math.Sqrt(stdev / (stdevcount - 1)).ToString() + '\n');
                            }
                        }

                        if (Math.Abs(magCorrection) > maxChange)
                        {
                            maxChange = Math.Abs(magCorrection);
                        }

                        /*using (StreamWriter outputFile = new StreamWriter("calib_iter" + iterationNumber.ToString("D3") + "_filt" + filterNumber.ToString("D3") + ".txt"))
                        {
                            for (int i = 0; i < synthMagListForFilter[filterID].Count; ++i)
                            {
                                outputFile.Write(synthMagListForFilter[filterID][i].ToString() + '\t' + 
                                                    measuredMagListForFilter[filterID][i].ToString() + '\t' +
                                                    measuredMagErrorListForFilter[filterID][i].ToString() + '\t' +
                                                    synthFluxListForFilter[filterID][i].ToString() + '\t' +
                                                    measuredFluxListForFilter[filterID][i].ToString() + '\t' +
                                                    measuredFluxErrorListForFilter[filterID][i].ToString() + '\t' +
                                                    synthTemplateIDForFilter[filterID][i].ToString() + '\n');

                            }

                        }*/

                        ++filterNumber;

                    }

                //}

                ++iterationNumber;

            }
            while ( iterationNumber < iterationLimit &&
                    (maxChange > calibrationPrecision || prevMaxChange > calibrationPrecision || prevPrevMaxChange > calibrationPrecision));


            //TODO propagate the zero point correction to other filters if more are calibrated together

            return true;
        }

    }
}

