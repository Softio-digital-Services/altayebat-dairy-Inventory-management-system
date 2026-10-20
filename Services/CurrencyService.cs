using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventorySystem;

namespace InventorySystem.Services
{
    /// <summary>
    /// Manages supported currencies and exchange-rate conversions.
    /// Base (storage) currency is always USD.
    /// </summary>
    public static class CurrencyService
    {
        // --- State -------------------------------------------------------
        private static string _activeCurrency = "USD";

        public static event EventHandler CurrencyChanged;
 
        public static string ActiveCurrency
        {
            get => _activeCurrency;
            set 
            { 
                if (_activeCurrency != value)
                {
                    _activeCurrency = value; 
                    CurrencyChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        // --- Supported currencies ----------------------------------------
        private static List<CurrencyInfo> _supportedCurrencies = new List<CurrencyInfo>();
        public static List<CurrencyInfo> SupportedCurrencies => _supportedCurrencies;

        // --- Rate dictionary (base = USD) --------------------------------
        // Default fallback rates (updated at runtime from DB or API)
        private static Dictionary<string, decimal> _rates = new Dictionary<string, decimal>
        {
            { "USD", 1m },
            { "EUR", 0.92m },
            { "LBP", 89500m },
        };

        // ------------------------------------------------------------------ DB bootstrap ------------------------------------------------------------------
        public static void EnsureTable()
        {
            // Create table if it doesn't exist (SQLite style)
            DatabaseHelper.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS currency_rates (
                    code        TEXT PRIMARY KEY,
                    name        TEXT,
                    symbol      TEXT,
                    rate_vs_usd REAL DEFAULT 1,
                    last_updated TEXT DEFAULT (datetime('now'))
                );");

            // Seed if empty
            int count = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM currency_rates");
            if (count == 0)
            {
                DatabaseHelper.ExecuteNonQuery(@"
                    INSERT INTO currency_rates (code, name, symbol, rate_vs_usd) VALUES
                        ('USD', 'US Dollar',      '$',   1),
                        ('EUR', 'Euro',           '€',   0.92),
                        ('LBP', 'Lebanese Lira',  'ل.ل', 89500);");
            }

            // Ensure orders table has currency columns (SQLite doesn't support IF NOT EXISTS in ALTER TABLE)
            AddColumnIfNotExists("orders", "currency_code", "TEXT DEFAULT 'USD'");
            AddColumnIfNotExists("orders", "exchange_rate", "REAL DEFAULT 1");

            LoadRatesFromDb();
        }

        private static void AddColumnIfNotExists(string tableName, string columnName, string columnDefinition)
        {
            try
            {
                var dt = DatabaseHelper.ExecuteDataTable($"PRAGMA table_info({tableName})");
                bool exists = false;
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    if (row["name"].ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    DatabaseHelper.ExecuteNonQuery($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition}");
                }
            }
            catch { }
        }

        // ------------------------------------------------------------------ DB rate persistence ------------------------------------------------------
        public static void LoadRatesFromDb()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteDataTable("SELECT code, name, symbol, rate_vs_usd FROM currency_rates");
                _rates.Clear();
                _supportedCurrencies.Clear();
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    string code = row["code"].ToString();
                    string name = row["name"].ToString();
                    string symbol = row["symbol"].ToString();
                    decimal rate = Convert.ToDecimal(row["rate_vs_usd"]);
                    
                    _rates[code] = rate;
                    _supportedCurrencies.Add(new CurrencyInfo(code, name, symbol));
                }
            }
            catch { /* silently keep defaults if load fails */ }
        }

        public static void SaveRatesToDb(Dictionary<string, decimal> newRates)
        {
            foreach (var kvp in newRates)
            {
                DatabaseHelper.ExecuteNonQuery(
                    $"UPDATE currency_rates SET rate_vs_usd = {kvp.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}, last_updated = datetime('now') WHERE code = '{kvp.Key}'");
                _rates[kvp.Key] = kvp.Value;
            }
        }

        public static void UpdateCurrency(string code, string name, string symbol, decimal rate)
        {
            DatabaseHelper.ExecuteNonQuery(
                $"UPDATE currency_rates SET name = @name, symbol = @symbol, rate_vs_usd = @rate, last_updated = datetime('now') WHERE code = @code",
                new Microsoft.Data.Sqlite.SqliteParameter("@name", name),
                new Microsoft.Data.Sqlite.SqliteParameter("@symbol", symbol),
                new Microsoft.Data.Sqlite.SqliteParameter("@rate", rate),
                new Microsoft.Data.Sqlite.SqliteParameter("@code", code)
            );
            _rates[code] = rate;
            LoadRatesFromDb(); // Refresh internal list
        }

        // --- Live API fetch -----------------------------------------------
        /// <summary>
        /// Fetches live rates from exchangerate.host (free, no key needed).
        /// Returns updated rates or null on failure.
        /// </summary>
        public static async Task<Dictionary<string, decimal>> FetchLiveRatesAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Build dynamic URL for all non-USD currencies
                    List<string> codes = new List<string>();
                    foreach(var c in _supportedCurrencies) if(c.Code != "USD") codes.Add(c.Code);
                    string codeList = string.Join(",", codes);

                    string fallbackUrl = $"https://api.frankfurter.app/latest?from=USD&to={codeList}";

                    string json;
                    try
                    {
                        json = await client.GetStringAsync(fallbackUrl);
                    }
                    catch
                    {
                        return null;
                    }

                    // Parse Frankfurt JSON: {"amount":1,"base":"USD","date":"...","rates":{"EUR":0.92,"LBP":...}}
                    var result = new Dictionary<string, decimal> { { "USD", 1m } };
                    
                    // Simple JSON parsing (avoid heavy dependencies)
                    int ratesIdx = json.IndexOf("\"rates\":");
                    if (ratesIdx < 0) return null;

                    string ratesPart = json.Substring(ratesIdx);
                    foreach (string code in codes)
                    {
                        string key = $"\"{code}\":";
                        int idx = ratesPart.IndexOf(key);
                        if (idx < 0) continue;
                        idx += key.Length;
                        int end = ratesPart.IndexOfAny(new[] { ',', '}' }, idx);
                        string valStr = ratesPart.Substring(idx, end - idx).Trim().Replace("\"", "");
                        if (decimal.TryParse(valStr, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out decimal val))
                        {
                            result[code] = val;
                        }
                    }
                    return result;
                }
            }
            catch
            {
                return null;
            }
        }

        // --- Conversion helpers -------------------------------------------
        public static decimal ConvertAmount(decimal usdAmount, string toCurrency = null)
        {
            toCurrency = toCurrency ?? _activeCurrency;
            if (!_rates.TryGetValue(toCurrency, out decimal rate)) return usdAmount;
            return usdAmount * rate;
        }

        public static decimal GetRate(string currency = null)
        {
            currency = currency ?? _activeCurrency;
            return _rates.TryGetValue(currency, out decimal r) ? r : 1m;
        }

        public static string GetSymbol(string currency = null)
        {
            currency = currency ?? _activeCurrency;
            foreach (var c in SupportedCurrencies)
                if (c.Code == currency) return c.Symbol;
            return currency;
        }

        public static string Format(decimal usdAmount, string currency = null)
        {
            currency = currency ?? _activeCurrency;
            decimal converted = ConvertAmount(usdAmount, currency);
            string symbol = GetSymbol(currency);

            // LBP -" no decimals, use thousands separator
            if (currency == "LBP")
                return $"{symbol} {converted:N0}";

            return $"{symbol}{converted:N2}";
        }

        public static async Task<decimal?> FetchRateAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.ToUpper() == "USD") return 1m;
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"https://api.frankfurter.app/latest?from=USD&to={code.ToUpper()}";
                    string json = await client.GetStringAsync(url);
                    
                    int ratesIdx = json.IndexOf("\"rates\":");
                    if (ratesIdx < 0) return null;

                    string ratesPart = json.Substring(ratesIdx);
                    string key = $"\"{code.ToUpper()}\":";
                    int idx = ratesPart.IndexOf(key);
                    if (idx < 0) return null;
                    idx += key.Length;
                    int end = ratesPart.IndexOfAny(new[] { ',', '}' }, idx);
                    string valStr = ratesPart.Substring(idx, end - idx).Trim().Replace("\"", "").Replace(":", "");
                    if (decimal.TryParse(valStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal val))
                    {
                        return val;
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>Gets all currencies with current rates from DB.</summary>
        public static System.Data.DataTable GetAllCurrencies()
        {
            return DatabaseHelper.ExecuteDataTable(
                "SELECT code, name, symbol, rate_vs_usd, last_updated FROM currency_rates ORDER BY code");
        }
    }

    public class CurrencyInfo
    {
        public string Code   { get; }
        public string Name   { get; }
        public string Symbol { get; }
        public CurrencyInfo(string code, string name, string symbol)
        { Code = code; Name = name; Symbol = symbol; }
        public override string ToString() => $"{Code}  {Symbol}";
    }
}
