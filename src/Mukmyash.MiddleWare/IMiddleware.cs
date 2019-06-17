using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mukmyash.MiddleWare
{
    public interface IMiddleware<TContext>
        where TContext : ContextBase
    {
        Task InvokeAsync(TContext context, MiddlewareDelegate<TContext> next);
    }
}
