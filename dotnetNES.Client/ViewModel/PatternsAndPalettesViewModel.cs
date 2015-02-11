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
    public class PatternsAndPalettesViewModel : ViewModelBase
    {
        
        public Engine.Main.Engine Engine { get; set; }

        public WriteableBitmap PatternTable0 { get; set; }

        public WriteableBitmap PatternTable1 { get; set; }

        public WriteableBitmap Palettes { get; set; }
        
        public PatternsAndPalettesViewModel(Engine.Main.Engine engine)
            : this()
        {
            Engine = engine;
        }

        [PreferredConstructor]
        public PatternsAndPalettesViewModel()
        {
            PatternTable0 = new WriteableBitmap(128,128,300,300, PixelFormats.Bgr24, null);
            PatternTable1 = new WriteableBitmap(128, 128, 1, 1, PixelFormats.Bgr24, null);
            Palettes = new WriteableBitmap(512,16,300,300, PixelFormats.Bgra32, null);

            Messenger.Default.Register<NotificationMessage>(this, RefreshScreen);
        }

        private void RefreshScreen(NotificationMessage obj)
        {
            if (obj.Notification != "UpdateDebugScreens" || Engine == null)
                return;

            if (Application.Current != null)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,new Action(Refresh)); 
        }

        private void Refresh()
        {
            var patternTable0 = Engine.GetPatternTable0();

            PatternTable0.Lock();
            var bufferPtr = PatternTable0.BackBuffer;

            unsafe
            {
                var pbuff = (byte*)bufferPtr.ToPointer();

                for (var i = 0; i < patternTable0.Length; i++)
                {
                    pbuff[i] = patternTable0[i];
                }
            }
            PatternTable0.AddDirtyRect(new Int32Rect(0, 0, 128, 128));
            PatternTable0.Unlock();
            RaisePropertyChanged("PatternTable0");


            var patternTable1 = Engine.GetPatternTable1();

            PatternTable1.Lock();
            bufferPtr = PatternTable1.BackBuffer;

            unsafe
            {
                var pbuff = (byte*)bufferPtr.ToPointer();

                for (var i = 0; i < patternTable1.Length; i++)
                {
                    pbuff[i] = patternTable1[i];
                }
            }
            PatternTable1.AddDirtyRect(new Int32Rect(0, 0, 128, 128));
            PatternTable1.Unlock();
            RaisePropertyChanged("PatternTable1");
        }
    }
}
