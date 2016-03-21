using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Note_Recognizer.Scaler
{
    public class Scaler
    {
        public static double ScaleToDouble(long oldValue, long oldMax, long oldMin, long newMax, long newMin)
        {
            double oldRange = (oldMax - oldMin) ; 
            double newRange = (newMax - newMin) ;
            return (((oldValue - oldMin) * newRange) / oldRange) + newMin;
        }

        public static long ScaleToLong(long oldValue, long oldMax, long oldMin, long newMax, long newMin)
        {
            checked
            {
                long oldRange = (oldMax - oldMin);
                long newRange = (newMax - newMin);
                return (((oldValue - oldMin) * newRange) / oldRange) + newMin;
            }
        }

        public static double ScaleToDoubleGraphVersion(double oldValue, double oldMax, double oldMin, long newMax, long newMin)
        {
            double oldRange = (oldMax - oldMin);
            double newRange = (newMax - newMin);
            return (((oldValue - oldMin) * newRange) / oldRange) + newMin;
        }
    }
}
