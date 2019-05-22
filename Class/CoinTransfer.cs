using System;
using Microsoft.ML.Data;

namespace cryptowatcherAI.Class
{
    public class CoinTransfer
    {
        public double OpenTime { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public double CloseTime { get; set; }
        public double QuoteAssetVolume { get; set; }
        public double NumberOfTrades { get; set; }
        public double BuyBaseAssetVolume { get; set; }
        public double BuyQuoteAssetVolume { get; set; }
        public double Ignore { get; set; }
        public double Rsi { get; set; }
        public double Macd { get; set; }
        public double MacdSign { get; set; }
        public double MacdHist { get; set; }
        public double FuturePrice { get; set; }
    }
}
