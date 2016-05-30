﻿using Norma.Eta.Mvvm;
using Norma.Models;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Norma.ViewModels.Controls
{
    internal class AbemaStatusViewModel : ViewModel
    {
        public ReadOnlyReactiveProperty<string> Text { get; }

        public AbemaStatusViewModel()
        {
            // ??
            Text = StatusInfo.Instance.ObserveProperty(w => w.Text).ToReadOnlyReactiveProperty().AddTo(this);
        }
    }
}