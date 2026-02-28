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
    private FluentWindow? _window;

    [ObservableProperty] private bool _isLoginMode = true;
    [ObservableProperty] private bool _isLoading;

    public string Error => string.Empty;

    [RelayCommand]
    private void Loaded(FluentWindow window)
    {
        _window = window;
        _isInitialized = true;
    }

    private void NavigateToMain(ComputerClubIdentity user, string role)
    {
        App.CurrentUser = user;
        App.CurrentRole = role;
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
        _window?.Close();
    }

    #region Вход

    [ObservableProperty] private string _login = "root"; // TODO
    [ObservableProperty] private string _password = "Qwerty123!"; // TODO
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasLoginError;
    [ObservableProperty] private bool _hasPasswordError;
    [ObservableProperty] private string _loginError = string.Empty;
    [ObservableProperty] private string _passwordError = string.Empty;

    [RelayCommand]
    private void ShowLogin()
    {
        IsLoginMode = true;
        ErrorMessage = string.Empty;
    }

    public string this[string columnName]
    {
        get
        {
            if (!_isInitialized || columnName != nameof(Login)) return string.Empty;

            if (string.IsNullOrWhiteSpace(Login))
            {
                HasLoginError = true;
                return LoginError = "Введите логин";
            }

            if (Login.Length < 3)
            {
                HasLoginError = true;
                return LoginError = "Минимум 3 символа";
            }

            HasLoginError = false;
            LoginError = string.Empty;

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
            PasswordError = "Минимум 3 символа";
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
            return;
        }

        if (string.IsNullOrWhiteSpace(Login))
        {
            HasLoginError = true;
            LoginError = "Введите логин";
            return;
        }

        if (!ValidatePassword(passwordBox.Password)) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var user = await userManager.FindByNameAsync(Login);
            if (user is null || !await userManager.CheckPasswordAsync(user, passwordBox.Password))
            {
                ErrorMessage = "Неверный логин или пароль";
                return;
            }

            var roles = await userManager.GetRolesAsync(user);
            NavigateToMain(user, roles[0]);
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

    #endregion

    #region Регистрация

    [ObservableProperty] private string _regLogin = string.Empty;
    [ObservableProperty] private string _regFullName = string.Empty;
    [ObservableProperty] private string _regPhone = string.Empty;
    [ObservableProperty] private string _regErrorMessage = string.Empty;
    [ObservableProperty] private bool _hasRegLoginError;
    [ObservableProperty] private bool _hasRegFullNameError;
    [ObservableProperty] private bool _hasRegPhoneError;
    [ObservableProperty] private bool _hasRegPasswordError;
    [ObservableProperty] private string _regLoginError = string.Empty;
    [ObservableProperty] private string _regFullNameError = string.Empty;
    [ObservableProperty] private string _regPhoneError = string.Empty;
    [ObservableProperty] private string _regPasswordError = string.Empty;

    [RelayCommand]
    private void ShowRegister()
    {
        IsLoginMode = false;
        RegLogin = string.Empty;
        RegFullName = string.Empty;
        RegPhone = string.Empty;
        RegErrorMessage = string.Empty;
        HasRegLoginError = false;
        HasRegFullNameError = false;
        HasRegPhoneError = false;
        HasRegPasswordError = false;
    }

    public void ValidateRegPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            HasRegPasswordError = true;
            RegPasswordError = "Введите пароль";
            return;
        }

        if (password.Length < 6)
        {
            HasRegPasswordError = true;
            RegPasswordError = "Минимум 6 символов";
            return;
        }

        HasRegPasswordError = false;
        RegPasswordError = string.Empty;
    }

    private bool ValidateRegPasswordBool(string password)
    {
        ValidateRegPassword(password);
        return !HasRegPasswordError;
    }

    [RelayCommand]
    private async Task Register(object parameter)
    {
        if (parameter is not PasswordBox passwordBox) return;

        if (string.IsNullOrWhiteSpace(RegLogin) || RegLogin.Length < 3)
        {
            HasRegLoginError = true;
            RegLoginError = "Минимум 3 символа";
            return;
        }

        if (string.IsNullOrWhiteSpace(RegFullName))
        {
            HasRegFullNameError = true;
            RegFullNameError = "Введите ФИО";
            return;
        }

        if (string.IsNullOrWhiteSpace(RegPhone))
        {
            HasRegPhoneError = true;
            RegPhoneError = "Введите телефон";
            return;
        }

        if (!ValidateRegPasswordBool(passwordBox.Password)) return;

        try
        {
            IsLoading = true;
            RegErrorMessage = string.Empty;

            var existing = await userManager.FindByNameAsync(RegLogin);
            if (existing is not null)
            {
                RegErrorMessage = "Пользователь с таким логином уже существует";
                return;
            }

            var user = new ComputerClubIdentity
            {
                UserName = RegLogin,
                FullName = RegFullName.Trim(),
                PhoneNumber = RegPhone.Trim()
            };

            var result = await userManager.CreateAsync(user, passwordBox.Password);
            if (!result.Succeeded)
            {
                RegErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                return;
            }

            await userManager.AddToRoleAsync(user, "User");
            NavigateToMain(user, "User");
        }
        catch (Exception ex)
        {
            RegErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
}