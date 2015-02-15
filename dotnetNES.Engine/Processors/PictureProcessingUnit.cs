﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common.Logging;
using dotnetNES.Engine.Models;

namespace dotnetNES.Engine.Processors
{
    /// <summary>
    /// The Picture Processing Unit or PPU
    /// </summary>
    internal class PictureProcessingUnit
    {
        #region Private Properties

        #region Memory
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
        #endregion

        #region Memory Mapped Registers
        /// <summary>
        /// The PPUCTRL Register, maps to $2000. This register causes things to happen when it is written to. 
        /// 0: X scroll name table selection
        /// 1: Y scroll name table selection
        /// 0-1: Base NameTable Address 0= 0x2000 1= 0x2400 2= 0x2800 3= 0x2C00
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
        #endregion

        #region Internal Registers
        /// <summary>
        /// A 15 bit register used by the PPU. It is shared by both the PPUSCROLL and PPUADDR Registers
        /// YYY NN yyyyy xxxxx
        /// YYY = Fine Y scroll
        /// NN = NameTable select
        /// yyyyy = coarse Y scroll
        /// XXXXX = coarse X srcoll
        /// 
        /// The first write to the <see cref="ScrollRegister"/> will copy the upper five bits of the write to that register into the temporary address
        /// The second write to the <see cref="ScrollRegister"/> will copy the lower 3 bits to D14-D12 and the upper five bits to D9-D5 in the register.
        /// 
        /// The first write to the <see cref="AddressRegister"/> will clear D14, and D13-D8 will be loaded with the lower six bits of the value written
        /// The second write to the <see cref="AddressRegister"/> will be copied to D7-D0 of this register. After this write the temporary address will be copied into the <see cref="_currentAddress"/>
        /// 
        /// Writing to <see cref="ControlRegister"/> will copy the NameTable bits from the ControlRegister into D10-D11
        /// 
        /// At the beginning of each frame, this address will be copied into <see cref="_currentAddress"/>. This will also occur from cycle 280 to 304
        /// At cycle 257 D10, and D4-D0 are coped into <see cref="_currentAddress"/>
        /// </summary>
        private int _temporaryAddress;

        /// <summary>
        /// This is the current address. The PPU uses this address to get the data it needs to render a pixel.
        /// </summary>

        private int _currentAddress;
       

        /// <summary>
        /// Controls the Fine X Scroll. On the first write to <see cref="ScrollRegister"/> the lower three bits of the value are copied here
        /// </summary>
        private int _fineXScroll;

        /// <summary>
        /// This is the register that the <see cref="_highBackgroundTileByte"/> gets loaded into at the end of every 8 clock cycles.
        /// It is always loaded into the 8 high bits.
        /// This register is shifted each clock cycle to the right 1 bit
        /// </summary>
        private int _upperShiftRegister;

        /// <summary>
        /// This is the register that the <see cref="_lowBackgroundTileByte"/> gets loaded into at the end of every 8 clock cycles.
        /// It is always loaded into the 8 high bits.
        /// This register is shifted each clock cycle to the right 1 bit
        /// </summary>
        private int _lowerShiftRegister;


        //private int _attributeShiftRegister1;

        //private int _attributeShiftRegister2;
        #endregion

        #region Background Latches
        //These store the address during the first cycle of the 2 cycle operations. 
        private int _nameTableAddress;
        private int _attributeTableAddress;
        private int _highBackgroundTileAddress;
        private int _lowBackgroundTileAddress;

        //These store the bytes retrieved during the second cycle of the 2 cycle operation. They are fed into the shift registers every 8 cycles
        private byte _nameTableByte;
        private byte _attributeByte;
        private byte _highBackgroundTileByte;
        private byte _lowBackgroundTileByte;

        /// <summary>
        /// This latch gets flipped every time a bit is written to 
        /// <see cref="ScrollRegister"/> or <see cref="AddressRegister"/> 
        /// It is reset to 0 when <see cref="StatusRegister"/> is read from
        /// </summary>
        private bool _tempAddressHasBeenWrittenTo;
        #endregion

