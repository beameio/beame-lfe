using System;
using LFE.DataTokens;
using LFE.Dto.Mapper.Helper;
using LFE.Model;

namespace LFE.Dto.Mapper.EntityMapper
{
    public static class CertificateEntityMapper
    {
        public static CERT_CertificateLib Token2CertificateEntity(this CertificateDTO token)
        {
            return  new CERT_CertificateLib
            {
                CourseId       = token.CourseId
                ,TemplateId    = token.TemplateId
                ,Title         = token.Title
                ,CourseName    = token.CourseName
                ,Description   = token.Description
                ,PresentedBy   = token.PresentedBy
                ,IsActive      = token.IsActive
                ,SitgnatureUrl = token.SignatureImageUrl
                ,AddOn         = DateTime.Now
                ,CreatedBy     = DtoExtensions.CurrentUserId
            };
        }

        public static void UpdateCertificateEntity(this CERT_CertificateLib entity, CertificateDTO token)
        {
             entity.TemplateId    = token.TemplateId;
             entity.Title         = token.Title;
             entity.CourseName    = token.CourseName;
             entity.Description   = token.Description;
             entity.PresentedBy   = token.PresentedBy;
             entity.SitgnatureUrl = token.SignatureImageUrl;
            entity.IsActive       = token.IsActive;
             entity.UpdateDate    = DateTime.Now;
             entity.UpdatedBy     = DtoExtensions.CurrentUserId;
        }

        public static CERT_StudentCertificates CertificateEntity2StudentCertificateEntity(this CERT_CertificateLib entity,int userId)
        {
            return new CERT_StudentCertificates
            {
                StudentCertificateId = Guid.NewGuid()
                ,UserId              = userId
                ,CertificateId       = entity.CertificateId
                ,TemplateId          = entity.TemplateId
                ,Title               = entity.Title
                ,CourseName          = entity.CourseName
                ,Description         = entity.Description
                ,PresentedBy         = entity.PresentedBy
                ,SitgnatureUrl       = entity.SitgnatureUrl
                ,AddOn               = DateTime.Now
                ,CreatedBy           = DtoExtensions.CurrentUserId

            };
        }

        public static void UpdateCertificateSendDate(this CERT_StudentCertificates entity)
        {
            entity.SendOn     = DateTime.Now;
            entity.UpdateDate = DateTime.Now;
            entity.UpdatedBy  = DtoExtensions.CurrentUserId;
        }
    }
}
