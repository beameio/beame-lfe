using LFE.Core.Enums;
using LFE.Core.Utils;
using System;
using System.Collections.Generic;

namespace LFE.DataTokens
{

    public class FiltersToken
    {
        public int UserId { get; set; }
        public short CurrencyId { get; set; }
        public int PeriodTypeId { get; set; }
        public bool IsCompareMode { get; set; }
        public int? StoreId { get; set; }

        public DateRangeToken DateRange { get; set; }
    }

    public class DashboardSaleBoxToken
    {
        public DashboardSaleBoxToken()
        {
            
        }
        public DashboardSaleBoxToken(DashboardEnums.eSaleBoxType type)
        {
            Type     = type;
            Total    = 0;
            Quantity = 0;
        }

        public DashboardSaleBoxToken(DashboardEnums.eSaleBoxType type,decimal total, int qty)
        {
            Type     = type;
            Total    = total.FormatMoney(0);
            Quantity = qty;
        }

        public DashboardEnums.eSaleBoxType Type { get; set; }
        public string Title { get { return Utils.GetEnumDescription(Type); } }
        public decimal Total { get; set; }
        public int Quantity { get; set; }
        public bool IsUp { get; set; }
    }

    public class DashboardSaleBoxTokenLists
    {

        public FiltersToken Filters { get; set; }
        public BaseCurrencyDTO Currency { get; set; }
        public List<DashboardSaleBoxToken> List { get; set; }
        public List<DashboardSaleBoxToken> CompareToList { get; set; }
    }

    public class DashboardEventToken
    {
        public Guid Uid { get; set; }

        public DashboardEnums.eDbEventTypes Type { get; set; }

        public string Name { get; set; }

        public DateTime? Date { get; set; }

        public string Color { get; set; }

        public bool Enabled { get; set; }

        public bool IsStatic { get; set; }
    }

    public class DashboardPayoutToken
    {
        public BaseCurrencyDTO Currency { get; set; }
        public decimal Sales { get; set; }
        public decimal Mbg { get; set; }
        public decimal Fees { get; set; }

        public decimal TotalPayout
        {
            get { return (Sales - Mbg - Fees).FormatMoney(0); }
        }

        public bool IsUp { get; set; }
    }


    public class DashboardStatsToken
    {
        public int Stores { get; set; }
        public int Courses { get; set; }
        public int Bundles { get; set; }
        public int UnpublishedCourses { get; set; }

        public int ActiveSubscribers { get; set; }
    }

    public class DbRefundDetailToken : BaseOrderLineDTO
    {
        public decimal RefundAmount { get; set; }
        public DateTime RefundDate { get; set; }
    }

    public class DbSubscriptionDetailToken : BaseOrderLineDTO
    {        
        public DateTime? CancelledOn { get; set; }
    }
}
