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
    public partial class BTCSettingsForm : Form
    {

        BTCBalanceForm parent;
        public BTCSettingsForm(BTCBalanceForm parent)
        {
            InitializeComponent();
            this.parent = parent;
            //walletInfo = new Dictionary<string, string[]>();
            //walletInfo.Add("Bittrex", new string[] { "324", "3434" });
            foreach(string key in Wallets.walletInfo.Keys)
            {
                if (key=="Bittrex")
                {
                    for (int iter = 0; iter < Wallets.walletInfo[key].Length; iter += 2)
                    {
                        if (iter + 1 < Wallets.walletInfo[key].Length)
                            dataGridView1.Rows.Add(Wallets.walletInfo[key][iter], key, Wallets.walletInfo[key][iter + 1]);
                    }
                     
                }
                else 
                {
                    if (key == "NiceHash")
                    {
                        for (int iter = 0; iter < Wallets.walletInfo[key].Length; iter += 3)
                        {
                            if (iter + 2 < Wallets.walletInfo[key].Length)
                                dataGridView1.Rows.Add(Wallets.walletInfo[key][iter], key, Wallets.walletInfo[key][iter + 1] + ":" + Wallets.walletInfo[key][iter + 2]);
                        }
                    }
                    else
                    {
                        for (int iter = 0; iter < Wallets.walletInfo[key].Length; iter++)
                        {
                            dataGridView1.Rows.Add(Wallets.walletInfo[key][iter], key, "");
                        } 
                    }
                }
            }
        }
        private bool checkRowsValue()
        {
            return false;
        }
        private void parseDataGrid()
        {
            for (int iter=0;iter<dataGridView1.Rows.Count;iter++)
            {
                if ((string)dataGridView1.Rows[iter].Cells["WalletSite"].Value!=""
                    && Wallets.walletInfo.Keys.Contains((string)dataGridView1.Rows[iter].Cells["WalletSite"].Value))
                {
                    string siteName = (string)dataGridView1.Rows[iter].Cells["WalletSite"].Value;
                    if (siteName=="Bittrex")
                    {
                        if (!Wallets.walletInfo[siteName].Contains((string)dataGridView1.Rows[iter].Cells[0].Value))
                        {
                            string[] tempArray = Wallets.walletInfo[siteName];
                            Array.Resize(ref tempArray, Wallets.walletInfo[siteName].Length + 2);
                            tempArray[Wallets.walletInfo[siteName].Length] = (string)dataGridView1.Rows[iter].Cells[0].Value;
                            tempArray[Wallets.walletInfo[siteName].Length + 1] = (string)dataGridView1.Rows[iter].Cells[2].Value;
                            Wallets.walletInfo[siteName] = tempArray;

                            tempArray = Wallets.walletInfoDescription[siteName];
                            Array.Resize(ref tempArray, Wallets.walletInfoDescription[siteName].Length + 2);
                            tempArray[Wallets.walletInfoDescription[siteName].Length] = "";
                            tempArray[Wallets.walletInfoDescription[siteName].Length + 1] = "";
                            Wallets.walletInfoDescription[siteName] = tempArray;
                        }
                    }
                    else
                    {
                        if (siteName == "NiceHash")
                        {
                            if (!Wallets.walletInfo[siteName].Contains((string)dataGridView1.Rows[iter].Cells[0].Value))
                            {
                                string[] tempArray = Wallets.walletInfo[siteName];
                                Array.Resize(ref tempArray, Wallets.walletInfo[siteName].Length + 3);
                                tempArray[Wallets.walletInfo[siteName].Length] = (string)dataGridView1.Rows[iter].Cells[0].Value;
                                string secretData = dataGridView1.Rows[iter].Cells[2].Value.ToString();
                                tempArray[Wallets.walletInfo[siteName].Length + 1] = secretData.Remove(secretData.IndexOf(":"));
                                tempArray[Wallets.walletInfo[siteName].Length + 2] = secretData.Substring(secretData.IndexOf(":") + 1);
                                Wallets.walletInfo[siteName] = tempArray;

                                tempArray = Wallets.walletInfoDescription[siteName];
                                Array.Resize(ref tempArray, Wallets.walletInfoDescription[siteName].Length + 3);
                                tempArray[Wallets.walletInfoDescription[siteName].Length] = "";
                                tempArray[Wallets.walletInfoDescription[siteName].Length + 1] = "";
                                tempArray[Wallets.walletInfoDescription[siteName].Length + 2] = "";
                                Wallets.walletInfoDescription[siteName] = tempArray;
                            }
                        }
                        else
                        {
                            if (!Wallets.walletInfo[siteName].Contains((string)dataGridView1.Rows[iter].Cells[0].Value))
                            {
                                string[] tempArray = Wallets.walletInfo[siteName];
                                Array.Resize(ref tempArray, Wallets.walletInfo[siteName].Length + 1);
                                tempArray[Wallets.walletInfo[siteName].Length] = (string)dataGridView1.Rows[iter].Cells[0].Value;
                                Wallets.walletInfo[siteName] = tempArray;

                                tempArray = Wallets.walletInfoDescription[siteName];
                                Array.Resize(ref tempArray, Wallets.walletInfoDescription[siteName].Length + 1);
                                tempArray[Wallets.walletInfoDescription[siteName].Length] = "";
                                Wallets.walletInfoDescription[siteName] = tempArray;
                            }
                        }
                    }
                    
                }
            }
        }
        public void addRowToGrid(string wallet,string site,string secretKey="")
        {
            if (site != "Bittrex" && site != "NiceHash")
            {
                dataGridView1.Rows.Add(wallet, site, "");
            }
            else
            {
                dataGridView1.Rows.Add(wallet, site, secretKey);
            }

        }
        private  void button1_Click(object sender, EventArgs e)
        {
            if (checkRowsValue())
            {
                DialogResult confirmationWindow = MessageBox.Show("Error.Invalid row. Delete it before continue"
                                   , "Error", MessageBoxButtons.OK);
            }
            else
            {
                parseDataGrid();
                parent.createDataGrid();
                parent.Visible = true;
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            parent.Visible = true;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AddWallet addWaletForm = new AddWallet(this);
            this.Visible = false;
            addWaletForm.Show();
            //dataGridView1.Rows.Add(1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            if (dataGridView1.SelectedRows!=null)
            {
                DialogResult confirmationWindow = MessageBox.Show(" Delete this row?"              
                                   , "Deleting rows", MessageBoxButtons.YesNo);
                if (confirmationWindow == DialogResult.Yes)
                {
                    foreach (DataGridViewRow item in this.dataGridView1.SelectedRows)
                    {
                        Wallets.walletInfo[(string)item.Cells[1].Value] = Wallets.walletInfo[(string)item.Cells[1].Value].
                            Where(val => val != (string)dataGridView1.Rows[item.Index].Cells[0].Value).ToArray();
                        if ((string)item.Cells["WalletSite"].Value == "Bittrex")
                        {
                            Wallets.walletInfo[(string)item.Cells[1].Value] = Wallets.walletInfo[(string)item.Cells[1].Value].
                            Where(val => val != (string)dataGridView1.Rows[item.Index].Cells[3].Value).ToArray();
                        }
                        dataGridView1.Rows.RemoveAt(item.Index);
                    }
                }
            }
            else
            {
                DialogResult confirmationWindow = MessageBox.Show("Choose whole row for deleting"
                                   , "Advice", MessageBoxButtons.OK);
            }
        }

        private void BTCSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            parent.Visible = true;
        }



    }
}
