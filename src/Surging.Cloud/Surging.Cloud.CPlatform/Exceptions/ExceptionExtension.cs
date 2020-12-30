using Surging.Cloud.CPlatform.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Exceptions
{
    public static class ExceptionExtension
    {
        public static string GetExceptionMessage(this Exception exception)
        {
            if (exception == null)
                return string.Empty;

            var message = exception.Message;
            if ((AppConfig.ServerOptions.Environment == RuntimeEnvironment.Development || AppConfig.ServerOptions.ForceDisplayStackTrace)
                && !exception.IsBusinessException())       
                
            {
                message += Environment.NewLine + " 堆栈信息:" + exception.StackTrace;
                if (exception.InnerException != null)
                {
                    message += "|InnerException:" + GetExceptionMessage(exception.InnerException);
                }
            }
            else
            {
                if (!exception.IsBusinessException())
                {
                    message += ";" + GetExceptionMessage(exception.InnerException);
                }
            }

            return message;
        }

        public static StatusCode GetExceptionStatusCode(this Exception exception)
        {
            var statusCode = StatusCode.UnKnownError;
            if (exception is TimeoutException) 
            {
                return StatusCode.CommunicationError;
            }
            if (exception is CPlatformException)
            {
                statusCode = ((CPlatformException)exception).ExceptionCode;
                return statusCode;
            }
            if (exception.InnerException != null)
            {
                return exception.InnerException.GetExceptionStatusCode();
            }
            return statusCode;

        }

        public static bool IsBusinessException(this Exception exception)
        {
            var statusCode = exception.GetExceptionStatusCode();
            return statusCode == StatusCode.Success
                   || statusCode == StatusCode.ValidateError
                   || statusCode == StatusCode.UserFriendly
                   || statusCode == StatusCode.BusinessError
                   || statusCode == StatusCode.UnAuthentication
                   || statusCode == StatusCode.UnAuthorized;
        }
    }
}
