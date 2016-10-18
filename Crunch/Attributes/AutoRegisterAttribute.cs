using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crunch.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoRegisterAttribute : Attribute
    {
        public AutoRegisterAttribute(ServiceLifetime lifetime = ServiceLifetime.Transient) {
            this.ServiceLifetime = lifetime;
        }
        public ServiceLifetime ServiceLifetime = ServiceLifetime.Transient;
    }
}
