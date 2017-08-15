using System;
using System.Collections.Generic;
using LFE.DataTokens;

namespace LFE.Application.Services.Interfaces
{
    public interface ICertificateAdminServices
    {
        List<CertificateTemplateDTO> TempatesLOV { get; }
        CertificateDTO GetCourseCertificate(int courseId);        
        bool SaveCertificate(CertificateDTO token, out string error);
        bool DeleteCertificate(int certId, out string error);

        List<StudentCertificateDTO> FindStudentCertificatesByAuthor(int authorUserId);
    }

    public interface IStudentSertificateCervices
    {
        StudentCertificateDTO GetStudentCertificate(Guid attemptId, int userId);
        StudentCertificateDTO GetStudentCertificate(int courseId, int userId);
        StudentCertificateDTO GetStudentCertificate(Guid studentCertId);
        bool SendStudentCertificate(StudentCertificateDTO token, string certificateBody, out string error);
        StudentCertificateDTO GetStudentCertificate(string key);

        bool IsCourseHasCertificateOnCompletion(int courseId);
    }
}