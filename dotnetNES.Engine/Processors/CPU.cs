using System;
using System.Collections.ObjectModel;
using dotnetNES.Engine.Utilities;
using Processor;
using Disassembly = dotnetNES.Engine.Models.Disassembly;

namespace dotnetNES.Engine.Processors
{
    /// <summary>
    /// Overridden Processor. This was done so I can modify the behavior of several of the methods to work correctly in the NES, without breaking the emulator.
    /// </summary>
    internal sealed class CPU : Processor.Processor
    {
        private readonly ObservableCollection<Disassembly> _disassembledMemory = new ObservableCollection<Disassembly>();

        internal bool DisassemblyEnabled;

        internal bool IsDissasemblyInvalid { get; set; } = true;

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
            //Memory from 0x1000-0x17FF mirrors 0x0000-0x7FFF
            if (address < 0x1800)
            {
                address &= 0x7FF;
            }
            //Memory between 0x2008-0x3FFF mirrors every 8 bytes.
            else if (address > 0x1FFF && address < 0x4000)
            {
                address = (address & 0x7) + 0x2000;

                Memory[0x2002] = (byte)(Memory[0x2002] | data & 0x1F);               
            }

            //OAMDMA When a byte is written to 4014 this triggers the memory in location XX00-XXFF to be copied to 2004 in the PPU, where XX is the byte written.
            if (address == 0x4014)
            {                
                for (var i = 0; i < 0x100; i++)
                {
                    WriteMemoryAction(0x2004, ReadMemoryValueWithoutCycle(data << 8 | i));
                }                
            }
            
            Memory[address] = data;

            //Believe the cycle count is incremented before writing
            IncrementCycleCount();
           
            WriteMemoryAction(address, data);
        }

        /// <summary>
        /// Returns the byte at the given address.
        /// </summary>
        /// <param name="address">The address to return</param>
        /// <returns>the byte being returned</returns>
        public override byte ReadMemoryValue(int address)
        {
            //Memory from 0x1000-0x17FF mirrors 0x0000-0x7FFF
            if (address < 0x1800)
            {
                address &= 0x7FF;
            }
            //Memory between 0x2008-0x3FFF mirrors every 8 bytes.
            else if (address > 0x1FFF && address < 0x4000)
            {
                address = (address & 0x7) + 0x2000;
            }

            IncrementCycleCount();

            ReadMemoryAction(address);

            return Memory[address];
        }

        /// <summary>
        /// This method is used to read memory without incrementing the CycleCount, which puts the CPU into an endless loop. 
        /// </summary>
        /// <param name="address">The address to read from</param>
        /// <returns>the value from memory</returns>
        public override byte ReadMemoryValueWithoutCycle(int address)
        {
            //Memory from 0x1000-0x17FF mirrors 0x0000-0x7FFF
            if (address < 0x1800)
            {
                address &= 0x7FF;
            }
            //Memory between 0x2008-0x3FFF mirrors every 8 bytes.
            else if (address > 0x1FFF && address < 0x4000)
            {
                address = (address & 0x7) + 0x2000;
            }

           return Memory[address];
        }

        /// <summary>
        ///  This method is used to write memory without incrementing the CycleCount, which puts the CPU into an endless loop. 
        /// </summary>
        /// <param name="address">The address to write to</param>
        /// <param name="data">The data to write</param>
        public void WriteMemoryValueWithoutCycle(int address, byte data)
        { 
            //Memory from 0x1000-0x17FF mirrors 0x0000-0x7FFF
            if (address < 0x1800)
            {
                address &= 0x7FF;
            }
            //Memory between 0x2008-0x3FFF mirrors every 8 bytes.
            else if (address > 0x1FFF && address < 0x4000)
            {
                address = (address & 0x7) + 0x2000;
            }
            Memory[address] = data;

            //IsDissasemblyInvalid = true;
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
            var memoryValue = ReadMemoryValue(GetAddressByAddressingMode(addressingMode));
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
            var memoryValue = ReadMemoryValue(GetAddressByAddressingMode(addressingMode));
            var newValue = Accumulator - memoryValue - (CarryFlag ? 0 : 1);

            CarryFlag = newValue >= 0;
            OverflowFlag = (((Accumulator ^ newValue) & 0x80) != 0) && (((Accumulator ^ memoryValue) & 0x80) != 0);

            if (newValue < 0)
                newValue += 256;
            

            SetNegativeFlag(newValue);
            SetZeroFlag(newValue);

            Accumulator = newValue;
        }

        internal ObservableCollection<Disassembly> GenerateDisassembledMemory()
        {            
            if (!IsDissasemblyInvalid)
            {
                return _disassembledMemory;
            }

            _disassembledMemory.Clear();
            IsDissasemblyInvalid = false;

            var i = 0;
            while (i < Memory.Length - 1)
            {
                if (!OpCodeLookup.OpCodes.ContainsKey(Memory[i]))
                {
                    i++;
                    continue;
                }
                
                var originalAddress = i;
                var opCode = OpCodeLookup.OpCodes[Memory[i++]];

                var firstByte = opCode.Length > 1 ? Memory[i++].ToString("X").PadLeft(2,'0') : string.Empty;
                var secondByte = opCode.Length > 2 ? Memory[i++].ToString("X").PadLeft(2, '0') : string.Empty;
                                   
                _disassembledMemory.Add(new Disassembly
                {
                    Address = originalAddress.ToString("X").PadLeft(2, '0'), 
                    RawAddress = originalAddress,
                    FormattedOpCode = opCode.Length == 1 ? opCode.Format : opCode.Length == 2 ? string.Format(opCode.Format, firstByte) : string.Format(opCode.Format, secondByte, firstByte)
                });                                
            }

            return _disassembledMemory;
        } 
    }
}
