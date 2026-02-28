using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComputerClub.Infrastructure.Entities;
using ComputerClub.Views;
using Microsoft.AspNetCore.Identity;
using Wpf.Ui.Controls;

namespace ComputerClub.ViewModels;

public partial class LoginWindowViewModel(
    UserManager<ComputerClubIdentity> userManager,
    MainWindow mainWindow
) : ObservableObject, IDataErrorInfo
{
    private bool _isInitialized;
    
    [ObservableProperty] private string _login = "root"; // TODO
    
    [ObservableProperty] private string _password = "Qwerty123!"; // TODO

    [ObservableProperty] private string _errorMessage = string.Empty;

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private bool _hasLoginError;

    [ObservableProperty] private bool _hasPasswordError;

    [ObservableProperty] private string _loginError = string.Empty;

    [ObservableProperty] private string _passwordError = string.Empty;

    public string Error => string.Empty;

    private FluentWindow? _window;
    
    [RelayCommand]
    private void Loaded(FluentWindow window)
    {
        _window = window;
        _isInitialized = true;
    }

    public string this[string columnName]
    {
        get
        {
            if (!_isInitialized) return string.Empty;
            
            switch (columnName)
            {
                case nameof(Login):
                    if (string.IsNullOrWhiteSpace(Login))
                    {
                        HasLoginError = true;
                        return LoginError = "Введите логин";
                    }

                    if (Login.Length < 3)
                    {
                        HasLoginError = true;
                        return LoginError = "Логин должен содержать минимум 3 символа";
                    }

                    HasLoginError = false;
                    LoginError = string.Empty;
                    break;
            }

            return string.Empty;
        }
    }

    public bool ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            HasPasswordError = true;
            PasswordError = "Введите пароль";
            return false;
        }

        if (password.Length < 3)
        {
            HasPasswordError = true;
            PasswordError = "Пароль должен содержать минимум 3 символа";
            return false;
        }

        HasPasswordError = false;
        PasswordError = string.Empty;
        return true;
    }

    [RelayCommand]
    private async Task LoginAsync(object parameter)
    {
        if (parameter is not PasswordBox passwordBox)
        {
            ErrorMessage = "Ошибка компонента пароля";
            return;
        }

        var password = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(Login))
        {
            HasLoginError = true;
            LoginError = "Введите логин";
            return;
        }

        if (!ValidatePassword(password))
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var user = await userManager.FindByNameAsync(Login);
            if (user is null)
            {
                ErrorMessage = "Неверный логин или пароль";
                return;
            }
            
            var result = await userManager.CheckPasswordAsync(user, password);
            if (!result)
            {
                ErrorMessage = "Неверный логин или пароль";
                return;
            }
            
            var role = await userManager.GetRolesAsync(user);
            
            App.CurrentUser = user;
            App.CurrentRole = role[0];

            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            
            _window?.Close();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}