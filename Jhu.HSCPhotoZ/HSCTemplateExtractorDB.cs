using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Jhu.SqlServer.Array;

namespace Jhu.HSCPhotoZ
{
    //This class reads and writes templates from/to the HSCFilterPortal database
    public class HSCTemplateExtractorDB
    {

        private string connectionString;

        public HSCTemplateExtractorDB(string aConnectionString)
        {
            connectionString = aConnectionString;
        }


        public Jhu.PhotoZ.Spectrum ExtractTemplateFromDB(int templateID, int templateSetID, out bool error, bool addPaddingInIR = false)
        {

            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                try
                {
                    double redshift = 0.0;
                    double luminosity = 0.0;
                    double[] wavelengths = new double[0];
                    double[] fluxes = new double[0];


                    conn.Open();

                    string readCommand;
                    if (templateSetID==2)
                    {
                        readCommand = @" SELECT Redshift, Luminosity, Wavelengths, Fluxes
                                        FROM TemplatesLePhare
                                        WHERE TemplateID=@ID";
                    }
                    else if (templateSetID == 1)
                    {
                        readCommand = @" SELECT Redshift, Luminosity, Wavelengths, Fluxes
                                        FROM TemplatesLogInterp
                                        WHERE TemplateID=@ID";
                    }
                    else
                    {
                        readCommand = @" SELECT Redshift, Luminosity, Wavelengths, Fluxes
                                        FROM Templates
                                        WHERE TemplateID=@ID";
                    }

                    using (SqlCommand cmd = new SqlCommand(readCommand, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", templateID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                redshift = (double)reader["Redshift"];
                                luminosity = (double)reader["Luminosity"];
                                wavelengths = new SqlFloatArrayMax(reader.GetSqlBytes(2)).ToArray();
                                fluxes = new SqlFloatArrayMax(reader.GetSqlBytes(3)).ToArray();
                            }
                        }
                    }

                    conn.Close();

                    if (wavelengths.Length > 0 && wavelengths.Length == fluxes.Length)
                    {
                        error = false;
                        return new Jhu.PhotoZ.Spectrum(wavelengths, fluxes, redshift, luminosity, addPaddingInIR);
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


        public bool WriteTemplateToDB(int templateID, int templateSetID, Jhu.PhotoZ.Spectrum template)
        {

            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                try
                {
                    conn.Open();

                    string writeCommand;
                    if (templateSetID == 2)
                    {
                        writeCommand = @" INSERT INTO TemplatesLePhare (TemplateID, Redshift, Luminosity, Wavelengths, Fluxes)
                                        VALUES (@ID, @Redshift, @Luminosity, @Wavelengths, @Fluxes)";
                    }
                    else if (templateSetID == 1)
                    {
                        writeCommand = @" INSERT INTO TemplatesLogInterp (TemplateID, Redshift, Luminosity, Wavelengths, Fluxes)
                                        VALUES (@ID, @Redshift, @Luminosity, @Wavelengths, @Fluxes)";
                    }
                    else
                    {
                        writeCommand = @" INSERT INTO Templates (TemplateID, Redshift, Luminosity, Wavelengths, Fluxes)
                                        VALUES (@ID, @Redshift, @Luminosity, @Wavelengths, @Fluxes)";
                    }



                    int rowsAffected = 0;
                    using (SqlCommand cmd = new SqlCommand(writeCommand, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", templateID);
                        cmd.Parameters.AddWithValue("@Redshift", template.Redshift);
                        cmd.Parameters.AddWithValue("@Luminosity", template.Luminosity);
                        cmd.Parameters.AddWithValue("@Wavelengths", SqlFloatArrayMax.FromArray(template.GetBinCenters()).ToSqlBuffer());
                        cmd.Parameters.AddWithValue("@Fluxes", SqlFloatArrayMax.FromArray(template.GetFluxes()).ToSqlBuffer());

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


    }
}
