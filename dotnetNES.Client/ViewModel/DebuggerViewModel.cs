using dotnetNES.Client.Models;
using dotnetNES.Engine.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace dotnetNES.Client.ViewModel
{
    public class DebuggerViewModel : DebuggingBaseViewModel
    {
        private object _disassemblyLock = new object();

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
                CPUFlags.UpdateFlags(Engine);
                RaisePropertyChanged("CPUFlags");
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

            CPUFlags = new CPUFlags();
            CPUFlags.UpdateFlags(Engine);
            RaisePropertyChanged("CPUFlags");
        }

        private void Engine_EnginePaused(object sender, System.EventArgs e)
        {
            CPUFlags.UpdateFlags(Engine);
            RaisePropertyChanged("CPUFlags");
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