        #region Internal Status
        
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
        /// This register is set to true at the start of vertical blanking. 
        /// It is set to false and the end of vertical blanking, and also when the Status Register is read
        /// </summary>
        private bool _nmiOccurred;
        /// <summary>
        /// This register is set to bit 7 of the Controller Register when it is written to.
        /// </summary>
        private bool _nmiOutput;
        #endregion

        /// <summary>
        /// A Buffer for reads form ppu memory when the address is in the 0x0 to 0x3EFF range.
        /// </summary>
        private byte _ppuDataReadBuffer;

        /// <summary>
        /// this flag is set when 0x00 is written to the <see cref="MaskRegister"/>
        /// </summary>
        private bool _isRenderingDisabled;

        /// <summary>
        /// The CPU
        /// </summary>
        private readonly CPU _cpu = new CPU();

        private static readonly ILog _logger = LogManager.GetLogger("PictureProcessingUnit");

        //This contains all of the colors the NES can display converted into RGB format.
        private static byte[] _pallet =
        {
            124, 124, 124, 0, 0, 252, 0, 0, 188, 68, 40, 188, 148, 0, 132, 168, 0, 32, 168, 16, 0, 136, 20, 0, 80, 48, 0,
            0, 120, 0, 0, 104, 0, 0, 88, 0, 0, 64, 88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 188, 188, 188, 0, 120, 248, 0, 88, 248,
            104, 68, 252, 216, 0, 204, 228, 0, 88, 248, 56, 0, 228, 92, 16, 172, 124, 0, 0, 184, 0, 0, 168, 0, 0, 168,
            68, 0, 136, 136, 0, 0, 0, 0, 0, 0, 0, 0, 0, 248, 248, 248, 60, 188, 252, 104, 136, 252, 152, 120, 248, 248,
            120, 248, 248, 88, 152, 248, 120, 88, 252, 160, 68, 248, 184, 0, 184, 248, 24, 88, 216, 84, 88, 248, 152, 0,
            232, 216, 120, 120, 12, 0, 0, 0, 0, 0, 0, 252, 252, 252, 164, 228, 252, 184, 184, 248, 216, 184, 248, 248,
            184, 248, 248, 164, 192, 240, 208, 176, 252, 224, 168, 248, 216, 120, 216, 248, 120, 184, 248, 184, 184, 248,
            216, 0, 252, 252, 248, 216, 248, 0, 0, 0, 0, 0, 0
        };

        /// <summary>
        /// This flag is set after a reset or power event. Any writes to 0x2000, 0x2001, 0x2005, or 0x2006 are ignored until the flag is cleared.
        /// </summary>
        private bool _internalResetFlag;

        /// <summary>
        /// This is set by <see cref="ControlRegister"/> bit 2
        /// </summary>
        private int _currentAddressIncrement;
        #endregion

        /// <summary>
        /// This action is fired each time a new frame is available
        /// </summary>
        internal Action OnNewFrameAction { get; set; }
        #region Constructor
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
            OnNewFrameAction = () => { };
            _internalResetFlag = true;
            _scanLine = 241;
            _cycleCount = 0;
        }
        #endregion

        #region Internal Methods

        internal void Reset()
        {
            _isOddFrame = false;
            _tempAddressHasBeenWrittenTo = false;
            ScrollRegister = 0;
            DataRegister = 0;
            ControlRegister = 0;
            MaskRegister = 0;
            _internalResetFlag = true;
            _scanLine = 241;
            _cycleCount = 0;
        }

        /// <summary>
        /// This gets the Pattern table 0 stored between 0x00 and 0x0FF
        /// </summary>
        /// <returns>A byte array of BRGA32 data</returns>
        internal byte[] GetPatternTable0()
        {
            return GetPatternTable(true);
        }

        /// <summary>
        /// This gets the Pattern table 0 stored between 0x00 and 0x0FF
        /// </summary>
        /// <returns>A byte array of BRGA32 data</returns>
        internal byte[] GetPatternTable1()
        {
            return GetPatternTable(false);
        }

