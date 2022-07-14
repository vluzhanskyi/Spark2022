namespace PlaybackPredictionWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;

        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var predictor = new Predictor(_logger);
                var interactionsFile = predictor.RunPrediction();

                var nppClient = new NppClient.NppClient(_config, interactionsFile, _logger);
                await nppClient.RequestPredictedCalls();

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}