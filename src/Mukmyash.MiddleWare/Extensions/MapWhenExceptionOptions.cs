using System;
using System.Collections.Generic;
using System.Text;

namespace Mukmyash.MiddleWare.Extensions
{
    internal class MapWhenExceptionOptions<TContext>
        where TContext : ContextBase
    {
        /// <summary>
        /// The branch taken for a positive match.
        /// </summary>
        public MiddlewareDelegate<TContext> Branch { get; set; }
    }
}
