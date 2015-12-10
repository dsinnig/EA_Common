using System;
using NQuotes;

namespace biiuse
{
    public class OrderFinal : OrderState
    {
        public OrderFinal(Order context, MqlApi mql4) : base(context, mql4)
        {
        }

        public override void update()
        {
            if (!context.getOrderCloseTime().Equals(new DateTime()))
            {
                context.OrderType = OrderType.FINAL;
            }
        }
    }
}