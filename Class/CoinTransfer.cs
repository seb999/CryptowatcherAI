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
        public double Ema { get; set; }
        public double Rsi { get; set; } //13
        public double Macd { get; set; } //14
        public double MacdSign { get; set; } //15
        public double MacdHistN3 { get; set; } //16
        public double MacdHistN2 { get; set; } //17
        public double MacdHistN1 { get; set; } //18
        public double MacdHistN0 { get; set; } //19
        public double FuturePrice { get; set; }
    }
}
