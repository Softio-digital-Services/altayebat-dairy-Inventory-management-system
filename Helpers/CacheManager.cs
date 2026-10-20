using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using InventorySystem.Data;

namespace InventorySystem.Helpers
{
    public static class CacheManager
    {
        private static ConcurrentDictionary<string, Bitmap> _imageCache = new ConcurrentDictionary<string, Bitmap>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Retrieves a cached product image or loads it from disk and caches it.
        /// Limits the cache size to prevent massive RAM usage.
        /// </summary>
        public static Bitmap GetProductImage(string imagePath, string category, int size)
        {
            string cacheKey = $"{imagePath}_{category}_{size}";

            if (_imageCache.TryGetValue(cacheKey, out Bitmap cachedImage))
            {
                return cachedImage;
            }

            // Limit cache size to 1000 items to prevent RAM bloat on huge databases
            if (_imageCache.Count > 1000)
            {
                _imageCache.Clear(); // Simple eviction strategy for now
            }

            Bitmap bmp = null;
            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    string fullPath = Path.Combine(Application.StartupPath, imagePath);
                    if (!File.Exists(fullPath))
                        fullPath = Path.Combine(Application.StartupPath, "Assets", "Products", Path.GetFileName(imagePath));

                    if (File.Exists(fullPath))
                    {
                        byte[] bytes = File.ReadAllBytes(fullPath);
                        using (var ms = new MemoryStream(bytes))
                        using (var original = Image.FromStream(ms))
                        {
                            bmp = new Bitmap(original, new Size(size, size));
                        }
                    }
                }
                catch { }
            }

            // Fallback to placeholder icon
            if (bmp == null)
            {
                bmp = new Bitmap(size, size);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    string iconName = "inventory";
                    if (!string.IsNullOrEmpty(category))
                    {
                        string cat = category.ToLower();
                        if (cat.Contains("engine")) iconName = "engine";
                        else if (cat.Contains("brake")) iconName = "brakes";
                        else if (cat.Contains("service")) iconName = "services";
                        else if (cat.Contains("accessory") || cat.Contains("accessories")) iconName = "accessories";
                        else if (cat.Contains("oil") || cat.Contains("fuel")) iconName = "oil";
                    }

                    Image icon = ThemeConfig.GetNuricon(iconName);
                    if (icon != null)
                    {
                        int sz = size / 2;
                        int pad = (size - sz) / 2;
                        g.DrawImage(icon, new Rectangle(pad, pad, sz, sz));
                    }
                }
            }

            _imageCache[cacheKey] = bmp;
            return bmp;
        }

        public static void ClearImageCache()
        {
            _imageCache.Clear();
        }
    }
}
