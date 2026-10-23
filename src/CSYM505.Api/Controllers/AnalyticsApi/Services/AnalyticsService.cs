using CSYM505.ORM.Context;

namespace CSYM505.Api.Controllers.AnalyticsApi.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public Task AnalyzeData()
        {
            throw new NotImplementedException();
        }
    }
}
