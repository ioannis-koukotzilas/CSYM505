using System;

namespace CSYM505.Api.Controllers.AnalyticsApi.Dtos
{
    public class MonthlyForecastObservation
    {
        public DateTime Month { get; set; }

        public decimal? Value { get; set; }

        public string Flags { get; set; } = string.Empty;

        public string RawValue { get; set; } = string.Empty;
    }
}
