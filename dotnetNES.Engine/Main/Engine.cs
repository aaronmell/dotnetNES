using dotnetNES.Engine.Models;
using dotnetNES.Engine.Utilities;
using CPU = Processor.Processor;

namespace dotnetNES.Engine.Main
{
    public class Engine
    {
        private readonly CartridgeModel _cartridgeModel;

        public readonly CPU Processor;

       

        public Engine(string fileName)
        {
           _cartridgeModel = CartridgeLoaderUtility.LoadCartridge(fileName);
            Processor = _cartridgeModel.GetProcessor();
        }

        public Engine(byte[] rawBytes)
        {
            Processor = new CPU();
            _cartridgeModel = CartridgeLoaderUtility.LoadCartridge(rawBytes);
        }
 

        /// <summary>
        /// Executes the Next Step
        /// </summary>
        public void NextStep()
        {
            Processor.NextStep();
        }
    }
}
