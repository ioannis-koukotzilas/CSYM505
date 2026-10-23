namespace CSYM505.Api.Controllers.ImportDataApi
{
    public interface IImportDataService
    {
        Task<List<ImportDataResult>> ImportData();
    }
}
