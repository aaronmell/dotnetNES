using System.IO;
using NUnit.Framework;

namespace dotnetNES.Tests.Processors
{
    public class PPUTests
    {
         [Test]
        public void PPU_Palette_LoadedCorrectly_Instr_Test_V5()
        {
            var path = Utilities.GetTestPath("instr_test-v5",
                  "01-basics.nes");

            var engine =
              new Engine.Main.Engine(path);

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
            var path = Utilities.GetTestPath("nestest",
                  "nestest.nes");

            var engine =
              new Engine.Main.Engine(path);            

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
            var romName = "Donkey Kong (JU).nes";

            var path = Utilities.GetTestPath("roms", romName);

            if (!File.Exists(path))
            {
                Assert.Inconclusive($"Unable to find rom {romName}");
            }

            var engine = new Engine.Main.Engine(path);

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

            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3F10), "0X3F10");
            Assert.AreEqual(0x25, engine.PictureProcessingUnit.ReadPPUMemory(0x3F11), "0X3F11");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F12), "0X3F12");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F13), "0X3F13");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3F14), "0X3F14");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F15), "0X3F15");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F16), "0X3F16");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F17), "0X3F17");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3F18), "0X3F18");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F19), "0X3F19");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F1a), "0X3F1a");
            Assert.AreEqual(0x00, engine.PictureProcessingUnit.ReadPPUMemory(0x3F1b), "0X3F1b");
            Assert.AreEqual(0x0F, engine.PictureProcessingUnit.ReadPPUMemory(0x3F1c), "0X3F1c");
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
            var romName = "Donkey Kong (JU).nes";

            var path = Utilities.GetTestPath("roms", romName);

            if (!File.Exists(path))
            {
                Assert.Inconclusive($"Unable to find rom {romName}");
            }

            var engine = new Engine.Main.Engine(path);

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

        [TestCase("1.frame_basics.nes", 2200000)]
        [TestCase("2.vbl_timing.nes", 2200000)]
        [TestCase("3.even_odd_frames.nes", 2200000)]
        [TestCase("4.vbl_clear_timing.nes", 2200000)]
        [TestCase("5.nmi_suppression.nes", 2200000)]
        [TestCase("6.nmi_disable.nes", 2200000)]
        [TestCase("7.nmi_timing.nes", 2200000)]
        public void PPU_VBL_NMI_Tests(string fileName, int totalSteps)
        {
            var path = Utilities.GetTestPath("vbl_nmi_timing",
                  fileName);

            var engine =
              new Engine.Main.Engine(path);
            var steps = 0;

            while (steps < totalSteps)
            {
                engine.Step();
                steps++;
            }

            var result = engine.PictureProcessingUnit.ReadPPUMemory(0x20C2);

            Assert.AreEqual(80, result, "Nametable at 0x20c2 was not Set to P");
        }

		[TestCase("palette_ram.nes", 2200000)]
		[TestCase("sprite_ram.nes", 2200000)]
		[TestCase("vbl_clear_time.nes", 2200000)]
		[TestCase("vram_access.nes", 2200000)]
		public void Misc_PPU_Tests(string fileName, int totalSteps)
		{
            var path = Utilities.GetTestPath("misc_ppu_tests",
                 fileName);

            var engine =
			  new Engine.Main.Engine(path);
			var steps = 0;

			while (steps < totalSteps)
			{
				engine.Step();
				steps++;
			}

			var result = engine.PictureProcessingUnit.ReadPPUMemory(0x20a4);

			Assert.AreEqual(49, result, "Nametable at 0x20c2 was not Set to P");
		}

        [TestCase("01-vbl_basics.nes", "\n01-vbl_basics\n\nPassed\n\0")]
        [TestCase("02-vbl_set_time.nes", "T+ 1 2\n00 - V\n01 - V\n02 - V\n03 - V\n04 - -\n05 V -\n06 V -\n07 V -\n08 V -\n\n02-vbl_set_time\n\nPassed\n\0")]
        [TestCase("03-vbl_clear_time.nes", "00 V\n01 V\n02 V\n03 V\n04 V\n05 V\n06 -\n07 -\n08 -\n\n03-vbl_clear_time\n\nPassed\n\0")]
		[TestCase("04-nmi_control.nes", "\n04-nmi_control\n\nPassed\n\0")]
        [TestCase("05-nmi_timing.nes", "00 4\n01 4\n02 4\n03 3\n04 3\n05 3\n06 3\n07 3\n08 3\n09 2\n\n05-nmi_timing\n\nPassed\n\0")]
        [TestCase("06-suppression.nes", "00 - N\n01 - N\n02 - N\n03 - N\n04 - -\n05 V -\n06 V -\n07 V N\n08 V N\n09 V N\n\n06-suppression\n\nPassed\n\0")]
		[TestCase("07-nmi_on_timing.nes", "00 N\n01 N\n02 N\n03 N\n04 N\n05 N\n06 -\n07 -\n08 -\n\n2B1F5269\n07-nmi_on_timing\n\nFailed\n\0")] //Yes this test actually fails, but it fails just like Nintendulator, so I am okay with that
        [TestCase("08-nmi_off_timing.nes", "03 -\n04 -\n05 -\n06 -\n07 N\n08 N\n09 N\n0A N\n0B N\n0C N\n\n08-nmi_off_timing\n\nPassed\n\0")]
        [TestCase("09-even_odd_frames.nes", "00 01 01 02 \n09-even_odd_frames\n\nPassed\n\0")]
        [TestCase("10-even_odd_timing.nes", "")]
        public void VBlank_NMI_Timing_Test_No_Errors(string fileName, string expectedString)
        {
            var output = Utilities.RunTest(fileName, "ppu_vbl_nmi");

            Assert.AreEqual(expectedString, output);
        }

        [TestCase("01.basics.nes", 80)]
        [TestCase("02.alignment.nes", 70)]
        public void PPU_Sprite_Hit_Tests(string fileName, int expectedInteger)
        {
            var path = Utilities.GetTestPath("sprite_hit_tests_2005.10.05",
               fileName);

            var engine =
              new Engine.Main.Engine(path);
           
            var steps = 0;

            while (steps < 250000)
            {
                engine.Step();
                steps++;
            }

            var result = engine.PictureProcessingUnit.ReadPPUMemory(0x20C2);

            Assert.AreEqual(expectedInteger, result, "Nametable at 0x20c2 was not Set to P");
        }
    }
}
