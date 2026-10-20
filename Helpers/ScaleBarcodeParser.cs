using System;
using System.Text.RegularExpressions;

namespace InventorySystem.Helpers
{
    public class ScaleBarcodeResult
    {
        public bool IsSuccess { get; set; }
        public string RawBarcode { get; set; }
        public string ProductCode { get; set; }
        public decimal WeightKg { get; set; }
        public decimal TotalPrice { get; set; }
        public string Unit { get; set; } = "kg";
        public string Message { get; set; }
        public ScaleBarcodeType BarcodeType { get; set; }
    }

    public enum ScaleBarcodeType
    {
        Unknown,
        WeightBased,
        PriceBased
    }

    public static class ScaleBarcodeParser
    {
        /// <summary>
        /// Checks whether barcode matches standard EAN-13 scale prefixes (20..29)
        /// </summary>
        public static bool IsScaleBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return false;
            barcode = barcode.Trim();
            if (barcode.Length != 13) return false;

            string prefix = barcode.Substring(0, 2);
            return prefix == "20" || prefix == "21" || prefix == "22" || 
                   prefix == "23" || prefix == "24" || prefix == "25" || 
                   prefix == "26" || prefix == "27" || prefix == "28" || prefix == "29";
        }

        /// <summary>
        /// Parses TM-A17 / EAN-13 scale printed barcode labels.
        /// Standard formats:
        /// 20 PPPPP WWWWW C -> 2-digit prefix, 5-digit PLU, 5-digit Weight (in grams or 0.001kg), Checksum
        /// 22 PPPPP $$$$$ C -> 2-digit prefix, 5-digit PLU, 5-digit Total Price (in cents), Checksum
        /// 28 PPPP WWWWWW C -> 2-digit prefix, 4-digit PLU, 6-digit Weight/Price
        /// </summary>
        public static ScaleBarcodeResult Parse(string barcode)
        {
            ScaleBarcodeResult res = new ScaleBarcodeResult { RawBarcode = barcode, IsSuccess = false };

            if (!IsScaleBarcode(barcode))
            {
                res.Message = "Not a recognized scale barcode format.";
                return res;
            }

            try
            {
                string prefix = barcode.Substring(0, 2);
                
                // Format 1: 5-digit PLU (indices 2 to 6), 5-digit weight/price (indices 7 to 11)
                string plu5 = barcode.Substring(2, 5);
                string val5 = barcode.Substring(7, 5);
                
                if (decimal.TryParse(val5, out decimal valNumber))
                {
                    res.ProductCode = plu5;
                    
                    // Prefix 20, 21, 28 are standard weight scale barcodes (Weight in grams / 0.001kg)
                    if (prefix == "20" || prefix == "21" || prefix == "28")
                    {
                        res.BarcodeType = ScaleBarcodeType.WeightBased;
                        res.WeightKg = valNumber / 1000m; // Convert grams to kg
                        res.Unit = "kg";
                        res.IsSuccess = true;
                        res.Message = $"Parsed Weight: {res.WeightKg:N3} kg (PLU: {res.ProductCode})";
                    }
                    // Prefix 22, 23, 29 are price-embedded barcodes (Total Price in cents / 0.01)
                    else if (prefix == "22" || prefix == "23" || prefix == "29")
                    {
                        res.BarcodeType = ScaleBarcodeType.PriceBased;
                        res.TotalPrice = valNumber / 100m; // Convert cents to currency
                        res.IsSuccess = true;
                        res.Message = $"Parsed Price: {res.TotalPrice:C2} (PLU: {res.ProductCode})";
                    }
                    else
                    {
                        // Default fallback to weight in grams
                        res.BarcodeType = ScaleBarcodeType.WeightBased;
                        res.WeightKg = valNumber / 1000m;
                        res.Unit = "kg";
                        res.IsSuccess = true;
                        res.Message = $"Parsed Scale Barcode: {res.WeightKg:N3} kg (PLU: {res.ProductCode})";
                    }
                }
                else
                {
                    res.Message = "Failed to parse numeric value from scale barcode.";
                }
            }
            catch (Exception ex)
            {
                res.Message = "Error parsing scale barcode: " + ex.Message;
            }

            return res;
        }
    }
}
