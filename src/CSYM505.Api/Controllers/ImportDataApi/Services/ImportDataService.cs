namespace CSYM505.Api.Controllers.ImportDataApi
{
    using System.Globalization;
    using CSYM505.Api.Helpers;
    using CSYM505.Model.Entities;
    using CSYM505.ORM.Context;
    using Microsoft.EntityFrameworkCore;

    public class ImportDataService : IImportDataService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ImportDataService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<List<ImportDataResult>> ImportData()
        {
            var results = new List<ImportDataResult>();

            var annualNightsRows = await ImportTourOccNin3();
            results.Add(new ImportDataResult { Dataset = "tour_occ_nin3", ImportedRows = annualNightsRows });

            var monthlyNightsRows = await ImportTourOccNin2m();
            results.Add(new ImportDataResult { Dataset = "tour_occ_nin2m", ImportedRows = monthlyNightsRows });

            var nationalMonthlyNightsRows = await ImportTourOccNim();
            results.Add(new ImportDataResult { Dataset = "tour_occ_nim", ImportedRows = nationalMonthlyNightsRows });

            var nationalMonthlyArrivalsRows = await ImportTourOccArm();
            results.Add(new ImportDataResult { Dataset = "tour_occ_arm", ImportedRows = nationalMonthlyArrivalsRows });

            var annualArrivalsRows = await ImportTourOccArn2();
            results.Add(new ImportDataResult { Dataset = "tour_occ_arn2", ImportedRows = annualArrivalsRows });

            var monthlyPlatformActivityRows = await ImportTourCeOmn12();
            results.Add(new ImportDataResult { Dataset = "tour_ce_omn12", ImportedRows = monthlyPlatformActivityRows });

            var nationalMonthlyPlatformActivityRows = await ImportTourCeOmr();
            results.Add(new ImportDataResult { Dataset = "tour_ce_omr", ImportedRows = nationalMonthlyPlatformActivityRows });

            var annualPlatformGuestNightsRows = await ImportTourCeOan3();
            results.Add(new ImportDataResult { Dataset = "tour_ce_oan3", ImportedRows = annualPlatformGuestNightsRows });

            var annualCapacityRows = await ImportTourCapNuts2();
            results.Add(new ImportDataResult { Dataset = "tour_cap_nuts2", ImportedRows = annualCapacityRows });

            var annualOccupancyRows = await ImportTourOccAnor2();
            results.Add(new ImportDataResult { Dataset = "tour_occ_anor2", ImportedRows = annualOccupancyRows });

            var travelReceiptsRows = await ImportBankOfGreeceRegionalTravelReceipts();
            results.Add(new ImportDataResult { Dataset = "bog_regional_travel_receipts", ImportedRows = travelReceiptsRows });

            var expenditurePerNightRows = await ImportBankOfGreeceRegionalExpenditurePerNight();
            results.Add(new ImportDataResult { Dataset = "bog_regional_expenditure_per_night", ImportedRows = expenditurePerNightRows });

            var lengthOfStayRows = await ImportBankOfGreeceRegionalLengthOfStay();
            results.Add(new ImportDataResult { Dataset = "bog_regional_length_of_stay", ImportedRows = lengthOfStayRows });

            return results;
        }

        private async Task<int> ImportTourOccNin3()
        {
            var entities = await ReadTourOccNin3();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.AnnualAccommodationNights.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.AnnualAccommodationNights.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<AnnualAccommodationNights>> ReadTourOccNin3()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "tour_occ_nin3.csv"));

            var expectedColumns = new[] { "freq", "c_resid", "unit", "nace_r2", "geo", "year", "value", "flags", "raw_value" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<AnnualAccommodationNights>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var residence = row[1];
                var unit = row[2];
                var accommodationType = row[3];
                var geo = row[4];

                if (!geo.StartsWith("EL", StringComparison.Ordinal))
                {
                    continue;
                }

                var valueText = row[6];

                if (frequency != "A" || string.IsNullOrWhiteSpace(residence) || string.IsNullOrWhiteSpace(unit) || string.IsNullOrWhiteSpace(accommodationType) || string.IsNullOrWhiteSpace(geo) || string.IsNullOrWhiteSpace(row[8]))
                {
                    throw new FormatException($"Missing or invalid dimensions at CSV row {rowNumber}.");
                }

                if (!int.TryParse(row[5], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1 || year > 9999)
                {
                    throw new FormatException($"Invalid year at CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (!string.IsNullOrEmpty(valueText))
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        throw new FormatException($"Invalid value at CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                if (unit == "NR" && value.HasValue && (value.Value < 0 || decimal.Truncate(value.Value) != value.Value))
                {
                    throw new FormatException($"The number of nights must be a non-negative integer at CSV row {rowNumber}.");
                }

                var entity = new AnnualAccommodationNights
                {
                    Frequency = frequency,
                    Residence = residence,
                    Unit = unit,
                    AccommodationType = accommodationType,
                    Geo = geo,
                    Year = year,
                    Value = value,
                    Flags = row[7],
                    RawValue = row[8],
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No Greek data rows were found in the CSV. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportTourOccNin2m()
        {
            var entities = await ReadTourOccNin2m();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.MonthlyAccommodationNights.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.MonthlyAccommodationNights.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<MonthlyAccommodationNights>> ReadTourOccNin2m()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "tour_occ_nin2m.csv"));

            var expectedColumns = new[] { "freq", "c_resid", "nace_r2", "month", "unit", "geo", "year", "value", "flags", "raw_value" };
            var validMonths = new[] { "M01", "M02", "M03", "M04", "M05", "M06", "M07", "M08", "M09", "M10", "M11", "M12", "TOTAL" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<MonthlyAccommodationNights>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var residence = row[1];
                var accommodationType = row[2];
                var month = row[3];
                var unit = row[4];
                var geo = row[5];

                if (!geo.StartsWith("EL", StringComparison.Ordinal))
                {
                    continue;
                }

                var valueText = row[7];

                if (frequency != "A" || string.IsNullOrWhiteSpace(residence) || string.IsNullOrWhiteSpace(unit) || string.IsNullOrWhiteSpace(accommodationType) || string.IsNullOrWhiteSpace(row[9]))
                {
                    throw new FormatException($"Missing or invalid dimensions at tour_occ_nin2m CSV row {rowNumber}.");
                }

                if (!validMonths.Contains(month))
                {
                    throw new FormatException($"Invalid month at tour_occ_nin2m CSV row {rowNumber}.");
                }

                if (!int.TryParse(row[6], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1 || year > 9999)
                {
                    throw new FormatException($"Invalid year at tour_occ_nin2m CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (!string.IsNullOrEmpty(valueText))
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        throw new FormatException($"Invalid value at tour_occ_nin2m CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                if (unit == "NR" && value.HasValue && (value.Value < 0 || decimal.Truncate(value.Value) != value.Value))
                {
                    throw new FormatException($"The number of nights must be a non-negative integer at tour_occ_nin2m CSV row {rowNumber}.");
                }

                var entity = new MonthlyAccommodationNights
                {
                    Frequency = frequency,
                    Residence = residence,
                    AccommodationType = accommodationType,
                    Month = month,
                    Unit = unit,
                    Geo = geo,
                    Year = year,
                    Value = value,
                    Flags = row[8],
                    RawValue = row[9],
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No Greek data rows were found in tour_occ_nin2m.csv. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportTourOccNim()
        {
            var entities = await ReadTourOccNim();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.NationalMonthlyAccommodationNights.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.NationalMonthlyAccommodationNights.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<NationalMonthlyAccommodationNights>> ReadTourOccNim()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "tour_occ_nim.csv"));

            var expectedColumns = new[] { "freq", "c_resid", "unit", "nace_r2", "geo", "time_period", "value", "flags", "raw_value" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<NationalMonthlyAccommodationNights>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var residence = row[1];
                var unit = row[2];
                var accommodationType = row[3];
                var geo = row[4];

                if (!geo.StartsWith("EL", StringComparison.Ordinal))
                {
                    continue;
                }

                var timePeriod = row[5];
                var valueText = row[6];

                if (frequency != "M" || string.IsNullOrWhiteSpace(residence) || string.IsNullOrWhiteSpace(unit) || string.IsNullOrWhiteSpace(accommodationType) || string.IsNullOrWhiteSpace(row[8]))
                {
                    throw new FormatException($"Missing or invalid dimensions at tour_occ_nim CSV row {rowNumber}.");
                }

                if (!DateTime.TryParseExact(timePeriod, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    throw new FormatException($"Invalid monthly period at tour_occ_nim CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (!string.IsNullOrEmpty(valueText))
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        throw new FormatException($"Invalid value at tour_occ_nim CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                if (unit == "NR" && value.HasValue && (value.Value < 0 || decimal.Truncate(value.Value) != value.Value))
                {
                    throw new FormatException($"The number of nights must be a non-negative integer at tour_occ_nim CSV row {rowNumber}.");
                }

                var entity = new NationalMonthlyAccommodationNights
                {
                    Frequency = frequency,
                    Residence = residence,
                    Unit = unit,
                    AccommodationType = accommodationType,
                    Geo = geo,
                    TimePeriod = timePeriod,
                    Value = value,
                    Flags = row[7],
                    RawValue = row[8],
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No Greek data rows were found in tour_occ_nim.csv. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportTourOccArm()
        {
            var entities = await ReadTourOccArm();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.NationalMonthlyAccommodationArrivals.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.NationalMonthlyAccommodationArrivals.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<NationalMonthlyAccommodationArrivals>> ReadTourOccArm()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "tour_occ_arm.csv"));

            var expectedColumns = new[] { "freq", "c_resid", "unit", "nace_r2", "geo", "time_period", "value", "flags", "raw_value" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<NationalMonthlyAccommodationArrivals>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var residence = row[1];
                var unit = row[2];
                var accommodationType = row[3];
                var geo = row[4];

                if (!geo.StartsWith("EL", StringComparison.Ordinal))
                {
                    continue;
                }

                var timePeriod = row[5];
                var valueText = row[6];

                if (frequency != "M" || string.IsNullOrWhiteSpace(residence) || string.IsNullOrWhiteSpace(unit) || string.IsNullOrWhiteSpace(accommodationType) || string.IsNullOrWhiteSpace(row[8]))
                {
                    throw new FormatException($"Missing or invalid dimensions at tour_occ_arm CSV row {rowNumber}.");
                }

                if (!DateTime.TryParseExact(timePeriod, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    throw new FormatException($"Invalid monthly period at tour_occ_arm CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (!string.IsNullOrEmpty(valueText))
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        throw new FormatException($"Invalid value at tour_occ_arm CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                if (unit == "NR" && value.HasValue && (value.Value < 0 || decimal.Truncate(value.Value) != value.Value))
                {
                    throw new FormatException($"The number of arrivals must be a non-negative integer at tour_occ_arm CSV row {rowNumber}.");
                }

                var entity = new NationalMonthlyAccommodationArrivals
                {
                    Frequency = frequency,
                    Residence = residence,
                    Unit = unit,
                    AccommodationType = accommodationType,
                    Geo = geo,
                    TimePeriod = timePeriod,
                    Value = value,
                    Flags = row[7],
                    RawValue = row[8],
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No Greek data rows were found in tour_occ_arm.csv. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportTourOccArn2()
        {
            var entities = await ReadTourOccArn2();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.AnnualAccommodationArrivals.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.AnnualAccommodationArrivals.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<AnnualAccommodationArrivals>> ReadTourOccArn2()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "tour_occ_arn2.csv"));

            var expectedColumns = new[] { "freq", "c_resid", "unit", "nace_r2", "geo", "year", "value", "flags", "raw_value" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<AnnualAccommodationArrivals>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var residence = row[1];
                var unit = row[2];
                var accommodationType = row[3];
                var geo = row[4];

                if (!geo.StartsWith("EL", StringComparison.Ordinal))
                {
                    continue;
                }

                var valueText = row[6];

                if (frequency != "A" || string.IsNullOrWhiteSpace(residence) || string.IsNullOrWhiteSpace(unit) || string.IsNullOrWhiteSpace(accommodationType) || string.IsNullOrWhiteSpace(row[8]))
                {
                    throw new FormatException($"Missing or invalid dimensions at tour_occ_arn2 CSV row {rowNumber}.");
                }

                if (!int.TryParse(row[5], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1 || year > 9999)
                {
                    throw new FormatException($"Invalid year at tour_occ_arn2 CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (!string.IsNullOrEmpty(valueText))
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        throw new FormatException($"Invalid value at tour_occ_arn2 CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                if (unit == "NR" && value.HasValue && (value.Value < 0 || decimal.Truncate(value.Value) != value.Value))
                {
                    throw new FormatException($"The number of arrivals must be a non-negative integer at tour_occ_arn2 CSV row {rowNumber}.");
                }

                var entity = new AnnualAccommodationArrivals
                {
                    Frequency = frequency,
                    Residence = residence,
                    Unit = unit,
                    AccommodationType = accommodationType,
                    Geo = geo,
                    Year = year,
                    Value = value,
                    Flags = row[7],
                    RawValue = row[8],
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No Greek data rows were found in tour_occ_arn2.csv. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportTourCeOmn12()
        {
            var entities = await ReadTourCeOmn12();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.MonthlyPlatformActivity.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.MonthlyPlatformActivity.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<MonthlyPlatformActivity>> ReadTourCeOmn12()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "tour_ce_omn12.csv"));

            var expectedColumns = new[] { "freq", "indic_to", "c_resid", "month", "unit", "geo", "year", "value", "flags", "raw_value" };
            var validIndicators = new[] { "LSTY", "NGT_SP", "STY" };
            var validMonths = new[] { "M01", "M02", "M03", "M04", "M05", "M06", "M07", "M08", "M09", "M10", "M11", "M12", "TOTAL" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<MonthlyPlatformActivity>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var indicator = row[1];
                var residence = row[2];
                var month = row[3];
                var unit = row[4];
                var geo = row[5];

                if (!geo.StartsWith("EL", StringComparison.Ordinal))
                {
                    continue;
                }

                var valueText = row[7];

                if (frequency != "A" || string.IsNullOrWhiteSpace(residence) || string.IsNullOrWhiteSpace(unit) || string.IsNullOrWhiteSpace(row[9]))
                {
                    throw new FormatException($"Missing or invalid dimensions at tour_ce_omn12 CSV row {rowNumber}.");
                }

                if (!validIndicators.Contains(indicator))
                {
                    throw new FormatException($"Invalid indicator at tour_ce_omn12 CSV row {rowNumber}.");
                }

                if (!validMonths.Contains(month))
                {
                    throw new FormatException($"Invalid month at tour_ce_omn12 CSV row {rowNumber}.");
                }

                if (!int.TryParse(row[6], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1 || year > 9999)
                {
                    throw new FormatException($"Invalid year at tour_ce_omn12 CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (!string.IsNullOrEmpty(valueText))
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        throw new FormatException($"Invalid value at tour_ce_omn12 CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                if (unit == "NR" && value.HasValue && (value.Value < 0 || decimal.Truncate(value.Value) != value.Value))
                {
                    throw new FormatException($"The platform activity count must be a non-negative integer at tour_ce_omn12 CSV row {rowNumber}.");
                }

                var entity = new MonthlyPlatformActivity
                {
                    Frequency = frequency,
                    Indicator = indicator,
                    Residence = residence,
                    Month = month,
                    Unit = unit,
                    Geo = geo,
                    Year = year,
                    Value = value,
                    Flags = row[8],
                    RawValue = row[9],
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No Greek data rows were found in tour_ce_omn12.csv. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportTourCeOmr()
        {
            var entities = await ReadTourCeOmr();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.NationalMonthlyPlatformActivity.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.NationalMonthlyPlatformActivity.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<NationalMonthlyPlatformActivity>> ReadTourCeOmr()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "tour_ce_omr.csv"));

            var expectedColumns = new[] { "freq", "indic_to", "c_resid", "month", "unit", "geo", "year", "value", "flags", "raw_value" };
            var validIndicators = new[] { "LSTY", "NGT_SP", "STY" };
            var validMonths = new[] { "M01", "M02", "M03", "M04", "M05", "M06", "M07", "M08", "M09", "M10", "M11", "M12", "TOTAL" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<NationalMonthlyPlatformActivity>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var indicator = row[1];
                var residence = row[2];
                var month = row[3];
                var unit = row[4];
                var geo = row[5];

                if (!geo.StartsWith("EL", StringComparison.Ordinal))
                {
                    continue;
                }

                var valueText = row[7];

                if (frequency != "A" || string.IsNullOrWhiteSpace(residence) || string.IsNullOrWhiteSpace(unit) || string.IsNullOrWhiteSpace(row[9]))
                {
                    throw new FormatException($"Missing or invalid dimensions at tour_ce_omr CSV row {rowNumber}.");
                }

                if (!validIndicators.Contains(indicator))
                {
                    throw new FormatException($"Invalid indicator at tour_ce_omr CSV row {rowNumber}.");
                }

                if (!validMonths.Contains(month))
                {
                    throw new FormatException($"Invalid month at tour_ce_omr CSV row {rowNumber}.");
                }

                if (!int.TryParse(row[6], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1 || year > 9999)
                {
                    throw new FormatException($"Invalid year at tour_ce_omr CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (!string.IsNullOrEmpty(valueText))
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        throw new FormatException($"Invalid value at tour_ce_omr CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                if (unit == "NR" && value.HasValue && (value.Value < 0 || decimal.Truncate(value.Value) != value.Value))
                {
                    throw new FormatException($"The platform activity count must be a non-negative integer at tour_ce_omr CSV row {rowNumber}.");
                }

                var entity = new NationalMonthlyPlatformActivity
                {
                    Frequency = frequency,
                    Indicator = indicator,
                    Residence = residence,
                    Month = month,
                    Unit = unit,
                    Geo = geo,
                    Year = year,
                    Value = value,
                    Flags = row[8],
                    RawValue = row[9],
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No Greek data rows were found in tour_ce_omr.csv. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportTourCeOan3()
        {
            var entities = await ReadTourCeOan3();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.AnnualPlatformGuestNights.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.AnnualPlatformGuestNights.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<AnnualPlatformGuestNights>> ReadTourCeOan3()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "tour_ce_oan3.csv"));

            var expectedColumns = new[] { "freq", "indic_to", "c_resid", "unit", "geo", "year", "value", "flags", "raw_value" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<AnnualPlatformGuestNights>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var indicator = row[1];
                var residence = row[2];
                var unit = row[3];
                var geo = row[4];

                if (!geo.StartsWith("EL", StringComparison.Ordinal))
                {
                    continue;
                }

                var valueText = row[6];

                if (frequency != "A" || string.IsNullOrWhiteSpace(residence) || string.IsNullOrWhiteSpace(unit) || string.IsNullOrWhiteSpace(row[8]))
                {
                    throw new FormatException($"Missing or invalid dimensions at tour_ce_oan3 CSV row {rowNumber}.");
                }

                if (indicator != "NGT_SP")
                {
                    throw new FormatException($"Invalid indicator at tour_ce_oan3 CSV row {rowNumber}.");
                }

                if (!int.TryParse(row[5], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1 || year > 9999)
                {
                    throw new FormatException($"Invalid year at tour_ce_oan3 CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (!string.IsNullOrEmpty(valueText))
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        throw new FormatException($"Invalid value at tour_ce_oan3 CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                if (unit == "NR" && value.HasValue && (value.Value < 0 || decimal.Truncate(value.Value) != value.Value))
                {
                    throw new FormatException($"The number of guest nights must be a non-negative integer at tour_ce_oan3 CSV row {rowNumber}.");
                }

                var entity = new AnnualPlatformGuestNights
                {
                    Frequency = frequency,
                    Indicator = indicator,
                    Residence = residence,
                    Unit = unit,
                    Geo = geo,
                    Year = year,
                    Value = value,
                    Flags = row[7],
                    RawValue = row[8],
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No Greek data rows were found in tour_ce_oan3.csv. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportTourCapNuts2()
        {
            var entities = await ReadTourCapNuts2();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.AnnualAccommodationCapacity.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.AnnualAccommodationCapacity.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<AnnualAccommodationCapacity>> ReadTourCapNuts2()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "tour_cap_nuts2.csv"));

            var expectedColumns = new[] { "freq", "accomunit", "unit", "nace_r2", "geo", "year", "value", "flags", "raw_value" };

            var validAccommodationUnits = new[] { "BEDPL", "BEDRM", "ESTBL" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<AnnualAccommodationCapacity>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var accommodationUnit = row[1];
                var unit = row[2];
                var accommodationType = row[3];
                var geo = row[4];

                if (!geo.StartsWith("EL", StringComparison.Ordinal))
                {
                    continue;
                }

                var valueText = row[6];

                if (frequency != "A" || string.IsNullOrWhiteSpace(unit) || string.IsNullOrWhiteSpace(accommodationType) || string.IsNullOrWhiteSpace(row[8]))
                {
                    throw new FormatException($"Missing or invalid dimensions at tour_cap_nuts2 CSV row {rowNumber}.");
                }

                if (!validAccommodationUnits.Contains(accommodationUnit))
                {
                    throw new FormatException($"Invalid accommodation unit at tour_cap_nuts2 CSV row {rowNumber}.");
                }

                if (!int.TryParse(row[5], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1 || year > 9999)
                {
                    throw new FormatException($"Invalid year at tour_cap_nuts2 CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (!string.IsNullOrEmpty(valueText))
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        throw new FormatException($"Invalid value at tour_cap_nuts2 CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                if (unit == "NR" && value.HasValue && (value.Value < 0 || decimal.Truncate(value.Value) != value.Value))
                {
                    throw new FormatException($"The accommodation capacity must be a non-negative integer at tour_cap_nuts2 CSV row {rowNumber}.");
                }

                var entity = new AnnualAccommodationCapacity
                {
                    Frequency = frequency,
                    AccommodationUnit = accommodationUnit,
                    Unit = unit,
                    AccommodationType = accommodationType,
                    Geo = geo,
                    Year = year,
                    Value = value,
                    Flags = row[7],
                    RawValue = row[8],
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No Greek data rows were found in tour_cap_nuts2.csv. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportTourOccAnor2()
        {
            var entities = await ReadTourOccAnor2();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.AnnualHotelOccupancy.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.AnnualHotelOccupancy.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<AnnualHotelOccupancy>> ReadTourOccAnor2()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "tour_occ_anor2.csv"));

            var expectedColumns = new[] { "freq", "accomunit", "unit", "geo", "year", "value", "flags", "raw_value" };

            var validAccommodationUnits = new[] { "BEDPL", "BEDRM" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<AnnualHotelOccupancy>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var accommodationUnit = row[1];
                var unit = row[2];
                var geo = row[3];

                if (!geo.StartsWith("EL", StringComparison.Ordinal))
                {
                    continue;
                }

                var valueText = row[5];

                if (frequency != "A" || unit != "PC" || string.IsNullOrWhiteSpace(row[7]))
                {
                    throw new FormatException($"Missing or invalid dimensions at tour_occ_anor2 CSV row {rowNumber}.");
                }

                if (!validAccommodationUnits.Contains(accommodationUnit))
                {
                    throw new FormatException($"Invalid accommodation unit at tour_occ_anor2 CSV row {rowNumber}.");
                }

                if (!int.TryParse(row[4], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1 || year > 9999)
                {
                    throw new FormatException($"Invalid year at tour_occ_anor2 CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (!string.IsNullOrEmpty(valueText))
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        throw new FormatException($"Invalid value at tour_occ_anor2 CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                if (value.HasValue && (value.Value < 0 || value.Value > 100))
                {
                    throw new FormatException($"The occupancy rate must be between 0 and 100 at tour_occ_anor2 CSV row {rowNumber}.");
                }

                var entity = new AnnualHotelOccupancy
                {
                    Frequency = frequency,
                    AccommodationUnit = accommodationUnit,
                    Unit = unit,
                    Geo = geo,
                    Year = year,
                    Value = value,
                    Flags = row[6],
                    RawValue = row[7],
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No Greek data rows were found in tour_occ_anor2.csv. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportBankOfGreeceRegionalTravelReceipts()
        {
            var entities = await ReadBankOfGreeceRegionalTravelReceipts();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.AnnualRegionalTravelReceipts.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.AnnualRegionalTravelReceipts.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<AnnualRegionalTravelReceipts>> ReadBankOfGreeceRegionalTravelReceipts()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "bog_regional_travel_receipts.csv"));

            var expectedColumns = new[] { "freq", "geo", "region", "country", "year", "unit", "value", "flags", "raw_value" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<AnnualRegionalTravelReceipts>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var geo = row[1];
                var region = row[2];
                var country = row[3];
                var unit = row[5];
                var valueText = row[6];
                var flags = row[7];
                var rawValue = row[8];

                if (geo != "EL52")
                {
                    continue;
                }

                if (!int.TryParse(row[4], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1 || year > 9999)
                {
                    throw new FormatException($"Invalid year at bog_regional_travel_receipts CSV row {rowNumber}.");
                }

                if (year < 2020 || year > 2025)
                {
                    continue;
                }

                if (frequency != "A" || unit != "MIO_EUR" || string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(rawValue))
                {
                    throw new FormatException($"Missing or invalid dimensions at bog_regional_travel_receipts CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (string.IsNullOrEmpty(valueText))
                {
                    if (flags != "(:)" || rawValue != "(:)")
                    {
                        throw new FormatException($"Invalid unpublished value at bog_regional_travel_receipts CSV row {rowNumber}.");
                    }
                }
                else
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue) || parsedValue < 0 || !string.IsNullOrEmpty(flags))
                    {
                        throw new FormatException($"Invalid value at bog_regional_travel_receipts CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                var entity = new AnnualRegionalTravelReceipts
                {
                    Frequency = frequency,
                    Geo = geo,
                    Region = region,
                    Country = country,
                    Year = year,
                    Unit = unit,
                    Value = value,
                    Flags = flags,
                    RawValue = rawValue,
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No EL52 data rows for 2020–2025 were found in bog_regional_travel_receipts.csv. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportBankOfGreeceRegionalExpenditurePerNight()
        {
            var entities = await ReadBankOfGreeceRegionalExpenditurePerNight();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.AnnualRegionalExpenditurePerNight.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.AnnualRegionalExpenditurePerNight.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<AnnualRegionalExpenditurePerNight>> ReadBankOfGreeceRegionalExpenditurePerNight()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "bog_regional_expenditure_per_night.csv"));

            var expectedColumns = new[] { "freq", "geo", "region", "country", "year", "unit", "value", "flags", "raw_value" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<AnnualRegionalExpenditurePerNight>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var geo = row[1];
                var region = row[2];
                var country = row[3];
                var unit = row[5];
                var valueText = row[6];
                var flags = row[7];
                var rawValue = row[8];

                if (geo != "EL52")
                {
                    continue;
                }

                if (!int.TryParse(row[4], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1 || year > 9999)
                {
                    throw new FormatException($"Invalid year at bog_regional_expenditure_per_night CSV row {rowNumber}.");
                }

                if (year < 2020 || year > 2025)
                {
                    continue;
                }

                if (frequency != "A" || unit != "EUR_PER_NIGHT" || string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(rawValue))
                {
                    throw new FormatException($"Missing or invalid dimensions at bog_regional_expenditure_per_night CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (string.IsNullOrEmpty(valueText))
                {
                    if (flags != "(:)" || rawValue != "(:)")
                    {
                        throw new FormatException($"Invalid unpublished value at bog_regional_expenditure_per_night CSV row {rowNumber}.");
                    }
                }
                else
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue) || parsedValue < 0 || !string.IsNullOrEmpty(flags))
                    {
                        throw new FormatException($"Invalid value at bog_regional_expenditure_per_night CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                var entity = new AnnualRegionalExpenditurePerNight
                {
                    Frequency = frequency,
                    Geo = geo,
                    Region = region,
                    Country = country,
                    Year = year,
                    Unit = unit,
                    Value = value,
                    Flags = flags,
                    RawValue = rawValue,
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No EL52 data rows for 2020–2025 were found in bog_regional_expenditure_per_night.csv. The existing table was not changed.");
            }

            return entities;
        }

        private async Task<int> ImportBankOfGreeceRegionalLengthOfStay()
        {
            var entities = await ReadBankOfGreeceRegionalLengthOfStay();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.AnnualRegionalLengthOfStay.ExecuteDeleteAsync();

            const int batchSize = 5000;

            foreach (var batch in entities.Chunk(batchSize))
            {
                _context.AnnualRegionalLengthOfStay.AddRange(batch);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();
            }

            await transaction.CommitAsync();

            return entities.Count;
        }

        private async Task<List<AnnualRegionalLengthOfStay>> ReadBankOfGreeceRegionalLengthOfStay()
        {
            var filePath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "etl", "data", "processed", "bog_regional_length_of_stay.csv"));

            var expectedColumns = new[] { "freq", "geo", "region", "country", "year", "unit", "value", "flags", "raw_value" };

            var rows = await CsvFileHelper.ReadRowsAsync(filePath, expectedColumns);
            var entities = new List<AnnualRegionalLengthOfStay>();
            var rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                var frequency = row[0];
                var geo = row[1];
                var region = row[2];
                var country = row[3];
                var unit = row[5];
                var valueText = row[6];
                var flags = row[7];
                var rawValue = row[8];

                if (geo != "EL52")
                {
                    continue;
                }

                if (!int.TryParse(row[4], NumberStyles.None, CultureInfo.InvariantCulture, out var year) || year < 1 || year > 9999)
                {
                    throw new FormatException($"Invalid year at bog_regional_length_of_stay CSV row {rowNumber}.");
                }

                if (year < 2020 || year > 2025)
                {
                    continue;
                }

                if (frequency != "A" || unit != "NIGHTS_PER_VISIT" || string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(rawValue))
                {
                    throw new FormatException($"Missing or invalid dimensions at bog_regional_length_of_stay CSV row {rowNumber}.");
                }

                decimal? value = null;

                if (string.IsNullOrEmpty(valueText))
                {
                    if (flags != "(:)" || rawValue != "(:)")
                    {
                        throw new FormatException($"Invalid unpublished value at bog_regional_length_of_stay CSV row {rowNumber}.");
                    }
                }
                else
                {
                    var numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

                    if (!decimal.TryParse(valueText, numberStyles, CultureInfo.InvariantCulture, out var parsedValue) || parsedValue < 0 || !string.IsNullOrEmpty(flags))
                    {
                        throw new FormatException($"Invalid value at bog_regional_length_of_stay CSV row {rowNumber}.");
                    }

                    value = parsedValue;
                }

                var entity = new AnnualRegionalLengthOfStay
                {
                    Frequency = frequency,
                    Geo = geo,
                    Region = region,
                    Country = country,
                    Year = year,
                    Unit = unit,
                    Value = value,
                    Flags = flags,
                    RawValue = rawValue,
                };

                entities.Add(entity);
            }

            if (entities.Count == 0)
            {
                throw new InvalidOperationException("No EL52 data rows for 2020–2025 were found in bog_regional_length_of_stay.csv. The existing table was not changed.");
            }

            return entities;
        }
    }
}
