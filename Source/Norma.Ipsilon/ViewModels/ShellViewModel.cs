﻿using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

using Norma.Eta;
using Norma.Eta.Mvvm;
using Norma.Ipsilon.Models;

using Prism.Commands;

namespace Norma.Ipsilon.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ShellViewModel : ViewModel
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Notifier _notifier;

        public ShellViewModel()
        {
            _notifier = new Notifier().AddTo(this);
            _notifier.Start();
        }

        #region OpenTimetableCommand

        private ICommand _openTimetableCommand;

        public ICommand OpenTimetableCommand
            => _openTimetableCommand ?? (_openTimetableCommand = new DelegateCommand(OpenTimetable));

        private void OpenTimetable()
        {
            if (File.Exists(NormaConstants.IotaExecutableFile))
                Process.Start(NormaConstants.IotaExecutableFile);
        }

        #endregion

        #region ExitCommand

        private ICommand _exitCommand;
        public ICommand ExitCommand => _exitCommand ?? (_exitCommand = new DelegateCommand(Exit));

        private void Exit() => Application.Current.Shutdown();

        #endregion
    }
}