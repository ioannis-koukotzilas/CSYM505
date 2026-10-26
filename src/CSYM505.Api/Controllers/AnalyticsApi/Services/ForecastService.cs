using System.Globalization;
using CSYM505.Api.Controllers.AnalyticsApi.Dtos;
using CSYM505.Api.Controllers.AnalyticsApi.Helpers;
using CSYM505.Model.Entities;
using CSYM505.ORM.Context;
using Microsoft.EntityFrameworkCore;

namespace CSYM505.Api.Controllers.AnalyticsApi.Services
{
    public class ForecastService : IForecastService
    {
        private readonly AppDbContext _context;
        private readonly ETSForecastRunner _etsForecastRunner;

        public ForecastService(AppDbContext context, ETSForecastRunner etsForecastRunner)
        {
            _context = context;
            _etsForecastRunner = etsForecastRunner;
        }

        /// <summary>
        /// Προετοιμάζει δύο χωριστές σειρές της Κεντρικής Μακεδονίας:
        /// F1 για τουριστικά καταλύματα και F2 για online πλατφόρμες.
        /// </summary>
        /// <remarks>
        /// Επιστρέφει 48 ελεγμένους μήνες ανά σειρά, από 2022-01 έως 2025-12.
        /// Τα έτη 2020–2021 εξαιρούνται λόγω COVID-19, όπως συμφωνήθηκε.
        /// Χρησιμοποιεί ιστορικό 2022–2025 και εκπαιδεύει χωριστό ETS για κάθε σειρά.
        /// Προβλέπει τους 24 μήνες Ιανουαρίου 2026–Δεκεμβρίου 2027.
        /// Αποθηκεύει τις προβλέψεις και των δύο σειρών σε ένα κοινό transaction.
        /// </remarks>
        public async Task<ForecastResult> AnalyzeForecast()
        {
            var startMonth = new DateTime(2022, 1, 1);
            var endMonth = new DateTime(2025, 12, 1);
            var horizon = 24;

            var accommodationObservations = await ReadAccommodationForecastObservations();
            var validatedAccommodationObservations = ValidateForecastObservations(accommodationObservations, "F1", startMonth, endMonth);

            var platformObservations = await ReadPlatformForecastObservations();
            var validatedPlatformObservations = ValidateForecastObservations(platformObservations, "F2", startMonth, endMonth);

            var accommodationPredictions = await CalculateETSForecast(validatedAccommodationObservations, horizon);
            var platformPredictions = await CalculateETSForecast(validatedPlatformObservations, horizon);

            var forecasts = new List<MonthlyForecast>();

            foreach (var prediction in accommodationPredictions)
            {
                forecasts.Add(
                    new MonthlyForecast
                    {
                        Series = "F1",
                        Dataset = "tour_occ_nin2m",
                        Geo = "EL52",
                        Unit = "NR",
                        TrainingStartMonth = startMonth,
                        TrainingEndMonth = endMonth,
                        HorizonMonths = horizon,
                        Month = prediction.Month,
                        Method = prediction.Method,
                        PredictedValue = prediction.PredictedValue,
                    }
                );
            }

            foreach (var prediction in platformPredictions)
            {
                forecasts.Add(
                    new MonthlyForecast
                    {
                        Series = "F2",
                        Dataset = "tour_ce_omn12",
                        Geo = "EL52",
                        Unit = "NR",
                        TrainingStartMonth = startMonth,
                        TrainingEndMonth = endMonth,
                        HorizonMonths = horizon,
                        Month = prediction.Month,
                        Method = prediction.Method,
                        PredictedValue = prediction.PredictedValue,
                    }
                );
            }

            await SaveMonthlyForecast(forecasts);

            return new ForecastResult
            {
                AccommodationObservations = validatedAccommodationObservations,
                PlatformObservations = validatedPlatformObservations,
                AccommodationPredictions = accommodationPredictions,
                PlatformPredictions = platformPredictions,
            };
        }

