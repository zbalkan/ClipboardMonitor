using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipboardMonitor.Helpers;

namespace ClipboardMonitor;

internal static class Program
{
    private static readonly HashSet<string> InstallArgs = new(StringComparer.Ordinal) { "-i", "/i", "--install" };
    private static readonly HashSet<string> UninstallArgs = new(StringComparer.Ordinal) { "-u", "/u", "--uninstall" };
    private static readonly HashSet<string> HelpArgs = new(StringComparer.Ordinal) { "-?", "-h", "/h", "--help" };

    [STAThread]
    private static void Main()
    {
        HandleArguments();

        if (!Logger.Instance.Check())
        {
            const string message = "You need to run as administrator with argument '-i' to install the application.";
            _ = MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            Environment.Exit(0);
        }

        if (ProcessHelper.IsDuplicate())
        {
            Logger.Instance.LogInfo("Killing redundant ClipboardMonitor instance.", 15);
            Environment.Exit(0);
        }

        ProcessHelper.Cover();
        SetupExceptionHandling();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Logger.Instance.LogInfo("Started a new ClipboardMonitor instance.", 10);
        AlertHandler.Instance.SubstituteText = "REDACTED";

        using var notification = new ClipboardNotification();
        PasteGuard.Initialize();

        Application.ApplicationExit += OnApplicationExit;
        Application.Run();

        static void OnApplicationExit(object? sender, EventArgs e)
        {
            Logger.Instance.LogInfo("ClipboardMonitor is shutting down.", 11);
            ProcessHelper.Uncover();
            PasteGuard.Dispose();
        }
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
                MessageBox.Show("ClipboardMonitor is already installed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            else
            {
                Logger.Instance.Install();
                MessageBox.Show("Installation completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        catch (SecurityException ex)
        {
            Debug.WriteLine(ex.Message);
            _ = MessageBox.Show("You need to run as administrator first to install the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
            MessageBox.Show("Uninstallation completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        catch (Win32Exception ex)
        {
            Debug.WriteLine(ex.Message);
            _ = MessageBox.Show("You need to run as administrator first to uninstall the application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
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
        _ = MessageBox.Show(message, "Help", MessageBoxButtons.OK, MessageBoxIcon.Information,
            MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        Environment.Exit(0);
    }

    private static void SetupExceptionHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

        Application.ThreadException += (s, e) =>
            LogUnhandledException(e.Exception, "Application.ThreadException");

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
}
