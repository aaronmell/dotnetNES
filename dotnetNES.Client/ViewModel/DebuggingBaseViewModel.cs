using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;

namespace dotnetNES.Client.ViewModel
{
    public abstract class DebuggingBaseViewModel : ViewModelBase
    {
        public static Engine.Main.Engine Engine { get; set; }
        public RelayCommand<CancelEventArgs> WindowClosingCommand { get; }
        public RelayCommand<EventArgs> WindowOpeningCommand { get; }
        
        [PreferredConstructor]
        protected DebuggingBaseViewModel()
        {
            WindowClosingCommand = new RelayCommand<CancelEventArgs>((args) =>
            {
                Messenger.Default.Unregister(this);
            });

            WindowOpeningCommand = new RelayCommand<EventArgs>((args) =>
            {
                Messenger.Default.Register<NotificationMessage>(this, LoadView);
                Messenger.Default.Register<NotificationMessage>(this, RefreshScreen);
            });

        }

        protected virtual void LoadView(NotificationMessage notificationMessage)
        {

        }
        protected abstract void Refresh();

        private void RefreshScreen(NotificationMessage obj)
        {
            if (obj.Notification != MessageNames.UpdateDebugScreens || Engine == null)
                return;

            if (Application.Current != null)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Refresh));
        }
    }
}
