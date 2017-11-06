using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Messaging;

namespace dotnetNES.Client.ViewModel
{
    /// <summary>
    /// The view model for the NameTables View
    /// </summary>
    public sealed class NameTablesViewModel : DebuggingBaseViewModel
    {
        #region Public Properties

        public WriteableBitmap NameTable0 { get; set; } = new WriteableBitmap(256, 240, 1, 1, PixelFormats.Bgr24, null);
        public WriteableBitmap NameTable1 { get; set; }
        public WriteableBitmap NameTable2 { get; set; }
        public WriteableBitmap NameTable3 { get; set; }
        #endregion

        #region Protected Methods
        protected override void LoadView(NotificationMessage obj)
        {
            if (obj.Notification != MessageNames.LoadDebugWindow)
            {
                return;
            }  

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
        
        protected override unsafe void Refresh()
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

            RaisePropertyChanged(nameof(NameTable0));
            RaisePropertyChanged(nameof(NameTable1));
            RaisePropertyChanged(nameof(NameTable2));
            RaisePropertyChanged(nameof(NameTable3));
        }
        #endregion
    }
}
