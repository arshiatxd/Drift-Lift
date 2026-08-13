using System.Windows;
namespace DriftLift.Dialogs
{
    public partial class CloseBehaviorDialog : Window
    {
        public bool MinimizeToTray { get; private set; }
        public bool RememberChoice => RememberCheckBox.IsChecked == true;
        public CloseBehaviorDialog()
        {
            InitializeComponent();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            MinimizeToTray = true;
            DialogResult = true;
            Close();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MinimizeToTray = false;
            DialogResult = true;
            Close();
        }
    }
}