        /// <summary>
        /// Reads the PPU memory. This is mainly used by tests
        /// </summary>
        /// <param name="address">The address to read</param>
        /// <returns>a byte of memory</returns>
        internal byte ReadPPUMemory(int address)
        {
            return _internalMemory[address];
        }

        /// <summary>
        /// Gets the background palette
        /// </summary>
        /// <returns>a BRG byte array of background palettes</returns>
        internal byte[] GetBackgroundPalette()
        {
            return GetPalette(true);
        }

        /// <summary>
        /// Gets the sprite palette
        /// </summary>
        /// <returns>A BRG array of sprite palettes</returns>
        internal byte[] GetSpritePalette()
        {
            return GetPalette(false);
        }
        #endregion

        #region Private Methods

        #region Main Loop
        private void CPUCycleCountIncremented()
        {
            if (_internalResetFlag && _cpu.GetCycleCount() > 29658)
                _internalResetFlag = false;
           
            StepPPU();
            StepPPU();
            StepPPU();
        }

        private void StepPPU()
        {
            WriteLog("Stepping PPU");

            if (_nmiOccurred && _nmiOutput)
            {
                _nmiOccurred = false;
                _cpu.NonMaskableInterrupt();

               
                WriteLog("NMI Occurred!");
            }

            if (!_isRenderingDisabled)
            {
                OuterCycleAction();
            }
            else
            {
                WriteLog("Rendering Is Disabled!");
            }           

            if (_cycleCount < 340)
                _cycleCount++;
            else
            {
                _cycleCount = 0;

                if (_scanLine < 261)
                    _scanLine++;
                else
                {
                    _scanLine = 0;
                    _isOddFrame = !_isOddFrame;
                    OnNewFrameAction();
                }
            }

            
        }

        private void OuterCycleAction()
        {
            if (_scanLine < 240)
            {
                //Skip the first cycle on the first scanline if an odd frame
                if (_scanLine == 0)
                {
                    //Copy Temporary to Current at the beginning of each Frame
                    //_currentAddress = _temporaryAddress;

                    if (_cycleCount == 0 && _isOddFrame)
                    {
                        WriteLog("Odd Frame, skipping first cycle");
                        _cycleCount++;
                    }
                }

                //Shift the Registers right one
                _upperShiftRegister >>= 1;
                _lowerShiftRegister >>= 1;
            }
            else if (_scanLine == 241 && _cycleCount == 2)
            {
                WriteLog("Setting _nmiOccurred");
                //StatusRegister |= 0x80;
                _nmiOccurred = true;

            }
            else if (_scanLine == 261)
            {
                if (_cycleCount == 1)
                {
                    //TODO: FIX these
                    //Clear Sprite 0 Hit
                    //StatusRegister &= byte.MaxValue ^ (1 << 6);
                    //Clear Sprite Overflow
                    //StatusRegister &= byte.MaxValue ^ (1 << 5);
                    _nmiOccurred = false;

                    WriteLog("Clearing _nmiOccurred");
                }
            }

            InnerCycleAction();
        }

