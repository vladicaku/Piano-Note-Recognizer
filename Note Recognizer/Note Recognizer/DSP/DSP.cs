using Note_Recognizer.DSP_Engine;
using Note_Recognizer.Notes_Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

namespace Note_Recognizer
{

    public class DSPEngine
    {
        public int WindowSize { get; set; }
        public int StepSize { get; set; }
        public int SampleRate { get; set; }
        public bool Debuging { get; set; }
        public double Tolerance { get; set; }
        public double[] Data { get; set; }
        public List<String> Keys { get; set; }
        public List<Double> Freqs { get; set; }

        public Int16 LoadProgress { get; set; }

        public DSPEngine()
        {
            Debuging = false;
            Tolerance = 10;
        }

        public void Start()
        {
            double[] hannWindow = CreateHanningWindow();
            Keys = new List<string>(Data.Length / WindowSize + 1); // it will increase when windows are overlaping
            Freqs = new List<double>(Data.Length / WindowSize + 1); // it will increase when windows are overlaping

            for (int i = 0; i < Data.Length - WindowSize; i += StepSize)
            {
                Complex[] data = WindowMulData(hannWindow, i);
                Complex[] longfft = FFT(data);
                Complex[] fft = new Complex[longfft.Length / 2 + 1];
                for (int j = 0; j < fft.Length; j++)
                {
                    fft[j] = Math.Abs(longfft[j].Real);
                }

                Complex[] hps1 = HarmonicProductSpectrum(fft);
                int maxBean1 = FindMaxBeanPosition(hps1);
                double baseFreq1 = ConvertToFreq(maxBean1);
                String key = PianoNotes.getKey(baseFreq1, Tolerance);
                
                Freqs.Add(baseFreq1);
                Keys.Add(key);
                LoadProgress = Convert.ToInt16(Scaler.Scaler.ScaleToLong(i, Data.Length, 0, 100, 0));

                if (Debuging)
                {
                    Plot plot = new Plot();
                    plot.label1.Text = "FFT";
                    plot.pictureBox1.Image = GraphDrawer.DrawSpectrumData(fft);
                    plot.Show();
                    plot.Location = new Point(0, 0);

                    Plot plot1 = new Plot();
                    plot1.label1.Text = "HPS " + String.Format("{0:0.000}", baseFreq1) + " - " + key;
                    plot1.pictureBox1.Image = GraphDrawer.DrawSpectrumData(hps1);
                    plot1.ShowDialog();
                    plot.Close();
                }
            }
        }

        public Complex[] WindowMulData(double[] window, int dataStartPos)
        {
            Complex[] array = new Complex[window.Length];
            for (int i = 0; i < window.Length; i++)
            {
                array[i] = window[i] * Data[i + dataStartPos];
            }
            return array;
        }

        public Complex[] HarmonicProductSpectrum(Complex[] data)
        {
            Complex[] hps2 = Downsample(data, 2);
            Complex[] hps3 = Downsample(data, 3);
            Complex[] hps4 = Downsample(data, 4);
            Complex[] hps5 = Downsample(data, 5);
            Complex[] array = new Complex[hps5.Length];
            for (int i = 0; i < array.Length; i++)
            {
                checked
                {
                    array[i] = data[i].Real * hps2[i].Real * hps3[i].Real * hps4[i].Real * hps5[i].Real;
                }
            }
            return array;
        }

        public Complex[] Downsample(Complex[] data, int n)
        {
            Complex[] array = new Complex[Convert.ToInt32(Math.Ceiling(data.Length * 1.0 / n))];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = data[i * n].Real;
            }
            return array;
        }

