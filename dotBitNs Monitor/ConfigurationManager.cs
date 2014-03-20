// Products: MeowBit dotBitNS
// THE BEASTLICK INTERNET POLICY COMMISSION & Alien Seed Software
// Author: Derrick Slopey derrick@alienseed.com
// March 19, 2014

using dotBitNs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace dotBitNs_Monitor
{
    class ConfigurationManager : DependencyObject
    {

        private static ConfigurationManager _Instance;
        public static ConfigurationManager Instance
        {
            get { return _Instance ?? (_Instance = new ConfigurationManager()); }
        } 


        static bool? defMinToTray = null;
        static bool? defStartMin = null;
        static bool? defMinOnClose = null;
        static bool? defAutoStart = null;

        public static DependencyProperty MinToTrayProperty = DependencyProperty.Register("MinToTray", typeof(bool?), typeof(ConfigurationManager), new PropertyMetadata(defMinToTray, OnDependencySettingChange));
        public static DependencyProperty MinOnCloseProperty = DependencyProperty.Register("MinOnClose", typeof(bool?), typeof(ConfigurationManager), new PropertyMetadata(defStartMin, OnDependencySettingChange));
        public static DependencyProperty StartMinProperty = DependencyProperty.Register("StartMin", typeof(bool?), typeof(ConfigurationManager), new PropertyMetadata(defMinOnClose, OnDependencySettingChange));
        public static DependencyProperty AutoStartProperty = DependencyProperty.Register("AutoStart", typeof(bool?), typeof(ConfigurationManager), new PropertyMetadata(defAutoStart, OnDependencySettingChange));

        private ConfigurationManager()
        {
            if (Properties.Settings.Default.NeedsUpgrade)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.NeedsUpgrade = false;
            }

            LoadSettings();
            Properties.Settings.Default.SettingsLoaded += OnSettingsLoaded;
            Properties.Settings.Default.SettingChanging += OnSavedSettingChanging;

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
            AutoStart = defAutoStart = s.AutoStart;
        }

        private static void OnDependencySettingChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string propName = e.Property.Name;
            Properties.Settings.Default[propName] = e.NewValue;
            Properties.Settings.Default.Save();
        }
        
        void OnSavedSettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            if(!e.Cancel)
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
                    case "AutoStart":
                        AutoStart = (bool)e.NewValue;
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

    }
}
