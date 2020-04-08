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

            //We take data 10 times 70 days (with interval 1h = 80 * 12 measure / j = 960 measures  )
            for (int i = 10; i >= 1; i--)
            {
                startTime = Math.Round(DateTime.UtcNow.AddDays(-80 * i).Subtract(new DateTime(1970, 1, 1)).TotalSeconds) * 1000;
                endTime = Math.Round(DateTime.UtcNow.AddDays(-80 * (i - 1)).Subtract(new DateTime(1970, 1, 1)).TotalSeconds) * 1000;

                apiUrl = string.Format("https://api.binance.com/api/v1/klines?symbol={0}&interval={1}&limit=1000&startTime={2}&endTime={3}",
                    symbol,
                    interval,
                    startTime,
                    endTime);

                //1 - we load a set of data from binance
                // payload = HttpHelper.GetApiData(new Uri(url));
                List<List<double>> coinQuotation = HttpHelper.GetApiData<List<List<double>>>(new Uri(apiUrl));

                //3 - We add each item to our final list (26 first doesn;t contain RSI neither MACD calulation)
                foreach (var item in coinQuotation)
                {
                    CoinTransfer newQuotation = new CoinTransfer()
                    {
                        OpenTime = item[0],
                        Open = item[1],
                        High = item[2],
                        Low = item[3],
                        Close = item[4],
                        Volume = item[5],
                        CloseTime = item[6],
                        QuoteAssetVolume = item[7],
                        NumberOfTrades = item[8],
                        BuyBaseAssetVolume = item[9],
                        BuyQuoteAssetVolume = item[10],
                        Ignore = item[11],
                    };
                    quotationHistory.Add(newQuotation);
                }
            }

            //Calculate change from next day to current day
            quotationHistory.Where((p, index) => CalculateFuturePrice(p, index, quotationHistory)).ToList();

            //Add RSI calculation to the list
            TradeIndicator.CalculateRsiList(14, ref quotationHistory);
            TradeIndicator.CalculateMacdList(ref quotationHistory);

            return quotationHistory.Skip(26).ToList();
        }

        private static bool CalculateFuturePrice(CoinTransfer p, int index, List<CoinTransfer> quotationHistory)
        {
            if (index == quotationHistory.Count - 1) return true;
            p.FuturePrice = quotationHistory[index + 1].Close - p.Close;
            return true;
        }
    }
}