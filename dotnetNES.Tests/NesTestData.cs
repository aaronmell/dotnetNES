namespace dotnetNES.Tests
{
    /// <summary>
    /// POCO that holds the data for NesTest
    /// </summary>
    internal class NesTestData
    {
        internal int Accumulator { get; set; }

        internal int XRegister { get; set; }

        internal int YRegister { get; set; }

        internal int StackPointer { get; set; }

        internal int Flags { get; set; }

        internal int CycleCount { get; set; }

        internal int ScanLine { get; set; }

        internal int ProgramCounter { get; set; }
    }
}
