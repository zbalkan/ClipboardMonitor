namespace ClipboardMonitor
{
    internal static class SuspiciousContent
    {
        public static bool HasSuspiciousText(string content) => _suspicious.Contains(content);

        private static readonly AhoCorasickTree _suspicious = new AhoCorasickTree(
          new[]
           {
                    "pwsh",
                    "powershell",
                    "mshta",
                    "cmd",
                    "msiexec",
                    "rundll",
                    "mshta",
                    "wscript",
                    "cscript",
                    "curl",
                    "wget",
                    "iwr",
                    "Invoke-WebRequest",
                    "irm",
                    "Invoke-RestMethod",
                    "iex",
                    "Invoke-Expression",
          }
      );
    }
}