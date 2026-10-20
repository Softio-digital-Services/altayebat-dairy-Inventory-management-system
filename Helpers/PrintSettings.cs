using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Global print defaults + per-printer profiles (any Windows printer).
    /// </summary>
    public static class PrintSettings
    {
        public const string LabelWidthKey = "print.label.widthMm";
        public const string LabelHeightKey = "print.label.heightMm";
        public const string LabelGapKey = "print.label.gapMm";
        public const string LabelMarginKey = "print.label.marginMm"; // legacy single
        public const string LabelMarginTopKey = "print.label.marginTopMm";
        public const string LabelMarginRightKey = "print.label.marginRightMm";
        public const string LabelMarginBottomKey = "print.label.marginBottomMm";
        public const string LabelMarginLeftKey = "print.label.marginLeftMm";
        public const string LabelColumnsKey = "print.label.columns";
        public const string LabelPaperModeKey = "print.label.paperMode";
        public const string LabelPageWidthKey = "print.label.pageWidthMm";
        public const string LabelPageHeightKey = "print.label.pageHeightMm";
        public const string ReceiptWidthKey = "print.receipt.widthMm";
        public const string ReceiptHeightKey = "print.receipt.heightMm";
        public const string ReceiptMarginKey = "print.receipt.marginMm";
        public const string ReceiptMarginTopKey = "print.receipt.marginTopMm";
        public const string ReceiptMarginRightKey = "print.receipt.marginRightMm";
        public const string ReceiptMarginBottomKey = "print.receipt.marginBottomMm";
        public const string ReceiptMarginLeftKey = "print.receipt.marginLeftMm";
        public const string LabelPrinterKey = "print.label.printer";
        public const string ReceiptPrinterKey = "print.receipt.printer";
        public const string ProfilesKey = "print.printerProfiles";

        public const double DefaultLabelWidthMm = 60;
        public const double DefaultLabelHeightMm = 36;
        public const double DefaultLabelGapMm = 5;
        public const double DefaultLabelMarginMm = 2;
        public const int DefaultLabelColumns = 0; // 0 = auto
        public const string DefaultLabelPaperMode = "sheet";
        public const double DefaultLabelPageWidthMm = 210;
        public const double DefaultLabelPageHeightMm = 297;
        public const double DefaultReceiptWidthMm = 80;
        public const double DefaultReceiptHeightMm = 0;
        public const double DefaultReceiptMarginMm = 2.5;

        public static PrintSettingsSnapshot GetSnapshot()
        {
            FeatureFlags.EnsureTable();
            double legacyLabel = GetDouble(LabelMarginKey, DefaultLabelMarginMm);
            double legacyReceipt = GetDouble(ReceiptMarginKey, DefaultReceiptMarginMm);
            return new PrintSettingsSnapshot
            {
                LabelWidthMm = Clamp(GetDouble(LabelWidthKey, DefaultLabelWidthMm), 20, 210),
                LabelHeightMm = Clamp(GetDouble(LabelHeightKey, DefaultLabelHeightMm), 10, 297),
                LabelGapMm = Clamp(GetDouble(LabelGapKey, DefaultLabelGapMm), 0, 20),
                LabelMarginMm = Clamp(legacyLabel, 0, 20),
                LabelMarginTopMm = Clamp(GetDouble(LabelMarginTopKey, legacyLabel), 0, 30),
                LabelMarginRightMm = Clamp(GetDouble(LabelMarginRightKey, legacyLabel), 0, 30),
                LabelMarginBottomMm = Clamp(GetDouble(LabelMarginBottomKey, legacyLabel), 0, 30),
                LabelMarginLeftMm = Clamp(GetDouble(LabelMarginLeftKey, legacyLabel), 0, 30),
                LabelColumns = ClampInt(GetInt(LabelColumnsKey, DefaultLabelColumns), 0, 4),
                LabelPaperMode = NormalizePaperMode(GetString(LabelPaperModeKey, DefaultLabelPaperMode)),
                LabelPageWidthMm = Clamp(GetDouble(LabelPageWidthKey, DefaultLabelPageWidthMm), 40, 330),
                LabelPageHeightMm = Clamp(GetDouble(LabelPageHeightKey, DefaultLabelPageHeightMm), 40, 500),
                ReceiptWidthMm = Clamp(GetDouble(ReceiptWidthKey, DefaultReceiptWidthMm), 40, 120),
                ReceiptHeightMm = Clamp(GetDouble(ReceiptHeightKey, DefaultReceiptHeightMm), 0, 500),
                ReceiptMarginMm = Clamp(legacyReceipt, 0, 15),
                ReceiptMarginTopMm = Clamp(GetDouble(ReceiptMarginTopKey, legacyReceipt), 0, 20),
                ReceiptMarginRightMm = Clamp(GetDouble(ReceiptMarginRightKey, legacyReceipt), 0, 20),
                ReceiptMarginBottomMm = Clamp(GetDouble(ReceiptMarginBottomKey, legacyReceipt), 0, 20),
                ReceiptMarginLeftMm = Clamp(GetDouble(ReceiptMarginLeftKey, legacyReceipt), 0, 20),
                LabelPrinter = GetString(LabelPrinterKey, ""),
                ReceiptPrinter = GetString(ReceiptPrinterKey, ""),
                PrinterProfiles = LoadProfiles()
            };
        }

        public static void Save(PrintSettingsSnapshot s)
        {
            if (s == null) return;
            FeatureFlags.EnsureTable();
            SetDouble(LabelWidthKey, Clamp(s.LabelWidthMm, 20, 210));
            SetDouble(LabelHeightKey, Clamp(s.LabelHeightMm, 10, 297));
            SetDouble(LabelGapKey, Clamp(s.LabelGapMm, 0, 20));
            SetDouble(LabelMarginKey, Clamp(s.LabelMarginMm, 0, 20));
            SetDouble(LabelMarginTopKey, Clamp(s.LabelMarginTopMm, 0, 30));
            SetDouble(LabelMarginRightKey, Clamp(s.LabelMarginRightMm, 0, 30));
            SetDouble(LabelMarginBottomKey, Clamp(s.LabelMarginBottomMm, 0, 30));
            SetDouble(LabelMarginLeftKey, Clamp(s.LabelMarginLeftMm, 0, 30));
            SetString(LabelColumnsKey, ClampInt(s.LabelColumns, 0, 4).ToString(CultureInfo.InvariantCulture));
            SetString(LabelPaperModeKey, NormalizePaperMode(s.LabelPaperMode));
            SetDouble(LabelPageWidthKey, Clamp(s.LabelPageWidthMm, 40, 330));
            SetDouble(LabelPageHeightKey, Clamp(s.LabelPageHeightMm, 40, 500));
            SetDouble(ReceiptWidthKey, Clamp(s.ReceiptWidthMm, 40, 120));
            SetDouble(ReceiptHeightKey, Clamp(s.ReceiptHeightMm, 0, 500));
            SetDouble(ReceiptMarginKey, Clamp(s.ReceiptMarginMm, 0, 15));
            SetDouble(ReceiptMarginTopKey, Clamp(s.ReceiptMarginTopMm, 0, 20));
            SetDouble(ReceiptMarginRightKey, Clamp(s.ReceiptMarginRightMm, 0, 20));
            SetDouble(ReceiptMarginBottomKey, Clamp(s.ReceiptMarginBottomMm, 0, 20));
            SetDouble(ReceiptMarginLeftKey, Clamp(s.ReceiptMarginLeftMm, 0, 20));
            SetString(LabelPrinterKey, s.LabelPrinter ?? "");
            SetString(ReceiptPrinterKey, s.ReceiptPrinter ?? "");
            if (s.PrinterProfiles != null)
                SaveProfiles(s.PrinterProfiles);
        }

        public static PrintSettingsSnapshot SavePrinterProfile(string printerName, string jobType, PrinterJobProfile profile)
        {
            if (string.IsNullOrWhiteSpace(printerName) || profile == null)
                return GetSnapshot();

            FeatureFlags.EnsureTable();
            var map = LoadProfiles();
            string key = printerName.Trim();
            if (!map.TryGetValue(key, out var entry) || entry == null)
                entry = new PrinterProfileEntry();

            jobType = (jobType ?? "").Trim().ToLowerInvariant();
            if (jobType == "receipt")
                entry.Receipt = NormalizeReceiptProfile(profile);
            else
                entry.Label = NormalizeLabelProfile(profile);

            map[key] = entry;
            SaveProfiles(map);

            var snap = GetSnapshot();
            if (jobType == "receipt")
            {
                ApplyReceiptProfileToSnapshot(snap, entry.Receipt);
                snap.ReceiptPrinter = key;
            }
            else
            {
                ApplyLabelProfileToSnapshot(snap, entry.Label);
                snap.LabelPrinter = key;
            }
            Save(snap);
            return GetSnapshot();
        }

        public static PrintSettingsSnapshot DeletePrinterProfile(string printerName)
        {
            FeatureFlags.EnsureTable();
            var map = LoadProfiles();
            if (!string.IsNullOrWhiteSpace(printerName))
            {
                string key = printerName.Trim();
                var match = map.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (match != null) map.Remove(match);
            }
            SaveProfiles(map);
            return GetSnapshot();
        }

        public static PrintSettingsSnapshot ClearAllPrinterProfiles()
        {
            FeatureFlags.EnsureTable();
            SaveProfiles(new Dictionary<string, PrinterProfileEntry>(StringComparer.OrdinalIgnoreCase));
            return GetSnapshot();
        }

        public static PrinterJobProfile ResolveForPrinter(string printerName, string jobType, PrintSettingsSnapshot fallback = null)
        {
            fallback ??= GetSnapshot();
            jobType = (jobType ?? "").Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(printerName)
                && fallback.PrinterProfiles != null
                && fallback.PrinterProfiles.TryGetValue(printerName.Trim(), out var entry)
                && entry != null)
            {
                if (jobType == "receipt" && entry.Receipt != null)
                    return entry.Receipt;
                if (jobType != "receipt" && entry.Label != null)
                    return entry.Label;
            }

            if (jobType == "receipt")
            {
                return new PrinterJobProfile
                {
                    WidthMm = fallback.ReceiptWidthMm,
                    HeightMm = fallback.ReceiptHeightMm,
                    MarginMm = fallback.ReceiptMarginMm,
                    MarginTopMm = fallback.ReceiptMarginTopMm,
                    MarginRightMm = fallback.ReceiptMarginRightMm,
                    MarginBottomMm = fallback.ReceiptMarginBottomMm,
                    MarginLeftMm = fallback.ReceiptMarginLeftMm,
                    PaperMode = "roll"
                };
            }

            return new PrinterJobProfile
            {
                WidthMm = fallback.LabelWidthMm,
                HeightMm = fallback.LabelHeightMm,
                GapMm = fallback.LabelGapMm,
                MarginMm = fallback.LabelMarginMm,
                MarginTopMm = fallback.LabelMarginTopMm,
                MarginRightMm = fallback.LabelMarginRightMm,
                MarginBottomMm = fallback.LabelMarginBottomMm,
                MarginLeftMm = fallback.LabelMarginLeftMm,
                Columns = fallback.LabelColumns,
                PaperMode = fallback.LabelPaperMode,
                PageWidthMm = fallback.LabelPageWidthMm,
                PageHeightMm = fallback.LabelPageHeightMm
            };
        }

        public static int MmToHundredths(double mm)
        {
            return Math.Max(0, (int)Math.Round(mm / 25.4 * 100.0));
        }

        private static void ApplyLabelProfileToSnapshot(PrintSettingsSnapshot snap, PrinterJobProfile p)
        {
            if (p == null) return;
            snap.LabelWidthMm = p.WidthMm;
            snap.LabelHeightMm = p.HeightMm;
            snap.LabelGapMm = p.GapMm;
            snap.LabelMarginMm = p.MarginMm >= 0 ? p.MarginMm : AverageMargin(p);
            snap.LabelMarginTopMm = EffectiveMargin(p.MarginTopMm, p.MarginMm, snap.LabelMarginMm);
            snap.LabelMarginRightMm = EffectiveMargin(p.MarginRightMm, p.MarginMm, snap.LabelMarginMm);
            snap.LabelMarginBottomMm = EffectiveMargin(p.MarginBottomMm, p.MarginMm, snap.LabelMarginMm);
            snap.LabelMarginLeftMm = EffectiveMargin(p.MarginLeftMm, p.MarginMm, snap.LabelMarginMm);
            snap.LabelColumns = p.Columns;
            snap.LabelPaperMode = p.PaperMode;
            snap.LabelPageWidthMm = p.PageWidthMm;
            snap.LabelPageHeightMm = p.PageHeightMm;
        }

        private static void ApplyReceiptProfileToSnapshot(PrintSettingsSnapshot snap, PrinterJobProfile p)
        {
            if (p == null) return;
            snap.ReceiptWidthMm = p.WidthMm;
            snap.ReceiptHeightMm = p.HeightMm;
            snap.ReceiptMarginMm = p.MarginMm >= 0 ? p.MarginMm : AverageMargin(p);
            snap.ReceiptMarginTopMm = EffectiveMargin(p.MarginTopMm, p.MarginMm, snap.ReceiptMarginMm);
            snap.ReceiptMarginRightMm = EffectiveMargin(p.MarginRightMm, p.MarginMm, snap.ReceiptMarginMm);
            snap.ReceiptMarginBottomMm = EffectiveMargin(p.MarginBottomMm, p.MarginMm, snap.ReceiptMarginMm);
            snap.ReceiptMarginLeftMm = EffectiveMargin(p.MarginLeftMm, p.MarginMm, snap.ReceiptMarginMm);
        }

        private static double EffectiveMargin(double side, double unified, double fallback)
        {
            if (side >= 0) return side;
            if (unified >= 0) return unified;
            return fallback;
        }

        private static double AverageMargin(PrinterJobProfile p)
        {
            var vals = new[] { p.MarginTopMm, p.MarginRightMm, p.MarginBottomMm, p.MarginLeftMm }
                .Where(v => v >= 0).ToList();
            if (vals.Count == 0) return p.MarginMm >= 0 ? p.MarginMm : 0;
            return vals.Average();
        }

        private static PrinterJobProfile NormalizeLabelProfile(PrinterJobProfile p)
        {
            double uni = p.MarginMm >= 0 ? p.MarginMm : DefaultLabelMarginMm;
            return new PrinterJobProfile
            {
                WidthMm = Clamp(p.WidthMm > 0 ? p.WidthMm : DefaultLabelWidthMm, 20, 210),
                HeightMm = Clamp(p.HeightMm > 0 ? p.HeightMm : DefaultLabelHeightMm, 10, 297),
                GapMm = Clamp(p.GapMm >= 0 ? p.GapMm : DefaultLabelGapMm, 0, 20),
                MarginMm = Clamp(uni, 0, 20),
                MarginTopMm = Clamp(p.MarginTopMm >= 0 ? p.MarginTopMm : uni, 0, 30),
                MarginRightMm = Clamp(p.MarginRightMm >= 0 ? p.MarginRightMm : uni, 0, 30),
                MarginBottomMm = Clamp(p.MarginBottomMm >= 0 ? p.MarginBottomMm : uni, 0, 30),
                MarginLeftMm = Clamp(p.MarginLeftMm >= 0 ? p.MarginLeftMm : uni, 0, 30),
                Columns = ClampInt(p.Columns, 0, 4),
                PaperMode = NormalizePaperMode(p.PaperMode),
                PageWidthMm = Clamp(p.PageWidthMm > 0 ? p.PageWidthMm : DefaultLabelPageWidthMm, 40, 330),
                PageHeightMm = Clamp(p.PageHeightMm > 0 ? p.PageHeightMm : DefaultLabelPageHeightMm, 40, 500)
            };
        }

        private static PrinterJobProfile NormalizeReceiptProfile(PrinterJobProfile p)
        {
            double uni = p.MarginMm >= 0 ? p.MarginMm : DefaultReceiptMarginMm;
            return new PrinterJobProfile
            {
                WidthMm = Clamp(p.WidthMm > 0 ? p.WidthMm : DefaultReceiptWidthMm, 40, 120),
                HeightMm = Clamp(p.HeightMm >= 0 ? p.HeightMm : DefaultReceiptHeightMm, 0, 500),
                MarginMm = Clamp(uni, 0, 15),
                MarginTopMm = Clamp(p.MarginTopMm >= 0 ? p.MarginTopMm : uni, 0, 20),
                MarginRightMm = Clamp(p.MarginRightMm >= 0 ? p.MarginRightMm : uni, 0, 20),
                MarginBottomMm = Clamp(p.MarginBottomMm >= 0 ? p.MarginBottomMm : uni, 0, 20),
                MarginLeftMm = Clamp(p.MarginLeftMm >= 0 ? p.MarginLeftMm : uni, 0, 20),
                PaperMode = "roll"
            };
        }

        private static Dictionary<string, PrinterProfileEntry> LoadProfiles()
        {
            try
            {
                string json = GetString(ProfilesKey, "");
                if (string.IsNullOrWhiteSpace(json))
                    return new Dictionary<string, PrinterProfileEntry>(StringComparer.OrdinalIgnoreCase);

                var map = JsonSerializer.Deserialize<Dictionary<string, PrinterProfileEntry>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return map != null
                    ? new Dictionary<string, PrinterProfileEntry>(map, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, PrinterProfileEntry>(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, PrinterProfileEntry>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static void SaveProfiles(Dictionary<string, PrinterProfileEntry> map)
        {
            try
            {
                string json = JsonSerializer.Serialize(map ?? new Dictionary<string, PrinterProfileEntry>());
                SetString(ProfilesKey, json);
            }
            catch { /* ignore */ }
        }

        private static string NormalizePaperMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode)) return DefaultLabelPaperMode;
            mode = mode.Trim().ToLowerInvariant();
            return mode == "roll" ? "roll" : "sheet";
        }

        private static double Clamp(double v, double min, double max)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return min;
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        private static int ClampInt(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        private static int GetInt(string key, int defaultValue)
        {
            try
            {
                string v = GetString(key, "");
                if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n))
                    return n;
                return defaultValue;
            }
            catch { return defaultValue; }
        }

        private static double GetDouble(string key, double defaultValue)
        {
            try
            {
                string v = DatabaseHelper.ExecuteScalar<string>(
                    "SELECT value FROM app_settings WHERE key = @k LIMIT 1",
                    new SqliteParameter("@k", key));
                if (string.IsNullOrWhiteSpace(v)) return defaultValue;
                if (double.TryParse(v.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                    return d;
                return defaultValue;
            }
            catch { return defaultValue; }
        }

        private static void SetDouble(string key, double value)
        {
            SetString(key, value.ToString("0.###", CultureInfo.InvariantCulture));
        }

        private static string GetString(string key, string defaultValue)
        {
            try
            {
                string v = DatabaseHelper.ExecuteScalar<string>(
                    "SELECT value FROM app_settings WHERE key = @k LIMIT 1",
                    new SqliteParameter("@k", key));
                return string.IsNullOrWhiteSpace(v) ? defaultValue : v.Trim();
            }
            catch { return defaultValue; }
        }

        private static void SetString(string key, string value)
        {
            DatabaseHelper.ExecuteNonQuery(
                @"INSERT INTO app_settings (key, value) VALUES (@k, @v)
                  ON CONFLICT(key) DO UPDATE SET value = excluded.value",
                new SqliteParameter("@k", key),
                new SqliteParameter("@v", value ?? ""));
        }
    }

    public class PrintSettingsSnapshot
    {
        public double LabelWidthMm { get; set; } = PrintSettings.DefaultLabelWidthMm;
        public double LabelHeightMm { get; set; } = PrintSettings.DefaultLabelHeightMm;
        public double LabelGapMm { get; set; } = PrintSettings.DefaultLabelGapMm;
        public double LabelMarginMm { get; set; } = PrintSettings.DefaultLabelMarginMm;
        public double LabelMarginTopMm { get; set; } = PrintSettings.DefaultLabelMarginMm;
        public double LabelMarginRightMm { get; set; } = PrintSettings.DefaultLabelMarginMm;
        public double LabelMarginBottomMm { get; set; } = PrintSettings.DefaultLabelMarginMm;
        public double LabelMarginLeftMm { get; set; } = PrintSettings.DefaultLabelMarginMm;
        public int LabelColumns { get; set; } = PrintSettings.DefaultLabelColumns;
        public string LabelPaperMode { get; set; } = PrintSettings.DefaultLabelPaperMode;
        public double LabelPageWidthMm { get; set; } = PrintSettings.DefaultLabelPageWidthMm;
        public double LabelPageHeightMm { get; set; } = PrintSettings.DefaultLabelPageHeightMm;
        public double ReceiptWidthMm { get; set; } = PrintSettings.DefaultReceiptWidthMm;
        public double ReceiptHeightMm { get; set; } = PrintSettings.DefaultReceiptHeightMm;
        public double ReceiptMarginMm { get; set; } = PrintSettings.DefaultReceiptMarginMm;
        public double ReceiptMarginTopMm { get; set; } = PrintSettings.DefaultReceiptMarginMm;
        public double ReceiptMarginRightMm { get; set; } = PrintSettings.DefaultReceiptMarginMm;
        public double ReceiptMarginBottomMm { get; set; } = PrintSettings.DefaultReceiptMarginMm;
        public double ReceiptMarginLeftMm { get; set; } = PrintSettings.DefaultReceiptMarginMm;
        public string LabelPrinter { get; set; } = "";
        public string ReceiptPrinter { get; set; } = "";
        public Dictionary<string, PrinterProfileEntry> PrinterProfiles { get; set; }
            = new Dictionary<string, PrinterProfileEntry>(StringComparer.OrdinalIgnoreCase);
    }

    public class PrinterProfileEntry
    {
        public PrinterJobProfile Label { get; set; }
        public PrinterJobProfile Receipt { get; set; }
    }

    public class PrinterJobProfile
    {
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
        public double GapMm { get; set; } = -1;
        public double MarginMm { get; set; } = -1;
        public double MarginTopMm { get; set; } = -1;
        public double MarginRightMm { get; set; } = -1;
        public double MarginBottomMm { get; set; } = -1;
        public double MarginLeftMm { get; set; } = -1;
        public int Columns { get; set; }
        public string PaperMode { get; set; }
        public double PageWidthMm { get; set; }
        public double PageHeightMm { get; set; }
    }
}
