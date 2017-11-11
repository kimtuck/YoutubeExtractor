using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace ExtractorUI
{
    public static class YoutubeUrl
    {
        public static IEnumerable<string> GetChromeTabs()
        {
            List<string> urls = new List<string>();
            Process[] procsChrome = Process.GetProcessesByName("chrome");
            foreach (Process chrome in procsChrome)
            {
                // the chrome process must have a window
                if (chrome.MainWindowHandle == IntPtr.Zero)
                {
                    continue;
                }
                //AutomationElement elm = AutomationElement.RootElement.FindFirst(TreeScope.Children,
                //         new PropertyCondition(AutomationElement.ClassNameProperty, "Chrome_WidgetWin_1"));
                // find the automation element
                AutomationElement elm = AutomationElement.FromHandle(chrome.MainWindowHandle);

                // manually walk through the tree, searching using TreeScope.Descendants is too slow (even if it's more reliable)
                var scope = elm.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, ""));
                foreach (System.Windows.Automation.AutomationElement automationElement in scope)
                {
                    var n = automationElement.GetUrlBar();
                    var elmUrlBar = automationElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"));
                    if (elmUrlBar == null)
                        continue;

                    // elmUrlBar is now the URL bar element. we have to make sure that it's out of keyboard focus if we want to get a valid URL
                    if ((bool)elmUrlBar.GetCurrentPropertyValue(AutomationElement.HasKeyboardFocusProperty))
                    {
                        continue;
                    }

                    // there might not be a valid pattern to use, so we have to make sure we have one
                    AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();
                    if (patterns.Length == 1)
                    {
                        string ret = "";
                        try
                        {
                            ret = ((ValuePattern)elmUrlBar.GetCurrentPattern(patterns[0])).Current.Value;
                        }
                        catch { }
                        if (ret != "")
                        {
                            // must match a domain name (and possibly "https://" in front)
                            if (Regex.IsMatch(ret, @"^(https:\/\/)?[a-zA-Z0-9\-\.]+(\.[a-zA-Z]{2,4}).*$"))
                            {
                                // prepend http:// to the url, because Chrome hides it if it's not SSL
                                if (!ret.StartsWith("http"))
                                {
                                    ret = "http://" + ret;
                                }

                                if (ret.StartsWith("https://www.youtube.com"))
                                    urls.Add(ret);
                            }
                        }
                    }
                }
            }
            return urls;
        }
    }
}
