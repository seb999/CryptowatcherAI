using System;

namespace cryptowatcherAI
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
        public double RSI { get; set; }
        public double MACD { get; set; }
        public double MACDSign { get; set; }
        public double MACDHist { get; set; }
    }
}
