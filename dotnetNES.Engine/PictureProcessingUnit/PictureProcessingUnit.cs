namespace dotnetNES.Engine.PictureProcessingUnit
{
    /// <summary>
    /// The Picture Processing Unit or PPU
    /// </summary>
    public class PictureProcessingUnit
    {
        private readonly Processor.Processor _cpu = new Processor.Processor();

        public PictureProcessingUnit(Processor.Processor cpu)
        {
            _cpu = cpu;
        }

        internal void Step()
        {
           _cpu.NextStep();
        }
    }
}
