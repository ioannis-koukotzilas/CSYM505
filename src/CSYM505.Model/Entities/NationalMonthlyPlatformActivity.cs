namespace CSYM505.Model.Entities
{
    public class NationalMonthlyPlatformActivity
    {
        public int Id { get; set; }

        public string Frequency { get; set; } = string.Empty;

        public string Indicator { get; set; } = string.Empty;

        public string Residence { get; set; } = string.Empty;

        public string Month { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public string Geo { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal? Value { get; set; }

        public string Flags { get; set; } = string.Empty;

        public string RawValue { get; set; } = string.Empty;
    }
}
