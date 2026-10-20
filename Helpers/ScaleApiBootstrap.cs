using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;

namespace InventorySystem.Helpers
{
    /// <summary>
    /// Registers TM-A17 / COM scale REST endpoints and SignalR weight push
    /// for the web POS shell.
    /// </summary>
    public static class ScaleApiBootstrap
    {
        private static Microsoft.AspNetCore.Http.IResult ScaleDisabled() =>
            Microsoft.AspNetCore.Http.Results.Json(
                new { success = false, error = "Scale feature is disabled", enabled = false },
                statusCode: 403);

        public static void WireBroadcasts()
        {
            ScaleService.Instance.WeightReceived += (weight, unit, stable) =>
            {
                if (!FeatureFlags.ScaleEnabled) return;
                _ = InventorySystem.InventoryBroadcaster.BroadcastObject("ScaleWeight", new
                {
                    weight,
                    unit,
                    stable,
                    connected = ScaleService.Instance.IsConnected
                });
            };
            ScaleService.Instance.StatusChanged += (connected, message) =>
            {
                if (!FeatureFlags.ScaleEnabled) return;
                _ = InventorySystem.InventoryBroadcaster.BroadcastObject("ScaleStatus", new
                {
                    connected,
                    message,
                    port = ScaleService.Instance.Config.PortName,
                    weight = ScaleService.Instance.LastWeight,
                    unit = ScaleService.Instance.LastUnit,
                    stable = ScaleService.Instance.IsLastStable
                });
            };

            try
            {
                if (FeatureFlags.ScaleEnabled && ScaleService.Instance.Config.AutoConnect)
                    ScaleService.Instance.Connect();
            }
            catch { }
        }

