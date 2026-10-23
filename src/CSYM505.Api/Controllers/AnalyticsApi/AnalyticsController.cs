using CSYM505.Api.Controllers.AnalyticsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CSYM505.Api.Controllers.AnalyticsApi
{
    [ApiController]
    [Route("api")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public AnalyticsController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpPost]
        [Route("analyze-data")]
        public async Task<IActionResult> AnalyzeData()
        {
            try
            {
                var service = _serviceProvider.GetRequiredService<IAnalyticsService>();
                await service.AnalyzeData();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
