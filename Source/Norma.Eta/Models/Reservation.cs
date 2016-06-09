﻿using System;
using System.Collections.ObjectModel;
using System.Linq;

using Norma.Eta.Database;
using Norma.Eta.Models.Reservations;
using Norma.Gamma.Models;

namespace Norma.Eta.Models
{
    public class Reservation
    {
        private readonly ReservationDbContext _dbContext;
        private readonly object _lockObj = new object();

        public ReadOnlyCollection<RsvAll> Reservations
            => _dbContext.Reservations.Cast<RsvAll>().ToList().AsReadOnly();

        public ReadOnlyCollection<RsvProgram> RsvsByProgram
            => _dbContext.Reservations.Where(w => w.Type == nameof(RsvProgram)).ToList()
                         .Select(w => w.Cast<RsvProgram>()).ToList().AsReadOnly();

        public ReadOnlyCollection<RsvTime> RsvsByTime
            => _dbContext.Reservations.Where(w => w.Type == nameof(RsvTime)).ToList()
                         .Select(w => w.Cast<RsvTime>()).ToList().AsReadOnly();

        public ReadOnlyCollection<RsvKeyword> RsvsByKeyword
            => _dbContext.Reservations.Where(w => w.Type == nameof(RsvKeyword)).ToList()
                         .Select(w => w.Cast<RsvKeyword>()).ToList().AsReadOnly();

        public Reservation()
        {
            _dbContext = new ReservationDbContext();
            Init();
        }

        private void Init()
        {
            _dbContext.Reservations.Create();
            _dbContext.SaveChanges();
        }

        public void Save()
        {
            lock (_lockObj)
            {
                _dbContext.SaveChanges();
            }
        }

        private void Migrate()
        {
            // Not yet
        }

        #region Time

        /// <summary>
        ///     時間を対象とした視聴予約を追加します。
        /// </summary>
        /// <param name="time"></param>
        /// <param name="repetition"></param>
        /// <param name="range"></param>
        public void AddReservation(DateTime time, RepetitionType repetition, DateRange range)
        {
            _dbContext.Reservations.Add(RsvAll.Create(new RsvTime
            {
                StartTime = time,
                DayOfWeek = repetition,
                Range = range
            }));
            Save();
        }

        /// <summary>
        ///     対象の RsvTime を更新します。
        /// </summary>
        /// <param name="time"></param>
        public void UpdateReservation(RsvTime time)
        {
            var target = _dbContext.Reservations.Single(w => w.Id == time.Id);
            target.StartDate = time.StartTime;
            target.DayOfWeek = time.DayOfWeek;
            target.Range = time.Range;
            Save();
        }

        #endregion

        #region Keyword

        /// <summary>
        ///     キーワード及び正規表現を対象とした視聴予約を追加します。
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="isRegex"></param>
        /// <param name="range"></param>
        public void AddReservation(string keyword, bool isRegex, DateRange range)
        {
            _dbContext.Reservations.Add(RsvAll.Create(new RsvKeyword
            {
                Keyword = keyword,
                IsRegexMode = isRegex,
                Range = range
            }));
            Save();
        }

        /// <summary>
        ///     対象の RsvKeyword を更新します。
        /// </summary>
        /// <param name="keyword"></param>
        public void UpdateReservation(RsvKeyword keyword)
        {
            var target = _dbContext.Reservations.Single(w => w.Id == keyword.Id);
            target.IsRegexMode = keyword.IsRegexMode;
            target.Keyword = keyword.Keyword;
            target.Range = keyword.Range;
            Save();
        }

        #endregion

        #region Program

        /// <summary>
        ///     Slot を対象とした視聴予約を追加します。
        /// </summary>
        /// <param name="slot"></param>
        public void AddReservation(Slot slot)
        {
            _dbContext.Reservations.Add(RsvAll.Create(new RsvProgram
            {
                ProgramId = slot.Id,
                StartDate = slot.StartAt
            }));
            Save();
        }

        /// <summary>
        ///     対象の RsvProgram を更新します。
        /// </summary>
        /// <param name="program"></param>
        public void UpdateReservation(RsvProgram program)
        {
            var target = _dbContext.Reservations.Single(w => w.Id == program.Id);
            target.ProgramId = program.ProgramId;
            target.StartDate = program.StartDate;
            Save();
        }

        #endregion
    }
}