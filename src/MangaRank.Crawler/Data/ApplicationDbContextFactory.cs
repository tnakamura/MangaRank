using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MangaRank.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            return CreateDbContext();
        }

        public ApplicationDbContext CreateDbContext()
        {
            var context = new ApplicationDbContext(
                Program.Configuration.GetConnectionString("DefaultConnection"));

            return context;
        }
    }
}
