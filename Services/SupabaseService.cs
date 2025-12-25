using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Supabase;
using Supabase.Gotrue;
using VaultTrack.Models;
using System.Collections.ObjectModel;

namespace VaultTrack.Services
{
    public class SupabaseService
    {
        private static SupabaseService? _instance;
        private readonly Supabase.Client _client;
        // подтверждение email удалено — никаких service_role полей не требуется
        private bool _initialized = false;

        public static SupabaseService Instance
        {
            get
            {
                _instance ??= new SupabaseService();
                return _instance;
            }
        }

        private SupabaseService()
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            // Читаем URL и KEY из переменных окружения, если они заданы (удобно для конфигурации и тестов).
            var url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? "https://ybeibesxmomvkhsatjog.supabase.co";
            var key = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? "sb_publishable_ug-Qift0PiJaDCloP_spGg_ZqlrfsVE";

            // Ранее здесь читался service_role ключ для авто-подтверждения — теперь эта опция удалена

            _client = new Supabase.Client(
                url,
                key,
                options
            );
        }

        public Supabase.Gotrue.Interfaces.IGotrueClient<Supabase.Gotrue.User, Supabase.Gotrue.Session> Auth => _client.Auth;

        // Новый метод для явной инициализации клиента и получения подробной ошибки, если хост не доступен
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                await _client.InitializeAsync();
                _initialized = true;
            }
            catch (Exception ex)
            {
                // Передаём полную информацию об ошибке дальше (включая inner exception)
                throw new Exception($"Помилка ініціалізації Supabase: {ex.Message}", ex);
            }
        }

        public async Task<Supabase.Gotrue.Session?> SignIn(string email, string password)
        {
            try
            {
                email = CleanEmail(email);
                await InitializeAsync();
                return await _client.Auth.SignIn(email, password);
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка входу: {ex.Message}", ex);
            }
        }

        public async Task<Supabase.Gotrue.User?> SignUp(string email, string password)
        {
            try
            {
                email = CleanEmail(email);
                await InitializeAsync();

                var response = await _client.Auth.SignUp(email, password);
                if (response == null || response.User == null)
                {
                    throw new Exception("Не вдалося зареєструвати користувача");
                }
                return response.User;
            }
            catch (Exception ex)
            {
                // Возвращаем полные детали исключения (включая тело ответа Gotrue), чтобы видеть код/сообщение от сервера
                throw new Exception($"Помилка реєстрації: {ex.Message}\n\nДеталі: {ex.ToString()}", ex);
            }
        }

        public async Task<List<Models.Transaction>> GetTransactions()
        {
            try
            {
                var response = await _client.From<Models.Transaction>().Get();
                return response.Models;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка отримання транзакцій: {ex.Message}", ex);
            }
        }

        public async Task SignOut()
        {
            try
            {
                await _client.Auth.SignOut();
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка виходу: {ex.Message}", ex);
            }
        }

        public async Task<Transaction?> AddTransaction(Transaction transaction)
        {
            try
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException(nameof(transaction));
                }

                var response = await _client.From<Models.Transaction>().Insert(transaction);
                if (response == null || response.Model == null)
                {
                    throw new Exception("Не вдалося додати транзакцію");
                }
                return response.Model;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка додавання транзакції: {ex.Message}", ex);
            }
        }

        public async Task<Transaction?> UpdateTransaction(Transaction transaction)
        {
            try
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException(nameof(transaction));
                }

                var response = await _client.From<Models.Transaction>()
                    .Where(t => t.Id == transaction.Id)
                    .Update(transaction);

                if (response == null || response.Model == null)
                {
                    throw new Exception("Не вдалося оновити транзакцію");
                }
                return response.Model;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка оновлення транзакції: {ex.Message}", ex);
            }
        }

        public async Task DeleteTransaction(int transactionId)
        {
            try
            {
                await _client.From<Models.Transaction>()
                    .Where(t => t.Id == transactionId)
                    .Delete();
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка видалення транзакції: {ex.Message}", ex);
            }
        }

        public async Task<bool> ResetPassword(string email)
        {
            try
            {
                await _client.Auth.ResetPasswordForEmail(email);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка скидання паролю: {ex.Message}", ex);
            }
        }

        public async Task<ObservableCollection<Transaction>> GetTransactions(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("ID користувача не може бути порожнім", nameof(userId));
                }

                Guid userGuid;
                if (!Guid.TryParse(userId, out userGuid))
                {
                    throw new ArgumentException("Некоректний формат ID користувача", nameof(userId));
                }

                var response = await _client
                    .From<Transaction>()
                    .Where(x => x.UserId == userGuid)
                    .Order(x => x.Date, Postgrest.Constants.Ordering.Descending)
                    .Get();

                return new ObservableCollection<Transaction>(response.Models);
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка отримання транзакцій користувача: {ex.Message}", ex);
            }
        }

        public async Task CreateTransaction(Transaction transaction)
        {
            try
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException(nameof(transaction));
                }

                await _client
                    .From<Transaction>()
                    .Insert(transaction);
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка створення транзакції: {ex.Message}", ex);
            }
        }

        public async Task<UserBalance?> GetUserBalance(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("ID користувача не може бути порожнім", nameof(userId));
                }

                var response = await _client
                    .From<UserBalance>()
                    .Where(x => x.UserId.ToString() == userId)
                    .Single();

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка отримання балансу користувача: {ex.Message}", ex);
            }
        }

        public async Task UpsertUserBalance(UserBalance userBalance)
        {
            try
            {
                if (userBalance == null)
                {
                    throw new ArgumentNullException(nameof(userBalance));
                }

                await _client
                    .From<UserBalance>()
                    .Upsert(userBalance);
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка оновлення балансу користувача: {ex.Message}", ex);
            }
        }

        // Удаляет невидимые и управляющие символы из email, нормализует форму Unicode
        private static string CleanEmail(string? email)
        {
            if (string.IsNullOrEmpty(email)) return string.Empty;
            // Normalize to a composed form
            var normalized = email.Normalize(NormalizationForm.FormC);

            // Remove control chars, format chars (like zero-width space), and all whitespace
            var chars = normalized.Where(c =>
                !char.IsControl(c) &&
                !char.IsWhiteSpace(c) &&
                UnicodeCategory.Format != CharUnicodeInfo.GetUnicodeCategory(c)
            ).ToArray();

            var result = new string(chars);

            // Trim common surrounding characters that users sometimes paste: quotes, angle brackets, BOM
            result = result.Trim().Trim('"', '\'', '<', '>', '\uFEFF');

            // Normalize case for comparison / server submission (Supabase treats emails case-insensitive)
            result = result.ToLowerInvariant();

            return result;
        }
    }
}
