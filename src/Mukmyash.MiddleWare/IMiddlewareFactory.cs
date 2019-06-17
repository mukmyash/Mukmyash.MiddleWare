using System;
using System.Collections.Generic;
using System.Text;

namespace Mukmyash.MiddleWare
{
    public interface IMiddlewareFactory<TContext>
        where TContext : ContextBase
    {
        IMiddleware<TContext> Create(Type middlewareType);
        void Release(IMiddleware<TContext> middleware);
    }
}
