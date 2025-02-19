using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BitcoinInfoMiner
{
    public partial class InputDialogForm : Form
    {
        Form parent;
        public InputDialogForm(Form parent,string text,string caption)
        {
            InitializeComponent();
            labelText.Text = text;
            this.Text = caption;
            this.parent = parent;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            parent.Visible = true;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            parent.Visible = true;
        }

    }
}
