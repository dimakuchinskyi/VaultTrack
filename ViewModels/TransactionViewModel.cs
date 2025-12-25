using System;
using System.Windows.Input;
using VaultTrack.Models;
using VaultTrack.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;

namespace VaultTrack.ViewModels
{
    public class TransactionViewModel : ViewModelBase
    {
        private Transaction _transaction;
        public event EventHandler<bool?>? RequestClose;

        public ObservableCollection<string> AvailableCategories { get; set; }
        public ObservableCollection<TransactionTypeItem> AvailableTypes { get; set; }
        private string _currentLanguage;

        private readonly SupabaseService _supabaseService;

        public TransactionViewModel(Transaction transaction)
        {
            _transaction = transaction;
            _supabaseService = SupabaseService.Instance;
            SaveCommand = new RelayCommand(async parameter => await SaveAndClose(parameter));
            CancelCommand = new RelayCommand(parameter => CancelAndClose(parameter));

            AvailableCategories = new ObservableCollection<string>
            {
                "Їжа",
                "Транспорт",
                "Розваги",
                "Комунальні послуги",
                "Зарплата",
                "Подарунок",
                "Інше"
            };

            _currentLanguage = ((App)Application.Current).CurrentAppLanguage;
            UpdateAvailableTypes();
            UpdateAvailableCategories();
            ((App)Application.Current).LanguageChanged += (s, e) => {
                UpdateAvailableTypes();
                UpdateAvailableCategories();
            };

            // Set a default category if none is selected or if it's a new transaction
            if (string.IsNullOrEmpty(_transaction.Category) && AvailableCategories.Any())
            {
                _transaction.Category = AvailableCategories.First();
            }

            // Set a default type if none is selected or if it's a new transaction
            if (string.IsNullOrEmpty(_transaction.Type) && AvailableTypes.Any())
            {
                _transaction.Type = AvailableTypes.First().Key;
            }

            // Debugging: Check if categories are populated
            if (AvailableCategories.Any())
            {
                //MessageBox.Show($"Категорії завантажено: {string.Join(", ", AvailableCategories)}", "Налагодження категорій");
            }
            else
            {
                //MessageBox.Show("Категорії не завантажено.", "Налагодження категорій");
            }
        }

        // Properties for data binding
        public TransactionTypeItem SelectedType
        {
            get => AvailableTypes.FirstOrDefault(t => t.Key == _transaction.Type) ?? AvailableTypes.First();
            set
            {
                _transaction.Type = value.Key;
                OnPropertyChanged(nameof(SelectedType));
                OnPropertyChanged(nameof(Type));
            }
        }

        public string Type
        {
            get => _transaction.Type ?? string.Empty;
            set
            {
                _transaction.Type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        public string Category
        {
            get => _transaction.Category ?? string.Empty;
            set
            {
                _transaction.Category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        public decimal Amount
        {
            get => _transaction.Amount;
            set
            {
                _transaction.Amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        public DateTime Date
        {
            get => _transaction.Date;
            set
            {
                _transaction.Date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        public string Description
        {
            get => _transaction.Description ?? string.Empty;
            set
            {
                _transaction.Description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public Transaction Transaction => _transaction; // Expose the transaction object

        // Commands
        public ICommand? SaveCommand { get; private set; }
        public ICommand? CancelCommand { get; private set; }

        public bool? DialogResult { get; private set; }

        private async Task SaveAndClose(object? parameter)
        {
            try
            {
                if (_transaction.Id == 0) // New transaction
                {
                    // MessageBox.Show("Спроба додати нову транзакцію до Supabase.", "Відлагодження", MessageBoxButton.OK, MessageBoxImage.Information);
                    await _supabaseService.AddTransaction(Transaction);
                }
                else // Existing transaction
                {
                    await _supabaseService.UpdateTransaction(Transaction);
                }
                DialogResult = true;
                RequestClose?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження транзакції: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
            }

            // Close the MaterialDesign DialogHost
            if (parameter is string dialogHostIdentifier && MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen(dialogHostIdentifier))
            {
                MaterialDesignThemes.Wpf.DialogHost.Close(dialogHostIdentifier, true);
            }
        }

        private void CancelAndClose(object? parameter)
        {
            DialogResult = false;
            RequestClose?.Invoke(this, false);

            // Close the MaterialDesign DialogHost
            if (parameter is string dialogHostIdentifier && MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen(dialogHostIdentifier))
            {
                MaterialDesignThemes.Wpf.DialogHost.Close(dialogHostIdentifier, false);
            }
        }

        private void UpdateAvailableTypes()
        {
            string incomeKey = "Income";
            string expenseKey = "Expense";
            string incomeDisplay = (string)Application.Current.TryFindResource("TypeIncome") ?? "Дохід";
            string expenseDisplay = (string)Application.Current.TryFindResource("TypeExpense") ?? "Витрата";
            AvailableTypes = new ObservableCollection<TransactionTypeItem>
            {
                new TransactionTypeItem { Key = incomeKey, Display = incomeDisplay },
                new TransactionTypeItem { Key = expenseKey, Display = expenseDisplay }
            };
            // Встановлюємо SelectedType згідно з поточним Type
            SelectedType = AvailableTypes.FirstOrDefault(t => t.Key == Type) ?? AvailableTypes.First();
        }

        private void UpdateAvailableCategories()
        {
            string food = (string)Application.Current.TryFindResource("CategoryFood") ?? "Їжа";
            string transport = (string)Application.Current.TryFindResource("CategoryTransport") ?? "Транспорт";
            string entertainment = (string)Application.Current.TryFindResource("CategoryEntertainment") ?? "Розваги";
            string utilities = (string)Application.Current.TryFindResource("CategoryUtilities") ?? "Комунальні послуги";
            string salary = (string)Application.Current.TryFindResource("CategorySalary") ?? "Зарплата";
            string gift = (string)Application.Current.TryFindResource("CategoryGift") ?? "Подарунок";
            string other = (string)Application.Current.TryFindResource("CategoryOther") ?? "Інше";
            AvailableCategories = new ObservableCollection<string> { food, transport, entertainment, utilities, salary, gift, other };
            OnPropertyChanged(nameof(AvailableCategories));
            // Якщо поточна категорія не співпадає з новим списком, оновити
            if (!AvailableCategories.Contains(Category))
                Category = food;
        }
    }

    public class TransactionTypeItem
    {
        public string Key { get; set; } // "Income" або "Expense"
        public string Display { get; set; } // "Дохід" або "Витрата"
        public override string ToString() => Display;
    }
} 