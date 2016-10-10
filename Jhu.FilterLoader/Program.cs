using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Jhu.PhotoZ;

namespace Jhu.FilterLoader
{
    class Program
    {

        private static readonly string[] aFilterPaths = new string[14] 
        {
            "filters\\CANDELS_GS_filter01_Uband.txt",
            "filters\\CANDELS_GS_filter02_f435w.txt",
            "filters\\CANDELS_GS_filter03_f606w.txt",
            "filters\\CANDELS_GS_filter04_f775w.txt",
            "filters\\CANDELS_GS_filter05_f850lp.txt",
            "filters\\CANDELS_GS_filter06_f098m.txt",
            "filters\\CANDELS_GS_filter07_f105w.txt",
            "filters\\CANDELS_GS_filter08_f125w.txt",
            "filters\\CANDELS_GS_filter09_f160w.txt",
            "filters\\CANDELS_GS_filter10_Ks.txt",
            "filters\\CANDELS_GS_filter11_IRACch1.txt",
            "filters\\CANDELS_GS_filter12_IRACch2.txt",
            "filters\\CANDELS_GS_filter13_IRACch3.txt",
            "filters\\CANDELS_GS_filter14_IRACch4.txt"
        };

        private static readonly string[] aFilterTags = new string[14] 
        {
            "U", "F435W", "F606W", "F775W", "F850LP", "F098M", "F105W", "F125W", "F160W", "Ks", "ch1", "ch2", "ch3", "ch4"
        };

        private static readonly double[] aFilterEffWavelengths = new double[14] 
        {
            3552, 4327.3, 5921.8, 7693.7, 9033.6, 9864.1, 10552, 12486, 15369, 21612, 35500, 44930, 57310, 78720
        };


        private static readonly string[] aFilterPaths2 = new string[18] 
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
            "filters\\K_flaming_resam.res",
            "filters\\irac_ch1.res",
            "filters\\irac_ch2.res",
            "filters\\irac_ch3.res",
            "filters\\irac_ch4.res"
        };

        private static readonly string[] aFilterTags2 = new string[18] 
        {
            "U", "B", "V", "R", "I", "Z", "F435W", "F606W", "F775W", "F850LP", "J", "H", "HK", "K", "ch1", "ch2", "ch3", "ch4"
        };

        private static readonly double[] aFilterEffWavelengths2 = new double[18] 
        {
            3552, 4478, 5493, 6550, 7996, 9054, 4327.3, 5921.8, 7693.7, 9033.6, 12483, 16313, 18947.38, 21900, 35500, 44930, 57310, 78720
        };



        static void Main(string[] args)
        {
            int filtersIDStart = 130; //112;
            int filterResponsesIDStart = 408561; //402156;

            using (StreamWriter outputFile1 = new StreamWriter("filters.txt"))
            using (StreamWriter outputFile2 = new StreamWriter("filterResponses.txt"))
            {
                for (int i = 0; i < aFilterPaths.Length; ++i)
                {

                    Filter tmpFilter = new Filter(aFilterPaths[i]);

                    string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    outputFile1.Write(  (filtersIDStart+i).ToString() + "\t" +
                                        "DBDBB043-F738-4B7F-A14D-8B371CB149E4" + "\t" +
                                        "CANDELS " + aFilterTags[i] + "\t" +
                                        "From: https://archive.stsci.edu/pub/hlsp/candels/goods-s/catalogs/v1/photoz-training/" + "\t" +
                                        " \t" +
                                        "1.0" + "\t" +	
                                        dateString + "\t" +
                                        dateString + "\t" +
                                        tmpFilter.GetBinCenters()[0].ToString() + "\t" +
                                        tmpFilter.GetBinCenters()[tmpFilter.GetBinCenters().Length-1].ToString() + "\t" +
                                        aFilterEffWavelengths[i] + "\t" +
                                        "0" + "\t" +
                                        "0\t" +
                                        " \r\n");

                    for (int j = 0; j < tmpFilter.GetBinCenters().Length; ++j)
                    {
                        outputFile2.Write(  (filterResponsesIDStart + j).ToString() + "\t" +
                                            (filtersIDStart + i).ToString() + "\t" +
                                            tmpFilter.GetBinCenters()[j] + "\t" +
                                            tmpFilter.GetResponses()[j] + "\r\n");
                    }
                    filterResponsesIDStart += tmpFilter.GetBinCenters().Length;

                }
                filtersIDStart += aFilterPaths.Length;

                for (int i = 0; i < aFilterPaths2.Length; ++i)
                {

                    Filter tmpFilter = new Filter(aFilterPaths2[i]);

                    string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    outputFile1.Write(  (filtersIDStart + i).ToString() + "\t" +
                                        "DBDBB043-F738-4B7F-A14D-8B371CB149E4" + "\t" +
                                        "PHAT " + aFilterTags2[i] + "\t" +
                                        "From: http://www.astro.caltech.edu/twiki_phat/bin/view/Main/GoodsNorth" + "\t" +
                                        " \t" +
                                        "1.0" + "\t" +
                                        dateString + "\t" +
                                        dateString + "\t" +
                                        tmpFilter.GetBinCenters()[0].ToString() + "\t" +
                                        tmpFilter.GetBinCenters()[tmpFilter.GetBinCenters().Length - 1].ToString() + "\t" +
                                        aFilterEffWavelengths2[i] + "\t" +
                                        "0" + "\t" +
                                        "0\t" +
                                        " \r\n");

                    for (int j = 0; j < tmpFilter.GetBinCenters().Length; ++j)
                    {
                        outputFile2.Write(  (filterResponsesIDStart + j).ToString() + "\t" +
                                            (filtersIDStart + i).ToString() + "\t" +
                                            tmpFilter.GetBinCenters()[j] + "\t" +
                                            tmpFilter.GetResponses()[j] + "\r\n");
                    }
                    filterResponsesIDStart += tmpFilter.GetBinCenters().Length;

                }
                filtersIDStart += aFilterPaths2.Length;



            }




        }
    }
}
