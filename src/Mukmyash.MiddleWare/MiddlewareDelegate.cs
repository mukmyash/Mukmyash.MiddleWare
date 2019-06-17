using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mukmyash.MiddleWare
{
    public delegate Task MiddlewareDelegate<TContext>(TContext context)
            where TContext : ContextBase;
}
