using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Messaging;
namespace dotnetNES.Client.ViewModel
{
    public sealed class SpritesViewModel : DebuggingBaseViewModel
    {
        #region Public Properties
        public WriteableBitmap Sprite0 { get; set; }
        public WriteableBitmap Sprite1 { get; set; }
        public WriteableBitmap Sprite2 { get; set; }
        public WriteableBitmap Sprite3 { get; set; }
        public WriteableBitmap Sprite4 { get; set; }
        public WriteableBitmap Sprite5 { get; set; }
        public WriteableBitmap Sprite6 { get; set; }
        public WriteableBitmap Sprite7 { get; set; }
        public WriteableBitmap Sprite8 { get; set; }
        public WriteableBitmap Sprite9 { get; set; }
        public WriteableBitmap Sprite10 { get; set; }
        public WriteableBitmap Sprite11 { get; set; }
        public WriteableBitmap Sprite12 { get; set; }
        public WriteableBitmap Sprite13 { get; set; }
        public WriteableBitmap Sprite14 { get; set; }
        public WriteableBitmap Sprite15 { get; set; }
        public WriteableBitmap Sprite16 { get; set; }
        public WriteableBitmap Sprite17 { get; set; }
        public WriteableBitmap Sprite18 { get; set; }
        public WriteableBitmap Sprite19 { get; set; }
        public WriteableBitmap Sprite20 { get; set; }
        public WriteableBitmap Sprite21 { get; set; }
        public WriteableBitmap Sprite22 { get; set; }
        public WriteableBitmap Sprite23 { get; set; }
        public WriteableBitmap Sprite24 { get; set; }
        public WriteableBitmap Sprite25 { get; set; }
        public WriteableBitmap Sprite26 { get; set; }
        public WriteableBitmap Sprite27 { get; set; }
        public WriteableBitmap Sprite28 { get; set; }
        public WriteableBitmap Sprite29 { get; set; }
        public WriteableBitmap Sprite30 { get; set; }
        public WriteableBitmap Sprite31 { get; set; }
        public WriteableBitmap Sprite32 { get; set; }
        public WriteableBitmap Sprite33 { get; set; }
        public WriteableBitmap Sprite34 { get; set; }
        public WriteableBitmap Sprite35 { get; set; }
        public WriteableBitmap Sprite36 { get; set; }
        public WriteableBitmap Sprite37 { get; set; }
        public WriteableBitmap Sprite38 { get; set; }
        public WriteableBitmap Sprite39 { get; set; }
        public WriteableBitmap Sprite40 { get; set; }
        public WriteableBitmap Sprite41 { get; set; }
        public WriteableBitmap Sprite42 { get; set; }
        public WriteableBitmap Sprite43 { get; set; }
        public WriteableBitmap Sprite44 { get; set; }
        public WriteableBitmap Sprite45 { get; set; }
        public WriteableBitmap Sprite46 { get; set; }
        public WriteableBitmap Sprite47 { get; set; }
        public WriteableBitmap Sprite48 { get; set; }
        public WriteableBitmap Sprite49 { get; set; }
        public WriteableBitmap Sprite50 { get; set; }
        public WriteableBitmap Sprite51 { get; set; }
        public WriteableBitmap Sprite52 { get; set; }
        public WriteableBitmap Sprite53 { get; set; }
        public WriteableBitmap Sprite54 { get; set; }
        public WriteableBitmap Sprite55 { get; set; }
        public WriteableBitmap Sprite56 { get; set; }
        public WriteableBitmap Sprite57 { get; set; }
        public WriteableBitmap Sprite58 { get; set; }
        public WriteableBitmap Sprite59 { get; set; }
        public WriteableBitmap Sprite60 { get; set; }
        public WriteableBitmap Sprite61 { get; set; }
        public WriteableBitmap Sprite62 { get; set; }
        public WriteableBitmap Sprite63 { get; set; }
        #endregion

        #region Protected Methods
        protected override void LoadView(NotificationMessage<Engine.Main.Engine> obj)
        {
            if (obj.Notification != MessageNames.LoadDebugWindow)
            {
                return;
            }

            Engine = obj.Content;

            Sprite0 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite1 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite2 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite3 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite4 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite5 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite6 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite7 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite8 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite9 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite10 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite11 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite12 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite13 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite14 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite15 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite16 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite17 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite18 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite19 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite20 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite21 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite22 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite23 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite24 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite25 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite26 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite27 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite28 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite29 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite30 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite31 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite32 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite33 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite34 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite35 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite36 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite37 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite38 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite39 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite40 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite41 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite42 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite43 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite44 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite45 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite46 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite47 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite48 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite49 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite50 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite51 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite52 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite53 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite54 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite55 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite56 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite57 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite58 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite59 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite60 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite61 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite62 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
            Sprite63 = new WriteableBitmap(8, 8, 1, 1, PixelFormats.Bgr24, null);
        }

