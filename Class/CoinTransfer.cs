using System;
using Microsoft.ML.Data;

namespace cryptowatcherAI.Class
{
    public class CoinTransfer
    {
        public double t { get; set; }
        public double o { get; set; }
        public double h { get; set; }
        public double l { get; set; }
        public double c { get; set; }
        public double v { get; set; }
        public double T { get; set; }
        public double q { get; set; }
        public double n { get; set; }
        public double V { get; set; }
        public double Q { get; set; }
        public double B { get; set; }
        public double Rsi { get; set; }
        public double Macd { get; set; }
        public double MacdSign { get; set; }
        public double MacdHist { get; set; }
        public double Ema { get; set; }
        public double FuturePrice { get; set; }
    }
}
