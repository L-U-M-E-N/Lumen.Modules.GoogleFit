using Lumen.Modules.GoogleFit.Common.Models;

using Microsoft.EntityFrameworkCore;

namespace Lumen.Modules.GoogleFit.Data {
    public class GoogleFitContext : DbContext {
        public const string SCHEMA_NAME = "GoogleFit";

        public GoogleFitContext(DbContextOptions<GoogleFitContext> options) : base(options) {
        }

        public DbSet<GoogleFitPointInTime> GoogleFit { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.HasDefaultSchema(SCHEMA_NAME);

            var GoogleFitModelBuilder = modelBuilder.Entity<GoogleFitPointInTime>();
            GoogleFitModelBuilder.Property(x => x.Time)
                .HasColumnType("timestamp with time zone");

            GoogleFitModelBuilder.Property(x => x.Value)
                .HasColumnType("integer");

            GoogleFitModelBuilder.HasKey(x => x.Time);
        }
    }
}
