using Microsoft.EntityFrameworkCore;
using DojoActivityCenter.Models;

namespace DojoActivityCenter.Models
{
    public class DacContext : DbContext
    {
        public DacContext(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<Join> Joins { get; set; }
    }
}