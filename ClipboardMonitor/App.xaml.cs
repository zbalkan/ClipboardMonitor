﻿using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

[assembly: CLSCompliant(true)]
namespace ClipboardMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        private TaskbarIcon? notifyIcon;
        private ClipboardNotification? notification;
        private bool disposedValue;

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
                    catch (System.Security.SecurityException ex)
                    {
                        Debug.WriteLine(ex.Message);
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
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        const string message = "You need to run as administrator first to uninstall the application.";
                        _ = MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    finally
                    {
                        Environment.Exit(0);
                    }
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

            if (ProcessHelper.IsDuplicate())
            {
                Logger.Instance.LogInfo($"Killing redundant ClipboardMonitor instance.", 15);
                Environment.Exit(0);
            }

            ProcessHelper.Cover();

            base.OnStartup(e);
            
            SetupExceptionHandling();

            //create the notify icon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            Logger.Instance.LogInfo($"Started a new ClipboardMonitor instance.", 10);
            notification = new ClipboardNotification("REDACTED");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Instance.LogInfo($"ClipboardMonitor us shutting down.", 11);

            base.OnExit(e);
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
                var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = $"Unhandled exception in { (object?)assemblyName.Name} v{ (object?)assemblyName.Version}";
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"Exception in LogUnhandledException.\n{ex}", 2);
            }
            finally
            {
                Logger.Instance.LogError($"{message}\n{exception}", 3);
                Environment.Exit(1); // We don't want this to freeze
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    notification?.Dispose();
                    notifyIcon?.Dispose(); //the icon would clean up automatically, but this is cleaner
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}