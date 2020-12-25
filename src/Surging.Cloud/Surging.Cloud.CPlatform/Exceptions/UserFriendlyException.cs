using System;

namespace Surging.Cloud.CPlatform.Exceptions
{
    public class UserFriendlyException : BusinessException
    {

        public UserFriendlyException(string message) : base(message, StatusCode.UserFriendly)
        {

        }
    }
}
