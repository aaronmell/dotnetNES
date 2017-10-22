using dotnetNES.Client.Models;
using dotnetNES.Engine.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System;

namespace dotnetNES.Client.ViewModel
{
    public class DebuggerViewModel : DebuggingBaseViewModel
    {
        private object _disassemblyLock = new object();

        public Dictionary<string,Disassembly> Disassembly { get; set; }  
        public CPUFlags CPUFlags { get; set; }

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
                UpdateAfterPause();
            });

            RunOneScanlineCommand = new RelayCommand(() => Engine.RuntoNextScanLine());
            RunOneFrameCommand = new RelayCommand(() => Engine.RuntoNextFrame());
        }

        private void UpdateAfterPause()
        {
            Disassembly = Engine.GetDisassembledMemory();
            BindingOperations.EnableCollectionSynchronization(Disassembly, _disassemblyLock);

            RaisePropertyChanged("Disassembly");

            CPUFlags.UpdateFlags(Engine);
            RaisePropertyChanged("CPUFlags");

        }

        protected override void LoadView(NotificationMessage notificationMessage)
        { 
            if (notificationMessage.Notification != MessageNames.LoadDebugWindow)
            {
                return;
            }

            Engine.EnginePaused += Engine_EnginePaused;
                        
            
            CPUFlags = new CPUFlags();

            UpdateAfterPause();           
        }

        private void Engine_EnginePaused(object sender, System.EventArgs e)
        {
            UpdateAfterPause();
        }        

        protected override void Refresh()
        {
            //throw new System.NotImplementedException();
        }
    }
}
