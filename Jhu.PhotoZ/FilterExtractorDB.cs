using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Jhu.PhotoZ
{
    //This class reads in filters from the SpectrumPortal3.1 database
    public class FilterExtractorDB
    {
        private string connectionString;
        private Dictionary<string, int> nameToFilterID;

        public FilterExtractorDB(string aConnectionString)
        {
            connectionString=aConnectionString;
            nameToFilterID = new Dictionary<string, int>(100);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                //An exception will be thrown here if the connection cannot be created

                conn.Open();

                string sqlString = @"SELECT ID, Name
                                    FROM Filters";

                char[] trimChars = new char[] { ' ' };

                using (SqlCommand cmd = new SqlCommand(sqlString, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            nameToFilterID[((string)reader["Name"]).TrimEnd(trimChars)] = (int)reader["ID"];
                        }

                    }
                }

                conn.Close();

            }

        }


        public Filter ExtractFilterFromDB(string filterName, out bool error)
        {

            if (nameToFilterID.ContainsKey(filterName))
            {
                return ExtractFilterFromDB(nameToFilterID[filterName], out error);
            }
            else 
            {
                error=true;
                return null;
            }

        }


        public Filter ExtractFilterFromDB(int filterID, out bool error)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                try
                {
                    conn.Open();

                    string sizeCommand = @" SELECT COUNT(*) AS cnt
                                            FROM FilterResponses
                                            WHERE FilterID=@ID";

                    int length = 0;

                    using (SqlCommand cmd = new SqlCommand(sizeCommand, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", filterID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                length = (int)reader["cnt"];
                            }

                        }
                    }

                    if (length > 0)
                    {

                        double[] wavelengths = new double[length];
                        double[] responses = new double[length];

                        string readCommand = @" SELECT Wavelength, Value
                                            FROM FilterResponses
                                            WHERE FilterID=@ID
                                            ORDER BY Wavelength";

                        int i = 0;
                        using (SqlCommand cmd = new SqlCommand(readCommand, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", filterID);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    wavelengths[i] = (double)reader["Wavelength"];
                                    responses[i] = (double)reader["Value"];
                                    i++;
                                }
                            }
                        }

                        readCommand = @" SELECT WavelengthEff
                                        FROM Filters
                                        WHERE ID=@ID";

                        double lambdaEff = Constants.missingDouble;

                        using (SqlCommand cmd = new SqlCommand(readCommand, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", filterID);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    lambdaEff = (double)reader["WavelengthEff"];
                                }
                            }
                        }


                        conn.Close();

                        error = false;
                        return new Filter(wavelengths, responses, lambdaEff);

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


    }
}
