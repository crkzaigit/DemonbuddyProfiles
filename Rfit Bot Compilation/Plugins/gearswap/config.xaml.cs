using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Input;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Common.Xml;
using Zeta.XmlEngine;



namespace GearSwap
{

    partial class Config
    {

        public static ListBox priority;
        
        public int ServerPort { get; set; }

        private static Window configWindow;

        public static void CloseWindow()
        {
            configWindow.Close();
        }

        public static void populateList()
        {

        }

        public static Window GetDisplayWindow()
        {
            if (configWindow == null)
            {
                configWindow = new Window();
            }

            string assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string xamlPath = Path.Combine(assemblyPath, "Plugins", "GearSwap", "config.xaml");
            string xamlContent = File.ReadAllText(xamlPath);
            // This hooks up our object with our UserControl DataBinding
            //configWindow.DataContext = new ConfigViewModel();
            configWindow.DataContext = GearSwapSettings.Instance;
            UserControl mainControl = (UserControl)XamlReader.Load(new MemoryStream(Encoding.UTF8.GetBytes(xamlContent)));
            configWindow.Content = mainControl;
            configWindow.Width = 460;
            configWindow.Height = 500;
            configWindow.ResizeMode = ResizeMode.NoResize;
            configWindow.Background = Brushes.DarkGray;
            configWindow.Title = "GearSwap";
            
            configWindow.Closed += ConfigWindow_Closed;
            Demonbuddy.App.Current.Exit += ConfigWindow_Closed;

            priority = LogicalTreeHelper.FindLogicalNode(mainControl, "priorityList") as ListBox;
            gearSwap.populatePriorityList();
            return configWindow;
        }

        static void ConfigWindow_Closed(object sender, System.EventArgs e)
        {
            GearSwapSettings.Instance.Save();
            gearSwap.updateSettings();
            if (configWindow != null)
            {
                configWindow.Closed -= ConfigWindow_Closed;
                configWindow = null;
            }
        }
    }
}
