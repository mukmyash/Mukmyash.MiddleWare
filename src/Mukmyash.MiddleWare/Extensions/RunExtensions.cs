using System;
using System.Collections.Generic;
using System.Text;

namespace Mukmyash.MiddleWare.Extensions
{
    public static class RunExtensions
    {
        public static void Run<TContext>(this IMiddlewareBuilder<TContext> app, MiddlewareDelegate<TContext> handler)
            where TContext : ContextBase
        {
            app.Use(_ => handler);
        }
    }
}
