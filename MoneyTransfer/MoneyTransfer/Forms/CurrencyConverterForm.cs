using System;
using System.Diagnostics;
using System.Windows.Forms;
using MoneyTransfer.Models;

namespace MoneyTransfer.Forms
{
    public class CurrencyConverterForm : Form
    {
        private CurrencyConverter converter;
        private System.Windows.Forms.Timer updateTimer;

        private ComboBox fromCurrencyComboBox;
        private ComboBox toCurrencyComboBox;
        private TextBox amountTextBox;
        private Button convertButton;
        private Button refreshButton;
        private Label resultLabel;
        private Label ratesLabel;
        private Label statusLabel;

        public CurrencyConverterForm()
        {
            this.Text = "Конвертер валют";
            this.Width = 340;
            this.Height = 280;

            fromCurrencyComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(10, 10),
                Width = 100,
                Items = { "USD", "EUR" },
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            toCurrencyComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(120, 10),
                Width = 100,
                Items = { "USD", "EUR" },
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            amountTextBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 45),
                Width = 210,
                PlaceholderText = "Сумма"
            };

            convertButton = new Button
            {
                Location = new System.Drawing.Point(10, 75),
                Text = "Конвертировать",
                Width = 150
            };
            convertButton.Click += ConvertButton_Click;

            refreshButton = new Button
            {
                Location = new System.Drawing.Point(170, 75),
                Text = "Обновить курс",
                Width = 120
            };
            refreshButton.Click += RefreshButton_Click;

            resultLabel = new Label
            {
                Location = new System.Drawing.Point(10, 115),
                Width = 300,
                Text = "Результат: —"
            };

            ratesLabel = new Label
            {
                Location = new System.Drawing.Point(10, 145),
                Width = 300,
                Text = "Курсы: загрузка..."
            };

            statusLabel = new Label
            {
                Location = new System.Drawing.Point(10, 175),
                Width = 300,
                Height = 150,
                ForeColor = System.Drawing.Color.Gray,
                Text = "Последнее обновление: —"
            };

            this.Controls.AddRange(new Control[]
            {
                fromCurrencyComboBox, toCurrencyComboBox,
                amountTextBox, convertButton, refreshButton,
                resultLabel, ratesLabel, statusLabel
            });

            converter = new CurrencyConverter();

            // Обновляем курсы при запуске
            _ = LoadRatesAsync();

            // Автообновление каждые 10 минут
            updateTimer = new System.Windows.Forms.Timer { Interval = 10 * 60 * 1000 };
            updateTimer.Tick += async (s, e) => await LoadRatesAsync();
            updateTimer.Start();
        }

        private async Task LoadRatesAsync()
        {
            refreshButton.Enabled = false;
            statusLabel.Text = "Обновление курсов...";
            statusLabel.ForeColor = System.Drawing.Color.Gray;

            try
            {
                await converter.UpdateRatesAsync();
                UpdateRatesDisplay();
                statusLabel.Text = $"Обновлено: {converter.LastUpdated:HH:mm:ss}";
                statusLabel.ForeColor = System.Drawing.Color.Green;
            }
            catch
            {
                statusLabel.Text = "Ошибка получения курсов.\nИспользуются последние данные.";
                statusLabel.ForeColor = System.Drawing.Color.OrangeRed;
                UpdateRatesDisplay();
            }
            finally
            {
                refreshButton.Enabled = true;
            }
        }

        private void UpdateRatesDisplay()
        {
            decimal usdEur = converter.GetRate("USD", "EUR");
            decimal eurUsd = converter.GetRate("EUR", "USD");
            ratesLabel.Text = $"1 USD = {usdEur} EUR   |   1 EUR = {eurUsd} USD";
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            await LoadRatesAsync();
        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(amountTextBox.Text))
            {
                MessageBox.Show("Введите сумму для конвертации!");
                return;
            }

            if (!decimal.TryParse(amountTextBox.Text, out decimal amount))
            {
                MessageBox.Show("Неверный формат суммы!");
                return;
            }

            if (fromCurrencyComboBox.SelectedItem == null || toCurrencyComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите валюты!");
                return;
            }

            string fromCurrency = fromCurrencyComboBox.SelectedItem.ToString();
            string toCurrency = toCurrencyComboBox.SelectedItem.ToString();

            try
            {
                var stopwatch = Stopwatch.StartNew();
                decimal result = converter.Convert(amount, fromCurrency, toCurrency);
                stopwatch.Stop();

                Debug.WriteLine($"[PERF] {amount} {fromCurrency} → {result} {toCurrency} | {stopwatch.Elapsed.TotalMilliseconds:F2} мс");

                resultLabel.Text = $"Результат: {amount} {fromCurrency} = {result} {toCurrency}";

                if (amount <= 1_000_000m && stopwatch.Elapsed.TotalMilliseconds > 100)
                    Debug.WriteLine($"[WARN] Превышено время: {stopwatch.Elapsed.TotalMilliseconds:F2} мс");
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(ex.Message, "Не поддерживается", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            updateTimer.Stop();
            updateTimer.Dispose();
            base.OnFormClosed(e);
        }
    }
}