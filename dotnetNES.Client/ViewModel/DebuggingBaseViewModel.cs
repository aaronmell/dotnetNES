using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;

namespace dotnetNES.Client.ViewModel
{
    public abstract class DebuggingBaseViewModel : ViewModelBase
    {
        public Engine.Main.Engine Engine { get; set; }

        //protected DebuggingBaseViewModel(Engine.Main.Engine engine)
        //    : this()
        //{
        //    Engine = engine;
        //}

        [PreferredConstructor]
        protected DebuggingBaseViewModel()
        {
            Messenger.Default.Register<NotificationMessage<Engine.Main.Engine>>(this, LoadView);
            Messenger.Default.Register<NotificationMessage>(this, RefreshScreen);
        }

        protected abstract void LoadView(NotificationMessage<Engine.Main.Engine> engine);
        protected abstract unsafe void Refresh();

        private void RefreshScreen(NotificationMessage obj)
        {
            if (obj.Notification != MessageNames.UpdateDebugScreens || Engine == null)
                return;

            if (Application.Current != null)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Refresh));
        }
    }
}
