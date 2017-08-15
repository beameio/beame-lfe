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
    
    public partial class CouponInstances
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CouponInstances()
        {
            this.SALE_OrderLines = new HashSet<SALE_OrderLines>();
        }
    
        public int Id { get; set; }
        public int CouponId { get; set; }
        public string CouponCode { get; set; }
        public int UsageLimit { get; set; }
        public string salesforce_id { get; set; }
        public string salesforce_checksum { get; set; }
        public System.DateTime AddOn { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
    
        public virtual Coupons Coupons { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SALE_OrderLines> SALE_OrderLines { get; set; }
    }
}