using System;
using System.Diagnostics;
using dotnetNES.Engine.Models;
using NLog;

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
			get => _cpu.ReadMemoryValueWithoutCycle(0x2000);
		    set => _cpu.WriteMemoryValueWithoutCycle(0x2000, value);
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
			get => _cpu.ReadMemoryValueWithoutCycle(0x2001);
		    set => _cpu.WriteMemoryValueWithoutCycle(0x2001, value);
		}

		/// <summary>
		/// The PPUSTATUS Register. When the CPU reads this register, it clears bit 7 of this register after returning the register, and also clears the Scroll and address register.
		/// 0-4: LSB previously written into a PPU Register
		/// 5: Sprite Overflow flag
		/// 6: Sprite 0 Hit
		/// 7: Vblank 0= Not in Vblank 1= in Vblank
		/// </summary>
		private byte StatusRegister
		{
			//get { return _cpu.ReadMemoryValueWithoutCycle(0x2002); }
			set => _cpu.WriteMemoryValueWithoutCycle(0x2002, value);
		}

		/// <summary>
		/// The OAMADDR (Object Attribute Memory) Address
		/// </summary>
		private byte ObjectAttributeMemoryRegister
		{
			get => _cpu.ReadMemoryValueWithoutCycle(0x2004);
		    set => _cpu.WriteMemoryValueWithoutCycle(0x2004, value);
		}

		/// <summary>
		/// The PPUSCROLL register. 
		/// This tells the PPU which pixel of the nametable should be at the top left corner of the rendered screen.
		/// If it is changed during rendering, it will take effect during the next frame.
		/// </summary>
		private byte ScrollRegister
		{
			get => _cpu.ReadMemoryValueWithoutCycle(0x2005);
		    set => _cpu.WriteMemoryValueWithoutCycle(0x2005, value);
		}

		/// <summary>
		/// The PPUADDR register.
		/// This is the address that data in the PPU's internal memory will be written to.
		/// </summary>
		private byte AddressRegister
		{
			get => _cpu.ReadMemoryValueWithoutCycle(0x2006);
		    set => _cpu.WriteMemoryValueWithoutCycle(0x2006, value);
		}

		/// <summary>
		/// This allows data to be written from or to the PPU.
		/// </summary>
		private byte DataRegister
		{
			get => _cpu.ReadMemoryValueWithoutCycle(0x2007);
		    set => _cpu.WriteMemoryValueWithoutCycle(0x2007, value);
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
		/// The first write to the <see cref="ScrollRegister"/> will copy the upper five bitmapPointer of the write to that register into the temporary address
		/// The second write to the <see cref="ScrollRegister"/> will copy the lower 3 bitmapPointer to D14-D12 and the upper five bitmapPointer to D9-D5 in the register.
		/// 
		/// The first write to the <see cref="AddressRegister"/> will clear D14, and D13-D8 will be loaded with the lower six bitmapPointer of the value written
		/// The second write to the <see cref="AddressRegister"/> will be copied to D7-D0 of this register. After this write the temporary address will be copied into the <see cref="VRamAddress"/>
		/// 
		/// Writing to <see cref="ControlRegister"/> will copy the NameTable bitmapPointer from the ControlRegister into D10-D11
		/// 
		/// At the beginning of each frame, this address will be copied into <see cref="VRamAddress"/>. This will also occur from cycle 280 to 304
		/// At cycle 257 D10, and D4-D0 are coped into <see cref="VRamAddress"/>
		/// </summary>
		private int _temporaryAddress;		

		/// <summary>
		/// 
		/// </summary>
		private int _objectAttributeMemoryAddress;

        /// <summary>
        /// This is the current address. The PPU uses this address to get the data it needs to render a pixel.
        /// </summary>
        internal int VRamAddress { get; private set; }

        /// <summary>
        /// Controls the Fine X Scroll. On the first write to <see cref="ScrollRegister"/> the lower three bitmapPointer of the value are copied here
        /// </summary>
        internal int FineXScroll { get; private set; }
	   
		#endregion

		#region Background Latches
		internal int NameTableAddress { get; private set; }
		private int _attributeTableAddress;
		private int _highBackgroundTileAddress;
		private int _lowBackgroundTileAddress;
		private int _lowSpriteTileAddress;
		private int _highSpriteTileAddress;

		private byte _nameTableByte;
		private byte _attributeByte;
		private byte _highBackgroundTileByte;
		private byte _lowBackgroundTileByte;
		private byte _lowSpriteTileByte;
		private byte _highSpriteTileByte;


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
		internal int CycleCount;

		/// <summary>
		/// The current scanLine of the PPU
		/// </summary>
		internal int ScanLine;

	    /// <summary>
	    /// Odd frames skip a cycle on the first cycle and scanline
	    /// </summary>
	    private bool _isOddFrame;

		/// <summary>
		/// A Buffer for reads from ppu memory when the address is in the 0x0 to 0x3EFF range.
		/// </summary>
		private byte _ppuDataReadBuffer;

		/// <summary>
		/// this flag is set when 0x00 is written to the <see cref="MaskRegister"/>
		/// </summary>
		private bool _isRenderingDisabled;

        internal long TotalCycles { get; private set; }

        internal PPUStatusFlags PPUStatusFlags { get; set; } = new PPUStatusFlags();
        #endregion



        /// <summary>
        /// The CPU
        /// </summary>
        private readonly CPU _cpu;

        private static readonly ILogger Logger = LogManager.GetLogger("PictureProcessingUnit");

		//This contains all of the colors the NES can display converted into RGB format.
		private static readonly byte[] Pallet =
		{
			0x66, 0x66, 0x66, 0x00, 0x2a, 0x88, 0x14, 0x12, 0xa7, 0x3b, 0x00, 0xa4, 0x5c, 0x00, 0x7e, 0x6e,
			0x00, 0x40, 0x6c, 0x07, 0x00, 0x56, 0x1d, 0x00, 0x33, 0x35, 0x00, 0x0c, 0x48, 0x00, 0x00, 0x52,
			0x00, 0x00, 0x4f, 0x08, 0x00, 0x40, 0x4d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0xad, 0xad, 0xad, 0x15, 0x5f, 0xd9, 0x42, 0x40, 0xff, 0x75, 0x27, 0xfe, 0xa0, 0x1a, 0xcc, 0xb7,
			0x1e, 0x7b, 0xb5, 0x31, 0x20, 0x99, 0x4e, 0x00, 0x6b, 0x6d, 0x00, 0x38, 0x87, 0x00, 0x0d, 0x93,
			0x00, 0x00, 0x8f, 0x32, 0x00, 0x7c, 0x8d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0xff, 0xff, 0xff, 0x64, 0xb0, 0xff, 0x92, 0x90, 0xff, 0xc6, 0x76, 0xff, 0xf2, 0x6a, 0xff, 0xff,
			0x6e, 0xcc, 0xff, 0x81, 0x70, 0xea, 0x9e, 0x22, 0xbc, 0xbe, 0x00, 0x88, 0xd8, 0x00, 0x5c, 0xe4,
			0x30, 0x45, 0xe0, 0x82, 0x48, 0xcd, 0xde, 0x4f, 0x4f, 0x4f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0xff, 0xff, 0xff, 0xc0, 0xdf, 0xff, 0xd3, 0xd2, 0xff, 0xe8, 0xc8, 0xff, 0xfa, 0xc2, 0xff, 0xff,
			0xc4, 0xea, 0xff, 0xcc, 0xc5, 0xf7, 0xd8, 0xa5, 0xe4, 0xe5, 0x94, 0xcf, 0xef, 0x96, 0xbd, 0xf4,
			0xab, 0xb3, 0xf3, 0xcc, 0xb5, 0xeb, 0xf2, 0xb8, 0xb8, 0xb8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
		};

		/// <summary>
		/// This is used when mirroring is enable to flip the sprite
		/// </summary>
		private static readonly byte[] SpriteMirror =
		{
			0x00, 0x80, 0x40, 0xC0, 0x20, 0xA0, 0x60, 0xE0, 0x10, 0x90, 0x50, 0xD0, 0x30, 0xB0, 0x70, 0xF0,
			0x08, 0x88, 0x48, 0xC8, 0x28, 0xA8, 0x68, 0xE8, 0x18, 0x98, 0x58, 0xD8, 0x38, 0xB8, 0x78, 0xF8,
			0x04, 0x84, 0x44, 0xC4, 0x24, 0xA4, 0x64, 0xE4, 0x14, 0x94, 0x54, 0xD4, 0x34, 0xB4, 0x74, 0xF4,
			0x0C, 0x8C, 0x4C, 0xCC, 0x2C, 0xAC, 0x6C, 0xEC, 0x1C, 0x9C, 0x5C, 0xDC, 0x3C, 0xBC, 0x7C, 0xFC,
			0x02, 0x82, 0x42, 0xC2, 0x22, 0xA2, 0x62, 0xE2, 0x12, 0x92, 0x52, 0xD2, 0x32, 0xB2, 0x72, 0xF2,
			0x0A, 0x8A, 0x4A, 0xCA, 0x2A, 0xAA, 0x6A, 0xEA, 0x1A, 0x9A, 0x5A, 0xDA, 0x3A, 0xBA, 0x7A, 0xFA,
			0x06, 0x86, 0x46, 0xC6, 0x26, 0xA6, 0x66, 0xE6, 0x16, 0x96, 0x56, 0xD6, 0x36, 0xB6, 0x76, 0xF6,
			0x0E, 0x8E, 0x4E, 0xCE, 0x2E, 0xAE, 0x6E, 0xEE, 0x1E, 0x9E, 0x5E, 0xDE, 0x3E, 0xBE, 0x7E, 0xFE,
			0x01, 0x81, 0x41, 0xC1, 0x21, 0xA1, 0x61, 0xE1, 0x11, 0x91, 0x51, 0xD1, 0x31, 0xB1, 0x71, 0xF1,
			0x09, 0x89, 0x49, 0xC9, 0x29, 0xA9, 0x69, 0xE9, 0x19, 0x99, 0x59, 0xD9, 0x39, 0xB9, 0x79, 0xF9,
			0x05, 0x85, 0x45, 0xC5, 0x25, 0xA5, 0x65, 0xE5, 0x15, 0x95, 0x55, 0xD5, 0x35, 0xB5, 0x75, 0xF5,
			0x0D, 0x8D, 0x4D, 0xCD, 0x2D, 0xAD, 0x6D, 0xED, 0x1D, 0x9D, 0x5D, 0xDD, 0x3D, 0xBD, 0x7D, 0xFD,
			0x03, 0x83, 0x43, 0xC3, 0x23, 0xA3, 0x63, 0xE3, 0x13, 0x93, 0x53, 0xD3, 0x33, 0xB3, 0x73, 0xF3,
			0x0B, 0x8B, 0x4B, 0xCB, 0x2B, 0xAB, 0x6B, 0xEB, 0x1B, 0x9B, 0x5B, 0xDB, 0x3B, 0xBB, 0x7B, 0xFB,
			0x07, 0x87, 0x47, 0xC7, 0x27, 0xA7, 0x67, 0xE7, 0x17, 0x97, 0x57, 0xD7, 0x37, 0xB7, 0x77, 0xF7,
			0x0F, 0x8F, 0x4F, 0xCF, 0x2F, 0xAF, 0x6F, 0xEF, 0x1F, 0x9F, 0x5F, 0xDF, 0x3F, 0xBF, 0x7F, 0xFF,
		};

		/// <summary>
		/// This is set by <see cref="ControlRegister"/> bit 2
		/// </summary>
		private int _currentAddressIncrement;

		/// <summary>
		/// This is set by <see cref="ControlRegister"/> bit 5
		/// </summary>
		private int _backgroundPatternTableAddressOffset;

		/// <summary>
		/// This is set by <see cref="ControlRegister"/> bit 4
		/// </summary>
		private int _spritePatternTableAddressOffset;

		/// <summary>
		/// This is set by <see cref="ControlRegister"/> bit 6
		/// </summary>
		private bool _use8X16Sprite;

		/// <summary>
		/// This is used by the screen to draw to the correct location on the screen.
		/// </summary>
		private int _backgroundFrameIndex;

		/// <summary>
		/// This is the buffer for the frame. We only draw directly to this frame.
		/// </summary>
		private static byte[] _newFrame;

		/// <summary>
		/// Temp frame used when we swap frames
		/// </summary>
		private static byte[] _tempFrame;

		/// <summary>
		/// This buffer holds the 8 sprites that will be drawn on the next scanline
		/// </summary>
		private readonly byte[] _objectAttributeMemoryBufferNextLine = new byte[32];

		/// <summary>
		/// This buffer holds the 8 sprites that will be drawn on the current scanline
		/// </summary>
		private readonly byte[] _objectAttributeMemoryBufferCurrentLine = new byte[32];

		/// <summary>
		/// This array is used for sprite priority and holds a bool value that indicates if a sprite's pixel has already been drawn to the column index on the current row being rendered
		/// </summary>
		private readonly bool[] _spritePriorityMap = new bool[256];

		/// <summary>
		/// This array is used for sprite 0 detection, and holds a bool value that indicates if an opaque background pixel has already been drawn to the column index on the current row being rendered
		/// </summary>
		private readonly bool[] _backgroundPixelOpaqueMap = new bool[256];

		/// <summary>
		/// This is the evaluation state for sprites and is used for logic control.
		/// </summary>
		private int _spriteEvaluationState;
		/// <summary>
		/// The total number of sprites found during evaulation
		/// </summary>
		private int _totalSpritesFound;
		/// <summary>
		/// Holds the temporary sprite read from OAM during odd cycles
		/// </summary>
		private byte _tempSprite;

		/// <summary>
		/// This is the bit in the sprite currently being evaluated
		/// </summary>
		private int _spriteSubIndex;

		/// <summary>
		/// The current index of the OAM buffer. <see cref="_objectAttributeMemoryBufferNextLine"/>
		/// </summary>
		private byte _objectAttributeMemoryBufferIndex;

        /// <summary>
        /// Holds the current pixel being drawn
        /// </summary>
        private int _pixelIndex;
        #endregion

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
			ScanLine = 241;
			CycleCount = 0;
			_isRenderingDisabled = true;
            TotalCycles = 0;

            CurrentFrame = new byte[195840];
			_newFrame = new byte[195840];
			_tempFrame = new byte[195840];
		}
		#endregion

		#region Internal Properties
		/// <summary>
		/// This action is fired each time a new frame is available
		/// </summary>
		internal Action OnNewFrameAction { get; set; }

		/// <summary>
		/// The current frame being rendered
		/// </summary>
		internal static byte[] CurrentFrame;

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
			ScanLine = 0;
			CycleCount = 0;
			_isRenderingDisabled = true;
			_objectAttributeMemoryAddress = 0;
            TotalCycles = 0;

            CurrentFrame = new byte[195840];
			_newFrame = new byte[195840];
			_tempFrame = new byte[195840];
			Array.Clear(_objectAttributeMemory, 0, _objectAttributeMemory.Length);

            PPUStatusFlags.GenerateNMI = 0;
            PPUStatusFlags.SpriteOverflow = 0;
            PPUStatusFlags.SpriteZeroHit = 0;

            TotalCycles = 0;
		}

		internal void Power()
		{
			_isOddFrame = false;
			_tempAddressHasBeenWrittenTo = false;
			ScrollRegister = 0;
			DataRegister = 0;
			ControlRegister = 0;
			MaskRegister = 0;
			ScanLine = 0;
			CycleCount = 0;
			_isRenderingDisabled = true;
			_objectAttributeMemoryAddress = 0;
            TotalCycles = 0;

            CurrentFrame = new byte[195840];
			_newFrame = new byte[195840];
			_tempFrame = new byte[195840];
			Array.Clear(_objectAttributeMemory,0, _objectAttributeMemory.Length);

            PPUStatusFlags.GenerateNMI = 0;
            PPUStatusFlags.SpriteOverflow = 0;
            PPUStatusFlags.SpriteZeroHit = 0;

            TotalCycles = 0;            
        }



        /// <summary>
        /// This sets the Pattern table 1 stored between 0x00 and 0x0FF on its bitmap
        /// </summary>
        /// <param name="bitmapPointer">A pointer that points to the pattern table 1 bitmap</param>
        internal unsafe void DrawPatternTable0(byte* bitmapPointer)
		{
			DrawPatternTableToBitmapArray(bitmapPointer, true);
		}

		/// <summary>
		/// This sets the Pattern table 1 stored between 0x100 and 0x1FF on its bitmap
		/// </summary>
		/// <param name="bitmapPointer">A pointer that points to the pattern table 1 bitmap</param>
		internal unsafe void DrawPatternTable1(byte* bitmapPointer)
		{
			DrawPatternTableToBitmapArray(bitmapPointer, false);
		}

		/// <summary>
		/// Reads the PPU memory. This is mainly used by tests
		/// </summary>
		/// <param name="address">The address to read</param>
		/// <returns>a byte of memory</returns>
		internal byte ReadPPUMemory(int address)
		{
			return ReadInternalMemory(address);
		}

		/// <summary>
		/// Draws the background palette on its bitmap
		/// </summary>
		/// <param name="palettePointer">A pointer that points to the sprite bitmap</param>
		internal unsafe void DrawBackgroundPalette(byte* palettePointer)
		{
			WritePaletteToByteArray(palettePointer, true);
		}

		/// <summary>
		/// Draws the sprites palette on its bitmap
		/// </summary>
		/// <param name="palettePointer">A pointer that points to the sprite bitmap</param>
		internal unsafe void DrawSpritePalette(byte* palettePointer)
		{
			WritePaletteToByteArray(palettePointer, false);
		}

		/// <summary>
		/// Draws the nametable on its bitmap
		/// </summary>
		/// <param name="nameTablePointer">A pointer that points to the nametable bitmap</param>
		/// <param name="nameTableSelect">The nametable to select</param>
		internal unsafe void DrawNametable(byte* nameTablePointer, int nameTableSelect)
		{
			// 32 Tiles Wide
			// 30 Rows Tall
			var currentPosition = 0;
			var attributeTablePosition = 0;
			switch (nameTableSelect)
			{
				case 0:
					currentPosition = 0x2000;
					attributeTablePosition = 0x23bf;
					break;
				case 1:
					currentPosition = 0x2400;
					attributeTablePosition = 0x27bf;
					break;
				case 2:
					currentPosition = 0x2800;
					attributeTablePosition = 0x2Bbf;
					break;
				case 3:
					currentPosition = 0x2C00;
					attributeTablePosition = 0x2Fbf;
					break;
			}

			//Background pattern table address
			var attribute = 0;
			bool useTopByte = true;


			for (var tableRow = 0; tableRow < 30; tableRow++)
			{
				attributeTablePosition -= 8;

				if ((tableRow % 4) == 0)
				{
					attributeTablePosition += 8;
					useTopByte = true;
				}
				else if ((tableRow % 2) == 0)
				{
					useTopByte = false;
				}

				for (var tableColumn = 0; tableColumn < 32; tableColumn++)
				{
					if ((tableColumn % 4) == 0)
					{
						attributeTablePosition++;
						attribute = useTopByte ? _internalMemory[attributeTablePosition] & 0x03 : (_internalMemory[attributeTablePosition] >> 4) & 0x03;
					}
					else if ((tableColumn % 2) == 0)
					{
						attribute = useTopByte ? (_internalMemory[attributeTablePosition] >> 2) & 0x03 : (_internalMemory[attributeTablePosition] >> 6) & 0x03;
					}

					var nameTableByte = _internalMemory[currentPosition];
					DrawBackgroundTileToBitmapArray(nameTablePointer, 32, nameTableByte, _backgroundPatternTableAddressOffset, tableRow, tableColumn, attribute);
					currentPosition++;
				}
			}
		}
		#endregion

		#region Private Methods

		#region Main Loop
		private void CPUCycleCountIncremented()
		{
			StepPPU();           
            StepPPU();
			StepPPU();
		}

		internal void StepPPU()
		{
            ////WriteLog("Stepping PPU");
            TotalCycles++;  

            if (ScanLine == 240 && CycleCount == 340)
			{
				_isRenderingDisabled = true;
				OnNewFrameAction();
				SwapFrames();
			}
			else if (ScanLine == 261)
			{
				if (CycleCount == 0)
				{
                    _isRenderingDisabled = (MaskRegister & 0x18) == 0;
                    PPUStatusFlags.VerticalBlank = 0;
                    PPUStatusFlags.SpriteOverflow = 0;
                    PPUStatusFlags.SpriteZeroHit = 0;
                    //WriteLog("Clearing _nmiOccurred");
                }
                else if (CycleCount == 320)
				{
					_backgroundFrameIndex = 0;
                    Array.Clear(_backgroundPixelOpaqueMap, 0, 256);
				}
				else if (CycleCount == 339 && _isOddFrame && !_isRenderingDisabled)
				{
				    SpriteEvaluation();
				    InnerCycleAction();

                    //WriteLog("Odd Frame, skipping first cycle");
                    CycleCount = 0;
                    ScanLine = 0;
                    _isOddFrame = !_isOddFrame;

                    return;
				}
			}
			else if (ScanLine == 239 && CycleCount == 320)
			{
				_backgroundFrameIndex = 0;
			}

			if (!_isRenderingDisabled && (ScanLine < 240 || ScanLine == 261))
			{
				SpriteEvaluation();
				InnerCycleAction();
			}

		   if ((ScanLine == 240) && (CycleCount == 340))
            {
                PPUStatusFlags.VerticalBlank = 1;
                
                if (PPUStatusFlags.GenerateNMI == 1)
                {
                   _cpu.TriggerNmi = true;
                }                                     
            }				

			if (CycleCount < 340)
				CycleCount++;
			else
			{
				CycleCount = 0;

				if (ScanLine < 261)
				{
					ScanLine++;
				}				
				else
				{
					ScanLine = 0;
					_isOddFrame = !_isOddFrame;                   
                }
			}
		}

		//This method handles the sprite evaluation logic.
		// 1. Cycles 1-64 Initialize the OAM Buffer to $FF
		private void SpriteEvaluation()
		{
			//Cylces 0-63 Initialize the Buffer to 0xFF
			if (CycleCount < 64)
			{
				_objectAttributeMemoryBufferNextLine[(CycleCount >> 1)] = 0xFF;
				//WriteSpriteEvaluationLog(string.Format("Setting OAM Buffer at position {0} to 0xFF", (CycleCount >> 1)));
			}
			//Cycles 63 - 255 Sprite Evauation
			else if (CycleCount < 256)
			{
				if (CycleCount == 64)
				{
					//WriteSpriteEvaluationLog("Cycle64 Reset Occurred");
					
					_totalSpritesFound = 0;
					_tempSprite = 0;
					_objectAttributeMemoryBufferIndex = 0;
					_spriteEvaluationState = 0;
				}

				//Odd Cycle: Read Data from OAM
				if ((CycleCount & 1) == 0)
				{
					_tempSprite = _objectAttributeMemory[_objectAttributeMemoryAddress];
				    
					//WriteSpriteEvaluationLog(string.Format("Read Cycle TempSprite = {0} from Index {1}", _tempSprite, _objectAttributeMemoryAddress));
				}
				//Even Cycle: Write Data to OAM
				else
				{
					switch (_spriteEvaluationState)
					{
						case 0:
						{
                            //_objectAttributeMemoryAddress = _spritePosition;
                            _objectAttributeMemoryBufferNextLine[_objectAttributeMemoryBufferIndex] = _tempSprite;						  

							//Determine if current Y Coordinate is in range for this scanline
							if ((ScanLine >= _tempSprite) &&
								(ScanLine <= _tempSprite + (_use8X16Sprite ? 0xF : 0x7)))
							{
								//Y Coordinate is in range
								_spriteEvaluationState = 1;
								_totalSpritesFound++;
								_spriteSubIndex = 0;
								_objectAttributeMemoryAddress++;
							    _objectAttributeMemoryBufferIndex++;
							}
							else
							{
								_objectAttributeMemoryAddress += 4;

								if (_objectAttributeMemoryAddress >= 256)
								{
									_objectAttributeMemoryAddress = 0;
									_spriteEvaluationState = 4;
								}
                                else if (_totalSpritesFound == 8)
                                {
                                    _spriteEvaluationState = 2;
                                }
							}

							break;
						}
						case 1:
						{
							//Copy the remaining Data to the Buffer
							_objectAttributeMemoryBufferNextLine[_objectAttributeMemoryBufferIndex++] = _tempSprite;
							_objectAttributeMemoryAddress++;

							//All 4 bytes have been copied to the buffer
							if (++_spriteSubIndex == 3)
							{
								_spriteSubIndex = 0;

								if (_objectAttributeMemoryAddress == 256)
								{
									_objectAttributeMemoryAddress = 0;
									_spriteEvaluationState = 4;
								}
								else if (_totalSpritesFound == 8)
								{
									_spriteEvaluationState = 2;
								}
								else
								{
									_spriteEvaluationState = 0;
								}
							}
							break;
						}
						case 2:
						{
							//Determine if current Y Coordinate is in range for this scanline for the 9th sprite
							if ((ScanLine >= _tempSprite) &&
								(ScanLine <= _tempSprite + (_use8X16Sprite ? 0xF : 0x7)))
							{
                                //Set the sprite overflow flag
                                PPUStatusFlags.SpriteOverflow = 1;
								
								_spriteEvaluationState = 3;
								_objectAttributeMemoryBufferIndex = 1;

								_objectAttributeMemoryAddress = 1;
								_spriteSubIndex = 1;
							}
							else
							{
								//Sprite not in range. Increment Hardware Bug occurred.
								_objectAttributeMemoryAddress += 5;

								if (_objectAttributeMemoryAddress >= 256)
								{
									_objectAttributeMemoryAddress = 0;
									_spriteEvaluationState = 4;
								}
							}
							break;
						}
						case 3:
						{
							if (++_spriteSubIndex == 3)
							{
								_spriteSubIndex = 0;
								_objectAttributeMemoryAddress++;
							}

							if (_objectAttributeMemoryAddress == 256)
							{
								_objectAttributeMemoryAddress = 0;
								_spriteEvaluationState = 4;
							}

							break;
						}
						case 4:
						{
							_objectAttributeMemoryAddress++;

							if (_objectAttributeMemoryAddress == 256)
							{
								_objectAttributeMemoryAddress = 0;
							}
							break;
						}
						default:
						{
							Debug.Assert(false, "Invalid _spriteEvaluationState");
							break;
						}
					}
				}

			}
			else if (CycleCount < 320)
			{
				_objectAttributeMemoryAddress = 0;
			}			
		}

		private static void SwapFrames()
		{
			_tempFrame = CurrentFrame;
			CurrentFrame = _newFrame;
			_newFrame = _tempFrame;		   
		}

		private void InnerCycleAction()
		{
			switch (CycleCount)
			{
				#region Background
				//NameTable Address Fetch
				case 0:
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
				case 320:
				case 328:
					{
						NameTableAddress = 0x2000 | (VRamAddress & 0x0FFF);
						break;
					}

				//NameTable Byte Store
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
				case 321:
				case 329:
					{
						_nameTableByte = ReadInternalMemory(NameTableAddress);
						break;
					}
				//Attribute table Address Fetch
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
					{
						_attributeTableAddress = 0x23C0 | (VRamAddress & 0xC00) | ((VRamAddress & 0x380) >> 4) | ((VRamAddress & 0x1C) >> 2);
						break;
					}
				//Attribute Table Store
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
				case 323:
				case 331:
					{
						_attributeByte = ReadInternalMemory(_attributeTableAddress);
						break;
					}
				//LowBackground Tile Address Fetch
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
						_lowBackgroundTileAddress = (_nameTableByte << 4) | (VRamAddress >> 12) | (_backgroundPatternTableAddressOffset);
						break;
					}
				//LowBackground Tile Byte Store
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
				case 325:
				case 333:
					{
						_lowBackgroundTileByte = _internalMemory[_lowBackgroundTileAddress];
						break;
					}
				//HighBackground Tile Address Fetch
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
						_highBackgroundTileAddress = _lowBackgroundTileAddress | 8;
						break;
					}
				//HighBackground Tile Byte Store
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
				case 327:
				case 335:
					{
						_highBackgroundTileByte = _internalMemory[_highBackgroundTileAddress];
						if (CycleCount < 256 || CycleCount > 327)
						{
							IncrementHorizontalCoordinate();
						}

						//RenderBackground tile
						DrawBackgroundToScreen();

						break;
					}
				//Last Background Tile Fetch Cycle
				case 256:
					{
						_highBackgroundTileByte = _internalMemory[_lowBackgroundTileAddress | 8];
						IncrementVerticalCoordinate();

						//Garbage NT Fetch
						NameTableAddress = 0x2000 | (VRamAddress & 0x0FFF);

						Array.Copy(_objectAttributeMemoryBufferNextLine, _objectAttributeMemoryBufferCurrentLine, 32);
						Array.Clear(_spritePriorityMap, 0, 256);
						break;
					}
				case 257:
					{
						//WriteLog("Setting Hori(V) = Hori(T)");
						VRamAddress = (VRamAddress & 0x7BE0) | (_temporaryAddress & 0x041F);

						ObjectAttributeMemoryRegister = 0;
						
						//Garbage NT Byte
						_nameTableByte = ReadInternalMemory(NameTableAddress);
					    _pixelIndex = 0;
						break;
					}
				#endregion

				#region Sprites
				//Garbage nametable Address
				case 264:	
				case 272:
				case 280:	
				case 288:
				case 296:
				case 304:
				case 312:
					{
						NameTableAddress = 0x2000 | (VRamAddress & 0x0FFF);
						break;
					}
				//Garbarge nametable Byte
				case 265:	
				case 273:	
				case 281:	
				case 289:	
				case 297:	
				case 305:	
				case 313:
				{
					_nameTableByte = ReadInternalMemory(NameTableAddress);
					break;
				}
				//Garbage attribute table address
				case 258:	
				case 266:	
				case 274:	
				case 282:	
				case 290:	
				case 298:	
				case 306:	
				case 314:
				{
					_attributeTableAddress = 0x23C0 | (VRamAddress & 0xC00) | ((VRamAddress & 0x380) >> 4) | ((VRamAddress & 0x1C) >> 2);

					break;
				}
				//Garbage attibute table byte
				case 259:
				case 267:
				case 275:
				case 283:
				case 291:	
				case 299:
				case 307:
				case 315:
				{
					_attributeByte = ReadInternalMemory(_attributeTableAddress);
					break;
				}
				//Sprite Tile Fetch Low Address
				case 260:	
				case 268:	
				case 276:	
				case 284:	
				case 292:	
				case 300:	
				case 308:	
				case 316:
				{
					var address = (CycleCount >> 3 & 7) * 4;

					_lowSpriteTileAddress = ((ControlRegister & 0x8) == 0x8) ? 0x1000 : 0 | (_objectAttributeMemoryBufferCurrentLine[address + 1] * 16);
					_lowSpriteTileAddress += (ScanLine - _objectAttributeMemoryBufferCurrentLine[address]);
						break;
				}
				//Sprite Tile Fetch Low Byte
				case 261:
				case 269:
				case 277:
				case 285:
				case 293:
				case 301:
				case 309:
				case 317:
				{
					//Get the low byte of the tile. We need to check the sprite to see if its been mirrored and apply the mirrored version if so.
					_lowSpriteTileByte = (_objectAttributeMemoryBufferCurrentLine[((CycleCount >> 3 & 7)*4) + 2] & 0x40) == 0
						? _internalMemory[_lowSpriteTileAddress]
						: SpriteMirror[_internalMemory[_lowSpriteTileAddress]];
					break;
				}
				//Sprite Tile Fetch High Address
				case 262:	
				case 270:	
				case 278:	
				case 286:	
				case 294:	
				case 302:	
				case 310:	
				case 318:
				{
					_highSpriteTileAddress = _lowSpriteTileAddress | 0x08;
					break;
				}
				//Sprite Tile Fetch High Byte
				case 263:
				case 271:
				case 279:
				case 287:
				case 295:
				case 303:
				case 311:
				case 319:
				{
					//Get the high byte of the tile. We need to check the sprite to see if its been mirrored and apply the mirrored version if so.
					_highSpriteTileByte = (_objectAttributeMemoryBufferCurrentLine[((CycleCount >> 3 & 7) * 4) + 2] & 0x40) == 0
						? _internalMemory[_highSpriteTileAddress]
						: SpriteMirror[_internalMemory[_highSpriteTileAddress]];

					DrawSpriteToScreen();
					break;
				}
				#endregion
			}

			if (CycleCount < 256 || CycleCount > 320)
				return;

			ObjectAttributeMemoryRegister = 0;
			
			if (ScanLine == 261 && CycleCount > 279 && CycleCount < 305)
			{
				//WriteLog("Setting Vert(V) = Vert(T)");
				VRamAddress = (VRamAddress & 0x041F) | (_temporaryAddress & 0x7BE0);
			}
		}
		#endregion

		#region Background Methods
		//This method increments the Horizonal Coordinate
		private void IncrementHorizontalCoordinate()
		{
			//Perform the Coarse X Increment
			if ((VRamAddress & 0x001F) == 0x001F)
			{
				VRamAddress ^= 0x041F;
				//WriteLog("IncrementH: Wrapping Occurred");
			}
			else
			{
				VRamAddress++;
				//WriteLog("IncrementH: Current Address Incremented");
			}
		}

		//This method increments the Vertical Coordinate on Cycle 256 of the scanline
		private void IncrementVerticalCoordinate()
		{
			if ((VRamAddress & 0x7000) != 0x7000)
			{
				VRamAddress += 0x1000; // increment fine Y
				//WriteLog(string.Format("IncrementH: Current Address Incremented, _currentAddress is now {0}", VRamAddress));
			}
			else
			{
				VRamAddress ^= 0x7000;

				switch (VRamAddress & 0x3E0)
				{
					case 0x3A0: VRamAddress ^= 0xBA0; break;
					case 0x3E0: VRamAddress ^= 0x3E0; break;
					default: VRamAddress += 0x20; break;
				}

				//WriteLog(string.Format("IncrementH: Wrapping Occurred, _currentAddress is now {0}", VRamAddress));
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
                        StatusRegister = (byte)((PPUStatusFlags.SpriteOverflow << 5) |
                            (PPUStatusFlags.SpriteZeroHit << 6) |
                            (PPUStatusFlags.VerticalBlank << 7));

                        _tempAddressHasBeenWrittenTo = false;

                        PPUStatusFlags.VerticalBlank = 0;                        

                        if (ScanLine == 241 && CycleCount < 3)
                        {                           
                            PPUStatusFlags.VerticalBlank = 0;
                            _cpu.TriggerNmi = false;

                            if (CycleCount == 0)
                            {
                                //"Reading one PPU clock before vblank is set reads it as clear and never sets the flag or generates NMI for that frame. "
                                StatusRegister = (byte)((PPUStatusFlags.SpriteOverflow << 5) | (PPUStatusFlags.SpriteZeroHit << 6));
                            }                                                     
                        }                       

                        break;
				}
				case 0x2004:
				{
					if (!_isRenderingDisabled)
					{
						//If the cycle is less than 64 we return 0xFF
						if (CycleCount < 64)
						{
							_temporaryAddress = 0xff;
						}
						else if (CycleCount < 192)
						{
							_temporaryAddress = _objectAttributeMemory[((CycleCount - 64) << 1) & 0xFC];
						}
						else if (CycleCount < 256)
						{
							_temporaryAddress = ((CycleCount & 0x01) == 0x01)
								? _objectAttributeMemory[0xFC]
								: _objectAttributeMemory[((CycleCount - 192) << 1) & 0xFC];
						}
						else if (CycleCount < 320)
						{
							_temporaryAddress = 0xFF;
						}
						else
						{
							_temporaryAddress = _objectAttributeMemory[0];
						}
					}
					else
					{
						_temporaryAddress = _objectAttributeMemory[_objectAttributeMemoryAddress];
					}

					break;
				}
				//Reading from the PPUData Register
				case 0x2007:
				{
					//If the _crrent address is not a palette read, it goes into a buffer. Otherwise
					//It is ready directly
					if (VRamAddress  < 0x3F00)
					{
						DataRegister = _ppuDataReadBuffer;
						_ppuDataReadBuffer = ReadInternalMemory(VRamAddress);
					}
					else
					{

						DataRegister = ReadInternalMemory(VRamAddress);

						//When the PPU returns a palette byte, it sets the buffer differently
						var tempAddress = VRamAddress & 0x2FFF;
						_ppuDataReadBuffer = ReadInternalMemory(tempAddress);
					}

					VRamAddress = (VRamAddress + _currentAddressIncrement) & 0x7FFF;
					//WriteLog(string.Format("Memory: 0x2007 Read, Current Address Incremented to {0}", VRamAddress));
					break;
				}
			}
		}

		private void WriteMemoryAction(int address, byte value)
		{
			switch (address)
			{
				#region Internal PPU Registers
				case 0x2000:
				{
					_temporaryAddress = (_temporaryAddress & 0x73FF) | ((value & 0x3) << 10);

                    //"By toggling NMI_output ($2000 bit 7) during vertical blank without reading $2002, a program can cause /NMI to be pulled low multiple times, causing multiple NMIs to be generated."
                    int originalGenerateNmi = PPUStatusFlags.GenerateNMI;

                     PPUStatusFlags.GenerateNMI = (value & 0x80) == 0x80 ? 1 : 0;

                    if (originalGenerateNmi == 0 && PPUStatusFlags.GenerateNMI == 1 && PPUStatusFlags.VerticalBlank == 1 && (ScanLine != 261 || CycleCount != 0))
                    {
                           _cpu.TriggerNmi = true;
                    }
                    if (ScanLine == 241 && CycleCount < 3 && PPUStatusFlags.GenerateNMI == 0)
                    {
                            _cpu.TriggerNmi = false;
                    }

                    _currentAddressIncrement = ((value & 0x4) != 0) ? 32 : 1;
					_backgroundPatternTableAddressOffset = ((value & 0x10) != 0) ? 0x1000 : 0x0000;
					_spritePatternTableAddressOffset = ((value & 0x08) != 0) ? 0x1000 : 0x0000;
					_use8X16Sprite = (value & 0x20) != 0;
					break;
				}
				case 0x2001:
				{
					_isRenderingDisabled = (MaskRegister & 0x18) == 0 && ScanLine < 240;
					break;
				}
				case 0x2003:// $2003
				{
					_objectAttributeMemoryAddress = value;
					break;
				}
				case 0x2004:// $2004
				{
					if (!_isRenderingDisabled)
					{
						//value = 0xFF;
					}

					if ((_objectAttributeMemoryAddress & 0x03) == 0x02)
					{
						value &= 0xE3;
					}
					
					_objectAttributeMemory[_objectAttributeMemoryAddress++] = value;

					if (_objectAttributeMemoryAddress == 256)
						_objectAttributeMemoryAddress = 0;

					break;
				}
				case 0x2005:
				{
				   
					if (!_tempAddressHasBeenWrittenTo)
					{
						FineXScroll = value & 0x07;
						_temporaryAddress = (_temporaryAddress & 0x7FE0) | ((value & 0xF8) >> 3);
						//WriteLog(string.Format("Memory: 0x2005 write, value {0} written to _temporaryAddress latch. Latch is now {1}", value, _temporaryAddress));
					}
					else
					{
						_temporaryAddress = (_temporaryAddress & 0x0C1F) | ((value & 0x7) << 12) | ((value & 0xF8) << 2);
						//WriteLog(string.Format("Memory: 0x2005 write x2, value {0} written to _temporaryAddress latch. Latch is now {1}", value, _temporaryAddress));
					}
					_tempAddressHasBeenWrittenTo = !_tempAddressHasBeenWrittenTo;

					break;
				}
				case 0x2006:
				{
					if (!_tempAddressHasBeenWrittenTo)
					{
						_temporaryAddress = (_temporaryAddress & 0x00FF) | ((value & 0x3F) << 8);
						//WriteLog(string.Format("Memory: 0x2006 write, value {0} written to _temporaryAddress latch. Latch is now {1}", value, _temporaryAddress));
					}
					else
					{
						_temporaryAddress = (_temporaryAddress & 0x7F00) | value;
						VRamAddress = _temporaryAddress;
						//WriteLog(string.Format("Memory: 0x2006 write, value {0} written to _temporaryAddress latch. Latch is now {1}, _currentAddress is now {2} ", value, _temporaryAddress, VRamAddress));
					}
					_tempAddressHasBeenWrittenTo = !_tempAddressHasBeenWrittenTo;

					break;
					
				}
				case 0x2007:
				{
					WriteInternalMemory(VRamAddress, DataRegister);
					VRamAddress = (VRamAddress + _currentAddressIncrement) & 0x7FFF;
					break;
				}
				#endregion
			}
		}
		
		private byte ReadInternalMemory(int originalAddress)
		{
			var tempAddress = originalAddress & 0x3FFF;

			//Handling Wrapping in the Palette memory
			if (tempAddress > 0x2FFF)
			{
				tempAddress &= 0x3f1f;
				
			}
			//Handling wrapping in the nametable memory
			else if (tempAddress > 0x2fff && tempAddress < 0x3f00)
			{
				tempAddress -= 0x1000;
			}

			return _internalMemory[tempAddress];
		}

		private void WriteInternalMemory(int originalAddress, byte value)
		{
			var tempAddress = originalAddress & 0x3FFF;

			//Handling Wrapping in the Palette memory
			if (tempAddress > 0x2FFF)
			{
				tempAddress &= 0x3f1f;
				value &= 0x3f;
				_internalMemory[tempAddress] = value;

				//Handling Writing to the palette
				if ((tempAddress & 0x3) == 0x00)
				{
					_internalMemory[tempAddress ^ 0x10] = value;
				}

				return;
			}
			//Handling wrapping in the nametable memory
			if (tempAddress > 0x2fff)
			{
				tempAddress -= 0x1000;
			}

			//WriteLog(string.Format("Memory: write, value {0} written to address {1}", value, _temporaryAddress));
			_internalMemory[tempAddress] = value;
		}
		#endregion

		#region Drawing Methods
		/// <summary>
		/// Draws the sprite on its bitmap
		/// </summary>
		/// <param name="spritePointer">A pointer that points to the sprite bitmap</param>
		/// <param name="spriteIndex">The sprite to select</param>
		internal unsafe void DrawSprite(byte* spritePointer, int spriteIndex)
		{
			var spriteOffset = spriteIndex * 4;

			//0 = Bank ($0000 or $1000) of tiles
			//1-7 = Tile number of top of sprite (0 to 254; bottom half gets the next tile)
			var byte0 = _objectAttributeMemory[spriteOffset++];
			var byte1 = _objectAttributeMemory[spriteOffset++];
			var byte2 = _objectAttributeMemory[spriteOffset++];
			var byte3 = _objectAttributeMemory[spriteOffset];
			
			var paletteIndex = (byte2 & 0x3);

            //Calculate the starting place in memory of the tile. 
            var tileMemoryIndex = (16 * byte1) + _spritePatternTableAddressOffset;

		    var pixelArrayOffsetPosition = 0;

            //Iterate of each row of the tile and draw the array
            for (var pixelRow = 0; pixelRow < 8; pixelRow++)
            {
                if ((tileMemoryIndex & 0x07) != 0)
                    pixelArrayOffsetPosition = (24 * (tileMemoryIndex & 0x07));

                    DrawSpriteTileRowToBitmapArray(spritePointer, _internalMemory[tileMemoryIndex], _internalMemory[tileMemoryIndex + 8], pixelArrayOffsetPosition, paletteIndex, true, false, 7, true);

                tileMemoryIndex++;
            }
        }

		private unsafe void DrawSpriteToScreen()
		{
			//Draw Sprite here
			var address = (CycleCount >> 3 & 7) * 4;

			//X-Scroll values greater than F8 cause the sprite to be invisble
			var xCoordinate = _objectAttributeMemoryBufferCurrentLine[address + 3] + 8;

			if (xCoordinate > 0xf8)
				return;

			var paletteIndex = _objectAttributeMemoryBufferCurrentLine[address + 2] & 0x3;
		   
			var pixelStartPosition = ScanLine == 261 ? ((xCoordinate) * 3) : ((ScanLine)*816) + ((xCoordinate) * 3);

			fixed (byte* framePointer = _newFrame)
			{

				DrawSpriteTileRowToBitmapArray(framePointer, _lowSpriteTileByte, _highSpriteTileByte, pixelStartPosition, paletteIndex, (_objectAttributeMemoryBufferCurrentLine[address + 2] & 20) != 0x20, address == 0, xCoordinate + 7, false);
			}
		}

		/// <summary>
		/// This method is used during emulation to draw the current background row to the screen. 
		/// </summary>
		private unsafe void DrawBackgroundToScreen()
		{
			//There are 4 offsets per attribute byte top left, top right, bottom left and bottom right. 
			//The correct offset is calculated as follows. Every 63 bytes we flip from D0-D3 to D4-D7
			//Every 2 bytes we flip between D0-D1 or D4-D5 to D2-D3 or D6-D7
			var attributeOffset = (NameTableAddress & 0x40) == 0x40 ? 4 : 0;
			
			if ((NameTableAddress & 0x3) > 1)
				attributeOffset += 2; 

			fixed (byte* framePointer = _newFrame)
			{
				DrawBackgroundTileRowToBitmapArray(framePointer, _lowBackgroundTileByte, _highBackgroundTileByte, _backgroundFrameIndex, (_attributeByte >> attributeOffset) & 0x3);
			}
			_backgroundFrameIndex += 24;
		}

		/// <summary>
		/// Draws the entire pattern table to the bitmap array passed in
		/// </summary>
		/// <param name="bitmapArray">the bitmap to draw to</param>
		/// <param name="fetchPattern0">Determines if pattern 0 or pattern 1 is drawn to</param>
		private unsafe void DrawPatternTableToBitmapArray(byte* bitmapArray, bool fetchPattern0)
		{
			//Iterate over each row and column
			for (var row = 0; row < 16; row++)
			{
				 for (var column = 0; column < 16; column++)
				 {
					 DrawBackgroundTileToBitmapArray(bitmapArray, 16, (row * 16) + column, fetchPattern0 ? 0 : 0x1000, row, column, 0);
				 }
			}
		}

		/// <summary>
		/// Draws a tile to a bitmap array. This is used to output the background tiles, sprits, and nametables to the debug windows.
		/// </summary>
		/// <param name="bitmapArray">The array being drawn to</param>
		/// <param name="totalColumns">The total number of columns of tiles the bitmap array contains</param>
		/// <param name="tileAddress">The address of the tile to draw</param>
		/// <param name="tileOffset">the amount the tile address is offset. This is controlled by <see cref="ControlRegister"/> bit 4 or 5 depending on if its a sprite or a background tile</param>
		/// <param name="row">The row in the bitmap array this tile is being drawn on</param>
		/// <param name="column">The column in the bitmap array this tile is being drawn on</param>
		/// <param name="paletteOffset">The offset we are using when drawing. This determines which palette we are using</param>
		private unsafe void DrawBackgroundTileToBitmapArray(byte* bitmapArray, int totalColumns, int tileAddress, int tileOffset, int row, int column, int paletteOffset)
		{
			//Calculate the starting place in memory of the tile. 
			var tileMemoryIndex = (16 * tileAddress) + tileOffset;
		   

			//Calculate the StartPosition of the first pixel in the array;
			var pixelArrayInitialPosition = (row * totalColumns * 192) + (column * 24);
			var pixelArrayOffsetPosition = pixelArrayInitialPosition;

			//Iterate of each row of the tile and draw the array
			for (var pixelRow = 0; pixelRow < 8; pixelRow++)
			{
				//offset the array
				if ((tileMemoryIndex & 0x07) != 0)
					pixelArrayOffsetPosition = pixelArrayInitialPosition + (totalColumns*24 * (tileMemoryIndex & 0x07));
                
				DrawBackgroundTileRowToBitmapArray(bitmapArray, _internalMemory[tileMemoryIndex], _internalMemory[tileMemoryIndex + 8], pixelArrayOffsetPosition, paletteOffset);
				
				tileMemoryIndex++;
			}
		}
        

		/// <summary>
		/// Draws a single row of the tile to the bitmap array
		/// </summary>
		/// <param name="bitmapArray">The bitmap array we are drawing the tile on</param>
		/// <param name="lowTileByte">The low byte of the tile</param>
		/// <param name="highTileByte">The high byte of the tile</param>
		/// <param name="bitmapArrayStartIndex">The starting position of where we are drawing the tile on</param>
		/// <param name="paletteIndex">The offset we are using when drawing. This determines which palette we are using</param>
		private unsafe void DrawBackgroundTileRowToBitmapArray(byte* bitmapArray, byte lowTileByte, byte highTileByte, int bitmapArrayStartIndex, int paletteIndex)
		{
            //Each pixel has 2 bitmapPointer that control the color, a high bit and a low bit.
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

            //Calculating each bit
		    //var xPixelCoordinate = bitmapArrayStartIndex + 7;
		    //bitmapArrayStartIndex *= 3;
		    _pixelIndex += 7;

            var bit0 = (lowTileByte & 0x1) | ((highTileByte & 0x01) << 1);
            DrawBackgroundPixelToByteArray(bitmapArray, bitmapArrayStartIndex + 21, bit0, paletteIndex << 2, _pixelIndex--);

            var bit1 = ((lowTileByte & 0x2) >> 1) | (highTileByte & 0x02);
            DrawBackgroundPixelToByteArray(bitmapArray, bitmapArrayStartIndex + 18, bit1, paletteIndex << 2, _pixelIndex--);

            var bit2 = ((lowTileByte & 0x04) >> 2) | ((highTileByte & 0x04) >> 1);
            DrawBackgroundPixelToByteArray(bitmapArray, bitmapArrayStartIndex + 15, bit2, paletteIndex << 2, _pixelIndex--);

            var bit3 = ((lowTileByte & 0x08) >> 3) | ((highTileByte & 0x08) >> 2);
            DrawBackgroundPixelToByteArray(bitmapArray, bitmapArrayStartIndex + 12, bit3, paletteIndex << 2, _pixelIndex--);

            var bit4 = ((lowTileByte & 0x10) >> 4) | ((highTileByte & 0x10) >> 3);
            DrawBackgroundPixelToByteArray(bitmapArray, bitmapArrayStartIndex + 9, bit4, paletteIndex << 2, _pixelIndex--);

            var bit5 = ((lowTileByte & 0x20) >> 5) | ((highTileByte & 0x20) >> 4);
            DrawBackgroundPixelToByteArray(bitmapArray, bitmapArrayStartIndex + 6, bit5, paletteIndex << 2, _pixelIndex--);

            var bit6 = ((lowTileByte & 0x40) >> 6) | ((highTileByte & 0x40) >> 5);
            DrawBackgroundPixelToByteArray(bitmapArray, bitmapArrayStartIndex + 3, bit6, paletteIndex << 2, _pixelIndex--);

            var bit7 = ((lowTileByte & 0x80) >> 7) | (((highTileByte & 0x80) >> 6));
            DrawBackgroundPixelToByteArray(bitmapArray, bitmapArrayStartIndex, bit7, paletteIndex << 2, _pixelIndex);

		    _pixelIndex += 8;
		}
    

        private unsafe void DrawBackgroundPixelToByteArray(byte* bitmapArray, int bitmapArrayStartIndex, int bit, int paletteIndex, int xPixelCoordinate)
        {           
            if (bit > 0 && xPixelCoordinate >= 0 && xPixelCoordinate < 256)
            {
                _backgroundPixelOpaqueMap[xPixelCoordinate] = true;
            }

            bit = 0x3f00 + bit | (paletteIndex);
            bit = ReadInternalMemory(((bit & 0x3) != 0X0) ? bit : 0x3f00);
            DrawPixelToByteArray(bitmapArray, bitmapArrayStartIndex, bit);
        }

        private unsafe void DrawSpriteTileRowToBitmapArray(byte* bitmapArray, byte lowTileByte, byte highTileByte,
			    int bitmapArrayStartIndex, int paletteIndex, bool isFrontSprite, bool isSpriteZero, int xCoordinate, bool skipChecks)
		{
			    //Each pixel has 2 bitmapPointer that control the color, a high bit and a low bit.
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

		   
			    //var spriteZeroHitFlag = (StatusRegister & 40) == 0x40;

			    var bit0 = (lowTileByte & 0x1) | ((highTileByte & 0x01) << 1);
			    DrawSpritePixelToByteArray(bitmapArray, bitmapArrayStartIndex + 21, bit0, paletteIndex << 2, xCoordinate--, isFrontSprite, isSpriteZero, skipChecks);
			
			    var bit1 = ((lowTileByte & 0x2) >> 1) | (highTileByte & 0x02);
			    DrawSpritePixelToByteArray(bitmapArray, bitmapArrayStartIndex + 18, bit1, paletteIndex << 2, xCoordinate--, isFrontSprite, isSpriteZero, skipChecks);

                var bit2 = ((lowTileByte & 0x04) >> 2) | ((highTileByte & 0x04) >> 1);
			    DrawSpritePixelToByteArray(bitmapArray, bitmapArrayStartIndex + 15, bit2, paletteIndex << 2, xCoordinate--, isFrontSprite, isSpriteZero, skipChecks);

                var bit3 = ((lowTileByte & 0x08) >> 3) | ((highTileByte & 0x08) >> 2);
			    DrawSpritePixelToByteArray(bitmapArray, bitmapArrayStartIndex + 12, bit3, paletteIndex << 2, xCoordinate--, isFrontSprite, isSpriteZero, skipChecks);

                var bit4 = ((lowTileByte & 0x10) >> 4) | ((highTileByte & 0x10) >> 3);
			    DrawSpritePixelToByteArray(bitmapArray, bitmapArrayStartIndex + 9, bit4, paletteIndex << 2, xCoordinate--, isFrontSprite, isSpriteZero, skipChecks);

                var bit5 = ((lowTileByte & 0x20) >> 5) | ((highTileByte & 0x20) >> 4);
			    DrawSpritePixelToByteArray(bitmapArray, bitmapArrayStartIndex + 6, bit5, paletteIndex << 2, xCoordinate--, isFrontSprite, isSpriteZero, skipChecks);

                var bit6 = ((lowTileByte & 0x40) >> 6) | ((highTileByte & 0x40) >> 5);
			    DrawSpritePixelToByteArray(bitmapArray, bitmapArrayStartIndex + 3, bit6, paletteIndex << 2, xCoordinate--, isFrontSprite, isSpriteZero, skipChecks);

                var bit7 = ((lowTileByte & 0x80) >> 7) | (((highTileByte & 0x80) >> 6));
			    DrawSpritePixelToByteArray(bitmapArray, bitmapArrayStartIndex, bit7, paletteIndex << 2, xCoordinate, isFrontSprite, isSpriteZero, skipChecks);
        }

		private unsafe void DrawSpritePixelToByteArray(byte* bitmapArray, int bitmapArrayStartIndex, int bit, int paletteIndex, int xCoordinate, bool isFrontSprite, bool isSpriteZero, bool skipChecks)
		{
			if (bit <= 0 || _spritePriorityMap[xCoordinate]) return;

			if (isFrontSprite)
			{
				bit = 0x3f10 + bit | (paletteIndex);
				bit = ReadInternalMemory(((bit & 0x3) != 0X0) ? bit : 0x3f00);
				DrawPixelToByteArray(bitmapArray, bitmapArrayStartIndex, bit);
			}

		    if (skipChecks) return;

			_spritePriorityMap[xCoordinate] = true;

			if (!isSpriteZero) return;

            //Sprite0 does not occur when x = 255, x - 0 to 7 if clipping is enabled, or if the background or sprite pixel is transparent.
            if (_backgroundPixelOpaqueMap[xCoordinate] && (!_isRenderingDisabled) && (MaskRegister & 0x18) == 0x18) //&& xCoordinate != 255 && (xCoordinate > 7 || ((MaskRegister & 0x3) == 0 || ((MaskRegister & 0x1) == 0))))
            {
                PPUStatusFlags.SpriteZeroHit = 1;
            }
        }

		/// <summary>
		/// Writes all of the palettes to its corresponding byteArray to be drawn on the screen. 
		/// </summary>
		/// <param name="paletteBitmapArray">The palette bitmap array that we are drawing the palette to</param>
		/// <param name="background">A boolean that determines if we are drawing the palette for the backgroun images or the sprites</param>
		private unsafe void WritePaletteToByteArray(byte* paletteBitmapArray, bool background)
		{
			var rowOffset = 0;

			var startposition = background ? 0x3F00 : 0x3F10;
			var endposition = background ? 0x3F10 : 0x3F20;

			for (var memoryLocation = startposition; memoryLocation < endposition; memoryLocation++)
			{
				var paletteLookup = _internalMemory[memoryLocation];

				for (var offset = 0; offset < 32; offset++)
				{
					for (var pixelOffset = 0; pixelOffset < 32; pixelOffset++)
					{
						DrawPixelToByteArray(paletteBitmapArray, rowOffset + (pixelOffset * 3) + (offset * 1536), paletteLookup);
					}
				}
				rowOffset += 96;
			}
		}

		/// <summary>
		/// Draws the actual pixels to the byte array using the specific collor
		/// </summary>
		/// <param name="bitmapArray">The array we are drawing the pixels to</param>
		/// <param name="pixelArrayIndex">The position in the array to draw the bits to</param>
		/// <param name="paletteIndex">The index of the color to draw</param>
		private static unsafe void DrawPixelToByteArray(byte* bitmapArray, int pixelArrayIndex, int paletteIndex)
		{
			bitmapArray[pixelArrayIndex] = Pallet[paletteIndex * 3 + 2];
			bitmapArray[pixelArrayIndex + 1] = Pallet[paletteIndex * 3 + 1];
			bitmapArray[pixelArrayIndex + 2] = Pallet[paletteIndex * 3];
		}
		#endregion

		//[Conditional("DEBUG")]
		//private void //WriteLog(string log)
		//{
  //          _logger.Debug("SL: {0} P: {1} IsOdd: {2} Rend: {3} CurrentAddress: {6} {7}", ScanLine, CycleCount, _isOddFrame, _isRenderingDisabled, VRamAddress.ToString("X"), log);
		//}

		//[Conditional("DEBUG")]
		//private void WriteSpriteEvaluationLog(string log)
		//{
		//	_logger.Trace("SL: {0} CYC: {1} Rend:{2} OAM ADDR: {3} {4}", ScanLine, CycleCount, _isRenderingDisabled, _objectAttributeMemoryAddress, log);
		//}
		#endregion
	}
}
