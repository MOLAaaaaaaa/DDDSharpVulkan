using System.Globalization;
using System.Linq;

namespace CSharpIDW
{
    public struct DPoint
    {
        public double Value { get; }

        public double[] Coordinates { get; }

        public DPoint(double value, params double[] coordinates)
        {
            Value = value;
            Coordinates = coordinates;
        }

        public override string ToString()
        {
            return $"{string.Join(";", Coordinates)} -> {Value}";
        }
    }
}