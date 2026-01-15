using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace VolunteerSystem.Avalonia.Views
{
    public partial class VolunteerProfileView : UserControl
    {
        public VolunteerProfileView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
