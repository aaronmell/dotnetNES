namespace dotnetNES.Engine.Models
{
    public struct Disassembly
    {
        public string Address { get; set; }

        public int RawAddress { get; set; }

        public string FormattedOpCode { get; set; }
    }

}