        protected override void Refresh()
        {
            DrawSprite(Sprite0, 0, "Sprite0");
            DrawSprite(Sprite1, 1, "Sprite1");
            DrawSprite(Sprite2, 2, "Sprite2");
            DrawSprite(Sprite3, 3, "Sprite3");
            DrawSprite(Sprite4, 4, "Sprite4");
            DrawSprite(Sprite5, 5, "Sprite5");
            DrawSprite(Sprite6, 6, "Sprite6");
            DrawSprite(Sprite7, 7, "Sprite7");
            DrawSprite(Sprite8, 8, "Sprite8");
            DrawSprite(Sprite9, 9, "Sprite9");
            DrawSprite(Sprite10, 10, "Sprite10");
            DrawSprite(Sprite11, 11, "Sprite11");
            DrawSprite(Sprite12, 12, "Sprite12");
            DrawSprite(Sprite13, 13, "Sprite13");
            DrawSprite(Sprite14, 14, "Sprite14");
            DrawSprite(Sprite15, 15, "Sprite15");
            DrawSprite(Sprite16, 16, "Sprite16");
            DrawSprite(Sprite17, 17, "Sprite17");
            DrawSprite(Sprite18, 18, "Sprite18");
            DrawSprite(Sprite19, 19, "Sprite19");
            DrawSprite(Sprite20, 20, "Sprite20");
            DrawSprite(Sprite21, 21, "Sprite21");
            DrawSprite(Sprite22, 22, "Sprite22");
            DrawSprite(Sprite23, 23, "Sprite23");
            DrawSprite(Sprite24, 24, "Sprite24");
            DrawSprite(Sprite25, 25, "Sprite25");
            DrawSprite(Sprite26, 26, "Sprite26");
            DrawSprite(Sprite27, 27, "Sprite27");
            DrawSprite(Sprite28, 28, "Sprite28");
            DrawSprite(Sprite29, 29, "Sprite29");
            DrawSprite(Sprite30, 30, "Sprite30");
            DrawSprite(Sprite31, 31, "Sprite31");
            DrawSprite(Sprite32, 32, "Sprite32");
            DrawSprite(Sprite33, 33, "Sprite33");
            DrawSprite(Sprite34, 34, "Sprite34");
            DrawSprite(Sprite35, 35, "Sprite35");
            DrawSprite(Sprite36, 36, "Sprite36");
            DrawSprite(Sprite37, 37, "Sprite37");
            DrawSprite(Sprite38, 38, "Sprite38");
            DrawSprite(Sprite39, 39, "Sprite39");
            DrawSprite(Sprite40, 40, "Sprite40");
            DrawSprite(Sprite41, 41, "Sprite41");
            DrawSprite(Sprite42, 42, "Sprite42");
            DrawSprite(Sprite43, 43, "Sprite43");
            DrawSprite(Sprite44, 44, "Sprite44");
            DrawSprite(Sprite45, 45, "Sprite45");
            DrawSprite(Sprite46, 46, "Sprite46");
            DrawSprite(Sprite47, 47, "Sprite47");
            DrawSprite(Sprite48, 48, "Sprite48");
            DrawSprite(Sprite49, 49, "Sprite49");
            DrawSprite(Sprite50, 50, "Sprite50");
            DrawSprite(Sprite51, 51, "Sprite51");
            DrawSprite(Sprite52, 52, "Sprite52");
            DrawSprite(Sprite53, 53, "Sprite53");
            DrawSprite(Sprite54, 54, "Sprite54");
            DrawSprite(Sprite55, 55, "Sprite55");
            DrawSprite(Sprite56, 56, "Sprite56");
            DrawSprite(Sprite57, 57, "Sprite57");
            DrawSprite(Sprite58, 58, "Sprite58");
            DrawSprite(Sprite59, 59, "Sprite59");
            DrawSprite(Sprite60, 60, "Sprite60");
            DrawSprite(Sprite61, 61, "Sprite61");
            DrawSprite(Sprite62, 62, "Sprite62");
            DrawSprite(Sprite63, 63, "Sprite63");
        }
        #endregion

        private unsafe void DrawSprite(WriteableBitmap sprite, int spriteNumber, string propertyName)
        {
            sprite.Lock();
            var bufferPtr = sprite.BackBuffer;

            Engine.DrawSprite((byte*)bufferPtr.ToPointer(), spriteNumber);

            sprite.AddDirtyRect(new Int32Rect(0, 0, 8, 8));
            sprite.Unlock();
            RaisePropertyChanged(propertyName);
        }
    }
}
