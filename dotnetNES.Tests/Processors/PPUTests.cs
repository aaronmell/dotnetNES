using System;
using System.IO;
using NUnit.Framework;

namespace dotnetNES.Tests.Processors
{
    public class PPUTests
    {
         [Test]
        public void PPU_Palette_LoadedCorrectly_Instr_Test_V5()
        {
            
            var engine =
              new Engine.Main.Engine(Path.Combine(Environment.CurrentDirectory, "TestRoms", "instr_test-v5",
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


        [Test]
        public void PPU_Palette_LoadedCorrectly_Nes_Test_Rom()
        {

            var engine =
              new Engine.Main.Engine(Path.Combine(Environment.CurrentDirectory, "TestRoms", "nestest",
                  "nestest.nes"));

            var steps = 0;
            while (steps < 124262)
            {
                engine.Step();
                steps++;
            }

            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f00), "0X3F00");
            Assert.AreEqual(0x06, engine.PictureProcessingUnit.ReadPPUMemory(0x3f01), "0X3F01");
            Assert.AreEqual(0x12, engine.PictureProcessingUnit.ReadPPUMemory(0x3f02), "0X3F02");
            Assert.AreEqual(0x33, engine.PictureProcessingUnit.ReadPPUMemory(0x3f03), "0X3F03");
            Assert.AreEqual(0x33, engine.PictureProcessingUnit.ReadPPUMemory(0x3f04), "0X3F04");
            Assert.AreEqual(0x06, engine.PictureProcessingUnit.ReadPPUMemory(0x3f05), "0X3F05");
            Assert.AreEqual(0x12, engine.PictureProcessingUnit.ReadPPUMemory(0x3f06), "0X3F06");
            Assert.AreEqual(0x33, engine.PictureProcessingUnit.ReadPPUMemory(0x3f07), "0X3F07");
            Assert.AreEqual(0x38, engine.PictureProcessingUnit.ReadPPUMemory(0x3f08), "0X3F08");
            Assert.AreEqual(0x06, engine.PictureProcessingUnit.ReadPPUMemory(0x3f09), "0X3F09");
            Assert.AreEqual(0x12, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0a), "0X3F0a");
            Assert.AreEqual(0x33, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0b), "0X3F0b");
            Assert.AreEqual(0x3A, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0c), "0X3F0c");
            Assert.AreEqual(0x06, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0d), "0X3F0d");
            Assert.AreEqual(0x12, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0e), "0X3F0e");
            Assert.AreEqual(0x33, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0f), "0X3F0f");
        }

        /// <summary>
        /// You will need the valid rom in order to run this test!
        /// </summary>
        [Test]
        public void PPU_Palette_LoadedCorrectly_DK_Rom()
        {

            var engine =
              new Engine.Main.Engine(Path.Combine("F:", "roms", "Donkey Kong (JU).nes"));

            var steps = 0;
            while (steps < 40000)
            {
                engine.Step();
                steps++;
            }

            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f00), "0X3F00");
            Assert.AreEqual(0x2C, engine.PictureProcessingUnit.ReadPPUMemory(0x3f01), "0X3F01");
            Assert.AreEqual(0x38, engine.PictureProcessingUnit.ReadPPUMemory(0x3f02), "0X3F02");
            Assert.AreEqual(0x12, engine.PictureProcessingUnit.ReadPPUMemory(0x3f03), "0X3F03");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f04), "0X3F04");
            Assert.AreEqual(0x27, engine.PictureProcessingUnit.ReadPPUMemory(0x3f05), "0X3F05");
            Assert.AreEqual(0x27, engine.PictureProcessingUnit.ReadPPUMemory(0x3f06), "0X3F06");
            Assert.AreEqual(0x27, engine.PictureProcessingUnit.ReadPPUMemory(0x3f07), "0X3F07");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f08), "0X3F08");
            Assert.AreEqual(0x30, engine.PictureProcessingUnit.ReadPPUMemory(0x3f09), "0X3F09");
            Assert.AreEqual(0x30, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0a), "0X3F0a");
            Assert.AreEqual(0x30, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0b), "0X3F0b");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0c), "0X3F0c");
            Assert.AreEqual(0x0, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0d), "0X3F0d");
            Assert.AreEqual(0x0, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0e), "0X3F0e");
            Assert.AreEqual(0x0, engine.PictureProcessingUnit.ReadPPUMemory(0x3f0f), "0X3F0f");

            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F10), "0X3F10");
            Assert.AreEqual(0x25, engine.PictureProcessingUnit.ReadPPUMemory(0x3F11), "0X3F11");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F12), "0X3F12");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F13), "0X3F13");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F14), "0X3F14");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F15), "0X3F15");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F16), "0X3F16");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F17), "0X3F17");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F18), "0X3F18");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F19), "0X3F19");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F1a), "0X3F1a");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F1b), "0X3F1b");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F1c), "0X3F1c");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F1d), "0X3F1d");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F1e), "0X3F1e");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F1f), "0X3F1f");

        }
    
    
        /// <summary>
        /// You will need the valid rom in order to run this test!
        /// </summary>
        [Test]
        public void PPU_NameTables_LoadedCorrectly_DK_Rom()
        {
            var engine =
               new Engine.Main.Engine(Path.Combine("F:", "roms", "Donkey Kong (JU).nes"));

            var steps = 0;
            while (steps < 154262)
            {
                engine.Step();
                steps++;
            }


            for (var i = 0x2000; i < 0x2083; i++)
            {
                Assert.AreEqual(0x24, engine.PictureProcessingUnit.ReadPPUMemory(i), i.ToString("X"));
            }
        }
    }
}
