using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using ClipboardMonitor.Helpers;

namespace ClipboardMonitor
{
    public partial class App : Application, IDisposable
    {
        private static readonly HashSet<string> InstallArgs = new(StringComparer.Ordinal) { "-i", "/i", "--install" };
        private static readonly HashSet<string> UninstallArgs = new(StringComparer.Ordinal) { "-u", "/u", "--uninstall" };
        private static readonly HashSet<string> HelpArgs = new(StringComparer.Ordinal) { "-?", "-h", "/h", "--help" };

        private ClipboardNotification? _notification;
        private bool _disposedValue;

        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            HandleArguments();

            if (!Logger.Instance.Check())
            {
                const string message = "You need to run as administrator with argument '-i' to install the application.";
                _ = MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                Environment.Exit(0);
            }

            if (ProcessHelper.IsDuplicate())
            {
                Logger.Instance.LogInfo("Killing redundant ClipboardMonitor instance.", 15);
                Environment.Exit(0);
            }

            ProcessHelper.Cover();

            base.OnStartup(e);

            SetupExceptionHandling();

            Logger.Instance.LogInfo("Started a new ClipboardMonitor instance.", 10);
            AlertHandler.Instance.SubstituteText = "REDACTED";

            _notification = new ClipboardNotification();
            PasteGuard.Initialize();
        }

        private static void HandleArguments()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 2)
            {
                throw new ArgumentException($"Invalid arguments: {string.Join(" ", args)}");
            }

            if (args.Length == 1)
            {
                return;
            }

            var command = args[1];
            if (InstallArgs.Contains(command))
            {
                HandleInstall();
            }
            else if (UninstallArgs.Contains(command))
            {
                HandleUninstall();
            }
            else if (HelpArgs.Contains(command))
            {
                ShowHelp();
            }
            else
            {
                throw new ArgumentException($"Invalid arguments: {string.Join(" ", args)}");
            }
        }

        private static void HandleInstall()
        {
            try
            {
                if (Logger.Instance.Check())
                {
                    MessageBox.Show("ClipboardMonitor is already installed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information,
                        MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    Logger.Instance.Install();
                    MessageBox.Show("Installation completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information,
                        MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            catch (SecurityException ex)
            {
                Debug.WriteLine(ex.Message);
                _ = MessageBox.Show("You need to run as administrator first to install the application.", "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        private static void HandleUninstall()
        {
            try
            {
                Logger.Instance.Uninstall();
                MessageBox.Show("Uninstallation completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information,
                    MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            catch (Win32Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _ = MessageBox.Show("You need to run as administrator first to uninstall the application.", "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        private static void ShowHelp()
        {
            const string message =
                "USAGE: ClipboardMonitor [ARGUMENTS]\n\n-i,/i,--install\tInstalls the application (Needs Admin rights).\n-u,/u,--uninstall\tInstalls the application (Needs Admin rights).\n-?, -h, /h, --help\tDisplays this message box.";
            _ = MessageBox.Show(message, "Help", MessageBoxButton.OK, MessageBoxImage.Information,
                MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
            Environment.Exit(0);
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) => {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) => {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private static void LogUnhandledException(Exception exception, string source)
        {
            var message = $"Unhandled exception ({source})";
            try
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName();
                message = $"Unhandled exception in {assemblyName.Name} v{assemblyName.Version}";
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"Exception in LogUnhandledException.\n{ex}", 2);
            }
            finally
            {
                Logger.Instance.LogError($"{message}\n{exception}", 3);
                Environment.Exit(1);
            }
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            Logger.Instance.LogInfo("ClipboardMonitor is shutting down.", 11);
            base.OnExit(e);
        }

        private void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            e.Cancel = true;
            e.Cancel = false;
            base.OnSessionEnding(e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                ProcessHelper.Uncover();
                _notification?.Dispose();
                PasteGuard.Dispose();
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
