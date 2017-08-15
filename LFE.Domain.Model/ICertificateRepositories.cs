using LFE.Domain.Core;
using LFE.Model;

namespace LFE.Domain.Model
{
    public interface ICertificateRepository : IRepository<CERT_CertificateLib>
    {
    }

    public interface IStudentCertificatesRepository : IRepository<CERT_StudentCertificates>
    {
    }

    public interface IStudentCertificatesViewRepository : IGetRepository<vw_CERT_StudentCertificates>
    {
    }
}
