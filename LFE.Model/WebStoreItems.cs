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
    
    public partial class WebStoreItems
    {
        public int WebstoreItemId { get; set; }
        public byte ItemTypeId { get; set; }
        public Nullable<int> CourseId { get; set; }
        public Nullable<int> BundleId { get; set; }
        public int WebStoreCategoryID { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public int Ordinal { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime AddOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
    
        public virtual WebStoreCategories WebStoreCategories { get; set; }
        public virtual CRS_Bundles CRS_Bundles { get; set; }
        public virtual Courses Courses { get; set; }
    }
}
