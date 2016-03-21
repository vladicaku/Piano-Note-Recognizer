using NAudio.Wave;
using Note_Recognizer.DSP_Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Note_Recognizer.Timeline;

namespace Note_Recognizer
{
    public partial class Form1 : Form
    {
        WAVLoader myWavReader;
        DSPEngine myDSP;
        WaveOut waveOut;
        Image originalImage;
        Timeline.Timeline timeline;
        
        int onScreenKeySize;
        String speed;

        double[] data;
        byte[] rawData;
        Thread myThread;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myWavReader = new WAVLoader();
            myDSP = new DSPEngine();
            timeline = new Timeline.Timeline();
            waveOut = new WaveOut();

            openFileDialog1.Filter = "Wav files (*.wav) | *.wav";
            openFileDialog1.Title = "Open file";
            openFileDialog1.Multiselect = false;

            cmbSpeed.SelectedIndex = 0;
            onScreenKeySize = 100;

            myDSP.WindowSize = 4096;
            myDSP.StepSize = 4096;
            myDSP.Tolerance = 7;
            myDSP.Debuging = false;
        }

        private void FillDSPEngine()
        {
            myDSP.SampleRate = myWavReader.SampleRate;
            myDSP.Data = data;
        }

        private void Start()
        {
            if (rawData == null)
            {
                return;
            }

            int newSampleRate = myWavReader.SampleRate;
            if (speed == "Slow")
            {
                newSampleRate = (int)(newSampleRate * 0.8);
            }
            else if (speed == "Slower")
            {
                newSampleRate = (int)(newSampleRate * 0.5);
            }

            pictureBox1.Image = originalImage;
            timeline.Reset();

            NAudio.Wave.WaveFormat format = new NAudio.Wave.WaveFormat(newSampleRate, myWavReader.BitDepth, myWavReader.Channels);
            waveOut = new WaveOut();
            BufferedWaveProvider waveProvider = new BufferedWaveProvider(format);
            waveProvider.BufferLength = rawData.Length;
            waveProvider.AddSamples(rawData, 0, rawData.Length);
            waveOut.Init(waveProvider);
            panel1.HorizontalScroll.Value = 0;
            tmrTimeline.Start();
            waveOut.Play();
        }

        private void Pause()
        {
            waveOut.Pause();
        }

        private void Stop()
        {
            waveOut.Stop();
            tmrTimeline.Stop();
        }

        private void Recognize()
        {
            if (myDSP.Data != null)
            {
                prgbrLoad.Value = 0;
                prgbrLoad.Visible = true;
                myThread = new Thread(myDSP.Start);
                tmrLoad.Start();
                myThread.Start();
            }
        }


        private void Settings()
        {
            Settings settings = new Settings();
            settings.txtWindowSize.Text = myDSP.WindowSize.ToString();
            settings.txtStepSize.Text = myDSP.StepSize.ToString();
            settings.txtTolerance.Text = myDSP.Tolerance.ToString();
            settings.chkDebuging.Checked = myDSP.Debuging;

            if (settings.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                myDSP.WindowSize = Convert.ToInt32(settings.txtWindowSize.Text);
                myDSP.StepSize = Convert.ToInt32(settings.txtStepSize.Text);
                myDSP.Tolerance = Convert.ToInt32(settings.txtTolerance.Text);
                myDSP.Debuging = settings.chkDebuging.Checked;
            }
        }

        private void Open()
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                myWavReader.FileLocation = openFileDialog1.FileName;
                data = myWavReader.LoadData();
                rawData = myWavReader.LoadBytes();
                txtFileName.Text = String.Format("File name:  {0}", openFileDialog1.SafeFileName);
                txtSampleRate.Text = String.Format("Sample rate:  {0} Hz", myWavReader.SampleRate);
                txtBitDepth.Text = String.Format("Bith depth:  {0} bits", myWavReader.BitDepth);
                FillDSPEngine();
                Recognize();
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Convert.ToInt32(waveOut.GetPosition()) >= myWavReader.BytesLen)
            {
                Debug.WriteLine("STOP");
                waveOut.Stop();
                tmrTimeline.Stop();
            }

            // set timeline position
            int timelinePosition = Convert.ToInt32(Scaler.Scaler.ScaleToLong(Convert.ToInt32(waveOut.GetPosition()), myWavReader.BytesLen, 0, originalImage.Width, 0));
            timeline.CurrentPosition = timelinePosition;
            pictureBox1.Image = timeline.GetImage();

            // auto-scrool
            if (timelinePosition >= panel1.Width + panel1.HorizontalScroll.Value)
            {
                panel1.HorizontalScroll.Value += panel1.Width;
            }
        }

        private void WriteDataToFile(double[] data, int startPos, int len, String filePath)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = startPos; i < startPos + len; i++)
            {
                builder.Append(String.Format("{0:#.000000}", data[i]));
                builder.Append(",");
            }
            File.WriteAllText(filePath, builder.ToString());
        }

        private void WriteKeysToFile(string[] keys, int startPos, int len, String filePath)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = startPos; i < startPos + len; i++)
            {
                builder.Append(keys[i]);
                builder.Append(",");
            }
            File.WriteAllText(filePath, builder.ToString());
        }

      

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void tmrLoadProgress_Tick(object sender, EventArgs e)
        {
            if (myThread != null && !myThread.IsAlive)
            {
                prgbrLoad.Visible = false;
                originalImage = GraphDrawer.DramPcm(myDSP.Data, myDSP.Keys.ToArray(), myDSP.Freqs.ToArray(), myDSP.WindowSize, myDSP.StepSize, onScreenKeySize);
                timeline.SetOriginalImage(originalImage);
                pictureBox1.Image = originalImage;
                tmrLoad.Stop();
            }
            if (myDSP.LoadProgress <= 100)
            {
                prgbrLoad.Value = myDSP.LoadProgress;
            }
            else
            {
                prgbrLoad.Value = prgbrLoad.Maximum;
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void cmbSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            speed = cmbSpeed.Items[cmbSpeed.SelectedIndex].ToString();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Piano Notes Recognizer v1.0 \nVladica Jovanovski \nFINKI © 2015", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void setWindowParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings();
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            Settings();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            Recognize();
        }

        private void recognizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Recognize();
        }

        
    }
}
