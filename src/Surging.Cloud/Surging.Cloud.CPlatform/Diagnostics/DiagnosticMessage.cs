using Surging.Cloud.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Diagnostics
{
    public class DiagnosticMessage: TransportMessage
    {
        public string MessageName { get; set; }
    }
}
