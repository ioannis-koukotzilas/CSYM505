using CSYM505.Api.Controllers.AnalyticsApi.Dtos;

namespace CSYM505.Api.Controllers.AnalyticsApi.Services
{
    public interface IAnalyticsService
    {
        Task<AnalyticsResult> AnalyzeData();
    }
}
