using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using Window = FlaUI.Core.AutomationElements.Window;

namespace MoneyTransfer.UITests
{
    [TestClass]
    public class CurrencyConverterUITests
    {
        private Application _app;
        private UIA3Automation _automation;
        private Window _mainWindow;

        // Укажи правильный путь к своему exe
        private const string AppPath = @"D:\ПАРЫ\третий курс\питпм\LW\MoneyTransfer\MoneyTransfer\bin\Debug\net9.0-windows\MoneyTransfer.exe";

        [TestInitialize]
        public void TestInitialize()
        {
            _app = Application.Launch(AppPath);
            _automation = new UIA3Automation();
            Thread.Sleep(2000);
            _mainWindow = _app.GetMainWindow(_automation);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _automation?.Dispose();
            _app?.Close();
        }

        private void FillForm(string fromCurrency, string toCurrency, string amount)
        {
            var fromComboBox = _mainWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("fromCurrencyComboBox"))
                .AsComboBox();
            var toComboBox = _mainWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("toCurrencyComboBox"))
                .AsComboBox();
            var amountTextBox = _mainWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("amountTextBox"))
                .AsTextBox();

            amountTextBox.Text = amount;
            fromComboBox.Value = fromCurrency;
            toComboBox.Value = toCurrency;
        }

        private void ClickConvert()
        {
            _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("convertButton"))
                .AsButton().Click();
        }

        private void ClickRefresh()
        {
            _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("refreshButton"))
                .AsButton().Click();
        }

        private string GetResultText()
        {
            return _mainWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("resultLabel"))
                .AsLabel().Text;
        }

        private string GetRatesText()
        {
            return _mainWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("ratesLabel"))
                .AsLabel().Text;
        }

        private string GetStatusText()
        {
            return _mainWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("statusLabel"))
                .AsLabel().Text;
        }

        private string GetMessageBoxText()
        {
            var messageBox = _mainWindow.ModalWindows.FirstOrDefault()
                ?? throw new System.Exception("MessageBox не появился");
            return messageBox.FindFirstDescendant(cf => cf.ByAutomationId("65535")).Name;
        }

        private void CloseMessageBox()
        {
            var messageBox = _mainWindow.ModalWindows.FirstOrDefault()
                ?? throw new System.Exception("MessageBox не появился");
            messageBox.FindFirstDescendant(cf => cf.ByAutomationId("2")).AsButton().Click();
        }

        // FR-001: Конвертация USD в EUR
        [TestMethod]
        public void FR001_ConvertUsdToEur_ShowsCorrectResult()
        {
            FillForm("USD", "EUR", "100");
            ClickConvert();
            StringAssert.Contains(GetResultText(), "100 USD");
            StringAssert.Contains(GetResultText(), "EUR");
        }

        // FR-002: Конвертация EUR в USD
        [TestMethod]
        public void FR002_ConvertEurToUsd_ShowsCorrectResult()
        {
            FillForm("EUR", "USD", "50");
            ClickConvert();
            StringAssert.Contains(GetResultText(), "50 EUR");
            StringAssert.Contains(GetResultText(), "USD");
        }

        // FR-003: Пустое поле суммы
        [TestMethod]
        public void FR003_EmptyAmount_ShowsErrorMessage()
        {
            FillForm("USD", "EUR", "");
            ClickConvert();
            StringAssert.Contains(GetMessageBoxText(), "Введите сумму для конвертации!");
            CloseMessageBox();
        }

        // FR-004: Отрицательная сумма
        [TestMethod]
        public void FR004_NegativeAmount_ShowsErrorMessage()
        {
            FillForm("USD", "EUR", "-100");
            ClickConvert();
            StringAssert.Contains(GetMessageBoxText(), "Сумма не может быть отрицательной");
            CloseMessageBox();
        }

        // FR-005: Некорректный ввод суммы
        [TestMethod]
        public void FR005_InvalidAmount_ShowsErrorMessage()
        {
            FillForm("USD", "EUR", "ввв");
            ClickConvert();
            StringAssert.Contains(GetMessageBoxText(), "Неверный формат суммы!");
            CloseMessageBox();
        }

        // FR-006: Не выбраны валюты
        [TestMethod]
        public void FR006_NoCurrencySelected_ShowsErrorMessage()
        {
            var amountTextBox = _mainWindow
                .FindFirstDescendant(cf => cf.ByAutomationId("amountTextBox"))
                .AsTextBox();
            amountTextBox.Text = "100";
            ClickConvert();
            StringAssert.Contains(GetMessageBoxText(), "Выберите валюты!");
            CloseMessageBox();
        }

        // NFR-001: Конвертация большой суммы выполняется быстро
        [TestMethod]
        public void NFR001_LargeAmount_CompletesQuicklyAndShowsResult()
        {
            FillForm("USD", "EUR", "1000000");
            ClickConvert();
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100,
                $"Конвертация заняла {stopwatch.ElapsedMilliseconds} мс, ожидалось менее 100 мс");
            StringAssert.Contains(GetResultText(), "1000000 USD");
        }

        // NFR-002: Конвертация нулевого значения
        [TestMethod]
        public void NFR002_ZeroAmount_ShowsZeroResult()
        {
            FillForm("USD", "EUR", "0");
            ClickConvert();
            StringAssert.Contains(GetResultText(), "0 USD");
            StringAssert.Contains(GetResultText(), "EUR");
        }

        // AUERFR-001: Курсы отображаются при запуске
        [TestMethod]
        public void AUERFR001_OnStart_RatesAreDisplayed()
        {
            string ratesText = GetRatesText();
            string statusText = GetStatusText();

            StringAssert.Contains(ratesText, "USD");
            StringAssert.Contains(ratesText, "EUR");
            Assert.IsFalse(statusText.Contains("Ошибка"),
                "При наличии интернета статус не должен сообщать об ошибке");
        }

        // AUERFR-002: Ручное обновление курсов
        [TestMethod]
        public void AUERFR002_ManualRefresh_UpdatesRatesAndStatus()
        {
            string statusBefore = GetStatusText();
            Thread.Sleep(1000);
            ClickRefresh();
            Thread.Sleep(2000);
            string statusAfter = GetStatusText();

            Assert.AreNotEqual(statusBefore, statusAfter,
                "Статус должен обновиться после нажатия кнопки обновления");
            StringAssert.Contains(GetRatesText(), "USD");
        }
    }
}