using System;

namespace Surging.Cloud.CPlatform.Runtime.Server.Implementation
{
    public class ServiceTokenGenerator : IServiceTokenGenerator
    {
        public string _serviceToken;

        public ServiceTokenGenerator()
        {
            _serviceToken = null;
        }

        public string GeneratorToken(string code)
        {
            _serviceToken = code;
            return _serviceToken;
        }

        public string GetToken()
        {
            return _serviceToken;
        }
    }
}
