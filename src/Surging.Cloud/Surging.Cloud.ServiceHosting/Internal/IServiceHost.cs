using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Cloud.ServiceHosting.Internal
{
   public interface IServiceHost : IDisposable
    {
        IDisposable Run();

        IContainer Initialize();
    }
}
