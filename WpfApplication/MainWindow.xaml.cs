using System.Windows;
using VaultTrack.Services;
using System.Linq;

namespace VaultTrack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SupabaseService _supabaseService;

        public MainWindow()
        {
            InitializeComponent();
            _supabaseService = SupabaseService.Instance;
            this.Loaded += MainWindow_Loaded;
            this.DataContext = new ViewModels.MainViewModel();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel viewModel)
            {
                try
                {
                    await viewModel.OnWindowLoadedAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при завантаженні головної сторінки: {ex.Message}", "Критична помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown(); // Закрити додаток, якщо виникла критична помилка
                }
            }
        }
        private void SwitchLanguage_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;

            // Get the current language from the App's property
            string currentLanguage = app.CurrentAppLanguage;

            // Toggle language
            string newLanguage = (currentLanguage == "uk") ? "en" : "uk";
            app.ChangeLanguage(newLanguage);
        }

        private void EditTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel viewModel)
            {
                var button = (System.Windows.Controls.Button)sender;
                var transaction = (Models.Transaction)button.DataContext;
                viewModel.EditTransactionCommand.Execute(transaction);
            }
        }

        private void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel viewModel)
            {
                var button = (System.Windows.Controls.Button)sender;
                var transaction = (Models.Transaction)button.DataContext;
                viewModel.DeleteTransactionCommand.Execute(transaction);
            }
        }
    }
}