        public static void MapEndpoints(WebApplication app)
        {
            app.MapGet("/api/scale/status", () =>
            {
                if (!FeatureFlags.ScaleEnabled)
                {
                    return Microsoft.AspNetCore.Http.Results.Ok(new
                    {
                        enabled = false,
                        connected = false,
                        weight = 0m,
                        unit = "kg",
                        stable = true,
                        port = "",
                        baudRate = 9600,
                        autoConnect = false,
                        defaultUnit = "kg"
                    });
                }

                var s = ScaleService.Instance;
                return Microsoft.AspNetCore.Http.Results.Ok(new
                {
                    enabled = true,
                    connected = s.IsConnected,
                    weight = s.LastWeight,
                    unit = s.LastUnit,
                    stable = s.IsLastStable,
                    port = s.Config.PortName,
                    baudRate = s.Config.BaudRate,
                    autoConnect = s.Config.AutoConnect,
                    defaultUnit = s.Config.DefaultUnit
                });
            });

            app.MapGet("/api/scale/ports", () =>
                FeatureFlags.ScaleEnabled
                    ? Microsoft.AspNetCore.Http.Results.Ok(ScaleService.GetAvailablePorts())
                    : ScaleDisabled());

            app.MapGet("/api/scale/weight", () =>
            {
                if (!FeatureFlags.ScaleEnabled) return ScaleDisabled();
                var s = ScaleService.Instance;
                return Microsoft.AspNetCore.Http.Results.Ok(new
                {
                    connected = s.IsConnected,
                    weight = s.LastWeight,
                    unit = s.LastUnit,
                    stable = s.IsLastStable
                });
            });

            app.MapPost("/api/scale/connect", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
            {
                if (!FeatureFlags.ScaleEnabled) return ScaleDisabled();
                try
                {
                    string port = null;
                    int baud = 0;
                    try
                    {
                        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ScaleConnectPayload>(
                            request.Body,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (body != null)
                        {
                            port = body.Port;
                            baud = body.BaudRate;
                        }
                    }
                    catch { }

                    bool ok = ScaleService.Instance.Connect(port, baud);
                    return ok
                        ? Microsoft.AspNetCore.Http.Results.Ok(new { success = true, port = ScaleService.Instance.Config.PortName })
                        : Microsoft.AspNetCore.Http.Results.BadRequest(new { success = false, error = "Failed to connect to scale." });
                }
                catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
            });

            app.MapPost("/api/scale/disconnect", () =>
            {
                if (!FeatureFlags.ScaleEnabled) return ScaleDisabled();
                ScaleService.Instance.Disconnect();
                return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
            });

            app.MapPost("/api/scale/tare", () =>
            {
                if (!FeatureFlags.ScaleEnabled) return ScaleDisabled();
                ScaleService.Instance.SendTare();
                return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
            });

            app.MapPost("/api/scale/zero", () =>
            {
                if (!FeatureFlags.ScaleEnabled) return ScaleDisabled();
                ScaleService.Instance.SendZero();
                return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
            });

            app.MapPost("/api/scale/request", () =>
            {
                if (!FeatureFlags.ScaleEnabled) return ScaleDisabled();
                ScaleService.Instance.RequestWeight();
                return Microsoft.AspNetCore.Http.Results.Ok(new
                {
                    success = true,
                    weight = ScaleService.Instance.LastWeight,
                    unit = ScaleService.Instance.LastUnit,
                    stable = ScaleService.Instance.IsLastStable
                });
            });

            app.MapPost("/api/scale/config", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
            {
                if (!FeatureFlags.ScaleEnabled) return ScaleDisabled();
                try
                {
                    var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ScaleConfig>(
                        request.Body,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (body == null) return Microsoft.AspNetCore.Http.Results.BadRequest("Invalid config");
                    ScaleService.Instance.UpdateConfig(body);
                    return Microsoft.AspNetCore.Http.Results.Ok(new { success = true, config = ScaleService.Instance.Config });
                }
                catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
            });

            app.MapPost("/api/scale/simulate", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
            {
                if (!FeatureFlags.ScaleEnabled) return ScaleDisabled();
                try
                {
                    var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ScaleSimulatePayload>(
                        request.Body,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (body == null) return Microsoft.AspNetCore.Http.Results.BadRequest("Invalid payload");
                    ScaleService.Instance.SimulateWeight(
                        body.Weight,
                        string.IsNullOrWhiteSpace(body.Unit) ? "kg" : body.Unit,
                        body.Stable);
                    return Microsoft.AspNetCore.Http.Results.Ok(new { success = true });
                }
                catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
            });

            app.MapPost("/api/scale/resolve-barcode", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
            {
                if (!FeatureFlags.ScaleEnabled)
                    return Microsoft.AspNetCore.Http.Results.Ok(new { isScaleBarcode = false, enabled = false });
                try
                {
                    var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ScaleBarcodePayload>(
                        request.Body,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    string barcode = body?.Barcode?.Trim();
                    if (string.IsNullOrEmpty(barcode))
                        return Microsoft.AspNetCore.Http.Results.BadRequest(new { error = "Barcode required" });

                    if (!ScaleBarcodeParser.IsScaleBarcode(barcode))
                        return Microsoft.AspNetCore.Http.Results.Ok(new { isScaleBarcode = false });

                    var parsed = ScaleBarcodeParser.Parse(barcode);
                    if (!parsed.IsSuccess)
                        return Microsoft.AspNetCore.Http.Results.BadRequest(new { isScaleBarcode = true, error = parsed.Message });

                    var dt = DatabaseHelper.ExecuteDataTable(
                        @"SELECT id, part_name, selling_price, quantity_in_stock, item_type, barcode, part_number
                          FROM parts
                          WHERE date_deleted IS NULL
                            AND (barcode = @bc OR part_number = @bc
                                 OR part_number LIKE @plu OR barcode LIKE @plu)
                          LIMIT 1",
                        new SqliteParameter("@bc", barcode),
                        new SqliteParameter("@plu", "%" + parsed.ProductCode + "%"));

                    if (dt.Rows.Count == 0)
                        return Microsoft.AspNetCore.Http.Results.NotFound(new
                        {
                            isScaleBarcode = true,
                            error = "Product not found for scale PLU",
                            plu = parsed.ProductCode
                        });

                    var r = dt.Rows[0];
                    int id = Convert.ToInt32(r["id"]);
                    string name = r["part_name"].ToString();
                    decimal unitPrice = Convert.ToDecimal(r["selling_price"]);
                    int stock = Convert.ToInt32(r["quantity_in_stock"]);
                    string itemType = r["item_type"] != DBNull.Value ? r["item_type"].ToString() : "Product";
                    bool isService = string.Equals(itemType, "Service", StringComparison.OrdinalIgnoreCase);

                    decimal linePrice = unitPrice;
                    int qty = 1;
                    string lineName = name;
                    if (parsed.BarcodeType == ScaleBarcodeType.WeightBased)
                    {
                        decimal weight = parsed.WeightKg;
                        // Line stores total for this weight; qty stays 1 so cart never double-counts.
                        linePrice = Math.Round(unitPrice * weight, 2);
                        qty = 1;
                        lineName = $"{name} ({weight:N3}kg)";
                    }
                    else if (parsed.BarcodeType == ScaleBarcodeType.PriceBased)
                    {
                        linePrice = parsed.TotalPrice;
                        qty = 1;
                        lineName = name;
                    }

                    return Microsoft.AspNetCore.Http.Results.Ok(new
                    {
                        isScaleBarcode = true,
                        barcodeType = parsed.BarcodeType.ToString(),
                        plu = parsed.ProductCode,
                        weightKg = parsed.WeightKg,
                        totalPrice = parsed.TotalPrice,
                        message = parsed.Message,
                        product = new
                        {
                            id,
                            name,
                            price = unitPrice,
                            stock,
                            isService,
                            barcode = r["barcode"]?.ToString(),
                            sku = r["part_number"]?.ToString()
                        },
                        line = new
                        {
                            name = lineName,
                            price = linePrice,
                            qty,
                            weightKg = parsed.WeightKg,
                            stockQty = parsed.BarcodeType == ScaleBarcodeType.WeightBased
                                ? (int)Math.Max(1, Math.Round(parsed.WeightKg * 1000m))
                                : 0
                        }
                    });
                }
                catch (Exception ex) { return Microsoft.AspNetCore.Http.Results.Problem(ex.Message); }
            });
        }

        private class ScaleConnectPayload
        {
            public string Port { get; set; }
            public int BaudRate { get; set; }
        }

        private class ScaleSimulatePayload
        {
            public decimal Weight { get; set; }
            public string Unit { get; set; }
            public bool Stable { get; set; } = true;
        }

        private class ScaleBarcodePayload
        {
            public string Barcode { get; set; }
        }
    }
}
