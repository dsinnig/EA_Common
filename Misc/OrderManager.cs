using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NQuotes;

namespace biiuse
{
    public enum ErrorType
    { ///Rename to OrderManager
        NO_ERROR,
        RETRIABLE_ERROR,
        NON_RETRIABLE_ERROR
    };

    public class OrderManager
    {

        public static bool existsActiveOrderWithMagicNumber(int magic, MqlApi mql4)
        {
            for (int cnt = mql4.OrdersTotal(); cnt > 0; cnt--)
            {
                if (mql4.OrderSelect(cnt, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES))
                {
                    if (mql4.OrderSymbol() != mql4.Symbol()) continue;
                    if (mql4.OrderClosePrice() == 0) continue;
                    if (mql4.OrderMagicNumber() == magic) return true;
                }
            }
            return false;
        }

        public static bool existsActiveLongOrderWithMagicNumber(int magic, MqlApi mql4)
        {
            for (int cnt = mql4.OrdersTotal(); cnt > 0; cnt--)
            {
                if (mql4.OrderSelect(cnt, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES))
                {
                    if (mql4.OrderSymbol() != mql4.Symbol()) continue;
                    if (mql4.OrderClosePrice() == 0) continue;
                    if ((mql4.OrderType() != MqlApi.OP_BUY) && (mql4.OrderType() != MqlApi.OP_BUYLIMIT) && (mql4.OrderType() != MqlApi.OP_BUYSTOP)) continue;
                    if (mql4.OrderMagicNumber() == magic) return true;
                }
            }
            return false;
        }

        public static bool existsActiveShortOrderWithMagicNumber(int magic, MqlApi mql4)
        {
            for (int cnt = mql4.OrdersTotal(); cnt > 0; cnt--)
            {
                if (mql4.OrderSelect(cnt, MqlApi.SELECT_BY_POS, MqlApi.MODE_TRADES))
                {
                    if (mql4.OrderSymbol() != mql4.Symbol()) continue;
                    if (mql4.OrderClosePrice() == 0) continue;
                    if ((mql4.OrderType() != MqlApi.OP_SELL) && (mql4.OrderType() != MqlApi.OP_SELLLIMIT) && (mql4.OrderType() != MqlApi.OP_SELLSTOP)) continue;
                    if (mql4.OrderMagicNumber() == magic) return true;
                }
            }
            return false;
        }

        public static bool isSymbol(string symbol, MqlApi mql4)
        {
            double bid = mql4.MarketInfo(symbol, MqlApi.MODE_BID);
            if (mql4.GetLastError() == 4106) // unknown symbol
                return (false);
            else
                return (true);
        }

        public static double getPipValue(MqlApi mql4)
        {
            int factor;

            if (mql4.Digits == 5)
                factor = 1;
            else
                factor = 10;

            if (!mql4.IsTesting())
            {
                return (mql4.MarketInfo(mql4.Symbol(), MqlApi.MODE_TICKVALUE) * factor);
            }
            else
            {
                //this works only for FOREX
                string accountCurrency = mql4.AccountInfoString(MqlApi.ACCOUNT_CURRENCY);
                //check if quotation currnecy is account currency. 
                string quotationCurrency = mql4.Symbol().Substring(3, 3);
                //if the same, then pip value is 1. 
                if (accountCurrency.Equals(quotationCurrency))
                {
                    return 1.0;

                }
                //if not - find the current rate of the quotation currency compared to account currnecy. 
                string ACCQUO = accountCurrency + quotationCurrency;
                if (isSymbol(ACCQUO, mql4))
                {

                    double revPipValue = (mql4.iOpen(ACCQUO, MqlApi.PERIOD_M1, 0)) * factor;
                    if (revPipValue == 0)
                    {
                        mql4.Print("Data for ", ACCQUO, " not found. It's required for pip value conversion. Please load data.");
                        mql4.ExpertRemove();
                        return 0;
                    }
                    return ((1 / mql4.MarketInfo(ACCQUO, MqlApi.MODE_BID))) * factor;
                }
                //if symbol does not exist try to the other way around
                string QUOACC = quotationCurrency + accountCurrency;
                if (isSymbol(QUOACC, mql4))
                {
                    double pipValue = mql4.iOpen(QUOACC, MqlApi.PERIOD_M1, 0) * factor;
                    if (pipValue == 0)
                    {
                        mql4.Print("Data for ", QUOACC, " not found. It's required for pip value conversion. Please load data.");
                        mql4.ExpertRemove();
                        return 0;
                    }
                    return pipValue;
                }
                //otherwise
                return 0.0;
            }
            
        }

        public static double getLotSize(double riskCapital, int riskPips, MqlApi mql4)
        {
            double pipValue = OrderManager.getPipValue(mql4);
            return riskCapital / ((double)riskPips * pipValue);
        }

        public static double getPipConversionFactor(MqlApi mql4)
        {
            //multiplier depending on YEN or non YEN pairs
            if (mql4.Digits == 5)
                return 100000.00;
            else
                return 10000.00;
        }
    }
}
