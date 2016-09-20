using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace netCoreTest.Attributes
{
    public enum ServiceLifetime
    {
        Transient,
        Singleton,
        PerRequest
    }
}
