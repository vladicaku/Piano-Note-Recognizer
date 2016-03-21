using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Note_Recognizer.Timeline
{
    public class Timeline
    {
        public int CurrentPosition { get; set; }
        private Image originalImage;
        private Image image;

        public void SetOriginalImage(Image originalImage)
        {
            this.originalImage = originalImage;
        }

        public void Reset()
        {
            image = new Bitmap(originalImage);
        }

        public Image GetImage()
        {
            if (CurrentPosition > image.Width)
            {
                return image;
            }
            Graphics graph = Graphics.FromImage(image);
            graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            Pen pen = new Pen(Color.Red, 4);
            graph.DrawLine(pen, 0, image.Height - 2, CurrentPosition, image.Height - 2);
            return image;
        }
    }
}
