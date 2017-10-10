using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Messaging;

namespace dotnetNES.Client.ViewModel
{
    public sealed class PatternsAndPalettesViewModel : DebuggingBaseViewModel
    {
        #region Public Properties
       

        public WriteableBitmap PatternTable0 { get; set; }

        public WriteableBitmap PatternTable1 { get; set; }

        public WriteableBitmap BackgroundPalettes { get; set; }

        public WriteableBitmap SpritePalettes { get; set; }
        #endregion

        #region Protected Methods

        protected override void LoadView(NotificationMessage<Engine.Main.Engine> obj)
        {
            if (obj.Notification != MessageNames.LoadDebugWindow)
            {
                return;
            }

            if (Engine == null)
            {
                Engine = obj.Content;
            }

            PatternTable0 = new WriteableBitmap(128, 128, 1, 1, PixelFormats.Bgr24, null);
            PatternTable1 = new WriteableBitmap(128, 128, 1, 1, PixelFormats.Bgr24, null);
            BackgroundPalettes = new WriteableBitmap(512, 32, 1, 1, PixelFormats.Bgr24, null);
            SpritePalettes = new WriteableBitmap(512, 32, 1, 1, PixelFormats.Bgr24, null);
        }

        protected override unsafe void Refresh()
        {
            #region Left Pattern Table
            PatternTable0.Lock();
            var bufferPtr = PatternTable0.BackBuffer;

            Engine.DrawPatternTable0((byte*)bufferPtr.ToPointer());
           
            PatternTable0.AddDirtyRect(new Int32Rect(0, 0, 128, 128));
            PatternTable0.Unlock();
            RaisePropertyChanged("PatternTable0");
            #endregion

            #region Right Pattern Table
            

            PatternTable1.Lock();
            bufferPtr = PatternTable1.BackBuffer;
            Engine.DrawPatternTable1((byte*)bufferPtr.ToPointer());
           
            PatternTable1.AddDirtyRect(new Int32Rect(0, 0, 128, 128));
            PatternTable1.Unlock();
            RaisePropertyChanged("PatternTable1");
            #endregion

            #region Background Palette
         

            BackgroundPalettes.Lock();
            bufferPtr = BackgroundPalettes.BackBuffer;
            Engine.DrawBackgroundPalette((byte*)bufferPtr.ToPointer());
            
            BackgroundPalettes.AddDirtyRect(new Int32Rect(0, 0, 512, 32));
            BackgroundPalettes.Unlock();
            RaisePropertyChanged("BackgroundPalettes");
            #endregion

            #region Sprite Palette
            
            SpritePalettes.Lock();
            bufferPtr = SpritePalettes.BackBuffer;
            Engine.DrawSpritePalette((byte*)bufferPtr.ToPointer());
            
            SpritePalettes.AddDirtyRect(new Int32Rect(0, 0, 512, 32));
            SpritePalettes.Unlock();
            RaisePropertyChanged("SpritePalettes");
            #endregion
        }
        #endregion
    }
}
