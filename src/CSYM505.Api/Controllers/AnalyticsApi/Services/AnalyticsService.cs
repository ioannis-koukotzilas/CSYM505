using CSYM505.Api.Controllers.AnalyticsApi.Dtos;
using CSYM505.Model.Entities;
using CSYM505.ORM.Context;
using Microsoft.EntityFrameworkCore;

namespace CSYM505.Api.Controllers.AnalyticsApi.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsResult> AnalyzeData()
        {
            var annualDemand = await AnalyzeAnnualDemand();

            await SaveAnnualDemand(annualDemand);

            var result = new AnalyticsResult { AnnualDemand = annualDemand };

            return result;
        }

        private async Task SaveAnnualDemand(List<AnnualDemand> results)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var existingResults = await _context.AnnualDemand.ToListAsync();

            foreach (var result in results)
            {
                var existingResult = existingResults.FirstOrDefault(existing => existing.Geo == result.Geo && existing.Year == result.Year);

                if (existingResult == null)
                {
                    _context.AnnualDemand.Add(result);
                    continue;
                }

                result.Id = existingResult.Id;

                existingResult.Dataset = result.Dataset;
                existingResult.Frequency = result.Frequency;
                existingResult.Residence = result.Residence;
                existingResult.AccommodationType = result.AccommodationType;
                existingResult.Month = result.Month;
                existingResult.Unit = result.Unit;
                existingResult.Nights = result.Nights;
                existingResult.PreviousYearNights = result.PreviousYearNights;
                existingResult.AnnualChangePercent = result.AnnualChangePercent;
                existingResult.Flags = result.Flags;
                existingResult.PreviousYearFlags = result.PreviousYearFlags;
                existingResult.RawValue = result.RawValue;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        /// <summary>
        /// Αναλύει την ετήσια εξέλιξη των διανυκτερεύσεων και υπολογίζει
        /// την ποσοστιαία μεταβολή τους από το προηγούμενο έτος (K1).
        /// </summary>
        /// <remarks>
        /// Περιοχές:
        /// EL = Ελλάδα.
        /// EL52 = Κεντρική Μακεδονία.
        /// EL527 = Χαλκιδική.
        ///
        /// Περίοδος: 2020–2025.
        ///
        /// Φίλτρα:
        /// freq=A: ετήσια συχνότητα.
        /// c_resid=TOTAL: όλοι οι επισκέπτες, ανεξαρτήτως χώρας κατοικίας.
        /// nace_r2=I551-I553: ξενοδοχεία και παρόμοια καταλύματα,
        /// καταλύματα διακοπών και σύντομης διαμονής, καθώς και χώροι κατασκήνωσης.
        /// unit=NR: πλήθος διανυκτερεύσεων.
        ///
        /// Πηγές:
        /// tour_occ_nin2m για Ελλάδα και Κεντρική Μακεδονία,
        /// με month=TOTAL, δηλαδή το δημοσιευμένο ετήσιο σύνολο.
        /// tour_occ_nin3 για Χαλκιδική, χωρίς διάσταση μήνα.
        ///
        /// K1 = (διανυκτερεύσεις έτους / διανυκτερεύσεις προηγούμενου έτους - 1) * 100.
        /// Η σύγκριση αφορά την ίδια περιοχή και το αμέσως προηγούμενο έτος.
        /// Η μεταβολή παραμένει null όταν λείπει προηγούμενη παρατήρηση,
        /// κάποια τιμή είναι null ή ο παρονομαστής είναι μηδέν.
        /// Για το 2020 παραμένει null, επειδή δεν επιλέγουμε το 2019.
        ///
        /// Οι υπολογισμοί γίνονται με decimal, χωρίς στρογγυλοποίηση.
        /// Διατηρούνται τα flags και το raw περιεχόμενο της πηγής.
        /// </remarks>
        private async Task<List<AnnualDemand>> AnalyzeAnnualDemand()
        {
            var results = await _context
                .MonthlyAccommodationNights.AsNoTracking()
                .Where(entity =>
                    entity.Frequency == "A"
                    && entity.Residence == "TOTAL"
                    && entity.AccommodationType == "I551-I553"
                    && entity.Month == "TOTAL"
                    && entity.Unit == "NR"
                    && (entity.Geo == "EL" || entity.Geo == "EL52")
                    && entity.Year >= 2020
                    && entity.Year <= 2025
                )
                .OrderBy(entity => entity.Geo)
                .ThenBy(entity => entity.Year)
                .Select(entity => new AnnualDemand
                {
                    Dataset = "tour_occ_nin2m",
                    Frequency = entity.Frequency,
                    Residence = entity.Residence,
                    AccommodationType = entity.AccommodationType,
                    Month = entity.Month,
                    Unit = entity.Unit,
                    Geo = entity.Geo,
                    Year = entity.Year,
                    Nights = entity.Value,
                    Flags = entity.Flags,
                    RawValue = entity.RawValue,
                })
                .ToListAsync();

            var chalkidikiResults = await _context
                .AnnualAccommodationNights.AsNoTracking()
                .Where(entity => entity.Frequency == "A" && entity.Residence == "TOTAL" && entity.AccommodationType == "I551-I553" && entity.Unit == "NR" && entity.Geo == "EL527" && entity.Year >= 2020 && entity.Year <= 2025)
                .Select(entity => new AnnualDemand
                {
                    Dataset = "tour_occ_nin3",
                    Frequency = entity.Frequency,
                    Residence = entity.Residence,
                    AccommodationType = entity.AccommodationType,
                    Month = null,
                    Unit = entity.Unit,
                    Geo = entity.Geo,
                    Year = entity.Year,
                    Nights = entity.Value,
                    Flags = entity.Flags,
                    RawValue = entity.RawValue,
                })
                .ToListAsync();

            results.AddRange(chalkidikiResults);

            var orderedResults = results.OrderBy(result => result.Geo).ThenBy(result => result.Year).ToList();

            foreach (var result in orderedResults)
            {
                var previousYearResult = orderedResults.FirstOrDefault(previous => previous.Geo == result.Geo && previous.Year == result.Year - 1);

                if (previousYearResult == null)
                {
                    continue;
                }

                result.PreviousYearNights = previousYearResult.Nights;
                result.PreviousYearFlags = previousYearResult.Flags;

                if (!result.Nights.HasValue || !previousYearResult.Nights.HasValue || previousYearResult.Nights.Value == 0m)
                {
                    continue;
                }

                var annualRatio = result.Nights.Value / previousYearResult.Nights.Value;

                result.AnnualChangePercent = (annualRatio - 1m) * 100m;
            }

            return orderedResults;
        }
    }
}
