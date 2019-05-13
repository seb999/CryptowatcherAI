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

        public static List<CoinTransfer> GetCoin(string symbol, string interval)
        {
            List<CoinTransfer> quotationHistory = new List<CoinTransfer>();
            string url = "";
            string payload = "";
            double startTime = 0;
            double endTime = 0;
            double previousClose = 0;

            //We take data 10 times 70 days (with interval 2h = 70j * 12 measure / j = 837 measures  )
            for (int i = 10; i >= 1; i--)
            {
                startTime = Math.Round(DateTime.UtcNow.AddDays(-70 * i).Subtract(new DateTime(1970, 1, 1)).TotalSeconds) * 1000;
                endTime = Math.Round(DateTime.UtcNow.AddDays(-70 * (i - 1)).Subtract(new DateTime(1970, 1, 1)).TotalSeconds) * 1000;

                url = string.Format("https://api.binance.com/api/v1/klines?symbol={0}&interval={1}&limit=1000&startTime={2}&endTime={3}",
                    symbol,
                    interval,
                    startTime,
                    endTime);

                //1 - we load a set of data from binance
                payload = HttpHelper.GetApiData(new Uri(url));

                //2 - we desirelize
                var result = JsonConvert.DeserializeObject<List<List<double>>>(payload);

                //3 - We add each item to our final list
                foreach (var item in result)
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
                        Change = Math.Round(item[4]-previousClose,2),
                    };
                    quotationHistory.Add(newQuotation);
                    previousClose = item[4];
                }
            }

            //Add RSI calculation to the list
            TradeIndicator.CalculateRsiList(14, ref quotationHistory);
            TradeIndicator.CalculateMacdList(ref quotationHistory);

            return quotationHistory.ToList();
        }
    }
}