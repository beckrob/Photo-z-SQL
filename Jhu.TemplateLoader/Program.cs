using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Jhu.PhotoZ;
using Jhu.HSCPhotoZ;
using Jhu.SqlServer.Array;

namespace Jhu.TemplateLoader
{
    class Program
    {
        private const string aConnectionStringTemplates = @"data source=future1;initial catalog=beckrob23;multipleactiveresultsets=true;Integrated Security=true";

        private static void WriteSpectraFile(Spectrum spec, StreamWriter outputFile, int globalID, int setID, string setString)
        {
            string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            outputFile.Write(   globalID.ToString() + "\t" +
                                "00000000-0000-0000-0000-000000000000" + "\t" +
                                "0" + "\t" +
                                "1" + "\t" +
                                "ivo://" + setString + "/templates#" + setID.ToString() + "\t" +
                                setString + "_template_" + setID.ToString() + "\t" +
                                "GALAXY" + "\t" +
                                "GALAXY" + "\t" +
                                "ARTIFICAL" + "\t" +
                                dateString + "\t" +
                                "03\t0\t0\t0\t-1\t0\t0\t1\t0\t0\t0\tA\t0\t10e-17 erg s-1 cm-2 A-1\tCALIBRATED\r\n");

        }

        private static void WriteSpectrumDataFile(Spectrum spec, StreamWriter outputFile, int globalID)
        {
            outputFile.Write(   "Flux_Value" + "\t" +
                                globalID.ToString() + "\t" +
                                "1" + "\t");
            /*
            outputFile.Flush();
            using (BinaryWriter bwriter = new BinaryWriter(outputFile.BaseStream, outputFile.Encoding, true)){
                SqlFloatArrayMax.FromArray(spec.GetFluxes()).Write(bwriter);
            }*/
            foreach (double value in spec.GetFluxes())
            {
                outputFile.Write(BitConverter.ToString(BitConverter.GetBytes(value)).Replace("-", ""));
            }

            outputFile.Write("\r\n");
            
            outputFile.Write(   "Spectral_Value" + "\t" +
                                globalID.ToString() + "\t" +
                                "1" + "\t");

            /*
            outputFile.Flush();
            using (BinaryWriter bwriter = new BinaryWriter(outputFile.BaseStream, outputFile.Encoding, true)){
                SqlFloatArrayMax.FromArray(spec.GetBinCenters()).Write(bwriter);
            }*/
            foreach (double value in spec.GetBinCenters())
            {
                outputFile.Write(BitConverter.ToString(BitConverter.GetBytes(value)).Replace("-", ""));
            }

            outputFile.Write("\r\n");
        }

