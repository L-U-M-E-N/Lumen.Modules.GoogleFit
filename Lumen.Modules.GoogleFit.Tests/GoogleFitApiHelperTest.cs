using Lumen.Modules.GoogleFit.Common;

using Microsoft.Extensions.Configuration;

namespace Lumen.Modules.GoogleFit.Tests {
    public class GoogleFitApiHelperTest {
        private readonly string API_KEY;
        private readonly string ACCESS_TOKEN;

        public GoogleFitApiHelperTest() {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddUserSecrets<GoogleFitApiHelperTest>()
                .Build();

            API_KEY = config["API_KEY"]!;
            ACCESS_TOKEN = config["ACCESS_TOKEN"]!;
        }

        [Fact]
        public async Task GetDailySteps_Today_ReturnsNonNegativeValue() {
            var steps = await GoogleFitApiHelper.GetDailyStepsAsync(API_KEY, ACCESS_TOKEN, DateTime.UtcNow);

            Assert.True(steps >= 0);
        }

        [Fact]
        public async Task GetDailySteps_Yesterday_ReturnsPositiveSteps() {
            var steps = await GoogleFitApiHelper.GetDailyStepsAsync(API_KEY, ACCESS_TOKEN, DateTime.UtcNow.AddDays(-1));

            Assert.True(steps > 0);
        }

        [Fact]
        public async Task GetHourlySteps_Today_Returns24Buckets() {
            var buckets = await GoogleFitApiHelper.GetHourlyStepsAsync(API_KEY, ACCESS_TOKEN, DateOnly.FromDateTime(DateTime.UtcNow));

            Assert.Equal(24, buckets.Count);
        }

        [Fact]
        public async Task GetHourlySteps_Today_BucketStepsSumMatchesDailyTotal() {
            var day = DateTime.UtcNow.Date;
            var daily = await GoogleFitApiHelper.GetDailyStepsAsync(API_KEY, ACCESS_TOKEN, day);
            var hourly = await GoogleFitApiHelper.GetHourlyStepsAsync(API_KEY, ACCESS_TOKEN, DateOnly.FromDateTime(day));
            var hourlyTotal = hourly.Sum(x => x.Steps);

            // Allow a small delta – Google Fit aggregation can differ slightly between calls.
            Assert.InRange(hourlyTotal, daily - 10, daily + 10);
        }
    }
}
