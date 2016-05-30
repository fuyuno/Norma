﻿using System;
using System.Windows;

using Norma.Ipsilon.Views;

namespace Norma.Ipsilon
{
    /// <summary>
    ///     App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        #region Overrides of Application

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            (Shell.TaskbarIcon.DataContext as IDisposable)?.Dispose();
            Shell.TaskbarIcon.Dispose();
            base.OnExit(e);
        }

        #endregion
    }
}