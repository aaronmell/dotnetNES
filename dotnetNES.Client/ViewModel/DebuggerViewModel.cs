using System;
using dotnetNES.Client.Models;
using dotnetNES.Engine.Models;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.ObjectModel;

namespace dotnetNES.Client.ViewModel
{
    public class DebuggerViewModel : DebuggingBaseViewModel
    {
        private bool _isrunningToNext;

        public ObservableCollection<Disassembly> Disassembly { get; set; }  
        public int SelectedValue { get; set; }       
        
        public CPUFlags CPUFlags { get; set; } = new CPUFlags();
        public PPUFlags PPUFlags { get; set; } = new PPUFlags();

        public ObservableCollection<BreakPoint> BreakPoints { get; set; } = new ObservableCollection<BreakPoint>();
        public ObservableCollection<BreakPointType> BreakPointTypes { get; set; } = new ObservableCollection<BreakPointType>
        {
            BreakPointType.Execute,
            //BreakPointType.Read,
            //BreakPointType.Write
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

            RunOneScanlineCommand = new RelayCommand(() =>
            {
                if (!_isrunningToNext)
                {
                    _isrunningToNext = true;
                    Engine.RuntoNextScanLine();
                }
            });
            RunOneFrameCommand = new RelayCommand(() =>
            {
                if (!_isrunningToNext)
                {
                    _isrunningToNext = true;
                    Engine.RuntoNextFrame();
                }
     
            });
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

            Engine.OnEnginePaused += Engine_OnEnginePaused;

            RaisePropertyChanged(nameof(BreakPoints));
            RaisePropertyChanged(nameof(BreakPointTypes));
            Engine.BreakPoints = BreakPoints;

            Engine.PauseEngine();

            Disassembly = Engine.GetDisassembledMemory();
            RaisePropertyChanged(nameof(Disassembly));
        }

        private void Engine_OnEnginePaused(object sender, EventArgs e)
        {
            _isrunningToNext = false;
            UpdateAfterPause();
        }        

        protected override void Refresh()
        {
            //throw new System.NotImplementedException();
        }
    }
}
