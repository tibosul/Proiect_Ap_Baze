using System.Windows;
using Microsoft.EntityFrameworkCore;
using VolunteerSystem.Data;
using VolunteerSystem.Data.Services;
using VolunteerSystem.UI.ViewModels;

namespace VolunteerSystem.UI
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        private ApplicationDbContext _dbContext;

        public App()
        {
            // Manual DI setup
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                // TODO: Implement configuration for WPF
                // .UseSqlServer("Server=localhost;Database=VolunteerSystem;User Id=sa;Password=REDACTED;TrustServerCertificate=True;")
                .UseSqlServer("Server=localhost;Database=VolunteerSystem;User Id=sa;Password=[PLACEHOLDER];TrustServerCertificate=True;")
                .Options;
            
            _dbContext = new ApplicationDbContext(options);
            _dbContext.Database.EnsureCreated(); // Auto-create DB without migrations for demo

            var authService = new AuthenticationService(_dbContext);
            var opportunityService = new OpportunityService(_dbContext);
            var mainViewModel = new MainViewModel();
            
            // Set initial view
            mainViewModel.CurrentView = new LoginViewModel(authService, opportunityService, mainViewModel);

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _dbContext?.Dispose();
            base.OnExit(e);
        }
    }
}
