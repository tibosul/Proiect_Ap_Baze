using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
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
            // DI Setup
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=localhost;Database=VolunteerSystem;User Id=sa;Password=Andra1104!;TrustServerCertificate=True;")
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