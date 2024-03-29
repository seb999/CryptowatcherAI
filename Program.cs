﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using cryptowatcherAI.Class;
using cryptowatcherAI.Misc;
using Microsoft.ML;
using Microsoft.ML.Data;
using static CryptowatcherAI.Class.Prediction;

namespace cryptowatcherAI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("press 0 to create new csv from all coin");
            Console.WriteLine("press 1 to create csv for specified pair");
            Console.WriteLine("press 2 to create and save all model");
            Console.WriteLine("press 3 to create and save one model");
            Console.WriteLine("press 4 to test model");
            var userEntry = Console.ReadLine();

            if (userEntry == "0")
            {
                Console.WriteLine("############ Create csv for allcoin!!! ###########");
                Console.WriteLine("Enter market : USDT or BNB or BTC ?");
                var market = Console.ReadLine();
                //Get existing csv list
                var rootFolder = Environment.CurrentDirectory + "/Csv/";
                var csvList = Directory.GetFiles(rootFolder, "*", SearchOption.AllDirectories);

                List<string> symbolList = BinanceMarket.GetSymbolList(market);
                foreach (var coinName in symbolList)
                {
                    bool isCsvExisting = false;
                    foreach (var csvPath in csvList)
                    {
                        if (Path.GetFileName(csvPath).IndexOf("-") < 0) continue;
                        if (Path.GetFileName(csvPath).Substring(0, (Path.GetFileName(csvPath).IndexOf("-"))) == coinName) isCsvExisting = true;
                    }
                    if (!isCsvExisting) CreateCsv(coinName);
                }
            }

            if (userEntry == "1")
            {
                Console.WriteLine("############ Create csv ###########");
                Console.WriteLine("Enter valide coin pair value: ex: BTCUSDT");
                var coin = Console.ReadLine();
                //CreateCsv(coin);
                CreateCsv("BTCUSDT");
                Console.ReadLine();
            }


            if (userEntry == "2")
            {
                Console.WriteLine("############ Create and save all model ###########");

                //List all csv available
                var csvFolder = Environment.CurrentDirectory + "/CSV/";
                var csvList = Directory.GetFiles(csvFolder, "*", SearchOption.AllDirectories);

                var modelFolder = Environment.CurrentDirectory + "/MODEL/";
                var modelList = Directory.GetFiles(modelFolder, "*", SearchOption.AllDirectories);

                foreach (var csvItem in csvList)
                {
                    bool isCsvExisting = false;
                    foreach (var modelItem in modelList)
                    {
                        if (Path.GetFileName(modelItem).Substring(0, (Path.GetFileName(modelItem).IndexOf("-"))) ==
                            Path.GetFileName(csvItem).Substring(0, (Path.GetFileName(csvItem).IndexOf("-")))) isCsvExisting = true;
                    }
                    if (!isCsvExisting) CreateModel(csvItem);
                }

                Console.WriteLine("Models completed, press any key to exit....");
                Console.ReadLine();
            }

            if (userEntry == "3")
            {
                Console.WriteLine("############ Create and save one model ###########");
                Console.WriteLine("Enter valide coin pair value");
                var coin = Console.ReadLine();

                //List all csv available
                var rootFolder = Environment.CurrentDirectory + "/Csv/";
                var modelPathList = Directory.GetFiles(rootFolder, "*", SearchOption.AllDirectories);

                foreach (var coinPath in modelPathList)
                {
                    var fileName = Path.GetFileName(coinPath);
                    var symbol = fileName.Substring(0, fileName.IndexOf("-"));
                    if (symbol == coin)
                    {
                        CreateModel(coinPath);
                    }
                }

                Console.WriteLine("Models completed, press any key to exit....");
                Console.ReadLine();
            }

            if (userEntry == "4")
            {
                TestModel();
                Console.WriteLine("Metrics");
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
            List<CoinTransfer> binanceData = BinanceMarket.GetCoin(symbol, "15m");

            foreach (var ticker in binanceData)
            {
                csv.Append(ticker.t + "," +
                ticker.o.ToString().Replace(",", ".") + "," +
                ticker.h.ToString().Replace(",", ".") + "," +
                ticker.l.ToString().Replace(",", ".") + "," +
                ticker.c.ToString().Replace(",", ".") + "," +
                ticker.v.ToString().Replace(",", ".") + "," +
                ticker.T + "," +
                ticker.q.ToString().Replace(",", ".") + "," +
                ticker.n.ToString().Replace(",", ".") + "," +
                ticker.V.ToString().Replace(",", ".") + "," +
                ticker.Q.ToString().Replace(",", ".") + "," +
                ticker.B.ToString().Replace(",", ".") + "," +
                ticker.Ema.ToString().Replace(",", ".") + "," +
                ticker.Rsi.ToString().Replace(",", ".") + "," +
                ticker.Macd.ToString().Replace(",", ".") + "," +
                ticker.MacdSign.ToString().Replace(",", ".") + "," +
                ticker.MacdHistN3.ToString().Replace(",", ".") + "," +
                ticker.MacdHistN2.ToString().Replace(",", ".") + "," +
                ticker.MacdHistN1.ToString().Replace(",", ".") + "," +
                ticker.MacdHistN0.ToString().Replace(",", ".") + "," +
                ticker.FuturePrice.ToString().Replace(",", "."));

                csv.AppendLine();
            }

            //3 - Create output file name
            string resultFileName = string.Format("{0}-TrainData.csv", symbol);

            //4 - save file to drive
            var resultFilePath = string.Format("{0}/Csv/{1}", Environment.CurrentDirectory, resultFileName);
            File.WriteAllText(resultFilePath, csv.ToString());

            //3-save csv and print the file name
            Console.WriteLine(resultFileName);
        }

        private static void CreateModel(string sourcePath)
        {
            try
            {
                ITransformer model;
                MLContext mlContext = new MLContext(seed: 0);

                //1 - Load data from csv
                IDataView baseTrainingDataView = mlContext.Data.LoadFromTextFile<CoinData>(path: sourcePath, hasHeader: true, separatorChar: ',');

                //2 - Create pipeline
                var pipeline1 = CreatePipeline(mlContext).Append(mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression()); ;
                model = pipeline1.Fit(baseTrainingDataView);
                SaveModelAsFile(mlContext, model, sourcePath, baseTrainingDataView, "lbfgs");

                var pipeline2 = CreatePipeline(mlContext).Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());
                model = pipeline2.Fit(baseTrainingDataView);
                SaveModelAsFile(mlContext, model, sourcePath, baseTrainingDataView, "sda");

                var pipeline3 = CreatePipeline(mlContext).Append(mlContext.BinaryClassification.Trainers.LdSvm());
                model = pipeline3.Fit(baseTrainingDataView);
                SaveModelAsFile(mlContext, model, sourcePath, baseTrainingDataView, "LdSvm");
            }
            catch (System.Exception e)
            {
               Console.WriteLine(e);
            }
            finally
            {
                //;
            }

        }

        private static void TestModel()
        {
            CoinData testData = new CoinData
            {
                // v = (float)83.825741,
                Rsi = (float)60.89498,
                MacdHistN3 = (float)-0.46286118,
                MacdHistN2 = (float)-8.640091,
                MacdHistN1 = (float)-13.21102,
                MacdHistN0 = (float)-16.47304,
            };

            ITransformer model;
            MLContext mlContext = new MLContext(seed: 0);

            var rootFolder = Environment.CurrentDirectory + "/MODEL";
            var modelPathList = Directory.GetFiles(rootFolder, "*", SearchOption.AllDirectories);
            foreach (var modelPath in modelPathList)
            {
                if (Path.GetFileName(modelPath).IndexOf("-") < 0) continue;

                ITransformer trainedModel = mlContext.Model.Load(modelPath, out var modelInputSchema);

                // Create prediction engine related to the loaded trained model
                var predEngine = mlContext.Model.CreatePredictionEngine<CoinData, CoinPrediction>(trainedModel);

                //Score
                var resultprediction = predEngine.Predict(testData);
            }

            // // STEP 5: We load the model FOR DEBUGGING




            // loadedModel = LoadModelFromFile(mlContext, sourcePath, "Fast Forest");

            // //FINAL STEP: we do a prediction based on the model generated privously
            // var predictionFunction = mlContext.Model.CreatePredictionEngine<CoinData, CoinPrediction>(loadedModel);
            // CoinPrediction prediction = predictionFunction.Predict(new CoinData
            // {
            //     v = (float)83.825741,
            //     c = (float)4136.48,
            //     Rsi = (float)51.72,
            //     MacdHist = (float)-2.01,
            //     Ema = (float)4136.48,
            // });
        }

        #region helper

        private static EstimatorChain<ColumnConcatenatingTransformer> CreatePipeline(MLContext mlContext)
        {
            return mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(CoinData.FuturePrice)) //the output with LABEL as name

            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CoinData.Rsi)))
            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CoinData.MacdHistN3)))
            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CoinData.MacdHistN2)))
            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CoinData.MacdHistN1)))
            .Append(mlContext.Transforms.NormalizeMeanVariance(outputColumnName: nameof(CoinData.MacdHistN0)))
            .Append(mlContext.Transforms.Concatenate("Features",
                                        nameof(CoinData.Rsi), 
                                        nameof(CoinData.MacdHistN3), 
                                        nameof(CoinData.MacdHistN2), 
                                        nameof(CoinData.MacdHistN1), 
                                        nameof(CoinData.MacdHistN0)));
        }
        private static void SaveModelAsFile(MLContext mlContext, ITransformer model, string sourcePath, IDataView trainingDataView, string modelType)
        {
            var fileName = Path.GetFileName(sourcePath);
            var symbol = fileName.Substring(0, fileName.IndexOf("-"));
            var modelPath = string.Format("{0}/MODEL/{1}-{2}.zip", Environment.CurrentDirectory, symbol, modelType);

            using (var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(model, trainingDataView.Schema, fileStream);
        }

        private static ITransformer LoadModelFromFile(MLContext mlContext, string sourcePath, string modelType)
        {
            var fileName = Path.GetFileName(sourcePath);
            var symbol = fileName.Substring(0, fileName.IndexOf("-"));
            var modelPath = string.Format("{0}\\MODEL\\{1}-{2}.zip", Environment.CurrentDirectory, symbol, modelType);

            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return null;
                //return mlContext.Model.Load(stream);
            }
        }

        #endregion
    }
}
