using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace VolunteerSystem.Avalonia.Views
{
    public partial class ReportView : UserControl
    {
        public ReportView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
