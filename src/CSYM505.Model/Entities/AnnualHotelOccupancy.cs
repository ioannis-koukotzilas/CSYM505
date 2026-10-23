namespace CSYM505.Model.Entities
{
    public class AnnualHotelOccupancy
    {
        public int Id { get; set; }

        public string Frequency { get; set; } = string.Empty;

        public string AccommodationUnit { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public string Geo { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal? Value { get; set; }

        public string Flags { get; set; } = string.Empty;

        public string RawValue { get; set; } = string.Empty;
    }
}
