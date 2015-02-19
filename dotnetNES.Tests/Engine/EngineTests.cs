using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace dotnetNES.Tests.Engine
{
    public class EngineTests
    {
        [TestCase("01-basics.nes", "\n01-basics\n\nPassed\n\0")]
        //[TestCase("02-implied.nes", "\n02-implied\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("03-immediate.nes", "\n03-immediate\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("04-zero_page.nes", "\n04-zero_page\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("05-zp_xy.nes", "\n05-zp_xy\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("06-absolute.nes", "\n06-absolute\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("07-abs_xy.nes", "\n07-abs_xy\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("08-ind_x.nes", "\n08-ind_x\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("09-ind_y.nes", "\n09-ind_y\n\nPassed\n")] //Fails due to unsupported op codes
        [TestCase("10-branches.nes", "\n10-branches\n\nPassed\n\0")]
        [TestCase("11-stack.nes", "\n11-stack\n\nPassed\n\0")]
        [TestCase("12-jmp_jsr.nes", "\n12-jmp_jsr\n\nPassed\n\0")]
        [TestCase("13-rts.nes", "\n13-rts\n\nPassed\n\0")]
        [TestCase("14-rti.nes", "\n14-rti\n\nPassed\n\0")]
        [TestCase("15-brk.nes", "\n15-brk\n\nPassed\n\0")]
        [TestCase("16-special.nes", "\n16-special\n\nPassed\n\0")]
        public void CPU_Instruction_Test_No_Errors(string fileName, string expectedString)
        {
            var output = RunTest(fileName, "instr_test-v5");

            Assert.AreEqual(expectedString, output);
        }

        [TestCase("01-abs_x_wrap.nes", "\n01-abs_x_wrap\n\nPassed\n\0")]
        [TestCase("02-branch_wrap.nes", "\n02-branch_wrap\n\nPassed\n\0")]
        //[TestCase("03-dummy_reads.nes", "\n02-branch_wrap\n\nPassed\n\0")]
        public void CPU_Instruction_Misc_No_Errors(string fileName, string expectedString)
        {
            var output = RunTest(fileName, "instr_misc");

            Assert.AreEqual(expectedString, output);
        }

        //[TestCase("1-cli_latency.nes","")] //Needs APU
        //[TestCase("2-nmi_and_brk.nes", "")]
        //[TestCase("3-nmi_and_irq.nes", "")]
        //[TestCase("4-irq_and_dma.nes", "")]
        //[TestCase("5-branch_delays_irq.nes", "")]
        public void CPU_Interrupts_No_Errors(string fileName, string expectedString)
        {
            var output = RunTest(fileName, "cpu_int_v2");

            Assert.AreEqual(expectedString, output);
        }

        //[TestCase("1.frame_basics.nes", "")] //I have no way to pass this test yet, because I am not rendering anything!
        public void VBlank_NMI_Timing_Test_No_Errors(string fileName, string expectedString)
        {
            var output = RunTest(fileName, "vbl_nmi_timing");

            Assert.AreEqual(expectedString, output);
        }

        [Test]
        public void Nestest_Matches()
        {
            var engine =
               new dotnetNES.Engine.Main.Engine(Path.Combine(Environment.CurrentDirectory, "TestRoms", "nestest",
                   "nestest.nes"));
            //Changing the Initial PC to 0xC000
            engine.Processor.WriteMemoryValueWithoutCycle(65532, 0);
            engine.Processor.Reset();

            var testData = LoadNesTestData();



            var steps = 1;
            while (steps < 5002) //Can't run the full test, since past this point unofficial op codes show up.
            {
               engine.Step();

               if (steps == 4275)
               {
                   var x = 1;
                   var y = x;
               }

               Assert.AreEqual(testData[steps].ProgramCounter, engine.Processor.ProgramCounter, string.Format("Step {0} PC: ", steps));
               Assert.AreEqual(testData[steps].Accumulator, engine.Processor.Accumulator, string.Format("Step {0} Accumulator: ", steps));
               Assert.AreEqual(testData[steps].XRegister, engine.Processor.XRegister, string.Format("Step {0} XRegister: ", steps));
               Assert.AreEqual(testData[steps].YRegister, engine.Processor.YRegister, string.Format("Step {0} YRegister: ", steps));

               Assert.AreEqual(testData[steps].StackPointer, engine.Processor.StackPointer, string.Format("Step {0} StackPointer: ", steps));

               Assert.AreEqual(testData[steps].CycleCount, engine.PictureProcessingUnit.CycleCount, string.Format("Step {0} CycleCount: ", steps));
               Assert.AreEqual(testData[steps].ScanLine, engine.PictureProcessingUnit.ScanLine, string.Format("Step {0} ScanLine: ", steps));
                steps++;
            }
        }

        private List<NesTestData> LoadNesTestData()
        {
            var reader = new StreamReader(File.OpenRead(Path.Combine(Environment.CurrentDirectory, "TestRoms", "nestest",
                   "nestest.csv")));

            var data = new List<NesTestData>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                data.Add(new NesTestData
                {
                    ProgramCounter = Int32.Parse(values[0], System.Globalization.NumberStyles.HexNumber),
                    Accumulator = Int32.Parse(values[1], System.Globalization.NumberStyles.HexNumber),
                    XRegister = Int32.Parse(values[2], System.Globalization.NumberStyles.HexNumber),
                    YRegister = Int32.Parse(values[3], System.Globalization.NumberStyles.HexNumber),
                    Flags = Int32.Parse(values[4], System.Globalization.NumberStyles.HexNumber),
                    StackPointer = Int32.Parse(values[5], System.Globalization.NumberStyles.HexNumber),
                    CycleCount = int.Parse(values[6]),
                    ScanLine = int.Parse(values[7]),
                });
            }

            return data;
        }

        private string RunTest(string fileName, string folder)
        {
            var engine =
               new dotnetNES.Engine.Main.Engine(Path.Combine(Environment.CurrentDirectory, "TestRoms", folder,
                   fileName));

            var steps = 0;
            while (steps < 31000 || engine.Processor.ReadMemoryValueWithoutCycle(0x6000) >= 0x80)
            {
                engine.Step();
                steps++;

                if (engine.Processor.ReadMemoryValueWithoutCycle(0x6000) > 0x00 &&
                    engine.Processor.ReadMemoryValueWithoutCycle(0x6000) < 0x80)
                    break;
            }

            var testOutput = new List<byte>();
            var startAddress = 0x6004;

            while (true)
            {
                testOutput.Add(engine.Processor.ReadMemoryValueWithoutCycle(startAddress));

                if (System.Text.Encoding.ASCII.GetString(testOutput.ToArray()).Contains("\n\0"))
                {
                    break;
                }
               
                startAddress++;
            }
            return System.Text.Encoding.ASCII.GetString(testOutput.ToArray());
        }
    
        
    }
}
