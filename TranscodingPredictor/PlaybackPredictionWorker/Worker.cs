using DbClient;
using TranscodingPredictor;

namespace PlaybackPredictionWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly IPredictionEngine _predictionEngine;

        public Worker(ILogger<Worker> logger, IConfiguration config, IPredictionEngine predictor)
        {
            _logger = logger;
            _config = config;
            _predictionEngine = predictor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var interactionsFile = _predictionEngine.RunPrediction();
                var nppClient = new NppClient.NppClient(_config, interactionsFile, _logger);
                await nppClient.RequestPredictedCalls();

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}