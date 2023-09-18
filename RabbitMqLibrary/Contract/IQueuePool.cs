using RabbitMQ.Client;
using RabbitMqLibrary.Config;

namespace RabbitMqLibrary.Contract;

public interface IQueuePool
{
    bool QueuePublish(VirtualHostType vHost, string qname, object header, object body, bool connectionPool = false);
 
    bool QueueSubscribe(VirtualHostType vHost, string qname, Action<string, string, IModel> messageReceiver);
}
