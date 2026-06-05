using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoneyTransfer.Models;

namespace MoneyTransfer.Tests
{
    [TestClass]
    public class CurrencyConverterUpdateTests
    {
        private CurrencyConverter converter;

        [TestInitialize]
        public void Setup() => converter = new CurrencyConverter();

        // Тестирует: Курс USD/EUR больше нуля после успешного обновления
        // Тип: Позитивный тест
        [TestMethod]
        public async Task UpdateRatesAsync_OnSuccess_RateIsPositive()
        {
            await converter.UpdateRatesAsync();
            Assert.IsTrue(converter.GetRate("USD", "EUR") > 0);
        }

        // Тестирует: LastUpdated обновляется после успешного запроса к API
        // Тип: Позитивный тест
        [TestMethod]
        public async Task UpdateRatesAsync_OnSuccess_LastUpdatedIsSet()
        {
            await converter.UpdateRatesAsync();
            Assert.AreNotEqual(DateTime.MinValue, converter.LastUpdated);
        }
    }
}