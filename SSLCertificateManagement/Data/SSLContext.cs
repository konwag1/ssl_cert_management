using Microsoft.EntityFrameworkCore;
using Namespace.Models;

namespace Namespace.Data
{
    public class SSLContext : DbContext
    {
        public SSLContext(DbContextOptions<SSLContext> options)
            : base(options)
        {
        }

        public DbSet<SSLCertificate> SSLCertificates { get; set; }
    }
}
