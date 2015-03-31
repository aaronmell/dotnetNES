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
    /// <summary>
    /// The view model for the NameTables View
    /// </summary>
    public sealed class NameTablesViewModel : ViewModelBase
    {
        #region Public Properties
        public Engine.Main.Engine Engine { get; set; }

        public WriteableBitmap NameTable0 { get; set; }
        public WriteableBitmap NameTable1 { get; set; }
        public WriteableBitmap NameTable2 { get; set; }
        public WriteableBitmap NameTable3 { get; set; }
        #endregion

        #region Constructors
        public NameTablesViewModel(Engine.Main.Engine engine)
            : this()
        {
            Engine = engine;
        }

        [PreferredConstructor]
        public NameTablesViewModel()
        {
            Messenger.Default.Register<NotificationMessage<Engine.Main.Engine>>(this, LoadView);
            Messenger.Default.Register<NotificationMessage>(this, RefreshScreen);
        }
        #endregion

        #region Private Methods
        private void LoadView(NotificationMessage<Engine.Main.Engine> obj)
        {
            if (obj.Notification != MessageNames.LoadDebugWindow)
            {
                return;
            }

            Engine = obj.Content;
            NameTable0 = new WriteableBitmap(256, 240, 1, 1, PixelFormats.Bgr24, null);

            if (Engine.IsVerticalMirroringEnabled)
            {
                NameTable2 = NameTable0;
                NameTable1 = new WriteableBitmap(256, 240, 1, 1, PixelFormats.Bgr24, null);
                NameTable3 = NameTable1;
            }
			else
            {
                NameTable1 = NameTable0;
                NameTable2 = new WriteableBitmap(256, 240, 1, 1, PixelFormats.Bgr24, null);
                NameTable3 = NameTable2;
            }
        }

        private void RefreshScreen(NotificationMessage obj)
        {
            if (obj.Notification != MessageNames.UpdateDebugScreens || Engine == null)
                return;

            if (Application.Current != null)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Refresh));
        }

        private unsafe void Refresh()
        {
            NameTable0.Lock();
            var nameTable0Ptr = NameTable0.BackBuffer;

            Engine.DrawNameTable((byte*)nameTable0Ptr.ToPointer(),0);
           
            NameTable0.AddDirtyRect(new Int32Rect(0, 0, 256, 240));
            NameTable0.Unlock();
           

            if (Engine.IsVerticalMirroringEnabled)
            {
               
                NameTable1.Lock();
                var nameTable1Ptr = NameTable1.BackBuffer;

                Engine.DrawNameTable((byte*)nameTable1Ptr.ToPointer(), 1);

                NameTable1.AddDirtyRect(new Int32Rect(0, 0, 256, 240));
                NameTable1.Unlock();
                
            }
            else
            {
                NameTable2.Lock();
                var nameTable2Ptr = NameTable2.BackBuffer;

                Engine.DrawNameTable((byte*)nameTable2Ptr.ToPointer(), 2);

                NameTable2.AddDirtyRect(new Int32Rect(0, 0, 256, 240));
                NameTable2.Unlock();
            }

            RaisePropertyChanged("NameTable0");
            RaisePropertyChanged("NameTable1");
            RaisePropertyChanged("NameTable2");
            RaisePropertyChanged("NameTable3");
        }
        #endregion
    }
}
