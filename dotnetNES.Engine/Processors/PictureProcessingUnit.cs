using System;
using dotnetNES.Engine.Models;

namespace dotnetNES.Engine.Processors
{
    /// <summary>
    /// The Picture Processing Unit or PPU
    /// </summary>
    internal class PictureProcessingUnit
    {
        /// <summary>
        /// The internal Memory of the PPU
        /// Address range 	Size 	Description
        /// 0x0000-0x0FFF 	0x1000 	Pattern table 0
        /// 0x1000-0x1FFF 	0x1000 	Pattern Table 1
        /// 0x2000-0x23FF 	0x0400 	Nametable 0
        /// 0x2400-0x27FF 	0x0400 	Nametable 1
        /// 0x2800-0x2BFF 	0x0400 	Nametable 2
        /// 0x2C00-0x2FFF 	0x0400 	Nametable 3
        /// 0x3000-0x3EFF 	0x0F00 	Mirrors of 0x2000-0x2EFF
        /// 0x3F00-0x3F1F 	0x0020 	Palette RAM indexes
        /// 0x3F20-0x3FFF 	0x00E0 	Mirrors of 0x3F00-0x3F1F 
        /// </summary>
        private readonly byte[] _internalMemory = new byte[16384];

        /// <summary>
        /// The OAM. This is manipulated by reads/writes to 0x2003 0x2004 and 0x4014
        /// Address range 	    Size 	Description
        /// 0x00-0x0C (0 of 4) 	0x40 	Sprite Y coordinate
        /// 0x01-0x0D (1 of 4) 	0x40 	Sprite tile #
        /// 0x02-0x0E (2 of 4) 	0x40 	Sprite attribute
        /// 0x03-0x0F (3 of 4) 	0x40 	Sprite X coordinate 
        /// </summary>
        private readonly byte[] _objectAttributeMemory = new byte[256];

        private readonly CPU _cpu = new CPU();

        /// <summary>
        /// The number of cycles that have currently elapsed
        /// </summary>
        private int _cycleCount;
        /// <summary>
        /// The current scanLine of the PPU
        /// </summary>
        private int _scanLine;
        /// <summary>
        /// Odd frames skip a cycle on the first cycle and scanline
        /// </summary>
        private bool _isOddFrame;

        /// <summary>
        /// This register is set to true at the start of vertical blanking. It is set to false and the end of vertical blanking, and also when the Status Register is read
        /// </summary>
        private bool _nmiOccurred;
        /// <summary>
        /// This register is set to bit 7 of the Controller Register when it is written to.
        /// </summary>
        private bool _nmiOutput;

        /// <summary>
        /// The PPUCTRL Register, maps to $2000. This register causes things to happen when it is written to. 
        /// 0: X scroll name table selection
        /// 1: Y scroll name table selection
        /// 2: The amount to increment the Vram address per Read/Write of 0x2007. 0 = increment 1, 1 = increment 32
        /// 3: Sprite atten table address for 8x8 Sprites. 0= 0x0000 1= 0x1000. This is ignored in 8x16 mode
        /// 4: Background pattern table address 0= 0x0000 1= 0x1000
        /// 5: Sprite Size 0= 8x8 1= 8x16
        /// 6: Master/Slave Select 0= Read backdrop from EXT Pins. 1=Output color on ExtPins
        /// 7: Generate an NMI.
        /// </summary>
        private byte ControlRegister
        {
            get { return _cpu.ReadMemoryValueWithoutCycle(0x2000); }
            set { _cpu.WriteMemoryValueWithoutCycle(0x2000, value); }
        }

        /// <summary>
        /// The PPUMASK Register. 
        /// 0: Grayscale 0=Color 1= Monochrome. When this is enabled, the PPU will bitwise AND with $30 any value read from $3F00-$3FFF, 
        ///     both on the display and through PPUDATA. Will not affect writes
        /// 1: 0=Hide 1=show background in leftmost 8 pixels of screen
        /// 2: 0=Hide 1=show sprites in leftmost 8 pixels of screen
        /// 3: 0=Hide 1=Show brackground
        /// 4: 0=Hide 1=show sprites
        /// 5: Intensify Reds
        /// 6: Intensify greens
        /// 7: Intensify blues
        /// </summary>
        private byte MaskRegister
        {
            get { return _cpu.ReadMemoryValueWithoutCycle(0x2001); }
            set { _cpu.WriteMemoryValueWithoutCycle(0x2001, value); }
        }

