using dotnetNES.Engine.Main;
using dotnetNES.Engine.Models;
using dotnetNES.Engine.Utilities;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace dotnetNES.Client.ViewModel
{
    public class DebuggerViewModel : DebuggingBaseViewModel
    {
        public ObservableCollection<KeyValuePair<string,Disassembly>> Disassembly
        {
            get
            {
                if (Engine.GetDisassembledMemory() == null)
                {
                    Engine.EnableDisassembly();
                }
                return new ObservableCollection<KeyValuePair<string, Disassembly>>(Engine.GetDisassembledMemory().OrderBy(x => x.Key).ToList());
            }
        }
        private object _disassemblyLock = new object();

        public RelayCommand ContinueCommand { get; set; }
        public RelayCommand BreakCommand { get; set; }
        public RelayCommand StepCommand { get; set; }
        public RelayCommand RunOneScanlineCommand { get; set; }
        public RelayCommand RunOneFrameCommand { get; set; }

        public DebuggerViewModel()
        {
            ContinueCommand = new RelayCommand(() => Engine.UnPauseEngine());
            BreakCommand = new RelayCommand(() => Engine.PauseEngine());
            StepCommand = new RelayCommand(() => 
            {
                Engine.Step();
            });

            RunOneScanlineCommand = new RelayCommand(() => Engine.RuntoNextScanLine());
            RunOneFrameCommand = new RelayCommand(() => Engine.RuntoNextFrame());
        }

        protected override void LoadView(NotificationMessage notificationMessage)
        { 
            if (notificationMessage.Notification != MessageNames.LoadDebugWindow)
            {
                return;
            }           

            if (Engine.GetDisassembledMemory() == null)
            {
                Engine.EnableDisassembly();
            }

            Engine.EnginePaused += Engine_EnginePaused;

            BindingOperations.EnableCollectionSynchronization(Disassembly, _disassemblyLock);   
            RaisePropertyChanged("Disassembly");
        }

        private void Engine_EnginePaused(object sender, System.EventArgs e)
        {
           
        }

        public override void Cleanup()
        {
            if (Engine != null)
            {
                Engine.DisableDisassembly();
            }

            base.Cleanup();
        }

        protected override void Refresh()
        {
            //throw new System.NotImplementedException();
        }
    }
}
