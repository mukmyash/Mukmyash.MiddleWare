using System;
using System.Collections.Generic;

namespace Mukmyash.MiddleWare
{
    public interface IMiddlewareBuilder<TContext>
            where TContext : ContextBase
    {
        IServiceProvider ApplicationServices { get; }

        IMiddlewareBuilder<TContext> Use(Func<MiddlewareDelegate<TContext>, MiddlewareDelegate<TContext>> middleware);

        IMiddlewareBuilder<TContext> New();

        MiddlewareDelegate<TContext> Build();
    }
}
