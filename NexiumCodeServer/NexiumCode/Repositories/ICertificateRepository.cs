using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public interface ICertificateRepository : IRepository<Certificate>
    {
        Task<IEnumerable<Certificate>> GetCertificatesByUser(int userId);
        Task<Certificate> GetCertificateByUserAndCourse(int userId, int courseId);

        Task IssueCertificate(int userId, int courseId, string certificateUrl);
    }
}
