using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

            _client = new Supabase.Client(
                "https://bzbebtuuittefkahhomf.supabase.co",
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImJ6YmVidHV1aXR0ZWZrYWhob21mIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDk0ODU4MzAsImV4cCI6MjA2NTA2MTgzMH0.sR01OvPwyOWyZ626eAKdsbgLSnOeL1z5FNlMz58yQD4",
                options
            );
        }

        public Supabase.Gotrue.Interfaces.IGotrueClient<Supabase.Gotrue.User, Supabase.Gotrue.Session> Auth => _client.Auth;

        public async Task<Supabase.Gotrue.Session?> SignIn(string email, string password)
        {
            try
            {
                return await _client.Auth.SignIn(email, password);
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка входу: {ex.Message}");
            }
        }

        public async Task<Supabase.Gotrue.User?> SignUp(string email, string password)
        {
            try
            {
                var response = await _client.Auth.SignUp(email, password);
                if (response == null || response.User == null)
                {
                    throw new Exception("Не вдалося зареєструвати користувача");
                }
                return response.User;
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка реєстрації: {ex.Message}");
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
                throw new Exception($"Помилка отримання транзакцій: {ex.Message}");
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
                throw new Exception($"Помилка виходу: {ex.Message}");
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
                throw new Exception($"Помилка додавання транзакції: {ex.Message}");
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
                throw new Exception($"Помилка оновлення транзакції: {ex.Message}");
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
                throw new Exception($"Помилка видалення транзакції: {ex.Message}");
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
                throw new Exception($"Помилка скидання паролю: {ex.Message}");
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
                throw new Exception($"Помилка отримання транзакцій користувача: {ex.Message}");
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
                throw new Exception($"Помилка створення транзакції: {ex.Message}");
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
                throw new Exception($"Помилка отримання балансу користувача: {ex.Message}");
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
                throw new Exception($"Помилка оновлення балансу користувача: {ex.Message}");
            }
        }
    }
} 