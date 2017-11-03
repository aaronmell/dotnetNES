using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetNES.Engine.Models
{
    public class PPUStatusFlags
    {
        /// <summary>
        /// Bit 5 of 0x2002 Sprite overflow. The intent was for this flag to be set
        /// whenever more than eight sprites appear on a scanline, but a
        /// hardware bug causes the actual behavior to be more complicated
        /// and generate false positives as well as false negatives; see
        /// PPU sprite evaluation.This flag is set during sprite
        /// evaluation and cleared at dot 1 (the second dot) of the
        /// pre-render line.
        /// </summary>
        public int SpriteOverflow { get; set; }

        /// <summary>
        ///  Bit 6 of 0x2002 Sprite 0 Hit.  Set when a nonzero pixel of sprite 0 overlaps
        /// a nonzero background pixel; cleared at dot 1 of the pre-render
        /// line.Used for raster timing.
        /// </summary>
        public int SpriteZeroHit { get; set; }

        /// <summary>
        /// Bit 7 of 0x2002 Vertical blank has started (0: not in vblank; 1: in vblank).
        /// Set at dot 1 of line 241 (the line * after* the post-render
        /// line); cleared after reading $2002 and at dot 1 of the
        /// pre-render line.
        /// </summary>
        public int VerticalBlank { get; set; }

        /// <summary>
        /// Bit 7 of 0x2000 Generate an NMI at the start of the vertical blanking interval(0: off; 1: on)
        /// </summary>
        public int GenerateNMI { get; set; }
    }
}
