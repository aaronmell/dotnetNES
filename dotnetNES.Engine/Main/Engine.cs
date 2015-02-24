using System;
using dotnetNES.Engine.Models;
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
        private readonly CartridgeModel _cartridgeModel;

        public bool IsVerticalMirroringEnabled
        {
            get { return _cartridgeModel.IsVerticalMirroringEnabled; }
        }

        /// <summary>
        /// Public Constructor for the Engine
        /// </summary>
        /// <param name="fileName">The full path of a .nes cartridge file</param>
        public Engine(string fileName)
        {
            
            _cartridgeModel = CartridgeLoaderUtility.LoadCartridge(fileName);
            Processor = _cartridgeModel.GetProcessor();
            PictureProcessingUnit = new PPU(_cartridgeModel, Processor);
        }

        /// <summary>
        /// Public Constructor for the Engine
        /// </summary>
        /// <param name="rawBytes">The raw bytes from a .net cartridge file</param>
        public Engine(byte[] rawBytes)
        {
            _cartridgeModel = CartridgeLoaderUtility.LoadCartridge(rawBytes);
            Processor = _cartridgeModel.GetProcessor();
            PictureProcessingUnit = new PPU(_cartridgeModel, Processor);
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
        /// This sets PatternTable0 on its bitmap.
        /// </summary>
        /// <param name="bitmapPointer">A pointer to the bitmap object that draws PatternTable0</param>
        public unsafe void SetPatternTable0(byte* bitmapPointer)
        {
            PictureProcessingUnit.GetPatternTable0(bitmapPointer);
        }

        /// <summary>
        /// This sets PatternTable1 on its bitmap.
        /// </summary>
        /// <param name="bitmapPointer">A pointer to the bitmap object that draws PatternTable1</param>
        public unsafe void SetPatternTable1(byte* bitmapPointer)
        {
            PictureProcessingUnit.SetPatternTable1(bitmapPointer);
        }

        /// <summary>
        /// This sets the background palette on its bitmap.
        /// </summary>
        /// <param name="bitmapPointer">A pointer to the bitmap object that draws the background palette</param>
        public unsafe void SetBackgroundPalette(byte* bitmapPointer)
        {
            PictureProcessingUnit.SetBackgroundPalette(bitmapPointer);
        }

        /// <summary>
        /// This sets the sprite palette on its bitmap.
        /// </summary>
        /// <param name="bitmapPointer">A pointer to the bitmap object that draws the sprite palette</param>
        public unsafe void SetSpritePalette(byte* bitmapPointer)
        {
            PictureProcessingUnit.SetSpritePalette(bitmapPointer);
        }

        /// <summary>
        /// This sets the nametable on its bitmap.
        /// </summary>
        /// <param name="bitmapPointer">A pointer to the bitmap object that draws the nametable</param>
        /// <param name="nameTableSelect">The nametable to fetch from</param>
        public unsafe void SetNameTables(byte* bitmapPointer, int nameTableSelect)
        {
            PictureProcessingUnit.SetNameTable(bitmapPointer, nameTableSelect);
        }
    }
}
