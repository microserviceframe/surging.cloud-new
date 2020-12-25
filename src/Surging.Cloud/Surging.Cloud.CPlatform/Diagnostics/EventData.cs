using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Diagnostics
{
    public class EventData
    {
        public EventData(Guid operationId)
        {
            OperationId = operationId; 
        }

        public Guid OperationId { get; set; }

    }
}
