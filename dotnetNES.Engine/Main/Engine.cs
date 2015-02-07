using System;
using dotnetNES.Engine.Processors;
using dotnetNES.Engine.Utilities;
using PPU = dotnetNES.Engine.Processors.PictureProcessingUnit;

namespace dotnetNES.Engine.Main
{
    /// <summary>
    /// The Engine class, this is the heart of the emulator and contains all of the logic needed to drive different pieces of the emulator
    /// </summary>
    public class Engine
    {
        internal readonly CPU Processor;
        internal readonly PPU PictureProcessingUnit;
       
        /// <summary>
        /// Public Constructor for the Engine
        /// </summary>
        /// <param name="fileName">The full path of a .nes cartridge file</param>
        public Engine(string fileName)
        {
            var cartridgeModel = CartridgeLoaderUtility.LoadCartridge(fileName);
            Processor = cartridgeModel.GetProcessor();
            PictureProcessingUnit = new PPU(cartridgeModel, Processor);
        }

        /// <summary>
        /// Public Constructor for the Engine
        /// </summary>
        /// <param name="rawBytes">The raw bytes from a .net cartridge file</param>
        public Engine(byte[] rawBytes)
        {
            var cartridgeModel = CartridgeLoaderUtility.LoadCartridge(rawBytes);
            Processor = cartridgeModel.GetProcessor();
            PictureProcessingUnit = new PPU(cartridgeModel, Processor);
        }
        
        /// <summary>
        /// Runs a single step of the engine.
        /// </summary>
        public void Step()
        {
           Processor.NextStep();
        }

        /// <summary>
        /// Resets the Engine, mimics the behavior of pressing the reset button on the console
        /// </summary>
        public void Reset()
        {
            Processor.Reset();
            PictureProcessingUnit.Reset();
        }

        /// <summary>
        /// An Action method that fires each time a new Frame Occurs. This is by the API to redraw the screen for example.
        /// </summary>
        public Action OnNewFrameAction {
            get { return PictureProcessingUnit.OnNewFrameAction; }
            set { PictureProcessingUnit.OnNewFrameAction = value; } }


        /// <summary>
        /// This gets the Pattern table 0
        /// </summary>
        /// <returns>A byte array of BRGA32 data that represents Pattern Table 0</returns>
        public byte[] GetPatternTable0()
        {
            return PictureProcessingUnit.GetPatternTable0();
        }

        /// <summary>
        /// This gets the Pattern table 1
        /// </summary>
        /// <returns>A byte array of BRGA32 data that represents Pattern Table 1</returns>
        public byte[] GetPatternTable1()
        {
            return PictureProcessingUnit.GetPatternTable1();
        }
    }
}
