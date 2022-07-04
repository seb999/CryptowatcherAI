using System;
using System.Collections.Generic;
using System.Linq;
using cryptowatcherAI.Class;
using TicTacTec.TA.Library;

namespace cryptowatcherAI.Misc
{
    public static class TradeIndicator
    {

        public static void CalculateRsiList(int period, ref List<CoinTransfer> quotationList)
        {
            var data = quotationList.Select(p => p.c).ToArray();
            int beginIndex;
            int outNBElements;
            double[] rsiValues = new double[data.Length];

            var returnCode = Core.Rsi(0, data.Length - 1, data, 14, out beginIndex, out outNBElements, rsiValues);

            if (returnCode == Core.RetCode.Success && outNBElements > 0)
            {
                for (int i = 0; i <= outNBElements - 1; i++)
                {
                    quotationList[i + beginIndex].Rsi = rsiValues[i];
                }
            }
        }

        public static void CalculateMacdList(ref List<CoinTransfer> quotationList)
        {
            var data = quotationList.Select(p => p.c).ToArray();
            int beginIndex;
            int outNBElements;
            double[] outMACD = new double[data.Length];
            double[] outMACDSignal = new double[data.Length];
            double[] outMACDHist = new double[data.Length];

            var status = Core.Macd(0, data.Length - 1, data, 12, 26, 9, out beginIndex, out outNBElements, outMACD, outMACDSignal, outMACDHist);

            if (status == Core.RetCode.Success && outNBElements > 0)
            {
                var macdMax = outMACD.Max();
                var macdMin = outMACD.Min();
                var macdSignMax = outMACDSignal.Max();
                var macdSignMin = outMACDSignal.Min();

                //we normalise the MACD in order to use different coins with same data
                for (int i = 0; i < quotationList.Count - 33; i++)
                {
                    quotationList[i + 33].Macd =  ((outMACD[i] - macdMin) / (macdMax- macdMin))*100;
                    quotationList[i + 33].MacdSign =  ((outMACDSignal[i] - macdSignMin) / (macdSignMax - macdSignMin))*100;
                    quotationList[i + 33].MacdHistN0 =  quotationList[i + 33].Macd - quotationList[i + 33].MacdSign;
                    // quotationList[i + 33].Macd = outMACD[i];
                    // quotationList[i + 33].MacdHist = outMACDHist[i];
                    // quotationList[i + 33].MacdSign = outMACDSignal[i];
                }

                //we stick N-3, N-2, N-1 MACDHist to each item
                for(int i=3;i<quotationList.Count;i++)
                {
                     quotationList[i].MacdHistN1 = quotationList[i-1].MacdHistN0;
                     quotationList[i].MacdHistN2 = quotationList[i-2].MacdHistN0;
                     quotationList[i].MacdHistN3 = quotationList[i-3].MacdHistN0;
                }
            }
        }

        public static void CalculateEMA50(ref List<CoinTransfer> quotationList)
        {
            var data = quotationList.Select(p => p.c).ToArray();
            int beginIndex;
            int outNBElements;
            double[] emaValues = new double[data.Length];

            var statusEma = Core.Ema(0, data.Length - 1, data, 50, out beginIndex, out outNBElements, emaValues);
            if (statusEma == Core.RetCode.Success && outNBElements > 0)
            {
                for (int i = 0; i < outNBElements; i++)
                {
                    quotationList[i + beginIndex].Ema = emaValues[i];
                }
            }
        }
    }
}