        private void InnerCycleAction()
        {
            switch (_cycleCount)
            {
                //NameTable Address Fetch
                case 1:
                case 9:
                case 17:
                case 25:
                case 33:
                case 41:
                case 49:
                case 57:
                case 65:
                case 73:
                case 81:
                case 89:
                case 97:
                case 105:
                case 113:
                case 121:
                case 129:
                case 137:
                case 145:
                case 153:
                case 161:
                case 169:
                case 177:
                case 185:
                case 193:
                case 201:
                case 209:
                case 217:
                case 225:
                case 233:
                case 241:
                case 249:
                case 265:
                case 273:
                case 281:
                case 289:
                case 297:
                case 305:
                case 313:
                case 321:
                case 329:
                {
                    _nameTableAddress = 0x2000 | (_currentAddress & 0x0FFF);
                    break;
                }

                //NameTable Byte Store
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
                case 258:
                case 266:
                case 274:
                case 282:
                case 290:
                case 298:
                case 306:
                case 314:
                case 322:
                case 330:
                case 338:
                case 340:
                    {
                        _nameTableByte = _internalMemory[_nameTableAddress];
                        break;
                    }
                //Attribute table Address Fetch
                case 3:
                case 11:
                case 19:
                case 27:
                case 35:
                case 43:
                case 51:
                case 59:
                case 67:
                case 75:
                case 83:
                case 91:
                case 99:
                case 107:
                case 115:
                case 123:
                case 131:
                case 139:
                case 147:
                case 155:
                case 163:
                case 171:
                case 179:
                case 187:
                case 195:
                case 203:
                case 211:
                case 219:
                case 227:
                case 235:
                case 243:
                case 251:
                case 259:
                case 267:
                case 275:
                case 283:
                case 291:
                case 299:
                case 307:
                case 315:
                case 323:
                case 331:
                {
                    _attributeTableAddress = 0x23C0 | (_currentAddress & 0x0C00) | ((_currentAddress >> 4) & 0x38) |
                                             ((_currentAddress >> 2) & 0x07);
                    break;
                }
                //Attribute Table Store
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
                case 260:
                case 268:
                case 276:
                case 284:
                case 292:
                case 300:
                case 308:
                case 316:
                case 324:
                case 332:
                    {
                        _attributeByte = _internalMemory[_attributeTableAddress];
                        break;
                    }
                //LowBackground Tile Address Fetch
                case 5:
                case 13:
                case 21:
                case 29:
                case 37:
                case 45:
                case 53:
                case 61:
                case 69:
                case 77:
                case 85:
                case 93:
                case 101:
                case 109:
                case 117:
                case 125:
                case 133:
                case 141:
                case 149:
                case 157:
                case 165:
                case 173:
                case 181:
                case 189:
                case 197:
                case 205:
                case 213:
                case 221:
                case 229:
                case 237:
                case 245:
                case 253:
                case 261:
                case 269:
                case 277:
                case 285:
                case 293:
                case 301:
                case 309:
                case 317:
                case 325:
                case 333:
                {
                    _lowBackgroundTileAddress = (_nameTableByte << 4) | (_currentAddress >> 12) |
                                               ((ControlRegister & 0x10) << 8);
                    break;
                }
                //LowBackground Tile Byte Store
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
                case 262:
                case 270:
                case 278:
                case 286:
                case 294:
                case 302:
                case 310:
                case 318:
                case 326:
                case 334:
                    {
                        _lowBackgroundTileByte = _internalMemory[_lowBackgroundTileAddress];
                        break;
                    }
                //HighBackground Tile Address Fetch
                case 7:
                case 15:
                case 23:
                case 31:
                case 39:
                case 47:
                case 55:
                case 63:
                case 71:
                case 79:
                case 87:
                case 95:
                case 103:
                case 111:
                case 119:
                case 127:
                case 135:
                case 143:
                case 151:
                case 159:
                case 167:
                case 175:
                case 183:
                case 191:
                case 199:
                case 207:
                case 215:
                case 223:
                case 231:
                case 239:
                case 247:
                case 255:
                case 263:
                case 271:
                case 279:
                case 287:
                case 295:
                case 303:
                case 311:
                case 319:
                case 327:
                case 335:
                {
                    _highBackgroundTileAddress = _lowBackgroundTileAddress | 8;
                    break;
                }
                //HighBackground Tile Byte Store
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
                case 264:
                case 272:
                case 280:
                case 288:
                case 296:
                case 304:
                case 312:
                case 320:
                case 328:
                case 336:
                    {
                        _highBackgroundTileByte = _internalMemory[_highBackgroundTileAddress];
                        _upperShiftRegister |= _highBackgroundTileByte << 8;
                        _lowerShiftRegister |= _lowBackgroundTileByte << 8;

                        if (_cycleCount < 256 || _cycleCount > 327)
                        {
                            IncrementHorizontalCoordinate();
                        }

                        break;
                    }
                //Last Background Tile Fetch Cycle
                case 256:
                    {
                        _highBackgroundTileByte = _internalMemory[_lowBackgroundTileAddress | 8];
                        _upperShiftRegister |= _highBackgroundTileByte << 8;
                        _lowerShiftRegister |= _lowBackgroundTileByte << 8;

                        
                        IncrementVerticalCoordinate();
                        
                        break;
                    }
                case 257:
                    {
                        _nameTableAddress = 0x2000 | (_currentAddress & 0x0FFF);
                        _currentAddress = (_currentAddress & 0x7BE0) | (_temporaryAddress & 0x041F);
                        ObjectAttributeMemoryRegister = 0;
                        break;
                    }
            }

            if (_cycleCount < 256 || _cycleCount > 320)
                return;

            ObjectAttributeMemoryRegister = 0;


            if (_cycleCount > 279 && _cycleCount < 305)
            {
                _currentAddress = (_currentAddress & 0x041F) | (_temporaryAddress & 0x7BE0);
            }
        }

