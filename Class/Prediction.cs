using Microsoft.ML.Data;

namespace CryptowatcherAI.Class
{
    public class Prediction
    {
        public class CoinData
        {
            [LoadColumn(13)]
            public float Rsi { get; set; }

            [LoadColumn(16)]
            public float MacdHistN3 { get; set; }

            [LoadColumn(17)]
            public float MacdHistN2 { get; set; }

            [LoadColumn(18)]
            public float MacdHistN1 { get; set; }

            [LoadColumn(19)]
            public float MacdHistN0 { get; set; }

            [LoadColumn(20)]
            public bool FuturePrice { get; set; }
        }

        public class CoinPrediction
        {
            [ColumnName("PredictedLabel")]
            public bool Prediction { get; set; }
            
            public float Probability;
            public float Score;
        }

    }
}