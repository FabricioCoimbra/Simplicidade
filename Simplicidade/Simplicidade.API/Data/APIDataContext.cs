using Microsoft.EntityFrameworkCore;
using Simplicidade.Compartilhada.Model;

namespace Simplicidade.API.Data
{
    public class APIDataContext : DbContext
    {
        public APIDataContext(DbContextOptions<APIDataContext> options)
           : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
    }
}
