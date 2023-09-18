using RabbitMQ.Client;
using RabbitMqLibrary.Config;
using RabbitMqLibrary.Contract;
using System.Collections.Concurrent;

namespace RabbitMqLibrary;

public class QueuePool : IQueuePool, IDisposable
{
    private readonly QueuePool queueInstance;
    private readonly IConnectionManagement _connectionManagement;
    private IConnection _connection;
    private readonly ConcurrentDictionary<string, IConnection> _connectionPool;
    private string _vhost;
    public QueuePool(IConnectionManagement connectionManagement)
    {
        _connectionManagement = connectionManagement;
        _connectionPool = new ConcurrentDictionary<string, IConnection>();
    }


    public bool QueuePublish(VirtualHostType vHost, string qname, object header, object body, bool connectionPool = false)
    {
        _vhost = vHost.ToString();
        if (connectionPool)
        {
            _ = _connectionPool.TryGetValue(_vhost, out _connection);
            if (_connection == null)
            {
                _connection = _connectionManagement.GetCnn(vHost.ToString().ToLower());
                _ = _connectionPool.TryAdd(vHost.ToString(), _connection);
            }

            if (_connection != null && !_connection.IsOpen)
            {
                try { _connection.Close(); _connection.Dispose(); } catch (Exception) { }
                _ = _connectionPool.Remove(vHost.ToString(), out IConnection? connection);
                _connection = _connectionManagement.GetCnn(vHost.ToString().ToLower());
                _ = _connectionPool.TryAdd(vHost.ToString(), _connection);
            }
            return QueuePublisher.Publisher(_connection, header, body, qname);
        }
        else
        {
            var con = _connectionManagement.GetConnection(vHost.ToString().ToLower());
            return QueuePublisher.Publisher(con, header, body, qname);
        }

    }

    public bool QueueSubscribe(VirtualHostType vHost, string qname, Action<string, string, IModel> messageReceiver)
    {
        var con = _connectionManagement.GetConnection(vHost.ToString().ToLower());
        new QueueReceived().Subscribe(con, qname, messageReceiver);
        return true;
    }

    public void Dispose()
    {
        if (_connection != null)
        {
            if (_connection.IsOpen)
            {
                _connection.Close();
            }
            _connection.Dispose();
        }
        foreach (KeyValuePair<string, IConnection> item in _connectionPool)
        {
            if (item.Value.IsOpen)
            {
                item.Value.Close();
            }

            item.Value.Dispose();
        }
    }

}
