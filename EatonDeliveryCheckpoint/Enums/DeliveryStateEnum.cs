using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Enums
{
    public enum DeliveryStateEnum
    {
        New,
        Delivery,
        Finish,
        Alert,
        Disable
    }

    public static class DeliveryStateEnumExtensions
    {
        public static string ToDescription(this DeliveryStateEnum error)
        {
            switch (error)
            {
                case DeliveryStateEnum.New:
                    return "new";
                case DeliveryStateEnum.Delivery:
                    return "delivery";
                case DeliveryStateEnum.Finish:
                    return "finish";
                case DeliveryStateEnum.Alert:
                    return "alert";
                case DeliveryStateEnum.Disable:
                    return "disable";
                default:
                    return "unknown";
            }
        }
    }
}
