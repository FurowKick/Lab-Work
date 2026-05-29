using System;
using System.Diagnostics;
using System.Windows.Forms;
using MoneyTransfer.Models;

namespace MoneyTransfer.Forms
{
    public class CurrencyConverterForm : Form
    {
        private CurrencyConverter converter;
        private ComboBox fromCurrencyComboBox;
        private ComboBox toCurrencyComboBox;
        private TextBox amountTextBox;
        private Button convertButton;
        private Label resultLabel;

        public CurrencyConverterForm()
        {
            this.Text = "Конвертер валют";
            this.Width = 300;
            this.Height = 200;

            fromCurrencyComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(10, 10),
                Width = 100,
                Items = { "USD", "EUR" }
            };

            toCurrencyComboBox = new ComboBox
            {
                Location = new System.Drawing.Point(120, 10),
                Width = 100,
                Items = { "USD", "EUR" }
            };

            amountTextBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 40),
                Width = 210,
                PlaceholderText = "Сумма"
            };

            convertButton = new Button
            {
                Location = new System.Drawing.Point(10, 70),
                Text = "Конвертировать",
                Width = 210
            };
            convertButton.Click += ConvertButton_Click;

            resultLabel = new Label
            {
                Location = new System.Drawing.Point(10, 100),
                Width = 210,
                Text = "Результат: "
            };

            this.Controls.Add(fromCurrencyComboBox);
            this.Controls.Add(toCurrencyComboBox);
            this.Controls.Add(amountTextBox);
            this.Controls.Add(convertButton);
            this.Controls.Add(resultLabel);

            converter = new CurrencyConverter();
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
                {
                    Debug.WriteLine($"[WARN] Превышено время: {stopwatch.Elapsed.TotalMilliseconds:F2} мс");
                }
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
                MessageBox.Show($"Непредвиденная ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}