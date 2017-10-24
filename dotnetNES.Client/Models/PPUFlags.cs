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

        public void UpdateFlags(dotnetNES.Engine.Main.Engine engine)
        {
            ScanLine = engine.GetScanLine();
            Cycle = engine.GetPPUCycleCount();
            VRAMAddress = engine.GetVRAMAddress().ToString("X").PadLeft(4,'0');
            NTAddress = engine.GetNTAddress().ToString("X").PadLeft(4, '0');
            XScroll = engine.GetXScroll();
        }
    }
}
