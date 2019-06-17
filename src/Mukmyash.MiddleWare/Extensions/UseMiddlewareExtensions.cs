using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mukmyash.MiddleWare.Extensions
{
    public static class UseMiddlewareExtensions
    {
        internal const string InvokeMethodName = "Invoke";
        internal const string InvokeAsyncMethodName = "InvokeAsync";

        private static readonly MethodInfo GetServiceInfo
            = typeof(UseMiddlewareExtensions).GetMethod(nameof(GetService), BindingFlags.NonPublic | BindingFlags.Static);

        public static IMiddlewareBuilder<TContext> UseMiddleware<TMiddleware, TContext>(this IMiddlewareBuilder<TContext> builder, params object[] args)
            where TContext : ContextBase
        {
            return builder.UseMiddleware<TContext>(typeof(TMiddleware), args);
        }

        public static IMiddlewareBuilder<TContext> UseMiddleware<TContext>(this IMiddlewareBuilder<TContext> builder, Type middleware, params object[] args)
            where TContext : ContextBase
        {
            if (typeof(IMiddleware<TContext>).GetTypeInfo().IsAssignableFrom(middleware.GetTypeInfo()))
            {
                if (args.Length > 0)
                {
                    throw new NotSupportedException("IMiddleware doesn't support passing args directly since it's");
                }

                return UseMiddlewareInterface(builder, middleware);
            }

            var applicationServices = builder.ApplicationServices;
            return builder.Use(next =>
            {
                var methods = middleware.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                var invokeMethods = methods.Where(m =>
                    string.Equals(m.Name, InvokeMethodName, StringComparison.Ordinal)
                    || string.Equals(m.Name, InvokeAsyncMethodName, StringComparison.Ordinal)
                    ).ToArray();

                if (invokeMethods.Length > 1)
                {
                    throw new InvalidOperationException($"Find over then one invoke methode.");
                }

                if (invokeMethods.Length == 0)
                {
                    throw new InvalidOperationException("Can't find invoke methode.");
                }

                var methodInfo = invokeMethods[0];
                if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                {
                    throw new InvalidOperationException($"Invoke methode must return Task.");
                }

                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 0 || parameters[0].ParameterType.BaseType != typeof(ContextBase))
                {
                    throw new InvalidOperationException($"Can't find methode {InvokeMethodName} or {InvokeAsyncMethodName} with parameters");
                }

                var ctorArgs = new object[args.Length + 1];
                ctorArgs[0] = next;
                Array.Copy(args, 0, ctorArgs, 1, args.Length);
                var instance = ActivatorUtilities.CreateInstance(builder.ApplicationServices, middleware, ctorArgs);
                if (parameters.Length == 1)
                {
                    return (MiddlewareDelegate<TContext>)methodInfo.CreateDelegate(typeof(MiddlewareDelegate<TContext>), instance);
                }

                var factory = Compile<object, TContext>(methodInfo, parameters);

                return context =>
                {
                    var serviceProvider = context.ContextServices ?? applicationServices;
                    if (serviceProvider == null)
                    {
                        throw new InvalidOperationException("ServiceProvider not available.");
                    }

                    return factory(instance, context, serviceProvider);
                };
            });
        }

        //private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        //{
        //    while (toCheck != null && toCheck != typeof(object))
        //    {
        //        var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
        //        if (generic == cur)
        //        {
        //            return true;
        //        }
        //        toCheck = toCheck.BaseType;
        //    }
        //    return false;
        //}

        private static IMiddlewareBuilder<TContext> UseMiddlewareInterface<TContext>(IMiddlewareBuilder<TContext> app, Type middlewareType)
            where TContext : ContextBase
        {
            return app.Use(next =>
            {
                return async context =>
                {
                    var middlewareFactory = (IMiddlewareFactory<TContext>)context.ContextServices.GetService(typeof(IMiddlewareFactory<TContext>));
                    if (middlewareFactory == null)
                    {
                        // No middleware factory
                        throw new InvalidOperationException("No middleware factory.");
                    }

                    var middleware = middlewareFactory.Create(middlewareType);
                    if (middleware == null)
                    {

                        throw new InvalidOperationException("The factory returned null, it's a broken implementation.");
                    }

                    try
                    {
                        await middleware.InvokeAsync(context, next);
                    }
                    finally
                    {
                        middlewareFactory.Release(middleware);
                    }
                };
            });
        }

        private static Func<TMiddleware, TContext, IServiceProvider, Task> Compile<TMiddleware, TContext>(MethodInfo methodInfo, ParameterInfo[] parameters)
            where TContext : ContextBase
        {
            // If we call something like
            //
            // public class Middleware
            // {
            //    public Task Invoke(HttpContext context, ILoggerFactory loggerFactory)
            //    {
            //
            //    }
            // }
            //

            // We'll end up with something like this:
            //   Generic version:
            //
            //   Task Invoke(Middleware instance, HttpContext httpContext, IServiceProvider provider)
            //   {
            //      return instance.Invoke(httpContext, (ILoggerFactory)UseMiddlewareExtensions.GetService(provider, typeof(ILoggerFactory));
            //   }

            //   Non generic version:
            //
            //   Task Invoke(object instance, HttpContext httpContext, IServiceProvider provider)
            //   {
            //      return ((Middleware)instance).Invoke(httpContext, (ILoggerFactory)UseMiddlewareExtensions.GetService(provider, typeof(ILoggerFactory));
            //   }

            var middleware = typeof(TMiddleware);

            var contextArg = Expression.Parameter(typeof(TContext), "context");
            var providerArg = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
            var instanceArg = Expression.Parameter(middleware, "middleware");

            var methodArguments = new Expression[parameters.Length];
            methodArguments[0] = contextArg;
            for (int i = 1; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsByRef)
                {
                    throw new NotSupportedException($"ref params not suported for methode {InvokeMethodName}");
                }

                var parameterTypeExpression = new Expression[]
                {
                    providerArg,
                    Expression.Constant(parameterType, typeof(Type)),
                    Expression.Constant(methodInfo.DeclaringType, typeof(Type))
                };

                var getServiceCall = Expression.Call(GetServiceInfo, parameterTypeExpression);
                methodArguments[i] = Expression.Convert(getServiceCall, parameterType);
            }

            Expression middlewareInstanceArg = instanceArg;
            if (methodInfo.DeclaringType != typeof(TMiddleware))
            {
                middlewareInstanceArg = Expression.Convert(middlewareInstanceArg, methodInfo.DeclaringType);
            }

            var body = Expression.Call(middlewareInstanceArg, methodInfo, methodArguments);

            var lambda = Expression.Lambda<Func<TMiddleware, TContext, IServiceProvider, Task>>(body, instanceArg, contextArg, providerArg);

            return lambda.Compile();
        }

        private static object GetService(IServiceProvider sp, Type type, Type middleware)
        {
            var service = sp.GetService(type);
            if (service == null)
            {
                throw new InvalidOperationException($"Service {type.FullName} for middleware {middleware.FullName} not found.");
            }

            return service;
        }
    }
}