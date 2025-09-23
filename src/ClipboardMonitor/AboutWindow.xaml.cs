using System.Windows;

namespace ClipboardMonitor
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            OKButton.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Close();
    }
}
