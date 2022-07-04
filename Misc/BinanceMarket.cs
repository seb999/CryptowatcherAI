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

            //1 interval = 15min, so 960 measure represent 10 days. 35 iteration represente 1 years. We want 5 years so 35x5 = 175 
            for (int i = 175; i >= 1; i--)
            {
                startTime = Math.Round(DateTime.UtcNow.AddDays(-10 * i).Subtract(new DateTime(1970, 1, 1)).TotalSeconds) * 1000;
                endTime = Math.Round(DateTime.UtcNow.AddDays(-10 * (i - 1)).Subtract(new DateTime(1970, 1, 1)).TotalSeconds) * 1000;

                Console.WriteLine(startTime + "/" + endTime);
                apiUrl = string.Format("https://api3.binance.com/api/v3/klines?symbol={0}&interval={1}&limit=960&startTime={2}&endTime={3}",
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
            if(index<1) return true;
            if(p.c - quotationHistory[index-1].c  > 0)
            {
                p.FuturePrice = 1;
            }
            else{
                p.FuturePrice = 0;
            }

            return true;
        }
    }
}