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
    
    public partial class CRS_BundleInfoToken
    {
        public int BundleId { get; set; }
        public string BundleName { get; set; }
        public string BundleDescription { get; set; }
        public int AuthorId { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FacebookID { get; set; }
        public string BannerImage { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> MonthlySubscriptionPrice { get; set; }
        public string OverviewVideoIdentifier { get; set; }
        public string MetaTags { get; set; }
        public System.Guid uid { get; set; }
        public short StatusId { get; set; }
        public string StatusCode { get; set; }
        public string StatusName { get; set; }
        public Nullable<long> FbObjectId { get; set; }
        public bool FbObjectPublished { get; set; }
        public string BundleUrlName { get; set; }
    }
}
