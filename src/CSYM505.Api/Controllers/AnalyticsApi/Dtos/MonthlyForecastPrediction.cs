using System;

namespace CSYM505.Api.Controllers.AnalyticsApi.Dtos
{
    public class MonthlyForecastPrediction
    {
        public DateTime Month { get; set; }

        public string Method { get; set; } = string.Empty;

        public double PredictedValue { get; set; }
    }
}
