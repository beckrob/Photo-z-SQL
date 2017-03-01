//------------------------------------------------------------------------------
// <copyright file="CSSqlFunction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Server;
using Jhu.SqlServer.Array;
using System.Globalization;


using Jhu.PhotoZ;

public partial class UserDefinedFunctions
{

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = true,
                                            Name = "Util.ParseDoubleArray1D")]
    public static SqlBytes ParseDoubleArray1D([SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = -1)] SqlString doubleListAsStr)
    {
        string[] separatedDoubleListAsStr = ((string)doubleListAsStr).Split((CultureInfo.CurrentCulture.TextInfo.ListSeparator+"[]").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        SqlFloatArrayMax arr = new SqlFloatArrayMax(separatedDoubleListAsStr.Length);

        for (int i=0; i<separatedDoubleListAsStr.Length; ++i)
        {
            double tmp;
            if (double.TryParse(separatedDoubleListAsStr[i], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out tmp))
            {
                arr[i]=tmp;
            }      
        }

        return arr.ToSqlBuffer();
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = true,
                                            Name = "Util.ParseDoubleArray1DInvariant")]
    public static SqlBytes ParseDoubleArray1DInvariant([SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = -1)] SqlString doubleListAsStr)
    {
        string[] separatedDoubleListAsStr = ((string)doubleListAsStr).Split((CultureInfo.InvariantCulture.TextInfo.ListSeparator + "[]").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        SqlFloatArrayMax arr = new SqlFloatArrayMax(separatedDoubleListAsStr.Length);

        for (int i = 0; i < separatedDoubleListAsStr.Length; ++i)
        {
            double tmp;
            if (double.TryParse(separatedDoubleListAsStr[i], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out tmp))
            {
                arr[i] = tmp;
            }
        }

        return arr.ToSqlBuffer();
    }


    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = true,
                                            Name = "Util.ParseIntArray1D")]
    public static SqlBytes ParseIntArray1D([SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = -1)] SqlString intListAsStr)
    {
        string[] separatedIntListAsStr = ((string)intListAsStr).Split((CultureInfo.CurrentCulture.TextInfo.ListSeparator + "[]").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        SqlIntArrayMax arr = new SqlIntArrayMax(separatedIntListAsStr.Length);

        for (int i = 0; i < separatedIntListAsStr.Length; ++i)
        {
            int tmp;
            if (int.TryParse(separatedIntListAsStr[i], NumberStyles.Integer, CultureInfo.CurrentCulture, out tmp))
            {
                arr[i] = tmp;
            }
        }

        return arr.ToSqlBuffer();
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = true,
                                            Name = "Util.ParseIntArray1DInvariant")]
    public static SqlBytes ParseIntArray1DInvariant([SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = -1)] SqlString intListAsStr)
    {
        string[] separatedIntListAsStr = ((string)intListAsStr).Split((CultureInfo.InvariantCulture.TextInfo.ListSeparator + "[]").ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        SqlIntArrayMax arr = new SqlIntArrayMax(separatedIntListAsStr.Length);

        for (int i = 0; i < separatedIntListAsStr.Length; ++i)
        {
            int tmp;
            if (int.TryParse(separatedIntListAsStr[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out tmp))
            {
                arr[i] = tmp;
            }
        }

        return arr.ToSqlBuffer();
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Util.GetTotalCLRMemoryUsage")]
    public static SqlInt64 GetTotalCLRMemoryUsage()
    {
        return (SqlInt64)GC.GetTotalMemory(true);
    }


    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupTemplateList_URL")]
    public static SqlInt32 PhotoZSetupTemplateList_URL( [SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = -1)] SqlString templateURLList,
                                                        SqlDouble fluxMultiplier,
                                                        SqlDouble redshiftFrom,
                                                        SqlDouble redshiftTo,
                                                        SqlDouble redshiftStep,
                                                        SqlBoolean logarithmicRedshiftSteps,
                                                        SqlInt32 luminosityStepNumber)
    {
        int retCode = -9999;
        try
        {
            List<string> templateURLs = new List<string>(((string)templateURLList).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

            retCode = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupTemplateList(templateURLs,
                                                                                (double)fluxMultiplier,
                                                                                (double)redshiftFrom,
                                                                                (double)redshiftTo,
                                                                                (double)redshiftStep,
                                                                                (bool)logarithmicRedshiftSteps,
                                                                                (int)luminosityStepNumber);
        }
        catch
        {
            retCode = -9999;
        }

        return retCode;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupTemplateList_ID")]
    public static SqlInt32 PhotoZSetupTemplateList_ID( SqlBytes templateIDList,
                                                       SqlDouble fluxMultiplier,
                                                       SqlDouble redshiftFrom,
                                                       SqlDouble redshiftTo,
                                                       SqlDouble redshiftStep,
                                                       SqlBoolean logarithmicRedshiftSteps,
                                                       SqlInt32 luminosityStepNumber)
    {
        int retCode = -9999;
        try
        {
            List<int> templateIDs = new List<int>(new SqlIntArrayMax(templateIDList).ToArray());
            List<string> templateURLs = new List<string>(templateIDs.Select(x => "http://voservices.net/spectrum/search_details.aspx?format=ascii&id=ivo%3a%2f%2fjhu%2ftemplates%23" + x.ToString()));

            retCode = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupTemplateList(templateURLs,
                                                                                (double)fluxMultiplier,
                                                                                (double)redshiftFrom,
                                                                                (double)redshiftTo,
                                                                                (double)redshiftStep,
                                                                                (bool)logarithmicRedshiftSteps,
                                                                                (int)luminosityStepNumber);
        }
        catch
        {
            retCode = -9999;
        }

        return retCode;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupTemplateList_LuminositySpecified_URL")]
    public static SqlInt32 PhotoZSetupTemplateList_LuminositySpecified_URL( [SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = -1)] SqlString templateURLList,
                                                                            SqlDouble fluxMultiplier,
                                                                            SqlDouble redshiftFrom,
                                                                            SqlDouble redshiftTo,
                                                                            SqlDouble redshiftStep,
                                                                            SqlBoolean logarithmicRedshiftSteps,
                                                                            SqlDouble luminosityFrom,
                                                                            SqlDouble luminosityTo,
                                                                            SqlDouble luminosityStep,
                                                                            SqlBoolean logarithmicLuminositySteps)
    {
        int retCode = -9999;
        try
        {
            List<string> templateURLs = new List<string>(((string)templateURLList).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

            retCode = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupTemplateList(templateURLs,
                                                                                (double)fluxMultiplier,
                                                                                (double)redshiftFrom,
                                                                                (double)redshiftTo,
                                                                                (double)redshiftStep,
                                                                                (bool)logarithmicRedshiftSteps,
                                                                                Constants.missingInt,
                                                                                (double)luminosityFrom,
                                                                                (double)luminosityTo,
                                                                                (double)luminosityStep,
                                                                                (bool)logarithmicLuminositySteps);
        }
        catch
        {
            retCode = -9999;
        }

        return retCode;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupTemplateList_LuminositySpecified_ID")]
    public static SqlInt32 PhotoZSetupTemplateList_LuminositySpecified_ID(  SqlBytes templateIDList,
                                                                            SqlDouble fluxMultiplier,
                                                                            SqlDouble redshiftFrom,
                                                                            SqlDouble redshiftTo,
                                                                            SqlDouble redshiftStep,
                                                                            SqlBoolean logarithmicRedshiftSteps,
                                                                            SqlDouble luminosityFrom,
                                                                            SqlDouble luminosityTo,
                                                                            SqlDouble luminosityStep,
                                                                            SqlBoolean logarithmicLuminositySteps)
    {
        int retCode = -9999;
        try
        {
            List<int> templateIDs = new List<int>(new SqlIntArrayMax(templateIDList).ToArray());
            List<string> templateURLs = new List<string>(templateIDs.Select(x => "http://voservices.net/spectrum/search_details.aspx?format=ascii&id=ivo%3a%2f%2fjhu%2ftemplates%23" + x.ToString()));

            retCode = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupTemplateList(templateURLs,
                                                                                (double)fluxMultiplier,
                                                                                (double)redshiftFrom,
                                                                                (double)redshiftTo,
                                                                                (double)redshiftStep,
                                                                                (bool)logarithmicRedshiftSteps,
                                                                                Constants.missingInt,
                                                                                (double)luminosityFrom,
                                                                                (double)luminosityTo,
                                                                                (double)luminosityStep,
                                                                                (bool)logarithmicLuminositySteps);
        }
        catch
        {
            retCode = -9999;
        }

        return retCode;
    }


    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupExtinctionLaw_URL")]
    public static SqlInt32 PhotoZSetupExtinctionLaw_URL([SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = -1)] SqlString referenceSpectrumURL,
                                                        SqlDouble dustParameterR_V,
                                                        SqlBoolean useFitzpatrickExtinctionInsteadOfODonnell)
    {
        int retCode = -9999;
        try
        {
            retCode = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupExtinctionLaw(   (string)referenceSpectrumURL,
                                                                                    (double)dustParameterR_V,
                                                                                    (bool)useFitzpatrickExtinctionInsteadOfODonnell);
        }
        catch
        {
            retCode = -9999;
        }

        return retCode;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupExtinctionLaw_ID")]
    public static SqlInt32 PhotoZSetupExtinctionLaw_ID( SqlInt32 referenceSpectrumID,
                                                        SqlDouble dustParameterR_V,
                                                        SqlBoolean useFitzpatrickExtinctionInsteadOfODonnell)
    {
        int retCode = -9999;
        try
        {
            string referenceSpectrumURL = "http://voservices.net/spectrum/search_details.aspx?format=ascii&id=ivo%3a%2f%2fjhu%2ftemplates%23" + ((int)referenceSpectrumID).ToString();
            retCode = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupExtinctionLaw(   referenceSpectrumURL,
                                                                                    (double)dustParameterR_V,
                                                                                    (bool)useFitzpatrickExtinctionInsteadOfODonnell);
        }
        catch
        {
            retCode = -9999;
        }

        return retCode;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.RemoveExtinctionLaw")]
    public static SqlInt32 PhotoZRemoveExtinctionLaw()
    {
        Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.RemoveExtinctionLaw();
        return 0;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.AddMissingValueSpecifier")]
    public static SqlInt32 PhotoZAddMissingValueSpecifier(SqlDouble aMissingValue)
    {
        Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.AddMissingValueSpecifier( (double)aMissingValue );
        return 0;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.ClearMissingValueSpecifiers")]
    public static SqlInt32 PhotoZClearMissingValueSpecifiers()
    {
        Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.ClearMissingValueSpecifiers();
        return 0;
    }


    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupFlatPrior")]
    public static SqlInt32 PhotoZSetupFlatPrior()
    {
        try
        {
            Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupFlatPrior();
        }
        catch
        {
            return -9999;
        }

        return 0;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupAbsoluteMagnitudeLimitPrior_URL")]
    public static SqlInt32 PhotoZSetupAbsoluteMagnitudeLimitPrior_URL(  [SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = -1)] SqlString referenceFilterURL,
                                                                        SqlDouble absMagLimit,
                                                                        SqlDouble h,
                                                                        SqlDouble Omega_m,
                                                                        SqlDouble Omega_lambda)
    {
        int retCode = -9999;

        try
        {
            retCode = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupAbsoluteMagnitudeLimitPrior( (string)referenceFilterURL,
                                                                                                (double)absMagLimit,
                                                                                                (double)h,
                                                                                                (double)Omega_m,
                                                                                                (double)Omega_lambda);
        }
        catch
        {
            return -9999;
        }

        return retCode;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupAbsoluteMagnitudeLimitPrior_ID")]
    public static SqlInt32 PhotoZSetupAbsoluteMagnitudeLimitPrior_ID( SqlInt32 referenceFilterID,
                                                                      SqlDouble absMagLimit,
                                                                      SqlDouble h,
                                                                      SqlDouble Omega_m,
                                                                      SqlDouble Omega_lambda)
    {
        int retCode = -9999;

        try
        {

            retCode = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupAbsoluteMagnitudeLimitPrior( "http://voservices.net/filter/filterascii.aspx?FilterID=" + ((int)referenceFilterID).ToString(),
                                                                                                (double)absMagLimit,
                                                                                                (double)h,
                                                                                                (double)Omega_m,
                                                                                                (double)Omega_lambda);
        }
        catch
        {
            return -9999;
        }

        return retCode;
    }

    //Reference filter for this prior was originally: WFPC2 F814W
    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupBenitezHDFPrior_URL")]
    public static SqlInt32 PhotoZSetupBenitezHDFPrior_URL(  [SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = -1)] SqlString referenceFilterURL,
                                                            SqlBoolean usingLePhareTemplates)
    {
        int retCode = -9999;

        try
        {
            retCode = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupBenitezHDFPrior( (string)referenceFilterURL,
                                                                                    (bool)usingLePhareTemplates);
        }
        catch
        {
            return -9999;
        }

        return retCode;
    }

    //Reference filter for this prior was originally: WFPC2 F814W
    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupBenitezHDFPrior_ID")]
    public static SqlInt32 PhotoZSetupBenitezHDFPrior_ID(   SqlInt32 referenceFilterID,
                                                            SqlBoolean usingLePhareTemplates)
    {
        int retCode = -9999;

        try
        {

            retCode = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupBenitezHDFPrior( "http://voservices.net/filter/filterascii.aspx?FilterID=" + ((int)referenceFilterID).ToString(),
                                                                                    (bool)usingLePhareTemplates);
        }
        catch
        {
            return -9999;
        }

        return retCode;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.SetupTemplateTypePrior")]
    public static SqlInt32 PhotoZSetupTemplateTypePrior(SqlBytes templateProbabilities)
    {
        int retCode = -9999;

        try
        {
            double[] templateProbs = new SqlFloatArrayMax(templateProbabilities).ToArray();

            retCode = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.SetupTemplateTypePrior(templateProbs);
        }
        catch
        {
            return -9999;
        }

        return retCode;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false,
                                            Name = "Config.RemoveInitialization")]
    public static SqlInt32 PhotoZRemoveInitialization()
    {
        Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.RemoveInitialization();
        return 0;
    }


    public static void RedshiftProbDensityFillRow(Object obj, out SqlDouble Redshift, out SqlDouble Probability)
    {
        Tuple<double,double> valuePair = (Tuple<double,double>)obj;
        Redshift = (SqlDouble)valuePair.Item1;
        Probability = (SqlDouble)valuePair.Item2;
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess=SystemDataAccessKind.None,
                                            IsPrecise=false,
                                            IsDeterministic=false,
                                            FillRowMethodName = "RedshiftProbDensityFillRow",
                                            TableDefinition = "Redshift float, Probability float",
                                            Name = "Compute.PhotoZBayesian_URL")]
    public static System.Collections.IEnumerable CalculatePhotoZBayesian_URL(   SqlBytes magOrFluxArray,
                                                                                SqlBytes magOrFluxErrorArray,
                                                                                SqlBoolean inputInMags,
                                                                                [SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = -1)] SqlString filterURLList,
                                                                                SqlDouble extinctionMapValue, 
                                                                                SqlBoolean fitInFluxSpace, 
                                                                                SqlDouble errorSmoothening)
    {
        try
        {
            double[] magsOrFluxes = new SqlFloatArrayMax(magOrFluxArray).ToArray();
            double[] magOrFluxErrors = new SqlFloatArrayMax(magOrFluxErrorArray).ToArray();

            List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeFluxList =
                new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(magsOrFluxes.Zip(magOrFluxErrors, (a, b) => ((bool)inputInMags)
                    ? (ValueWithErrorConvolveableFromFilterAndSpectrum)new Magnitude() { Value = a, Error = b, MagSystem = MagnitudeSystem.Type.AB }
                    : (ValueWithErrorConvolveableFromFilterAndSpectrum)new Flux() { Value = a, Error = b }));


            List<string> filterURLs = new List<string>(((string)filterURLList).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

            List<double> redshifts, redshiftProbabilities;
            int fitError;
            Spectrum result = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.CalculatePhotoZBayesian(  magnitudeFluxList,
                                                                                                filterURLs,
                                                                                                (double)extinctionMapValue,
                                                                                                (bool)fitInFluxSpace,
                                                                                                (double)errorSmoothening,
                                                                                                false,
                                                                                                out redshifts,
                                                                                                out redshiftProbabilities,
                                                                                                out fitError);

            if (fitError==0)
            {
                //Create the IEnumerable<Tuple<double,double>> list of probability distributions
                return redshifts.Zip(redshiftProbabilities, (a, b) => Tuple.Create(a, b));
            }
            else
            {
                return new Tuple<double, double>[1] { new Tuple<double, double>(fitError, 0) };
            }
        }
        catch
        {
            return new Tuple<double, double>[1] { new Tuple<double, double>(-9999, 0) };
        }
    }

    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = false,
                                            IsDeterministic = false,
                                            FillRowMethodName = "RedshiftProbDensityFillRow",
                                            TableDefinition = "Redshift float, Probability float",
                                            Name = "Compute.PhotoZBayesian_ID")]
    public static System.Collections.IEnumerable CalculatePhotoZBayesian_ID(SqlBytes magOrFluxArray,
                                                                            SqlBytes magOrFluxErrorArray,
                                                                            SqlBoolean inputInMags,
                                                                            SqlBytes filterIDList,
                                                                            SqlDouble extinctionMapValue,
                                                                            SqlBoolean fitInFluxSpace,
                                                                            SqlDouble errorSmoothening)
    {
        try
        {
            double[] magsOrFluxes = new SqlFloatArrayMax(magOrFluxArray).ToArray();
            double[] magOrFluxErrors = new SqlFloatArrayMax(magOrFluxErrorArray).ToArray();

            List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeFluxList =
                new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(magsOrFluxes.Zip(magOrFluxErrors, (a, b) => ((bool)inputInMags)
                    ? (ValueWithErrorConvolveableFromFilterAndSpectrum)new Magnitude() { Value = a, Error = b, MagSystem = MagnitudeSystem.Type.AB }
                    : (ValueWithErrorConvolveableFromFilterAndSpectrum)new Flux() { Value = a, Error = b }));

            List<int> filterIDs = new List<int>(new SqlIntArrayMax(filterIDList).ToArray());
            List<string> filterURLList = new List<string>(filterIDs.Select(x => "http://voservices.net/filter/filterascii.aspx?FilterID=" + x.ToString()));

            List<double> redshifts, redshiftProbabilities;
            int fitError;
            Spectrum result = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.CalculatePhotoZBayesian(  magnitudeFluxList,
                                                                                                filterURLList,
                                                                                                (double)extinctionMapValue,
                                                                                                (bool)fitInFluxSpace,
                                                                                                (double)errorSmoothening,
                                                                                                false,
                                                                                                out redshifts,
                                                                                                out redshiftProbabilities,
                                                                                                out fitError);

            if (fitError==0)
            {
                //Create the IEnumerable<Tuple<double,double>> list of probability distributions
                return redshifts.Zip(redshiftProbabilities, (a, b) => Tuple.Create(a, b));
            }
            else
            {
                return new Tuple<double, double>[1] { new Tuple<double, double>(fitError, 0) };
            }
        }
        catch
        {
            return new Tuple<double, double>[1] { new Tuple<double, double>(-9999, 0) };
        }
    }


    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = false,
                                            IsDeterministic = false,
                                            Name = "Compute.PhotoZMinChiSqr_URL")]
    public static SqlDouble CalculatePhotoZMinChiSqr_URL(   SqlBytes magOrFluxArray,
                                                            SqlBytes magOrFluxErrorArray,
                                                            SqlBoolean inputInMags,
                                                            [SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = -1)] SqlString filterURLList,
                                                            SqlDouble extinctionMapValue,
                                                            SqlBoolean fitInFluxSpace,
                                                            SqlDouble errorSmoothening)
    {
        try
        {
            double[] magsOrFluxes = new SqlFloatArrayMax(magOrFluxArray).ToArray();
            double[] magOrFluxErrors = new SqlFloatArrayMax(magOrFluxErrorArray).ToArray();

            List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeFluxList =
                new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(magsOrFluxes.Zip(magOrFluxErrors, (a, b) => ((bool)inputInMags)
                    ? (ValueWithErrorConvolveableFromFilterAndSpectrum)new Magnitude() { Value = a, Error = b, MagSystem = MagnitudeSystem.Type.AB }
                    : (ValueWithErrorConvolveableFromFilterAndSpectrum)new Flux() { Value = a, Error = b }));


            List<string> filterURLs = new List<string>(((string)filterURLList).Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

            int fitError;
            Spectrum result = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.CalculatePhotoZBestChiSquare( magnitudeFluxList,
                                                                                                    filterURLs,
                                                                                                    (double)extinctionMapValue,
                                                                                                    (bool)fitInFluxSpace,
                                                                                                    (double)errorSmoothening,
                                                                                                    false,
                                                                                                    out fitError);

            if (fitError==0)
            {
                return (SqlDouble)result.Redshift;
            }
            else
            {
                return (SqlDouble)fitError;
            }
        }
        catch
        {
            return (SqlDouble)(-9999);
        }
    }


    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = false,
                                            IsDeterministic = false,
                                            Name = "Compute.PhotoZMinChiSqr_ID")]
    public static SqlDouble CalculatePhotoZMinChiSQR_ID(SqlBytes magOrFluxArray,
                                                        SqlBytes magOrFluxErrorArray,
                                                        SqlBoolean inputInMags,
                                                        SqlBytes filterIDList,
                                                        SqlDouble extinctionMapValue,
                                                        SqlBoolean fitInFluxSpace,
                                                        SqlDouble errorSmoothening)
    {
        try
        {

            double[] magsOrFluxes = new SqlFloatArrayMax(magOrFluxArray).ToArray();
            double[] magOrFluxErrors = new SqlFloatArrayMax(magOrFluxErrorArray).ToArray();


            List<ValueWithErrorConvolveableFromFilterAndSpectrum> magnitudeFluxList =
                new List<ValueWithErrorConvolveableFromFilterAndSpectrum>(magsOrFluxes.Zip(magOrFluxErrors, (a, b) => ((bool)inputInMags)
                    ? (ValueWithErrorConvolveableFromFilterAndSpectrum)new Magnitude() { Value = a, Error = b, MagSystem = MagnitudeSystem.Type.AB }
                    : (ValueWithErrorConvolveableFromFilterAndSpectrum)new Flux() { Value = a, Error = b }));



            List<int> filterIDs = new List<int>(new SqlIntArrayMax(filterIDList).ToArray());
            List<string> filterURLList = new List<string>(filterIDs.Select(x => "http://voservices.net/filter/filterascii.aspx?FilterID=" + x.ToString()));


            int fitError;
            Spectrum result = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.CalculatePhotoZBestChiSquare( magnitudeFluxList,
                                                                                                    filterURLList,
                                                                                                    (double)extinctionMapValue,
                                                                                                    (bool)fitInFluxSpace,
                                                                                                    (double)errorSmoothening,
                                                                                                    false,
                                                                                                    out fitError);

            if (fitError==0)
            {
                return (SqlDouble)result.Redshift;
            }
            else
            {
                return (SqlDouble)fitError;
            }

        }
        catch
        {
            return (SqlDouble)(-9999);
        }
    }

            /*double maximumProb = 0.0;
            double maximumProbZ = -9999;
            double redshiftResult = -9999;
            List<Magnitude> synthMags = new List<Magnitude>(aMagnitudeList.Count);

            if (!fitError)
            {
                redshiftResult = result.Redshift;

                for (int i = 0; i < aMagnitudeList.Count; ++i)
                {
                    synthMags.Add(new Magnitude() { MagSystem = aMagnitudeList[i].MagSystem, ABZeroPoint = aMagnitudeList[i].ABZeroPoint });
                    synthMags[i].ConvolveMagnitudeFromFilterAndSpectrum(result, filterList[i]);
                }

                for (int i = 0; i < redshifts.Count; ++i)
                {
                    if (redshiftProbabilities[i] > maximumProb)
                    {
                        maximumProb = redshiftProbabilities[i];
                        maximumProbZ = redshifts[i];
                    }
                }
            }*/

    // [SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = 4000)] SqlString filterURLList
    //System.Security.Principal.WindowsIdentity currentIdentity = SqlContext.WindowsIdentity;
    //System.Security.Principal.WindowsImpersonationContext impersonatedIdentity = currentIdentity.Impersonate();
    //impersonatedIdentity.Undo();

}

