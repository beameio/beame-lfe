//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LFE.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class vw_WS_Items
    {
        public int WebstoreItemId { get; set; }
        public string ItemName { get; set; }
        public byte ItemTypeId { get; set; }
        public string ItemDescription { get; set; }
        public int Ordinal { get; set; }
        public string AuthorName { get; set; }
        public int AuthorID { get; set; }
        public int ItemId { get; set; }
        public string UrlName { get; set; }
        public string ImageURL { get; set; }
        public int NumSubscribers { get; set; }
        public int Rating { get; set; }
        public Nullable<bool> IsFreeCourse { get; set; }
        public System.DateTime Created { get; set; }
        public string TrackingID { get; set; }
        public short ItemStatusId { get; set; }
        public int StoreID { get; set; }
        public int OwnerUserID { get; set; }
        public int WebStoreCategoryID { get; set; }
        public int CategoryOrdinal { get; set; }
        public int CoursesCnt { get; set; }
        public decimal AffiliateCommission { get; set; }
        public Nullable<System.Guid> ItemProvisionUid { get; set; }
    }
}