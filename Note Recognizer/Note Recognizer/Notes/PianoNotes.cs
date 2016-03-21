using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Note_Recognizer.Notes_Engine
{
    public class PianoNotes
    {
        public static String[] keys = { "C", "Db" ,"D", "Eb", "E", "F", "Gb", "G", "Ab", "A", "Bb", "B" };
        // Starts with C3, ends with C8
        public static double[] freqs = {   65.4064, 69.2957, 73.4162, 77.7817, 82.4069, 87.3071, 92.4986, 97.9989, 103.826, 110.000, 116.541, 123.471, // C2
                                           130.813, 138.591, 146.832, 155.563, 164.814, 174.614, 184.997, 195.998, 207.652, 220.000, 233.082, 246.942, // C3
                                           261.626, 277.183, 293.665, 311.127, 329.628, 349.228, 369.994, 391.995, 415.305, 440.000, 466.164, 493.883, // C4
                                           523.251, 554.365, 587.330, 622.254, 659.255, 698.456, 739.989, 783.991, 830.609, 880.000, 932.328, 987.767, // C5
                                           1046.50, 1108.73, 1174.66, 1244.51, 1318.51, 1396.91, 1479.98, 1567.98, 1661.22, 1760.00, 1864.66, 1975.53, // C6
                                           2093.00, 2217.46, 2349.32, 2489.02, 2637.02, 2793.83, 2959.96, 3135.96, 3322.44, 3520.00, 3729.31, 3951.07, // C7
                                           4186.01	// C8
                                       };


        public static String getKey(double frequency, double tolerance)
        {
            String note = "";
            for (int i = 0; i < freqs.Length; i++)
            {
                if (Math.Abs(freqs[i] - frequency) <= tolerance)
                {
                    int octave = i / 12 + 2;
                    note = String.Format("{0}{1}", keys[i % 12], octave, frequency);
                }
            }

            return note;
        }
        
    }
}
