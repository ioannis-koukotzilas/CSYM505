namespace CSYM505.Model.Entities
{
    public class AnnualRegionalTravelReceipts
    {
        public int Id { get; set; }

        public string Frequency { get; set; } = string.Empty;

        public string Geo { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public int Year { get; set; }

        public string Unit { get; set; } = string.Empty;

        public decimal? Value { get; set; }

        public string Flags { get; set; } = string.Empty;

        public string RawValue { get; set; } = string.Empty;
    }
}
