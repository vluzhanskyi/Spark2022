
using Microsoft.ML;
using Microsoft.ML.Trainers;

Console.WriteLine("Hello, World!");

MLContext mlContext = new MLContext();

(IDataView trainingDataView, IDataView testDataView) = LoadData(mlContext);

ITransformer model = BuildAndTrainModel(mlContext, trainingDataView);

EvaluateModel(mlContext, testDataView, model);
UseModelForSinglePrediction(mlContext, model);
SaveModel(mlContext, trainingDataView.Schema, model);


Console.ReadKey();


(IDataView trainingDataView, IDataView testDataView) LoadData(MLContext mlContext)
{
    var trainingDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "interactions-train.csv");
    var testDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "recommend-interactions.csv");

    IDataView trainingDataView = mlContext.Data.LoadFromTextFile<InteractionsData>(trainingDataPath, hasHeader: true, separatorChar: ',');
    IDataView testDataView = mlContext.Data.LoadFromTextFile<InteractionsData>(testDataPath, hasHeader: true, separatorChar: ',');

    return (trainingDataView, testDataView);
}

ITransformer BuildAndTrainModel(MLContext mlContext, IDataView trainingDataView)
{
    IEstimator<ITransformer> estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "AgentIdEncoded", inputColumnName: "AgentId")
        .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "InteractionIdEncoded", inputColumnName: "InteractionId"));

    var options = new MatrixFactorizationTrainer.Options
    {
        MatrixColumnIndexColumnName = "AgentIdEncoded",
        MatrixRowIndexColumnName = "InteractionIdEncoded",
        LabelColumnName = "InteractionId",
        NumberOfIterations = 20,
        ApproximationRank = 100
    };

    var trainerEstimator = estimator.Append(mlContext.Recommendation().Trainers.MatrixFactorization(options));
    Console.WriteLine("=============== Training the model ===============");
    ITransformer model = trainerEstimator.Fit(trainingDataView);

    return model;
}

void EvaluateModel(MLContext mlContext, IDataView testDataView, ITransformer model)
{
    Console.WriteLine("=============== Evaluating the model ===============");
    var prediction = model.Transform(testDataView);
    var metrics = mlContext.Regression.Evaluate(prediction, labelColumnName: "InteractionId", scoreColumnName: "AgentId");
    Console.WriteLine("Root Mean Squared Error : " + metrics.RootMeanSquaredError.ToString());
    Console.WriteLine("RSquared: " + metrics.RSquared.ToString());
}

void UseModelForSinglePrediction(MLContext mlContext, ITransformer model)
{
    Console.WriteLine("=============== Making a prediction ===============");
    var predictionEngine = mlContext.Model.CreatePredictionEngine<InteractionsData, InteractionsPrediction>(model);
    var testInput = new InteractionsData
    {
        Playback_Initiator = 15, 
        InteractionId = 7119854815151194137,
        AgentId = 9,
        Call_duration = 103726767,
        Media_OutputType = (int) MediaOutputType.Default
    };
    var movieRatingPrediction = predictionEngine.Predict(testInput);
    if (Math.Round(movieRatingPrediction.Score, 1) > 1)
    {
        Console.WriteLine("Interaction " + testInput.InteractionId + " is recommended for user " + testInput.Playback_Initiator);
    }
    else
    {
        Console.WriteLine("Interaction " + testInput.InteractionId + " is not recommended for user " + testInput.Playback_Initiator);
    }

}

void SaveModel(MLContext mlContext, DataViewSchema trainingDataViewSchema, ITransformer model)
{
    var modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "RecommenderModel.zip");

    Console.WriteLine("=============== Saving the model to a file ===============");
    mlContext.Model.Save(model, trainingDataViewSchema, modelPath);

}