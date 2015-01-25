using System;
using System.IO;
using NUnit.Framework;

namespace dotnetNES.Tests.Engine
{
    public class EngineTests
    {
        [TestCase("01-basics.nes", "\n01-basics\n\nPassed\n")]
        //[TestCase("02-implied.nes", "\n02-implied\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("03-immediate.nes", "\n03-immediate\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("04-zero_page.nes", "\n04-zero_page\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("05-zp_xy.nes", "\n05-zp_xy\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("06-absolute.nes", "\n06-absolute\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("07-abs_xy.nes", "\n07-abs_xy\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("08-ind_x.nes", "\n08-ind_x\n\nPassed\n")] //Fails due to unsupported op codes
        //[TestCase("09-ind_y.nes", "\n09-ind_y\n\nPassed\n")] //Fails due to unsupported op codes
        [TestCase("10-branches.nes", "\n10-branches\n\nPasse")]
        [TestCase("11-stack.nes", "\n11-stack\n\nPassed\n\0")]
        [TestCase("12-jmp_jsr.nes", "\n12-jmp_jsr\n\nPassed")]
        [TestCase("13-rts.nes", "\n13-rts\n\nPassed\n\0\0\0")]
        [TestCase("14-rti.nes", "\n14-rti\n\nPassed\n\0\0\0")]
        [TestCase("15-brk.nes", "\n15-brk\n\nPassed\n\0\0\0")]
        [TestCase("16-special.nes", "\n16-special\n\nPassed")]
        public void Instruction_Test_No_Errors(string fileName, string expectedString)
        {
            var engine = new dotnetNES.Engine.Main.Engine(Path.Combine(Environment.CurrentDirectory, "TestRoms", "instr_test-v5",
                    fileName));

            var steps = 0;
            while (steps < 31000 || engine.Processor.ReadMemoryValueWithoutCycle(0x6000) >= 0x80)
            {
                engine.Step();
                steps++;

                if (engine.Processor.ReadMemoryValueWithoutCycle(0x6000) > 0x00 && engine.Processor.ReadMemoryValueWithoutCycle(0x6000) < 0x80)
                    break;
            }

            var testOutput = new byte[19];
            var position = 0;
            for (var i = 0x6004; i < 0x6017; i++)
            {
                testOutput[position] = engine.Processor.ReadMemoryValueWithoutCycle(i);
                position++;
            }

            var output = System.Text.Encoding.ASCII.GetString(testOutput);

            Assert.AreEqual(expectedString, output);
        }
    }
}
