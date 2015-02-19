using dotnetNES.Engine.Utilities;
using NUnit.Framework;

namespace dotnetNES.Tests.Utilities
{
    [TestFixture]
    public class LoaderTests
    {
        [TestCase(2, true)]
        [TestCase(253, false)]
        public void Sets_IsBatteryBackedRamEnabled_Correctly(byte byte6, bool result)
        {
            var array = GetStandardByteArray();
            array[6] = byte6;

            var cartridge = CartridgeLoaderUtility.LoadCartridge(array);
            Assert.AreEqual(result,cartridge.IsBatteryBackedRamEnabled);
        }

        [TestCase(1, false)]
        [TestCase(254, true)]
        public void Sets_IsNTSC_Correctly(byte byte6, bool result)
        {
            var array = GetStandardByteArray();
            array[9] = byte6;

            var cartridge = CartridgeLoaderUtility.LoadCartridge(array);
            Assert.AreEqual(result, cartridge.IsNTSC);
        }

        [TestCase(4, true)]
        [TestCase(251, false)]
        public void Sets_IsTrainerPresent_Correctly(byte byte6, bool result)
        {
            var array = GetStandardByteArray();
            array[6] = byte6;

            var cartridge = CartridgeLoaderUtility.LoadCartridge(array);
            Assert.AreEqual(result, cartridge.IsTrainerPresent);
        }

        [TestCase(1, true)]
        [TestCase(254, false)]
        public void Sets_IsVerticalMirroringEnabled_Correctly(byte byte6, bool result)
        {
            var array = GetStandardByteArray();
            array[6] = byte6;

            var cartridge = CartridgeLoaderUtility.LoadCartridge(array);
            Assert.AreEqual(result, cartridge.IsVerticalMirroringEnabled);
        }

        [TestCase(15, 15, 0)]
        [TestCase(240, 240, 255)]
        [TestCase(128, 128, 136)]
        [TestCase(64, 64, 68)]
        [TestCase(32, 32, 34)]
        [TestCase(16, 16, 17)]
        public void Sets_MapperType_Correctly(byte byte6, byte byte7, byte expectedResult)
        {
            var array = GetStandardByteArray();
            array[6] = byte6;
            array[7] = byte7;

            var cartridge = CartridgeLoaderUtility.LoadCartridge(array);
            Assert.AreEqual(expectedResult, cartridge.MapperType);
        }

        [Test]
        public void Copies_Trainer_When_IsTrainerPresent()
        {
            var array = GetStandardByteArray();
            array[6] = 4;


            for (var i = 16; i < 529; i++)
            {
                array[i] = 1;
            }

            array[15] = 255;
            array[529] = 255;

            var cartridge = CartridgeLoaderUtility.LoadCartridge(array);

            foreach (var i in cartridge.Trainer)
            {
                Assert.AreEqual(1,i);
            }
        }

        [Test]
        public void Mirrors_Rom_When_Single_Bank()
        {
            var array = GetStandardByteArray(32784);
            array[4] = 1;

            for (var i = 16; i < 16400; i++)
            {
                array[i] = 1;
            }

            array[15] = 255;

            var cartridge = CartridgeLoaderUtility.LoadCartridge(array);

            foreach (var i in cartridge.ROMBanks[0])
            {
                Assert.AreEqual(1, i);
            }

            Assert.AreEqual(16384, cartridge.ROMBanks[0].Length);

            foreach (var i in cartridge.ROMBanks[1])
            {
                Assert.AreEqual(1, i);
            }

            Assert.AreEqual(16384, cartridge.ROMBanks[1].Length);

        }

        [Test]
        public void Copies_ROM_With_Multiple_Banks()
        {
            var array = GetStandardByteArray(32784);
            array[4] = 2;


            for (var i = 16; i < 16400; i++)
            {
                array[i] = 1;
            }

            for (var i = 16400; i < 32784; i++)
            {
                array[i] = 2;
            }

            array[15] = 255;

            var cartridge = CartridgeLoaderUtility.LoadCartridge(array);

            foreach (var i in cartridge.ROMBanks[0])
            {
                Assert.AreEqual(1, i);
            }

            Assert.AreEqual(16384, cartridge.ROMBanks[0].Length);

            foreach (var i in cartridge.ROMBanks[1])
            {
                Assert.AreEqual(2, i);
            }

            Assert.AreEqual(16384, cartridge.ROMBanks[1].Length);
        }

        [Test]
        public void Copies_VROM_With_Multiple_Banks()
        {
            var array = GetStandardByteArray(49170);
            array[4] = 2;
            array[5] = 2;
            
            for (var i = 16; i < 16400; i++)
            {
                array[i] = 1;
            }

            for (var i = 16400; i < 32784; i++)
            {
                array[i] = 2;
            }

            for (var i = 32784; i < 40976; i++)
            {
                array[i] = 3;
            }

            for (var i = 40976; i < 49169; i++)
            {
                array[i] = 4;
            }

            array[15] = 255;

            var cartridge = CartridgeLoaderUtility.LoadCartridge(array);

            foreach (var i in cartridge.VROMBanks[0])
            {
                Assert.AreEqual(3, i);
            }

            Assert.AreEqual(8192, cartridge.VROMBanks[0].Length);

            foreach (var i in cartridge.VROMBanks[1])
            {
                Assert.AreEqual(4, i);
            }

            Assert.AreEqual(8192, cartridge.VROMBanks[1].Length);
        }

        [TestCase(0,1)]
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        public void Sets_VRAM_Bank_Size_Correctly(byte bankSize, byte expectedValue)
        {
            var array = GetStandardByteArray(65558);
            array[8] = bankSize;

            var cartridge = CartridgeLoaderUtility.LoadCartridge(array);

            Assert.AreEqual(expectedValue, cartridge.VRAMBankCount);
        }

        internal static byte[] GetStandardByteArray(int size = 24016)
        {
            var array = new byte[size];

            array[0] = 78;
            array[1] = 69;
            array[2] = 83;
            array[3] = 26;

            return array;
        }
    }
}