        /// <summary>
        /// Διαβάζει τη F1: διανυκτερεύσεις σε ξενοδοχεία, καταλύματα διακοπών
        /// και κάμπινγκ της Κεντρικής Μακεδονίας από το tour_occ_nin2m.
        /// </summary>
        /// <remarks>
        /// Φίλτρα: EL52, A, κατοικία TOTAL, I551-I553, NR και μήνες M01–M12.
        /// Η πηγή χρησιμοποιεί συχνότητα A με χωριστό πεδίο μήνα.
        /// Το ετήσιο TOTAL αποκλείεται, ώστε να μη μετρηθεί ως επιπλέον μήνας.
        /// Μετατρέπει έτος/μήνα στην πρώτη ημέρα του μήνα και κρατά τιμή, flags/raw.
        /// Διαβάζει τις διαθέσιμες εγγραφές 2020–2026 χωρίς αλλαγή στη βάση.
        /// Η επιλογή διαστήματος και η απόρριψη άγνωστων τιμών γίνονται στον έλεγχο.
        /// </remarks>
        private async Task<List<MonthlyForecastObservation>> ReadAccommodationForecastObservations()
        {
            var validMonths = new[] { "M01", "M02", "M03", "M04", "M05", "M06", "M07", "M08", "M09", "M10", "M11", "M12" };

            var sourceRows = await _context
                .MonthlyAccommodationNights.AsNoTracking()
                .Where(entity =>
                    entity.Geo == "EL52"
                    && entity.Frequency == "A"
                    && entity.Residence == "TOTAL"
                    && entity.AccommodationType == "I551-I553"
                    && entity.Unit == "NR"
                    && validMonths.Contains(entity.Month)
                    && entity.Year >= 2020
                    && entity.Year <= 2026
                )
                .OrderBy(entity => entity.Year)
                .ThenBy(entity => entity.Month)
                .ToListAsync();

            var observations = new List<MonthlyForecastObservation>();

            foreach (var sourceRow in sourceRows)
            {
                var monthNumber = int.Parse(sourceRow.Month.Substring(1), CultureInfo.InvariantCulture);

                var observation = new MonthlyForecastObservation
                {
                    Month = new DateTime(sourceRow.Year, monthNumber, 1),
                    Value = sourceRow.Value,
                    Flags = sourceRow.Flags,
                    RawValue = sourceRow.RawValue,
                };

                observations.Add(observation);
            }

            return observations;
        }

        /// <summary>
        /// Διαβάζει τη F2: διανυκτερεύσεις επισκεπτών μέσω online πλατφορμών
        /// στην Κεντρική Μακεδονία από το tour_ce_omn12.
        /// </summary>
        /// <remarks>
        /// Φίλτρα: EL52, A, NGT_SP, κατοικία TOTAL, NR και μήνες M01–M12.
        /// Το NGT_SP μετρά διανυκτερεύσεις επισκεπτών· το ετήσιο TOTAL αποκλείεται.
        /// Μετατρέπει έτος/μήνα στην πρώτη ημέρα του μήνα και κρατά τιμή, flags/raw.
        /// Διαβάζει τις διαθέσιμες εγγραφές 2020–2026 χωρίς αλλαγή στη βάση.
        /// Διατηρεί τα null για να ελεγχθούν, αντί να τα μετατρέψει σε μηδέν.
        /// Η F2 παραμένει χωριστή από τη F1, επειδή οι αγορές μπορεί να επικαλύπτονται.
        /// </remarks>
        private async Task<List<MonthlyForecastObservation>> ReadPlatformForecastObservations()
        {
            var validMonths = new[] { "M01", "M02", "M03", "M04", "M05", "M06", "M07", "M08", "M09", "M10", "M11", "M12" };

            var sourceRows = await _context
                .MonthlyPlatformActivity.AsNoTracking()
                .Where(entity =>
                    entity.Geo == "EL52" && entity.Frequency == "A" && entity.Indicator == "NGT_SP" && entity.Residence == "TOTAL" && entity.Unit == "NR" && validMonths.Contains(entity.Month) && entity.Year >= 2020 && entity.Year <= 2026
                )
                .OrderBy(entity => entity.Year)
                .ThenBy(entity => entity.Month)
                .ToListAsync();

            var observations = new List<MonthlyForecastObservation>();

            foreach (var sourceRow in sourceRows)
            {
                var monthNumber = int.Parse(sourceRow.Month.Substring(1), CultureInfo.InvariantCulture);

                var observation = new MonthlyForecastObservation
                {
                    Month = new DateTime(sourceRow.Year, monthNumber, 1),
                    Value = sourceRow.Value,
                    Flags = sourceRow.Flags,
                    RawValue = sourceRow.RawValue,
                };

                observations.Add(observation);
            }

            return observations;
        }

