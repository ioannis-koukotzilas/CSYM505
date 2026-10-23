namespace CSYM505.Api.Controllers.DataImportApi
{
    using CSYM505.Api.Controllers.ImportDataApi;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api")]
    public class EurostatImportController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public EurostatImportController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpPost]
        [Route("import-data")]
        public async Task<IActionResult> ImportData()
        {
            try
            {
                var service = _serviceProvider.GetRequiredService<IImportDataService>();
                var results = await service.ImportData();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
