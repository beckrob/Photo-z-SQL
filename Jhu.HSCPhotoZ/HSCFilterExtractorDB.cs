using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Jhu.SqlServer.Array;

namespace Jhu.HSCPhotoZ
{
    //This class reads and writes filters from/to the HSCFilterPortal database
    public class HSCFilterExtractorDB
    {

        private string connectionString;
        private int MJDStepSize;

        private Dictionary<HSCFilterSpecifier, int> specifierToFilterID;
        private Dictionary<int, int> filterIDMJDStart;
        private Dictionary<int, int> filterIDMJDEnd;

        public HSCFilterExtractorDB(string aConnectionString, int aMJDStepSize = 30)
        {
            connectionString=aConnectionString;
            MJDStepSize = aMJDStepSize;
            specifierToFilterID = new Dictionary<HSCFilterSpecifier, int>(640);
            filterIDMJDStart = new Dictionary<int, int>(300);
            filterIDMJDEnd = new Dictionary<int, int>(300);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                //An exception will be thrown here if the connection cannot be created

                conn.Open();

                string IDString = @"SELECT *
                                    FROM FilterIDLookup";

                char[] trimChars = new char[]{' '};

                using (SqlCommand cmd = new SqlCommand(IDString, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            specifierToFilterID[new HSCFilterSpecifier(  ((string) reader["Instrument"]).TrimEnd(trimChars),
                                                                         ((string) reader["Detector"]).TrimEnd(trimChars),
                                                                         ((string) reader["Aperture"]).TrimEnd(trimChars),
                                                                         ((string) reader["Filter"]).TrimEnd(trimChars) )] = (int) reader["FilterID"];
                        }
                    }
                }

                string MJDString = @"SELECT FilterID, MJDStart, MJDEnd
                                    FROM FilterParameters";


                using (SqlCommand cmd = new SqlCommand(MJDString, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            filterIDMJDStart[(int)reader["FilterID"]] = (int)reader["MJDStart"];
                            filterIDMJDEnd[(int)reader["FilterID"]] = (int)reader["MJDEnd"];
                        }
                    }
                }

