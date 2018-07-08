using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Interop;
using Timer = System.Timers.Timer;
using Start9.Api.Controls;
using Start9.Api;
using IWshRuntimeLibrary;
using File = System.IO.File;
using System.Drawing;
using System.Runtime.InteropServices;
using Start9.Api.DiskItems;

namespace JumpstartMenu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Window TempStartWindow;

        public Boolean DebugMode = false;

        public MainWindow(Boolean debugMode)
        {
            InitializeComponent();
            Application.Current.MainWindow = this;
            DebugMode = debugMode;
            Loaded += MainWindow_Loaded;
            Show();


            Deactivated += (sender, e) => Hide();
        }

        private void TempStart_SizeChanged(Object sender, SizeChangedEventArgs e)
        {
            TempStartWindow.Width = (TempStartWindow.Content as ToggleButton).Width;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            var extendedStyle = WinApi.GetWindowLong(hwnd, WinApi.GwlExstyle);
            WinApi.SetWindowLong(hwnd, WinApi.GwlExstyle, extendedStyle.ToInt32() | WinApi.WsExToolwindow);
        }

        private void TempStartWindow_Loaded(Object sender, RoutedEventArgs e)
        {
            if (DebugMode)
            {
                (TempStartWindow.Content as ToggleButton).MouseRightButtonUp += delegate { LoadResourceDictionaryButton_Click(null, null); };
            }

            var hwnd = new WindowInteropHelper(TempStartWindow).Handle;
            var extendedStyle = WinApi.GetWindowLong(hwnd, WinApi.GwlExstyle);
            WinApi.SetWindowLong(hwnd, WinApi.GwlExstyle, extendedStyle.ToInt32() | WinApi.WsExToolwindow);

            Timer topTimer = new Timer(1);

            topTimer.Elapsed += delegate
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    WinApi.SetWindowPos(new WindowInteropHelper(TempStartWindow).Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0010);
                }));
            };

            topTimer.Start();


            Timer debugTimer = new Timer(1);
            debugTimer.Start();
        }

        private void MainWindow_Loaded(Object sender, RoutedEventArgs e)
        {
            /*if (args.Length >= 1)
            {
                if (args[0] == ("DebugMode"))
                {
                    LoadResourceDictionaryButton.Visibility = Visibility.Visible;
                }
            }*/

            if (DebugMode)
            {
                //LoadResourceDictionaryButton.Visibility = Visibility.Visible;
            }

            /*var showMenuThickness = (((Storyboard)Resources["ShowMenu"]).Children[0] as ThicknessAnimation);
            var hideMenuThickness = (((Storyboard)Resources["HideMenu"]).Children[0] as ThicknessAnimation);
            //Storyboard.SetTargetName(showMenuThickness, RootGrid.Name);
            //Storyboard.SetTargetName(hideMenuThickness, RootGrid.Name);
            try
            {
                showMenuThickness.Completed += delegate
                {
                    RootGrid.BeginAnimation(Grid.MarginProperty, null);
                    RootGrid.Margin = new Thickness(0, 0, 0, 0);
                };

                hideMenuThickness.Completed += delegate
                {
                    RootGrid.BeginAnimation(Grid.MarginProperty, null);
                    RootGrid.Margin = new Thickness(-256, 0, 256, 0);
                };
            }
            catch (NullReferenceException ex) { }*/

            AllAppsTree.Items.Clear();
            AllAppsTree.ItemsSource = PopulateAllAppsList();
        }

        private List<IconTreeViewItem> PopulateAllAppsList()
        {
            List<IconTreeViewItem> AllAppsAppDataItems = GetAllAppsFoldersAsTree(Environment.ExpandEnvironmentVariables(@"%appdata%\Microsoft\Windows\Start Menu\Programs"));
            List<IconTreeViewItem> AllAppsProgramDataItems = GetAllAppsFoldersAsTree(Environment.ExpandEnvironmentVariables(@"%programdata%\Microsoft\Windows\Start Menu\Programs"));
            List<IconTreeViewItem> AllAppsItems = new List<IconTreeViewItem>();
            List<IconTreeViewItem> AllAppsReorgItems = new List<IconTreeViewItem>();

            Dispatcher.Invoke(new Action(() =>
            {
                foreach (IconTreeViewItem t in AllAppsAppDataItems)
                {
                    var FolderIsDuplicate = false;

                    foreach (IconTreeViewItem v in AllAppsProgramDataItems)
                    {
                        List<IconTreeViewItem> SubItemsList = new List<IconTreeViewItem>();

                        if (Directory.Exists(t.Tag.ToString()))
                        {
                            if ((t.Tag.ToString().Substring(t.Tag.ToString().LastIndexOf(@"\"))) == (v.Tag.ToString().Substring(v.Tag.ToString().LastIndexOf(@"\"))))
                            {
                                FolderIsDuplicate = true;
                                foreach (IconTreeViewItem i in t.Items)
                                {
                                    SubItemsList.Add(i);
                                }

                                foreach (IconTreeViewItem j in v.Items)
                                {
                                    SubItemsList.Add(j);
                                }
                            }

                            /*if (SubItemsList.Count != 0)
                            {
                                v.ItemsSource = SubItemsList;
                            }*/
                        }

                        if (!AllAppsItems.Contains(v))
                        {
                            AllAppsItems.Add(v);
                        }
                    }

                    if ((!AllAppsItems.Contains(t)) && (!FolderIsDuplicate))
                    {
                        AllAppsItems.Add(t);
                    }
                }

                foreach (IconTreeViewItem x in AllAppsItems)
                {
                    if (File.Exists(x.Tag.ToString()))
                    {
                        AllAppsReorgItems.Add(x);
                    }
                }

                foreach (IconTreeViewItem x in AllAppsItems)
                {
                    if (Directory.Exists(x.Tag.ToString()))
                    {
                        AllAppsReorgItems.Add(x);
                    }
                }
            }));

            return AllAppsReorgItems;
        }


        struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public UInt32 dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public String szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public String szTypeName;
        };

        class Win32
        {
            public const UInt32 SHGFI_ICON = 0x100;
            public const UInt32 SHGFI_LARGEICON = 0x0;    // 'Large icon
            public const UInt32 SHGFI_SMALLICON = 0x1;    // 'Small icon

            [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr SHGetFileInfo(String pszPath,
                                        UInt32 dwFileAttributes,
                                        ref SHFILEINFO psfi,
                                        UInt32 cbSizeFileInfo,
                                        UInt32 uFlags);
        }

        public IconTreeViewItem AllAppsListGetItem(String path)
        {
            var target = path;

            if (System.IO.Path.GetExtension(path).Contains("lnk"))
            {
                target = GetTargetPath(path);
            }

            IconTreeViewItem item = new IconTreeViewItem()
            {   
                Tag = target
            };

            if (Directory.Exists(target))
            {
                foreach (var s in Directory.EnumerateFiles(target))
                {
                    var subItem = AllAppsListGetItem(s);
                    subItem.MinWidth = item.MinWidth + 16;
                    item.Items.Add(subItem);
                }
            }

            item.Header = System.IO.Path.GetFileNameWithoutExtension(path);

            if (Directory.Exists(item.Tag.ToString()))
            {
                item.MouseDoubleClick += Item_Opened;
            }
            else if (File.Exists(item.Tag.ToString()))
            {
                item.Expanded += Item_Opened;
            }

            if ((File.Exists(item.Tag.ToString())) | (Directory.Exists(item.Tag.ToString())))
            {
                //var fi = new SHFILEINFO();

                //var img = Win32.SHGetFileInfo(item.Tag.ToString(), 
                //    0,
                //    ref fi, 
                //    (uint)Marshal.SizeOf(fi),
                //    Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);

                //ImageSource entryIconImageSource = Imaging.CreateBitmapSourceFromHIcon(
                //    SystemIcons.Shield.Handle,
                //    Int32Rect.Empty,
                //    BitmapSizeOptions.FromWidthAndHeight(SystemScaling.RealPixelsToWpfUnits(16), SystemScaling.RealPixelsToWpfUnits(16)));

                item.Icon = new Canvas()
                {
                    Width = 16,
                    Height = 16,
                    Background = (ImageBrush) new DiskItemToIconImageBrushConverter().Convert(new DiskItem(item.Tag.ToString()), null, 64, null)
                };
            }

            return item;
        }

        private void Item_Opened(Object sender, RoutedEventArgs e)
        {
            var item = (sender as IconTreeViewItem);
            try
            {
                ProcessStartInfo info = new ProcessStartInfo()
                {
                    FileName = item.Tag.ToString(),
                    WorkingDirectory = Path.GetDirectoryName(item.Tag.ToString()),
                    UseShellExecute = true
                };
                Process.Start(info);
            }
            catch (Exception ex)
            {
                if (ex is Win32Exception)
                {
                    Debug.WriteLine(ex);
                    Debug.WriteLine("It's a Win32Exception, so probably just the user selecting a program that requires UAC privileges, and then canceling out.");
                }
                else
                {
                    try
                    {
                        ProcessStartInfo info = new ProcessStartInfo()
                        {
                            FileName = item.Tag.ToString(),
                            WorkingDirectory = Path.GetDirectoryName(item.Tag.ToString())
                        };
                        Process.Start(info);
                    }
                    catch { }                                                                   
                }
            }
            Hide();
        }

        private List<IconTreeViewItem> GetAllAppsFoldersAsTree(String Path)
        {
            List<IconTreeViewItem> AllAppsItems = new List<IconTreeViewItem>();

            foreach (var s in Directory.EnumerateFiles(Path))
            {
                IconTreeViewItem t = AllAppsListGetItem(s);
                /*((string)(((t.Header as DockPanel).Children[1] as System.Windows.Controls.Label).Content as string)*/
                /*(string)(((t.Header as DockPanel).Children[1] as System.Windows.Controls.Label).Content as string)*/
                if (!(t.Header.ToString().ToLower().Contains("desktop")))
                {
                    /*if (System.IO.Path.GetExtension(t.Tag.ToString()).Contains("lnk"))
                    {
                        t.Tag = GetTargetPath(t.Tag.ToString());
                    }*/
                    AllAppsItems.Add(t);
                }
            }

            foreach (var s in Directory.EnumerateDirectories(Path))
            {
                IconTreeViewItem t = AllAppsListGetItem(s);
                /*if (((string)(((t.Header as DockPanel).Children[1] as System.Windows.Controls.Label).Content as string) != "desktop") & ((string)(((t.Header as DockPanel).Children[1] as System.Windows.Controls.Label).Content as string) != "desktop.ini"))*/
                /*if ((t.Header.ToString() != "desktop") & (t.Header.ToString() != "desktop.ini"))*/
                if (!(t.Header.ToString().ToLower().Contains("desktop")))
                {
                    if (t.Items.Count != 0)
                    {
                        AllAppsItems.Add(t);
                    }
                }
            }
            return AllAppsItems;
        }

        public String GetTargetPath(String filePath)
        {
            String targetPath = null;

            if (targetPath == null)
            {
                targetPath = ResolveShortcut(filePath);
            }

            if (targetPath == null)
            {
                targetPath = GetInternetShortcut(filePath);
            }

            if (targetPath == null | targetPath == "" | targetPath.Replace(" ", "") == "")
            {
                return filePath;
            }
            else
            {
                return targetPath;
            }
        }

        public String GetInternetShortcut(String filePath)
        {
            try
            {
                var url = "";

                using (TextReader reader = new StreamReader(filePath))
                {
                    var line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("URL="))
                        {
                            String[] splitLine = line.Split('=');
                            if (splitLine.Length > 0)
                            {
                                url = splitLine[1];
                                break;
                            }
                        }
                    }
                }
                return url;
            }
            catch
            {
                return null;
            }
        }

        String ResolveShortcut(String filePath)
        {
            // IWshRuntimeLibrary is in the COM library "Windows Script Host Object Model"
            var shell = new WshShell();

            try
            {
                IWshShortcut shortcut = shell.CreateShortcut(filePath);
                return shortcut.TargetPath;
            }
            catch
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                return null;
            }
        }


        public const Int32 MaxFeatureLength = 38;
        public const Int32 MaxGuidLength = 38;
        public const Int32 MaxPathLength = 1024;

        public enum InstallState
        {
            NotUsed = -7,
            BadConfig = -6,
            Incomplete = -5,
            SourceAbsent = -4,
            MoreData = -3,
            InvalidArg = -2,
            Unknown = -1,
            Broken = 0,
            Advertised = 1,
            Removed = 1,
            Absent = 2,
            Local = 3,
            Source = 4,
            Default = 5
        }

        //string ResolveMsiShortcut(string file)
        //{
        //    StringBuilder product = new StringBuilder(MaxGuidLength + 1);
        //    StringBuilder feature = new StringBuilder(MaxFeatureLength + 1);
        //    StringBuilder component = new StringBuilder(MaxGuidLength + 1);

        //    MsiGetShortcutTarget(file, product, feature, component);

        //    int pathLength = MaxPathLength;
        //    StringBuilder path = new StringBuilder(pathLength);

        //    InstallState installState = MsiGetComponentPath(product.ToString(), component.ToString(), path, ref pathLength);
        //    if (installState == InstallState.Local)
        //    {
        //        return path.ToString();
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        new public void Show()
        {
            //Ststem.Drawing.Point
            var screen = System.Windows.Forms.Screen.FromPoint(System.Windows.Forms.Cursor.Position);
            Left = SystemScaling.RealPixelsToWpfUnits(screen.WorkingArea.Left);
            Top = SystemScaling.RealPixelsToWpfUnits(screen.WorkingArea.Top);
            Height = SystemScaling.RealPixelsToWpfUnits(screen.WorkingArea.Height);
            base.Show();
            BeginStoryboard((Storyboard)Resources["ShowMenu"]);
        }

        new public void Hide()
        {
            ((Storyboard)Resources["HideMenu"]).Completed += delegate { base.Hide(); };
            BeginStoryboard((Storyboard)Resources["HideMenu"]);
        }

        private void ShutDownButton_Click(Object sender, RoutedEventArgs e)
        {
            SystemContext.ShutDownSystem();
        }

        private void RestartButton_Click(Object sender, RoutedEventArgs e)
        {
            SystemContext.RestartSystem();
        }

        private void SignOutButton_Click(Object sender, RoutedEventArgs e)
        {
            SystemContext.SignOut();
        }

        private void SwitchUserButton_Click(Object sender, RoutedEventArgs e)
        {
            SystemContext.LockUserAccount();
        }


        private void LoadResourceDictionaryButton_Click(Object sender, RoutedEventArgs e)
        {
            SelectResourceDictionary();
            var tempStart = (TempStartWindow.Content as ToggleButton);
            tempStart.Style = (Style)Resources["StartStyle"];
        }

        void SelectResourceDictionary()
        {
            var openDictionaryDialog = new OpenFileDialog();
            var result = openDictionaryDialog.ShowDialog();
            if (result == true)
                Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(openDictionaryDialog.FileName) });
        }
        //{ InitialDirectory = @"C:\Users\Splitwirez\Documents\Start9\Modules\JumpstartMenu\ResourceDictionaries" };

        private void SleepButton_Click(Object sender, RoutedEventArgs e)
        {
            SystemContext.SleepSystem();
        }
    }
}