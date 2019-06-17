using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mukmyash.MiddleWare.Extensions
{
    internal class MapWhenExceptionMiddleware<TContext>
        where TContext : ContextBase
    {
        private readonly MiddlewareDelegate<TContext> _next;
        private readonly MapWhenExceptionOptions<TContext> _options;

        /// <summary>
        /// Creates a new instance of <see cref="MapWhenExceptionMiddleware"/>.
        /// </summary>
        /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
        /// <param name="options">The middleware options.</param>
        public MapWhenExceptionMiddleware(
            MiddlewareDelegate<TContext> next,
            MapWhenExceptionOptions<TContext> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Executes the middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>A task that represents the execution of this middleware.</returns>
        public async Task Invoke(TContext context)
        {
            try
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }
                await _next(context);
            }
            catch (Exception e)
            {
                context.Error = e;
                await _options.Branch(context);
            }
        }
    }
}