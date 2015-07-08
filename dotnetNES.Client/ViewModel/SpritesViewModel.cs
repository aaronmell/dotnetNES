using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
namespace dotnetNES.Client.ViewModel
{
    public sealed class SpritesViewModel : DebuggingBaseViewModel
    {
        #region Public Properties
        public WriteableBitmap Sprite0 { get; set; }
        #endregion

        #region Private Methods
        protected override void LoadView(NotificationMessage<Engine.Main.Engine> obj)
        {
            if (obj.Notification != MessageNames.LoadDebugWindow)
            {
                return;
            }

            Engine = obj.Content;

            Sprite0 = new WriteableBitmap(128, 128, 1, 1, PixelFormats.Bgr24, null);
          
        }

        protected override unsafe void Refresh()
        {
        }
        #endregion


    }
}
