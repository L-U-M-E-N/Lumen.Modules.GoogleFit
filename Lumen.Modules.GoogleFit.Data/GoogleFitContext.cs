using Lumen.Modules.GoogleFit.Common.Models;

using Microsoft.EntityFrameworkCore;

namespace Lumen.Modules.GoogleFit.Data {
    public class GoogleFitContext(DbContextOptions<GoogleFitContext> options) : DbContext(options) {
        public const string SCHEMA_NAME = "googlefit";

        public DbSet<DailyStepsPointInTime> DailySteps { get; set; } = null!;
        public DbSet<HourlyStepsPointInTime> HourlySteps { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.HasDefaultSchema(SCHEMA_NAME);

            var dailyBuilder = modelBuilder.Entity<DailyStepsPointInTime>();
            dailyBuilder.HasKey(x => x.Date);

            var hourlyBuilder = modelBuilder.Entity<HourlyStepsPointInTime>();
            hourlyBuilder.HasKey(x => x.HourStart);
        }
    }
}
