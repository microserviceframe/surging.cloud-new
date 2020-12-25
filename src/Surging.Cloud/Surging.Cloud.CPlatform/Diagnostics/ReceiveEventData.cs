using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Diagnostics
{
    public class ReceiveEventData : EventData
    {
        public ReceiveEventData(DiagnosticMessage message): base(Guid.Parse(message.Id))
        {
            Message = message;
        }
         
        public TracingHeaders Headers { get; set; }

        public DiagnosticMessage Message { get; set; }
    }
}
