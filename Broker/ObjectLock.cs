using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker
{
    internal static class ObjectLock
    {
        internal static readonly object Self;
        static ObjectLock()
        {
            Self = new object();
        }
    }
}
