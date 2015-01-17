using dotnetNES.Engine.Utilities;
using CPU = Processor.Processor;
using PPU = dotnetNES.Engine.PictureProcessingUnit.PictureProcessingUnit;

namespace dotnetNES.Engine.Main
{
    public class Engine
    {
        public readonly CPU Processor;
        public readonly PPU PictureProcessingUnit;

        public bool Paused { get; set; }

        public Engine(string fileName)
        {
            var cartridgeModel = CartridgeLoaderUtility.LoadCartridge(fileName);
            Processor = cartridgeModel.GetProcessor();
            PictureProcessingUnit = new PPU(Processor);
        }

        public Engine(byte[] rawBytes)
        {
            var cartridgeModel = CartridgeLoaderUtility.LoadCartridge(rawBytes);
            Processor = cartridgeModel.GetProcessor();
            PictureProcessingUnit = new PPU(Processor);
        }
        
        public void Step()
        {
            PictureProcessingUnit.Step();
        }
    }
}
