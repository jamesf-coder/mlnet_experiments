// Inspired by: 
// * https://learn.microsoft.com/en-us/dotnet/machine-learning/tutorials/image-classification-api-transfer-learning
// * https://github.com/dotnet/machinelearning-samples/tree/main/samples/csharp/getting-started/DeepLearning_ImageClassification_Training

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
// using ImageClassification.DataModels;
using Microsoft.ML;
using Microsoft.ML.Transforms;
using Microsoft.ML.Vision;
using static Microsoft.ML.Transforms.ValueToKeyMappingEstimator;
using System.Diagnostics;

namespace MLExperiments
{

    public class SymbolAnalyser
    {
        private MLContext mlContext = new MLContext();
        private ITransformer? trainedModel;
        private static string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
        // private static string workspaceRelativePath = Path.Combine(Analyser.projectDirectory, "workspace");
        private static string imagesRelativePath = Path.Combine(projectDirectory, "images/symbols");

        private static string outputMlNetModelFilePath = Path.Combine(projectDirectory, "models");

        public void Train()
        {
            Console.WriteLine("Training...");

            // 1. ML Context
            var mlContext = new MLContext(seed: 1);

            // 2. Load the initial full image-set into an IDataView and shuffle so it'll be better balanced
            IEnumerable<ImageData> images = MLExperiments.Images.LoadImagesFromDirectory(folder: imagesRelativePath, useFolderNameAsLabel: true);
            IDataView fullImagesDataset = mlContext.Data.LoadFromEnumerable(images);
            IDataView shuffledFullImageFilePathsDataset = mlContext.Data.ShuffleRows(fullImagesDataset);


            // 3. Load Images with in-memory type within the IDataView and Transform Labels to Keys (Categorical)
            IDataView shuffledFullImagesDataset = mlContext.Transforms.Conversion.
                    MapValueToKey(outputColumnName: "LabelAsKey", inputColumnName: "Label", keyOrdinality: KeyOrdinality.ByValue)
                .Append(mlContext.Transforms.LoadRawImageBytes(
                                                outputColumnName: "Image",
                                                imageFolder: imagesRelativePath,
                                                inputColumnName: "ImagePath"))
                .Fit(shuffledFullImageFilePathsDataset)
                .Transform(shuffledFullImageFilePathsDataset);
                

            // 4. Split the data 80:20 into train and test sets, train and evaluate.
            var trainTestData = mlContext.Data.TrainTestSplit(shuffledFullImagesDataset, testFraction: 0.2);
            IDataView trainDataView = trainTestData.TrainSet;
            IDataView testDataView = trainTestData.TestSet;

            // 5. Define the model's training pipeline using DNN default values
            //
            var pipeline = mlContext.MulticlassClassification.Trainers
                    .ImageClassification(featureColumnName: "Image",
                                            labelColumnName: "LabelAsKey",
                                            validationSet: testDataView)
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: "PredictedLabel",
                                                                    inputColumnName: "PredictedLabel"));


            // 5.1 (OPTIONAL) Define the model's training pipeline by using explicit hyper-parameters
            // 
            // var options = new ImageClassificationTrainer.Options()
            // {
            //     FeatureColumnName = "Image",
            //     LabelColumnName = "LabelAsKey",
            //     // Just by changing/selecting InceptionV3/MobilenetV2/ResnetV250
            //     // you can try a different DNN architecture (TensorFlow pre-trained model).
            //     Arch = ImageClassificationTrainer.Architecture.MobilenetV2,
            //     Epoch = 50,       //100
            //     BatchSize = 10,
            //     LearningRate = 0.01f,
            //     MetricsCallback = (metrics) => Console.WriteLine(metrics),
            //     ValidationSet = testDataView
            // };

            // var pipeline = mlContext.MulticlassClassification.Trainers.ImageClassification(options)
            //         .Append(mlContext.Transforms.Conversion.MapKeyToValue(
            //             outputColumnName: "PredictedLabel",
            //             inputColumnName: "PredictedLabel"));


            // 4. Train/create the ML model
            trainedModel = pipeline.Fit(trainDataView);

            // 5. Get the quality metrics (accuracy, etc.)
            EvaluateModel(mlContext, testDataView, trainedModel);

