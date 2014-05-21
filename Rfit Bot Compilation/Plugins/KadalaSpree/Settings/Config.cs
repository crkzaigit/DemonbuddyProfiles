using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace KadalaSpree
{
    class Config
    {
        public int ServerPort { get; set; }

        private static Window _configWindow;

        public static void CloseWindow()
        {
            _configWindow.Close();
        }

        private static string _xamlPath;

        public static Window GetDisplayWindow()
        {
            if (_configWindow == null)
            {
                _configWindow = new Window();
            }

            try
            {

                string assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                if (assemblyPath != null)
                {

                    if (_xamlPath == null)
                    {
                        string xamlDir1 = Path.Combine(assemblyPath, "Plugins", "db-kadalaspree");
                        string xamlDir2 = Path.Combine(assemblyPath, "Plugins", "KadalaSpree");

                        if (Directory.Exists(xamlDir1))
                        {
                            _xamlPath = Path.Combine(xamlDir1, "Settings", "Config.xaml");
                        }
                        else if (Directory.Exists(xamlDir2))
                        {
                            _xamlPath = Path.Combine(xamlDir1, "Settings", "Config.xaml");
                        }
                        else
                        {
                            Logger.Log("Path to Settings XAML not found, install plugin in either /Plugins/KadalaSpree/ or /plugins/db-kadalaspree/ directory");
                            return null;
                        }
                    }

                    string xamlContent = File.ReadAllText(_xamlPath);

                    // This hooks up our object with our UserControl DataBinding
                    _configWindow.DataContext = KadalaSpreeSettings.Instance;

                    UserControl mainControl = (UserControl)XamlReader.Load(new MemoryStream(Encoding.UTF8.GetBytes(xamlContent)));
                    _configWindow.Content = mainControl;
                }

                _configWindow.Width = 360;
                _configWindow.Height = 540;
                _configWindow.ResizeMode = ResizeMode.NoResize;
                _configWindow.Background = Brushes.DarkGray;
                _configWindow.Title = "KadalaSpree";

                _configWindow.Closed += ConfigWindow_Closed;
                Application.Current.Exit += ConfigWindow_Closed;

            }
            catch (Exception ex)
            {
                Logger.Log("Failed to load Config UI {0}", ex);
            }

            return _configWindow;
        }

        static void ConfigWindow_Closed(object sender, System.EventArgs e)
        {
            KadalaSpreeSettings.Instance.Save();
            if (_configWindow == null)
                return;
            _configWindow.Closed -= ConfigWindow_Closed;
            _configWindow = null;
        }
    }
}
