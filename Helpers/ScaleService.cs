using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading;

namespace InventorySystem.Helpers
{
    public class ScaleConfig
    {
        public string PortName { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public string Parity { get; set; } = "None";
        public string StopBits { get; set; } = "One";
        public string DefaultUnit { get; set; } = "kg";
        public bool AutoConnect { get; set; } = false;
    }

    public class ScaleService
    {
        private static ScaleService _instance;
        public static ScaleService Instance => _instance ??= new ScaleService();

        private SerialPort _serialPort;
        private StringBuilder _readBuffer = new StringBuilder();
        private readonly object _lockObj = new object();

        public ScaleConfig Config { get; private set; } = new ScaleConfig();
        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;
        public decimal LastWeight { get; private set; } = 0m;
        public string LastUnit { get; private set; } = "kg";
        public bool IsLastStable { get; private set; } = true;

        public event Action<decimal, string, bool> WeightReceived;
        public event Action<bool, string> StatusChanged;

        private ScaleService()
        {
            LoadConfig();
        }

        public static string[] GetAvailablePorts()
        {
            try
            {
                return SerialPort.GetPortNames();
            }
            catch
            {
                return new string[0];
            }
        }

        public bool Connect(string portName = null, int baudRate = 0)
        {
            Disconnect();

            string port = string.IsNullOrEmpty(portName) ? Config.PortName : portName;
            int baud = baudRate > 0 ? baudRate : Config.BaudRate;

            if (string.IsNullOrEmpty(port))
            {
                StatusChanged?.Invoke(false, "No COM port specified.");
                return false;
            }

            try
            {
                Parity parity = Enum.TryParse(Config.Parity, true, out Parity p) ? p : Parity.None;
                StopBits stopBits = Enum.TryParse(Config.StopBits, true, out StopBits sb) ? sb : StopBits.One;

                _serialPort = new SerialPort(port, baud, parity, Config.DataBits, stopBits)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000,
                    DtrEnable = true,
                    RtsEnable = true
                };

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                Config.PortName = port;
                Config.BaudRate = baud;
                SaveConfig();

                StatusChanged?.Invoke(true, $"Connected to scale on {port} ({baud} baud).");
                return true;
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(false, $"Failed to connect on {port}: {ex.Message}");
                return false;
            }
        }

        public void Disconnect()
        {
            if (_serialPort != null)
            {
                try
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    _serialPort.Dispose();
                }
                catch { }
                finally
                {
                    _serialPort = null;
                    StatusChanged?.Invoke(false, "Scale disconnected.");
                }
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen) return;

                string incoming = _serialPort.ReadExisting();
                lock (_lockObj)
                {
                    _readBuffer.Append(incoming);
                    string rawData = _readBuffer.ToString();

                    int newlineIndex;
                    while ((newlineIndex = rawData.IndexOfAny(new char[] { '\r', '\n' })) >= 0)
                    {
                        string line = rawData.Substring(0, newlineIndex).Trim();
                        rawData = rawData.Substring(newlineIndex + 1);
                        _readBuffer.Clear();
                        _readBuffer.Append(rawData);

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            ParseWeightLine(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ScaleService.SerialPort_DataReceived");
            }
        }

        public void ParseWeightLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            // Pattern for standard scale outputs: e.g. ST,GS,+00.525kg, WW00.525kg, ST 0.525 kg, 0.525
            bool isStable = !line.ToUpper().Contains("US") && !line.ToUpper().Contains("DYN") && !line.Contains("?");

            string unit = "kg";
            if (line.ToLower().Contains("g") && !line.ToLower().Contains("kg")) unit = "g";
            else if (line.ToLower().Contains("lb")) unit = "lb";
            else if (line.ToLower().Contains("oz")) unit = "oz";

            // Extract numeric portion (including decimal point and sign)
            Match match = Regex.Match(line, @"[-+]?\d*\.?\d+");
            if (match.Success && decimal.TryParse(match.Value, out decimal weight))
            {
                LastWeight = weight;
                LastUnit = unit;
                IsLastStable = isStable;
                WeightReceived?.Invoke(weight, unit, isStable);
            }
        }

        public void SendTare()
        {
            SendCommand("T\r\n");
        }

        public void SendZero()
        {
            SendCommand("Z\r\n");
        }

        public void RequestWeight()
        {
            SendCommand("W\r\n");
        }

        private void SendCommand(string command)
        {
            try
            {
                if (IsConnected)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(command);
                    _serialPort.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"ScaleService.SendCommand({command.Trim()})");
            }
        }

        public void SimulateWeight(decimal weight, string unit = "kg", bool isStable = true)
        {
            LastWeight = weight;
            LastUnit = unit;
            IsLastStable = isStable;
            WeightReceived?.Invoke(weight, unit, isStable);
        }

        private string GetConfigPath()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, "scale_settings.json");
        }

        public void LoadConfig()
        {
            try
            {
                string path = GetConfigPath();
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    Config = JsonSerializer.Deserialize<ScaleConfig>(json) ?? new ScaleConfig();
                }
            }
            catch { Config = new ScaleConfig(); }
        }

        public void SaveConfig()
        {
            try
            {
                string path = GetConfigPath();
                string json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "ScaleService.SaveConfig");
            }
        }

        /// <summary>Applies config fields and persists. Does not open/close the port.</summary>
        public void UpdateConfig(ScaleConfig next)
        {
            if (next == null) return;
            if (!string.IsNullOrWhiteSpace(next.PortName)) Config.PortName = next.PortName.Trim();
            if (next.BaudRate > 0) Config.BaudRate = next.BaudRate;
            if (next.DataBits > 0) Config.DataBits = next.DataBits;
            if (!string.IsNullOrWhiteSpace(next.Parity)) Config.Parity = next.Parity;
            if (!string.IsNullOrWhiteSpace(next.StopBits)) Config.StopBits = next.StopBits;
            if (!string.IsNullOrWhiteSpace(next.DefaultUnit)) Config.DefaultUnit = next.DefaultUnit;
            Config.AutoConnect = next.AutoConnect;
            SaveConfig();
        }
    }
}
