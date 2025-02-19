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
    public partial class AddWallet : Form
    {
        BTCSettingsForm parent;
        public AddWallet(BTCSettingsForm parent)
        {
            InitializeComponent();
            this.parent = parent;
            textBox2.Visible = false;
            labelKey.Visible = false;
            textBox3.Visible = false;
            labelSecret.Visible = false;
        }
        public bool checkRowsValue()
        {
            if (textBox1.Text == "")
                return false;
            if (comboBox1.SelectedItem==null)
                return false;
            if ((string)comboBox1.SelectedItem == "Bittrex" && textBox2.Text == "")
                return false;
            if ((string)comboBox1.SelectedItem == "NiceHash" && (textBox2.Text == "" || textBox3.Text == ""))
                return false;
            label4.Visible = false;
            buttonOk.Visible = true;
            return true;
        }
        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (!checkRowsValue())
            {
                DialogResult confirmationWindow = MessageBox.Show("Заполните всю информацию о кошельках"
                                   , "Подсказка", MessageBoxButtons.OK);
            }
            else
            {
                string secret = textBox2.Text + (textBox3.Text != "" ? ":" : "") + textBox3.Text;
                parent.addRowToGrid(textBox1.Text, (string)comboBox1.SelectedItem,secret);          
                parent.Visible = true;
                this.Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            parent.Visible = true;
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!checkRowsValue())
            {
                label4.Visible = true;
                buttonOk.Visible = false;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!checkRowsValue())
            {
                label4.Visible = true;
                buttonOk.Visible = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString()== "Bittrex")
            {
                textBox2.Visible = true;
                labelKey.Visible = true;
                textBox3.Visible = false;
                labelSecret.Visible = false;
                labelKey.Text = "Bittrex secret";
                textBox3.Text = "";
            }
            else
            {
                if (comboBox1.SelectedItem.ToString() == "NiceHash")
                {
                    textBox2.Visible = true;
                    labelKey.Visible = true;
                    labelKey.Text = "NiceHash ID";
                    textBox3.Visible = true;
                    labelSecret.Visible = true;
                    labelSecret.Text = "NiceHash Key (Readonly)";
                }
                else
                {
                    textBox2.Visible = false;
                    labelKey.Visible = false;
                    textBox3.Visible = false;
                    labelSecret.Visible = false;
                    textBox2.Text = "";
                    textBox3.Text = "";
                }
            }
            if (!checkRowsValue())
            {
                label4.Visible = true;
                buttonOk.Visible = false;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (!checkRowsValue())
            {
                label4.Visible = true;
                buttonOk.Visible = false;
            }
        }

    }
}
