using System;
using Processor;

namespace dotnetNES.Engine.Processors
{
    /// <summary>
    /// Overridden Processor. This was done so I can modify the behavior of several of the methods to work correctly in the NES, without breaking the emulator.
    /// </summary>
    internal sealed class CPU : Processor.Processor
    {
        internal CPU()
        {
            ReadMemoryAction = x => { };
            WriteMemoryAction = (x, y) => {};
        }

        /// <summary>
        /// Writes data to the given address.
        /// </summary>
        /// <param name="address">The address to write data to</param>
        /// <param name="data">The data to write</param>
        public override void WriteMemoryValue(int address, byte data)
        {
            Memory[address < 0x4000 ? address & 0x7FF : address] = data;
            IncrementCycleCount();
            //Not sure if this is in the right place
            WriteMemoryAction(address, data);
        }

        /// <summary>
        /// Returns the byte at the given address.
        /// </summary>
        /// <param name="address">The address to return</param>
        /// <returns>the byte being returned</returns>
        public override byte ReadMemoryValue(int address)
        {
            ReadMemoryAction(address);

            var value = Memory[address < 0x4000 ? address & 0x7FF : address];
            IncrementCycleCount();
            return value;
        }

        /// <summary>
        /// This method is used to read memory without incrementing the CycleCount, which puts the CPU into an endless loop. 
        /// </summary>
        /// <param name="address">The address to read from</param>
        /// <returns>the value from memory</returns>
        public byte ReadMemoryValueWithoutCycle(int address)
        {
            var value = Memory[address < 0x4000 ? address & 0x7FF : address];
            return value;
        }

        /// <summary>
        ///  This method is used to write memory without incrementing the CycleCount, which puts the CPU into an endless loop. 
        /// </summary>
        /// <param name="address">The address to write to</param>
        /// <param name="data">The data to write</param>
        public void WriteMemoryValueWithoutCycle(int address, byte data)
        {
            Memory[address < 0x4000 ? address & 0x7FF : address] = data;
        }

        /// <summary>
        /// An Action that occurs when a Memory Location is read
        /// </summary>
        internal Action<int> ReadMemoryAction { get; set; }

        internal Action<int, byte> WriteMemoryAction { get; set; }

        /// <summary>
        /// Overriding the ADC Operation to remove decimal mode
        /// </summary>
        /// <param name="addressingMode"></param>
        protected override void AddWithCarryOperation(AddressingMode addressingMode)
        {
            //Accumulator, Carry = Accumulator + ValueInMemoryLocation + Carry 
            var memoryValue = ReadMemoryValueWithoutCycle(GetAddressByAddressingMode(addressingMode));
            var newValue = memoryValue + Accumulator + (CarryFlag ? 1 : 0);

            OverflowFlag = (((Accumulator ^ newValue) & 0x80) != 0) && (((Accumulator ^ memoryValue) & 0x80) == 0);
            
            if (newValue > 255)
            {
                CarryFlag = true;
                newValue -= 256;
            }
            else
            {
                CarryFlag = false;
            }

            SetZeroFlag(newValue);
            SetNegativeFlag(newValue);

            Accumulator = newValue;
        }

        //Overriding the SBC operaiton to remove decimal mode
        protected override void SubtractWithBorrowOperation(AddressingMode addressingMode)
        {
            var memoryValue = ReadMemoryValueWithoutCycle(GetAddressByAddressingMode(addressingMode));
            var newValue = Accumulator - memoryValue - (CarryFlag ? 0 : 1);

            CarryFlag = newValue >= 0;
            OverflowFlag = (((Accumulator ^ newValue) & 0x80) != 0) && (((Accumulator ^ memoryValue) & 0x80) != 0);

            if (newValue < 0)
                newValue += 256;
            

            SetNegativeFlag(newValue);
            SetZeroFlag(newValue);

            Accumulator = newValue;
        }
    }
}
