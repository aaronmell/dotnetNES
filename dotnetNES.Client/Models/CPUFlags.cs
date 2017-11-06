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
        public int RawProgramCounter { get; set; }
        public long CycleCount { get; set; }
        public bool CarryFlag { get; set; }
        public bool ZeroFlag { get; set; }
        public bool DisableInterruptFlag { get; set; }
        public bool DecimalFlag { get; set; }
        public bool OverflowFlag { get; set; }
        public bool NegativeFlag { get; set; }
        public string FlagsRegister { get; set; }

        public void UpdateFlags(dotnetNES.Engine.Main.Engine engine)
        {
            Accumulator = engine.GetAccumulator();
            XRegister = engine.GetXRegister();
            YRegister = engine.GetYRegister();
            StackPointer = engine.GetStackPointer();
            ProgramCounter = engine.GetProgramCounter();
            RawProgramCounter = engine.GetRawProgramCounter();
            CarryFlag = engine.GetCarryFlag();
            ZeroFlag = engine.GetZeroFlag();
            DisableInterruptFlag = engine.GetDisableInterruptFlag();
            DecimalFlag = engine.GetDecimalFlag();
            OverflowFlag = engine.GetOverflowFlag();
            NegativeFlag = engine.GetNegativeFlag();
            CycleCount = engine.GetProcessorCycles();
            FlagsRegister = engine.GetFlagsRegister();
        }
    }
}
