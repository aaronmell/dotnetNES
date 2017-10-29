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

        public ObservableCollection<Disassembly> Disassembly { get; set; }  
        public int SelectedValue { get; set; }       
        
        public CPUFlags CPUFlags { get; set; } = new CPUFlags();
        public PPUFlags PPUFlags { get; set; } = new PPUFlags();

        public ObservableCollection<BreakPoint> BreakPoints { get; set; } = new ObservableCollection<BreakPoint>();
        public ObservableCollection<BreakPointType> BreakPointTypes { get; set; } = new ObservableCollection<BreakPointType>
        {
            BreakPointType.Execute,
            BreakPointType.Read,
            BreakPointType.Write
        };

        public RelayCommand ContinueCommand { get; set; }
        public RelayCommand BreakCommand { get; set; }
        public RelayCommand StepCommand { get; set; }
        public RelayCommand RunOneScanlineCommand { get; set; }
        public RelayCommand RunOneFrameCommand { get; set; }

        public DebuggerViewModel()
        {
            ContinueCommand = new RelayCommand(() => 
            {
                SelectedValue = -1;
                RaisePropertyChanged(nameof(SelectedValue));
                Engine.UnPauseEngine();
            });
            BreakCommand = new RelayCommand(() => Engine.PauseEngine());
            StepCommand = new RelayCommand(() => 
            {
                Engine.PauseEngine();
                Engine.Step();
                UpdateAfterPause();
            });

            RunOneScanlineCommand = new RelayCommand(() => Engine.RuntoNextScanLine());
            RunOneFrameCommand = new RelayCommand(() => Engine.RuntoNextFrame());
        }

        private void UpdateAfterPause()
        {
            CPUFlags.UpdateFlags(Engine);
            RaisePropertyChanged(nameof(CPUFlags));

            PPUFlags.UpdateFlags(Engine);
            RaisePropertyChanged(nameof(PPUFlags));

            SelectedValue = CPUFlags.RawProgramCounter;
            RaisePropertyChanged(nameof(SelectedValue));            
        }        

        protected override void LoadView(NotificationMessage notificationMessage)
        {
            if (notificationMessage.Notification != MessageNames.LoadDebugWindow)
            {
                return;
            }

            Engine.EnginePaused += Engine_EnginePaused;

            RaisePropertyChanged(nameof(BreakPoints));
            RaisePropertyChanged(nameof(BreakPointTypes));
            Engine.BreakPoints = BreakPoints;

            Engine.PauseEngine();

            Disassembly = Engine.GetDisassembledMemory();
            RaisePropertyChanged(nameof(Disassembly));
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
