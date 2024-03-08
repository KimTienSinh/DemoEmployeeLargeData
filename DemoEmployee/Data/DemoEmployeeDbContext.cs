using Microsoft.EntityFrameworkCore;

namespace DemoEmployee.Data
{
    public class DemoEmployeeDbContext : DbContext
    {
        public DemoEmployeeDbContext(DbContextOptions<DemoEmployeeDbContext> options) : base(options) { }

        public virtual DbSet<Employee> Employees { get; set; }
    }
}
