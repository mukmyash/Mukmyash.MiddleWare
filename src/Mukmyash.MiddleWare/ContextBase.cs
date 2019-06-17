using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Mukmyash.MiddleWare
{
    public abstract class ContextBase
    {
        public abstract IServiceProvider ContextServices { get; }

        public Exception Error { get; internal set; }
    }
}
