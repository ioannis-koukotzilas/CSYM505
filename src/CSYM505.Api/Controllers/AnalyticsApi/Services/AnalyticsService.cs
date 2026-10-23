using CSYM505.Api.Controllers.AnalyticsApi.Dtos;
using CSYM505.Model.Entities;
using CSYM505.ORM.Context;
using MathNet.Numerics.Statistics;
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
            var visitorComposition = await AnalyzeVisitorComposition();
            var monthlyLengthOfStay = await AnalyzeMonthlyLengthOfStay();

            await SaveAnnualDemand(annualDemand);
            await SaveMonthlyDemand(monthlyDemand);
            await SaveAnnualSeasonality(annualSeasonality);
            await SaveVisitorComposition(visitorComposition);
            await SaveMonthlyLengthOfStay(monthlyLengthOfStay);

            var result = new AnalyticsResult
            {
                AnnualDemand = annualDemand,
                MonthlyDemand = monthlyDemand,
                AnnualSeasonality = annualSeasonality,
                VisitorComposition = visitorComposition,
                MonthlyLengthOfStay = monthlyLengthOfStay,
            };

            return result;
        }

        #region Annual Demand
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
        #endregion

        #region Monthly Demand
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
        #endregion

        #region Annual Seasonality
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

                CalculateSeasonalityStatistics(group.ToList(), result);

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

        /// <summary>
        /// Υπολογίζει τη μέση μηνιαία ζήτηση, την πληθυσμιακή τυπική απόκλιση
        /// και τον συντελεστή μεταβλητότητας για μία περιοχή και χρονιά.
        /// </summary>
        /// <remarks>
        /// Χρησιμοποιεί τις διανυκτερεύσεις των δώδεκα μηνών M01–M12.
        /// Απαιτεί πλήρες δωδεκάμηνο, χωρίς διπλότυπα ή άγνωστες τιμές.
        /// Οι μηδενικές τιμές είναι έγκυρες και συμμετέχουν στον υπολογισμό.
        ///
        /// Χρησιμοποιείται πληθυσμιακή τυπική απόκλιση, επειδή εξετάζουμε
        /// όλους τους μήνες του έτους.
        /// Συντελεστής μεταβλητότητας = τυπική απόκλιση / μέση τιμή * 100.
        /// Μηδενική μέση τιμή αφήνει τον συντελεστή μεταβλητότητας null.
        ///
        /// Οι τιμές μετατρέπονται σε double για τη χρήση του Math.NET.
        /// Δεν εφαρμόζεται ρητή στρογγυλοποίηση.
        /// </remarks>
        private void CalculateSeasonalityStatistics(List<MonthlyDemand> monthlyDemand, AnnualSeasonality result)
        {
            if (monthlyDemand.Count != 12)
            {
                return;
            }

            var distinctMonths = monthlyDemand.Select(item => item.Month).Distinct().Count();

            if (distinctMonths != 12)
            {
                return;
            }

            var monthCodes = new[] { "M01", "M02", "M03", "M04", "M05", "M06", "M07", "M08", "M09", "M10", "M11", "M12" };

            var monthlyNights = new List<double>();

            foreach (var month in monthlyDemand)
            {
                if (!monthCodes.Contains(month.Month))
                {
                    return;
                }

                var monthNights = month.Nights;

                if (!monthNights.HasValue)
                {
                    return;
                }

                monthlyNights.Add((double)monthNights.Value);
            }

            var meanMonthlyNights = Statistics.Mean(monthlyNights);

            var populationStandardDeviation = Statistics.PopulationStandardDeviation(monthlyNights);

            result.MeanMonthlyNights = meanMonthlyNights;
            result.PopulationStandardDeviation = populationStandardDeviation;

            if (meanMonthlyNights == 0d)
            {
                return;
            }

            result.CoefficientOfVariationPercent = populationStandardDeviation / meanMonthlyNights * 100d;
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

                existingResult.MeanMonthlyNights = result.MeanMonthlyNights;
                existingResult.PopulationStandardDeviation = result.PopulationStandardDeviation;
                existingResult.CoefficientOfVariationPercent = result.CoefficientOfVariationPercent;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        #endregion

        #region Visitor Composition
        /// <summary>
        /// Διαβάζει την ετήσια σύνθεση των διανυκτερεύσεων τουριστικών καταλυμάτων
        /// κατά χώρα κατοικίας επισκέπτη από το tour_occ_nin2m.
        /// </summary>
        /// <remarks>
        /// Καλύπτει Ελλάδα (EL) και Κεντρική Μακεδονία (EL52) για το 2020–2025
        /// και Χαλκιδική (EL527) για το 2024–2025.
        /// Χρησιμοποιεί ετήσια στοιχεία (freq=A), ετήσιο σύνολο μηνών (month=TOTAL),
        /// ξενοδοχεία, καταλύματα διακοπών και κάμπινγκ (I551-I553),
        /// με μονάδα τον αριθμό διανυκτερεύσεων (NR).
        /// Οι κατηγορίες DOM, FOR και TOTAL αφορούν κατοίκους Ελλάδας,
        /// κατοίκους εξωτερικού και το σύνολό τους αντίστοιχα.
        /// Διατηρεί χωριστά τις τιμές, τα flags και το raw περιεχόμενο.
        /// Ο K4 υπολογίζεται ως διανυκτερεύσεις κατοίκων εξωτερικού
        /// προς συνολικές διανυκτερεύσεις, επί 100.
        /// Παραμένει null όταν λείπει απαιτούμενη τιμή ή το σύνολο είναι μηδέν.
        /// Ο υπολογισμός γίνεται με decimal, χωρίς ρητή στρογγυλοποίηση.
        /// </remarks>
        private async Task<List<VisitorComposition>> AnalyzeVisitorComposition()
        {
            var residenceCodes = new[] { "DOM", "FOR", "TOTAL" };

            var observations = await _context
                .MonthlyAccommodationNights.AsNoTracking()
                .Where(entity =>
                    entity.Frequency == "A"
                    && entity.AccommodationType == "I551-I553"
                    && entity.Month == "TOTAL"
                    && entity.Unit == "NR"
                    && entity.Year >= 2020
                    && entity.Year <= 2025
                    && (entity.Geo == "EL" || entity.Geo == "EL52" || (entity.Geo == "EL527" && entity.Year >= 2024))
                    && residenceCodes.Contains(entity.Residence)
                )
                .ToListAsync();

            var groupedObservations = observations.GroupBy(entity => new { entity.Geo, entity.Year }).OrderBy(group => group.Key.Geo).ThenBy(group => group.Key.Year);

            var results = new List<VisitorComposition>();

            foreach (var group in groupedObservations)
            {
                var firstObservation = group.First();

                var domestic = group.SingleOrDefault(entity => entity.Residence == "DOM");
                var foreign = group.SingleOrDefault(entity => entity.Residence == "FOR");
                var total = group.SingleOrDefault(entity => entity.Residence == "TOTAL");

                var result = new VisitorComposition
                {
                    Dataset = "tour_occ_nin2m",
                    Frequency = firstObservation.Frequency,
                    AccommodationType = firstObservation.AccommodationType,
                    Month = firstObservation.Month,
                    Unit = firstObservation.Unit,
                    Geo = group.Key.Geo,
                    Year = group.Key.Year,
                    DomesticNights = domestic?.Value,
                    ForeignNights = foreign?.Value,
                    TotalNights = total?.Value,
                    DomesticFlags = domestic?.Flags,
                    ForeignFlags = foreign?.Flags,
                    TotalFlags = total?.Flags,
                    DomesticRawValue = domestic?.RawValue,
                    ForeignRawValue = foreign?.RawValue,
                    TotalRawValue = total?.RawValue,
                };

                results.Add(result);

                var foreignNights = result.ForeignNights;
                var totalNights = result.TotalNights;

                if (!foreignNights.HasValue || !totalNights.HasValue || totalNights.Value == 0m)
                {
                    continue;
                }

                var foreignRatio = foreignNights.Value / totalNights.Value;

                result.ForeignSharePercent = foreignRatio * 100m;
            }

            return results;
        }

        private async Task SaveVisitorComposition(List<VisitorComposition> results)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var existingResults = await _context.VisitorComposition.ToListAsync();

            foreach (var result in results)
            {
                var existingResult = existingResults.SingleOrDefault(existing => existing.Geo == result.Geo && existing.Year == result.Year);

                if (existingResult == null)
                {
                    _context.VisitorComposition.Add(result);
                    continue;
                }

                result.Id = existingResult.Id;

                existingResult.Dataset = result.Dataset;
                existingResult.Frequency = result.Frequency;
                existingResult.AccommodationType = result.AccommodationType;
                existingResult.Month = result.Month;
                existingResult.Unit = result.Unit;

                existingResult.DomesticNights = result.DomesticNights;
                existingResult.ForeignNights = result.ForeignNights;
                existingResult.TotalNights = result.TotalNights;
                existingResult.ForeignSharePercent = result.ForeignSharePercent;

                existingResult.DomesticFlags = result.DomesticFlags;
                existingResult.ForeignFlags = result.ForeignFlags;
                existingResult.TotalFlags = result.TotalFlags;

                existingResult.DomesticRawValue = result.DomesticRawValue;
                existingResult.ForeignRawValue = result.ForeignRawValue;
                existingResult.TotalRawValue = result.TotalRawValue;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        #endregion

        #region Monthly Length Of Stay
        /// <summary>
        /// Διαβάζει τις μηνιαίες διανυκτερεύσεις για την ανάλυση
        /// της μέσης διάρκειας παραμονής στην Ελλάδα.
        /// </summary>
        /// <remarks>
        /// Χρησιμοποιεί το tour_occ_nim για Ιανουάριο 2020–Δεκέμβριο 2025.
        /// Επιλέγει Ελλάδα (EL), ξενοδοχεία και παρόμοια καταλύματα (I551),
        /// όλες τις κατηγορίες κατοικίας επισκέπτη μαζί (c_resid=TOTAL),
        /// μηνιαία συχνότητα (M) και αριθμό διανυκτερεύσεων (NR).
        /// Διατηρεί τις άγνωστες τιμές, τα flags και το raw περιεχόμενο.
        /// Αντιστοιχίζει αφίξεις από το tour_occ_arm με τα ίδια φίλτρα,
        /// για τον ίδιο μήνα και περιοχή.
        /// Η μονάδα NR αφορά αριθμό διανυκτερεύσεων στην πρώτη πηγή
        /// και αριθμό αφίξεων στη δεύτερη.
        /// Ο K5 υπολογίζεται ως διανυκτερεύσεις προς αφίξεις,
        /// με αποτέλεσμα σε νύχτες ανά άφιξη.
        /// Παραμένει null όταν λείπει απαιτούμενη τιμή ή οι αφίξεις είναι μηδέν.
        /// Ο υπολογισμός γίνεται με decimal, χωρίς ρητή στρογγυλοποίηση.
        /// </remarks>
        private async Task<List<MonthlyLengthOfStay>> AnalyzeMonthlyLengthOfStay()
        {
            var results = await _context
                .NationalMonthlyAccommodationNights.AsNoTracking()
                .Where(entity =>
                    entity.Frequency == "M"
                    && entity.Residence == "TOTAL"
                    && entity.AccommodationType == "I551"
                    && entity.Unit == "NR"
                    && entity.Geo == "EL"
                    && entity.TimePeriod.CompareTo("2020-01") >= 0
                    && entity.TimePeriod.CompareTo("2025-12") <= 0
                )
                .OrderBy(entity => entity.TimePeriod)
                .Select(entity => new MonthlyLengthOfStay
                {
                    NightsDataset = "tour_occ_nim",
                    ArrivalsDataset = "tour_occ_arm",
                    Frequency = entity.Frequency,
                    Residence = entity.Residence,
                    AccommodationType = entity.AccommodationType,
                    Unit = entity.Unit,
                    Geo = entity.Geo,
                    TimePeriod = entity.TimePeriod,
                    Nights = entity.Value,
                    NightsFlags = entity.Flags,
                    NightsRawValue = entity.RawValue,
                })
                .ToListAsync();

            var arrivals = await _context
                .NationalMonthlyAccommodationArrivals.AsNoTracking()
                .Where(entity =>
                    entity.Frequency == "M"
                    && entity.Residence == "TOTAL"
                    && entity.AccommodationType == "I551"
                    && entity.Unit == "NR"
                    && entity.Geo == "EL"
                    && entity.TimePeriod.CompareTo("2020-01") >= 0
                    && entity.TimePeriod.CompareTo("2025-12") <= 0
                )
                .ToListAsync();

            foreach (var result in results)
            {
                var monthlyArrivals = arrivals.SingleOrDefault(entity => entity.Geo == result.Geo && entity.TimePeriod == result.TimePeriod);

                if (monthlyArrivals == null)
                {
                    continue;
                }

                result.Arrivals = monthlyArrivals.Value;
                result.ArrivalsFlags = monthlyArrivals.Flags;
                result.ArrivalsRawValue = monthlyArrivals.RawValue;

                var nights = result.Nights;
                var arrivalsCount = result.Arrivals;

                if (!nights.HasValue || !arrivalsCount.HasValue || arrivalsCount.Value == 0m)
                {
                    continue;
                }

                result.AverageStayNights = nights.Value / arrivalsCount.Value;
            }

            return results;
        }

        private async Task SaveMonthlyLengthOfStay(List<MonthlyLengthOfStay> results)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var existingResults = await _context.MonthlyLengthOfStay.ToListAsync();

            foreach (var result in results)
            {
                var existingResult = existingResults.SingleOrDefault(existing => existing.Geo == result.Geo && existing.TimePeriod == result.TimePeriod);

                if (existingResult == null)
                {
                    _context.MonthlyLengthOfStay.Add(result);
                    continue;
                }

                result.Id = existingResult.Id;

                existingResult.NightsDataset = result.NightsDataset;
                existingResult.ArrivalsDataset = result.ArrivalsDataset;
                existingResult.Frequency = result.Frequency;
                existingResult.Residence = result.Residence;
                existingResult.AccommodationType = result.AccommodationType;
                existingResult.Unit = result.Unit;

                existingResult.Nights = result.Nights;
                existingResult.Arrivals = result.Arrivals;
                existingResult.AverageStayNights = result.AverageStayNights;

                existingResult.NightsFlags = result.NightsFlags;
                existingResult.ArrivalsFlags = result.ArrivalsFlags;

                existingResult.NightsRawValue = result.NightsRawValue;
                existingResult.ArrivalsRawValue = result.ArrivalsRawValue;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        #endregion
    }
}
