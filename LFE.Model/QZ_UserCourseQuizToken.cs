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
    
    public partial class QZ_UserCourseQuizToken
    {
        public System.Guid QuizId { get; set; }
        public string Title { get; set; }
        public byte StatusId { get; set; }
        public Nullable<byte> PassPercent { get; set; }
        public int CourseId { get; set; }
        public Nullable<short> AvailableAfter { get; set; }
        public bool IsMandatory { get; set; }
        public Nullable<byte> Attempts { get; set; }
        public Nullable<short> TimeLimit { get; set; }
        public System.DateTime AddOn { get; set; }
        public Nullable<bool> Passed { get; set; }
        public decimal Score { get; set; }
        public int Sid { get; set; }
        public bool IsAttached { get; set; }
    }
}