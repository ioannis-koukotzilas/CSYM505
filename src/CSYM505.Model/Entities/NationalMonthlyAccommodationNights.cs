namespace CSYM505.Model.Entities
{
    public class NationalMonthlyAccommodationNights
    {
        public int Id { get; set; }

        public string Frequency { get; set; } = string.Empty;

        public string Residence { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public string AccommodationType { get; set; } = string.Empty;

        public string Geo { get; set; } = string.Empty;

        public string TimePeriod { get; set; } = string.Empty;

        public decimal? Value { get; set; }

        public string Flags { get; set; } = string.Empty;

        public string RawValue { get; set; } = string.Empty;
    }
}
