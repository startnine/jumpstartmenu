using JumpstartMenu.Views;
using System;
using System.AddIn;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JumpstartMenu
{
    [AddIn("Jumpstart Menu", Description = "Jumpstarts your day!", Version = "1.0.0.0", Publisher = "Start9")]
    public class JumpstartMenuAddIn : IModule
    {
        public static JumpstartMenuAddIn Instance { get; private set; }

        public IConfiguration Configuration { get; set; } = new JumpstartMenuConfiguration();

        public IMessageContract MessageContract => null;

        public IReceiverContract ReceiverContract { get; } = new JumpstartMenuReceiverContract();

        public IHost Host { get; private set; }

        public void Initialize(IHost host)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => Start9.Api.Plex.MessageBox.Show(null, e.ExceptionObject.ToString(), "Uh Oh Exception!");

            void Start()
            {
                Instance = this;
                Application.ResourceAssembly = Assembly.GetExecutingAssembly();
                App.Main();
            }

            var t = new Thread(Start);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

    }

    public class JumpstartMenuReceiverContract : IReceiverContract
    {
        public JumpstartMenuReceiverContract()
        {
            StartMenuOpenedEntry.MessageReceived += (sender, e) =>
            {
                ((MainWindow)Application.Current.MainWindow).Topmost = true;
                ((MainWindow)Application.Current.MainWindow).Show();
            };
        }
        public IList<IReceiverEntry> Entries => new[] { StartMenuOpenedEntry };
        public IReceiverEntry StartMenuOpenedEntry { get; } = new ReceiverEntry("Open menu");
    }


    public class JumpstartMenuConfiguration : IConfiguration
    {
        public IList<IConfigurationEntry> Entries => new[] { new ConfigurationEntry(PinnedItems, "Pinned Items"), new ConfigurationEntry(Places, "Places") };

        public IList<String> PinnedItems { get; } = new List<String>();
        public IList<String> Places { get; } = new List<String>();
    }

}