        /// <summary>
        /// Επιλέγει ένα διάστημα και ελέγχει ότι σχηματίζει πλήρη μηνιαία σειρά.
        /// </summary>
        /// <remarks>
        /// Τα όρια δίνονται ως πρώτη ημέρα μήνα και περιλαμβάνονται στο διάστημα.
        /// Επιστρέφει χρονολογική σειρά με μία γνωστή, μη αρνητική τιμή ανά μήνα.
        /// Διπλότυπος ή ελλιπής μήνας, null ή αρνητική τιμή διακόπτει τη διαδικασία.
        /// Το μηδέν είναι έγκυρη παρατήρηση, ενώ το null δηλώνει άγνωστη τιμή.
        /// Δεν συμπληρώνει ή παραλείπει προβληματικούς μήνες, ώστε να διατηρούνται
        /// η μηνιαία συνέχεια και οι πραγματικές τιμές της πηγής.
        /// </remarks>
        private List<MonthlyForecastObservation> ValidateForecastObservations(List<MonthlyForecastObservation> observations, string seriesName, DateTime startMonth, DateTime endMonth)
        {
            var selectedObservations = observations.Where(observation => observation.Month >= startMonth && observation.Month <= endMonth).OrderBy(observation => observation.Month).ToList();

            if (selectedObservations.Count == 0)
            {
                throw new InvalidOperationException($"No observations were found for {seriesName} " + $"between {startMonth:yyyy-MM} and {endMonth:yyyy-MM}.");
            }

            var duplicateMonth = selectedObservations.GroupBy(observation => observation.Month).FirstOrDefault(group => group.Count() > 1);

            if (duplicateMonth != null)
            {
                throw new InvalidOperationException($"Duplicate month {duplicateMonth.Key:yyyy-MM} " + $"was found for {seriesName}.");
            }

            var expectedMonth = startMonth;

            foreach (var observation in selectedObservations)
            {
                if (observation.Month != expectedMonth)
                {
                    throw new InvalidOperationException($"Missing month {expectedMonth:yyyy-MM} " + $"for {seriesName}.");
                }

                if (!observation.Value.HasValue)
                {
                    throw new InvalidOperationException($"Unknown value for {seriesName} " + $"at {observation.Month:yyyy-MM}.");
                }

                if (observation.Value.Value < 0m)
                {
                    throw new InvalidOperationException($"Negative value for {seriesName} " + $"at {observation.Month:yyyy-MM}.");
                }

                expectedMonth = expectedMonth.AddMonths(1);
            }

            if (expectedMonth <= endMonth)
            {
                throw new InvalidOperationException($"Missing month {expectedMonth:yyyy-MM} " + $"for {seriesName}.");
            }

            return selectedObservations;
        }

        /// <summary>
        /// Στέλνει το ελεγμένο ιστορικό στο ETS και αντιστοιχίζει
        /// τις εκτιμήσεις στους μήνες που ακολουθούν το ιστορικό.
        /// </summary>
        private async Task<List<MonthlyForecastPrediction>> CalculateETSForecast(List<MonthlyForecastObservation> trainingObservations, int horizon)
        {
            if (trainingObservations.Count < 24)
            {
                throw new InvalidOperationException("ETS requires at least 24 training months.");
            }

            var values = new List<double>();

            foreach (var observation in trainingObservations)
            {
                var value = observation.Value ?? throw new InvalidOperationException($"Unknown training value at {observation.Month:yyyy-MM}.");

                values.Add((double)value);
            }

            var response = await _etsForecastRunner.RunForecast(values, horizon);

            var lastTrainingMonth = trainingObservations[trainingObservations.Count - 1].Month;

            var predictions = new List<MonthlyForecastPrediction>();

            for (var index = 0; index < response.Predictions.Count; index++)
            {
                var prediction = new MonthlyForecastPrediction
                {
                    Month = lastTrainingMonth.AddMonths(index + 1),
                    Method = response.Method,
                    PredictedValue = response.Predictions[index],
                };

                predictions.Add(prediction);
            }

            return predictions;
        }

        /// <summary>
        /// Αποθηκεύει τις προβλέψεις και τα στοιχεία εκπαίδευσης.
        /// Η επανεκτέλεση ενημερώνει την ίδια σειρά, περιοχή και μήνα.
        /// </summary>
        private async Task SaveMonthlyForecast(List<MonthlyForecast> results)
        {
            if (results.Count == 0)
            {
                throw new InvalidOperationException("No monthly forecasts were provided for saving.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var existingResults = await _context.MonthlyForecast.ToListAsync();

            foreach (var result in results)
            {
                var existingResult = existingResults.SingleOrDefault(existing => existing.Series == result.Series && existing.Geo == result.Geo && existing.Month == result.Month);

                if (existingResult == null)
                {
                    _context.MonthlyForecast.Add(result);
                    continue;
                }

                result.Id = existingResult.Id;

                existingResult.Dataset = result.Dataset;
                existingResult.Unit = result.Unit;
                existingResult.TrainingStartMonth = result.TrainingStartMonth;
                existingResult.TrainingEndMonth = result.TrainingEndMonth;
                existingResult.HorizonMonths = result.HorizonMonths;
                existingResult.Method = result.Method;
                existingResult.PredictedValue = result.PredictedValue;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
    }
}
