using Surging.Cloud.KestrelHttpServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Stage.Internal
{
    public interface IWebServerListener
    {
        void Listen(WebHostContext context);
    }
}