        private static void WriteSpectrumFieldsFile(Spectrum spec, StreamWriter outputFile, int globalID, int setID, string setString)
        {
            string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            outputFile.Write("Spectrum.Curation.Contact.ContactEmail\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tbeckrob23@caesar.elte.hu\t\t\t\t\t\t\tmeta.ref.url;meta.email\t\r\n");

            outputFile.Write("Spectrum.Curation.Contact.ContactName\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tRobert Beck\t\t\t\t\t\t\tmeta.bib.author;meta.curation\t\r\n");

            outputFile.Write("Spectrum.Curation.Date\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t3\t\t\t\t\t");
            outputFile.Write(dateString);
            outputFile.Write("\t\t\t\t\r\n");

            outputFile.Write("Spectrum.Curation.Publisher\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tELTE VO\t\t\t\t\t\t\tmeta.curation\t\r\n");

            outputFile.Write("Spectrum.Curation.PublisherDID\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\t#");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t\t\t\t\t\t\tmeta.ref.url;meta.curation\t\r\n");

            outputFile.Write("Spectrum.Curation.Rights\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tPUBLIC\t\t\t\t\t\t\t\t\r\n");

            outputFile.Write("Spectrum.Curation.Version\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\t3\t\t\t\t\t\t\tmeta.version;meta.curation\t\r\n");

            outputFile.Write("Spectrum.Data.FluxAxis.Calibration\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tCALIBRATED\t\t\t\t\t\t\tmeta.code.qual\t\r\n");

            outputFile.Write("Spectrum.Data.FluxAxis.Ucd\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tphot.flux.density;em.wl\t\t\t\t\t\t\t\t\r\n");

            outputFile.Write("Spectrum.Data.FluxAxis.Unit\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\t10e-17 erg s-1 cm-2 A-1\t\t\t\t\t\t\t\t\r\n");

            outputFile.Write("Spectrum.Data.FluxAxis.Value\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t0\t\t\t\t10e-17 erg s-1 cm-2 A-1\tphot.flux.density;em.wl\t\r\n");

            outputFile.Write("Spectrum.Data.SpectralAxis.Coverage.Bounds.Start\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t0\t\t\t\tA\tem.wl;stat.min\t\r\n");

            outputFile.Write("Spectrum.Data.SpectralAxis.Coverage.Bounds.Stop\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t0\t\t\t\tA\tem.wl;stat.max\t\r\n");

            outputFile.Write("Spectrum.Data.SpectralAxis.ResPower\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t0\t\t\t\t\tspect.resolution\t\r\n");

            outputFile.Write("Spectrum.Data.SpectralAxis.Value\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t0\t\t\t\tA\tem.wl\t\r\n");

            outputFile.Write("Spectrum.DataId.Collection\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\t");
            outputFile.Write(setString + " Templates");
            outputFile.Write("\t\t\t\t\t\t\t\t\r\n");

            outputFile.Write("Spectrum.DataId.CreationType\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tARTIFICAL\t\t\t\t\t\t\t\t\r\n");

            outputFile.Write("Spectrum.DataId.CreatorDID\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tivo://");
            outputFile.Write(setString);
            outputFile.Write("/templates#");
            outputFile.Write(setID);
            outputFile.Write("\t\t\t\t\t\t\tmeta.id\t\r\n");

            outputFile.Write("Spectrum.DataId.DatasetId\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tivo://");
            outputFile.Write(setString);
            outputFile.Write("/templates\t\t\t\t\t\t\tmeta.id;meta.dataset\t\r\n");

            outputFile.Write("Spectrum.DataId.DataSource\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tARTIFICAL\t\t\t\t\t\t\t\t\r\n");

            outputFile.Write("Spectrum.DataId.Date\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t3\t\t\t\t\t");
            outputFile.Write(dateString);
            outputFile.Write("\t\t\ttime;meta.dataset\t\r\n");

            outputFile.Write("Spectrum.DataId.Title\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\t");
            outputFile.Write(setString + " Templates");
            outputFile.Write("\t\t\t\t\t\t\tmeta.title;meta.dataset\t\r\n");

            outputFile.Write("Spectrum.DataId.Version\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\t3\t\t\t\t\t\t\tmeta.version;meta.dataset\t\r\n");

            outputFile.Write("Spectrum.DataModel\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tSPECTRUM-1.0\t\t\t\t\t\t\t\t\r\n");

            outputFile.Write("Spectrum.Derived.Redshift.Confidence\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t1\t\t\t\t\t\t\r\n");

            outputFile.Write("Spectrum.Derived.Redshift.StatError\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t0\t\t\t\t\tstat.error;src.redshift\t\r\n");

            outputFile.Write("Spectrum.Derived.Redshift.Value\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t0\t\t\t\t\t\t\r\n");

            outputFile.Write("Spectrum.Derived.SNR\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t-1\t\t\t\t\tstat.snr\t\r\n");

            outputFile.Write("Spectrum.Derived.VarAmpl\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t0\t\t\t\t\tsrc.var.amplitude;arith.ratio\t\r\n");

            outputFile.Write("Spectrum.FluxSI\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\t10e-17 erg s-1 cm-2 A-1\t\t\t\t\t\t\t\t\r\n");

            outputFile.Write("Spectrum.Length\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t1\t\t");
            outputFile.Write(spec.GetBinCenters().Length.ToString());
            outputFile.Write("\t\t\t\t\t\tmeta.number\t\r\n");

            outputFile.Write("Spectrum.SpectralSI\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tA\t\t\t\t\t\t\t\t\r\n");

            outputFile.Write("Spectrum.Target.Class\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tGALAXY\t\t\t\t\t\t\tsrc.class\t\r\n");

            outputFile.Write("Spectrum.Target.Description\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\t");
            outputFile.Write(setString + "_template_" + setID.ToString());
            outputFile.Write("\t\t\t\t\t\t\tmeta.note;src\t\r\n");

            outputFile.Write("Spectrum.Target.Name\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\t");
            outputFile.Write(setString + "_template_" + setID.ToString());
            outputFile.Write("\t\t\t\t\t\t\tmeta.id;src\t\r\n");

            outputFile.Write("Spectrum.Target.Pos\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t4\t\t\t0\t0\t\t\t\tpos.eq;src\t\r\n");

            outputFile.Write("Spectrum.Target.Redshift\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t0\t\t\t\t\tsrc.redshift\t\r\n");

            outputFile.Write("Spectrum.Target.SpectralClass\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tGALAXY\t\t\t\t\t\t\tsrc.spType\t\r\n");

            outputFile.Write("Spectrum.Target.VarAmpl\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t2\t\t\t0\t\t\t\t\tsrc.var.amplitude\t\r\n");

            outputFile.Write("Spectrum.Type\t");
            outputFile.Write(globalID.ToString());
            outputFile.Write("\t0\tSPECTRUM\t\t\t\t\t\t\t\t\r\n");

        }