                conn.Close();

            }

        }

        public bool GetFilterIDFromSpecifier(string instrument, string detector, string aperture, string filter, out int filterID)
        {
            HSCFilterSpecifier specifier = new HSCFilterSpecifier(instrument, detector, aperture, filter);

            if (specifierToFilterID.ContainsKey(specifier))
            {
                filterID = specifierToFilterID[specifier];
                return true;
            }
            else
            {
                filterID = Jhu.PhotoZ.Constants.missingInt;
                return false;
            }
        }


        public Jhu.PhotoZ.Filter ExtractFilterFromDB(string instrument, string detector, string aperture, string filter, double MJD, out bool error)
        {
            int filterID;
            if (GetFilterIDFromSpecifier(instrument, detector, aperture, filter, out filterID))
            {
                return ExtractFilterFromDB(filterID, MJD, out error);
            }
            else 
            {
                error=true;
                return null;
            }

        }


        public Jhu.PhotoZ.Filter ExtractFilterFromDB(int filterID, double MJD, out bool error)
        {
            MJD = 1e23;

            int closestMJD;
            if (GetClosestMJD(filterID, MJD, out closestMJD))
            {            

                using (SqlConnection conn = new SqlConnection(connectionString))
                {

                    try
                    {
                        double[] wavelengths = new double[0];
                        double[] responses = new double[0];

                        conn.Open();

                        string readCommand = @" SELECT Wavelengths, Responses
                                            FROM FilterResponsesForMJD
                                            WHERE FilterID=@ID AND MJD=@ClosestMJD";


                        using (SqlCommand cmd = new SqlCommand(readCommand, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", filterID);
                            cmd.Parameters.AddWithValue("@ClosestMJD", closestMJD);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    wavelengths = new SqlFloatArrayMax(reader.GetSqlBytes(0)).ToArray();
                                    responses = new SqlFloatArrayMax(reader.GetSqlBytes(1)).ToArray();
                                }
                            }
                        }

                        readCommand =   @" SELECT UnitResponseInFlam, PivotWavelength
                                        FROM FilterParametersForMJD
                                        WHERE FilterID=@ID AND MJD=@ClosestMJD";

                        double photFLam = Jhu.PhotoZ.Constants.missingDouble;
                        double photPLam = Jhu.PhotoZ.Constants.missingDouble; //Pivot wavelength

                        using (SqlCommand cmd = new SqlCommand(readCommand, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", filterID);
                            cmd.Parameters.AddWithValue("@ClosestMJD", closestMJD);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    photFLam = (double)reader["UnitResponseInFlam"];
                                    photPLam = (double)reader["PivotWavelength"];
                                }
                            }
                        }

                        /*
                        double ABZeroPoint = -48.6;
                        if (photFLam != Jhu.PhotoZ.Constants.missingDouble && photPLam != Jhu.PhotoZ.Constants.missingDouble)
                        {
                            //For HSC filters
                            //ABMAG_ZEROPOINT = -2.5 Log (PHOTFLAM) - 21.10 - 5 Log (PHOTPLAM) + 18.6921
                            //http://www.stsci.edu/hst/wfc3/phot_zp_lbn
                            //This correction is already done when HSC magnitudes are computed - not time-dependently, though


                            ABZeroPoint = -2.5 * Math.Log10(photFLam) - 21.10 - 5 * Math.Log10(photPLam) + 18.6921;
                        }
                        */

                        //Using aperture correction of point sources to infinite aperture as a zero point correction between instruments
                        //TODO hardcoded 0.15 arcsec aperture for now
                        readCommand = @"SELECT fAp.ApertureCorrectionToInfinite_mags
                                        FROM FilterApertureCorrection fAp
                                        INNER JOIN FilterIDLookup AS fID
                                            ON fID.Detector=fAp.Detector AND fID.Filter=fAp.Filter AND fID.FilterID=@ID AND fAp.Aperture_arcsec=0.15";

                        double apertureCorrection = 0.0;
                        using (SqlCommand cmd = new SqlCommand(readCommand, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", filterID);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    apertureCorrection = (double)reader["ApertureCorrectionToInfinite_mags"];
                                }
                            }
                        }

                        conn.Close();

                        if (wavelengths.Length > 0 && wavelengths.Length == responses.Length)
                        {
                            error = false;
                            return new Jhu.PhotoZ.Filter(wavelengths, responses, photPLam, apertureCorrection, false);
                        }
                        else
                        {
                            error = true;
                            return null;
                        }
                    }
                    catch
                    {
                        error = true;
                        return null;
                    }

                }
            }
            else
            {
                error = true;
                return null;
            }

        }

        public bool WriteFilterToDB(string instrument, string detector, string aperture, string filter, double MJD, Jhu.PhotoZ.Filter filterCurve)
        {
            int filterID;
            if (GetFilterIDFromSpecifier(instrument, detector, aperture, filter, out filterID))
            {
                return WriteFilterToDB(filterID, MJD, filterCurve);
            }
            else
            {
                return false;
            }
        }


        public bool WriteFilterToDB(int filterID, double MJD, Jhu.PhotoZ.Filter filterCurve)
        {
            
            int closestMJD;
            //Writing is only allowed to places from where ExtractFilterFromDB can extract
            //So the logic of MJDs present from start to end with a stepsize of MJDStepSize has to be intact
            //GetClosestMJD() and GetRealMJDEnd() have to be modified if this changes
            if (GetClosestMJD(filterID, MJD, out closestMJD))
            {

                using (SqlConnection conn = new SqlConnection(connectionString))
                {

                    try
                    {
                        conn.Open();

                        string writeCommand = @" INSERT INTO FilterResponsesForMJD (FilterID, MJD, Wavelengths, Responses)
                                            VALUES (@ID, @ClosestMJD, @Wavelengths, @Responses)";


                        int rowsAffected = 0;
                        using (SqlCommand cmd = new SqlCommand(writeCommand, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", filterID);
                            cmd.Parameters.AddWithValue("@ClosestMJD", closestMJD);
                            cmd.Parameters.AddWithValue("@Wavelengths", SqlFloatArrayMax.FromArray(filterCurve.GetBinCenters()).ToSqlBuffer());
                            cmd.Parameters.AddWithValue("@Responses", SqlFloatArrayMax.FromArray(filterCurve.GetResponses()).ToSqlBuffer());

                            rowsAffected = cmd.ExecuteNonQuery();
                        }

                        conn.Close();

                        return (rowsAffected > 0);

                    }
                    catch
                    {
                        return false;
                    }

                }
            }
            else
            {
                return false;
            }

        }

        private bool GetClosestMJD(int filterID, double MJD, out int closestMJD)
        {

            int realMJDEnd;
            if (filterIDMJDStart.ContainsKey(filterID) && GetRealMJDEnd(filterID, out realMJDEnd))
            {

                if (MJD <= filterIDMJDStart[filterID])
                {
                    closestMJD = filterIDMJDStart[filterID];
                }
                else if (MJD >= realMJDEnd)
                {
                    closestMJD = realMJDEnd;
                }
                else
                {
                    closestMJD = filterIDMJDStart[filterID] + ((int)Math.Round((MJD - filterIDMJDStart[filterID]) / MJDStepSize)) * MJDStepSize;
                }

                return true;
            }
            else
            {
                closestMJD = Jhu.PhotoZ.Constants.missingInt;
                return false;
            }
        }

        private bool GetRealMJDEnd(int filterID, out int realMJDEnd)
        {
            if (filterIDMJDStart.ContainsKey(filterID) && filterIDMJDEnd.ContainsKey(filterID))
            {
                realMJDEnd = filterIDMJDStart[filterID] + ((filterIDMJDEnd[filterID] - filterIDMJDStart[filterID]) / MJDStepSize) * MJDStepSize;
                return true;
            }
            else
            {
                realMJDEnd = Jhu.PhotoZ.Constants.missingInt;
                return false;
            }
        }


    }
}