        /// <summary>
        /// The PPUSTATUS Register. When the CPU reads this register, it clears bit 7 of this register, and also clears the Scroll and address register.
        /// 0-4: LSB previously written into a PPU Register
        /// 5: Sprite Overflow flag
        /// 6: Sprite 0 Hit
        /// 7: Vblank 0= Not in Vblank 1= in Vblank
        /// </summary>
        private byte StatusRegister
        {
            get { return _cpu.ReadMemoryValueWithoutCycle(0x2002); }
            set { _cpu.WriteMemoryValueWithoutCycle(0x2002, value); }
        }

        /// <summary>
        /// The OAMADDR (Object Attribute Memory) Address
        /// </summary>
        private byte ObjectAttributeMemoryRegister
        {
            get { return _cpu.ReadMemoryValueWithoutCycle(0x2004); }
            set { _cpu.WriteMemoryValueWithoutCycle(0x2004, value); }
        }

        /// <summary>
        /// The PPUSCROLL register. 
        /// This tells the PPU which pixel of the nametable should be at the top left corner of the rendered screen.
        /// If it is changed during rendering, it will take effect during the next frame.
        /// </summary>
        private byte ScrollRegister
        {
            get { return _cpu.ReadMemoryValueWithoutCycle(0x2005); }
            set { _cpu.WriteMemoryValueWithoutCycle(0x2005, value); }
        }

        /// <summary>
        /// The PPUADDR register.
        /// This is the address that data in the PPU's internal memory will be written to.
        /// </summary>
        private byte AddressRegister
        {
            get { return _cpu.ReadMemoryValueWithoutCycle(0x2006); }
            set { _cpu.WriteMemoryValueWithoutCycle(0x2006, value); }
        }

        /// <summary>
        /// This allows data to be written from or to the PPU.
        /// </summary>
        private byte DataRegister
        {
            get { return _cpu.ReadMemoryValueWithoutCycle(0x2007); }
            set { _cpu.WriteMemoryValueWithoutCycle(0x2007, value); }
        }

        /// <summary>
        /// Constructor for the PPU
        /// </summary>
        /// <param name="cartridgeModel"></param>
        /// <param name="cpu"></param>
        internal PictureProcessingUnit(CartridgeModel cartridgeModel, CPU cpu)
        {
            _cpu = cpu;
            _cpu.CycleCountIncrementedAction = CPUCycleCountIncremented;
            _cpu.ReadMemoryAction = ReadMemoryAction;
            _cpu.WriteMemoryAction = WriteMemoryAction;

            LoadInitialMemory(cartridgeModel);
        }

        private void LoadInitialMemory(CartridgeModel cartridgeModel)
        {
           Array.Copy(cartridgeModel.VROMBanks[0],_internalMemory, 8192);
        }

