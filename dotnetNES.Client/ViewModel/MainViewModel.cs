using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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

        public WriteableBitmap Screen { get; set; }

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
            ResetNesCommand = new RelayCommand(() =>
            {
                _backgroundWorker.CancelAsync();
                Engine.Reset();
                _backgroundWorker.RunWorkerAsync();
            });
            PowerNesCommand = new RelayCommand(() =>
            {
                Engine = new Engine.Main.Engine(_fileName) {OnNewFrameAction = OnNewFrameAction};
            });
            OpenPatternsAndPalettesCommand = new RelayCommand(() => OpenDebugWindowWithEngine(MessageNames.OpenPatternsAndPalettes));
            OpenNameTablesCommand = new RelayCommand(() => OpenDebugWindowWithEngine(MessageNames.OpenNameTables));

            PauseCommand = new RelayCommand(PauseEngine);
            
            IsEnginePaused = false;
            RaisePropertyChanged("IsEnginePaused");

            _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true, WorkerReportsProgress = false };
            _backgroundWorker.DoWork += BackgroundWorkerDoWork;

            Screen = new WriteableBitmap(272, 240, 1, 1, PixelFormats.Bgr24, null);
            RaisePropertyChanged("Screen");
        }

        private void OpenDebugWindowWithEngine(string windowName)
        {
            Messenger.Default.Send(new NotificationMessage(windowName));
            Messenger.Default.Send(new NotificationMessage<Engine.Main.Engine>(Engine, MessageNames.LoadDebugWindow));
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            while (true)
            {
                if (worker != null && worker.CancellationPending || IsEnginePaused)
                {
                    e.Cancel = true;
                    return;
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

            if (_frameCount % 60 == 0)
            {
                Messenger.Default.Send(new NotificationMessage(MessageNames.UpdateDebugScreens));
            }

            if (Application.Current != null)
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(Refresh));
        }

        private unsafe void Refresh()
        {
            Screen.Lock();
            var nameTable0Ptr = Screen.BackBuffer;
            var locking = new object();
            lock (locking)
            {

                var screen = Engine.GetScreen();
                //var ptr = (byte*) nameTable0Ptr.ToPointer();

               
                var pbuff = (byte*) nameTable0Ptr.ToPointer();

                for (var i = 0; i < screen.Length; i++)
                {
                    pbuff[i] = screen[i];
                }
                
            }

            Screen.AddDirtyRect(new Int32Rect(0, 0, 272, 240));
            Screen.Unlock();
            RaisePropertyChanged("Screen");
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