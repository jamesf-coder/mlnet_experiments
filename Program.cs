using System;
using Microsoft.ML;

using SymbolAnalysis;

namespace TestML
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Hello world.");

            var sa = new Analyser();
            sa.Train();

        }
    }

}