        private void CPUCycleCountIncremented()
        {
            var isRenderingDisabled = IsRenderingDisabled();
            
            if (_nmiOccurred && _nmiOutput)
            {
                _cpu.NonMaskableInterrupt();
            }

            if (_scanLine < 240)
            {
                //Skip the first cycle on the first scanline if an odd frame
                if (_scanLine == 0 && _cycleCount == 0 && _isOddFrame && !isRenderingDisabled)
                {
                    _cycleCount++;
                }

                switch (_cycleCount)
                {
                    case 2:
                    case 10:
                    case 18:
                    case 26:
                    case 34:
                    case 42:
                    case 50:
                    case 58:
                    case 66:
                    case 74:
                    case 82:
                    case 90:
                    case 98:
                    case 106:
                    case 114:
                    case 122:
                    case 130:
                    case 138:
                    case 146:
                    case 154:
                    case 162:
                    case 170:
                    case 178:
                    case 186:
                    case 194:
                    case 202:
                    case 210:
                    case 218:
                    case 226:
                    case 234:
                    case 242:
                    case 250:
                    case 322:
                    case 330:
                    case 338:
                    case 340:
                    {
                        //TODO NT Byte
                        break;
                    }
                    case 4:
                    case 12:
                    case 20:
                    case 28:
                    case 36:
                    case 44:
                    case 52:
                    case 60:
                    case 68:
                    case 76:
                    case 84:
                    case 92:
                    case 100:
                    case 108:
                    case 116:
                    case 124:
                    case 132:
                    case 140:
                    case 148:
                    case 156:
                    case 164:
                    case 172:
                    case 180:
                    case 188:
                    case 196:
                    case 204:
                    case 212:
                    case 220:
                    case 228:
                    case 236:
                    case 244:
                    case 252:
                    case 324:
                    case 332:

                    {
                        //TODO AT Byte
                        break;
                    }
                    case 6:
                    case 14:
                    case 22:
                    case 30:
                    case 38:
                    case 46:
                    case 54:
                    case 62:
                    case 70:
                    case 78:
                    case 86:
                    case 94:
                    case 102:
                    case 110:
                    case 118:
                    case 126:
                    case 134:
                    case 142:
                    case 150:
                    case 158:
                    case 166:
                    case 174:
                    case 182:
                    case 190:
                    case 198:
                    case 206:
                    case 214:
                    case 222:
                    case 230:
                    case 238:
                    case 246:
                    case 254:
                    case 326:
                    case 334:
                    {
                        //TODO Low BG tile byte
                        break;
                    }
                    case 8:
                    case 16:
                    case 24:
                    case 32:
                    case 40:
                    case 48:
                    case 56:
                    case 64:
                    case 72:
                    case 80:
                    case 88:
                    case 96:
                    case 104:
                    case 112:
                    case 120:
                    case 128:
                    case 136:
                    case 144:
                    case 152:
                    case 160:
                    case 168:
                    case 176:
                    case 184:
                    case 192:
                    case 200:
                    case 208:
                    case 216:
                    case 224:
                    case 232:
                    case 240:
                    case 248:
                    case 256:
                    case 328:
                    case 336:
                    {
                        //TODO High BG tile byte
                        //TODO Inc hori(v)
                        break;
                    }
                    //TODO: Add Cycles 257-320
                }
            }
            else if (_scanLine == 241)
            {
                if (_cycleCount == 2)
                {
                    StatusRegister |= 0x80;
                    _nmiOccurred = true;
                }
            }
            else
            {
                if (_cycleCount == 1)
                {
                    //Clear Vertical Blank
                    StatusRegister &= byte.MaxValue ^ (1 << 7); 
                    //Clear Sprite 0 Hit
                    StatusRegister &= byte.MaxValue ^ (1 << 6);
                    //Clear Sprite Overflow
                    StatusRegister &= byte.MaxValue ^ (1 << 5);
                    _nmiOccurred = false;
                }
            }

            _cycleCount++;

            if (_cycleCount < 340)
                _cycleCount++;
            else
            {
                _cycleCount = 0;
                _scanLine++;
            }

            if (_scanLine < 261)
                _scanLine++;
            else
            {
                _scanLine = 0;
            }
            
            _isOddFrame = !_isOddFrame;
        }

        private void ReadMemoryAction(int address)
        {
            if (address >= 0x4000 || (address & 0x7ff) != 0x2002)
                return;

            StatusRegister &= byte.MaxValue ^ (1 << 7);
            AddressRegister = 0;
            ScrollRegister = 0;
            _nmiOccurred = false;
            
        }

        private void WriteMemoryAction(int address, byte value)
        {
            if (address >= 0x4000 || (address & 0x7ff) != 0x2000)
                return;

            _nmiOutput = (ControlRegister & 0x80) == 80;
        }

        private bool IsRenderingDisabled()
        {
            return (MaskRegister & 0x1E) == 0;
        }

        internal void Step()
        {
            _cpu.NextStep();
        }
    }
}
