using System.Windows;
using System.Windows.Controls;
using VaultTrack.Services;
using System.Text.RegularExpressions;

namespace VaultTrack.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly SupabaseService _supabaseService;
        private bool _isLoading;

        public LoginWindow()
        {
            InitializeComponent();
            _supabaseService = SupabaseService.Instance;
            LoginButton.Click += LoginButton_Click;
            RegisterButton.Click += RegisterButton_Click;
            
            // Add input validation
            EmailTextBox.TextChanged += ValidateInputs;
            PasswordBox.PasswordChanged += ValidateInputs;
        }

        private void ValidateInputs(object sender, RoutedEventArgs e)
        {
            bool isEmailValid = IsValidEmail(EmailTextBox.Text);
            bool isPasswordValid = !string.IsNullOrWhiteSpace(PasswordBox.Password);

            LoginButton.IsEnabled = isEmailValid && isPasswordValid;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        private void SetLoadingState(bool isLoading)
        {
            _isLoading = isLoading;
            LoginButton.IsEnabled = !isLoading;
            RegisterButton.IsEnabled = !isLoading;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }

        private void SwitchLanguage_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            string currentLanguage = app.CurrentAppLanguage;
            string newLanguage = (currentLanguage == "uk") ? "en" : "uk";
            app.ChangeLanguage(newLanguage);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;

            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            try
            {
                SetLoadingState(true);
                var session = await _supabaseService.SignIn(email, password);
                
                if (session != null)
                {
                    var mainWindow = new MainWindow();
                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Невірний email або пароль (сесія null).", "Помилка входу", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка входу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }
    }
} 