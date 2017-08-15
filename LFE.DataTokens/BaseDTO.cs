using System;
using System.ComponentModel;
using LFE.Core.Enums;

namespace LFE.DataTokens
{

    public class JsonResponseToken
    {
        public bool success { get; set; }
        public object result { get; set; }
        public string message { get; set; }
        public string error { get; set; }
    }

    public class DateRangeToken
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }

        public bool Equals(DateRangeToken token)
        {
            return token.from.Equals(from) && token.to.Equals(to);
        }
    }
    public class BaseFileDTO
    {
        public string name { get; set; }
        public long length { get; set; }

        public string refId { get; set; }
    }
    public class TrxStatsDTO
    {
        public TrxStatsDTO()
        {
            TotalTrx       = 0;
            TotalLearner   = 0;
            TotalTrxAmount = 0;
        }
        public int? TotalTrx { get; set; }
        public int? TotalLearner { get; set; }
        public double? TotalTrxAmount { get; set; }
        public string period { get; set; }
    }

    public class BaseItemListDTO : BaseListDTO
    {
        public BillingEnums.ePurchaseItemTypes type { get; set; }
        public string desc { get; set; }
    }

    public class BaseWebstoreItemListDTO : BaseItemListDTO
    {
        public int? storeItemId { get; set; }
        public bool attach { get; set; }
        public CourseEnums.CourseStatus status { get; set; }
    }

    public class BaseListDTO
    {
        public int id { get; set; }
        public Guid uid { get; set; }
        public string name { get; set; }
        public int? index { get; set; }
    }

    public class BaseEntityDTO
    {
        public BaseEntityDTO()
        {
            Uid = Guid.NewGuid();
        }
        public BaseEntityDTO(string name,Guid uid)
        {
            Uid       = uid;
            id        = -1;
            this.name = name;
        }
        public Guid? Uid { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }

    public class BaseModelState
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }

    public class TreeBaseToken
    {
        public string id { get; set; }
        public string Name { get; set; }

        public bool hasChildren { get; set; }
    }
   
}
