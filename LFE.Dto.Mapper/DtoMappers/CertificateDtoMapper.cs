using LFE.Core.Utils;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.DtoMappers
{
    public static class CertificateDtoMapper
    {
        private const string TEMPLATES_ROOT_PATH = "/Content/assets/cert/";

        public static CertificateDTO Entity2CertificateDto(this CERT_CertificateLib entity, bool send2Students,bool attached2Quiz)
        {
            return new CertificateDTO
            {
                CertificateId       = entity.CertificateId
                ,TemplateId         = entity.TemplateId
                ,CourseId           = entity.CourseId
                ,Title              = entity.Title
                ,CourseName         = entity.CourseName
                ,PresentedBy        = entity.PresentedBy
                ,Description        = entity.Description
                ,SignatureImageUrl  = entity.Entity2SignatureUrl(Constants.ImageBaseUrl,Constants.DefaultCertificateSignatureUrl)
                ,IsActive           = entity.IsActive
                ,SendToStudents     = send2Students
                ,AttachedToQuiz     = attached2Quiz
                ,IsValid            = true
            };
        }

        public static CertificateTemplateDTO Entity2TemplateDto(this CERT_TemplatesLOV entity)
        {
            return new CertificateTemplateDTO
            {
                Value     = entity.TemplateId.ToString()
                ,Text     = entity.Name
                ,ImageUrl = string.Format("{0}{1}",TEMPLATES_ROOT_PATH,entity.ImageName)
            };
        }

        public static StudentCertificateDTO Entity2StudentCertificateDto(this vw_CERT_StudentCertificates entity)
        {
            return new StudentCertificateDTO
            {
                StudentCertificateId = entity.StudentCertificateId
                ,CertificateId       = entity.CertificateId
                ,SendOn              = entity.SendOn
                ,TemplateId          = entity.TemplateId
                ,CourseId            = entity.CourseId
                ,Title               = entity.Title
                ,CourseName          = entity.CourseName
                ,PresentedBy         = entity.PresentedBy
                ,Description         = entity.Description
                ,BgFullUrl           = string.Format("{0}{1}{2}",Utils.GetKeyValue("baseUrl"),TEMPLATES_ROOT_PATH,entity.TemplateImageName)
                ,AddOn               = entity.AddOn
                ,SignatureImageUrl   = entity.Entity2SignatureUrl(Constants.ImageBaseUrl,Constants.DefaultCertificateSignatureUrl)                
                ,IsValid             = true               
                ,StudentInfo         = new BaseUserInfoDTO
                                        {
                                            UserId    = entity.StudentUserId
                                            ,Email    = entity.StudentEmail
                                            ,FullName = entity.Entity2FullName()
                                        }
            };
        }
    }
}