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
        #region Private Fields
        private string _fileName;
        private double _frameCount;
        #endregion

        #region Public Properties
        
        public static Engine.Main.Engine Engine { get; set; }
        public bool IsCartridgeLoaded { get; set; }
        public bool IsEnginePaused
        {
            get
            {
                if (Engine == null)
                {
                    return true;
                }

                return Engine.IsPaused;                  
            }
        }
        public WriteableBitmap Screen { get; set; }
        #endregion

        #region Command Properties
        public RelayCommand LoadFileCommand { get; set; }

        public RelayCommand ResetNesCommand { get; set; }

        public RelayCommand PowerNesCommand { get; set; }

        public RelayCommand OpenPatternsAndPalettesCommand { get; set; }

        public RelayCommand OpenNameTablesCommand { get; set; }

        public RelayCommand OpenSpritesCommand { get; set; }

        public RelayCommand OpenDebuggerCommand { get; set; }

        public RelayCommand PauseCommand { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            LoadFileCommand = new RelayCommand(LoadFile);
            ResetNesCommand = new RelayCommand(() =>
            {
                Engine.BeginReset();
                RaisePropertyChanged(nameof(IsEnginePaused));
            });
            PowerNesCommand = new RelayCommand(() =>
            {
                Engine.BeginPower();
                RaisePropertyChanged(nameof(IsEnginePaused));
            });
            OpenPatternsAndPalettesCommand = new RelayCommand(() => OpenDebugWindowWithEngine(MessageNames.OpenPatternsAndPalettes));
            OpenNameTablesCommand = new RelayCommand(() => OpenDebugWindowWithEngine(MessageNames.OpenNameTables));
            OpenSpritesCommand = new RelayCommand(() => OpenDebugWindowWithEngine(MessageNames.OpenSprites));
            OpenDebuggerCommand = new RelayCommand(() => OpenDebugWindowWithEngine(MessageNames.OpenDebugger));

            PauseCommand = new RelayCommand(PauseEngine);

            RaisePropertyChanged(nameof(IsEnginePaused));

            Screen = new WriteableBitmap(272, 240, 1, 1, PixelFormats.Bgr24, null);
            RaisePropertyChanged(nameof(Screen));            
        }        
        #endregion

        #region Private Methods
        private void OpenDebugWindowWithEngine(string windowName)
        {
            Messenger.Default.Send(new NotificationMessage(windowName));
            Messenger.Default.Send(new NotificationMessage(MessageNames.LoadDebugWindow));
        }       

        private void LoadFile()
        {
            if (Engine != null)
            {
                Engine.PauseEngine();
                RaisePropertyChanged(nameof(IsEnginePaused));                
            }

            var dlg = new OpenFileDialog {DefaultExt = ".nes", Filter = "NES Roms (*.nes)|*.nes"};

            if (dlg.ShowDialog() != true)
            {
                if (Engine != null)
                {
                    Engine.UnPauseEngine();
                }
                return;
            }

            _fileName = dlg.FileName;
            Engine = new Engine.Main.Engine(_fileName) {OnNewFrameAction = OnNewFrameAction};
            Engine.EngineUnPaused += Engine_OnEngineUnPaused;
            Engine.OnEnginePaused += Engine_OnEnginePaused;
            DebuggerViewModel.Engine = Engine;
            
            IsCartridgeLoaded = true;
            RaisePropertyChanged(nameof(IsCartridgeLoaded));

            Engine.BeginPower();
            Engine.UnPauseEngine();
        }

        private void Engine_OnEnginePaused(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(IsEnginePaused));
        }

        private void Engine_OnEngineUnPaused(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(IsEnginePaused));
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
               
                var pbuff = (byte*) nameTable0Ptr.ToPointer();

                for (var i = 0; i < screen.Length; i++)
                {
                    pbuff[i] = screen[i];
                }
            }

            Screen.AddDirtyRect(new Int32Rect(0, 0, 272, 240));
            Screen.Unlock();
            RaisePropertyChanged(nameof(Screen));
        }

        private void PauseEngine()
        {
            if (Engine.IsPaused)
                Engine.UnPauseEngine();
            else
            {
                Engine.PauseEngine();
            }
        }
        #endregion
    }
}