            // Save the model to assets/outputs (You get ML.NET .zip model file and TensorFlow .pb model file)
            mlContext.Model.Save(trainedModel, trainDataView.Schema, outputMlNetModelFilePath);

            // IEnumerable<ImageData> images = LoadImagesFromDirectory(folder: assetsRelativePath, useFolderNameAsLabel: true);
            // IDataView imageData = mlContext.Data.LoadFromEnumerable(images);
            // IDataView shuffledData = mlContext.Data.ShuffleRows(imageData);

            // // Machine learning models expect input to be in numerical format. 
            // // Therefore, some preprocessing needs to be done on the data prior 
            // // to training. Create an EstimatorChain made up of the MapValueToKey 
            // // and LoadRawImageBytes transforms. The MapValueToKey transform 
            // // takes the categorical value in the Label column, converts it to 
            // // a numerical KeyType value and stores it in a new column called 
            // // LabelAsKey. The LoadImages takes the values from the ImagePath 
            // // column along with the imageFolder parameter to load images for training.
            // var preprocessingPipeline = mlContext.Transforms.Conversion.MapValueToKey(
            //         inputColumnName: "Label",
            //         outputColumnName: "LabelAsKey")
            //     .Append(mlContext.Transforms.LoadRawImageBytes(
            //         outputColumnName: "Image",
            //         imageFolder: assetsRelativePath,
            //         inputColumnName: "ImagePath"));
                                

            // IDataView preProcessedData = preprocessingPipeline
            //         .Fit(shuffledData)
            //         .Transform(shuffledData);

            // // split 70/30 for training/validation
            // TrainTestData trainSplit = mlContext.Data.TrainTestSplit(data: preProcessedData, testFraction: 0.3);
            // // further split validation 90/10 for validation/testing
            // TrainTestData validationTestSplit = mlContext.Data.TrainTestSplit(trainSplit.TestSet, testFraction: 0.1);

            // IDataView trainSet = trainSplit.TrainSet;
            // IDataView validationSet = validationTestSplit.TrainSet;
            // IDataView testSet = validationTestSplit.TestSet;

            // var classifierOptions = new ImageClassificationTrainer.Options()
            // {
            //     FeatureColumnName = "Image",
            //     LabelColumnName = "LabelAsKey",
            //     ValidationSet = validationSet,
            //     Arch = ImageClassificationTrainer.Architecture.ResnetV2101,
            //     MetricsCallback = (metrics) => Console.WriteLine(metrics),
            //     TestOnTrainSet = false,
            //     ReuseTrainSetBottleneckCachedValues = true,
            //     ReuseValidationSetBottleneckCachedValues = true
            // };            
        }

        private static void EvaluateModel(MLContext mlContext, IDataView testDataset, ITransformer trainedModel)
        {
            Console.WriteLine("Making predictions in bulk for evaluating model's quality...");

            // Measuring time
            var watch = Stopwatch.StartNew();

            var predictionsDataView = trainedModel.Transform(testDataset);

            var metrics = mlContext.MulticlassClassification.Evaluate(predictionsDataView, labelColumnName:"LabelAsKey", predictedLabelColumnName: "PredictedLabel");
            // ConsoleHelper.PrintMultiClassClassificationMetrics("TensorFlow DNN Transfer Learning", metrics);

            watch.Stop();
            var elapsed2Ms = watch.ElapsedMilliseconds;

            Console.WriteLine($"Predicting and Evaluation took: {elapsed2Ms / 1000} seconds");
        }


        public void TrySinglePrediction(string imagesFolderPathForPredictions)
        {
            // Create prediction function to try one prediction
            var predictionEngine = mlContext.Model.CreatePredictionEngine<InMemoryImageData, ImagePrediction>(trainedModel);

            var testImages = FileUtils.LoadInMemoryImagesFromDirectory(imagesFolderPathForPredictions, false);

            var imageToPredict = testImages.First();

            var prediction = predictionEngine.Predict(imageToPredict);

            Console.WriteLine(
                $"Image Filename : [{imageToPredict.ImageFileName}], " +
                $"Scores : [{string.Join(",", prediction.Score)}], " +
                $"Predicted Label : {prediction.PredictedLabel}");
        }
    }
}