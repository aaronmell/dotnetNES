using System;
using System.IO;
using dotnetNES.Engine.Models;

namespace dotnetNES.Engine.Utilities
{
    /// <summary>
    /// This class is responsible for loading the cartridge. This loader only supports loading unzipped iNES formatted cartridges. 
    /// </summary>
    public static class Loader
    {
        /// <summary>
        /// This method takes a fileName as input and loads it into a cartridge. It only accepts valid iNes formats.
        /// </summary>
        /// <param name="fileName">The file to load</param>
        /// <returns>The cartridge model</returns>
        public static CartridgeModel LoadCartridge(string fileName)
        {
            var rawData = LoadRawData(fileName);
            return LoadCartridge(rawData);
        }

        /// <summary>
        /// This method takes a byte array of raw data and loads it into a cartridge. Only used for unit testing.
        /// </summary>
        /// <param name="rawData">The raw data to load</param>
        /// <returns>The cartridge model</returns>
        public static CartridgeModel LoadCartridge(byte[] rawData)
        {
            //First 3 bits are the always the ascii bits
            if (System.Text.Encoding.ASCII.GetString(rawData, 0, 3).ToUpper() != "NES")
                throw new InvalidOperationException("File loaded is not a valid .nes Cartridge");

            //Bit 4 is the number of ROM Banks
            var romBanksCount = rawData[4];
            //Bit 5 is the number of VROM Banks
            var vromBanksCount = rawData[5];
            //Bit 8 is the number of VRAM Banks. For compatibility, assume this always has a minimum of 1 bank.
            var ramBanksCount = rawData[8] > 0 ? rawData[8] : 1;

            //bit 0     1 for vertical mirroring, 0 for horizontal mirroring.
            //bit 1     1 for battery-backed RAM at $6000-$7FFF.
            //bit 2     1 for a 512-byte trainer at $7000-$71FF.
            //bit 3     1 for a four-screen VRAM layout. 
            //bit 4-7   Four lower bits of ROM Mapper Type.
            var flags6 = rawData[6];

            //bit 0     1 for VS-System cartridges.
            //bit 1-3   Reserved, must be zeroes!
            //bit 4-7   Four higher bits of ROM Mapper Type.
            var flags7 = rawData[7];
            
            //bit 0     1 for PAL cartridges, otherwise assume NTSC.
            //bit 1-7   Reserved, must be zeroes!
            var flags9 = rawData[9];

            var isTrainerPresent = (flags6 & 4) == 4;

            var trainer = new byte[512];
            if (isTrainerPresent)
                Array.Copy(rawData, 16, trainer, 0, 512);

            var romBanks = new byte[romBanksCount][];
            var offset = isTrainerPresent ? 529 : 16;
            
            for (var i = 0; i < romBanksCount; i++)
            {
                romBanks[i] = new byte[16384];
                Array.Copy(rawData, offset, romBanks[i], 0, 16384);
                offset += 16385;
            }

            var vromBanks = new byte[vromBanksCount][];
            for (var i = 0; i < vromBanksCount; i++)
            {
                vromBanks[i] = new byte[8192];
                Array.Copy(rawData, offset, vromBanks[i], 0, 8192);
                offset += 8193;
            }

            return new CartridgeModel
            {
                VROMBanks = vromBanks,
                IsBatteryBackedRamEnabled = (flags6 & 2) == 2,
                IsNTSC = (flags9 & 1) == 0,
                IsTrainerPresent = isTrainerPresent,
                IsVerticalMirroringEnabled = (flags6 & 0x01) == 1,
                MapperType = (byte) ((flags6 >> 4) | (flags7 & 240)),
                ROMBanks = romBanks,
                Trainer = trainer,
                VRAMBankCount = ramBanksCount
            };
        }

        private static byte[] LoadRawData(string fileName)
        {
            var stream = File.Open(fileName, FileMode.Open);

            var buffer = new byte[stream.Length];
            var read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    var nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    var newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            var ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
    }
}
