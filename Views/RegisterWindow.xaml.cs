using System;
using System.Windows;
using System.Windows.Controls;
using VaultTrack.Services;
using System.Text.RegularExpressions;
using VaultTrack.Views;

namespace VaultTrack.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly SupabaseService _supabaseService;
        private bool _isLoading;

        public RegisterWindow()
        {
            InitializeComponent();
            _supabaseService = SupabaseService.Instance;
            
            // Add input validation
            EmailTextBox.TextChanged += ValidateInputs;
            PasswordBox.PasswordChanged += ValidateInputs;
            ConfirmPasswordBox.PasswordChanged += ValidateInputs;
        }

        private void ValidateInputs(object sender, RoutedEventArgs e)
        {
            bool isEmailValid = IsValidEmail(EmailTextBox.Text);
            bool isPasswordValid = !string.IsNullOrWhiteSpace(PasswordBox.Password) && PasswordBox.Password.Length >= 6;
            bool doPasswordsMatch = PasswordBox.Password == ConfirmPasswordBox.Password;

            RegisterButton.IsEnabled = isEmailValid && isPasswordValid && doPasswordsMatch;
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
            RegisterButton.IsEnabled = !isLoading;
            BackToLoginButton.IsEnabled = !isLoading;
            RegisterProgressBar.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;

            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Нормализуем email: убираем пробелы и приводим к нижнему регистру
            email = (email ?? string.Empty).Trim().ToLowerInvariant();

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Будь ласка, введіть коректну email адресу.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                MessageBox.Show("Пароль повинен містити мінімум 6 символів.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Паролі не співпадають.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SetLoadingState(true);
                var user = await _supabaseService.SignUp(email, password);
                
                if (user != null)
                {
                    var dialog = new SuccessDialog("Реєстрація успішна! Тепер ви можете увійти в систему.");
                    dialog.ShowDialog();
                    BackToLoginButton_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("Помилка реєстрації. Можливо, користувач вже існує.", 
                                  "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                // В режиме разработки показываем полный стек и inner исключения для диагностики
                MessageBox.Show($"Помилка реєстрації: {ex.ToString()}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
#else
                MessageBox.Show($"Помилка реєстрації: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void SwitchLanguage_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            string currentLanguage = app.CurrentAppLanguage;
            string newLanguage = (currentLanguage == "uk") ? "en" : "uk";
            app.ChangeLanguage(newLanguage);
        }
    }
}
