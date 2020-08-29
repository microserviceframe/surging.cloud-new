using Newtonsoft.Json;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Routing;
using Surging.Core.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Surging.Core.Caching;
using System.Text.RegularExpressions;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Exceptions;
using JWT.Builder;
using JWT.Algorithms;
using System.Security.Claims;
using ClaimTypes = Surging.Core.CPlatform.ClaimTypes;
using JWT;
using Surging.Core.CPlatform.Utilities;
using JWT.Exceptions;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;

namespace Surging.Core.ApiGateWay.OAuth
{
    /// <summary>
    /// 授权服务提供者
    /// </summary>
    public class AuthorizationServerProvider: IAuthorizationServerProvider
    {
        private readonly IServiceProxyProvider _serviceProxyProvider;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly CPlatformContainer _serviceProvider;
        private readonly ICacheProvider _cacheProvider;
        public AuthorizationServerProvider(IServiceProxyProvider serviceProxyProvider
           ,IServiceRouteProvider serviceRouteProvider
            , CPlatformContainer serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceProxyProvider = serviceProxyProvider;
            _serviceRouteProvider = serviceRouteProvider;
            _cacheProvider = CacheContainer.GetService<ICacheProvider>(AppConfig.CacheMode);
        }

        public async Task<string> IssueToken(Dictionary<string, object> parameters)
        {
            string result = null;
            var payload = await _serviceProxyProvider.Invoke<IDictionary<string,object>>(parameters,AppConfig.AuthenticationRoutePath, HttpMethod.POST, AppConfig.AuthenticationServiceKey);
            if (payload !=null && !payload.Equals("null") )
            {
                if (!payload.ContainsKey(ClaimTypes.UserId) || !payload.ContainsKey(ClaimTypes.UserName)) 
                {
                    throw new AuthException($"认证接口实现不正确,接口返回值必须包含{ClaimTypes.UserId}和{ClaimTypes.UserName}");
                }
                var jwtBuilder = GetJwtBuilder(AppConfig.JwtSecret);
                var exp = AppConfig.DefaultExpired;
                if (payload.ContainsKey(ClaimTypes.Expired)) 
                {
                    exp = payload[ClaimTypes.Expired].To<int>();
                    payload.Remove(ClaimTypes.Expired);
                }
                jwtBuilder.AddClaim(ClaimTypes.Expired, DateTimeOffset.UtcNow.AddHours(exp).ToUnixTimeSeconds());
                foreach (var para in payload) 
                {
                    jwtBuilder.AddClaim(para.Key, para.Value);
                }
                result = jwtBuilder.Encode();
            }
            return result;
        }

        public IDictionary<string, object> GetPayload(string token)
        {
            var jwtBuilder = GetJwtBuilder(AppConfig.JwtSecret);
            return jwtBuilder
                .MustVerifySignature()
                .Decode<IDictionary<string, object>>(token);
           
        }

        public string RefreshToken(string token)
        {
            var payload = GetPayloadDoNotVerifySignature(token);
            var exp = AppConfig.DefaultExpired;
            if (payload.ContainsKey(ClaimTypes.Expired)) 
            {
                exp = payload[ClaimTypes.Expired].To<int>();
            }
            var jwtBuilder = GetJwtBuilder(AppConfig.JwtSecret);
            jwtBuilder.AddClaim(ClaimTypes.Expired, DateTimeOffset.UtcNow.AddHours(exp).ToUnixTimeSeconds());
            foreach (var para in payload)
            {
                jwtBuilder.AddClaim(para.Key, para.Value);
            }
            return jwtBuilder.Encode();

        }

        public ValidateResult ValidateClientAuthentication(string token)
        {
            try
            {
                var jwtBuilder = GetJwtBuilder(AppConfig.JwtSecret);
                jwtBuilder.MustVerifySignature()
                .Decode<IDictionary<string, object>>(token);
                return ValidateResult.Success;
            }
            catch (TokenExpiredException)
            {
                return ValidateResult.TokenExpired;
            }
            catch (SignatureVerificationException)
            {
                return ValidateResult.SignatureError;
            }
            catch (Exception) 
            {
                return ValidateResult.TokenFormatError;
            }



        }

        private JwtBuilder GetJwtBuilder(string secret, IJwtAlgorithm algorithm = null) 
        {
            if (secret.IsNullOrEmpty()) 
            {
                throw new AuthException("未设置TokenSecret,请先设置TokenSecret", StatusCode.IssueTokenError);
            }
            if (algorithm == null) 
            {
                algorithm = new HMACSHA256Algorithm();
            }
            return new JwtBuilder()
                .WithAlgorithm(algorithm) 
                .WithSecret(secret);
        }

        private IDictionary<string, object> GetPayloadDoNotVerifySignature(string token)
        {
            var jwtBuilder = GetJwtBuilder(AppConfig.JwtSecret);
            return jwtBuilder
                .DoNotVerifySignature()
                .Decode<IDictionary<string, object>>(token);

        }

    }
}
