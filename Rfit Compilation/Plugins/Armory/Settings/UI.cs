using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;


namespace Armory.Settings
{
    public class UI
    {
        private static Window _window;
        public static Window GetDisplayWindow()
        {
            try
            {
                if (_window == null)
                    _window = new Window();

                string assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                string xamlPath = Path.Combine(assemblyPath, "Plugins", "Armory", "Settings", "UI.xaml");

                string xamlContent = File.ReadAllText(xamlPath);

                // Change reference to custom class
                xamlContent = xamlContent.Replace(
                    "xmlns:ArmorySettings=\"clr-namespace:Armory.Settings\"",
                    "xmlns:ArmorySettings=\"clr-namespace:Armory.Settings;assembly=" + assemblyName + "\"");

                // This hooks up our object with our UserControl DataBinding
                _window.DataContext = ArmorySettings.Instance;

                UserControl mainControl = (UserControl)XamlReader.Load(new MemoryStream(Encoding.UTF8.GetBytes(xamlContent)));
                _window.Content = mainControl;
                _window.Width = 400;
                _window.Height = 500;
                _window.MinWidth = 350;
                _window.MinHeight = 360;
                _window.ResizeMode = ResizeMode.CanResizeWithGrip;

                _window.Title = "Armory";

                _window.Closed += ConfigWindow_Closed;
                Application.Current.Exit += ConfigWindow_Closed;

                return _window;
            }
            catch (Exception ex)
            {
                Logger.Log("Error opening Config window: " + ex);
                return null;
            }
        }

        static void ConfigWindow_Closed(object sender, EventArgs e)
        {
            ArmorySettings.Instance.Save();
            ArmorySettings.Instance.ProtectedSlots.Save();
            ArmorySettings.Instance.MysteryItemSlots.Save();
            if (_window != null)
            {
                _window.Closed -= ConfigWindow_Closed;
                _window = null;
            }
        }

        public ArmorySettings Settings
        {
            get { return ArmorySettings.Instance; }
        }

        public UI()
        {

        }

    }
}