        public Complex[] DFT(Complex[] data)
        {
            int len = data.Length;
            int n = len / 2 + 1; // Maybe we should use len / 2 + 1 -> only so called "positive" frequencies, they are symetric anyway
            Complex[] realPart = new Complex[n];
            double pi_div = -2.0D * (double)Math.PI / (double)len;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < len; j++)
                {
                    realPart[i] += data[j].Real * Math.Cos(pi_div * (double)i * (double)j);
                }
            }

            for (int i = 0; i < n; i++)
            {
                realPart[i] = Math.Abs(realPart[i].Real);
            }
            return realPart;
        }

        public double[] CreateHanningWindow()
        {
            return CreateHanningWindow(WindowSize);
        }

        public double[] CreateHanningWindow(int size)
        {
            double[] array = new double[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / size));
            }
            return array;
        }

        public int FindMaxBeanPosition(Complex[] data)
        {
            double max = Double.MinValue;
            int pos = -1;
            // skip 0 freq
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i].Real > max)
                {
                    max = data[i].Real;
                    pos = i;
                }
            }
            return pos;
        }

        public double ConvertToFreq(int pos)
        {
            return pos * 1.0 / WindowSize * SampleRate;
        }

        public static Complex[] FFT(Complex[] x)
        {
            int N = x.Length;

            // base case
            if (N == 1) return new Complex[] { x[0] };

            // radix 2 Cooley-Tukey FFT
            if (N % 2 != 0) { throw new Exception("N is not a power of 2"); }

            // fft of even terms
            Complex[] even = new Complex[N / 2];
            for (int k = 0; k < N / 2; k++)
            {
                even[k] = x[2 * k];
            }
            Complex[] q = FFT(even);

            // fft of odd terms
            Complex[] odd = even;  // reuse the array
            for (int k = 0; k < N / 2; k++)
            {
                odd[k] = x[2 * k + 1];
            }
            Complex[] r = FFT(odd);

            // combine
            Complex[] y = new Complex[N];
            for (int k = 0; k < N / 2; k++)
            {
                double kth = -2.0D * (double)k * (double)Math.PI / (double)N;
                Complex wk = new Complex(Math.Cos(kth), Math.Sin(kth));
                y[k] = q[k] + (wk * (r[k]));
                y[k + N / 2] = q[k] - (wk * (r[k]));
            }
            return y;
        }

        public static void transformRadix2(double[] real, double[] imag) 
        {
            // Initialization
            if (real.Length != imag.Length)
	            throw new Exception("Mismatched lengths");
            int n = real.Length;
            
            int levels = 31 - (int)Math.Log(n, 2);  // Equal to floor(log2(n))
            if (1 << levels != n)
	            throw new Exception("Length is not a power of 2");
            double[] cosTable = new double[n / 2];
            double[] sinTable = new double[n / 2];
            for (int i = 0; i < n / 2; i++) {
	            cosTable[i] = Math.Cos(2.0 * Math.PI * i / n);
	            sinTable[i] = Math.Sin(2.0 * Math.PI * i / n);
            }
		
            // Bit-reversed addressing permutation
            for (uint i = 0; i < n; i++) {
	            //uint j = Integer.reverse(i) >> (32 - levels);
                uint j = reverseBits(i, 32 - levels);
	            if (j > i) {
		            double temp = real[i];
		            real[i] = real[j];
		            real[j] = temp;
		            temp = imag[i];
		            imag[i] = imag[j];
		            imag[j] = temp;
	            }
            }
		
            // Cooley-Tukey decimation-in-time radix-2 FFT
            for (int size = 2; size <= n; size *= 2) {
	            int halfsize = size / 2;
	            int tablestep = n / size;
	            for (int i = 0; i < n; i += size) {
		            for (int j = i, k = 0; j < i + halfsize; j++, k += tablestep) {
			            double tpre =  real[j+halfsize] * cosTable[k] + imag[j+halfsize] * sinTable[k];
			            double tpim = -real[j+halfsize] * sinTable[k] + imag[j+halfsize] * cosTable[k];
			            real[j + halfsize] = real[j] - tpre;
			            imag[j + halfsize] = imag[j] - tpim;
			            real[j] += tpre;
			            imag[j] += tpim;
		            }
	            }
	            if (size == n)  // Prevent overflow in 'size *= 2'
		            break;
            }
        }
        private static uint reverseBits(uint x, int n)
        {
            uint result = 0;
            uint i;
            for (i = 0; i < n; i++, x >>= 1)
                result = (result << 1) | (x & 1);
            return result;
        }

    }
}
