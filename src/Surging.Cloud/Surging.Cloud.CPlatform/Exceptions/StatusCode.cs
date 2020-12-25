namespace Surging.Cloud.CPlatform.Exceptions
{
    public enum StatusCode
    {
        Success = 200,
        /// <summary>
        /// 通信错误
        /// </summary>
        CommunicationError = 501,

        /// <summary>
        /// 平台架构异常
        /// </summary>
        CPlatformError = 602,

        /// <summary>
        /// 业务处理异常
        /// </summary>
        BusinessError = 503,

        /// <summary>
        /// 输入错误
        /// </summary>
        ValidateError = 504,

        /// <summary>
        /// 用户友好类异常
        /// </summary>
        UserFriendly = 506,

        /// <summary>
        /// 路由配置错误
        /// </summary>
        RouteError = 507,


        LockerTimeout = 507,

        /// <summary>
        /// 数据访问错误
        /// </summary>
        DataAccessError = 705,


        RegisterConnection = 707,

        /// <summary>
        /// 请求错误
        /// </summary>
        RequestError = 708,

         /// <summary>
        /// 服务不可用
        /// </summary>
        ServiceUnavailability = 709,

        /// <summary>
        /// 未被认证
        /// </summary>
        UnAuthentication = 401,

        TokenExpired = 406,

        IssueTokenError = 407,


        /// <summary>
        /// 未授权
        /// </summary>
        UnAuthorized = 402,

        Http405EndpointStatusCode = 405,

        Http404EndpointStatusCode = 404,


        /// <summary>
        /// 未知错误
        /// </summary>
        UnKnownError = -1,

 
    }
}
