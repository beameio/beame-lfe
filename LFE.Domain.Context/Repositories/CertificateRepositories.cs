using LFE.Domain.Core;
using LFE.Domain.Core.Data;
using LFE.Domain.Model;
using LFE.Model;

namespace LFE.Domain.Context.Repositories
{
    public class CertificateRepository : Repository<CERT_CertificateLib>, ICertificateRepository
    {
        public CertificateRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class StudentCertificatesRepository : Repository<CERT_StudentCertificates>, IStudentCertificatesRepository
    {
        public StudentCertificatesRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

    public class StudentCertificatesViewRepository: Repository<vw_CERT_StudentCertificates>, IStudentCertificatesViewRepository
    {
        public StudentCertificatesViewRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }
}
