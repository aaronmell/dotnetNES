using System;
using Processor;
using dotnetNES.Engine.Utilities;

namespace dotnetNES.Engine.Models
{
    /// <summary>
    /// Overridden Processor. This was done so I can modify the behavior of several of the methods to work correctly in the NES, without breaking the emulator.
    /// </summary>
    internal sealed class CPU : Processor.Processor
    {
        internal bool DisassemblyEnabled;

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
                    WriteMemoryAction(0x2004, (byte)(ReadMemoryValueWithoutCycle(data << 8 | i)));
                }                
            }
            
            Memory[address] = data;
            
            //Not sure if this is in the right place
            WriteMemoryAction(address, data);

            //Doing this here so the memory has already been updated
            UpdateDisassemblerOnMemoryWrite(address);

            IncrementCycleCount();            
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
            else if (address > 0x1FFF &&  address < 0x4000) 
            {
                address = (address & 0x7) + 0x2000;
            }


			ReadMemoryAction(address);
            IncrementCycleCount();
            
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

            UpdateDisassemblerOnMemoryWrite(address);
        }

        /// <summary>
        /// An Action that occurs when a Memory Location is read
        /// </summary>
        internal Action<int> ReadMemoryAction { get; set; }

        internal Action<int, byte> WriteMemoryAction { get; set; }

        internal ObservableConcurrentDictionary<string, Disassembly> DisassembledMemory { get; private set; }

        public object DisassemblyLock { get; set; } = new object();

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

        internal void EnableDisassembly()
        {           
            GenerateDisassembledMemory();
            ///Prevents concurrent writes to dictionary
            DisassemblyEnabled = true;
        }

        internal void DisableDisassembly()
        {
            DisassemblyEnabled = false;
        }

        internal void GenerateDisassembledMemory()
        {            
            DisassembledMemory = new ObservableConcurrentDictionary<string, Disassembly>();

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
                                   
                DisassembledMemory.Add(originalAddress.ToString("X").PadLeft(2, '0'), new Disassembly
                {
                    Instruction = opCode.Instruction.ToString("X").PadLeft(2, '0'),
                    InstructionAddress = $"{secondByte}{firstByte}",
                    FormattedOpCode = opCode.Length == 1 ? opCode.Format : opCode.Length == 2 ? string.Format(opCode.Format, firstByte) : string.Format(opCode.Format, secondByte, firstByte)
                });
                                
            }
        }

        private void UpdateDisassemblerOnMemoryWrite(int address)
        {
            if (!DisassemblyEnabled)
            {
                return;
            }

            var count = 0;

            do
            {
                //If the data written is a valid instruction then do update the disassembly with the new instruction. 
                if (OpCodeLookup.OpCodes.ContainsKey(Memory[address]))
                {
                    UpdateDisassembler(address - count);
                    return;
                }

                count++;

            } while (address > 0 && count < 3);            
        }


        private void UpdateDisassembler(int address)
        {
            var originalAddress = address.ToString("X").PadLeft(2, '0'); ;
            var opCode = OpCodeLookup.OpCodes[Memory[address++]];

            var firstByte = opCode.Length > 1 ? Memory[address++].ToString("X").PadLeft(2, '0') : string.Empty;
            var secondByte = opCode.Length > 2 ? Memory[address++].ToString("X").PadLeft(2, '0') : string.Empty;           

            if (DisassembledMemory.ContainsKey(originalAddress))
            {
                DisassembledMemory[originalAddress].Instruction = opCode.Instruction.ToString("X").PadLeft(2, '0');
                DisassembledMemory[originalAddress].InstructionAddress = $"{secondByte}{firstByte}";
                DisassembledMemory[originalAddress].FormattedOpCode = opCode.Length == 1 ? opCode.Format : opCode.Length == 2 ? string.Format(opCode.Format, firstByte) : string.Format(opCode.Format, secondByte, firstByte);
            }
            else
            {
                DisassembledMemory.Add(originalAddress, new Disassembly
                {
                    Instruction = opCode.Instruction.ToString("X").PadLeft(2, '0'),
                    InstructionAddress = $"{secondByte}{firstByte}",
                    FormattedOpCode = opCode.Length == 1 ? opCode.Format : opCode.Length == 2 ? string.Format(opCode.Format, firstByte) : string.Format(opCode.Format, secondByte, firstByte)
                });
            }
        }
    }
}
