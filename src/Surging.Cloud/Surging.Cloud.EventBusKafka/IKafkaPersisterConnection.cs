using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.EventBusKafka
{
   public interface IKafkaPersisterConnection : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        Object CreateConnect();
    }
}
