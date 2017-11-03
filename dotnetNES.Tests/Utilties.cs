using System;
using System.Collections.Generic;
using System.IO;

namespace dotnetNES.Tests
{
    public static class Utilities
    {
        public static string GetTestPath(string folder, string fileName)
        {
            const string EnvironmentVariable = "TestDataDirectory";
            string testDataDir = Environment.GetEnvironmentVariable(EnvironmentVariable);

            if (string.IsNullOrWhiteSpace(testDataDir))
            {
                testDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestRoms");
            }

            return Path.Combine(testDataDir, folder,
                   fileName);
        }

        public static string RunTest(string fileName, string folder)
        {           

            var engine =
               new Engine.Main.Engine(GetTestPath(folder, fileName));

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
