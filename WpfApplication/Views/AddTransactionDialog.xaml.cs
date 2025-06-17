using System;
using System.Windows;
using System.Windows.Controls;

namespace VaultTrack.Views
{
    public partial class AddTransactionDialog : Window
    {
        public decimal Amount { get; private set; }
        public string? Description { get; private set; }
        public string? TransactionType { get; private set; }

        public AddTransactionDialog()
        {
            InitializeComponent();
            TypeComboBox.SelectedIndex = 0;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            Amount = decimal.Parse(AmountTextBox.Text);
            Description = DescriptionTextBox.Text;
            TransactionType = ((ComboBoxItem)TypeComboBox.SelectedItem).Content.ToString();

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
            if (string.IsNullOrWhiteSpace(AmountTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть суму.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Будь ласка, введіть коректну додатню суму.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть опис.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
} 