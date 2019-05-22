using Microsoft.ML.Data;

namespace CryptowatcherAI.Class
{
    public class Prediction
    {
        public class CoinData
        {
            [LoadColumn(0)]
            public float OpenTime { get; set; }
            [LoadColumn(1)]
            public float Open { get; set; }
            [LoadColumn(2)]
            public float High { get; set; }
            [LoadColumn(3)]
            public float Low { get; set; }
            [LoadColumn(4)]
            public float Close { get; set; }
            [LoadColumn(5)]
            public float Volume { get; set; }
            [LoadColumn(6)]
            public float CloseTime { get; set; }
            [LoadColumn(7)]
            public float QuoteAssetVolume { get; set; }
            [LoadColumn(8)]
            public float NumberOfTrades { get; set; }
            [LoadColumn(9)]
            public float BuyBaseAssetVolume { get; set; }
            [LoadColumn(10)]
            public float BuyQuoteAssetVolume { get; set; }
            [LoadColumn(11)]
            public float Ignore { get; set; }
            [LoadColumn(12)]
            public float RSI { get; set; }
            [LoadColumn(13)]
            public float MACD { get; set; }
            [LoadColumn(14)]
            public float MACDSign { get; set; }
            [LoadColumn(15)]
            public float MACDHist { get; set; }
            [LoadColumn(16)]
            public float FuturePrice { get; set; }
        }

        public class CoinPrediction
        {
            [ColumnName("Score")]
            public float FuturePrice { get; set; }
        }

    }
}