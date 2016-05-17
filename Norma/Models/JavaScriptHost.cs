﻿using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;

using CefSharp;
using CefSharp.Wpf;

using Prism.Mvvm;

namespace Norma.Models
{
    // References: https://github.com/nakayuki805/AbemaTVChromeExtension
    // Not support features are:
    //  * CM related features.
    //  * Comment related features.
    internal class JavaScriptHost : BindableBase, IDisposable
    {
        private readonly AbemaState _abemaState;
        private readonly Configuration _configuration;
        private readonly IWpfWebBrowser _wpfWebBrowser;
        private IDisposable _disposable;

        public string Address { get; set; }

        public JavaScriptHost(IWpfWebBrowser wpfWebBrowser, AbemaState abemaState, Configuration configuration)
        {
            _wpfWebBrowser = wpfWebBrowser;
            _abemaState = abemaState;
            _configuration = configuration;
            Address = "";
            _wpfWebBrowser.ConsoleMessage += (sender, e) => Debug.WriteLine("[Chromium]" + e.Message);
            _wpfWebBrowser.FrameLoadStart += (sender, e) => _disposable?.Dispose();
            _wpfWebBrowser.FrameLoadEnd += (sender, e) =>
            {
                if (!Address.StartsWith("https://abema.tv/now-on-air/"))
                    return;
                Run();
                Observable.Return(1).Delay(TimeSpan.FromSeconds(1)).Subscribe(w => RunLater());
            };
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        #endregion

        private void Run()
        {
            if (_configuration.Root.Browser.DisableChangeChannelByMouseWheel)
                DisableChangeChannelByMouseScroll();
            DisableContextMenu();
            if (_configuration.Root.Browser.HiddenHeaderControls)
                HideTvContainerHeader();
            if (_configuration.Root.Browser.HiddenFooterControls)
                HideTvContainerFooter();
            if (_configuration.Root.Browser.HiddenSideControls)
                HideTvContainerSide();
        }

        private void RunLater()
        {
            // 1秒遅らせている都合上、 null になることがある
            if (_wpfWebBrowser == null)
                return;

            _disposable?.Dispose();

            var val = _configuration.Root.Operation.SamplingIntervalOfProgramState;
            _disposable = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(val)).Subscribe(w => GetIsBroadcastCm());
        }

        private void DisableChangeChannelByMouseScroll()
        {
            const string jsCode = @"
window.addEventListener('mousewheel', function(e) {
  e.stopImmediatePropagation();
}, true);
";
            StatusInfo.Instance.Text = "Disable change channel by mouse wheel.";
            WrapExecuteScriptAsync(jsCode);
        }

        private void DisableContextMenu()
        {
            const string jsCode = @"
window.addEventListener('contextmenu', function(e) {
  e.preventDefault();
}, true);
";
            StatusInfo.Instance.Text = "Disable context menu.";
            WrapExecuteScriptAsync(jsCode);
        }

        private void HideTvContainerHeader()
        {
            const string jsCode = @"
function cs_HideTvContainerHeader() {
  var appContainerHeader = window.document.querySelector('[class^=""AppContainer__header-container___""]');
  if (appContainerHeader == null) {
    return;
  }
  appContainerHeader.style.display = 'none';
};
setTimeout(cs_HideTvContainerHeader, 500);
";
            StatusInfo.Instance.Text = "Hide container headers.";
            WrapExecuteScriptAsync(jsCode);
        }

        private void HideTvContainerFooter()
        {
            const string jsCode = @"
function cs_HideTvContainerFooter() {
  var appContainerFooter = window.document.querySelector('[class^=""TVContainer__footer-container___""]');
  if (appContainerFooter == null) {
    return;
  }
  appContainerFooter.style.display = 'none';
};
setTimeout(cs_HideTvContainerFooter, 500);
";
            StatusInfo.Instance.Text = "Hide container footers.";
            WrapExecuteScriptAsync(jsCode);
        }

        private void HideTvContainerSide()
        {
            const string jsCode = @"
function cs_HideTvContainerSide() {
  var appContainerSide = window.document.querySelector('[class^=""TVContainer__side___""]');
  if (appContainerSide == null) {
    return;
  }
  appContainerSide.style.display = 'none';
};
setTimeout(cs_HideTvContainerSide, 500);
";
            StatusInfo.Instance.Text = "Hide container sides.";
            WrapExecuteScriptAsync(jsCode);
        }

        private void GetIsBroadcastCm()
        {
            const string jsCode = @"
(function () {
  var appContainerHeading = window.document.querySelector('[class^=""style__heading2___""]');
  if (appContainerHeading == null) {
    return true;
  }
  return false;
})();
";
            var task = WrapEvaluateScriptAsync(jsCode);
            task.ContinueWith(w =>
            {
                if (w.IsFaulted)
                    return;
                var response = task.Result;
                var oldState = _abemaState.IsBroadcastCm;
                if (!response.Success || bool.Parse(response.Result.ToString()))
                    _abemaState.IsBroadcastCm = true;
                else
                    _abemaState.IsBroadcastCm = false;
                // ここですべきじゃない気がする
                if (!_configuration.Root.Browser.ReloadPageOnBroadcastCommercials)
                    return;
                if (_abemaState.IsBroadcastCm && oldState != _abemaState.IsBroadcastCm)
                    _wpfWebBrowser.Reload();
            }, TaskScheduler.Default);
        }

        #region Wrap IWpfWebBrowser Js Executor

        private void WrapExecuteScriptAsync(string jsCode)
        {
            try
            {
                _wpfWebBrowser.ExecuteScriptAsync(jsCode);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private Task<JavascriptResponse> WrapEvaluateScriptAsync(string jsCode)
        {
            try
            {
                return _wpfWebBrowser.EvaluateScriptAsync(jsCode, null);
            }
            catch (Exception e)
            {
                var completionSource = new TaskCompletionSource<JavascriptResponse>();
                completionSource.SetException(e);
                return completionSource.Task;
            }
        }

        #endregion
    }
}