        #endregion

        #region Background Methods
        //This method increments the Horizonal Coordinate
        private void IncrementHorizontalCoordinate()
        {
            //Perform the Coarse X Increment
            //if ((_currentAddress & 0x001F) == 0x001F)
            //{
            //    _currentAddress ^= 0x041F;
            //    WriteLog(string.Format("IncrementV: Wrapping Occurred, _currentAddress is now {0}", _currentAddress));
            //}
            //else
            //{
            //    _currentAddress++;
            //    WriteLog(string.Format("IncrementV: Current Address Incremented, _currentAddress is now {0}", _currentAddress));
            //}
               
        }

        //This method increments the Vertical Coordinate on Cycle 256 of the scanline
        private void IncrementVerticalCoordinate()
        {
            if ((_currentAddress & 0x7000) != 0x7000)
            {
                //_currentAddress += 0x1000; // increment fine Y
                //WriteLog(string.Format("IncrementH: Current Address Incremented, _currentAddress is now {0}", _currentAddress));
            }
            else
            {
                _currentAddress &= ~0x7000;                    // fine Y = 0

                switch (_currentAddress & 0x3E0)
                {
                    case 0x3A0: _currentAddress ^= 0xBA0; break;
                    case 0x3E0: _currentAddress ^= 0x3E0; break;
                    default: _currentAddress += 0x20; break;
                        
                }

                WriteLog(string.Format("IncrementH: Wrapping Occurred, _currentAddress is now {0}", _currentAddress));
            }
        }
        #endregion

        #region Memory Methods
        private void LoadInitialMemory(CartridgeModel cartridgeModel)
        {
            Array.Copy(cartridgeModel.VROMBanks[0], _internalMemory, 8192);
        }

        private void ReadMemoryAction(int address)
        {
            switch (address)
            {
                //Reading from the Status Register
                case 0x2002:
                {
                   
                    if (_nmiOccurred)
                    {
                        StatusRegister |= 0x80;
                    }
                    else
                    {
                        StatusRegister &= byte.MaxValue ^ (1 << 7);
                    }

                    _tempAddressHasBeenWrittenTo = false;
                    _nmiOccurred = false;
                    break;
                }
                
                //Reading from the PPUData Register
                case 0x2007:
                    {
                        //If the _crrent address is not a palette read, it goes into a buffer. Otherwise
                        //It is ready directly
                        if (_currentAddress < 0x3F00)
                        {
                            DataRegister = _ppuDataReadBuffer;
                            _ppuDataReadBuffer = _internalMemory[_currentAddress];
                        }
                        else
                        {
                            DataRegister = _internalMemory[_currentAddress];
                            //PPU Memory Mirror fix
                            _ppuDataReadBuffer = _internalMemory[_currentAddress];
                        }

                        _currentAddress = (_currentAddress + _currentAddressIncrement) & 0x7FFF;
                        WriteLog(string.Format("Memory: 0x2007 Read, Current Address Incremented to {0}", _currentAddress));
                    }
                    break;
            }
        }

