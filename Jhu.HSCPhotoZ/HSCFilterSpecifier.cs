using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.HSCPhotoZ
{
    public struct HSCFilterSpecifier
    {
        //The detector field already contains the instrument name, so that is superfluous
        //public string instrument;
        public string detector;
        //TODO decide whether to differentiate between different apertures of the same instrument (e.g. WFC1/WFC2/WFCENTER for ACS)
        public string aperture;
        public string filter;


        public HSCFilterSpecifier(string aInst, string aDet, string aAper, string aFilt)
        {
            //instrument = aInst;
            detector = aDet;
            aperture = aAper;
            filter = aFilt;        
        }
    }
}
