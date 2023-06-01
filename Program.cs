using System;
using MLExperiments;

Console.WriteLine("Hello world.");

var sa = new MLExperiments.SymbolAnalyser();
sa.Train();

sa.TrySinglePrediction("./images/symbols_test/");
