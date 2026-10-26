using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CSYM505.Api.Controllers.AnalyticsApi.Dtos;

namespace CSYM505.Api.Controllers.AnalyticsApi.Helpers
{
    public class ETSForecastRunner
    {
        private readonly string _pythonExecutable;
        private readonly string _scriptPath;
        private readonly ILogger<ETSForecastRunner> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public ETSForecastRunner(IConfiguration configuration, IWebHostEnvironment environment, ILogger<ETSForecastRunner> logger)
        {
            var pythonExecutable = configuration["Forecasting:PythonExecutable"] ?? throw new InvalidOperationException("Forecasting:PythonExecutable is not configured.");

            var scriptPath = configuration["Forecasting:ScriptPath"] ?? throw new InvalidOperationException("Forecasting:ScriptPath is not configured.");

            _pythonExecutable = Path.GetFullPath(pythonExecutable, environment.ContentRootPath);

            _scriptPath = Path.GetFullPath(scriptPath, environment.ContentRootPath);

            _logger = logger;
        }

        /// <summary>
        /// Στέλνει μία χρονικά ταξινομημένη σειρά στην Python
        /// και επιστρέφει τις επόμενες μηνιαίες εκτιμήσεις ETS.
        /// </summary>
        public async Task<ETSForecastResponse> RunForecast(List<double> values, int horizon)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (horizon < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(horizon));
            }

            if (!File.Exists(_pythonExecutable))
            {
                throw new FileNotFoundException("The Python executable was not found.", _pythonExecutable);
            }

            if (!File.Exists(_scriptPath))
            {
                throw new FileNotFoundException("The ETS script was not found.", _scriptPath);
            }

            var requestJson = JsonSerializer.Serialize(new { values, horizon });

            var startInfo = new ProcessStartInfo
            {
                FileName = _pythonExecutable,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardInputEncoding = new UTF8Encoding(false),
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            startInfo.ArgumentList.Add("-B");
            startInfo.ArgumentList.Add(_scriptPath);

            using var process = new Process { StartInfo = startInfo };

            if (!process.Start())
            {
                throw new InvalidOperationException("The ETS process could not be started.");
            }

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(120));

            // Διαβάζουμε παράλληλα τα δύο κανάλια, ώστε κανένα
            // να μη γεμίσει και μπλοκάρει τη διεργασία.
            var outputTask = process.StandardOutput.ReadToEndAsync(timeout.Token);

            var errorTask = process.StandardError.ReadToEndAsync(timeout.Token);

            try
            {
                await process.StandardInput.WriteAsync(requestJson.AsMemory(), timeout.Token);

                process.StandardInput.Close();

                await process.WaitForExitAsync(timeout.Token);

                var output = await outputTask;
                var error = await errorTask;

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"ETS failed with exit code {process.ExitCode}: {error.Trim()}");
                }

                if (!string.IsNullOrWhiteSpace(error))
                {
                    _logger.LogWarning("ETS completed with diagnostic messages: {Diagnostics}", error.Trim());
                }

                var response = JsonSerializer.Deserialize<ETSForecastResponse>(output, JsonOptions);

                if (response == null || string.IsNullOrWhiteSpace(response.Method) || response.Predictions == null || response.Predictions.Count != horizon)
                {
                    throw new InvalidOperationException("ETS returned an incomplete forecast response.");
                }

                if (response.Predictions.Any(value => !double.IsFinite(value) || value < 0))
                {
                    throw new InvalidOperationException("ETS returned invalid forecast values.");
                }

                return response;
            }
            catch (OperationCanceledException error)
            {
                throw new TimeoutException("ETS exceeded the execution limit of 120 seconds.", error);
            }
            finally
            {
                // Το Dispose μόνο του δεν τερματίζει την Python.
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync();
                }
            }
        }
    }
}
