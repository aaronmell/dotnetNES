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

        public WriteableBitmap BackgroundPalettes { get; set; }

        public WriteableBitmap SpritePalettes { get; set; }
        
        public PatternsAndPalettesViewModel(Engine.Main.Engine engine)
            : this()
        {
            Engine = engine;
        }

        [PreferredConstructor]
        public PatternsAndPalettesViewModel()
        {
            PatternTable0 = new WriteableBitmap(128,128,1,1, PixelFormats.Bgr24, null);
            PatternTable1 = new WriteableBitmap(128, 128, 1, 1, PixelFormats.Bgr24, null);
            BackgroundPalettes = new WriteableBitmap(512, 32,1,1, PixelFormats.Bgr24, null);
            SpritePalettes = new WriteableBitmap(512, 32, 1, 1, PixelFormats.Bgr24, null);

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
            #region Left Pattern Table
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
            #endregion

            #region Right Pattern Table
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
            #endregion

            #region Background Palette
            var backgroundPalette = Engine.GetBackgroundPalette();

            BackgroundPalettes.Lock();
            bufferPtr = BackgroundPalettes.BackBuffer;

            unsafe
            {
                var pbuff = (byte*)bufferPtr.ToPointer();

                for (var i = 0; i < backgroundPalette.Length; i++)
                {
                    pbuff[i] = backgroundPalette[i];
                }
            }
            BackgroundPalettes.AddDirtyRect(new Int32Rect(0, 0, 512, 32));
            BackgroundPalettes.Unlock();
            RaisePropertyChanged("BackgroundPalettes");
            #endregion

            #region Sprite Palette
            var spritePalette = Engine.GetSpritePalette();
            SpritePalettes.Lock();
            bufferPtr = SpritePalettes.BackBuffer;

            unsafe
            {
                var pbuff = (byte*)bufferPtr.ToPointer();

                for (var i = 0; i < spritePalette.Length; i++)
                {
                    pbuff[i] = spritePalette[i];
                }
            }
            SpritePalettes.AddDirtyRect(new Int32Rect(0, 0, 512, 32));
            SpritePalettes.Unlock();
            RaisePropertyChanged("SpritePalettes");
            #endregion
        }
    }
}
