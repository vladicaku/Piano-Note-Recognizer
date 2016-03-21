using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Note_Recognizer
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(txtWindowSize.Text) % 2 != 0)
            {
                MessageBox.Show("Window size should be a power of 2.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void txtWindowSize_Enter(object sender, EventArgs e)
        {
            BeginInvoke((Action)delegate
            {
                txtWindowSize.SelectAll();
            });
        }

        private void txtStepSize_Enter(object sender, EventArgs e)
        {
            BeginInvoke((Action)delegate
            {
                txtStepSize.SelectAll();
            });
        }

        private void txtTolerance_Enter(object sender, EventArgs e)
        {
            BeginInvoke((Action)delegate
            {
                txtTolerance.SelectAll();
            });
        }
    }
}
