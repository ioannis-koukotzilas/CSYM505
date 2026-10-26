namespace CSYM505.Api.Controllers.AnalyticsApi.Dtos
{
    public class ForecastResult
    {
        public List<MonthlyForecastObservation> AccommodationObservations { get; set; } = [];

        public List<MonthlyForecastObservation> PlatformObservations { get; set; } = [];

        public List<MonthlyForecastPrediction> AccommodationPredictions { get; set; } = [];

        public List<MonthlyForecastPrediction> PlatformPredictions { get; set; } = [];
    }
}
