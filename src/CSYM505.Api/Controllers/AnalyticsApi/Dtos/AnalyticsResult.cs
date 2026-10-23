using CSYM505.Model.Entities;

namespace CSYM505.Api.Controllers.AnalyticsApi.Dtos
{
    public class AnalyticsResult
    {
        public List<AnnualDemand> AnnualDemand { get; set; } = [];

        public List<MonthlyDemand> MonthlyDemand { get; set; } = [];

        public List<AnnualSeasonality> AnnualSeasonality { get; set; } = [];
    }
}
