using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mukmyash.MiddleWare.Extensions
{
    public static class UseExtensions
    {
        public static IMiddlewareBuilder<TContext> Use<TContext>(this IMiddlewareBuilder<TContext> app, Func<TContext, Func<Task>, Task> middleware)
            where TContext : ContextBase
        {
            return app.Use(next =>
            {
                return context =>
                {
                    Func<Task> simpleNext = () => next(context);
                    return middleware(context, simpleNext);
                };
            });
        }
    }
}
