# VaultTrack

Коротко для користувача
- VaultTrack — десктопний WPF додаток для обліку персональних фінансів: додавання транзакцій (дохід/витрата), перегляд списку транзакцій, підрахунок балансу.
- Працює з бекендом Supabase для зберігання користувачів і даних.

Швидко запустити
1) Встановіть .NET 8 SDK (Windows) з підтримкою WPF.
2) Задайте змінні оточення:
   - `SUPABASE_URL` — адреса вашого Supabase проекту
   - `SUPABASE_KEY` — публічний ключ (anon/publishable)
3) Побудуйте та запустіть проект у IDE (Visual Studio / Rider) або через термінал:

```powershell
# Збірка
dotnet build "c:\Users\dima\Desktop\WpfApplication\VaultTrack.csproj" -c Debug
# Запуск
dotnet run --project "c:\Users\dima\Desktop\WpfApplication\VaultTrack.csproj" -c Debug
```

Структура проекту (коротко)
- `App.xaml`, `App.xaml.cs` — точка запуску додатка; тут налаштовано простий DI та глобальні ресурси.
- `VaultTrack.csproj` — файл проєкту (.NET), містить залежності (NuGet) і налаштування збірки.
- `Views/` — WPF вікна і діалоги (LoginWindow, MainWindow, RegisterWindow, TransactionsWindow, TransactionWindow тощо).
- `ViewModels/` — ViewModel класи (логіка вікон): `MainViewModel`, `TransactionViewModel`, `ViewModelBase`.
- `Services/` — сервіси для взаємодії з бекендом: `SupabaseService.cs` (робота з Supabase: auth, CRUD).
- `Models/` — моделі даних: `Transaction`, `UserBalance`, `User`.
- `Converters/` — WPF конвертери (IValueConverter) для прив'язок (наприклад колір по типу транзакції).
- `Helpers/` — допоміжні класи (RelayCommand, інші утиліти).
- `Assets/` — статичні ресурси (іконки, шрифти). Іконка додатку: `Assets/Icons/app.ico`.
- `Resources/` — спільні стилі та локалізовані рядки.

Коротко про DI
- Є простий контейнер у `App.xaml.cs`: реєструється `SupabaseService.Instance` і `MainViewModel`. Це мінімальний DI для зручності створення ViewModel і доступу до сервісів.

