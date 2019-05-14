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
            CreateModel("BTCUSDT");

            Console.WriteLine("press 0 to create new csv from binance API");
            Console.WriteLine("press 1 to create and save new model");
            var userEntry = Console.ReadLine();

            if (userEntry == "0")
            {
                Console.WriteLine("############ Create csv ###########");
                Console.WriteLine("Enter valide coin pair value");
                var coin = Console.ReadLine();
                CreateCsv(coin);
            }

            if (userEntry == "1")
            {
                Console.WriteLine("############ Create and save new model ###########");
                Console.WriteLine("Enter valide coin pair value");
                var coin = Console.ReadLine();
                CreateModel(coin);
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
            List<CoinTransfer> binanceData = BinanceMarket.GetCoin(symbol, "2h");

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
                ticker.RSI.ToString().Replace(",", ".") + "," +
                ticker.MACD.ToString().Replace(",", ".") + "," +
                ticker.MACDSign.ToString().Replace(",", ".") + "," +
                ticker.MACDHist.ToString().Replace(",", ".") + "," +
                ticker.Change.ToString().Replace(",", "."));

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

        private static void CreateModel(string symbol)
        {
            string sourcePath = string.Format("{0}-TrainData.csv", symbol);
            string modelPath = "";
            ITransformer model;
            MLContext mlContext = new MLContext();

            //1 - Load data from csv
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<CoinData>(path: sourcePath, hasHeader: true, separatorChar: ',');

            //2 - Define pipeline
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(CoinData.Change)) //the output with LABEL as name
             .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Volume", inputColumnName: nameof(CoinData.Volume)))
             .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Open", inputColumnName: nameof(CoinData.Open)))
             .Append(mlContext.Transforms.CopyColumns(outputColumnName: "MACDHist", inputColumnName: nameof(CoinData.MACDHist)))
            .Append(mlContext.Transforms.Concatenate("Features", "Volume", "Open", "RSI", "MACDHist"))//concat all
            .Append(mlContext.Regression.Trainers.FastForest());

            //3 - Train your model based on the data set
            model = pipeline.Fit(trainingDataView);
            modelPath = string.Format("{0}-{1}.zip", symbol, "Fast Forest"); 
            // STEP 4: We save the model
            SaveModelAsFile(mlContext, model, modelPath);

        
            var pipeline2 = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(CoinData.Change)) //the output with LABEL as name
             .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Volume", inputColumnName: nameof(CoinData.Volume)))
             .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Open", inputColumnName: nameof(CoinData.Open)))
             .Append(mlContext.Transforms.CopyColumns(outputColumnName: "MACDHist", inputColumnName: nameof(CoinData.MACDHist)))
            .Append(mlContext.Transforms.Concatenate("Features", "Volume", "Open", "RSI", "MACDHist"))//concat all
            .Append(mlContext.Regression.Trainers.FastTree());
            model = pipeline2.Fit(trainingDataView);
            modelPath = string.Format("{0}-{1}.zip", symbol, "Fast Tree"); 
            // STEP 4: We save the model
            SaveModelAsFile(mlContext, model, modelPath);

             var pipeline3 = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(CoinData.Change)) //the output with LABEL as name
             .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Volume", inputColumnName: nameof(CoinData.Volume)))
             .Append(mlContext.Transforms.CopyColumns(outputColumnName: "Open", inputColumnName: nameof(CoinData.Open)))
             .Append(mlContext.Transforms.CopyColumns(outputColumnName: "MACDHist", inputColumnName: nameof(CoinData.MACDHist)))
            .Append(mlContext.Transforms.Concatenate("Features", "Volume", "Open", "RSI", "MACDHist"))//concat all
            .Append(mlContext.Regression.Trainers.OnlineGradientDescent());
            model = pipeline2.Fit(trainingDataView);
            modelPath = string.Format("{0}-{1}.zip", symbol, "Gradient Descent"); 
            // STEP 4: We save the model
            SaveModelAsFile(mlContext, model, modelPath);





            // STEP 5: We load the model 
            ITransformer loadedModel;
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(stream);
            }

            // FINAL STEP: we do a prediction based on the model generated privously
            var predictionFunction = mlContext.Model.CreatePredictionEngine<CoinData, CoinPrediction>(loadedModel);
            CoinPrediction prediction = predictionFunction.Predict(new CoinData
            {
                Volume = (float)83.825741,
                Open = (float)4136.48,
                RSI = (float)51.72,
                MACDHist = (float)-2.01
            });

            //Metrics
            // IDataView dataView = mlContext.Data.LoadFromTextFile<CoinData>("testMe.csv", hasHeader: false, separatorChar: ',');
            // var predictions = model.Transform(dataView);
            // var metrics = mlContext.Regression.Evaluate(predictions, "Label", "Score");

            Console.WriteLine("Models completed, press any key to exit....");
            Console.ReadLine();
        }

        #region helper

        private static void SaveModelAsFile(MLContext mlContext, ITransformer model, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(model, fileStream);
        }

        #endregion
    }
}
