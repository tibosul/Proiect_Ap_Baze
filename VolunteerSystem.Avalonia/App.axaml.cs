using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using VolunteerSystem.Avalonia.ViewModels;
using VolunteerSystem.Avalonia.Views;
using VolunteerSystem.Data;
using VolunteerSystem.Data.Services;

namespace VolunteerSystem.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Build Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            // Simple interpolation for SA_PASSWORD if present
            var saPassword = Environment.GetEnvironmentVariable("SA_PASSWORD");
            if (!string.IsNullOrEmpty(saPassword) && connectionString.Contains("${SA_PASSWORD}"))
            {
                connectionString = connectionString.Replace("${SA_PASSWORD}", saPassword);
            }

            // DI Setup
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;
            var dbContext = new ApplicationDbContext(options);
            
            var authService = new AuthenticationService(dbContext);
            var opportunityService = new OpportunityService(dbContext);
            var mainViewModel = new MainViewModel();
            var loginViewModel = new LoginViewModel(authService, opportunityService, mainViewModel);

            mainViewModel.CurrentView = loginViewModel;

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}