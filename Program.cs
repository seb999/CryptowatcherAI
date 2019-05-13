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

namespace cryptowatcherAI
{
    class Program
    {
        // STEP 1: Define your data structures
        // - Price is what you are predicting, and is only set when training
        public class BevrageData
        {
            [LoadColumn(0)]
            public string FullName { get; set; }

            [LoadColumn(1)]
            public float Price { get; set; }

            [LoadColumn(2)]
            public float Volume { get; set; }

            [LoadColumn(3)]
            public string Type { get; set; }

            [LoadColumn(4)]
            public string Country { get; set; }
        }

        public class BevragePrediction
        {
            [ColumnName("Score")]
            public float Price { get; set; }
        }

        static void Main(string[] args)
        {
            //Debug
            //BinanceMarket.GetCoin("BTCUSDT", "2h");

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
                CreateTrainSaveModel();
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
            //File.WriteAllText(@"C:\Temp\" + resultFileName, csv.ToString());  //For Windows
            File.WriteAllText(resultFileName, csv.ToString());  //For Mac

            //3-save csv and print the file name
            Console.WriteLine(resultFileName);
            Console.ReadLine();
        }

        private static void CreateTrainSaveModel()
        {
            // STEP 2: Create a ML.NET environment
            MLContext mlContext = new MLContext();

            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<BevrageData>(path: "train-data.csv", hasHeader: true, separatorChar: ',');

            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(BevrageData.Price)) //the output with LABEL as name
             .Append(mlContext.Transforms.CopyColumns(outputColumnName: "CatVolume", inputColumnName: nameof(BevrageData.Volume)))
             .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CatFeaturesType", inputColumnName: nameof(BevrageData.Type))) //convert string into numeric
             .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CatFeaturesCountry", inputColumnName: nameof(BevrageData.Country))) //convert string into numeric
             .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "CatFullName", inputColumnName: nameof(BevrageData.FullName))) //convert string into numeric
             .Append(mlContext.Transforms.Concatenate("Features", "CatVolume", "CatFullName", "CatFeaturesType", "CatFeaturesCountry") //concat all
             .Append(mlContext.Regression.Trainers.FastTree()));
            //.Append(mlContext.Regression.Trainers.PoissonRegression(labelColumnName: "Label", featureColumnName: "Features"));

            // STEP 3: Train your model based on the data set
            ITransformer model = pipeline.Fit(trainingDataView);

            // STEP 4: We save the model
            SaveModelAsFile(mlContext, model);

            // STEP 5: We load the model 
            ITransformer loadedModel;
            using (var stream = new FileStream(@"C:\Users\sdubos\Desktop\toto.zip", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                loadedModel = mlContext.Model.Load(stream);
            }

            // STEP 5: Use your model to make a prediction
            IEnumerable<BevrageData> drinks = new[]
           {
                new BevrageData { FullName="Cheap Lager", Type="Öl", Volume=500, Country="Sverige" },
                new BevrageData { FullName="Dummy Weiss", Type="Öl", Volume=500, Country="Tyskland" },
                new BevrageData { FullName="New Trappist", Type="Öl", Volume=330, Country="Belgien" },
                new BevrageData { FullName="Mortgage 10 years", Type="Whisky", Volume=700, Country="Storbritannien" },
                new BevrageData { FullName="Mortgage 21 years", Type="Whisky", Volume=700, Country="Storbritannien" },
                new BevrageData { FullName="Merlot Classic", Type="Rött vin", Volume=750, Country="Frankrike" },
                new BevrageData { FullName="Merlot Grand Cru", Type="Rött vin", Volume=750, Country="Frankrike" },
                new BevrageData { FullName="Palinka", Type="Likör", Volume=750, Country="Romania" }
            };

            // FINAL STEP: we do a prediction based on the model generated privously
            var predictionFunction = mlContext.Model.CreatePredictionEngine<BevrageData, BevragePrediction>(loadedModel);
            var prediction = predictionFunction.Predict(new BevrageData { FullName = "Cheap Lager", Type = "Öl", Volume = 500, Country = "Sverige" });


            Console.WriteLine("Press any key to exit....");
            Console.ReadLine();
        }

        private static void SaveModelAsFile(MLContext mlContext, ITransformer model)
        {
            using (var fileStream = new FileStream(@"C:\Users\sdubos\Desktop\toto.zip", FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(model, fileStream);
        }

    }
}
