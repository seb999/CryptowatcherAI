using System;
using System.Collections.Generic;
using cryptowatcherAI.Class;
using cryptowatcherAI.Misc;
using Newtonsoft.Json;
using System.Linq;

namespace cryptowatcherAI.Misc
{
    public static class BinanceMarket
    {
        public static List<string> GetSymbolList(string baseMarket)
        {
            Uri apiUrl = new Uri("https://api.binance.com/api/v1/ticker/24hr");

            //Get data from Binance API
            List<SymbolTransfer> coinList = HttpHelper.GetApiData<List<SymbolTransfer>>(apiUrl);
            coinList = coinList.Where(p => p.Symbol.Substring(p.Symbol.Length - baseMarket.ToString().Length) == baseMarket.ToString()).Select(p => p).ToList();
            
            return coinList.Select(p => p.Symbol).ToList();
        }

        public static List<CoinTransfer> GetCoin(string symbol, string interval)
        {
            List<CoinTransfer> quotationHistory = new List<CoinTransfer>();
            string apiUrl = "";
            double startTime = 0;
            double endTime = 0;

            //1 measure by min = 480 measure by day so 1000 measure represent 2 days. 360 iteration represente 2 years / 180 iterations is 1 y
            for (int i = 200; i >= 1; i--)
            {
                startTime = Math.Round(DateTime.UtcNow.AddDays(-2 * i).Subtract(new DateTime(1970, 1, 1)).TotalSeconds) * 1000;
                endTime = Math.Round(DateTime.UtcNow.AddDays(-2 * (i - 1)).Subtract(new DateTime(1970, 1, 1)).TotalSeconds) * 1000;

                apiUrl = string.Format("https://api3.binance.com/api/v3/klines?symbol={0}&interval={1}&limit=1000&startTime={2}&endTime={3}",
                    symbol,
                    interval,
                    startTime,
                    endTime);

                //1 - we load a set of data from binance
                List<List<double>> coinQuotation = HttpHelper.GetApiData<List<List<double>>>(new Uri(apiUrl));

                //3 - We add each item to our final list (26 first doesn;t contain RSI neither MACD calulation)
                foreach (var item in coinQuotation)
                {
                    CoinTransfer newQuotation = new CoinTransfer()
                    {
                        t = item[0],
                        o = item[1],
                        h = item[2],
                        l = item[3],
                        c = item[4],
                        v = item[5],
                        T = item[6],
                        q = item[7],
                        n = item[8],
                        V = item[9],
                        Q = item[10],
                        B = item[11],
                    };
                    quotationHistory.Add(newQuotation);
                }
            }

            //Add RSI calculation to the list
            TradeIndicator.CalculateRsiList(14, ref quotationHistory);
            TradeIndicator.CalculateMacdList(ref quotationHistory);
           // TradeIndicator.CalculateEMA50(ref quotationHistory);

            //Calculate change from next day to current day
            quotationHistory.Where((p, index) => CalculateFuturePrice(p, index, quotationHistory)).ToList();

            //we remove the first 50 line : cannot calculate rsi and EMA
            return quotationHistory.SkipLast(5).Skip(50).ToList();
        }

        private static bool CalculateFuturePrice(CoinTransfer p, int index, List<CoinTransfer> quotationHistory)
        {
            if (index >= quotationHistory.Count - 5) return true;
            double averageFuturPrice = 0; 
            for(int i=1;i<=5;i++)
            {
                averageFuturPrice += quotationHistory[index + i].c;
            }
            averageFuturPrice = averageFuturPrice/5;
            p.FuturePrice = averageFuturPrice - p.c;

            return true;
        }
    }
}