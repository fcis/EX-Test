using System;

namespace Core.Common
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;

        // Store port as int to avoid conversion issues
        private string _portString = "587";
        private int _port = 587;

        public object Port
        {
            get => _port;
            set
            {
                // Handle both string and int configurations
                if (value is string portStr)
                {
                    _portString = portStr;
                    if (int.TryParse(portStr, out int parsedPort))
                    {
                        _port = parsedPort;
                    }
                }
                else if (value is int portInt)
                {
                    _port = portInt;
                    _portString = portInt.ToString();
                }
            }
        }

        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Additional properties for more flexible configuration
        public bool EnableSsl { get; set; } = true;
        public int Timeout { get; set; } = 30000; // 30 seconds default timeout
    }
}