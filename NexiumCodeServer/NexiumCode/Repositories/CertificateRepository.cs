using Microsoft.EntityFrameworkCore;
using NexiumCode.Context;
using NexiumCode.Models;

namespace NexiumCode.Repositories
{
    public class CertificateRepository : GenericRepository<Certificate>, ICertificateRepository
    {
        public CertificateRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Certificate>> GetCertificatesByUser(int userId)
        {
            return await _context.Certificates
                .Where(c => c.UserId == userId)
                .Include(c => c.Course)
                .ToListAsync();
        }

        public async Task<Certificate> GetCertificateByUserAndCourse(int userId, int courseId) 
        {
            return await _context.Certificates
                .Where(c => c.UserId == userId && c.CourseId == courseId)
                .FirstOrDefaultAsync();
        }

        public async Task IssueCertificate(int userId, int courseId, string certificateUrl)
        {
            var certificate = new Certificate
            {
                UserId = userId,
                CourseId = courseId,
                CertificateUrl = certificateUrl,
                IssueDate = DateTimeOffset.UtcNow
            };
            await Add(certificate);
        }
    }
}
