using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VaultTrack.Models;
using VaultTrack.Services;

namespace VaultTrack.Views
{
    public partial class TransactionsWindow : Window
    {
        private readonly SupabaseService _supabaseService;
        private readonly string _userId;
        private decimal _initialBalance;

        public TransactionsWindow(string userId)
        {
            InitializeComponent();
            _supabaseService = SupabaseService.Instance;
            _userId = userId;
            LoadTransactionsAndBalance();
        }

        private async void LoadTransactionsAndBalance()
        {
            try
            {
                // Load initial balance
                var userBalance = await _supabaseService.GetUserBalance(_userId);
                _initialBalance = userBalance?.BalanceAmount ?? 0m;
                BalanceInputTextBox.Text = _initialBalance.ToString(); // Display initial balance in the TextBox

                // Load transactions
                var transactions = await _supabaseService.GetTransactions(_userId);
                TransactionsListView.ItemsSource = transactions;
                CalculateTotalBalance(transactions);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateTotalBalance(ObservableCollection<Transaction> transactions)
        {
            decimal transactionsTotal = 0;
            foreach (var transaction in transactions)
            {
                if (transaction.Type == "Дохід")
                    transactionsTotal += transaction.Amount;
                else
                    transactionsTotal -= transaction.Amount;
            }
            // Total balance is initial balance + transactions total
            // The BalanceInputTextBox should show the _initialBalance, and transactions are applied on top of it for a final display
            // We update the BalanceInputTextBox only when saving, not on every transaction
            // So, for display purposes, we might need a separate TextBlock if we want to show current calculated balance and editable initial balance.
            // For now, let's keep BalanceInputTextBox as the editable initial balance.
            BalanceInputTextBox.Text = (_initialBalance + transactionsTotal).ToString("0.00");
        }

        // private async void AddTransaction_Click(object sender, RoutedEventArgs e)
        // {
        //     var dialog = new AddTransactionDialog();
        //     if (dialog.ShowDialog() == true)
        //     {
        //         try
        //         {
        //             var transaction = new Transaction
        //             {
        //                 UserId = Guid.Parse(_userId),
        //                 Amount = dialog.Amount,
        //                 Description = dialog.Description,
        //                 Type = dialog.TransactionType,
        //                 Date = DateTime.UtcNow
        //             };

        //             await _supabaseService.CreateTransaction(transaction);
        //             LoadTransactionsAndBalance();
        //         }
        //         catch (Exception ex)
        //         {
        //             MessageBox.Show($"Помилка додавання транзакції: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        //         }
        //     }
        // }

        private async void SaveBalance_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(BalanceInputTextBox.Text, out decimal newInitialBalance))
            {
                try
                {
                    var userBalance = new UserBalance
                    {
                        UserId = Guid.Parse(_userId),
                        BalanceAmount = newInitialBalance,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _supabaseService.UpsertUserBalance(userBalance);
                    _initialBalance = newInitialBalance; // Update local initial balance
                    LoadTransactionsAndBalance(); // Recalculate and display total
                    MessageBox.Show("Баланс успішно збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка збереження балансу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, введіть коректне числове значення для балансу.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
} 