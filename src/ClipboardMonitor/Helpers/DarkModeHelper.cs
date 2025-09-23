namespace ClipboardMonitor
{
    using Microsoft.Win32;

    public static class DarkModeHelper
    {
        /// <summary>
        /// Returns true if the system prefers dark mode for apps, false if light.
        /// </summary>
        public static bool IsDarkModeEnabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key == null)
                    {
                        return false;
                    }

                    var value = key.GetValue("AppsUseLightTheme");
                    if (value == null)
                    {
                        return false;
                    }

                    var intValue = (int)value;
                    return intValue == 0; // 0 = Dark mode, 1 = Light mode
                }
            }
            catch
            {
                return false;
            }
        }
    }

}
