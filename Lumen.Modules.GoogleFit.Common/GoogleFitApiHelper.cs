using Google.Apis.Auth.OAuth2;
using Google.Apis.Fitness.v1;
using Google.Apis.Fitness.v1.Data;
using Google.Apis.Services;

using Lumen.Modules.GoogleFit.Common.Models;

namespace Lumen.Modules.GoogleFit.Common {
    public static class GoogleFitApiHelper {
        private const long MS_PER_DAY = 86_400_000L;
        private const long MS_PER_HOUR = 3_600_000L;

        public static async Task<long> GetDailyStepsAsync(string apiKey, string accessToken, DateTime dayUtc) {
            var service = BuildService(apiKey, accessToken);

            var startMs = ToEpochMs(dayUtc.Date);
            var endMs = ToEpochMs(dayUtc.Date.AddDays(1));

            var buckets = await AggregateStepsAsync(service, startMs, endMs, durationMs: MS_PER_DAY);

            return SumBucketSteps(buckets.FirstOrDefault());
        }

        public static async Task<IReadOnlyList<HourlyStepsPointInTime>> GetHourlyStepsAsync(string apiKey, string accessToken, DateOnly date) {
            var service = BuildService(apiKey, accessToken);

            var startMs = ToEpochMs(date.ToDateTime(TimeOnly.MinValue));
            var endMs = ToEpochMs(date.ToDateTime(TimeOnly.MinValue).AddDays(1));

            var buckets = await AggregateStepsAsync(service, startMs, endMs, durationMs: MS_PER_HOUR);

            return [.. buckets
                .Select(bucket => new HourlyStepsPointInTime {
                    HourStart = DateTimeOffset
                        .FromUnixTimeMilliseconds((long)bucket.StartTimeMillis!)
                        .UtcDateTime,
                    Steps = SumBucketSteps(bucket)
                })];
        }

        private static FitnessService BuildService(string apiKey, string accessToken) =>
            new(new BaseClientService.Initializer {
                ApplicationName = "Lumen.Modules.GoogleFit",
                ApiKey = apiKey,
                HttpClientInitializer = GoogleCredential.FromAccessToken(accessToken)
            });

        private static async Task<IList<AggregateBucket>> AggregateStepsAsync(FitnessService service, long startMs, long endMs, long durationMs) {

            var request = service.Users.Dataset.Aggregate(
                new AggregateRequest {
                    AggregateBy = [
                        new AggregateBy { DataTypeName = "com.google.step_count.delta" }
                    ],
                    BucketByTime = new BucketByTime { DurationMillis = durationMs },
                    StartTimeMillis = startMs,
                    EndTimeMillis = endMs
                },
                userId: "me"
            );

            var response = await request.ExecuteAsync();
            return response.Bucket ?? [];
        }

        private static long SumBucketSteps(AggregateBucket? bucket) {
            if (bucket is null) return 0L;

            return bucket.Dataset
                .SelectMany(ds => ds.Point ?? [])
                .SelectMany(pt => pt.Value ?? [])
                .Sum(v => (long)(v.IntVal ?? 0));
        }

        private static long ToEpochMs(DateTime utc) =>
            new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToUnixTimeMilliseconds();
    }
}
