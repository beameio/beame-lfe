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
    
    public partial class CRS_Bundles
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CRS_Bundles()
        {
            this.Coupons = new HashSet<Coupons>();
            this.CRS_BundleCategories = new HashSet<CRS_BundleCategories>();
            this.CRS_BundleCourses = new HashSet<CRS_BundleCourses>();
            this.PAYPAL_PaymentRequests = new HashSet<PAYPAL_PaymentRequests>();
            this.SALE_OrderLines = new HashSet<SALE_OrderLines>();
            this.USER_Bundles = new HashSet<USER_Bundles>();
            this.UserSessionsEventLogs = new HashSet<UserSessionsEventLogs>();
            this.WebStoreItems = new HashSet<WebStoreItems>();
            this.CHIMP_ListSegments = new HashSet<CHIMP_ListSegments>();
        }
    
        public int BundleId { get; set; }
        public System.Guid uid { get; set; }
        public int AuthorId { get; set; }
        public string BundleName { get; set; }
        public string BundleUrlName { get; set; }
        public string BundleDescription { get; set; }
        public string OverviewVideoIdentifier { get; set; }
        public decimal AffiliateCommission { get; set; }
        public string MetaTags { get; set; }
        public string BannerImage { get; set; }
        public Nullable<long> FbObjectId { get; set; }
        public bool FbObjectPublished { get; set; }
        public short StatusId { get; set; }
        public System.DateTime AddOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> PublishDate { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> MonthlySubscriptionPrice { get; set; }
        public Nullable<System.Guid> ProvisionUid { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Coupons> Coupons { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRS_BundleCategories> CRS_BundleCategories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRS_BundleCourses> CRS_BundleCourses { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PAYPAL_PaymentRequests> PAYPAL_PaymentRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SALE_OrderLines> SALE_OrderLines { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USER_Bundles> USER_Bundles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserSessionsEventLogs> UserSessionsEventLogs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WebStoreItems> WebStoreItems { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CHIMP_ListSegments> CHIMP_ListSegments { get; set; }
        public virtual Users Users { get; set; }
        public virtual StatusLOV StatusLOV { get; set; }
    }
}