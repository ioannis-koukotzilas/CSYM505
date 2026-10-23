namespace CSYM505.Api.Controllers.ImportDataApi
{
    public class ImportDataResult
    {
        public string Dataset { get; set; } = string.Empty;

        public int ImportedRows { get; set; }
    }
}