        private void WriteMemoryAction(int address, byte value)
        {
            switch (address)
            {
                case 0x2000:
                {
                    if (_internalResetFlag)
                        return;

                    _temporaryAddress |= (value & 0x3) << 10;

                    _nmiOutput = (ControlRegister & 0x80) != 0;

                    _currentAddressIncrement = ((value & 0x4) != 0) ? 32 : 1;

                    break;
                }
                case 0x2001:
                {
                    _isRenderingDisabled = (MaskRegister & 0x1E) == 0;
                    break;
                }
                case 0x2005:
                {
                    if (_internalResetFlag)
                        return;

                    if (!_tempAddressHasBeenWrittenTo)
                    {
                        _fineXScroll = value & 0x07;
                        _temporaryAddress = (value >> 3) & 0x1F;
                        WriteLog(string.Format("Memory: 0x2005 write, value {0} written to _temporaryAddress latch. Latch is now {1}", value, _temporaryAddress));
                    }
                    else
                    {
                        _temporaryAddress = (value & 7) << 12 | (value & 0x1f8) << 2;
                        WriteLog(string.Format("Memory: 0x2005 write x2, value {0} written to _temporaryAddress latch. Latch is now {1}", value, _temporaryAddress));
                    }
                    _tempAddressHasBeenWrittenTo = !_tempAddressHasBeenWrittenTo;

                    break;
                }
                case 0x2006:
                {
                    if (_internalResetFlag)
                        return;

                    if (!_tempAddressHasBeenWrittenTo)
                    {
                        _temporaryAddress = ((value & 0x3F) << 8);
                        WriteLog(string.Format("Memory: 0x2006 write, value {0} written to _temporaryAddress latch. Latch is now {1}", value, _temporaryAddress));
                    }
                    else
                    {
                        _temporaryAddress |= (value & 0xFF);
                        _currentAddress = _temporaryAddress;
                        WriteLog(string.Format("Memory: 0x2006 write, value {0} written to _temporaryAddress latch. Latch is now {1}, _currentAddress is now {2} ", value, _temporaryAddress, _currentAddress));
                    }
                    _tempAddressHasBeenWrittenTo = !_tempAddressHasBeenWrittenTo;

                    break;
                    
                }
                case 0x2007:
                {
                    //Mirroring
                    var mirrorAddress = 0;
                    
                    if (_currentAddress > 0x3F1F)
                    {
                        mirrorAddress = _currentAddress - 0x20;
                    }
                    else
                        mirrorAddress = _currentAddress;
                    
                    _internalMemory[mirrorAddress] = DataRegister;

                    _currentAddress = (_currentAddress + _currentAddressIncrement) & 0x7FFF;
                    WriteLog(string.Format("Memory: 0x2007 write, value {0} written to address {1}  Current Address Incremented to {2}", DataRegister, mirrorAddress, _currentAddress));
                    break;
                }
            }
        }
        #endregion

