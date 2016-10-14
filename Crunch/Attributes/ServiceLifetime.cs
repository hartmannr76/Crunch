using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crunch.Attributes
{
    public enum ServiceLifetime
    {
        Transient,
        Singleton,
        PerRequest
    }
}
