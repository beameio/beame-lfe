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
    
    public partial class DSC_MessageUsers
    {
        public long RowId { get; set; }
        public long MessageId { get; set; }
        public int UserId { get; set; }
    
        public virtual DSC_Messages DSC_Messages { get; set; }
        public virtual Users Users { get; set; }
    }
}
