using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EatonDeliveryCheckpoint.Enums
{
    public enum ErrorEnum
    {
        None,
        NoData,
        InvalidProperties,
        DuplicatedFileName,
        DuplicatedDelivery,
        FailedToAccessDatabase,
    }

    public static class ErrorEnumExtensions
    {
        public static string ToDescription(this ErrorEnum error)
        {
            switch (error)
            {
                case ErrorEnum.None:
                    return "";
                case ErrorEnum.NoData:
                    return "NoData";
                case ErrorEnum.InvalidProperties:
                    return "InvalidProperties";
                case ErrorEnum.DuplicatedFileName:
                    return "DuplicatedFileName";
                case ErrorEnum.DuplicatedDelivery:
                    return "DuplicatedDelivery";
                case ErrorEnum.FailedToAccessDatabase:
                    return "FailedToAccessDatabase";
                default:
                    return "Unknown";
            }
        }

        public static string ToChineseDescription(this ErrorEnum error)
        {
            switch (error)
            {
                case ErrorEnum.None:
                    return "";
                case ErrorEnum.NoData:
                    return "無資料";
                case ErrorEnum.InvalidProperties:
                    return "資料欄位錯誤";
                case ErrorEnum.DuplicatedFileName:
                    return "檔案重複";
                case ErrorEnum.DuplicatedDelivery:
                    return "Delivery重複";
                case ErrorEnum.FailedToAccessDatabase:
                    return "資料庫存取錯誤";
                default:
                    return "";
            }
        }
    }
}
