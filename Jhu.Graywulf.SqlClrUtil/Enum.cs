using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jhu.Graywulf.SqlClrUtil
{
    public enum AssemblySecurityLevel
    {
        Safe = 1,
        External = 2,
        Unrestricted = 3,
    }

    enum SqlObjectRank : int
    {
        UserDefinedType = 0,
        Function = 1,
        Aggregate = 2
    }
}
