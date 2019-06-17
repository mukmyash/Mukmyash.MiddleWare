using System;
using System.Collections.Generic;
using System.Text;

namespace Mukmyash.MiddleWare.Extensions
{
    public static class MapWhenExceptionExtensions
    {
        /// <summary>
        /// Branches the request pipeline based on the result of the given predicate.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="predicate">Invoked with the request environment to determine if the branch should be taken</param>
        /// <param name="configuration">Configures a branch to take</param>
        /// <returns></returns>
        public static IMiddlewareBuilder<TContext> MapWhenException<TContext>(this IMiddlewareBuilder<TContext> app, Action<IMiddlewareBuilder<TContext>> configuration)
            where TContext : ContextBase
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // create branch
            var branchBuilder = app.New();
            configuration(branchBuilder);
            var branch = branchBuilder.Build();

            // put middleware in pipeline
            var options = new MapWhenExceptionOptions<TContext>
            {
                Branch = branch,
            };
            return app.Use(next => new MapWhenExceptionMiddleware<TContext>(next, options).Invoke);
        }
    }
}