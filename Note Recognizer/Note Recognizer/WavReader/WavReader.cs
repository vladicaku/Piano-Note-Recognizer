using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Note_Recognizer
{
    public class WAVLoader
    {
        public String FileLocation {get; set;}
        public int SampleRate { get; set; }
        public int BitDepth { get; set; }
        public int Channels { get; set; }

        public int BytesLen { get; set; }
        public int DataLen { get; set; }

        public byte[] LoadBytes()
        {
            if (String.IsNullOrEmpty(FileLocation))
            {
                throw new Exception("Invalid file location.");
            }

            BinaryReader reader = new BinaryReader(File.Open(FileLocation, FileMode.Open));
            int chunkID = reader.ReadInt32();
            int fileSize = reader.ReadInt32();
            int riffType = reader.ReadInt32();
            int fmtID = reader.ReadInt32();
            int fmtSize = reader.ReadInt32();
            int fmtCode = reader.ReadInt16();
            int channels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            int fmtAvgBPS = reader.ReadInt32();
            int fmtBlockAlign = reader.ReadInt16();
            int bitDepth = reader.ReadInt16();
            if (fmtSize == 18)
            {
                // Read any extra values
                int fmtExtraSize = reader.ReadInt16();
                reader.ReadBytes(fmtExtraSize);
            }
            if (channels != 1)
            {
                throw new Exception("Sorry. Only one channel (mono) files are supported for now.");
            }
            SampleRate = sampleRate;
            BitDepth = bitDepth;
            Channels = channels;

            int dataID = reader.ReadInt32();
            int dataSize = reader.ReadInt32();
            List<byte> data = new List<byte>(dataSize);

            for (int i = 0; i < dataSize; i++)
            {
                data.Add(reader.ReadByte());
            }

            reader.Close();
            BytesLen = data.Count;
            return data.ToArray();
        }

        public double[] LoadData()
        {
            if (String.IsNullOrEmpty(FileLocation))
            {
                throw new Exception("Invalid file location.");
            }

            BinaryReader reader = new BinaryReader(File.Open(FileLocation, FileMode.Open));
            int chunkID = reader.ReadInt32();
            int fileSize = reader.ReadInt32();
            int riffType = reader.ReadInt32();
            int fmtID = reader.ReadInt32();
            int fmtSize = reader.ReadInt32();
            int fmtCode = reader.ReadInt16();
            int channels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            int fmtAvgBPS = reader.ReadInt32();
            int fmtBlockAlign = reader.ReadInt16();
            int bitDepth = reader.ReadInt16();
            if (fmtSize == 18)
            {
                // Read any extra values
                int fmtExtraSize = reader.ReadInt16();
                reader.ReadBytes(fmtExtraSize);
            }
            if (channels != 1)
            {
                throw new Exception("Sorry. Only one channel (mono) files are supported for now."); 
            }
            SampleRate = sampleRate;
            BitDepth = bitDepth;
            Channels = channels;

            int dataID = reader.ReadInt32();
            int dataSize = reader.ReadInt32();
            List<double> data = new List<double>(dataSize);
            
            if (bitDepth == 8)
            {
                for (int i = 0; i < dataSize / fmtBlockAlign; i++)
                {
                    // should be tested
                    byte temp = reader.ReadByte();
                    int oldValue = Convert.ToUInt16(temp);
                    double newValue = Scaler.Scaler.ScaleToDouble(oldValue, Byte.MaxValue, 0, 1, -1);
                    data.Add(newValue);
                }
            }
            else if (bitDepth == 16)
            {
                for (int i = 0; i < dataSize / fmtBlockAlign; i++)
                {
                    byte[] temp = reader.ReadBytes(2);
                    temp.Reverse();
                    int oldValue = BitConverter.ToInt16(temp, 0);
                    double newValue = Scaler.Scaler.ScaleToDouble(oldValue, Int16.MaxValue, Int16.MinValue, 1, -1);
                    data.Add(newValue);
                }
            }
            else if (bitDepth == 24)
            {
                throw new Exception("Audio files with bit depth 24 are not supported.");
                
            }
            else if (bitDepth == 32)
            {
                for (int i = 0; i < dataSize / fmtBlockAlign; i++)
                {
                    byte[] temp = reader.ReadBytes(4);
                    temp.Reverse();
                    long oldValue = BitConverter.ToInt32(temp, 0);
                    double newValue = Scaler.Scaler.ScaleToDouble(oldValue, Int32.MaxValue, Int32.MinValue, 1, -1);
                    data.Add(newValue);
                }
            }
            
            reader.Close();
            DataLen = data.Count;
            return data.ToArray();
        }
        
    }
}
