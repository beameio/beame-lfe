using System;
using LFE.Core.Enums;
using System.Web.Mvc;

namespace LFE.Portal.Areas.PortalAdmin.Models
{
    public enum eReportTypes
    {
        Grid,
        List
    }
    
    public class DataSourceSortToken
    {
        public string name { get; set; }
        public string field { get; set; }
        public string dir { get; set; }
    }

    public class ComboListItem : SelectListItem
    {
        public int Index { get; set; }
    }

    public class UserViewToken
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class ReportFilterViewToken
    {
        public string Action { get; set; }
    }

    public class FbTesterToken
    {
        public string BaseAccessToken { get; set; }
        public string MessageAccessToken { get; set; }
        public string Message { get; set; }
        public bool UsePageId { get; set; }
        public long ObjectId { get; set; }
        public FbEnums.eFbActions Action { get; set; }

    }

    public class EventFiltersToken
    {
        public ReportEnums.ePeriodSelectionKinds PeriodKind { get; set; }

        public bool ShowOnlyPeriodCombo { get; set; }
    }

    public class HostAbandonFilterToken
    {
        public HostAbandonFilterToken()
        {
            from = DateTime.Now.AddYears(-1);
            last = DateTime.Now.AddMonths(-1);
        }

        public DateTime from { get; set; }
        public DateTime last { get; set; }
    }
}