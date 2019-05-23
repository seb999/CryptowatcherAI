using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using cryptowatcherAI.Class;
using cryptowatcherAI.Misc;
using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;
using static CryptowatcherAI.Class.Prediction;

namespace cryptowatcherAI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("press 0 to create new csv from binance API");
            Console.WriteLine("press 1 to create csv from all binance coin for specified market");
            Console.WriteLine("press 2 to create and save new model");
            var userEntry = Console.ReadLine();

            if (userEntry == "0")
            {
                Console.WriteLine("############ Create csv for allcoin!!! ###########");
                Console.WriteLine("Enter market : USDT or BNB or BTC ?");
                var market = Console.ReadLine();
                List<string> symbolList = BinanceMarket.GetSymbolList(market);
                foreach (var coinName in symbolList)
                {
                    CreateCsv(coinName);
                }
            }

            if (userEntry == "1")
            {
                Console.WriteLine("############ Create csv ###########");
                Console.WriteLine("Enter valide coin pair value");
                var coin = Console.ReadLine();
                CreateCsv(coin);
            }


            if (userEntry == "2")
            {
                Console.WriteLine("############ Create and save new model ###########");
                Console.ReadLine();

                //List all csv available
                var rootFolder = Environment.CurrentDirectory + "/Csv/";
                var modelPathList = Directory.GetFiles(rootFolder, "*", SearchOption.AllDirectories);
                foreach (var coinPath in modelPathList)
                {
                    CreateModel(coinPath);
                }

                Console.WriteLine("Models completed, press any key to exit....");
                Console.ReadLine();
            }
        }

        private static void CreateCsv(string symbol)
        {
            //0 - Create a StringBuilder output
            var csv = new StringBuilder();

            //1 - Add header to output csv
            PropertyInfo[] propertyInfos;
            propertyInfos = typeof(CoinTransfer).GetProperties();
            var headerLine = "";
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                headerLine += propertyInfo.Name + ",";
            }
            headerLine = headerLine.Substring(0, headerLine.Length - 1);
            csv.Append(headerLine);
            csv.AppendLine();

            //2-Actract data from Binance API and push to output
            List<CoinTransfer> binanceData = BinanceMarket.GetCoin(symbol, "1h");

            foreach (var ticker in binanceData)
            {
                csv.Append(ticker.OpenTime + "," +
                ticker.Open.ToString().Replace(",", ".") + "," +
                ticker.High.ToString().Replace(",", ".") + "," +
                ticker.Low.ToString().Replace(",", ".") + "," +
                ticker.Close.ToString().Replace(",", ".") + "," +
                ticker.Volume.ToString().Replace(",", ".") + "," +
                ticker.CloseTime + "," +
                ticker.QuoteAssetVolume.ToString().Replace(",", ".") + "," +
                ticker.NumberOfTrades.ToString().Replace(",", ".") + "," +
                ticker.BuyBaseAssetVolume.ToString().Replace(",", ".") + "," +
                ticker.BuyQuoteAssetVolume.ToString().Replace(",", ".") + "," +
                ticker.Ignore.ToString().Replace(",", ".") + "," +
                ticker.Rsi.ToString().Replace(",", ".") + "," +
                ticker.Macd.ToString().Replace(",", ".") + "," +
                ticker.MacdSign.ToString().Replace(",", ".") + "," +
                ticker.MacdHist.ToString().Replace(",", ".") + "," +
                ticker.FuturePrice.ToString().Replace(",", "."));

                csv.AppendLine();
            }

            //3 - Create output name
            string resultFileName = string.Format("{0}-TrainData.csv", symbol);

            //4 - save file to drive
            File.WriteAllText(resultFileName, csv.ToString());

            //3-save csv and print the file name
            Console.WriteLine(resultFileName);
            Console.ReadLine();
        }

        private static void CreateModel(string sourcePath)
        {

            ITransformer model;
            MLContext mlContext = new MLContext();

            //1 - Load data from csv
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<CoinData>(path: sourcePath, hasHeader: true, separatorChar: ',');
            //2 - Create pipeline
            var pipeline1 = CreatePipeline(mlContext).Append(mlContext.Regression.Trainers.FastForest()); ;
            //3 - Train your model based on the data set
            model = pipeline1.Fit(trainingDataView);
            //4: We save the model
            SaveModelAsFile(mlContext, model, sourcePath, "Fast Forest");

            var pipeline2 = CreatePipeline(mlContext).Append(mlContext.Regression.Trainers.FastTree()); ;
            model = pipeline2.Fit(trainingDataView);
            SaveModelAsFile(mlContext, model, sourcePath, "Fast Tree");

            var pipeline3 = CreatePipeline(mlContext).Append(mlContext.Regression.Trainers.FastTreeTweedie()); ;
            model = pipeline3.Fit(trainingDataView);
            SaveModelAsFile(mlContext, model, sourcePath, "Fast Tree Tweedie");

            var pipeline4 = CreatePipeline(mlContext).Append(mlContext.Regression.Trainers.GeneralizedAdditiveModels()); ;
            model = pipeline4.Fit(trainingDataView);
            SaveModelAsFile(mlContext, model, sourcePath, "Additive Model");

            try
            {
                var pipeline5 = CreatePipeline(mlContext).Append(mlContext.Regression.Trainers.OnlineGradientDescent()); ;
                model = pipeline5.Fit(trainingDataView);
                SaveModelAsFile(mlContext, model, sourcePath, "Gradient Descent");
            }
            catch (System.Exception)
            {
                
            }

            var pipeline7 = CreatePipeline(mlContext).Append(mlContext.Regression.Trainers.StochasticDualCoordinateAscent()); ;
            model = pipeline7.Fit(trainingDataView);
            SaveModelAsFile(mlContext, model, sourcePath, "Stochastic dual Coordinate");

            //     // STEP 5: We load the model FOR DEBUGGING
            //     ITransformer loadedModel;
            //    loadedModel = LoadModelFromFile(mlContext, sourcePath, "Fast Forest");

            //     // FINAL STEP: we do a prediction based on the model generated privously
            //     var predictionFunction = mlContext.Model.CreatePredictionEngine<CoinData, CoinPrediction>(loadedModel);
            //     CoinPrediction prediction = predictionFunction.Predict(new CoinData
            //     {
            //         Volume = (float)83.825741,
            //         Open = (float)4136.48,
            //         Rsi = (float)51.72,
            //         MacdHist = (float)-2.01
            //     });
        }

        #region helper

        private static EstimatorChain<ColumnConcatenatingTransformer> CreatePipeline(MLContext mlContext)
        {
            return mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(CoinData.FuturePrice)) //the output with LABEL as name
            .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Volume", inputColumnName: nameof(CoinData.Volume)))
            .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Open", inputColumnName: nameof(CoinData.Open)))
            .Append(mlContext.Transforms.CopyColumns(outputColumnName: "MacdHist", inputColumnName: nameof(CoinData.MacdHist)))
            .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Rsi", inputColumnName: nameof(CoinData.Rsi)))
            .Append(mlContext.Transforms.Concatenate("Features", "Volume", "Open", "Rsi", "MacdHist"));
        }
        private static void SaveModelAsFile(MLContext mlContext, ITransformer model, string sourcePath, string modelType)
        {
            var fileName = Path.GetFileName(sourcePath);
            var symbol = fileName.Substring(0, fileName.IndexOf("-"));
            var modelPath = string.Format("{0}\\MODEL\\{1}-{2}.zip", Environment.CurrentDirectory, symbol, modelType);

            using (var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(model, fileStream);
        }

        private static ITransformer LoadModelFromFile(MLContext mlContext, string sourcePath, string modelType)
        {
            var fileName = Path.GetFileName(sourcePath);
            var symbol = fileName.Substring(0, fileName.IndexOf("-"));
            var modelPath = string.Format("{0}\\MODEL\\{1}-{2}.zip", Environment.CurrentDirectory, symbol, modelType);

            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return mlContext.Model.Load(stream);
            }
        }

        #endregion
    }
}
