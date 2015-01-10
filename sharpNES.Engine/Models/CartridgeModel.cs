namespace sharpNES.Engine.Models
{
    public class CartridgeModel
    {
        /// <summary>
        /// The ROM Banks
        /// </summary>
        public byte[][] ROMBanks { get; set; }

        /// <summary>
        /// The VROM Banks
        /// </summary>
        public byte[][] VROMBanks { get; set; }

        public byte[] Trainer { get; set; }

        /// <summary>
        /// Horizontal mirroring is used if this is set to false, otherwise vertical mirroring is used
        /// </summary>
        public bool IsVerticalMirroringEnabled { get; set; }

        /// <summary>
        /// Determines if the the battery backed ram is used at $6000-$7FFF
        /// </summary>
        public bool IsBatteryBackedRamEnabled { get; set; }

        /// <summary>
        /// Determines if this cartridge has a trainer
        /// </summary>
        public bool IsTrainerPresent { get; set; }

        /// <summary>
        /// The type of mapper used by this cartridge
        /// </summary>
        public byte MapperType { get; set; }

        /// <summary>
        /// If set to true, this cartridge runs on NTSC instead of PAL
        /// </summary>
        public bool IsNTSC { get; set; }

        /// <summary>
        /// The number of Video Ram Banks
        /// </summary>
        public int VRAMBankCount { get; set; }

        /// <summary>
        /// Determines if the cartridge is using the 4 screen VRAM layout
        /// </summary>
        public bool UsesFourScreenVRAMLayout { get; set; }
    }
}
