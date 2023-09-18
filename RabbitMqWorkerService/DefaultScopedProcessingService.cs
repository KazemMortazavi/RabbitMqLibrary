using RabbitMQ.Client;
using RabbitMqLibrary.Contract;

namespace RabbitMqWorkerService
{
    public interface IScopedProcessingService
    {
        Task DoWorkAsync(CancellationToken stoppingToken);
    }

    public sealed class DefaultScopedProcessingService : IScopedProcessingService
    {
        private int _executionCount;
        private readonly ILogger<DefaultScopedProcessingService> _logger;
        private readonly IQueuePool _queueClient;
        public DefaultScopedProcessingService(
            ILogger<DefaultScopedProcessingService> logger
            , IQueuePool queueClient
            )
        {
            _logger = logger;
            _queueClient = queueClient;
        }

        public async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ++_executionCount;

                _queueClient.QueueSubscribe(RabbitMqLibrary.Config.VirtualHostType.command, "test", Message_Received);

                _queueClient.QueuePublish(RabbitMqLibrary.Config.VirtualHostType.command, "test", null, "hello world", connectionPool: true);

                //var lessons = await _LessonService.Get(0, 10000);
                //foreach (var item in lessons.Item1)
                //{
                //    if (!string.IsNullOrEmpty(item.MediaLink) && item.MediaLink.Contains("https://abrehamrahi.ir/ngp?type=vod&id=") && string.IsNullOrEmpty(item.MediaLink2))
                //    {
                //        var videoId = item.MediaLink.Replace("https://abrehamrahi.ir/ngp?type=vod&id=", "").Replace("&autoplay=true&fullscreen=true&landscape=true&site=zarebin.ir", "").Replace("ظ", "");
                //    label1:
                //        var m3u8Content = new GetM3U8Response();
                //        try { m3u8Content = await _abreHamrahiVODApiService.GetDetailsAsync(videoId); }
                //        catch (ApplicationException ex) { _logger.LogError(ex.Message); }
                //        catch (Exception ex) { await Task.Delay(1_000, stoppingToken); goto label1; }
                //        item.MediaLink2 = m3u8Content.stream_link;
                //        await _LessonService.Update(_mapper.Map<LessonRequestDto>(item));
                //        _logger.LogInformation($"media updated {videoId}");
                //    }
                //}

                _logger.LogInformation(
                    "{ServiceName} working, execution count: {Count}",
                    nameof(DefaultScopedProcessingService),
                    _executionCount);

                await Task.Delay(60_000, stoppingToken);
            }
        }

        private void Message_Received(string body, string header, IModel model)
        {
            Console.WriteLine("Message_Received " + body);
        }
    }
    public sealed class ScopedBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScopedBackgroundService> _logger;

        public ScopedBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ScopedBackgroundService> logger) =>
            (_serviceProvider, _logger) = (serviceProvider, logger);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(ScopedBackgroundService)} is running.");

            await DoWorkAsync(stoppingToken);
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(ScopedBackgroundService)} is working.");

            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                IScopedProcessingService scopedProcessingService =
                    scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();

                await scopedProcessingService.DoWorkAsync(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"{nameof(ScopedBackgroundService)} is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}