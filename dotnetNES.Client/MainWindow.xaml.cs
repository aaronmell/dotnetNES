using dotnetNES.Client.ViewModel;
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
            Messenger.Default.Register<NotificationMessage<Engine.Main.Engine>>(this, NotificationMessageReceived);
        }

        private static void NotificationMessageReceived(NotificationMessage<Engine.Main.Engine> notificationMessage)
        {
            if (notificationMessage.Notification == "OpenPatternsAndPalettes")
            {
                var patternsAndPalettes = new PatternsAndPalettes {DataContext = new PatternsAndPalettesViewModel(notificationMessage.Content)};

                patternsAndPalettes.Closing += (sender, args) => Messenger.Default.Unregister(patternsAndPalettes);
                patternsAndPalettes.Show(); 
            }

            if (notificationMessage.Notification == "OpenNameTables")
            {
                var nameTables = new NameTables { DataContext = new NameTablesViewModel(notificationMessage.Content) };

                nameTables.Closing += (sender, args) => Messenger.Default.Unregister(nameTables);
                nameTables.Show();
            }
        }
    }
}
