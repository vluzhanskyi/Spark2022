namespace PlaybackPredictionWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var predictor = new Predictor(_logger);
                predictor.RunPrediction();
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}