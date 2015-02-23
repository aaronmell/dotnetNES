using System.ComponentModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;

namespace dotnetNES.Client.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public sealed class MainViewModel : ViewModelBase
    {
        private string _fileName;
        private readonly BackgroundWorker _backgroundWorker;
        private int _frameCount;

        public Engine.Main.Engine Engine { get; set; }

        public RelayCommand LoadFileCommand { get; set; }

        public RelayCommand ResetNesCommand { get; set; }

        public RelayCommand PowerNesCommand { get; set; }

        public RelayCommand OpenPatternsAndPalettesCommand { get; set; }

        public RelayCommand OpenNameTablesCommand { get; set; }

        public RelayCommand PauseCommand { get; set; }

        public bool IsCartridgeLoaded { get; set; }
        public bool IsEnginePaused { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            LoadFileCommand = new RelayCommand(LoadFile);
            ResetNesCommand = new RelayCommand(() => Engine.Reset());
            PowerNesCommand = new RelayCommand(() =>
            {
                Engine = new Engine.Main.Engine(_fileName) {OnNewFrameAction = OnNewFrameAction};
            });
            OpenPatternsAndPalettesCommand =
                new RelayCommand(
                    () =>
                        Messenger.Default.Send(new NotificationMessage<Engine.Main.Engine>(Engine,
                            "OpenPatternsAndPalettes")));

            OpenNameTablesCommand =
                new RelayCommand(
                    () => Messenger.Default.Send(new NotificationMessage<Engine.Main.Engine>(Engine, "OpenNameTables")));

            PauseCommand = new RelayCommand(PauseEngine);
            
            IsEnginePaused = false;
            RaisePropertyChanged("IsEnginePaused");

            _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true, WorkerReportsProgress = false };
            _backgroundWorker.DoWork += BackgroundWorkerDoWork;
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            while (true)
            {
                if (worker != null && worker.CancellationPending || IsEnginePaused)
                {
                    e.Cancel = true;
                    break;
                }
                Engine.Step();
            }
         }

        private void LoadFile()
        {
            IsEnginePaused = true;
            RaisePropertyChanged("IsEnginePaused");

            var dlg = new OpenFileDialog {DefaultExt = ".nes", Filter = "NES Roms (*.nes)|*.nes"};

            if (dlg.ShowDialog() != true)
            {
                IsEnginePaused = false;
                RaisePropertyChanged("IsEnginePaused");
                return;
            }

            _fileName = dlg.FileName;
            Engine = new Engine.Main.Engine(_fileName) {OnNewFrameAction = OnNewFrameAction};
            
            IsCartridgeLoaded = true;
            RaisePropertyChanged("IsCartridgeLoaded");

            _backgroundWorker.CancelAsync();

            while (_backgroundWorker.IsBusy)
            {

            }

            IsEnginePaused = false;
            RaisePropertyChanged("IsEnginePaused");

            _backgroundWorker.RunWorkerAsync();
        }

        private void OnNewFrameAction()
        {
            _frameCount++;

            if (_frameCount % 180 == 0)
            {
                Messenger.Default.Send(new NotificationMessage("UpdateDebugScreens"));
            }
        }

        private void PauseEngine()
        {
                if (IsEnginePaused) 
                    _backgroundWorker.RunWorkerAsync();
                else
                {
                    _backgroundWorker.CancelAsync();
                }

            IsEnginePaused = !IsEnginePaused;
            RaisePropertyChanged("IsEnginePaused");
        }
    }
}