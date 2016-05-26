﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

using Norma.Eta.Models;
using Norma.Eta.Mvvm;
using Norma.Iota.ViewModels.Controls;

namespace Norma.Iota.ViewModels
{
    internal class ShellViewModel : ViewModel
    {
        private readonly Timetable _timetable;
        private int _index; // 日付管理用(0 = 今日, 6 = 一週間後みたいな)
        public ObservableCollection<ChannelCellViewModel> Channels { get; }
        public List<string> AvailableDates { get; }

        public ShellViewModel(Timetable timetable)
        {
            _timetable = timetable;
            _index = (DateTime.Now - timetable.LastSyncTime).Days;
            AvailableDates = new List<string>();
            Channels = new ObservableCollection<ChannelCellViewModel>();
            for (var i = 0; i < 7; i++)
                AvailableDates.Add(timetable.LastSyncTime.AddDays(i).ToString("MM/dd"));

            SelectedDate = AvailableDates[0];
        }

        private void UpdateChannels()
        {
            Application.Current.Dispatcher.Invoke(() => Channels.Clear());
            var list = new List<ChannelCellViewModel>();
            foreach (var channel in _timetable.Channels)
            {
                var slots = _timetable.ChannelSchedules.Where(w => w.ChannelId == channel.Id).ElementAt(_index);
                var vm = new ChannelCellViewModel(channel, slots.Slots, slots.Date).AddTo(this);
                list.Add(vm);
            }
            foreach (var vm in list)
                Application.Current.Dispatcher.Invoke(() => Channels.Add(vm));
            IsLoading = false;
        }

        #region SelectedDate

        private string _selectedDate;

        public string SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                if (!SetProperty(ref _selectedDate, value))
                    return;
                _index = AvailableDates.IndexOf(_selectedDate);
                IsLoading = true;
                Observable.Return(0).Delay(TimeSpan.FromSeconds(1)).Subscribe(w => UpdateChannels());
            }
        }

        #endregion

        #region IsLoading

        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        #endregion
    }
}