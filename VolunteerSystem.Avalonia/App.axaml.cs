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
using VolunteerSystem.Core.Interfaces; // Ensure this is present if needed by App

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
            if (!string.IsNullOrEmpty(saPassword) && connectionString != null && connectionString.Contains("${SA_PASSWORD}"))
            {
                connectionString = connectionString.Replace("${SA_PASSWORD}", saPassword);
                Console.WriteLine("[App] SA_PASSWORD found and substituted.");
            }
            else
            {
                Console.WriteLine($"[App] SA_PASSWORD env var is '{saPassword}'. ConnectionString contains substitution? {connectionString?.Contains("${SA_PASSWORD}")}");
            }
            
            Console.WriteLine($"[App] ConnectionString (redacted): {connectionString?.Replace(saPassword ?? "SA_PASSWORD", "***")}");

            // DI Setup
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null))
                .Options;
            var dbContext = new ApplicationDbContext(options);

            // AUTO-MIGRATION / FIX SCHEMA
            try
            {
                // 1. Add IsPresent
                dbContext.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = 'IsPresent' AND Object_ID = Object_ID('Applications'))
                    BEGIN
                        ALTER TABLE dbo.Applications ADD IsPresent BIT NOT NULL DEFAULT 0;
                        PRINT 'Added IsPresent column';
                    END
                ");

                // 2. Fix Feedback Date -> CreatedAt
                dbContext.Database.ExecuteSqlRaw(@"
                    IF EXISTS(SELECT * FROM sys.columns WHERE Name = 'Date' AND Object_ID = Object_ID('Feedbacks'))
                    BEGIN
                        EXEC sp_rename 'dbo.Feedbacks.Date', 'CreatedAt', 'COLUMN';
                        PRINT 'Renamed Date to CreatedAt';
                    END
                    ELSE IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = 'CreatedAt' AND Object_ID = Object_ID('Feedbacks'))
                    BEGIN
                        ALTER TABLE dbo.Feedbacks ADD CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME();
                        PRINT 'Added CreatedAt column';
                    END
                ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SchemaFix] Error: {ex.Message}");
            }
            
            var authService = new AuthenticationService(dbContext);
            var userService = new UserService(dbContext);
            var pointsService = new PointsService(dbContext); // New service
            var opportunityService = new OpportunityService(dbContext, pointsService);
            var chatService = new ChatService(dbContext);
            var reportService = new ReportService(dbContext);
            
            var mainViewModel = new MainViewModel();
            var loginViewModel = new LoginViewModel(authService, userService, opportunityService, chatService, pointsService, reportService, mainViewModel);

            mainViewModel.CurrentView = loginViewModel;

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}