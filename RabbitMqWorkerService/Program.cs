namespace RabbitMqWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<ScopedBackgroundService>();
                })
                .Build();

            host.Run();
        }
    }
}