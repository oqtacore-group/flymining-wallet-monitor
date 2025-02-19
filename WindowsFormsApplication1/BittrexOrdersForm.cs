using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
namespace BitcoinInfoMiner
{

    public partial class BittrexOrdersForm : Form
    {
        private string currentWallet;
        public BTCBalanceForm parent;
        public string bittrexOldOrderHistory = "BittrexOrders";

        public BittrexOrdersForm(BTCBalanceForm parent,string wallet)
        {
            InitializeComponent();
            currentWallet = wallet;
            this.parent = parent;
            populateDataGrid();
            
        }
        private  void populateDataGrid()
        {
            for (int iter = 0; iter < Wallets.walletInfo["Bittrex"].Count(); iter ++)
                if (Wallets.walletInfo["Bittrex"][iter]==currentWallet)
                {
                      Wallets.parseOrderHistory(currentWallet, Wallets.walletInfo["Bittrex"][iter+1]);
                }            
            dataGridView1.Rows.Clear();
            parseOrderHistory(currentWallet);
            //for (int iter = 0; iter < Wallets.walletInfo["Bittrex"].Count(); iter += 2)
            //    if (iter + 1 < Wallets.walletInfo["Bittrex"].Count())
            //    {
            //        parseOrderHistory(Wallets.walletInfo["Bittrex"][iter]);
            //    }
             parseWallets(Wallets.walletInfo["Bittrex"]); 
        }
        public  void parseWallets(string[] wallets)
        {

            for (int iterMain = 0; iterMain < wallets.Length; iterMain += 2)
            {
                if (wallets[iterMain] == currentWallet)
                {
                    string responce = "";
                    if (iterMain + 1 < wallets.Length)
                        responce =  Wallets.sendBittrexRequest("account/getorderhistory",
                             wallets[iterMain], wallets[iterMain + 1]);
                    else
                        break;

                    OrderResponce operationResponse = new OrderResponce();
                    operationResponse = JsonConvert.DeserializeObject<OrderResponce>(responce);

                    //JArray blogPostArray = JArray.Parse(operationResponse3.txs);

                    IList<OrderBody> mainBodyAll = operationResponse.result.Select(p => new OrderBody
                    {
                        Closed = (string)p["Closed"],
                        Exchange = (string)p["Exchange"],
                        OrderType = (string)p["OrderType"],
                        Quantity = (string)p["Quantity"],
                        PricePerUnit = (double)p["PricePerUnit"],
                        Price = (string)p["Price"],
                        Commission = (string)p["Commission"],
                        Opened = (string)p["TimeStamp"],
                        OrderUuid = (string)p["OrderUuid"],
                        Limit = (string)p["Limit"],
                    }).ToList();

                    for (int iter = 0; iter < mainBodyAll.Count; iter++)
                    {
                        bool check = false;
                        mainBodyAll[iter].Closed = DateTime.ParseExact(mainBodyAll[iter].Closed, "MM'/'dd'/'yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        mainBodyAll[iter].Opened = DateTime.ParseExact(mainBodyAll[iter].Opened, "MM'/'dd'/'yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        for (int i = 0; i < Wallets.orderHistory.Count; i++)
                        {
                            if (Wallets.orderHistory[i].Closed == mainBodyAll[iter].Closed)
                                check = true;
                        }
                        if (!check)
                        {
                            try
                            {
                                dataGridView1.Rows.Add(1);
                                Wallets.orderHistory.Add(Wallets.orderHistory.Count, mainBodyAll[iter]);
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0].Value = Convert.ToDateTime(mainBodyAll[iter].Closed, CultureInfo.InvariantCulture);
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = mainBodyAll[iter].Exchange;
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[2].Value = mainBodyAll[iter].OrderType;
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[3].Value = mainBodyAll[iter].Quantity;
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Value = mainBodyAll[iter].PricePerUnit;
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[5].Value = Convert.ToDouble(mainBodyAll[iter].Price, CultureInfo.InvariantCulture)
                                    - Convert.ToDouble(mainBodyAll[iter].Commission, CultureInfo.InvariantCulture);
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[6].Value = iter;
                            }
                            catch (Exception ex)
                            {
                                Log.logDebug("parsingWallets Orders " + Convert.ToString(ex));
                                check = true;
                            }
                        }


                    }
                    parent.saveOrderHistory(wallets[iterMain]);
                    dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);
                }
            }
        }
        public void parseOrderHistory(string wallet)
        {
            //string fileName = Directory.GetCurrentDirectory() + "//" + bittrexOldOrderHistory + wallet + ".csv";
            //if (!File.Exists(fileName))
            //{
            //    File.Create(fileName);
            //}
            //string[] fileText = File.ReadAllText(fileName, Encoding.Unicode).Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            //Dictionary<string, string[]> result = new Dictionary<string, string[]>();
            //string[] columnName = fileText[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //for (int iter = 0; iter < columnName.Count(); iter++)
            //    result.Add(columnName[iter], new string[fileText.Count() - 1]);

            //for (int iter = 1; iter < fileText.Count(); iter++)
            //{
            //    string[] columnValue = fileText[iter].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //    for (int iterIner = 0; iterIner < columnValue.Count(); iterIner++)
            //    {
            //        result[columnName[iterIner]][iter - 1] = columnValue[iterIner];
            //    }
            //}

            for (int iter = 0; iter < Wallets.orderHistory.Count(); iter++)
            {
                dataGridView1.Rows.Add(1);
                dataGridView1.Rows[iter].Cells[0].Value = Convert.ToDateTime(Wallets.orderHistory[iter].Closed, CultureInfo.InvariantCulture);
                dataGridView1.Rows[iter].Cells[1].Value = Wallets.orderHistory[iter].Exchange;
                dataGridView1.Rows[iter].Cells[2].Value = Wallets.orderHistory[iter].OrderType;
                dataGridView1.Rows[iter].Cells[3].Value = Wallets.orderHistory[iter].Quantity;
                dataGridView1.Rows[iter].Cells[4].Value = Convert.ToDouble(Wallets.orderHistory[iter].Price, CultureInfo.InvariantCulture)
                    / Convert.ToDouble(Wallets.orderHistory[iter].Quantity, CultureInfo.InvariantCulture);
                dataGridView1.Rows[iter].Cells[5].Value = Convert.ToDouble(Wallets.orderHistory[iter].Price, CultureInfo.InvariantCulture)
                    - Convert.ToDouble(Wallets.orderHistory[iter].Commission, CultureInfo.InvariantCulture);
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[6].Value = iter;

            }
            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);
        }

        private void buttonCSVImport_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
            if (!string.IsNullOrWhiteSpace(openFileDialog.FileName) && File.Exists(openFileDialog.FileName) )
            {
                //string parseData = File.ReadAllText(openFileDialog.FileName);
                string fileName = Directory.GetCurrentDirectory() + "//" + Wallets.bittrexOldOrderHistory + currentWallet + ".csv";
                File.Copy(openFileDialog.FileName, fileName,true);
                populateDataGrid();
            }
        }
    }
}
