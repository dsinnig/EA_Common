﻿using NQuotes;

namespace biiuse
{
    internal class SellStopOrderTrendTradePlaced : TradeState
    {
        private BOTrade context; //hides context in Trade

        public SellStopOrderTrendTradePlaced(BOTrade aContext, MqlApi mql4) : base(mql4)
        {
            this.context = aContext;
        }

        public override void update()
        {
            if (mql4.Bid >= context.getCancelPrice())
            {
                context.addLogEntry(2, "Bid price went above cancel level. Attempting to delete order.");
                //delete Order
                ErrorType result = context.Order.deleteOrder();

                if (result == ErrorType.NO_ERROR)
                {
                    context.addLogEntry("Order deleted successfully", true);
                    context.setState(new TradeClosed(context, mql4));
                    return;
                }

                if (result == ErrorType.RETRIABLE_ERROR)
                {
                    context.addLogEntry("Order could not be deleted. Will re-try at next tick.", true);
                    return;
                }

                if (result == ErrorType.NON_RETRIABLE_ERROR)
                {
                    context.addLogEntry("Order could not be deleted. Abort trade.", true);
                    context.setState(new TradeClosed(context, mql4));
                    return;
                }
            }


            if (context.Order.OrderType == OrderType.SELL)
            {
                context.addLogEntry(1, "Order got filled at price: " + mql4.DoubleToStr(context.Order.getOrderOpenPrice(), mql4.Digits));
                context.setActualEntry(context.Order.getOrderOpenPrice());
                context.setState(new SellOrderFilledTrendTrade(context, mql4));
                return;
            }

        }
    }
}