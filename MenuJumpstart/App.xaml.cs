using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace JumpstartMenu
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Boolean DebugMode = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Contains("-DebugMode"))
            {
                MainWindow MainWindow = new MainWindow(true);
            }
            else
            {
                MainWindow MainWindow = new MainWindow(false);
            }
        }
    }
}
