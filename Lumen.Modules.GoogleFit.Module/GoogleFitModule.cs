using Lumen.Modules.GoogleFit.Common;
using Lumen.Modules.GoogleFit.Data;
using Lumen.Modules.Sdk;

using Microsoft.EntityFrameworkCore;

namespace Lumen.Modules.GoogleFit.Module {
    public class GoogleFitModule(IEnumerable<ConfigEntry> configEntries, ILogger<LumenModuleBase> logger, IServiceProvider provider) : LumenModuleBase(configEntries, logger, provider) {
        public const string API_KEY = nameof(API_KEY);

        public override Task InitAsync(LumenModuleRunsOnFlag currentEnv) {
            return RunAsync(currentEnv, DateTime.UtcNow);
        }

        public override async Task RunAsync(LumenModuleRunsOnFlag currentEnv, DateTime date) {
            try {
                logger.LogTrace($"[{nameof(GoogleFitModule)}] Running tasks ...");

                switch (currentEnv) {
                    case LumenModuleRunsOnFlag.API:
                        await RunAPIAsync(date);
                        break;
                    case LumenModuleRunsOnFlag.UI:
                        await RunUIAsync(date);
                        break;
                }

                logger.LogTrace($"[{nameof(GoogleFitModule)}] Running tasks ... Done!");
            } catch (Exception ex) {
                logger.LogError(ex, $"[{nameof(GoogleFitModule)}] Error when running tasks.");
            }
        }

        private async Task RunAPIAsync(DateTime date) {
            var apiKey = GetConfigValue(API_KEY);
            var accessToken = "TODO"; // TODO

            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GoogleFitContext>();

            // Daily steps
            foreach (var day in new[] { date.Date, date.Date.AddDays(-1) }) {
                var dayUtc = DateTime.SpecifyKind(day, DateTimeKind.Utc);
                var dateUtc = DateOnly.FromDateTime(dayUtc);
                var newSteps = await GoogleFitApiHelper.GetDailyStepsAsync(apiKey, accessToken, dayUtc);

                var existing = await context.DailySteps
                    .FirstOrDefaultAsync(x => x.Date == dateUtc);

                if (existing is null) {
                    logger.LogTrace($"[{nameof(GoogleFitModule)}] Inserting daily steps for {dayUtc:yyyy-MM-dd}: {newSteps}");
                    context.DailySteps.Add(new Common.Models.DailyStepsPointInTime {
                        Date = dateUtc,
                        Steps = newSteps
                    });
                } else if (existing.Steps != newSteps) {
                    logger.LogTrace($"[{nameof(GoogleFitModule)}] Updating daily steps for {dayUtc:yyyy-MM-dd}: {existing.Steps} → {newSteps}");
                    existing.Steps = newSteps;
                    context.DailySteps.Update(existing);
                }
            }

            // Hourly steps
            var todayUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var hourlyBuckets = await GoogleFitApiHelper.GetHourlyStepsAsync(apiKey, accessToken, DateOnly.FromDateTime(todayUtc));

            foreach (var bucket in hourlyBuckets) {
                var hourStart = DateTime.SpecifyKind(bucket.HourStart, DateTimeKind.Utc);

                var existing = await context.HourlySteps
                    .FirstOrDefaultAsync(x => x.HourStart == hourStart);

                if (existing is null) {
                    context.HourlySteps.Add(new Common.Models.HourlyStepsPointInTime {
                        HourStart = hourStart,
                        Steps = bucket.Steps
                    });
                } else if (existing.Steps != bucket.Steps) {
                    existing.Steps = bucket.Steps;
                    context.HourlySteps.Update(existing);
                }
            }

            await context.SaveChangesAsync();
        }

        private Task RunUIAsync(DateTime date) {
            // TODO
            return Task.CompletedTask;
        }

        private string GetConfigValue(string key) {
            var entry = configEntries.FirstOrDefault(x => x.ConfigKey == key);
            if (entry is null || entry.ConfigValue is null) {
                logger.LogError($"[{nameof(GoogleFitModule)}] Config key \"{key}\" is missing!");
            }
            return entry?.ConfigValue ?? string.Empty;
        }

        public override bool ShouldRunNow(LumenModuleRunsOnFlag currentEnv, DateTime date) {
            return currentEnv switch {
                LumenModuleRunsOnFlag.API => date.Second == 0 && date.Minute == 0,
                LumenModuleRunsOnFlag.UI => false,
                _ => false,
            };
        }

        public override Task ShutdownAsync() {
            return Task.CompletedTask;
        }

        public static new void SetupServices(LumenModuleRunsOnFlag currentEnv, IServiceCollection serviceCollection, string? postgresConnectionString) {
            if (currentEnv == LumenModuleRunsOnFlag.API) {
                serviceCollection.AddDbContext<GoogleFitContext>(o =>
                    o.UseNpgsql(postgresConnectionString,
                        x => x.MigrationsHistoryTable("__EFMigrationsHistory", GoogleFitContext.SCHEMA_NAME)));
            }
        }

        public override Type GetDatabaseContextType() {
            return typeof(GoogleFitContext);
        }
    }
}
