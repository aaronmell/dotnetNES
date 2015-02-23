using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;

namespace dotnetNES.Client.ViewModel
{
    public class NameTablesViewModel : ViewModelBase
    {
        public Engine.Main.Engine Engine { get; set; }

        public WriteableBitmap NameTable { get; set; }

        public NameTablesViewModel(Engine.Main.Engine engine)
            : this()
        {
            Engine = engine;
        }

        [PreferredConstructor]
        public NameTablesViewModel()
        {
            NameTable = new WriteableBitmap(256, 240, 1, 1, PixelFormats.Bgr24, null);
            Messenger.Default.Register<NotificationMessage>(this, RefreshScreen);
        }

        private void RefreshScreen(NotificationMessage obj)
        {
            if (obj.Notification != "UpdateDebugScreens" || Engine == null)
                return;

            if (Application.Current != null)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Refresh));
        }

        private void Refresh()
        {
            var nameTables = Engine.GetNameTables();

            NameTable.Lock();
            var bufferPtr = NameTable.BackBuffer;

            unsafe
            {
                var pbuff = (byte*)bufferPtr.ToPointer();

                for (var i = 0; i < nameTables.Length; i++)
                {
                    pbuff[i] = nameTables[i];
                }
            }
            NameTable.AddDirtyRect(new Int32Rect(0, 0, 256, 240));
            NameTable.Unlock();
            RaisePropertyChanged("NameTable");
        }
    }
}
