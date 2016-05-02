using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace Jhu.PhotoZ
{
    public abstract class Template
    {

        public Template(TemplateParameterAdditive aRedshift, TemplateParameterMultiplicative aLuminosity = null) : this()
        {
            parameterList = new List<TemplateParameter>(5);

            if (!ReferenceEquals(aLuminosity, null))
            {
                parameterList.Add(new TemplateParameterMultiplicative(aLuminosity) { Name = "Luminosity" });
            } 
            else 
            {
                parameterList.Add(new TemplateParameterMultiplicative() { Name = "Luminosity", Value = 1.0 });
            }
            parameterList.Add(new TemplateParameterAdditive(aRedshift) { Name = "Redshift" });

            IgnoreLuminosity = true;
            IterationStepsWhenIgnoringLuminosity = 21;

            iterationStartID = Constants.missingInt;
            iterationLastID = Constants.missingInt;

            currentSpectrum = null;
            syntheticFluxCache = new ConcurrentDictionary<Filter, double[]>();

            redshiftCoverageSize = Constants.missingInt;
            redshiftIndexInCoverage = Constants.missingInt;
            savedRedshiftParameter = null;
        }

        protected Template() {}

        //Creates a lightweight copy so that the fitting steps can be run parallelly
        public abstract Template CloneLightWeight();

        protected void CloneLightWeighValues(Template other)
        {
            other.parameterList = new List<TemplateParameter>(parameterList.Select(x => (TemplateParameter)x.Clone()));

            other.IgnoreLuminosity = IgnoreLuminosity;
            other.IterationStepsWhenIgnoringLuminosity = IterationStepsWhenIgnoringLuminosity;

            other.iterationStartID = iterationStartID;
            other.iterationLastID = iterationLastID;

            other.redshiftCoverageSize = redshiftCoverageSize;
            other.redshiftIndexInCoverage = redshiftIndexInCoverage;
            if (!ReferenceEquals(savedRedshiftParameter, null))
            {
                other.savedRedshiftParameter = (TemplateParameter)savedRedshiftParameter.Clone();
            }
            else
            {
                other.savedRedshiftParameter = null;
            }

            if (!ReferenceEquals(currentSpectrum, null))
            {
                other.currentSpectrum = new Spectrum(currentSpectrum);
            }
            else 
            {
                other.currentSpectrum = null;
            }

            other.syntheticFluxCache = syntheticFluxCache;
        }

        private ConcurrentDictionary<Filter, double[]> syntheticFluxCache;
        private int redshiftCoverageSize;
        private int redshiftIndexInCoverage;
        private TemplateParameter savedRedshiftParameter;

        //Luminosity and redhsift are "post-process" parameters, they are passed to the spectrum object
        //So they are handled separately, in that they always occupy slot 1 and 2, respectively, in the parameter list
        //All the other parameters (that create the restframe spectrum) follow them
        protected List<TemplateParameter> parameterList;

        //The option to ignore luminosity in the parameter iteration 
        //This has to be used when fitting the constant magnitude shift for the minimum chi-square fit
        //Also used when we do not want to cover a fixed range in the luminosity, but instead iterate
        //around a central (min chi-square) magnitude value in magnitudes
        public bool IgnoreLuminosity { get; set; }

        //When iterating around a central (min chi-square) magnitude/flux value, these values specify how many positions to evaluate
        //The size of the steps is currently hardcoded in Templatefit.cs to span 3 times the average error in both directions
        public int IterationStepsWhenIgnoringLuminosity { get; set; }

        //Helper values that enable the iteration process
        private int iterationStartID;
        private int iterationLastID;

        //Keeping the lastly generated spectrum during iteration so that simple transformations
        //can be done without recalculating everything
        private Spectrum currentSpectrum;

        public abstract Spectrum GenerateSpectrum();

        //Returns the actual parameter list so that a prior can be calculated
        //Parameters should not be externally modified inside an iteration
        //(Except luminosity with IgnoreLuminosity==true)
        public List<TemplateParameter> GetParameterList()
        {
            return parameterList;
        }

        public bool SetParameterList(List<TemplateParameter> aParams)
        {
            if (parameterList.Count != aParams.Count)
            {
                return false;
            }
            
            for (int i = 0; i < parameterList.Count; ++i)
            {
                if (parameterList[i].Name != aParams[i].Name)
                {
                    return false;
                }
            }

            parameterList = new List<TemplateParameter>(aParams.Select(x => (TemplateParameter)x.Clone()));
            return true;
        }


        public List<double> GetParameterCoverage(string paramName)
        {
            foreach (TemplateParameter param in parameterList)
            {
                if (param.Name == paramName)
                {
                    return param.GetParameterCoverage();
                }
            }

            return new List<double>(0);
        }

        public int GetParameterIndexInCoverage(string paramName)
        {
            foreach (TemplateParameter param in parameterList)
            {
                if (param.Name == paramName)
                {
                    return param.GetParameterIndexInCoverage();
                }
            }

            return Constants.missingInt;
        }

        public double GetClosestParameterValueInCoverage(string paramName, double paramValue)
        {
            foreach (TemplateParameter param in parameterList)
            {
                if (param.Name == paramName)
                {
                    return param.GetClosestParameterValueInCoverage(paramValue);
                }
            }

            return Constants.missingInt;
        }

        public double GetParameterValue(string paramName)
        {
            foreach (TemplateParameter param in parameterList)
            {
                if (param.Name == paramName)
                {
                    return param.Value;
                }
            }

            return Constants.missingDouble;
        }

        public bool SetParameterValue(string paramName, double aValue)
        {
            foreach (TemplateParameter param in parameterList)
            {
                if (param.Name == paramName)
                {
                    param.Value = aValue;
                    return true;
                }
            }

            return false;
        }

        public double GetParameterValue(int paramIndex)
        {
            if (paramIndex>=0 && paramIndex < parameterList.Count)
            {
                return parameterList[paramIndex].Value;
            }

            return Constants.missingDouble;
        }

        public bool SetParameterValue(int paramIndex, double aValue)
        {
            if (paramIndex>=0 && paramIndex < parameterList.Count)
            {
                parameterList[paramIndex].Value = aValue;
                return true;
            }

            return false;
        }


        public double LockRedshiftAtFixedValue(double aRedshift)
        {
            if (ReferenceEquals(savedRedshiftParameter, null))
            { 
                savedRedshiftParameter = parameterList[1];
            }

            double closestRedshift = savedRedshiftParameter.GetClosestParameterValueInCoverage(aRedshift);
            redshiftIndexInCoverage = savedRedshiftParameter.GetParameterIndexInCoverage(closestRedshift);
            redshiftCoverageSize = savedRedshiftParameter.GetParameterCoverageSize();           
            
            parameterList[1] = new TemplateParameterAdditive(closestRedshift, closestRedshift, 1.0) { Name = "Redshift", Value = closestRedshift };


            //Returns the value where it was actually locked, based on the coverage
            return closestRedshift;
        }

        public bool UnlockRedshiftFromFixedValue()
        {
            if (!ReferenceEquals(savedRedshiftParameter, null))
            {
                parameterList[1] = savedRedshiftParameter;
                savedRedshiftParameter = null;

                redshiftIndexInCoverage = Constants.missingInt;
                redshiftCoverageSize = Constants.missingInt;

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGetFluxFromCache(Filter aFilter, out double cachedFlux)
        {
            double[] fluxArray;
            if (syntheticFluxCache.TryGetValue(aFilter, out fluxArray))
            {
                cachedFlux = Interlocked.CompareExchange(ref fluxArray[GetIndexInFluxCacheArray()], -1.0, -1.0);

                return cachedFlux != Constants.missingDouble;
            }
            else
            {
                cachedFlux = Constants.missingDouble;
                return false;
            }
        }

        public bool TryAddFluxToCache(Filter aFilter, double uncorrectedFlux)
        {
            double[] fluxArray;
            if (syntheticFluxCache.TryGetValue(aFilter, out fluxArray))
            {
                
                Interlocked.CompareExchange(ref fluxArray[GetIndexInFluxCacheArray()], uncorrectedFlux, Constants.missingDouble);

                return true;
            }
            else 
            {
                fluxArray = new double[GetFluxCacheArraySize()];

                for (int i = 0; i < fluxArray.Length; ++i)
                {
                    fluxArray[i]=Constants.missingDouble;
                }

                fluxArray[GetIndexInFluxCacheArray()] = uncorrectedFlux;

                return syntheticFluxCache.TryAdd(aFilter, fluxArray);
            }
        }


        private int GetIndexInFluxCacheArray()
        {
            int blockSize = 1;
            int index = 0;

            int startingParam = 0;
            if (IgnoreLuminosity)
            {
                startingParam = 1;
            }

            for (int i = startingParam; i < parameterList.Count; ++i)
            {
                if (i == 1 && redshiftCoverageSize != Constants.missingInt)
                {
                    index += redshiftIndexInCoverage * blockSize;
                    blockSize *= redshiftCoverageSize;
                }
                else
                {
                    index += parameterList[i].GetParameterIndexInCoverage() * blockSize;
                    blockSize *= parameterList[i].GetParameterCoverageSize();
                }
            }

            return index;
        }

        private int GetFluxCacheArraySize()
        {
            int blockSize = 1;

            int startingParam = 0;
            if (IgnoreLuminosity)
            {
                startingParam = 1;
            }

            for (int i = startingParam; i < parameterList.Count; ++i)
            {
                if (i == 1 && redshiftCoverageSize != Constants.missingInt)
                {
                    blockSize *= redshiftCoverageSize;
                }
                else
                {
                    blockSize *= parameterList[i].GetParameterCoverageSize();
                }
            }

            return blockSize;
        }


        public void InitializeIteration()
        {
            foreach (TemplateParameter param in parameterList)
            {
                param.ResetValue();
            }

            if (IgnoreLuminosity)
            {
                parameterList[0].Value = 1.0;
                iterationStartID = 1;
            }
            else
            {
                iterationStartID = 0;
            }

            iterationLastID = Constants.missingInt;

            currentSpectrum = null;
        }

        public bool IterateParameters()
        {
            int iterationCurrentID = iterationStartID;

            //Stepping the lowest-level parameter while possible
            //When not possible, stepping the following order, while resetting the lowest and continuing stepping the lowest
            //When stepping two levels are both not possible, stepping the third, while resetting first and second
            //And so on for all levels
            while (iterationCurrentID < parameterList.Count() && !parameterList[iterationCurrentID].StepValue())
            {
                parameterList[iterationCurrentID].ResetValue();
                ++iterationCurrentID;

                //Making sure that spectrum is generated at least once for parameters higher that redshift and luminosity
                if (iterationCurrentID > 1)
                {
                    currentSpectrum = null;
                }
            }

            iterationLastID = iterationCurrentID;

            return (iterationLastID < parameterList.Count());
        }

        public Spectrum GenerateIteratedSpectrum()
        {
            if (iterationLastID <= 1 && !ReferenceEquals(currentSpectrum, null))
            {
                currentSpectrum.Luminosity = parameterList[0].Value;
                currentSpectrum.Redshift = parameterList[1].Value;
            }
            else
            {
                currentSpectrum = GenerateSpectrum();
            }

            return currentSpectrum;
        }
    }
}
