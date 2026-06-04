using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoneyTransfer.Models;
using System;

namespace MoneyTransfer.Tests
{
    [TestClass]
    public class CurrencyConverterTest
    {
        private CurrencyConverter _converter;

        [TestInitialize]
        public void SetUp() => _converter = new CurrencyConverter();

        // Тестирует: Корректность конвертации из USD в EUR — результат положительный и пропорциональный сумме
        // Тип: Позитивный тест
        [DataTestMethod]
        [DataRow(100.0, "USD", "EUR")]
        [DataRow(50.0, "USD", "EUR")]
        [DataRow(33.33, "USD", "EUR")]
        public void Convert_USD_to_EUR_ReturnsPositiveResult(double amount, string from, string to)
        {
            decimal result = _converter.Convert((decimal)amount, from, to);
            Assert.IsTrue(result > 0);
        }

        // Тестирует: Конвертация нуля всегда возвращает ноль независимо от курса
        // Тип: Позитивный тест
        [TestMethod]
        public void Convert_ZeroAmount_ReturnsZero()
        {
            Assert.AreEqual(0m, _converter.Convert(0m, "USD", "EUR"));
            Assert.AreEqual(0m, _converter.Convert(0m, "EUR", "USD"));
        }

        // Тестирует: Большая сумма конвертируется пропорционально меньшей
        // Тип: Позитивный тест
        [TestMethod]
        public void Convert_USD_to_EUR_LargerAmountGivesLargerResult()
        {
            decimal small = _converter.Convert(50m, "USD", "EUR");
            decimal large = _converter.Convert(100m, "USD", "EUR");
            Assert.IsTrue(large > small);
        }

        // Тестирует: Корректность конвертации из EUR в USD — результат положительный
        // Тип: Позитивный тест
        [DataTestMethod]
        [DataRow(100.0, "EUR", "USD")]
        [DataRow(50.0, "EUR", "USD")]
        [DataRow(25.5, "EUR", "USD")]
        public void Convert_EUR_to_USD_ReturnsPositiveResult(double amount, string from, string to)
        {
            decimal result = _converter.Convert((decimal)amount, from, to);
            Assert.IsTrue(result > 0);
        }

        // Тестирует: Выбрасывание ArgumentException при отрицательной сумме
        // Тип: Негативный тест
        [DataTestMethod]
        [DataRow(-1.0, "USD", "EUR")]
        [DataRow(-100.5, "EUR", "USD")]
        public void Convert_NegativeAmount_ThrowsArgumentException(double amount, string from, string to)
        {
            Assert.ThrowsException<ArgumentException>(() =>
                _converter.Convert((decimal)amount, from, to));
        }

        // Тестирует: Выбрасывание NotSupportedException для неподдерживаемых пар валют
        // Тип: Негативный тест
        [DataTestMethod]
        [DataRow(100.0, "USD", "GBP")]
        [DataRow(100.0, "EUR", "GBP")]
        [DataRow(100.0, "GBP", "USD")]
        [DataRow(100.0, "USD", "USD")]
        [DataRow(100.0, "EUR", "EUR")]
        [DataRow(100.0, "usd", "EUR")]
        [DataRow(100.0, "USD", "eur")]
        public void Convert_InvalidCurrencyPair_ThrowsNotSupportedException(double amount, string from, string to)
        {
            Assert.ThrowsException<NotSupportedException>(() =>
                _converter.Convert((decimal)amount, from, to));
        }
    }
}