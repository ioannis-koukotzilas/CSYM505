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
            var monthlyDemand = await AnalyzeMonthlyDemand();
            var annualSeasonality = AnalyzeAnnualSeasonality(monthlyDemand);

            await SaveAnnualDemand(annualDemand);
            await SaveMonthlyDemand(monthlyDemand);
            await SaveAnnualSeasonality(annualSeasonality);

            var result = new AnalyticsResult
            {
                AnnualDemand = annualDemand,
                MonthlyDemand = monthlyDemand,
                AnnualSeasonality = annualSeasonality,
            };

            return result;
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
        /// Αναλύει τη μηνιαία κατανομή των διανυκτερεύσεων και υπολογίζει
        /// το μερίδιο κάθε μήνα στο ετήσιο σύνολο της ίδιας περιοχής (K2).
        /// </summary>
        /// <remarks>
        /// Πηγή: tour_occ_nin2m.
        /// Περιοχές και περίοδοι:
        /// EL = Ελλάδα, 2020–2025.
        /// EL52 = Κεντρική Μακεδονία, 2020–2025.
        /// EL527 = Χαλκιδική, 2024–2025, βάσει διαθέσιμης μηνιαίας κάλυψης.
        ///
        /// Φίλτρα:
        /// freq=A: συχνότητα όπως δημοσιεύεται στην πηγή.
        /// Οι μήνες προσδιορίζονται χωριστά ως M01–M12, Ιανουάριος–Δεκέμβριος.
        /// c_resid=TOTAL: όλοι οι επισκέπτες, ανεξαρτήτως χώρας κατοικίας.
        /// nace_r2=I551-I553: ξενοδοχεία και παρόμοια καταλύματα,
        /// καταλύματα διακοπών και σύντομης διαμονής, καθώς και χώροι κατασκήνωσης.
        /// unit=NR: πλήθος διανυκτερεύσεων.
        ///
        /// K2 = (διανυκτερεύσεις μήνα / ετήσιες διανυκτερεύσεις) * 100.
        /// Ο παρονομαστής είναι το δημοσιευμένο month=TOTAL της ίδιας περιοχής
        /// και χρονιάς, με τα ίδια φίλτρα. Παραμένει το πλήρες ετήσιο σύνολο
        /// ακόμη και όταν στην παρουσίαση επιλέγονται συγκεκριμένοι μήνες.
        ///
        /// Το μερίδιο παραμένει null όταν λείπει η ετήσια παρατήρηση,
        /// κάποια τιμή είναι null ή το ετήσιο σύνολο είναι μηδέν.
        /// Μηδενική μηνιαία τιμή με θετικό ετήσιο σύνολο δίνει μερίδιο 0%.
        ///
        /// Οι υπολογισμοί γίνονται με decimal, χωρίς ρητή στρογγυλοποίηση.
        /// Διατηρούνται τα flags και το raw περιεχόμενο του μήνα και του έτους.
        /// Τα αποτελέσματα επιστρέφονται ταξινομημένα ανά περιοχή, έτος και μήνα.
        /// </remarks>
        private async Task<List<MonthlyDemand>> AnalyzeMonthlyDemand()
        {
            var monthCodes = new[] { "M01", "M02", "M03", "M04", "M05", "M06", "M07", "M08", "M09", "M10", "M11", "M12" };

            var results = await _context
                .MonthlyAccommodationNights.AsNoTracking()
                .Where(entity =>
                    entity.Frequency == "A"
                    && entity.Residence == "TOTAL"
                    && entity.AccommodationType == "I551-I553"
                    && entity.Unit == "NR"
                    && entity.Year >= 2020
                    && entity.Year <= 2025
                    && (entity.Geo == "EL" || entity.Geo == "EL52" || (entity.Geo == "EL527" && entity.Year >= 2024))
                    && monthCodes.Contains(entity.Month)
                )
                .OrderBy(entity => entity.Geo)
                .ThenBy(entity => entity.Year)
                .ThenBy(entity => entity.Month)
                .Select(entity => new MonthlyDemand
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

            var annualTotals = await _context
                .MonthlyAccommodationNights.AsNoTracking()
                .Where(entity =>
                    entity.Frequency == "A"
                    && entity.Residence == "TOTAL"
                    && entity.AccommodationType == "I551-I553"
                    && entity.Unit == "NR"
                    && entity.Month == "TOTAL"
                    && entity.Year >= 2020
                    && entity.Year <= 2025
                    && (entity.Geo == "EL" || entity.Geo == "EL52" || (entity.Geo == "EL527" && entity.Year >= 2024))
                )
                .ToListAsync();

            foreach (var result in results)
            {
                var annualTotal = annualTotals.SingleOrDefault(total => total.Geo == result.Geo && total.Year == result.Year);

                if (annualTotal == null)
                {
                    continue;
                }

                result.AnnualNights = annualTotal.Value;
                result.AnnualFlags = annualTotal.Flags;
                result.AnnualRawValue = annualTotal.RawValue;

                if (!result.Nights.HasValue || !result.AnnualNights.HasValue || result.AnnualNights.Value == 0m)
                {
                    continue;
                }

                var monthlyRatio = result.Nights.Value / result.AnnualNights.Value;

                result.MonthlySharePercent = monthlyRatio * 100m;
            }

            return results;
        }

        private async Task SaveMonthlyDemand(List<MonthlyDemand> results)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var existingResults = await _context.MonthlyDemand.ToListAsync();

            foreach (var result in results)
            {
                var existingResult = existingResults.SingleOrDefault(existing => existing.Geo == result.Geo && existing.Year == result.Year && existing.Month == result.Month);

                if (existingResult == null)
                {
                    _context.MonthlyDemand.Add(result);
                    continue;
                }

                result.Id = existingResult.Id;

                existingResult.Dataset = result.Dataset;
                existingResult.Frequency = result.Frequency;
                existingResult.Residence = result.Residence;
                existingResult.AccommodationType = result.AccommodationType;
                existingResult.Unit = result.Unit;
                existingResult.Nights = result.Nights;
                existingResult.AnnualNights = result.AnnualNights;
                existingResult.MonthlySharePercent = result.MonthlySharePercent;
                existingResult.Flags = result.Flags;
                existingResult.AnnualFlags = result.AnnualFlags;
                existingResult.RawValue = result.RawValue;
                existingResult.AnnualRawValue = result.AnnualRawValue;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        /// <summary>
        /// Υπολογίζει τη συγκέντρωση των διανυκτερεύσεων
        /// την περίοδο Ιουνίου–Σεπτεμβρίου (K3).
        /// </summary>
        /// <remarks>
        /// Χρησιμοποιεί τα αποτελέσματα της AnalyzeMonthlyDemand():
        /// Ελλάδα (EL) και Κεντρική Μακεδονία (EL52), 2020–2025,
        /// Χαλκιδική (EL527), 2024–2025.
        ///
        /// K3 = (άθροισμα διανυκτερεύσεων M06–M09 / ετήσιο TOTAL) * 100.
        /// Απαιτούνται και οι τέσσερις διαφορετικοί μήνες με γνωστές τιμές.
        /// Διαφορετικά το θερινό άθροισμα και το μερίδιο παραμένουν null.
        /// Άγνωστο ή μηδενικό ετήσιο σύνολο δίνει null μερίδιο.
        ///
        /// Τα θερινά flags συνοδεύονται από τον αντίστοιχο μήνα.
        /// Το αναλυτικό raw περιεχόμενο διατηρείται στη MonthlyDemand.
        /// Οι υπολογισμοί γίνονται με decimal, χωρίς ρητή στρογγυλοποίηση.
        /// </remarks>
        private List<AnnualSeasonality> AnalyzeAnnualSeasonality(List<MonthlyDemand> monthlyDemand)
        {
            var results = new List<AnnualSeasonality>();

            if (monthlyDemand.Count == 0)
            {
                return results;
            }

            var summerMonthCodes = new[] { "M06", "M07", "M08", "M09" };

            var groupedMonths = monthlyDemand.GroupBy(item => new { item.Geo, item.Year }).OrderBy(group => group.Key.Geo).ThenBy(group => group.Key.Year);

            foreach (var group in groupedMonths)
            {
                var firstMonth = group.First();

                var result = new AnnualSeasonality
                {
                    Dataset = firstMonth.Dataset,
                    Frequency = firstMonth.Frequency,
                    Residence = firstMonth.Residence,
                    AccommodationType = firstMonth.AccommodationType,
                    Unit = firstMonth.Unit,
                    Geo = group.Key.Geo,
                    Year = group.Key.Year,
                    AnnualNights = firstMonth.AnnualNights,
                    AnnualFlags = firstMonth.AnnualFlags,
                    AnnualRawValue = firstMonth.AnnualRawValue,
                };

                results.Add(result);

                var summerMonths = group.Where(item => summerMonthCodes.Contains(item.Month)).OrderBy(item => item.Month).ToList();

                var distinctSummerMonths = summerMonths.Select(item => item.Month).Distinct().Count();

                if (summerMonths.Count != 4 || distinctSummerMonths != 4)
                {
                    continue;
                }

                result.SummerFlags = string.Join("; ", summerMonths.Where(item => !string.IsNullOrEmpty(item.Flags)).Select(item => $"{item.Month}: {item.Flags}"));

                var summerNights = 0m;
                var hasUnknownValue = false;

                foreach (var month in summerMonths)
                {
                    var monthNights = month.Nights;

                    if (!monthNights.HasValue)
                    {
                        hasUnknownValue = true;
                        break;
                    }

                    summerNights += monthNights.Value;
                }

                if (hasUnknownValue)
                {
                    continue;
                }

                result.SummerNights = summerNights;

                if (!result.AnnualNights.HasValue || result.AnnualNights.Value == 0m)
                {
                    continue;
                }

                var summerRatio = summerNights / result.AnnualNights.Value;

                result.SummerSharePercent = summerRatio * 100m;
            }

            return results;
        }

        private async Task SaveAnnualSeasonality(List<AnnualSeasonality> results)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var existingResults = await _context.AnnualSeasonality.ToListAsync();

            foreach (var result in results)
            {
                var existingResult = existingResults.SingleOrDefault(existing => existing.Geo == result.Geo && existing.Year == result.Year);

                if (existingResult == null)
                {
                    _context.AnnualSeasonality.Add(result);
                    continue;
                }

                result.Id = existingResult.Id;

                existingResult.Dataset = result.Dataset;
                existingResult.Frequency = result.Frequency;
                existingResult.Residence = result.Residence;
                existingResult.AccommodationType = result.AccommodationType;
                existingResult.Unit = result.Unit;
                existingResult.SummerNights = result.SummerNights;
                existingResult.AnnualNights = result.AnnualNights;
                existingResult.SummerSharePercent = result.SummerSharePercent;
                existingResult.SummerFlags = result.SummerFlags;
                existingResult.AnnualFlags = result.AnnualFlags;
                existingResult.AnnualRawValue = result.AnnualRawValue;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
    }
}
