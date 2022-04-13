using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ClipboardMonitor
{
    /// <summary>
    /// <para>
    ///     Provides bindable properties and commands for the NotifyIcon. In this sample, the
    ///     view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    ///     in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </para>
    /// <para><see href="https://github.com/hardcodet/wpf-notifyicon"/></para>
    ///  </summary>
    public class NotifyIconViewModel
    {
        /// <summary>
        ///     Shows a window, if none is already open.
        /// </summary>
#pragma warning disable CA1822 // Mark members as static
        public ICommand AboutCommand => new DelegateCommand
        {
            CommandAction = () => new AboutWindow().Show(),
            CanExecuteFunc = () => !GetChildWindows()
                    .Any(
                aboutWindow => aboutWindow.GetType() == typeof(AboutWindow))
        };
#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        /// <para>This is required to get rid of the instances of Visual Studio debugging tool for XAML windows,
        /// <see cref="Microsoft.XamlDiagnostics.WpfTap.WpfVisualTreeService.Adorners.AdornerLayerWindow"/>.
        /// Without this, there will be issues with windows handling.
        /// </para>
        /// <para><see href="https://stackoverflow.com/questions/46416123/how-to-properly-ignore-windows-created-by-visual-studio-debugging-tool-for-xaml"/></para>
        /// </summary>
        /// <returns> Returns only the child windows of this application. </returns>
        private static List<Window> GetChildWindows() => Application.Current.Windows.Cast<Window>().Where(w => w.ActualWidth != 0).ToList();
    }
}
