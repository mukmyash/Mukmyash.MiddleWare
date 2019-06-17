using Microsoft.Extensions.DependencyInjection;
using Mukmyash.MiddleWare.Tests.Model;
using Mukmyash.MiddleWare.Extensions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Mukmyash.MiddleWare.Tests
{
    public class MiddlewareBuilderTest
    {
        [Fact]
        public async Task InjectToConstructor_Middlewares()
        {
            IServiceProvider provider = new ServiceCollection()
                .BuildServiceProvider();

            IMiddlewareBuilder<TestContext> middlewareBuilder = new TestMiddlewareBuilder(provider);

            var resultBuild = middlewareBuilder
                .UseMiddleware<AddTextMiddleware, TestContext>("Hello")
                .UseMiddleware<AddTextMiddleware, TestContext>("World")
                .Build();

            TestContext context;
            using (var scopedProvider = provider.CreateScope())
            {
                context = new TestContext(scopedProvider.ServiceProvider);
                await resultBuild(context);
            }

            Assert.Equal($"Hello{Environment.NewLine}World{Environment.NewLine}", context.Message.ToString());
        }


        [Fact]
        public async Task InjectToConstructorAndInvoke_Middleware()
        {
            IServiceProvider provider = new ServiceCollection()
                .AddScoped(prov =>
                {
                    return new AddTextFromOptionsMiddlewareOptions()
                    {
                        Text = "World"
                    };
                })
                .BuildServiceProvider();

            IMiddlewareBuilder<TestContext> middlewareBuilder = new TestMiddlewareBuilder(provider);

            var resultBuild = middlewareBuilder
                .UseMiddleware<AddTextFromOptionsMiddleware, TestContext>("Hello")
                .UseMiddleware<AddTextFromOptionsMiddleware, TestContext>("GoodBy")
                .Build();

            TestContext context;
            using (var scopedProvider = provider.CreateScope())
            {
                context = new TestContext(scopedProvider.ServiceProvider);
                await resultBuild(context);
            }

            Assert.Equal($"HelloWorld{Environment.NewLine}GoodByWorld{Environment.NewLine}", context.Message.ToString());
        }
    }
}
