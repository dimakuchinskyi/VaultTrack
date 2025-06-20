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

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _supabaseService.SignOut();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Помилка виходу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            var loginWindow = new VaultTrack.Views.LoginWindow();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            this.Close();
        }

        private void AddTransactionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel viewModel)
            {
                if (viewModel.AddTransactionCommand.CanExecute(null))
                    viewModel.AddTransactionCommand.Execute(null);
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (MenuButton.ContextMenu != null)
            {
                MenuButton.ContextMenu.PlacementTarget = MenuButton;
                MenuButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                MenuButton.ContextMenu.IsOpen = true;
            }
        }
    }
}