using dotnetNES.Engine.Utilities;
using NUnit.Framework;

namespace dotnetNES.Tests.Utilities
{
    public class CPUUtilityTests
    {
        [Test]
        public void Loads_Memory_Correctly()
        {
            var array = LoaderTests.GetStandardByteArray(32784);
            array[4] = 2;


            for (var i = 16; i < 16400; i++)
            {
                array[i] = 1;
            }

            for (var i = 16400; i < 32784; i++)
            {
                array[i] = 2;
            }

            var cartridge = CartridgeLoaderUtility.LoadCartridge(array);
            var cpu = cartridge.GetProcessor();

            Assert.AreEqual(1, cpu.ReadMemoryValue(0x8000));
            Assert.AreEqual(1, cpu.ReadMemoryValue(0xBFFF));

            Assert.AreEqual(2, cpu.ReadMemoryValue(0xC000));
            Assert.AreEqual(2, cpu.ReadMemoryValue(0xFFFF));
        }
    }
}
