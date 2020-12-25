using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Exceptions
{
    public class DataAccessException : CPlatformException
    {
        public DataAccessException(string message, Exception innerException = null) : base(message, innerException,StatusCode.DataAccessError)
        {
        }
    }
}
