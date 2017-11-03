using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetNES.Client.Models
{
    public class PPUFlags
    {
        public int ScanLine { get; set; }

        public int Cycle { get; set; }

        public string VRAMAddress { get; set; }

        public string NTAddress { get; set; }

        public int XScroll { get; set; }

        public bool GrayScale { get; set; }

        public bool DrawLeftBackground { get; set; }

        public bool DrawLeftSprites { get; set; }

        public bool DrawBackGround { get; set; }

        public bool DrawSprites { get; set; }

        public bool IntensifyRed { get; set; }

        public bool IntensifyBlue { get; set; }

        public bool IntensifyGreen { get; set; }

        public int BaseNameTable { get; set; }

        public bool VRAMIncrement { get; set; }

        public bool SpriteTableAddress { get; set; }

        public bool BackgroundTableAddress { get; set; }

        public bool SpriteSize { get; set; }

        public bool PPUMasterSelect { get; set; }

        public bool GenerateNMI { get; set; }

        public bool SpriteOverflow { get; set; }

        public bool SpriteZeroHit { get; set; }

        public bool VblankEnabled { get; set; }

        public void UpdateFlags(dotnetNES.Engine.Main.Engine engine)
        {
            ScanLine = engine.GetScanLine();
            Cycle = engine.GetPPUCycleCount();
            VRAMAddress = engine.GetVRAMAddress().ToString("X").PadLeft(4,'0');
            NTAddress = engine.GetNTAddress().ToString("X").PadLeft(4, '0');
            XScroll = engine.GetXScroll();

            var mask = engine.GetMemoryLocation(0x2001);

            GrayScale = IsBitSet(mask, 0);
            DrawLeftBackground = IsBitSet(mask, 1);
            DrawLeftSprites = IsBitSet(mask, 2);
            DrawBackGround = IsBitSet(mask, 3);
            DrawSprites = IsBitSet(mask, 4);
            IntensifyRed = IsBitSet(mask, 5);
            IntensifyGreen = IsBitSet(mask, 6);
            IntensifyBlue = IsBitSet(mask, 7);

            var controller = engine.GetMemoryLocation(0x2000);

            BaseNameTable = controller & 0x03;
            VRAMIncrement = IsBitSet(controller, 2);
            SpriteTableAddress = IsBitSet(controller, 3);
            BackgroundTableAddress = IsBitSet(controller, 4);
            SpriteSize = IsBitSet(controller, 5);
            PPUMasterSelect = IsBitSet(controller, 6);
            GenerateNMI = IsBitSet(controller, 7);

            var status = engine.GetMemoryLocation(0x2002);

            SpriteOverflow = IsBitSet(status, 5);
            SpriteZeroHit = IsBitSet(status, 6);
            VblankEnabled = IsBitSet(status, 7);
        }

    private bool IsBitSet(byte bit, int position)
        {
            return (bit & (1 << position)) != 0;
        }
    }
}