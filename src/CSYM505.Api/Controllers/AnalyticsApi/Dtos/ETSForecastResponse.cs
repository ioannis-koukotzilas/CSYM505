namespace CSYM505.Api.Controllers.AnalyticsApi.Dtos
{
    public class ETSForecastResponse
    {
        public string Method { get; set; } = string.Empty;

        public List<double> Predictions { get; set; } = [];
    }
}
