using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lumen.Modules.GoogleFit.Data {
    public class GoogleFitDbContextFactory : IDesignTimeDbContextFactory<GoogleFitContext> {
        public GoogleFitContext CreateDbContext(string[] args) {
            var optionsBuilder = new DbContextOptionsBuilder<GoogleFitContext>();
            optionsBuilder.UseNpgsql();

            return new GoogleFitContext(optionsBuilder.Options);
        }
    }
}
