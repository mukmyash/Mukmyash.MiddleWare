using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mukmyash.MiddleWare
{
    public abstract class MiddlewareBuilderBase<TContext> : IMiddlewareBuilder<TContext>
        where TContext : ContextBase
    {
        private readonly IList<Func<MiddlewareDelegate<TContext>, MiddlewareDelegate<TContext>>> _components
            = new List<Func<MiddlewareDelegate<TContext>, MiddlewareDelegate<TContext>>>();

        private IDictionary<string, object> _properties;

        public MiddlewareBuilderBase(IServiceProvider serviceProvider)
        {
            _properties = new Dictionary<string, object>();
            ApplicationServices = serviceProvider;
        }

        protected MiddlewareBuilderBase(MiddlewareBuilderBase<TContext> builder)
        {
            _properties = builder._properties;
        }

        public IServiceProvider ApplicationServices
        {
            get => GetProperty<IServiceProvider>("ApplicationServices");
            private set => SetProperty("ApplicationServices", value);
        }


        protected T GetProperty<T>(string key)
        {
            return _properties.TryGetValue(key, out var value) ? (T)value : default(T);
        }

        protected void SetProperty<T>(string key, T value)
        {
            _properties[key] = value;
        }

        public IMiddlewareBuilder<TContext> Use(Func<MiddlewareDelegate<TContext>, MiddlewareDelegate<TContext>> middleware)
        {
            _components.Add(middleware);
            return this;
        }

        /// <summary>
        /// Create new <see cref="IMiddlewareBuilder{TContext}"/> based this builder
        /// </summary>
        /// <returns></returns>
        public abstract IMiddlewareBuilder<TContext> New();

        /// <summary>
        /// Build middleware flow
        /// </summary>
        /// <returns></returns>
        public MiddlewareDelegate<TContext> Build()
        {
            MiddlewareDelegate<TContext> endMiddleware = (context =>
            {
                return Task.CompletedTask;
            });

            foreach (var component in _components.Reverse())
            {
                endMiddleware = component(endMiddleware);
            }

            return endMiddleware;
        }
    }
}
