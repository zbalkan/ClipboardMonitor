// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Semantically, it does not mean anything in static context.", Scope = "member", Target = "~M:ClipboardMonitor.Logger.Install")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Semantically, it does not mean anything in static context.", Scope = "member", Target = "~M:ClipboardMonitor.Logger.Check~System.Boolean")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Handling unhandled exceptions. It can be of any type.", Scope = "member", Target = "~M:ClipboardMonitor.App.LogUnhandledException(System.Exception,System.String)")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "It is hard to determine wich exceptions would be thrown out of native methods.", Scope = "member", Target = "~M:ClipboardMonitor.ProcessHelper.CaptureProcessInfo~ClipboardMonitor.ProcessInformation")]
