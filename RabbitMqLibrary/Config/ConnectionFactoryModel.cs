namespace RabbitMqLibrary.Config;

public class ConnectionFactoryModel
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string HostName { get; set; }
    public required string Port { get; set; }
    public required string VirtualHost { get; set; }
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public int NetworkRecoveryInterval { get; set; } = 60;
    public int RequestedHeartbeat { get; set; } = 60;
}
