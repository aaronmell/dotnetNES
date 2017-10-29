using System;
using System.Diagnostics;
using dotnetNES.Engine.Models;
using dotnetNES.Engine.Utilities;
using PPU = dotnetNES.Engine.Models.PictureProcessingUnit;
using NLog;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;

namespace dotnetNES.Engine.Main
{
    /// <summary>
    /// The Engine class, this is the heart of the emulator and contains all of the logic needed to drive different pieces of the emulator
    /// </summary>
    public class Engine
    {
        private static readonly ILogger _logger = LogManager.GetLogger("Engine");
        private readonly CartridgeModel _cartridgeModel;
        private BackgroundWorker _backgroundWorker;
        private double cyclesToSkip = 0;
        private bool _skipCycles;

        internal CPU Processor { get; private set; }
        internal PPU PictureProcessingUnit { get; private set; }
      
        /// <summary>
        /// Collection of breakpoints
        /// </summary>
        public ObservableCollection<BreakPoint> BreakPoints { get; set; } = new ObservableCollection<BreakPoint>();       

        /// <summary>
        /// The property is used to determine if vertical mirroring is used by the current cartridge.
        /// If its set to true, it changes the drawing behavior of the Nametables screen.
        /// </summary>
        public bool IsVerticalMirroringEnabled
        {
            get { return _cartridgeModel.IsVerticalMirroringEnabled; }
        }

        /// <summary>
        /// returns a value indicating if the engine is running or paused
        /// </summary>
        public bool IsPaused { get; set; } = true;

        /// <summary>
        /// Public Constructor for the Engine
        /// </summary>
        /// <param name="fileName">The full path of a .nes cartridge file</param>
        public Engine(string fileName)
        {
            CreateNewBackgroundWorker();

            _cartridgeModel = CartridgeLoaderUtility.LoadCartridge(fileName);
            Processor = _cartridgeModel.GetProcessor();
            PictureProcessingUnit = new PPU(_cartridgeModel, Processor);

            if (Processor.DisassemblyEnabled)
            {
                Processor.GenerateDisassembledMemory();
            }            
        }             

        /// <summary>
        /// Public Constructor for the Engine
        /// </summary>
        /// <param name="rawBytes">The raw bytes from a .net cartridge file</param>
        public Engine(byte[] rawBytes)
        {
            CreateNewBackgroundWorker();

            _cartridgeModel = CartridgeLoaderUtility.LoadCartridge(rawBytes);
            Processor = _cartridgeModel.GetProcessor();
            PictureProcessingUnit = new PPU(_cartridgeModel, Processor);

            if (Processor.DisassemblyEnabled)
            {
                Processor.GenerateDisassembledMemory();
            }          
        }

        /// <summary>
        /// Runs a single step of the engine.
        /// </summary>
        public void Step()
        {
            Processor.NextStep();
            WriteLog();
        }

