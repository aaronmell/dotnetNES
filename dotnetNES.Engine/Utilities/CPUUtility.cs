using System;
using dotnetNES.Engine.Models;
using dotnetNES.Engine.Processors;

namespace dotnetNES.Engine.Utilities
{
    /// <summary>
    /// CPU Utility Classes
    /// </summary>
    internal static class CPUUtility
    {
        /// <summary>
        /// Gets a new instance of the 6502 Processor
        /// </summary>
        /// <param name="cartridgeModel">The cartridge to load</param>
        /// <returns>A Processor Object</returns>
        internal static CPU GetProcessor(this CartridgeModel cartridgeModel)
        {
            var initialProgram = GetInitialProgram(cartridgeModel);

            var cpu = new CPU();
            cpu.LoadProgram(0x8000, initialProgram);
           
            return cpu;
        }

        /// <summary>
        /// Gets a new instance of the 6502 Processor
        /// </summary>
        /// <param name="cartridgeModel">The cartridge to load. </param>
        /// <param name="programCounter">The initial Program Counter</param>
        /// <returns>A Processor Object</returns>
        internal static CPU GetProcessor(this CartridgeModel cartridgeModel, int programCounter)
        {
            var initialProgram = GetInitialProgram(cartridgeModel);

            var cpu = new CPU();
            cpu.LoadProgram(0x8000, initialProgram, programCounter);

            return cpu;
        }

        private static byte[] GetInitialProgram(CartridgeModel cartridgeModel)
        {
            var initialProgram = new byte[cartridgeModel.ROMBanks.GetLength(0) > 1 ? 32768 : 16384 ];

            Array.Copy(cartridgeModel.ROMBanks[0], initialProgram, 16384);
            Array.Copy(cartridgeModel.ROMBanks[1], 0, initialProgram, 16384, 16384);

            return initialProgram;
        }
    }
}
