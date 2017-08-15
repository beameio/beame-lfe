using System;
using System.Web.Mvc;

namespace LFE.Portal.Areas.AuthorAdmin.Models
{
    public enum eReportTypes
    {
        Grid,
        List
    }

    public enum eDashReportKinds
    {
        content,
        stores
    }

    public class DataSourceSortToken
    {
        public string name { get; set; }
        public string field { get; set; }
        public string dir { get; set; }
    }

  
}