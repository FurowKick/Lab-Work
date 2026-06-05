using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MoneyTransfer.Models
{
    public class CurrencyConverter
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private Dictionary<string, decimal> _rates = new()
        {
            ["USD_EUR"] = 0.88m,
            ["EUR_USD"] = 1.12m
        };

        public DateTime LastUpdated { get; private set; } = DateTime.MinValue;
        public bool IsUsingFallback { get; private set; } = false;

        public async Task UpdateRatesAsync()
        {
            try
            {
                // Получаем курсы относительно USD
                string url = "https://api.frankfurter.app/latest?from=USD";
                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);

                var rates = doc.RootElement.GetProperty("rates");
                decimal eurRate = rates.GetProperty("EUR").GetDecimal();

                _rates["USD_EUR"] = eurRate;
                _rates["EUR_USD"] = Math.Round(1m / eurRate, 6);

                LastUpdated = DateTime.Now;
                IsUsingFallback = false;
            }
            catch
            {
                IsUsingFallback = true;
                throw;
            }
        }

        public decimal Convert(decimal amount, string fromCurrency, string toCurrency)
        {
            if (amount < 0)
                throw new ArgumentException("Сумма не может быть отрицательной.");

            string key = $"{fromCurrency}_{toCurrency}";

            if (_rates.TryGetValue(key, out decimal rate))
                return Math.Round(amount * rate, 2);

            throw new NotSupportedException("Не поддерживаемая пара валют.");
        }

        public decimal GetRate(string fromCurrency, string toCurrency)
        {
            string key = $"{fromCurrency}_{toCurrency}";
            return _rates.TryGetValue(key, out decimal rate) ? rate : 0;
        }
    }
}