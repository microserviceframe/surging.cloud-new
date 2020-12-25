using System;

namespace Surging.Cloud.CPlatform.Exceptions
{
    public class ValidateException : BusinessException
    {

        public ValidateException(string message) : base(message, StatusCode.ValidateError)
        {

        }
    }
}