        #region Palette Methods
        private byte[] GetPatternTable(bool fetchPattern0)
        {
            var start = fetchPattern0 ? 0 : 0x1000;
            var end = fetchPattern0 ? 0x0FFF : 0x1FFF;

            var bits = new byte[49152];
            //We process across 8 vertical pixels for each iteration of the loop. The rowline, is used to calculate the correct offset in the array.
            var tileRow = 0;

            //This is the offset of each pixel column. 8 pixles * 4 bytes = 32 offset per tile
            var pixelColumnOffset = 0;
            for (var i = start; i < end; i++)
            {
                //Checking for 0 and 4096 so we don't increment the rowline on the very first loop!
                if ((i % 256) == 0 && i != 0 && i != 4096)
                {
                    tileRow++;
                    pixelColumnOffset = 0;
                }

                byte lowBit = _internalMemory[i];
                byte highBit = _internalMemory[i + 8];

                //Each pixel has 2 bits that control the color, a high bit and a low bit.
                //Each tile is 16 bytes.
                //$0xx0=$41  01000001
                //$0xx1=$C2  11000010
                //$0xx2=$44  01000100
                //$0xx3=$48  01001000
                //$0xx4=$10  00010000
                //$0xx5=$20  00100000         .1.....3
                //$0xx6=$40  01000000         11....3.
                //$0xx7=$80  10000000  =====  .1...3..
                //                            .1..3...
                //$0xx8=$01  00000001  =====  ...3.22.
                //$0xx9=$02  00000010         ..3....2
                //$0xxA=$04  00000100         .3....2.
                //$0xxB=$08  00001000         3....222
                //$0xxC=$16  00010110
                //$0xxD=$21  00100001
                //$0xxE=$42  01000010
                //$0xxF=$87  10000111

                var bit0 = _internalMemory[0x3F00 + (lowBit & 0x1) | ((highBit & 0x01) << 1)];
                var bit1 = _internalMemory[0x3F00 + ((lowBit & 0x2) >> 1) | (highBit & 0x02)];
                var bit2 = _internalMemory[0x3F00 + ((lowBit & 0x04) >> 2) | ((highBit & 0x04) >> 1)];
                var bit3 = _internalMemory[0x3F00 + ((lowBit & 0x08) >> 3) | ((highBit & 0x08) >> 2)];
                var bit4 = _internalMemory[0x3F00 + ((lowBit & 0x10) >> 4) | ((highBit & 0x10) >> 3)];
                var bit5 = _internalMemory[0x3F00 + ((lowBit & 0x20) >> 5) | ((highBit & 0x20) >> 4)];
                var bit6 = _internalMemory[0x3F00 + ((lowBit & 0x40) >> 6) | ((highBit & 0x40) >> 5)];
                var bit7 = _internalMemory[0x3F00 + (lowBit >> 7) | (((highBit & 0x80) >> 6))];

                var index = pixelColumnOffset + (tileRow * 3072);

                switch (i & 0x7)
                {
                    case 0:
                        {
                            SetColor(bits, index, bit7);
                            SetColor(bits, index + 3, bit6);
                            SetColor(bits, index + 6, bit5);
                            SetColor(bits, index + 9, bit4);
                            SetColor(bits, index + 12, bit3);
                            SetColor(bits, index + 15, bit2);
                            SetColor(bits, index + 18, bit1);
                            SetColor(bits, index + 21, bit0);
                            break;
                        }
                    case 1:
                        {
                            var offset = index + 384;
                            SetColor(bits, offset, bit7);
                            SetColor(bits, offset + 3, bit6);
                            SetColor(bits, offset + 6, bit5);
                            SetColor(bits, offset + 9, bit4);
                            SetColor(bits, offset + 12, bit3);
                            SetColor(bits, offset + 15, bit2);
                            SetColor(bits, offset + 18, bit1);
                            SetColor(bits, offset + 21, bit0);
                            break;
                        }
                    case 2:
                        {
                            var offset = index + 768;
                            SetColor(bits, offset, bit7);
                            SetColor(bits, offset + 3, bit6);
                            SetColor(bits, offset + 6, bit5);
                            SetColor(bits, offset + 9, bit4);
                            SetColor(bits, offset + 12, bit3);
                            SetColor(bits, offset + 15, bit2);
                            SetColor(bits, offset + 18, bit1);
                            SetColor(bits, offset + 21, bit0);
                            break;
                        }
                    case 3:
                        {
                            var offset = index + 1152;
                            SetColor(bits, offset, bit7);
                            SetColor(bits, offset + 3, bit6);
                            SetColor(bits, offset + 6, bit5);
                            SetColor(bits, offset + 9, bit4);
                            SetColor(bits, offset + 12, bit3);
                            SetColor(bits, offset + 15, bit2);
                            SetColor(bits, offset + 18, bit1);
                            SetColor(bits, offset + 21, bit0);
                            break;
                        }
                    case 4:
                        {
                            var offset = index + 1536;
                            SetColor(bits, offset, bit7);
                            SetColor(bits, offset + 3, bit6);
                            SetColor(bits, offset + 6, bit5);
                            SetColor(bits, offset + 9, bit4);
                            SetColor(bits, offset + 12, bit3);
                            SetColor(bits, offset + 15, bit2);
                            SetColor(bits, offset + 18, bit1);
                            SetColor(bits, offset + 21, bit0);
                            break;
                        }
                    case 5:
                        {
                            var offset = index + 1920;
                            SetColor(bits, offset, bit7);
                            SetColor(bits, offset + 3, bit6);
                            SetColor(bits, offset + 6, bit5);
                            SetColor(bits, offset + 9, bit4);
                            SetColor(bits, offset + 12, bit3);
                            SetColor(bits, offset + 15, bit2);
                            SetColor(bits, offset + 18, bit1);
                            SetColor(bits, offset + 21, bit0);
                            break;
                        }
                    case 6:
                        {
                            var offset = index + 2304;
                            SetColor(bits, offset, bit7);
                            SetColor(bits, offset + 3, bit6);
                            SetColor(bits, offset + 6, bit5);
                            SetColor(bits, offset + 9, bit4);
                            SetColor(bits, offset + 12, bit3);
                            SetColor(bits, offset + 15, bit2);
                            SetColor(bits, offset + 18, bit1);
                            SetColor(bits, offset + 21, bit0);
                            break;
                        }
                    case 7:
                        {
                            var offset = index + 2688;
                            SetColor(bits, offset, bit7);
                            SetColor(bits, offset + 3, bit6);
                            SetColor(bits, offset + 6, bit5);
                            SetColor(bits, offset + 9, bit4);
                            SetColor(bits, offset + 12, bit3);
                            SetColor(bits, offset + 15, bit2);
                            SetColor(bits, offset + 18, bit1);
                            SetColor(bits, offset + 21, bit0);

                            //Since we process both the high and low byte at once, 
                            //we need to skip the next 8 address in memory 
                            i += 8;
                            pixelColumnOffset += 24;
                            break;
                        }
                }
            }
            return bits;
        }

