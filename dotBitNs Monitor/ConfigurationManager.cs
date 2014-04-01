// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 19, 2014

// Additional Author: BoSkjoett via 'Add/Remove Startup Folder Shortcut to Your App' on Code Project,  17 Jan 2011

using dotBitNs.Models;
using IWshRuntimeLibrary;
using Shell32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace dotBitNs_Monitor
{
    class ConfigurationManager : DependencyObject, IDisposable
    {

        private static ConfigurationManager _Instance;
        public static ConfigurationManager Instance
        {
            get { return _Instance ?? (_Instance = new ConfigurationManager()); }
        }


        static bool? defMinToTray = null;
        static bool? defStartMin = null;
        static bool? defMinOnClose = null;

        public static DependencyProperty MinToTrayProperty = DependencyProperty.Register("MinToTray", typeof(bool?), typeof(ConfigurationManager), new PropertyMetadata(defMinToTray, OnDependencySettingChange));
        public static DependencyProperty MinOnCloseProperty = DependencyProperty.Register("MinOnClose", typeof(bool?), typeof(ConfigurationManager), new PropertyMetadata(defStartMin, OnDependencySettingChange));
        public static DependencyProperty StartMinProperty = DependencyProperty.Register("StartMin", typeof(bool?), typeof(ConfigurationManager), new PropertyMetadata(defMinOnClose, OnDependencySettingChange));
        public static DependencyProperty AutoStartProperty = DependencyProperty.Register("AutoStart", typeof(bool?), typeof(ConfigurationManager), new PropertyMetadata(null, OnAutostartChanged));

        FileSystemWatcher fsw;

        private ConfigurationManager()
        {
            if (Properties.Settings.Default.NeedsUpgrade)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.NeedsUpgrade = false;
            }

            fsw = new FileSystemWatcher(ConfigUtils.StartUpFolderPath);
            fsw.Changed += fsw_Changed;
            fsw.Created += fsw_Changed;
            fsw.Deleted += fsw_Changed;

            LoadSettings();
            Properties.Settings.Default.SettingsLoaded += OnSettingsLoaded;
            Properties.Settings.Default.SettingChanging += OnSavedSettingChanging;
        }

        void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateAutostartProperty());
        }

        void OnSettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            var s = Properties.Settings.Default;
            MinToTray = defMinToTray = s.MinToTray;
            StartMin = defStartMin = s.StartMin;
            MinOnClose = defMinOnClose = s.MinOnClose;
            UpdateAutostartProperty();
            fsw.EnableRaisingEvents = true;
            SupressAutostartChange = false;
        }

        private bool SupressAutostartChange = true;
        private static void OnAutostartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ConfigurationManager mgr = d as ConfigurationManager;
            if (mgr != null)
            {
                if (!mgr.SupressAutostartChange)
                {
                    if (true.Equals(e.NewValue))
                    {
                        if (!ConfigUtils.HasStartupShortcuts())
                            ConfigUtils.CreateStartupFolderShortcut();
                    }
                    else
                        ConfigUtils.DeleteStartupFolderShortcuts();
                }
            }
        }

        private void UpdateAutostartProperty()
        {
            AutoStart = ConfigUtils.HasStartupShortcuts();
        }

        private static void OnDependencySettingChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string propName = e.Property.Name;
            Properties.Settings.Default[propName] = e.NewValue;
            Properties.Settings.Default.Save();
        }

        void OnSavedSettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            if (!e.Cancel)
                switch (e.SettingName)
                {
                    case "MinToTray":
                        MinToTray = (bool)e.NewValue;
                        break;
                    case "StartMin":
                        StartMin = (bool)e.NewValue;
                        break;
                    case "MinOnClose":
                        MinOnClose = (bool)e.NewValue;
                        break;
                }
        }

        public bool? MinToTray
        {
            get { return (bool?)GetValue(MinToTrayProperty); }
            set { SetValue(MinToTrayProperty, value); }
        }

        public bool? MinOnClose
        {
            get { return (bool?)GetValue(MinOnCloseProperty); }
            set { SetValue(MinOnCloseProperty, value); }
        }

        public bool? StartMin
        {
            get { return (bool?)GetValue(StartMinProperty); }
            set { SetValue(StartMinProperty, value); }
        }

        public bool? AutoStart
        {
            get { return (bool?)GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }

        public void Dispose()
        {
            if (fsw != null)
            {
                fsw.Dispose();
                fsw = null;
            }

        }

        static class ConfigUtils
        {
            static readonly System.Reflection.Assembly Assembly = System.Reflection.Assembly.GetExecutingAssembly();
            static readonly string ProductName = Assembly.GetName().Name;
            static readonly System.Diagnostics.ProcessModule Process = System.Diagnostics.Process.GetCurrentProcess().MainModule;
            static readonly string ExecutablePath = Process.FileName;
            public static readonly string StartUpFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            public static bool HasStartupShortcuts()
            {
                return GetStartupLinks(ExecutablePath).Any();
            }

            public static void DeleteStartupFolderShortcuts()
            {
                DeleteStartupFolderShortcuts(ExecutablePath);
            }

            public static void CreateStartupFolderShortcut()
            {
                WshShellClass wshShell = new WshShellClass();
                IWshRuntimeLibrary.IWshShortcut shortcut;

                string shortcutFilename = StartUpFolderPath + "\\" + ProductName + ".lnk";
                if (System.IO.File.Exists(shortcutFilename))
                    System.IO.File.Delete(shortcutFilename);

                shortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(shortcutFilename);

                shortcut.TargetPath = ExecutablePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(ExecutablePath);
                shortcut.Description = "Launch My Application";
                shortcut.Save();
            }

            private static string GetShortcutTargetFile(string shortcutFilename)
            {
                string pathOnly = Path.GetDirectoryName(shortcutFilename);
                string filenameOnly = Path.GetFileName(shortcutFilename);

                Type ShellAppType = Type.GetTypeFromProgID("Shell.Application");
                if (ShellAppType != null)
                {
                    Shell32.Shell shell = Activator.CreateInstance(ShellAppType) as Shell32.Shell;
                    if (shell != null)
                    {
                        Shell32.Folder folder = shell.NameSpace(pathOnly);
                        if (folder != null)
                        {
                            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
                            if (folderItem != null)
                            {
                                Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                                return link.Path;
                            }
                        }
                    }
                }
                return String.Empty; // Not found
            }

            private static void DeleteStartupFolderShortcuts(string targetExeName)
            {
                foreach (var fi in GetStartupLinks(targetExeName))
                    System.IO.File.Delete(fi.FullName);
            }

            private static List<FileInfo> GetStartupLinks(string targetExeName)
            {
                List<FileInfo> toReturn = new List<FileInfo>();

                DirectoryInfo di = new DirectoryInfo(StartUpFolderPath);
                FileInfo[] files = di.GetFiles("*.lnk");
                foreach (FileInfo fi in files)
                {
                    string shortcutTargetFile = GetShortcutTargetFile(fi.FullName);

                    if (shortcutTargetFile.EndsWith(targetExeName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        toReturn.Add(fi);
                    }
                }
                return toReturn;
            }
        }
    }
}