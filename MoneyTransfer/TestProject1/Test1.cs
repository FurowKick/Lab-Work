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

        // Тестирует: Корректность конвертации из USD в EUR для различных сумм
        // Тип: Позитивный тест
        [DataTestMethod]
        [DataRow(100.0, "USD", "EUR", 88.0)]
        [DataRow(50.0, "USD", "EUR", 44.0)]
        [DataRow(0.0, "USD", "EUR", 0.0)]
        [DataRow(33.33, "USD", "EUR", 29.3304)]
        public void Convert_USD_to_EUR_ReturnsCorrectResult(double amount, string from, string to, double expected)
        {
            decimal result = _converter.Convert((decimal)amount, from, to);
            Assert.AreEqual((decimal)expected, result, 0.0001m);
        }

        // Тестирует: Корректность конвертации из EUR в USD для различных сумм
        // Тип: Позитивный тест
        [DataTestMethod]
        [DataRow(100.0, "EUR", "USD", 112.0)]
        [DataRow(50.0, "EUR", "USD", 56.0)]
        [DataRow(0.0, "EUR", "USD", 0.0)]
        [DataRow(25.5, "EUR", "USD", 28.56)]
        public void Convert_EUR_to_USD_ReturnsCorrectResult(double amount, string from, string to, double expected)
        {
            decimal result = _converter.Convert((decimal)amount, from, to);
            Assert.AreEqual((decimal)expected, result, 0.0001m);
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
        // Тип: Негативный тест (включая одинаковые валюты и неверный регистр)
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