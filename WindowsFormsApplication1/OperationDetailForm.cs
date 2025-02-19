using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;
namespace BitcoinInfoMiner
{
    public partial class OperationDetailForm : Form
    {
        public OperationDetailForm(DataGridViewRow row, TransactionInfo data,string [] wallets)
        {
            InitializeComponent();
            labelTime.Text = Convert.ToString(row.Cells[1].Value, CultureInfo.InvariantCulture);
            labelAmountSent.Text = Convert.ToString(row.Cells[4].Value, CultureInfo.InvariantCulture);
            labelFee.Text = Convert.ToString(Math.Round(Convert.ToDouble(row.Cells[8].Value, CultureInfo.InvariantCulture) / 100000000, 8));
            labelSize.Text = Convert.ToString(data.size, CultureInfo.InvariantCulture);
            textBox1.Text = Convert.ToString(row.Cells[2].Value, CultureInfo.InvariantCulture);
            listViewInput.Items.Clear();
            listViewInput.View = View.Details;
            listViewInput.Columns.Add("Data");
            listViewInput.Columns[0].Width = listViewInput.Width - 4;
            listViewInput.HeaderStyle = ColumnHeaderStyle.None;
            for (int iter = 0; iter < data.inputs.Count; iter++)
            {
                listViewInput.Items.Add(data.inputs[iter]);
                foreach (string search in wallets)
                {
                    if (data.inputs[iter].Contains(search.ToLower()) || data.inputs[iter].Contains(search))
                    {
                        listViewInput.Items[iter].BackColor = Color.LightGreen;
                    }
                }
            }
            listViewOutput.Items.Clear();
            listViewOutput.View = View.Details;
            listViewOutput.Columns.Add("Data");
            listViewOutput.Columns[0].Width = listViewOutput.Width - 4;
            listViewOutput.HeaderStyle = ColumnHeaderStyle.None;
            for (int iter = 0; iter < data.Out.Count; iter++)
            {
                listViewOutput.Items.Add(data.Out[iter]);
                foreach (string search in wallets)
                {
                    if (data.Out[iter].Contains(search.ToLower()) || data.Out[iter].Contains(search))
                    {
                        listViewOutput.Items[iter].BackColor = Color.LightGreen;
                    }
                }
            }

            

        }
        public OperationDetailForm(DataGridViewRow row, TransactionInfo data, string wallet)
        {
            InitializeComponent();
            labelTime.Text = Convert.ToString(row.Cells[1].Value, CultureInfo.InvariantCulture);
            labelAmountSent.Text = Convert.ToString(row.Cells[4].Value, CultureInfo.InvariantCulture);
            labelFee.Text = Convert.ToString(Math.Round(Convert.ToDouble(row.Cells[8].Value, CultureInfo.InvariantCulture) / 100000000, 8));
            labelSize.Text = Convert.ToString(data.size, CultureInfo.InvariantCulture);
            textBox1.Text = Convert.ToString(row.Cells[2].Value, CultureInfo.InvariantCulture);
            listViewInput.Items.Clear();
            listViewInput.View = View.Details;
            listViewInput.Columns.Add("Data");
            listViewInput.Columns[0].Width = listViewInput.Width - 4;
            listViewInput.HeaderStyle = ColumnHeaderStyle.None;
            for (int iter = 0; iter < data.inputs.Count; iter++)
            {
                listViewInput.Items.Add(data.inputs[iter]);
                if (data.inputs[iter].Contains(wallet.ToLower()) || data.inputs[iter].Contains(wallet))
                    {
                        listViewInput.Items[iter].BackColor = Color.LightGreen;
                    }
            }
            listViewOutput.Items.Clear();
            listViewOutput.View = View.Details;
            listViewOutput.Columns.Add("Data");
            listViewOutput.Columns[0].Width = listViewOutput.Width - 4;
            listViewOutput.HeaderStyle = ColumnHeaderStyle.None;
            for (int iter = 0; iter < data.Out.Count; iter++)
            {
                listViewOutput.Items.Add(data.Out[iter]);
                if (data.Out[iter].Contains(wallet.ToLower()) || data.Out[iter].Contains(wallet))
                    {
                        listViewOutput.Items[iter].BackColor = Color.LightGreen;
                    }
            }



        }

    }
}
