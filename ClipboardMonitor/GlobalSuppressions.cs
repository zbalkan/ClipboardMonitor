// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Semantically, it does not mean anything in static context.", Scope = "member", Target = "~M:ClipboardMonitor.Logger.Install")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Semantically, it does not mean anything in static context.", Scope = "member", Target = "~M:ClipboardMonitor.Logger.Check~System.Boolean")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Handling unhandled exceptions. It can be of any type.", Scope = "member", Target = "~M:ClipboardMonitor.App.LogUnhandledException(System.Exception,System.String)")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "It is hard to determine which exceptions would be thrown out of native methods.", Scope = "member", Target = "~M:ClipboardMonitor.ProcessHelper.CaptureProcessInfo~ClipboardMonitor.ProcessInformation")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Instead of throwing exception, we return null.", Scope = "member", Target = "~M:ClipboardMonitor.ProcessHelper.FindProcess(System.Int32)~System.Diagnostics.Process")]
[assembly: SuppressMessage("Minor Code Smell", "S3261:Namespaces should not be empty", Justification = "The file is used to mark CLS compliance only.", Scope = "namespace", Target = "~N:ClipboardMonitor")]
[assembly: SuppressMessage("Roslynator", "RCS1072:Remove empty namespace declaration.", Justification = "The file is used to mark CLS compliance only.", Scope = "namespace", Target = "~N:ClipboardMonitor")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "<Pending>", Scope = "member", Target = "~M:ClipboardMonitor.ClipboardNotification.NotificationForm.WndProc(System.Windows.Forms.Message@)")]
