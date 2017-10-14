namespace dotnetNES.Engine.Models
{
    internal class OpCode
    {
        public int Instruction { get; set; }

        public int Length { get; set; }

        public string Format { get; set; }
    }
}