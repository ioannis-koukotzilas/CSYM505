using CSYM505.Model.Entities;

namespace CSYM505.Api.Controllers.AnalyticsApi.Dtos
{
    public class AnalyticsResult
    {
        public List<AnnualDemand> AnnualDemand { get; set; } = [];

        public List<MonthlyDemand> MonthlyDemand { get; set; } = [];

        public List<AnnualSeasonality> AnnualSeasonality { get; set; } = [];

        public List<VisitorComposition> VisitorComposition { get; set; } = [];

        public List<MonthlyLengthOfStay> MonthlyLengthOfStay { get; set; } = [];

        public List<AnnualHotelRooms> AnnualHotelRooms { get; set; } = [];

        public List<AnnualRoomOccupancy> AnnualRoomOccupancy { get; set; } = [];

        public List<AnnualPlatformDemand> AnnualPlatformDemand { get; set; } = [];
    }
}
