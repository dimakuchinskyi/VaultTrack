﻿using System.Configuration;
using System.Data;
using System.Windows;
using VaultTrack.Views;
using System.Linq;
using System;

namespace VaultTrack;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public string CurrentAppLanguage { get; private set; } = "uk"; // Default language
    public event EventHandler? LanguageChanged;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Material Design theme will now be set via App.xaml

        // Додаємо глобальний обробник необроблених винятків
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;

        var loginWindow = new LoginWindow();
        Application.Current.MainWindow = loginWindow;
        loginWindow.Show();
    }

    private void App_DispatcherUnhandledException(object sender,
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        // Запобігаємо стандартній обробці винятку, яка може закрити додаток
        e.Handled = true;

        // Відображаємо інформацію про помилку
        MessageBox.Show(
            $"Виникла неочікувана помилка: {e.Exception.Message}\n\nСтек викликів:\n{e.Exception.StackTrace}",
            "Критична помилка",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        // Можливо, тут потрібно буде закрити додаток, якщо помилка критична
        // Application.Current.Shutdown();
    }

    public void ChangeLanguage(string language)
    {
        var newDictionary = new ResourceDictionary();
        string newSource = "";

        switch (language)
        {
            case "uk":
                newSource = "Resources/Strings.uk.xaml";
                break;
            case "en":
                newSource = "Resources/Strings.en.xaml";
                break;
            default:
                newSource = "Resources/Strings.uk.xaml"; // Fallback to Ukrainian
                break;
        }

        newDictionary.Source = new Uri(newSource, UriKind.Relative);

        // Find and replace the existing language dictionary
        var oldDictionary = Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("Strings.uk.xaml") || d.Source.OriginalString.Contains("Strings.en.xaml")));

        if (oldDictionary != null)
        {
            Resources.MergedDictionaries.Remove(oldDictionary);
        }

        Resources.MergedDictionaries.Add(newDictionary);
        CurrentAppLanguage = language; // Update the current language
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}

