using DbClient;
using PlaybackPredictionWorker;
using TranscodingPredictor;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IDbDataProvider, DbDataProvider>();
        services.AddSingleton<IPredictionDataLoader, DataLoader>();
        services.AddHostedService<Worker>()
            .AddSingleton<IPredictionEngine, Predictor>();
    })
    .Build();

await host.RunAsync();
