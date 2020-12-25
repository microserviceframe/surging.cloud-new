using Surging.Cloud.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Filters.Implementation
{
   public  class AuthorizationAttribute : AuthorizationFilterAttribute
    {
        public AuthorizationType AuthType { get; set; }
    }
}
