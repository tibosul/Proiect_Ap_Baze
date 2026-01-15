using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace VolunteerSystem.Avalonia.Views
{
    public partial class LeaderboardView : UserControl
    {
        public LeaderboardView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
