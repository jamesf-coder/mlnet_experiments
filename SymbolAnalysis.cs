// Inspired by: https://learn.microsoft.com/en-us/dotnet/machine-learning/tutorials/image-classification-api-transfer-learning

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.ML;
using static Microsoft.ML.DataOperationsCatalog;
// using Microsoft.ML.Vision;

class ImageData
{
    public string ImagePath { get; set; }

    public string Label { get; set; }
}

class ModelInput
{
    public byte[] Image { get; set; }
    
    public UInt32 LabelAsKey { get; set; }

    public string ImagePath { get; set; }

    public string Label { get; set; }
}

class ModelOutput
{
    public string ImagePath { get; set; }

    public string Label { get; set; }

    public string PredictedLabel { get; set; }
}

namespace SymbolAnalysis
{
    public class Analyser
    {
        private MLContext mLContext = new MLContext();

        public void Train()
        {
            Console.WriteLine("Training...");

            var mo = new ModelOutput();
        }

        IEnumerable<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameAsLabel = true)
        {

        }
    }
}