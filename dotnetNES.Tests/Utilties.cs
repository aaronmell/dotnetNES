using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetNES.Tests
{
    public static class Utilties
    {
        public static string RunTest(string fileName, string folder)
        {
            var engine =
               new Engine.Main.Engine(Path.Combine(Environment.CurrentDirectory, "TestRoms", folder,
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
