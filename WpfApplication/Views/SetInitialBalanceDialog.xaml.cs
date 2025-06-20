using System;
using System.Windows;

namespace VaultTrack.Views
{
    public partial class SetInitialBalanceDialog : Window
    {
        public decimal InitialBalance { get; private set; }

        public SetInitialBalanceDialog(decimal currentBalance)
        {
            InitializeComponent();
            BalanceTextBox.Text = currentBalance.ToString();
        }

        private void Set_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            InitialBalance = decimal.Parse(BalanceTextBox.Text);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(BalanceTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть суму балансу.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(BalanceTextBox.Text, out decimal balance))
            {
                MessageBox.Show("Будь ласка, введіть коректну суму балансу.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
} 