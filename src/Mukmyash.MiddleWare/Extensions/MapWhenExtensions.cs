using System;
using System.Collections.Generic;
using System.Text;

namespace Mukmyash.MiddleWare.Extensions
{
    public static class MapWhenExtensions
    {
        /// <summary>
        /// Branches the request pipeline based on the result of the given predicate.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="predicate">Invoked with the request environment to determine if the branch should be taken</param>
        /// <param name="configuration">Configures a branch to take</param>
        /// <returns></returns>
        public static IMiddlewareBuilder<TContext> MapWhen<TContext>(this IMiddlewareBuilder<TContext> app, Predicate<TContext> predicate, Action<IMiddlewareBuilder<TContext>> configuration)
            where TContext : ContextBase
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
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
            var options = new MapWhenOptions<TContext>
            {
                Predicate = predicate,
                Branch = branch,
            };
            return app.Use(next => new MapWhenMiddleware<TContext>(next, options).Invoke);
        }
    }
}