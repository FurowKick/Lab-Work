using System;
using System.Windows.Forms;
using MoneyTransfer.Forms;

namespace MoneyTransfer
{

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CurrencyConverterForm());
        }
    }
}