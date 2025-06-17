using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using VaultTrack.Models;
using VaultTrack.Services;
using System.ComponentModel;
using System.Windows.Input;
using System.Threading.Tasks;

namespace VaultTrack.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly SupabaseService _supabaseService;
        private ObservableCollection<Transaction> _transactions;
        private decimal _totalIncome;
        private decimal _totalExpense;
        private decimal _balance;

        public MainViewModel()
        {
            _supabaseService = SupabaseService.Instance;
            _transactions = new ObservableCollection<Transaction>();
            
            LoadTransactionsCommand = new RelayCommand(async _ => await LoadTransactionsAsync());
            AddTransactionCommand = new RelayCommand(_ => AddTransaction());
            EditTransactionCommand = new RelayCommand(EditTransaction, _ => SelectedTransaction != null);
            DeleteTransactionCommand = new RelayCommand(DeleteTransaction, _ => SelectedTransaction != null);
        }

        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set
            {
                _transactions = value;
                OnPropertyChanged(nameof(Transactions));
            }
        }

        public decimal TotalIncome
        {
            get => _totalIncome;
            set
            {
                _totalIncome = value;
                OnPropertyChanged(nameof(TotalIncome));
            }
        }

        public decimal TotalExpense
        {
            get => _totalExpense;
            set
            {
                _totalExpense = value;
                OnPropertyChanged(nameof(TotalExpense));
            }
        }

        public decimal Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                OnPropertyChanged(nameof(Balance));
            }
        }

        public RelayCommand LoadTransactionsCommand { get; }
        public RelayCommand AddTransactionCommand { get; }
        public RelayCommand EditTransactionCommand { get; }
        public RelayCommand DeleteTransactionCommand { get; }

        private Transaction? _selectedTransaction;
        public Transaction? SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                _selectedTransaction = value;
                OnPropertyChanged(nameof(SelectedTransaction));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private async Task LoadTransactionsAsync()
        {
            try
            {
                var currentUser = _supabaseService.Auth.CurrentUser;
                if (currentUser == null)
                {
                    return;
                }

                var transactions = await _supabaseService.GetTransactions(currentUser.Id);
                // MessageBox.Show($"Отримано транзакцій з Supabase (до фільтрації): {transactions.Count}", "Відлагодження завантаження", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear existing transactions and add new ones to ensure UI updates correctly
                Transactions.Clear();
                foreach (var transaction in transactions.DistinctBy(t => t.Id))
                {
                    Transactions.Add(transaction);
                }
                // MessageBox.Show($"Додано унікальних транзакцій до списку (після фільтрації): {Transactions.Count}", "Відлагодження завантаження", MessageBoxButton.OK, MessageBoxImage.Information);
                UpdateSummaryData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні транзакцій: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task OnWindowLoadedAsync()
        {
            await LoadTransactionsAsync();
        }

        private void UpdateSummaryData()
        {
            try
            {
                TotalIncome = Transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
                TotalExpense = Transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);
                Balance = TotalIncome - TotalExpense;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при оновленні підсумків: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddTransaction()
        {
            try
            {
                var currentUser = _supabaseService.Auth.CurrentUser;
                if (currentUser == null)
                {
                    MessageBox.Show("Будь ласка, увійдіть у систему, щоб додавати транзакції.", "Помилка авторизації", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(currentUser.Id))
                {
                    MessageBox.Show("Не вдалося отримати ідентифікатор поточного користувача. Будь ласка, спробуйте увійти знову.", "Помилка ідентифікатора користувача", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newTransaction = new Transaction 
                {
                    UserId = Guid.Parse(currentUser.Id),
                    Date = DateTime.Now, 
                    Amount = 0, 
                    Category = "", 
                    Description = ""
                };
                var viewModel = new TransactionViewModel(newTransaction);
                var transactionWindow = new Views.TransactionWindow(viewModel);

                if (transactionWindow.ShowDialog() == true)
                {
                    // The transaction is already saved by TransactionViewModel.SaveAndClose.
                    // No need to call AddTransaction again here.
                    // var addedTransaction = await _supabaseService.AddTransaction(viewModel.Transaction);
                    
                    // if (addedTransaction != null)
                    // {
                        // Transactions.Add(addedTransaction);
                        UpdateSummaryData();
                        await LoadTransactionsAsync();
                    // }
                    // else
                    // {
                    //     MessageBox.Show("Не вдалося додати транзакцію.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    // }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при додаванні транзакції: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditTransaction(object? parameter)
        {
            try
            {
                if (parameter is Transaction transactionToEdit)
                {
                    var currentUser = _supabaseService.Auth.CurrentUser;
                    if (currentUser == null || transactionToEdit.UserId.ToString() != currentUser.Id)
                    {
                        MessageBox.Show("Ви не маєте прав на редагування цієї транзакції.", "Помилка доступу", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var tempTransaction = new Transaction
                    {
                        Id = transactionToEdit.Id,
                        UserId = transactionToEdit.UserId,
                        Type = transactionToEdit.Type,
                        Category = transactionToEdit.Category,
                        Amount = transactionToEdit.Amount,
                        Date = transactionToEdit.Date,
                        Description = transactionToEdit.Description
                    };

                    var viewModel = new TransactionViewModel(tempTransaction);
                    var transactionWindow = new Views.TransactionWindow(viewModel);

                    if (transactionWindow.ShowDialog() == true)
                    {
                        var updatedTransaction = await _supabaseService.UpdateTransaction(viewModel.Transaction);

                        if (updatedTransaction != null)
                        {
                            var index = Transactions.IndexOf(transactionToEdit);
                            if (index != -1)
                            {
                                Transactions[index] = updatedTransaction;
                                OnPropertyChanged(nameof(Transactions));
                            }
                            UpdateSummaryData();
                            await LoadTransactionsAsync();
                        }
                        else
                        {
                            MessageBox.Show("Не вдалося оновити транзакцію.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при оновленні транзакції: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteTransaction(object? parameter)
        {
            try
            {
                if (parameter is Transaction transaction)
                {
                    var currentUser = _supabaseService.Auth.CurrentUser;
                    if (currentUser == null || transaction.UserId.ToString() != currentUser.Id)
                    {
                        MessageBox.Show("Ви не маєте прав на видалення цієї транзакції.", "Помилка доступу", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var result = MessageBox.Show($"Ви впевнені, що хочете видалити транзакцію '{transaction.Description}' на суму {transaction.Amount:C} ({transaction.Type}) від {transaction.Date:dd.MM.yyyy}?", "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _supabaseService.DeleteTransaction(transaction.Id);
                        Transactions.Remove(transaction);
                        UpdateSummaryData();
                        // MessageBox.Show("Транзакція успішно видалена.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при видаленні транзакції: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 