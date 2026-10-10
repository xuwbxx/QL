using Service.Wind.TimeWorker;
using Tool;

namespace WindWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly TyphoonWorkerService _typhoonWorkerService;
        private readonly WeatherWorkerService _weatherWorkerService;

        /// <summary>
        /// 台风数据收集间隔
        /// </summary>
        private readonly TimeSpan _typhoonPeriod = TimeSpan.FromHours(2);

        /// <summary>
        /// 气象数据收集间隔
        /// </summary>
        private readonly TimeSpan _weatherPeriod = TimeSpan.FromHours(4);

        public Worker(TyphoonWorkerService typhoonWorkerService, WeatherWorkerService weatherWorkerService)
        {
            _typhoonWorkerService = typhoonWorkerService;
            _weatherWorkerService = weatherWorkerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            LoggerUtils.Info($"定时服务已启动，台风间隔: {_typhoonPeriod}，气象间隔: {_weatherPeriod}", typeof(Worker));

            using var typhoonTimer = new PeriodicTimer(_typhoonPeriod);
            using var weatherTimer = new PeriodicTimer(_weatherPeriod);

            // 启动时各执行一次
            await SaveTyphoonDataAsync();
            await SaveWeatherDataAsync();

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var typhoonTask = typhoonTimer.WaitForNextTickAsync(stoppingToken).AsTask();
                    var weatherTask = weatherTimer.WaitForNextTickAsync(stoppingToken).AsTask();

                    var completedTask = await Task.WhenAny(typhoonTask, weatherTask);

                    if (completedTask == typhoonTask)
                    {
                        await typhoonTask;
                        await SaveTyphoonDataAsync();
                    }
                    else
                    {
                        await weatherTask;
                        await SaveWeatherDataAsync();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                LoggerUtils.Info("定时服务正在停止...", typeof(Worker));
            }
        }

        private async Task SaveTyphoonDataAsync()
        {
            try
            {
                LoggerUtils.Info($"台风数据收集开始，时间: {DateTimeOffset.Now}", typeof(Worker));
                await _typhoonWorkerService.SaveTyphoonData();
                LoggerUtils.Info($"台风数据收集完成，时间: {DateTimeOffset.Now}", typeof(Worker));
            }
            catch (Exception ex)
            {
                LoggerUtils.Error($"台风数据收集异常，时间: {DateTimeOffset.Now}", typeof(Worker), ex);
            }
        }

        private async Task SaveWeatherDataAsync()
        {
            try
            {
                LoggerUtils.Info($"气象数据收集开始，时间: {DateTimeOffset.Now}", typeof(Worker));
                await _weatherWorkerService.SavePXYWeatherByPoint();
                LoggerUtils.Info($"气象数据收集完成，时间: {DateTimeOffset.Now}", typeof(Worker));
            }
            catch (Exception ex)
            {
                LoggerUtils.Error($"气象数据收集异常，时间: {DateTimeOffset.Now}", typeof(Worker), ex);
            }
        }
    }
}
