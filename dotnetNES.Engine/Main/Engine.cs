using dotnetNES.Engine.Processors;
using dotnetNES.Engine.Utilities;
using PPU = dotnetNES.Engine.Processors.PictureProcessingUnit;

namespace dotnetNES.Engine.Main
{
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
    }
}
