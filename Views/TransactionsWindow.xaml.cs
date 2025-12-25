using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VaultTrack.Models;
using VaultTrack.Services;
using System.ComponentModel;
using System.Windows.Data;

namespace VaultTrack.Views
{
    public partial class TransactionsWindow : Window
    {
        private readonly SupabaseService _supabaseService;
        private readonly string _userId;
        private decimal _initialBalance;

        public ObservableCollection<string> FilterCategories { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<TransactionTypeItem> FilterTypes { get; set; } = new ObservableCollection<TransactionTypeItem>();

        private string _selectedFilterCategory;
        public string SelectedFilterCategory
        {
            get => _selectedFilterCategory;
            set { _selectedFilterCategory = value; ApplyFilters(); }
        }

        private TransactionTypeItem _selectedFilterType;
        public TransactionTypeItem SelectedFilterType
        {
            get => _selectedFilterType;
            set { _selectedFilterType = value; ApplyFilters(); }
        }

        private DateTime? _filterDateFrom;
        public DateTime? FilterDateFrom
        {
            get => _filterDateFrom;
            set { _filterDateFrom = value; ApplyFilters(); }
        }

        private DateTime? _filterDateTo;
        public DateTime? FilterDateTo
        {
            get => _filterDateTo;
            set { _filterDateTo = value; ApplyFilters(); }
        }

        private string _filterDescription;
        public string FilterDescription
        {
            get => _filterDescription;
            set { _filterDescription = value; ApplyFilters(); }
        }

        public ObservableCollection<Transaction> AllTransactions { get; set; } = new ObservableCollection<Transaction>();
        public ICollectionView FilteredTransactions { get; set; }

        public TransactionsWindow(string userId)
        {
            InitializeComponent();
            DataContext = this;
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
                AllTransactions = new ObservableCollection<Transaction>(transactions);
                FilteredTransactions = CollectionViewSource.GetDefaultView(AllTransactions);
                FilteredTransactions.Filter = FilterPredicate;
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

        private void ApplyFilters()
        {
            FilteredTransactions.Refresh();
        }

        private bool FilterPredicate(object obj)
        {
            if (obj is not Transaction t) return false;
            if (!string.IsNullOrEmpty(SelectedFilterCategory) && t.Category != SelectedFilterCategory) return false;
            if (SelectedFilterType != null && t.Type != SelectedFilterType.Key) return false;
            if (FilterDateFrom != null && t.Date < FilterDateFrom) return false;
            if (FilterDateTo != null && t.Date > FilterDateTo) return false;
            if (!string.IsNullOrWhiteSpace(FilterDescription) && (t.Description == null || !t.Description.ToLower().Contains(FilterDescription.ToLower()))) return false;
            return true;
        }

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

    public class TransactionTypeItem
    {
        public string Key { get; set; } // "Income" або "Expense"
        public string Display { get; set; } // "Дохід" або "Витрата"
        public override string ToString() => Display;
    }
} 