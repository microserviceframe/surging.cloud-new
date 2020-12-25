using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.ApiGateWay.OAuth
{
   public class JWTSecureDataHeader
    {
        public JWTSecureDataType Type { get; set; }

        public EncryptMode EncryptMode { get; set; }

        public string TimeStamp { get; set; }
    }
}
