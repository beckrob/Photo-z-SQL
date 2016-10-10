using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.Graywulf
{
    public static class Copyright
    {
#if DEBUG
        public const string InfoConfiguration = "Debug";
#else
        public const string InfoConfiguration = "Release";
#endif
        public const string InfoProduct = "Graywulf";
        public const string InfoCompany = "JHU IDIES / ELTE";
        public const string InfoCopyright = "Copyright (c) 2008-2015 László Dobos, IDIES, The Johns Hopkins University, Eötvös University";
        public const string InfoTrademark = "";
        public const string InfoCulture = "";
        public const string AssemblyVersion = "1.1.*";
    }
}
