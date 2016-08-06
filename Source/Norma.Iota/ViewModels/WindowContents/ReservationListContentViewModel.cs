﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;

using Norma.Delta.Models;
using Norma.Delta.Services;
using Norma.Eta.Mvvm;
using Norma.Eta.Notifications;
using Norma.Eta.Properties;
using Norma.Iota.Models;

using Prism.Commands;
using Prism.Interactivity.InteractionRequest;

using Reactive.Bindings;

namespace Norma.Iota.ViewModels.WindowContents
{
    internal class ReservationListContentViewModel : InteractionViewModel<DataPassingNotification>
    {
        private readonly DatabaseService _databaseService;
        public string WindowTitle => Resources.RsvList;

        public ObservableCollection<ReservationItemViewModel> Reservations { get; }
        public ReactiveProperty<ReservationItemViewModel> SelectedItem { get; }
        public InteractionRequest<Confirmation> ConfirmationRequest { get; }

        public InteractionRequest<DataPassingNotification> EditRequest { get; }
        public InteractionRequest<DataPassingNotification> ConditionalReservationRequest { get; }

        public ReservationListContentViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Reservations = new ObservableCollection<ReservationItemViewModel>();
            SelectedItem = new ReactiveProperty<ReservationItemViewModel>();
            ConfirmationRequest = new InteractionRequest<Confirmation>();
            ConditionalReservationRequest = new InteractionRequest<DataPassingNotification>();
            EditRequest = new InteractionRequest<DataPassingNotification>();
            SelectedItem.Subscribe(w => ((DelegateCommand) EditReservationCommand).RaiseCanExecuteChanged()).AddTo(this);
            ViewModelHelper.Subscribe(this, w => w.Notification, w => UpdateRsvList());
        }

        private void UpdateRsvList()
        {
            Reservations.Clear();
            List<Reservation> reservations;
            using (var connection = _databaseService.Connect())
                reservations = connection.Reservations.Where(w => w.IsEnabled)
                                         .Include(w => w.KeywordReservation)
                                         .Include(w => w.SeriesReservation.Series.Episodes)
                                         .Include(w => w.SlotReservation.Slot)
                                         .Include(w => w.SlotReservation2)
                                         .Include(w => w.TimeReservation)
                                         .ToList();
            foreach (var reservation in reservations)
                Reservations.Add(new ReservationItemViewModel(new ReservationItem(reservation)));
        }

        #region RegisterReservationCommand

        private ICommand _registerReservationCommand;

        public ICommand RegisterReservationCommand
            => _registerReservationCommand ?? (_registerReservationCommand = new DelegateCommand(RegisterReservation));

        private void RegisterReservation()
        {
            ConditionalReservationRequest.Raise(new DataPassingNotification {Title = Resources.Register});
            UpdateRsvList();
        }

        #endregion

        #region EditReservationCommand

        private ICommand _editRsvCommand;

        public ICommand EditReservationCommand
            => _editRsvCommand ?? (_editRsvCommand = new DelegateCommand(EditReservation, CanEditReservation));

        private void EditReservation()
        {
            //await EditRequest.RaiseAsync(new DataPassingNotification {Model = SelectedItem.Value.Model});
            //UpdateRsvList();
        }

        private bool CanEditReservation() => SelectedItem.Value?.IsEditable ?? false;

        #endregion

        #region DeleteReservationCommand

        private ICommand _deleteRsvCommand;

        public ICommand DeleteReservationCommand
            => _deleteRsvCommand ?? (_deleteRsvCommand = new DelegateCommand(DeleteReservation));

        private void DeleteReservation()
        {
            SelectedItem.Value.Delete();
            UpdateRsvList();
        }

        #endregion
    }
}