using System;

namespace CSYM505.Model.Entities
{
    public class MonthlyForecast
    {
        public int Id { get; set; }

        public string Series { get; set; } = string.Empty;

        public string Dataset { get; set; } = string.Empty;

        public string Geo { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public DateTime TrainingStartMonth { get; set; }

        public DateTime TrainingEndMonth { get; set; }

        public int HorizonMonths { get; set; }

        public DateTime Month { get; set; }

        public string Method { get; set; } = string.Empty;

        public double PredictedValue { get; set; }
    }
}
