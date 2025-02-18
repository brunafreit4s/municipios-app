using Microsoft.EntityFrameworkCore;
using municipios_api.Models;

namespace municipios_api.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        public DbSet<Municipio> Municipios { get; set; }
    }
}
