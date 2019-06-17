using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Mukmyash.MiddleWare.Tests.Model
{
    public class TestContext : ContextBase
    {
        public TestContext(IServiceProvider contextServices)
        {
            ContextServices = contextServices;
        }

        public StringBuilder Message { get; } = new StringBuilder();

        public override IServiceProvider ContextServices { get; }

    }
}
