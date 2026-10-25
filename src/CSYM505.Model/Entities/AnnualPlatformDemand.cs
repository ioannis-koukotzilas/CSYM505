using System;

namespace CSYM505.Model.Entities
{
    public class AnnualPlatformDemand
    {
        public int Id { get; set; }

        public string Dataset { get; set; } = string.Empty;

        public string Frequency { get; set; } = string.Empty;

        public string Indicator { get; set; } = string.Empty;

        public string Residence { get; set; } = string.Empty;

        public string? Month { get; set; }

        public string Unit { get; set; } = string.Empty;

        public string Geo { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal? GuestNights { get; set; }

        public decimal? PreviousYearGuestNights { get; set; }

        public decimal? AnnualChangePercent { get; set; }

        public string Flags { get; set; } = string.Empty;

        public string? PreviousYearFlags { get; set; }

        public string RawValue { get; set; } = string.Empty;
    }
}
