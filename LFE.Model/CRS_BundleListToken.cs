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
    
    public partial class CRS_BundleListToken
    {
        public int BundleId { get; set; }
        public System.Guid Uid { get; set; }
        public int AuthorUserId { get; set; }
        public string AuthorNickname { get; set; }
        public string AuthorFirstName { get; set; }
        public string AuthorLastName { get; set; }
        public string BannerImage { get; set; }
        public string OverviewVideoIdentifier { get; set; }
        public int LearnerCount { get; set; }
        public string BundleName { get; set; }
        public string BundleDescription { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> MonthlySubscriptionPrice { get; set; }
        public string MetaTags { get; set; }
        public Nullable<long> FbObjectId { get; set; }
        public bool FbObjectPublished { get; set; }
        public short StatusId { get; set; }
        public System.DateTime AddOn { get; set; }
    }
}
