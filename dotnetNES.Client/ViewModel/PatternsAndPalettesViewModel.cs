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
            Messenger.Default.Register<NotificationMessage<Engine.Main.Engine>>(this, LoadView);
            Messenger.Default.Register<NotificationMessage>(this, RefreshScreen);
        }

        private void LoadView(NotificationMessage<Engine.Main.Engine> obj)
        {
            if (obj.Notification != MessageNames.LoadDebugWindow)
            {
                return;
            }

            Engine = obj.Content;

            PatternTable0 = new WriteableBitmap(128, 128, 1, 1, PixelFormats.Bgr24, null);
            PatternTable1 = new WriteableBitmap(128, 128, 1, 1, PixelFormats.Bgr24, null);
            BackgroundPalettes = new WriteableBitmap(512, 32, 1, 1, PixelFormats.Bgr24, null);
            SpritePalettes = new WriteableBitmap(512, 32, 1, 1, PixelFormats.Bgr24, null);
        }

        private void RefreshScreen(NotificationMessage obj)
        {
            if (obj.Notification != MessageNames.UpdateDebugScreens || Engine == null)
                return;

            if (Application.Current != null)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,new Action(Refresh)); 
        }

        private unsafe void Refresh()
        {
            #region Left Pattern Table
           

            PatternTable0.Lock();
            var bufferPtr = PatternTable0.BackBuffer;
           
            Engine.SetPatternTable0((byte*)bufferPtr.ToPointer());
           
            PatternTable0.AddDirtyRect(new Int32Rect(0, 0, 128, 128));
            PatternTable0.Unlock();
            RaisePropertyChanged("PatternTable0");
            #endregion

            #region Right Pattern Table
            

            PatternTable1.Lock();
            bufferPtr = PatternTable1.BackBuffer;
            Engine.SetPatternTable1((byte*)bufferPtr.ToPointer());
           
            PatternTable1.AddDirtyRect(new Int32Rect(0, 0, 128, 128));
            PatternTable1.Unlock();
            RaisePropertyChanged("PatternTable1");
            #endregion

            #region Background Palette
         

            BackgroundPalettes.Lock();
            bufferPtr = BackgroundPalettes.BackBuffer;
            Engine.SetBackgroundPalette((byte*)bufferPtr.ToPointer());
            
            BackgroundPalettes.AddDirtyRect(new Int32Rect(0, 0, 512, 32));
            BackgroundPalettes.Unlock();
            RaisePropertyChanged("BackgroundPalettes");
            #endregion

            #region Sprite Palette
            
            SpritePalettes.Lock();
            bufferPtr = SpritePalettes.BackBuffer;
            Engine.SetSpritePalette((byte*)bufferPtr.ToPointer());
            
            SpritePalettes.AddDirtyRect(new Int32Rect(0, 0, 512, 32));
            SpritePalettes.Unlock();
            RaisePropertyChanged("SpritePalettes");
            #endregion
        }
    }
}
