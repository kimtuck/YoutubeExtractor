using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using YoutubeExtractor;
using System.Windows.Automation;
using System.Text.RegularExpressions;
using System.Threading;

namespace ExtractorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private bool loading;
        public MainWindow()
        {
            InitializeComponent();
            Message.Visibility = Visibility.Hidden;
            loading = false;
            MonitorChromeUrl();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DoDownload(url.Text);
        }

        private void DoDownload(string url)
        {
            Message.Visibility = Visibility.Visible;
            Message.Content = "Working...";
            loading = true;
            updateProgress(0);
            var downloader = new AudioDownloader(updateProgress);
            var successful = downloader.Download(url);
            loading = false;
            Message.Content = "Done.";
            if (!successful)
                Message.Content = "Can't get audio from this video.";
        }

        private void updateProgress(double progressPercent)
        {
            progress.Dispatcher.Invoke(() => progress.Value = progressPercent, DispatcherPriority.Background);
        }

        private void ChromeUrlButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty((string)ChromeUrl.Content) == false)
            {
                DoDownload((string)ChromeUrl.Content);
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (loading)
                return;

            var chromeUrl = YoutubeUrl.GetChromeTabs();
            ChromeUrl.Content = "";
            if (chromeUrl.Count() > 0)
                ChromeUrl.Content = chromeUrl.First();
        }

        private void MonitorChromeUrl()
        {
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
    }
}


public static class AutomationElementExtensions
{
    public static AutomationElement GetUrlBar(this AutomationElement element)
    {
        try
        {
            return InternalGetUrlBar(element);
        }
        catch
        {
            // Chrome has probably changed something, and above walking needs to be modified. :(
            // put an assertion here or something to make sure you don't miss it
            return null;
        }
    }

    public static string TryGetValue(this AutomationElement urlBar, AutomationPattern[] patterns)
    {
        try
        {
            return ((ValuePattern)urlBar.GetCurrentPattern(patterns[0])).Current.Value;
        }
        catch
        {
            return "";
        }
    }

    //

    private static AutomationElement InternalGetUrlBar(AutomationElement element)
    {
        // walking path found using inspect.exe (Windows SDK) for Chrome 29.0.1547.76 m (currently the latest stable)
        var elm1 = element.FindFirst(TreeScope.Children,
          new PropertyCondition(AutomationElement.NameProperty, "Google Chrome"));
        var elm2 = TreeWalker.RawViewWalker.GetLastChild(elm1); // I don't know a Condition for this for finding :(
        var elm3 = elm2.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, ""));
        var elm4 = elm3.FindFirst(TreeScope.Children,
          new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar));
        var result = elm4.FindFirst(TreeScope.Children,
          new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom));

        return result;
    }
}