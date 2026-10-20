using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace InventorySystem.Helpers
{
    public class BarcodeScanner
    {
        private StringBuilder _buffer = new StringBuilder();
        private Stopwatch _timer = new Stopwatch();
        private const int INTER_KEY_TIMEOUT = 50; // ms between keys
        private const int MIN_LENGTH = 3; // Min barcode length

        public event Action<string> BarcodeScanned;

        public void HandleKey(char key)
        {
            // If too much time passed, reset buffer (it was manual typing)
            if (_timer.IsRunning && _timer.ElapsedMilliseconds > INTER_KEY_TIMEOUT)
            {
                _buffer.Clear();
            }

            _timer.Restart();
            
            if (key == '\r' || key == '\n') // Enter key typically ends a scan
            {
                if (_buffer.Length >= MIN_LENGTH)
                {
                    BarcodeScanned?.Invoke(_buffer.ToString());
                }
                _buffer.Clear();
            }
            else
            {
                _buffer.Append(key);
            }
        }
    }
}