        private byte[] GetPalette(bool background)
        {
            var backgroundPalette = new byte[49152];
            var rowOffset = 0;

            var startposition = background ? 0x3F00 : 0x3F10;
            var endposition = background ? 0x3F10 : 0x3F20;

            for (var memoryLocation = startposition; memoryLocation < endposition; memoryLocation++)
            {
                WritePalette((memoryLocation & 3) == 0 ? 0x3f00 : memoryLocation, backgroundPalette, rowOffset);
                rowOffset += 96;
            }

            return backgroundPalette;
        }

        private void WritePalette(int palleteLocation, IList<byte> backgroundPalette, int columnStartPosition)
        {
            var paletteLookup = _internalMemory[palleteLocation];

            for (var rowOffset = 0; rowOffset < 32; rowOffset++)
            {
                for (var pixelOffset = 0; pixelOffset < 32; pixelOffset++)
                {
                    SetColor(backgroundPalette, columnStartPosition + (pixelOffset * 3) + (rowOffset * 1536), paletteLookup);
                }
            }
        }

        private static void SetColor(IList<byte> bits, int i, byte paletteLookup)
        {
            bits[i] = _pallet[paletteLookup * 3 + 2];
            bits[i + 1] = _pallet[paletteLookup * 3 + 1];
            bits[i + 2] = _pallet[paletteLookup * 3];

        }
        #endregion

        [Conditional("DEBUG")]
        private void WriteLog(string log)
        {
            _logger.DebugFormat("SL: {0} P: {1} IsOdd: {2} Rend: {3} NMIOccured: {4} NMIOutput: {5} {6}", _scanLine, _cycleCount, _isOddFrame, _isRenderingDisabled, _nmiOccurred, _nmiOutput, log);
        }
        #endregion
    }
}