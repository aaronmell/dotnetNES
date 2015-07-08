using GalaSoft.MvvmLight.Messaging;

namespace dotnetNES.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);
        }

        private static void NotificationMessageReceived(NotificationMessage notificationMessage)
        {
            if (notificationMessage.Notification == MessageNames.OpenPatternsAndPalettes)
            {
                var patternsAndPalettes = new PatternsAndPalettes();
                
                patternsAndPalettes.Closing += (sender, args) => Messenger.Default.Unregister(patternsAndPalettes);
                patternsAndPalettes.Show(); 
            }

            if (notificationMessage.Notification == MessageNames.OpenNameTables)
            {
                var nameTables = new NameTables();

                nameTables.Closing += (sender, args) => Messenger.Default.Unregister(nameTables);
                nameTables.Show();
            }

            if (notificationMessage.Notification == MessageNames.OpenSprites)
            {
                var sprites = new Sprites();

                sprites.Closing += (sender, args) => Messenger.Default.Unregister(sprites);
                sprites.Show();
            }
        }
    }
}
