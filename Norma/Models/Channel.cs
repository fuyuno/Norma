﻿using System;
using System.Reactive.Linq;

using Prism.Mvvm;

namespace Norma.Models
{
    internal class Channel : BindableBase, IDisposable
    {
        private readonly IDisposable _disposable;
        public AbemaChannel ChannelType { get; }
        public string LogoUrl { get; private set; }

        public Channel(AbemaChannel channel, Configuration configuration)
        {
            ChannelType = channel;
            LogoUrl = $"https://hayabusa.io/abema/channels/logo/{ChannelType.ToUrlString()}.w120.png";

            // 1分毎にサムネ更新
            var val = configuration.Root.Operation.UpdateIntervalOfThumbnails;
            _disposable = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(val)).Subscribe(w => UpdateThumbnail());
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        #endregion

        private void UpdateThumbnail()
        {
            var channel = ChannelType.ToUrlString();
            var date = DateTime.Now;
            if (date.Second % 10 != 0)
                date = date.AddSeconds(-(date.Second % 10)); // サムネイルが10秒に発行されるので、N % 10 == 0秒に修正する
            var time = date.ToString("yyyyMMddHHmmss");
            ThumbnailUrl = $"https://hayabusa.io/abema/channels/time/{time}/{channel}.w132.h75.png";
            StatusInfo.Instance.Text = "Reload channel thumbnails.";
        }

        #region ThumbnailUrl

        private string _thumbnailUrl;

        public string ThumbnailUrl
        {
            get { return _thumbnailUrl; }
            set { SetProperty(ref _thumbnailUrl, value); }
        }

        #endregion
    }
}