        private static void WriteTemplateSetsFile(StreamWriter outputFile, string setString, int setLineNum)
        {
            outputFile.Write(setLineNum.ToString() + "\t" + setString + " Templates\r\n");
        }

        private static void WriteTemplatesFile(StreamWriter outputFile, int setID, string setString, int setLineNum)
        {
            outputFile.Write(setLineNum.ToString() + "\t" + "ivo://" + setString + "/templates#" + setID.ToString() + "\r\n");
        }


        static void Main(string[] args)
        {
            /*
            byte[] floatVals = BitConverter.GetBytes(1234.1234);
            double f = BitConverter.ToDouble(floatVals, 0);
            Console.WriteLine("double convert = {0}", f);
            Console.WriteLine(BitConverter.ToString(floatVals));

            string hexString = "0000000000C05640";
                               
            UInt64 num = UInt64.Parse(hexString, System.Globalization.NumberStyles.AllowHexSpecifier);
            Console.WriteLine("uint64 = {0}", num);
            byte[] floatVals2 = BitConverter.GetBytes(num);
            Array.Reverse(floatVals2);

            double f2 = BitConverter.ToDouble(floatVals2, 0);
            Console.WriteLine("double convert = {0}", f2);
            Console.WriteLine(BitConverter.ToString(BitConverter.GetBytes(f2)).Replace("-", ""));
            */

            int templateIDStart = 440;

            using (StreamWriter outputFile1 = new StreamWriter("Spectra.txt"))
            using (StreamWriter outputFile2 = new StreamWriter("SpectrumData.txt"))
            using (StreamWriter outputFile3 = new StreamWriter("SpectrumFields.txt"))
            using (StreamWriter outputFile4 = new StreamWriter("Templates.txt"))
            using (StreamWriter outputFile5 = new StreamWriter("TemplateSets.txt"))
            {

                HSCTemplateExtractorDB templateExtractor = new HSCTemplateExtractorDB(aConnectionStringTemplates);

                WriteTemplateSetsFile(outputFile5,"BPZ",5);
                WriteTemplateSetsFile(outputFile5,"LePhare",6);

                for (int i = 0; i < 71; ++i)
                {
                    bool error;
                    Spectrum spec=templateExtractor.ExtractTemplateFromDB(i, 0, out error);

                    if (!error)
                    {
                        WriteSpectraFile(spec, outputFile1, templateIDStart, i, "BPZ");
                        WriteSpectrumDataFile(spec, outputFile2, templateIDStart);
                        WriteSpectrumFieldsFile(spec, outputFile3, templateIDStart++, i, "BPZ");
                        WriteTemplatesFile(outputFile4, i, "BPZ", 5);
                    }

                }
                for (int i = 0; i < 641; ++i)
                {
                    bool error;
                    Spectrum spec = templateExtractor.ExtractTemplateFromDB(i, 2, out error);

                    if (!error)
                    {
                        WriteSpectraFile(spec, outputFile1, templateIDStart, i, "LePhare");
                        WriteSpectrumDataFile(spec, outputFile2, templateIDStart);
                        WriteSpectrumFieldsFile(spec, outputFile3, templateIDStart++, i, "LePhare");
                        WriteTemplatesFile(outputFile4, i, "LePhare", 6);
                    }
                }

            }

        }
    }
}
