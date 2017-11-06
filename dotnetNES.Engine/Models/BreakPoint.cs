
namespace dotnetNES.Engine.Models
{
    public class BreakPoint
    {
        private string _address;

        public bool IsEnabled { get; set; }

        public BreakPointType BreakPointType { get; set; }
                
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                ConvertedAddress = int.Parse(_address, System.Globalization.NumberStyles.HexNumber);
                
            }
        }

        public int ConvertedAddress { get; private set; }
    }
}
