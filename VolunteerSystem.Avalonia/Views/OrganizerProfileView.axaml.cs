using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace VolunteerSystem.Avalonia.Views
{
    public partial class OrganizerProfileView : UserControl
    {
        public OrganizerProfileView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
