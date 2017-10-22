using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotnetNES.Engine.Main;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;

namespace dotnetNES.Client.Models
{
    public class CPUFlags : ObservableObject
    {
        public string Accumulator { get; set; }
        public string XRegister { get; set; }
        public string YRegister { get; set; }
        public string StackPointer { get; set; }
        public string ProgramCounter { get; set; }
        public long CycleCount { get; set; }
        public bool CarryFlag { get; set; }
        public bool ZeroFlag { get; set; }
        public bool DisableInterruptFlag { get; set; }
        public bool DecimalFlag { get; set; }
        public bool OverflowFlag { get; set; }
        public bool NegativeFlag { get; set; }

        public void UpdateFlags(dotnetNES.Engine.Main.Engine engine)
        {
            Accumulator = engine.GetAccumulator();
            XRegister = engine.GetXRegister();
            YRegister = engine.GetYRegister();
            StackPointer = engine.GetStackPointer();
            ProgramCounter = engine.GetProgramCounter();
            CarryFlag = engine.GetCarryFlag();
            ZeroFlag = engine.GetZeroFlag();
            DisableInterruptFlag = engine.GetDisableInterruptFlag();
            DecimalFlag = engine.GetDecimalFlag();
            OverflowFlag = engine.GetOverflowFlag();
            NegativeFlag = engine.GetNegativeFlag();
            CycleCount = engine.GetCycleCount();            
        }
    }
}
