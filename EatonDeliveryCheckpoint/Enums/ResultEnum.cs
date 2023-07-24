using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Enums
{
    public enum ResultEnum
    {
        True,
        False
    }

    public static class ResultEnumExtensions
    {
        public static bool ToBoolean(this ResultEnum result)
        {
            switch (result)
            {
                case ResultEnum.True:
                    return true;
                case ResultEnum.False:
                    return false;
                default:
                    return false;
            }
        }

        public static string ToString(this ResultEnum result)
        {
            switch (result)
            {
                case ResultEnum.True:
                    return "true";
                case ResultEnum.False:
                    return "false";
                default:
                    return "false";
            }
        }
    }
}
