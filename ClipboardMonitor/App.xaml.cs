﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Hardcodet.Wpf.TaskbarNotification;

namespace ClipboardMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        private TaskbarIcon? _notifyIcon;
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

            // Configure PAN search configuration. You can add new card types by following the same steps
            PANData.Instance.AddPaymentBrand(new Mastercard())
                .AddPaymentBrand(new Visa())
                .AddPaymentBrand(new Amex());

            _notification = new ClipboardNotification("REDACTED");

            // Create the notify icon (it's a resource declared in NotifyIconResources.xaml
            // Finally, show the icon
            _notifyIcon = FindTaskbarIcon();
        }

        private static void HandleArguments()
        {
            var args = Environment.GetCommandLineArgs();

            if (IsArgumentCountInvalid(args))
            {
                throw new ArgumentException($"Invalid arguments.{args[2]}");
            }

            if (IsNormalStart(args))
            {
                return;
            }

            if (IsInstallCommand(args))
            {
                try
                {
                    if (Logger.Instance.Check())
                    {
                        const string message = "ClipboardMonitor is already installed.";
                        MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information,
                            MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    else
                    {
                        Logger.Instance.Install(); // Creates the event log source

                        const string message = "Installation completed.";
                        MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information,
                            MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
                catch (SecurityException ex)
                {
                    Debug.WriteLine(ex.Message);
                    const string message = "You need to run as administrator first to install the application.";
                    _ = MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                        MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                finally
                {
                    Environment.Exit(0);
                }
            }
            else if (IsUninstallCommand(args))
            {
                try
                {
                    Logger.Instance.Uninstall(); // Creates the event log source

                    const string message = "Uninstallation completed.";
                    MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information,
                        MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                catch (Win32Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    const string message = "You need to run as administrator first to uninstall the application.";
                    _ = MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error,
                        MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                finally
                {
                    Environment.Exit(0);
                }
            }
            else if (IsHelpCommand(args))
            {
                const string message =
                    "USAGE: ClipboardMonitor [ARGUMENTS]\n\n-i,/i,--install\tInstalls the application (Needs Admin rights).\n-u,/u,--uninstall\tInstalls the application (Needs Admin rights).\n-?, -h, /h, --help\tDisplays this message box.";
                _ = MessageBox.Show(message, "Help", MessageBoxButton.OK, MessageBoxImage.Information,
                    MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);

                Environment.Exit(0);
            }
            else
            {
                throw new ArgumentException($"Invalid arguments.{args[2]}");
            }
        }

        private static bool IsNormalStart(string[] args) => args.Length == 1;

        private static bool IsHelpCommand(string[] args) =>
            args is { Length: 2 } && (args[1].Equals("-?", StringComparison.Ordinal) ||
                                    args[1].Equals("-h", StringComparison.Ordinal) ||
                                    args[1].Equals("/h", StringComparison.Ordinal) ||
                                    args[1].Equals("--help", StringComparison.Ordinal));

        private static bool IsUninstallCommand(string[] args) =>
            args is { Length: 2 } && (args[1].Equals("-u", StringComparison.Ordinal) ||
                                    args[1].Equals("/u", StringComparison.Ordinal) ||
                                    args[1].Equals("--uninstall", StringComparison.Ordinal));

        private static bool IsInstallCommand(string[] args) =>
            args is { Length: 2 } && (args[1].Equals("-i", StringComparison.Ordinal) ||
                                    args[1].Equals("/i", StringComparison.Ordinal) ||
                                    args[1].Equals("--install", StringComparison.Ordinal));

        private static bool IsArgumentCountInvalid(string[] args) => args is { Length: > 2 };

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
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
                message = $"Unhandled exception in { (object?)assemblyName.Name} v{ (object?)assemblyName.Version}";
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"Exception in LogUnhandledException.\n{ex}", 2);
            }
            finally
            {
                Logger.Instance.LogError($"{message}\n{exception}", 3);
                //ProcessHelper.UnsetCriticalProcess();
                Environment.Exit(1); // We don't want this to freeze
            }
        }

        private TaskbarIcon? FindTaskbarIcon()
        {
            TaskbarIcon? icon;

            try
            {
                icon = (TaskbarIcon?)FindResource("NotifyIcon");
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                return null;
            }

            return icon;
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            Logger.Instance.LogInfo("ClipboardMonitor is shutting down.", 11);
            //ProcessHelper.UnsetCriticalProcess();
            base.OnExit(e);
        }

        private void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            e.Cancel = true;
            //ProcessHelper.UnsetCriticalProcess();
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
                _notifyIcon?.Dispose(); //the icon would clean up automatically, but this is cleaner
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