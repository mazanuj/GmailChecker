using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using GmailApiLib;
using GmailChecker.Properties;

namespace GmailChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static ObservableCollection<string> DataItemsLog { get; set; }

        public MainWindow()
        {
            DataContext = this;
            DataItemsLog = new ObservableCollection<string>();

            Informer.OnResultStr +=
                async result =>
                    await Application.Current.Dispatcher.BeginInvoke(
                        new Action(() => DataItemsLog.Insert(0, result)));

            InitializeComponent();
            //Height = SystemParameters.WorkArea.Height;
        }

        private void LaunchGmailCheckerOnGitHub(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/mazanuj/GmailChecker/");
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ButtonStart.IsEnabled = false;
            await Initialize.StartDeleting(Settings.Default.Login, Settings.Default.From, Settings.Default.SearchStr);
            ButtonStart.IsEnabled = true;
        }
    }
}