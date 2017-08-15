
using System;
using System.ComponentModel;
using System.Web.Mvc;
using LFE.Core.Utils;

namespace LFE.DataTokens
{
    public class CertificateBaseToken : BaseModelState
    {
        public int CertificateId { get; set; }
        public string Title { get; set; }
    }

    public class CertificateDTO : CertificateBaseToken 
    {
        public CertificateDTO(int courseId,string courseName,string userFullName)
        {
            CourseId           = courseId;
            TemplateId         = 1;
            CourseName         = courseName;
            CertificateId      = -1;
            IsActive           = true;
            IsValid            = true;
            SignatureImageUrl = Constants.DefaultCertificateSignatureUrl;
            PresentedBy        = userFullName;
        }


        public CertificateDTO()
        {
            CertificateId = -1;
        }

        public int CourseId { get; set; }
        [DisplayName("Select Style")]
        public byte TemplateId { get; set; }
        [DisplayName("Course Name")]
        public string CourseName { get; set; }

        public string SignatureImageUrl { get; set; }

        [DisplayName("Presented By")]
        public string PresentedBy { get; set; }

        [AllowHtml]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Active Certificate")]
        public bool IsActive { get; set; }

        public bool SendToStudents { get; set; }
        public bool AttachedToQuiz { get; set; }
    }


    public class StudentCertificateDTO : CertificateDTO
    {
        public Guid StudentCertificateId { get; set; }
        public BaseUserInfoDTO StudentInfo { get; set; }      
        public DateTime? SendOn { get; set; }
        public DateTime AddOn { get; set; }
        public string Key { get; set; }
        public string OnlineCertificateUrl { get; set; }
        public string BgFullUrl { get; set; }
    }


    public class CertificateTemplateDTO : SelectListItem
    {
        public string ImageUrl { get; set; }
    }
}
