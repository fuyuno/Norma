﻿using System.Diagnostics;
using System.IO;
using System.Windows;

using MetroRadiance.UI;

using Norma.Eta;
using Norma.Eta.Models;
using Norma.Models;
using Norma.Views;

namespace Norma
{
    /// <summary>
    ///     アプリケーションの初期化時に呼ばれます。
    /// </summary>
    internal static class AppInitializer
    {
        private static StartupScreen _startupScreen;
        public static AbemaApiHost AbemaApiHost { get; private set; }
        public static AbemaState AbemaState { get; private set; }
        public static Configuration Configuration { get; private set; }
        public static Timetable Timetable { get; private set; }
        public static ConnectOps ConnectOps { get; private set; }
        public static Connector Connector { get; private set; }

        /// <summary>
        ///     PreInitialize is called by Application host.
        /// </summary>
        /// <param name="application"></param>
        public static void PreInitialize(Application application)
        {
            _startupScreen = new StartupScreen();
            _startupScreen.Show();

            if (!Directory.Exists(NormaConstants.CrashReportsDir))
                Directory.CreateDirectory(NormaConstants.CrashReportsDir);

            CefSetting.Init();
            ThemeService.Current.Register(application, Theme.Dark, Accent.Blue);

            Configuration = new Configuration();
            AbemaApiHost = new AbemaApiHost(Configuration);
            Timetable = new Timetable(AbemaApiHost);
            AbemaState = new AbemaState(Configuration, Timetable);
            ConnectOps = new ConnectOps();
            Connector = new Connector(ConnectOps);
        }

        /// <summary>
        ///     Initialize is called by Bootstrapper.
        /// </summary>
        public static void Initialize()
        {
            AbemaApiHost.Initialize();
            Timetable.Sync();
            AbemaState.Start();
        }

        /// <summary>
        ///     PostInitialize is called by Bootstrapper.
        /// </summary>
        public static void PostInitialize()
        {
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(NormaConstants.IpsilonFileName));
            if (processes.Length == 0)
                if (File.Exists(NormaConstants.IpsilonExecutableFile))
                    Process.Start(NormaConstants.IpsilonExecutableFile);

            _startupScreen.Hide();
            _startupScreen.Close();
            _startupScreen = null;
        }
    }
}