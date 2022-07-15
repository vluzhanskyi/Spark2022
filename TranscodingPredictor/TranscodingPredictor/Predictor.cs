using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using PlaybackModels;

namespace TranscodingPredictor;

public class Predictor : IPredictionEngine
{
    IPredictionDataLoader dataLoader;
    MLContext mlContext;
    private ILogger log;

    public Predictor(IPredictionDataLoader dbDataLoader, IConfiguration config, ILogger<Predictor> logger)
    {
        dataLoader = dbDataLoader;
        dataLoader.LoadSqlData();
        mlContext = new MLContext();
        log = logger;
    }

    public string RunPrediction()
    {
        (IDataView trainingDataView, IDataView testDataView) = LoadData(mlContext);

        ITransformer model = BuildAndTrainModel(mlContext, trainingDataView);

        EvaluateModel(mlContext, testDataView, model);
        var resultFile = UseModelForSinglePrediction(mlContext, model);
        SaveModel(mlContext, trainingDataView.Schema, model);
        return resultFile;
    }

    (IDataView trainingDataView, IDataView testDataView) LoadData(MLContext mlContext)
    {
        var trainingDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "interactions-train.csv");
        var testDataPath = dataLoader.GetLastPlaybackStatistics();// Path.Combine(Environment.CurrentDirectory, "Data", "recommend-interactions.csv"); 

        IDataView trainingDataView =
            mlContext.Data.LoadFromTextFile<InteractionsData>(trainingDataPath, hasHeader: true, separatorChar: ',');
        IDataView testDataView =
            mlContext.Data.LoadFromTextFile<InteractionsData>(testDataPath, hasHeader: true, separatorChar: ',');

        return (trainingDataView, testDataView);
    }

    ITransformer BuildAndTrainModel(MLContext mlContext, IDataView trainingDataView)
    {
        IEstimator<ITransformer> estimator = mlContext.Transforms.Conversion
            .MapValueToKey(outputColumnName: "AgentIdEncoded", inputColumnName: "AgentId")
            .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "DurationEncoded",
                inputColumnName: "Duration"));
        //.Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "OutputTypeEncoded",
        //    inputColumnName: "OutputType"));

        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = "AgentIdEncoded",
            MatrixRowIndexColumnName = "DurationEncoded",
            LabelColumnName = "Label",
            NumberOfIterations = 80,
            ApproximationRank = 100,
        };

        var trainerEstimator = estimator.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));
        log.LogInformation("=============== Training the model ===============");
        ITransformer model = trainerEstimator.Fit(trainingDataView);

        return model;
    }

    void EvaluateModel(MLContext mlContext, IDataView testDataView, ITransformer model)
    {
        log.LogInformation("=============== Evaluating the model ===============");
        var prediction = model.Transform(testDataView);
        var metrics = mlContext.Regression.Evaluate(prediction, labelColumnName: "Label", scoreColumnName: "Score");
        log.LogInformation("Root Mean Squared Error : " + metrics.RootMeanSquaredError.ToString());
        log.LogInformation("RSquared: " + metrics.RSquared.ToString());
    }

    string UseModelForSinglePrediction(MLContext mlContext, ITransformer model)
    {
        log.LogInformation("=============== Making a prediction ===============");
        var predictionEngine = mlContext.Model.CreatePredictionEngine<InteractionsData, PredictionResult>(model);
        var results = new List<PlaybackStatisticsItem>();

        foreach (InteractionsData testInput in dataLoader.GetNewInteractions())
        {
            var movieRatingPrediction = predictionEngine.Predict(testInput);
            if (movieRatingPrediction.Score > 1)
            {
                log.LogInformation($"Interaction {testInput.InteractionId} is recommended");
                results.Add(testInput);
            }
            else
            {
                Console.WriteLine($"Interaction {testInput.InteractionId} is not recommended for user ");
            }
        }

        return dataLoader.WriteResults(results);
    }

    string SaveModel(MLContext mlContext, DataViewSchema trainingDataViewSchema, ITransformer model)
    {
        var modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "RecommenderModel.zip");

        log.LogInformation("=============== Saving the model to a file ===============");
        mlContext.Model.Save(model, trainingDataViewSchema, modelPath);

        return modelPath;
    }
}