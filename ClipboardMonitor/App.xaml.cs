using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;

[assembly: CLSCompliant(true)]
namespace ClipboardMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public partial class App : Application
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    {
        private TaskbarIcon? notifyIcon;
        private ClipboardNotification? notification;

        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();

            if (args != null && args.Length > 2)
            {
                throw new ArgumentException($"Invalid arguments.{args[2]}");
            }

            if (args != null && args.Length == 2)
            {
                if (args[1].Equals("-i", StringComparison.Ordinal) || args[1].Equals("/i", StringComparison.Ordinal) || args[1].Equals("--install", StringComparison.Ordinal))
                {
#pragma warning disable CA1031 // Do not catch general exception types
                    try
                    {
                        if (Logger.Instance.Check())
                        {
                            const string message = "ClipboardMonitor is already installed.";
                            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        }
                        else
                        {
                            Logger.Instance.Install(); // Creates the event log source

                            const string message = "Installation completed.";
                            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        }
                    }
                    catch
                    {
                        const string message = "You need to run as administrator first to install the application.";
                        _ = MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    finally
                    {
                        Environment.Exit(0);
                    }
                }
                else if (args[1].Equals("-u", StringComparison.Ordinal) || args[1].Equals("/u", StringComparison.Ordinal) || args[1].Equals("--uninstall", StringComparison.Ordinal))
                {
                    try
                    {
                        Logger.Instance.Uninstall(); // Creates the event log source

                        const string message = "Uninstallation completed.";
                        MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    catch
                    {
                        const string message = "You need to run as administrator first to uninstall the application.";
                        _ = MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    finally
                    {
                        Environment.Exit(0);
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                }
                else if (args[1].Equals("-?", StringComparison.Ordinal) || args[1].Equals("-h", StringComparison.Ordinal) || args[1].Equals("/h", StringComparison.Ordinal) || args[1].Equals("--help", StringComparison.Ordinal))
                {
                    const string message =
                        "USAGE: ClipboardMonitor [ARGUMENTS]\n\n-i,/i,--install\tInstalls the application (Needs Admin rights).\n-u,/u,--uninstall\tInstalls the application (Needs Admin rights).\n-?, -h, /h, --help\tDisplays this message box.";
                    _ = MessageBox.Show(message, "Help", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    throw new ArgumentException($"Invalid arguments.{args[2]}");
                }
            }
            if (!Logger.Instance.Check())
            {
                const string message = "You need to run as administrator with argument '-i' to install the application.";
                _ = MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                Environment.Exit(0);
            }

            base.OnStartup(e);

            //create the notify icon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            Logger.Instance.LogInfo($"Started a new ClipboardMonitor instance.", 10);
            notification = new ClipboardNotification("REDACTED");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Instance.LogInfo($"ClipboardMonitor us shutting down.", 11);
            notification?.Dispose();
            notifyIcon?.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}