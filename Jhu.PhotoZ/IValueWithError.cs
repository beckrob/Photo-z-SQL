using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    interface IValueWithError<T>
    {
        T Value { get; set; }
        T Error { get; set; }
    }
}
