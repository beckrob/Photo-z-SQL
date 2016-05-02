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

public partial class UserDefinedFunctions
{


    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false)]
    public static SqlInt32 PhotoZInitializeForSDSS(SqlDouble redshiftFrom,
                                                SqlDouble redshiftTo,
                                                SqlDouble redshiftStep,
                                                [SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = 4000)] SqlString templateDBConnectionString,
                                                [SqlFacet(IsFixedLength = false, IsNullable = true, MaxSize = 4000)] SqlString filterDBConnectionString)
    {
        System.Security.Principal.WindowsIdentity currentIdentity = SqlContext.WindowsIdentity;
        System.Security.Principal.WindowsImpersonationContext impersonatedIdentity = currentIdentity.Impersonate();

        int retCode = 0;
        try
        {
            Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.InitializeForSDSSFromDB((double)redshiftFrom,
                                                                            (double)redshiftTo,
                                                                            (double)redshiftStep,
                                                                            (string)templateDBConnectionString,
                                                                            (string)filterDBConnectionString);
        }
        catch
        {
            retCode = -9999;
        }
        finally
        {
            impersonatedIdentity.Undo();
        }

        return retCode;
    }


    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.None,
                                            SystemDataAccess = SystemDataAccessKind.None,
                                            IsPrecise = true,
                                            IsDeterministic = false)]
    public static SqlInt32 PhotoZRemoveInitialization()
    {
        Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.RemoveInitialization();
        return 0;
    }


    [Microsoft.SqlServer.Server.SqlFunction(DataAccess=DataAccessKind.None,
                                            SystemDataAccess=SystemDataAccessKind.None,
                                            IsPrecise=false,
                                            IsDeterministic=false)]
    public static SqlDouble CalculatePhotoZForSDSS( SqlDouble uMag, 
                                                    SqlDouble gMag, 
                                                    SqlDouble rMag, 
                                                    SqlDouble iMag, 
                                                    SqlDouble zMag, 
                                                    SqlDouble uMagError,
                                                    SqlDouble gMagError,
                                                    SqlDouble rMagError,
                                                    SqlDouble iMagError,
                                                    SqlDouble zMagError,
                                                    SqlDouble extinctionMapValue,
                                                    SqlInt32 camCol)
    {

        List <Jhu.PhotoZ.Magnitude> magList= new List <Jhu.PhotoZ.Magnitude>(5);
        magList.Add(new Jhu.PhotoZ.Magnitude() { Value = (double)uMag, Error = (double)uMagError, MagSystem = Jhu.PhotoZ.MagnitudeSystem.Type.SDSS_u });
        magList.Add(new Jhu.PhotoZ.Magnitude() { Value = (double)gMag, Error = (double)gMagError, MagSystem = Jhu.PhotoZ.MagnitudeSystem.Type.SDSS_g });
        magList.Add(new Jhu.PhotoZ.Magnitude() { Value = (double)rMag, Error = (double)rMagError, MagSystem = Jhu.PhotoZ.MagnitudeSystem.Type.SDSS_r });
        magList.Add(new Jhu.PhotoZ.Magnitude() { Value = (double)iMag, Error = (double)iMagError, MagSystem = Jhu.PhotoZ.MagnitudeSystem.Type.SDSS_i });
        magList.Add(new Jhu.PhotoZ.Magnitude() { Value = (double)zMag, Error = (double)zMagError, MagSystem = Jhu.PhotoZ.MagnitudeSystem.Type.SDSS_z });

        if (camCol < 1 || camCol > 6)
        {
            camCol = 0;
        }
        List<int> filterIDList = new List<int>(Enumerable.Range(0 + ((int)camCol)*5, 5));


        List<double> redshifts, redshiftProbabilities;
        bool fitError;
        Jhu.PhotoZ.Spectrum result = Jhu.PhotoZSQL.PhotoZSQLWrapper.Instance.CalculatePhotoZ(   magList, 
                                                                                                filterIDList,
                                                                                                (double) extinctionMapValue,
                                                                                                out redshifts,
                                                                                                out redshiftProbabilities,
                                                                                                out fitError);

        double maximumProb = 0.0;
        double maximumProbZ = -9999;

        if (!fitError)
        {
            for (int i = 0; i < redshifts.Count; ++i)
            {
                if (redshiftProbabilities[i] > maximumProb)
                {
                    maximumProb = redshiftProbabilities[i];
                    maximumProbZ = redshifts[i];
                }
            }
        }

        return maximumProbZ;
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


}

