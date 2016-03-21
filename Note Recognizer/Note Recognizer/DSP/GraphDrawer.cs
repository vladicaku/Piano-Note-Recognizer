using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Note_Recognizer.DSP_Engine
{
    public class GraphDrawer
    {
        public static Image DramPcm(double[] data, string[] keys, double[] freqs, int windowSize, int stepSize, int onScreenKeySize)
        {
            if (stepSize == windowSize)
            {
                return DramPcmEqual(data, keys, freqs, windowSize, stepSize, onScreenKeySize);
            }
            else if (stepSize > windowSize)
            {
                return DramPcmLarger(data, keys, freqs, windowSize, stepSize, onScreenKeySize);
            }
            else
            {
                return DramPcmSmaller(data, keys, freqs, windowSize, stepSize, onScreenKeySize);
            }
        }

        private static Image DramPcmEqual(double[] data, string[] keys, double[] freqs, int windowSize, int stepSize, int onScreenKeySize)
        {
            int totalWidth = keys.Length * onScreenKeySize;
            if (keys.Length * windowSize < data.Length)
            {
                int extra = Convert.ToInt32(Scaler.Scaler.ScaleToLong(data.Length - keys.Length * windowSize, windowSize, 0, onScreenKeySize, 0));
                totalWidth += extra;
            }

            Image img = new Bitmap(totalWidth, 365);
            Graphics graph = Graphics.FromImage(img);
            Pen pen = new Pen(Color.DeepSkyBlue, 1);
            Pen pen1 = new Pen(Color.DimGray, 1);
            Font font = new Font("Arial", 15, FontStyle.Regular);
            Font font1 = new Font("Arial", 10, FontStyle.Regular);
            Brush whiteBrush = new SolidBrush(Color.White);
            Brush brush1 = new SolidBrush(Color.LawnGreen);

            graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            graph.FillRectangle(new SolidBrush(Color.Black), 0, 0, img.Width, img.Height);

            int incremental = windowSize / onScreenKeySize;

            for (int i = 0; i < keys.Length; i++)
            {
                int n = 0;
                int xPosition = i * onScreenKeySize;

                // draw windows boundaries
                graph.DrawLine(pen1, xPosition, 0, xPosition, img.Height);

                // Draw pulses
                for (int j = i * windowSize; j < i * windowSize + windowSize; j += incremental)
                {
                    if (j > data.Length)
                    {
                        break;
                    }
                  
                    int currValue = (int)(data[j] * 150);
                    if (j == 0)
                    {
                        graph.DrawLine(pen, xPosition + n, 175, xPosition + n, 175 - currValue);
                    }
                    else
                    {
                        int prevVal = (int)(data[j - incremental] * 150);
                        graph.DrawLine(pen, xPosition + n - 1, 175 - prevVal, xPosition + n, 175 - currValue);
                    }
                    n++;
                }

                // Draw keys
                if (i == 0)
                {
                    graph.DrawString(keys[i], font, whiteBrush, xPosition + 2, 2);
                    graph.DrawString(keys[i], font, whiteBrush, xPosition + 2, img.Height - 30);
                }
                else
                {
                    if (keys[i] != keys[i - 1] && !String.IsNullOrEmpty(keys[i]))
                    {
                        graph.DrawString(keys[i], font, whiteBrush, xPosition + 2, 2);
                        graph.DrawString(keys[i], font, whiteBrush, xPosition + 2, img.Height - 30);
                    }
                }

                // Draw freqs
                graph.DrawString(freqs[i].ToString("0.00"), font1, brush1, xPosition + 2, 25);

            }
            return img;
        }

        private static Image DramPcmLarger(double[] data, string[] keys, double[] freqs, int windowSize, int stepSize, int onScreenKeySize)
        {
            int scaledQuasiWindow = Convert.ToInt32(Scaler.Scaler.ScaleToDouble(windowSize, stepSize, 0, onScreenKeySize, 0));
            int totalWidth = (keys.Length - 1) * onScreenKeySize + scaledQuasiWindow;
            if ((keys.Length - 1) * stepSize + windowSize < data.Length)
            {
                int extra = Convert.ToInt32(Scaler.Scaler.ScaleToLong(data.Length - ((keys.Length - 1) * stepSize + windowSize), stepSize, 0, onScreenKeySize, 0));
                totalWidth += extra;
            }

            Image img = new Bitmap(totalWidth, 365);
            Graphics graph = Graphics.FromImage(img);
            Pen pen = new Pen(Color.DeepSkyBlue, 1);
            Pen pen1 = new Pen(Color.DimGray, 1);
            Pen pen2 = new Pen(Color.DarkGoldenrod, 1);
            Font font = new Font("Arial", 15, FontStyle.Regular);
            Font font1 = new Font("Arial", 10, FontStyle.Regular);
            Brush brush = new SolidBrush(Color.White);
            Brush brush1 = new SolidBrush(Color.LawnGreen);
            //Brush brush2 = new SolidBrush(Color.FromArgb(120, 50, 80, 130));
            Brush brush2 = new SolidBrush(Color.FromArgb(70, 230, 230, 230));
            graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            graph.FillRectangle(new SolidBrush(Color.Black), 0, 0, img.Width, img.Height);

            int incremental = (stepSize) / onScreenKeySize;

            for (int i = 0; i < keys.Length; i++)
            {
                int n = 0;
                int xPosition = i * onScreenKeySize;

                // draw windows boundaries
                graph.FillRectangle(brush2, xPosition, 0, scaledQuasiWindow, img.Width);
                graph.DrawLine(pen1, xPosition, 0, xPosition, img.Height);
                graph.DrawLine(pen1, xPosition + scaledQuasiWindow, 0, xPosition + scaledQuasiWindow, img.Height);

                // Draw pulses
                for (int j = i * stepSize; j < i * stepSize + stepSize; j += incremental)
                {
                    if (j > data.Length)
                    {
                        break;
                    }

                    int currValue = (int)(data[j] * 150);
                    if (j == 0)
                    {
                        graph.DrawLine(pen, xPosition + n, 175, xPosition + n, 175 - currValue);
                    }
                    else
                    {
                        int prevVal = (int)(data[j - incremental] * 150);
                        graph.DrawLine(pen, xPosition + n - 1, 175 - prevVal, xPosition + n, 175 - currValue);
                    }
                    n++;
                }

                // Draw keys
                if (i == 0)
                {
                    graph.DrawString(keys[i], font, brush, xPosition + 2, 2);
                    graph.DrawString(keys[i], font, brush, xPosition + 2, img.Height - 30);
                }
                else
                {
                    if (keys[i] != keys[i - 1] && !String.IsNullOrEmpty(keys[i]))
                    {
                        graph.DrawString(keys[i], font, brush, xPosition + 2, 2);
                        graph.DrawString(keys[i], font, brush, xPosition + 2, img.Height - 30);
                    }
                }

                // Draw freqs
                graph.DrawString(freqs[i].ToString("0.00"), font1, brush1, xPosition + 2, 25);

            }
            return img;
        }

        private static Image DramPcmSmaller(double[] data, string[] keys, double[] freqs, int windowSize, int stepSize, int onScreenKeySize)
        {
            int cekorSkaliran = Convert.ToInt32(Scaler.Scaler.ScaleToLong(stepSize, windowSize, 0, onScreenKeySize, 0));
            int totalWidth = (keys.Length - 1) * cekorSkaliran + onScreenKeySize;
            if ((keys.Length - 1) * stepSize + windowSize < data.Length)
            {
                int extra = Convert.ToInt32(Scaler.Scaler.ScaleToLong(data.Length - ((keys.Length - 1) * stepSize + windowSize), windowSize, 0, onScreenKeySize, 0));
                totalWidth += extra;
            }

            Image img = new Bitmap(totalWidth, 365);
            Graphics graph = Graphics.FromImage(img);
            Pen pen = new Pen(Color.DeepSkyBlue, 1);
            Pen pen1 = new Pen(Color.Gray, 1);
            Pen pen2 = new Pen(Color.Gray, 1);
            Font font = new Font("Arial", 15, FontStyle.Regular);
            Font font1 = new Font("Arial", 10, FontStyle.Regular);
            Brush brush = new SolidBrush(Color.White);
            Brush brush1 = new SolidBrush(Color.LawnGreen);
            Brush brush2 = new SolidBrush(Color.FromArgb(60, 82, 92, 200));
            graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            graph.FillRectangle(new SolidBrush(Color.Black), 0, 0, img.Width, img.Height);

            int incremental = stepSize / onScreenKeySize;
            if (incremental == 0)
            {
                incremental = 1;
            }
            int dodatno = stepSize;

            for (int i = 0; i < keys.Length; i++)
            {
                int n = 0;
                int xPosition = i * cekorSkaliran;

                // fill the window and drow boundaries
                graph.FillRectangle(brush2, xPosition, 0, onScreenKeySize, img.Height);
                graph.DrawLine(pen1, xPosition, 0, xPosition, img.Height);
                graph.DrawLine(pen2, xPosition + onScreenKeySize, 0, xPosition + onScreenKeySize, img.Height);
                
                // end case
                if (i == keys.Length - 1)
                {
                    incremental = windowSize / onScreenKeySize;
                    dodatno = windowSize;
                    Debug.Write("New incremental: " + incremental);
                }

                // Draw pulses
                for (int j = i * stepSize; j < i * stepSize +  dodatno; j += incremental)
                {
                    if (j > data.Length)
                    {
                        break;
                    }
                    int currValue = (int)(data[j] * 150);
                    if (j == 0)
                    {
                        graph.DrawLine(pen, xPosition + n, 175, xPosition + n, 175 - currValue);
                    }
                    else
                    {
                        int prevVal = (int)(data[j - incremental] * 150);
                        graph.DrawLine(pen, xPosition + n - 1, 175 - prevVal, xPosition + n, 175 - currValue);
                    }
                    n++;
                }

                // Draw keys
                if (i == 0)
                {
                    graph.DrawString(keys[i], font, brush, xPosition + 2, 2);
                    graph.DrawString(keys[i], font, brush, xPosition + 2, img.Height - 30);
                }
                else
                {
                    if (keys[i] != keys[i - 1] && !String.IsNullOrEmpty(keys[i]))
                    {
                        graph.DrawString(keys[i], font, brush, xPosition + 2, 2);
                        graph.DrawString(keys[i], font, brush, xPosition + 2, img.Height - 30);
                    }
                }

                // Draw freqs
                graph.DrawString(freqs[i].ToString("0.00"), font1, brush1, xPosition + 2, 25);
            }
            return img;
        }

        public static Image DrawSpectrumData(double[] data)
        {
            Image img = new Bitmap(data.Length * 2, 200);
            Graphics graph = Graphics.FromImage(img);
            graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            Brush brush = new SolidBrush(Color.Blue);
            double max = data.Max();

            for (int i = 0; i < data.Length; i++)
            {
                int newValue = Convert.ToInt32(Scaler.Scaler.ScaleToDoubleGraphVersion(data[i], max, 0, 200, 0));
                graph.FillRectangle(brush, i * 2, 200 - newValue, 2, newValue);
            }
            return img;
        }

        public static Image DrawSpectrumData(Complex[] data)
        {
            Image img = new Bitmap(data.Length * 2, 200);
            Graphics graph = Graphics.FromImage(img);
            graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            Brush brush = new SolidBrush(Color.Blue);
            double max = data.Max(value => value.Real);

            for (int i = 0; i < data.Length; i++)
            {
                int newValue = Convert.ToInt32(Scaler.Scaler.ScaleToDoubleGraphVersion(data[i].Real, max, 0, 200, 0));
                graph.FillRectangle(brush, i * 2, 200 - newValue, 2, newValue);
            }
            return img;
        }
    }
}