        [Conditional("DEBUG")]
        private void WriteLog()
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(string.Format("{0}  {1} {2} {3} {4} {5} A:{6} X:{7} Y:{8} P:{9} SP:{10} CYC:{11} SL:{12}",
               Processor.ProgramCounter.ToString("X").PadLeft(4, '0'),
               Processor.CurrentOpCode.ToString("X"),
               Processor.CurrentDisassembly.LowAddress.PadRight(2),
               Processor.CurrentDisassembly.HighAddress.PadRight(2),
               Processor.CurrentDisassembly.OpCodeString.PadRight(2),
               Processor.CurrentDisassembly.DisassemblyOutput.PadRight(16, ' '),
               Processor.Accumulator.ToString("X").PadLeft(2, '0'),
               Processor.XRegister.ToString("X").PadLeft(2, '0'),
               Processor.YRegister.ToString("X").PadLeft(2, '0'),
               ((byte)
                   ((Processor.CarryFlag ? 0x01 : 0) + (Processor.ZeroFlag ? 0x02 : 0) +
                    (Processor.DisableInterruptFlag ? 0x04 : 0) +
                    (Processor.DecimalFlag ? 8 : 0) + (0) + 0x20 + (Processor.OverflowFlag ? 0x40 : 0) +
                    (Processor.NegativeFlag ? 0x80 : 0))).ToString("X"),
               Processor.StackPointer.ToString("X").PadLeft(2, '0'), PictureProcessingUnit.CycleCount,
               PictureProcessingUnit.ScanLine));
            }
        }

        /// <summary>
        /// Resets the Engine, mimics the behavior of pressing the reset button on the console
        /// </summary>
        public void Reset()
        {
            PauseEngine();

            Processor.Reset();
            PictureProcessingUnit.Reset();

            if (Processor.DisassemblyEnabled)
            {
                Processor.GenerateDisassembledMemory();
            }

            UnPauseEngine();
        }      

        /// <summary>
        /// Resets the Engine, mimics the behavior of pressing the power button on the console
        /// </summary>
        public void Power()
        {
            PauseEngine();

            Processor.Reset();
            PictureProcessingUnit.Power();

            if (Processor.DisassemblyEnabled)
            {
                Processor.GenerateDisassembledMemory();
            }

            UnPauseEngine();
        }

        /// <summary>
        /// An Action method that fires each time a new Frame Occurs. This is by the API to redraw the screen for example.
        /// </summary>
        public Action OnNewFrameAction {
            get { return PictureProcessingUnit.OnNewFrameAction; }
            set { PictureProcessingUnit.OnNewFrameAction = value; } }

        /// <summary>
        /// This draws PatternTable0 on its bitmap.
        /// </summary>
        /// <param name="bitmapPointer">A pointer to the bitmap object that draws PatternTable0</param>
        public unsafe void DrawPatternTable0(byte* bitmapPointer)
        {
            PictureProcessingUnit.DrawPatternTable0(bitmapPointer);
        }

        /// <summary>
        /// This draws PatternTable1 on its bitmap.
        /// </summary>
        /// <param name="bitmapPointer">A pointer to the bitmap object that draws PatternTable1</param>
        public unsafe void DrawPatternTable1(byte* bitmapPointer)
        {
            PictureProcessingUnit.DrawPatternTable1(bitmapPointer);
        }

        /// <summary>
        /// This draws the background palette on its bitmap.
        /// </summary>
        /// <param name="bitmapPointer">A pointer to the bitmap object that draws the background palette</param>
        public unsafe void DrawBackgroundPalette(byte* bitmapPointer)
        {
            PictureProcessingUnit.DrawBackgroundPalette(bitmapPointer);
        }

        /// <summary>
        /// This draws the sprite palette on its bitmap.
        /// </summary>
        /// <param name="bitmapPointer">A pointer to the bitmap object that draws the sprite palette</param>
        public unsafe void DrawSpritePalette(byte* bitmapPointer)
        {
            PictureProcessingUnit.DrawSpritePalette(bitmapPointer);
        }

        /// <summary>
        /// This draws the nametable on its bitmap
        /// </summary>
        /// <param name="bitmapPointer">A pointer to the bitmap object that draws the nametable</param>
        /// <param name="nameTableSelect">The nametable to fetch from</param>
        public unsafe void DrawNameTable(byte* bitmapPointer, int nameTableSelect)
        {
            PictureProcessingUnit.DrawNametable(bitmapPointer, nameTableSelect);
        }

        /// <summary>
        /// Draws the sprite on its bitmap
        /// </summary>
        /// <param name="spritePointer">A pointer that points to the sprite bitmap</param>
        /// <param name="spriteSelect">The sprite to select</param>
        public unsafe void DrawSprite(byte* spritePointer, int spriteSelect)
        {
            PictureProcessingUnit.DrawSprite(spritePointer, spriteSelect);
        }

        /// <summary>
        /// This method gets the current Frame from the PPU
        /// </summary>
        /// <returns>A byte array of pixels</returns>
        public byte[] GetScreen()
        {   
            return PPU.CurrentFrame;
        }

        /// <summary>
        /// Returns the disassembled Memory from the CPU
        /// </summary>
        /// <returns>A collection of <see cref="Disassembly"/></returns>
        public ObservableCollection<Disassembly> GetDisassembledMemory()
        {
            return Processor.GenerateDisassembledMemory();
        }       
        
        public bool IsDissasemblyInvalid()
        {
            return Processor.IsDissasemblyInvalid;
        }

        /// <summary>
        /// An action that is fired whenever the Engine is paused
        /// </summary>
        public event EventHandler EnginePaused;

        protected virtual void OnEnginePaused(EventArgs e)
        {
            EnginePaused?.Invoke(this, e);
        }

        private void CreateNewBackgroundWorker()
        {
            _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true, WorkerReportsProgress = false };
            _backgroundWorker.DoWork += BackgroundWorkerDoWork;

        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var firstCycle = true;
            var worker = sender as BackgroundWorker;
            while (true)
            {
                if (worker != null && worker.CancellationPending || (!firstCycle && CheckforCancellation()))
                {
                    e.Cancel = true;
                    PauseEngine();
                    return;
                }               

                Step();
                firstCycle = false;
            }
        }

        private bool CheckforCancellation()
        {
            if (IsPaused)
            {
                return true;
            }

            if (_skipCycles && PictureProcessingUnit.TotalCycles >= cyclesToSkip)
            {
                _skipCycles = false;

                return true;
            }

            if (BreakPoints.Any(x => x.IsEnabled && x.BreakPointType == BreakPointType.Execute && x.ConvertedAddress == Processor.ProgramCounter))
            {
                return true;
            }

            return false;
        }

        public void UnPauseEngine()
        {
            if (!IsPaused)
            {
                return;
            }
            IsPaused = false;
            _backgroundWorker.RunWorkerAsync();
        }

        public void PauseEngine()
        {
            if (IsPaused)
            {
                return;
            }
            IsPaused = true;

            OnEnginePaused(EventArgs.Empty);           
        }

        public void RuntoNextScanLine()
        {
            _skipCycles = true;

            //This will always run slightly less than a full scanline, so it will never jump past the cycle you were previously on. 
            cyclesToSkip = PictureProcessingUnit.TotalCycles + 330;

            UnPauseEngine();
        }

        public void RuntoNextFrame()
        {
            _skipCycles = true;

            //This will always run slightly less than an entire frame, so you will never jump past the previous cycle scanline and cycle. 
            cyclesToSkip = PictureProcessingUnit.TotalCycles + 89327;

            UnPauseEngine();
        }

        public string GetAccumulator()
        {
            return Processor.Accumulator.ToString("X").PadLeft(2, '0');
        }

        public string GetYRegister()
        {
            return Processor.YRegister.ToString("X").PadLeft(2, '0');
        }

        public string GetXRegister()
        {
            return Processor.YRegister.ToString("X").PadLeft(2, '0');
        }

        public string GetProgramCounter()
        {
            return Processor.ProgramCounter.ToString("X").PadLeft(4, '0');
        }

        public int GetRawProgramCounter()
        {
            return Processor.ProgramCounter;
        }

        public bool GetDisableInterruptFlag()
        {
            return Processor.DisableInterruptFlag;
        }

        public bool GetOverflowFlag()
        {
            return Processor.OverflowFlag;
        }

        public bool GetNegativeFlag()
        {
            return Processor.NegativeFlag;
        }

        public bool GetDecimalFlag()
        {
            return Processor.DecimalFlag;
        }

        public bool GetZeroFlag()
        {
            return Processor.ZeroFlag;
        }

        public bool GetCarryFlag()
        {
            return Processor.CarryFlag;
        }

        public string GetStackPointer()
        {
            return Processor.StackPointer.ToString("X").PadLeft(4,'0');
        }

        public long GetProcessorCycles()
        {
            return Processor.GetCycleCount();
        }

        public string GetFlagsRegister()
        {
            return ((Processor.CarryFlag ? 0x01 : 0) + (Processor.ZeroFlag ? 0x02 : 0) + (Processor.DisableInterruptFlag ? 0x04 : 0) +
                         (Processor.DecimalFlag ? 0x08 : 0) + 0x20 + (Processor.OverflowFlag ? 0x40 : 0) + (Processor.NegativeFlag ? 0x80 : 0)).ToString("X").PadLeft(2,'0');
          
        }

        public int GetPPUCycleCount()
        {
            return PictureProcessingUnit.CycleCount;
        }

        public int GetNTAddress()
        {
            return PictureProcessingUnit.NameTableAddress;
        }

        public int GetXScroll()
        {
            return PictureProcessingUnit.FineXScroll;
        }

        public int GetVRAMAddress()
        {
            return PictureProcessingUnit.VRamAddress;
        }

        public int GetScanLine()
        {
            return PictureProcessingUnit.ScanLine;
        }

        public byte GetMemoryLocation(int address)
        {
            return Processor.ReadMemoryValueWithoutCycle(address);
        }
    }
}
