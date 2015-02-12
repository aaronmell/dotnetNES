using System;
using System.IO;
using NUnit.Framework;

namespace dotnetNES.Tests
{
    public class PPU_Tests
    {
        [TestCase("01-basics.nes", "\n01-basics\n\nPassed\n")]
        public void PPU_Palette_LoadedCorrectly(string fileName, string expectedString)
        {
            
            var engine =
              new dotnetNES.Engine.Main.Engine(Path.Combine(Environment.CurrentDirectory, "TestRoms", "instr_test-v5",
                  "01-basics.nes"));

            var steps = 0;
            while (steps < 126352)
            {
                engine.Step();
                steps++;
            }

            Assert.AreEqual(0x0F,engine.PictureProcessingUnit.ReadPPUMemory(0x3f00), "0X3F00 must be 0x0F");
            Assert.AreEqual(0x30, engine.PictureProcessingUnit.ReadPPUMemory(0x3f01), "0X3F01 must be 0x30");
            Assert.AreEqual(0x30, engine.PictureProcessingUnit.ReadPPUMemory(0x3f02), "0X3F02 must be 0x30");
            Assert.AreEqual(0x30, engine.PictureProcessingUnit.ReadPPUMemory(0x3f03), "0X3F03 must be 0x30");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f04), "0X3F04 must be 0x0F");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f05), "0X3F05 must be 0x0F");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f06), "0X3F06 must be 0x0F");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f07), "0X3F07 must be 0x0F");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f08), "0X3F08 must be 0x0F");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f09), "0X3F09 must be 0x0F");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0a), "0X3F0a must be 0x0F");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0b), "0X3F0b must be 0x0F");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0c), "0X3F0c must be 0x0F");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0d), "0X3F0d must be 0x0F");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0e), "0X3F0e must be 0x0F");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0f), "0X3F0f must be 0x0F");
        }
    }
}
