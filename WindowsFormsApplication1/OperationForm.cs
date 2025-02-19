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
namespace BitcoinInfoMiner
{



    public partial class OperationForm : Form
    {


        public TransactionInfo[] rowFullInfo;
        public BTCBalanceForm parent;
        private string wallet;
        private string[] wallets;
        private string key;

        ////Конструктор для одного холодного кошелька
        //public OperationForm(BTCBalanceForm parent,string wallet)
        //{
        //    InitializeComponent();
        //    this.parent = parent;
        //    this.wallet = wallet;
        //    this.wallets = new string[0];
        //    label1.Text = "Операции холодного кошелька " + wallet;
        //    parseBlockchainOperation();
        //}
        ////Конструктор для всех холодных кошельков(Временно)
        //public OperationForm(BTCBalanceForm parent, string[] wallets)
        //{
        //    InitializeComponent();
        //    this.parent = parent;
        //    this.wallet = "";
        //    this.wallets = wallets;
        //    label1.Text = "Все операции с холодными кошельками BTC";
            
        //}
        //Конструктор для Bittrex для конкретной валюты
        public OperationForm(BTCBalanceForm parent, string[] wallets, string key,string currency, double balance)
        {
            InitializeComponent();
            this.parent = parent;
            this.wallets = wallets;
            this.wallet = "";
            this.key = key;
            dataGridView1.Columns[4].HeaderText = "Amount " + currency;
            dataGridView1.Columns[5].HeaderText = "Balance " + currency;
            label1.Text = "Bittrex operations with  "+currency+" currency";
            for (int iter = 0; iter < wallets.Length;iter+=2 )
                label1.Text += "\n\r "+wallets[iter]; 
            if (key == "Bittrex")
                parseBittrexOperation(balance, currency);              
            else
                this.Close();

        }
        //Конструктор для других кошельков
        public OperationForm(BTCBalanceForm parent, string[] wallets,string key,double balance=0)
        {
            InitializeComponent();
            this.parent = parent;
            this.wallets = wallets;
            this.wallet = "";
            this.key = key;
            
            switch (key)
            {
                case "ETH":
                    label1.Text = "Operations ETH";
                    dataGridView1.Columns[4].HeaderText = "Amount ETH";
                    dataGridView1.Columns[5].HeaderText = "Balance ETH";
                     parseETHOperation(balance); 
                    break;
                case "LTC":
                    label1.Text = "Operations LTC";
                    dataGridView1.Columns[4].HeaderText = "Amount LTC";
                    dataGridView1.Columns[5].HeaderText = "Balance LTC";
                    parseLTCOperation(balance);
                    
                    break;
                case "NiceHash":
                    label1.Text = "Operations BTC";
                    dataGridView1.Columns[4].HeaderText = "Amount BTC";
                    dataGridView1.Columns[5].HeaderText = "Balance BTC";
                    parseNiceHashOperation(balance);

                    break;
                case "BTC cold wallet":
                    label1.Text = "Cold Wallet BTC";
                    dataGridView1.Columns[4].HeaderText = "Amount BTC";
                    dataGridView1.Columns[5].HeaderText = "Balance BTC";
                    parseAllBlockchainOperation();               
                    break;
            }
            for (int iter = 0; iter < wallets.Length; iter++)
                label1.Text += "\n\r " + wallets[iter];
            label1.TextAlign = ContentAlignment.MiddleCenter;
            groupBox1.Location = new System.Drawing.Point(0, label1.PreferredHeight+10);
            
        }
        //Парсинг Bittrex
        public  void parseBittrexOperation(double balance,string currency)
        {
            Dictionary<string, string> arg = new Dictionary<string, string>();
            arg.Add("currency", currency);
            for (int iterMain = 0; iterMain < wallets.Length; iterMain += 2)
            {
                if (iterMain + 1 >+ wallets.Length)
                    break;
                var mainBodyAll= Wallets.getBittrexOperation(wallets[iterMain],wallets[iterMain + 1],currency);
                int id = 0;
                rowFullInfo = new TransactionInfo[mainBodyAll.Count];
                for (int iter = 0; iter < mainBodyAll.Count; iter++)
                {
                    rowFullInfo[iter] = new TransactionInfo();
                    rowFullInfo[iter].Out = new Dictionary<int, string>();
                    rowFullInfo[iter].inputs = new Dictionary<int, string>();
                    dataGridView1.Rows.Add(1);
                    dataGridView1.Rows[id].Cells["ID"].Value = iter;
                    dataGridView1.Rows[id].Cells["Operation"].Value = mainBodyAll[iter].TxId;
                    dataGridView1.Rows[id].Cells["Data"].Value = Convert.ToDateTime(mainBodyAll[iter].date, CultureInfo.InvariantCulture);
                    if (mainBodyAll[iter].TxCost==0)
                    {
                        dataGridView1.Rows[id].Cells[4].Value = Math.Round(Convert.ToDecimal(mainBodyAll[iter].amount, CultureInfo.InvariantCulture), 8);
                        dataGridView1.Rows[id].Cells[4].Style.BackColor = Color.LightGreen;
                        dataGridView1.Rows[id].Cells[6].Style.BackColor = Color.LightGreen;
                        rowFullInfo[iter].inputs.Add(0, mainBodyAll[iter].address + "     Value=" + Convert.ToString(mainBodyAll[iter].amount, CultureInfo.InvariantCulture));
                        rowFullInfo[iter].Out.Add(0, wallets[iterMain] + "     Value=" + Convert.ToString(mainBodyAll[iter].amount, CultureInfo.InvariantCulture));
                    }
                    else                      
                    {
                        dataGridView1.Rows[id].Cells[4].Value = Math.Round(Convert.ToDecimal(mainBodyAll[iter].amount * -1, CultureInfo.InvariantCulture), 8); 
                        dataGridView1.Rows[id].Cells[4].Style.BackColor = Color.LightPink;
                        dataGridView1.Rows[id].Cells[6].Style.BackColor = Color.LightPink;
                        rowFullInfo[iter].Out.Add(0, mainBodyAll[iter].address + "     Value=" + Convert.ToString(mainBodyAll[iter].amount, CultureInfo.InvariantCulture));
                        rowFullInfo[iter].inputs.Add(0, wallets[iterMain] + "     Value=" + Convert.ToString(mainBodyAll[iter].amount, CultureInfo.InvariantCulture));
                    }
                    dataGridView1.Columns["ID"].Visible = false;
                    id++;
                }
                dataGridView1.Sort(dataGridView1.Columns["Data"], ListSortDirection.Descending);
                //string[] testing=new string[dataGridView1.Rows.Count];
                //for (int itern=0;itern<dataGridView1.Rows.Count;itern++)
                //    testing[itern]=Convert.ToString(dataGridView1.Rows[itern].Cells[5].Value);
                foreach (int key in Wallets.orderHistory.Keys)
                {
                    bool check = false;
                    if (Wallets.orderHistory[key].Exchange.Contains(currency))
                    {
                        double orderBalance = 0;
                        if (currency != "BTC")
                            orderBalance = Math.Round(Convert.ToDouble(Wallets.orderHistory[key].Quantity, CultureInfo.InvariantCulture), 8);
                        else
                            orderBalance = Math.Round(Convert.ToDouble(Wallets.orderHistory[key].Price, CultureInfo.InvariantCulture), 8);
                        if (Wallets.orderHistory[key].OrderType.Contains("BUY") && currency == "BTC")
                        {
                            orderBalance = orderBalance * -1 - Math.Round(Convert.ToDouble(Wallets.orderHistory[key].Commission, CultureInfo.InvariantCulture), 8);
                        }
                        else
                        {
                            if (Wallets.orderHistory[key].OrderType.Contains("SELL") && currency != "BTC")
                                orderBalance = orderBalance * -1;
                            else
                                if (Wallets.orderHistory[key].OrderType.Contains("SELL") && currency == "BTC")
                                    orderBalance = orderBalance - Math.Round(Convert.ToDouble(Wallets.orderHistory[key].Commission, CultureInfo.InvariantCulture), 8);
                        }
                        DateTime keyDate = DateTime.ParseExact(Wallets.orderHistory[key].Closed, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        DateTime test=new DateTime();
                        try
                        {
                            test = Convert.ToDateTime(Convert.ToString(dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells["Data"].Value, CultureInfo.InvariantCulture),
                                    CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex)
                        {
                            Log.logDebug("Bittrex orders :" + Convert.ToString(ex));
                            dataGridView1.Rows.Add(1);
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells["Data"].Value = keyDate;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells["Operation"].Value = "Order " +
                                Wallets.orderHistory[key].Exchange + " " + Wallets.orderHistory[key].OrderType;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Value = Math.Round(Convert.ToDecimal(orderBalance, CultureInfo.InvariantCulture), 8);
                            if (orderBalance > 0)
                            {
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Style.BackColor = Color.LightGreen;
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[6].Style.BackColor = Color.LightGreen;
                            }
                            else
                            {
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Style.BackColor = Color.LightPink;
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[6].Style.BackColor = Color.LightPink;
                            }
                            continue;
                        }
                        if (DateTime.Compare(keyDate, test) < 0)
                        {
                            dataGridView1.Rows.Add(1);
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells["Data"].Value = keyDate;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells["Operation"].Value = "Order " +
                                Wallets.orderHistory[key].Exchange + " " + Wallets.orderHistory[key].OrderType;
                            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Value = Math.Round(Convert.ToDecimal(orderBalance, CultureInfo.InvariantCulture), 8);
                            if (orderBalance > 0)
                            {
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Style.BackColor = Color.LightGreen;
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[6].Style.BackColor = Color.LightGreen;
                            }
                            else
                            {
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Style.BackColor = Color.LightPink;
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[6].Style.BackColor = Color.LightPink;
                            }
                        }
                        else
                        {
                            for (int iter = 0; iter < dataGridView1.Rows.Count; iter++)
                            {
                                if (DateTime.Compare(keyDate,
                                    Convert.ToDateTime(Convert.ToString(dataGridView1.Rows[iter].Cells["Data"].Value, CultureInfo.InvariantCulture),
                            CultureInfo.InvariantCulture)) >= 0)
                                {
                                    dataGridView1.Rows.Insert(iter, 1);
                                    dataGridView1.Rows[iter].Cells["Data"].Value = keyDate;
                                    dataGridView1.Rows[iter].Cells["Operation"].Value = "Order " + Wallets.orderHistory[key].Exchange + " " + Wallets.orderHistory[key].OrderType;
                                    dataGridView1.Rows[iter].Cells[4].Value = Math.Round(Convert.ToDecimal(orderBalance, CultureInfo.InvariantCulture), 8);
                                    if (orderBalance > 0)
                                    {
                                        dataGridView1.Rows[iter].Cells[4].Style.BackColor = Color.LightGreen;
                                        dataGridView1.Rows[iter].Cells[6].Style.BackColor = Color.LightGreen;
                                    }
                                    else
                                    {
                                        dataGridView1.Rows[iter].Cells[4].Style.BackColor = Color.LightPink;
                                        dataGridView1.Rows[iter].Cells[6].Style.BackColor = Color.LightPink;
                                    }
                                    break;
                                }
                            }
                        }                 
                        //for (int itern = 0; itern < dataGridView1.Rows.Count; itern++)
                        //    testing[itern] = Convert.ToString(dataGridView1.Rows[itern].Cells[5].Value);
                    }
                }

                for (int iter = 0; iter < dataGridView1.Rows.Count; iter++)
                {
                    if (iter == 0)
                    {
                        dataGridView1.Rows[iter].Cells[5].Value = Math.Round(balance, 8);
                    }
                    else
                    {
                        dataGridView1.Rows[iter].Cells[5].Value = Math.Round(
                            Convert.ToDecimal(dataGridView1.Rows[iter - 1].Cells[5].Value, CultureInfo.InvariantCulture)
                            - Convert.ToDecimal(dataGridView1.Rows[iter - 1].Cells[4].Value, CultureInfo.InvariantCulture), 8);
                    }

                }
                //for (int itern = 0; itern < dataGridView1.Rows.Count; itern++)
                //    testing[itern] = Convert.ToString(dataGridView1.Rows[itern].Cells[5].Value);
               
                    for (int iter = 0; iter < dataGridView1.Rows.Count; iter++)
                    {
                         try
                         {

                             long temp = Wallets.ToUnixFromDateTime(Convert.ToDateTime(dataGridView1.Rows[iter].Cells["Data"].Value, CultureInfo.InvariantCulture));
                        double usdRate =  Wallets.getUSDrateDate(temp * 1000, currency);
                        dataGridView1.Rows[iter].Cells["balanceDol"].Value = Math.Round(Convert.ToDecimal(dataGridView1.Rows[iter].Cells[5].Value,
                            CultureInfo.InvariantCulture) * Convert.ToDecimal(usdRate, CultureInfo.InvariantCulture), 2);
                        dataGridView1.Rows[iter].Cells["amountDol"].Value = Math.Round(Convert.ToDecimal(dataGridView1.Rows[iter].Cells[4].Value,
                            CultureInfo.InvariantCulture) * Convert.ToDecimal(usdRate, CultureInfo.InvariantCulture), 2);
                         }
                         catch (Exception ex)
                         {
                             Log.logDebug("usdRate:" + Convert.ToString(ex));
                         }
                        
                    }


            }

        }
        //Парсинг LTC
        public  void parseLTCOperation(double balance)
        {
            IList<LTCBody> mainBodyAll =  Wallets.getLTCOperation(wallets);
            int id = 0;
            rowFullInfo = new TransactionInfo[mainBodyAll.Count];
            for (int iter = 0; iter < mainBodyAll.Count; iter++)
            {
                rowFullInfo[iter] = new TransactionInfo();
                rowFullInfo[iter].Out = new Dictionary<int, string>();
                rowFullInfo[iter].inputs = new Dictionary<int, string>();
                dataGridView1.Rows.Add(1);
                dataGridView1.Rows[id].Cells["ID"].Value = iter;
                dataGridView1.Rows[id].Cells["Operation"].Value = mainBodyAll[iter].txid;

                dataGridView1.Rows[id].Cells["Data"].Value = Wallets.ToDateTimeFromUnix(mainBodyAll[iter].time).ToUniversalTime();
                if (mainBodyAll[iter].state)
                {
                    dataGridView1.Rows[id].Cells[4].Value = Math.Round(Convert.ToDecimal(mainBodyAll[iter].value, CultureInfo.InvariantCulture), 8);
                    dataGridView1.Rows[id].Cells[4].Style.BackColor = Color.LightGreen;
                    dataGridView1.Rows[id].Cells[6].Style.BackColor = Color.LightGreen;
                    for (int iterBody = 0; iterBody < mainBodyAll[iter].inputs.Count; iterBody++)
                    {
                        rowFullInfo[iter].inputs.Add(iterBody, mainBodyAll[iter].inputs[iterBody]["address"] + "     Value=" + Convert.ToString(mainBodyAll[iter].value, CultureInfo.InvariantCulture));
                    }
                    rowFullInfo[iter].Out.Add(0, mainBodyAll[iter].from);

                }
                else
                {
                    dataGridView1.Rows[id].Cells[4].Value = Math.Round(Convert.ToDecimal(mainBodyAll[iter].value, CultureInfo.InvariantCulture), 8) * -1;
                    dataGridView1.Rows[id].Cells[4].Style.BackColor = Color.LightPink;
                    dataGridView1.Rows[id].Cells[6].Style.BackColor = Color.LightPink;
                    for (int iterBody = 0; iterBody < mainBodyAll[iter].outputs.Count; iterBody++)
                    {
                        rowFullInfo[iter].Out.Add(iterBody, mainBodyAll[iter].outputs[iterBody]["address"] + "     Value=" + Convert.ToString(mainBodyAll[iter].value, CultureInfo.InvariantCulture));
                    }
                    rowFullInfo[iter].inputs.Add(0, mainBodyAll[iter].from);


                }


                //dataGridView1.Rows[id].Cells["Fee"].Value = mainBodyAll[iter].fee;

                dataGridView1.Columns["ID"].Visible = false;

                //Считаем сумму out                                      
                //outBody = parseOutBody(wallet, outBody);
                id++;
            }           
            dataGridView1.Sort(dataGridView1.Columns["Data"], ListSortDirection.Descending);
            for (int iter = 0; iter < dataGridView1.Rows.Count; iter++)
            {
                if (iter == 0)
                {
                    dataGridView1.Rows[iter].Cells[5].Value = balance;
                }
                else
                {

                    dataGridView1.Rows[iter].Cells[5].Value = Math.Round(
                        Convert.ToDecimal(dataGridView1.Rows[iter - 1].Cells[5].Value, CultureInfo.InvariantCulture) -
                        Convert.ToDecimal(dataGridView1.Rows[iter - 1].Cells[4].Value, CultureInfo.InvariantCulture), 12);
                }
            }
                for (int iter = 0; iter < dataGridView1.Rows.Count; iter++)
                {

                    double usdRate =  Wallets.getUSDrateDate(
                        Wallets.ToUnixFromDateTime(Convert.ToDateTime(dataGridView1.Rows[iter].Cells["Data"].Value, CultureInfo.InvariantCulture))
                        * 1000, "LTC");
                    dataGridView1.Rows[iter].Cells[7].Value = Math.Round(Convert.ToDecimal(dataGridView1.Rows[iter].Cells[5].Value,
                        CultureInfo.InvariantCulture) * Convert.ToDecimal(usdRate, CultureInfo.InvariantCulture), 2);
                    dataGridView1.Rows[iter].Cells[6].Value = Math.Round(Convert.ToDecimal(dataGridView1.Rows[iter].Cells[4].Value,
                        CultureInfo.InvariantCulture) * Convert.ToDecimal(usdRate, CultureInfo.InvariantCulture), 2);
                }
        }
        //Парсинг ETH
        public  void parseETHOperation(double balance)
        {
                IList<ETHBody> mainBodyAll =  Wallets.getETHOperation(wallets);
                int id = 0;
                rowFullInfo = new TransactionInfo[mainBodyAll.Count];
                for (int iter = 0; iter < mainBodyAll.Count; iter++)
                {
                    rowFullInfo[iter] = new TransactionInfo();
                    rowFullInfo[iter].Out = new Dictionary<int, string>();
                    rowFullInfo[iter].inputs = new Dictionary<int, string>();
                    double temp = Math.Round(Convert.ToDouble((mainBodyAll[iter].gasUsed * mainBodyAll[iter].gasPrice) / 1000000000, CultureInfo.InvariantCulture), 12);
                    mainBodyAll[iter].fee = temp / 1000000000;
                    dataGridView1.Rows.Add(1);
                    dataGridView1.Rows[id].Cells["ID"].Value = iter;
                    dataGridView1.Rows[id].Cells["Operation"].Value = mainBodyAll[iter].hash;
                    dataGridView1.Rows[id].Cells["Data"].Value = Wallets.ToDateTimeFromUnix(mainBodyAll[iter].time).ToUniversalTime();
                    if (wallets.Contains(mainBodyAll[iter].to))
                    {
                        dataGridView1.Rows[id].Cells[4].Value = Math.Round(Convert.ToDecimal(mainBodyAll[iter].value, 
                            CultureInfo.InvariantCulture) / 1000000000000000000, 12);
                        dataGridView1.Rows[id].Cells[4].Style.BackColor = Color.LightGreen;
                        dataGridView1.Rows[id].Cells[6].Style.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        dataGridView1.Rows[id].Cells[4].Value = Math.Round(Convert.ToDecimal(mainBodyAll[iter].value,
                            CultureInfo.InvariantCulture) / 1000000000000000000, 12) * -1 - Convert.ToDecimal(mainBodyAll[iter].fee);
                        dataGridView1.Rows[id].Cells[4].Style.BackColor = Color.LightPink;
                        dataGridView1.Rows[id].Cells[6].Style.BackColor = Color.LightPink;
                    }


                    dataGridView1.Rows[id].Cells["Fee"].Value = mainBodyAll[iter].fee;

                    dataGridView1.Columns["ID"].Visible = false;
   
                    //Считаем сумму out
                    rowFullInfo[iter].Out.Add(0, mainBodyAll[iter].to + "     Value=" + Convert.ToString(mainBodyAll[iter].value, CultureInfo.InvariantCulture));

                    //outBody = parseOutBody(wallet, outBody);
                    rowFullInfo[iter].inputs.Add(0, mainBodyAll[iter].from + "     Value=" + Convert.ToString(mainBodyAll[iter].value, CultureInfo.InvariantCulture));                  
                    id++;
                }
                dataGridView1.Sort(dataGridView1.Columns["Data"], ListSortDirection.Descending);
                for (int iter = 0; iter < dataGridView1.Rows.Count; iter++)
                {
                    if (iter == 0)
                    {
                        dataGridView1.Rows[iter].Cells[5].Value = Math.Round(Convert.ToDecimal(balance, CultureInfo.InvariantCulture), 12);
                    }
                    else
                    {

                        dataGridView1.Rows[iter].Cells[5].Value = Math.Round(
                            Convert.ToDecimal(dataGridView1.Rows[iter - 1].Cells[5].Value, CultureInfo.InvariantCulture) -
                            Convert.ToDecimal(dataGridView1.Rows[iter - 1].Cells[4].Value, CultureInfo.InvariantCulture), 12);
                    }
                }
                    for (int iter = 0; iter < dataGridView1.Rows.Count; iter++)
                    {

                        double usdRate =  Wallets.getUSDrateDate(
                             Wallets.ToUnixFromDateTime((DateTime)dataGridView1.Rows[iter].Cells["Data"].Value)* 1000, "ETH");
                        dataGridView1.Rows[iter].Cells[7].Value = Math.Round(Convert.ToDecimal(dataGridView1.Rows[iter].Cells[5].Value,
                            CultureInfo.InvariantCulture) * Convert.ToDecimal(usdRate), 2);
                        dataGridView1.Rows[iter].Cells[6].Value = Math.Round(Convert.ToDecimal(dataGridView1.Rows[iter].Cells[4].Value,
                            CultureInfo.InvariantCulture) * Convert.ToDecimal(usdRate), 2);
                    }

        }
        //Парсинг одиночного кошелька
        public  void  parseBlockchainOperation(double balance)
        {
            IList<OperationAllData> mainBody =  Wallets.getBlockchainOperation(wallet);
            int id = 0;
            rowFullInfo = new TransactionInfo[mainBody.Count];
            for (int iter=0;iter<mainBody.Count;iter++)
            {
                    rowFullInfo[iter] = new TransactionInfo();
                    rowFullInfo[iter].Out = new Dictionary<int, string>();
                    rowFullInfo[iter].inputs = new Dictionary<int, string>();

                OperationInfo operationInfo = new OperationInfo();               
                IList<OperationOut> outBody = mainBody[iter].Out.Select(p => new OperationOut
                {
                    addr = (string)p["addr"],
                    value = (long)p["value"],
                     spent = (bool)p["spent"]
                }).ToList();
                //Считаем сумму out
                for (int iterBody = 0; iterBody < outBody.Count; iterBody++)
                {
                    rowFullInfo[iter].Out.Add(iterBody, outBody[iterBody].addr + "     Value=" + Convert.ToString(outBody[iterBody].value, CultureInfo.InvariantCulture));

                    operationInfo.outAllSum += outBody[iterBody].value;
                    if (outBody[iterBody].addr==wallet )
                        operationInfo.outThisWallet += outBody[iterBody].value;

                }
                //outBody = parseOutBody(wallet, outBody);
                IList<OperationInputBody> inputBodyArgs = mainBody[iter].inputs.Select(p => new OperationInputBody
                {
                    addr = (string)p["prev_out"]["addr"],
                    value = (long)p["prev_out"]["value"]
                }).ToList();
                //Считаем сумму input
                for (int iterBody = 0; iterBody < inputBodyArgs.Count ; iterBody++)
                {
                    rowFullInfo[iter].inputs.Add(iterBody, inputBodyArgs[iterBody].addr + "     Value=" + Convert.ToString(inputBodyArgs[iterBody].value, CultureInfo.InvariantCulture));
                    operationInfo.inputAllSum+= inputBodyArgs[iterBody].value;
                }
                //inputBodyArgs = parseInputBody(wallet, inputBodyArgs);
                dataGridView1.Rows.Add(1);
                dataGridView1.Rows[id].Cells["ID"].Value = iter;
                if (iter==0)
                {
                    dataGridView1.Rows[id].Cells[5].Value = Math.Round(Convert.ToDouble(balance, CultureInfo.InvariantCulture) / 100000000, 8);
                }
                else 
                {
                    dataGridView1.Rows[id].Cells[5].Value = Math.Round(Convert.ToDecimal(dataGridView1.Rows[id - 1].Cells[5].Value, CultureInfo.InvariantCulture)
                        - Convert.ToDecimal(dataGridView1.Rows[id - 1].Cells[4].Value, CultureInfo.InvariantCulture), 8);
                }
                dataGridView1.Rows[id].Cells["Fee"].Value = operationInfo.inputAllSum - operationInfo.outAllSum;
                dataGridView1.Rows[id].Cells["Operation"].Value = mainBody[iter].hash;
                dataGridView1.Rows[id].Cells["Operation"].ToolTipText="Sum ="+ mainBody[iter].result;
                dataGridView1.Rows[id].Cells["Data"].Value = Wallets.ToDateTimeFromUnix(mainBody[iter].time).ToUniversalTime();
                dataGridView1.Rows[id].Cells["Data"].ToolTipText= "Sum ="+mainBody[iter].result;
                if (findOperationDirection(wallet, outBody, inputBodyArgs))//True if wallet in out| False if Wallet in input
                {
                    dataGridView1.Rows[id].Cells[4].Value = Math.Round(Convert.ToDecimal(operationInfo.outThisWallet, CultureInfo.InvariantCulture) / 100000000, 8);
                    dataGridView1.Rows[id].Cells[4].Style.BackColor = Color.LightGreen;
                    dataGridView1.Rows[id].Cells[6].Style.BackColor = Color.LightGreen;

                }
                else
                {
                    for (int iterBody = 0; iterBody < outBody.Count; iterBody++)
                    {
                        if (outBody[iterBody ].spent)
                            operationInfo.outSpent += outBody[iterBody].value;
                    }
                    dataGridView1.Rows[id].Cells[4].Value = Math.Round(Convert.ToDecimal(operationInfo.outSpent - operationInfo.outAllSum 
                        + operationInfo.inputAllSum, CultureInfo.InvariantCulture) / 100000000, 8) * (-1);
                    dataGridView1.Rows[id].Cells[4].Style.BackColor = Color.LightPink;
                    dataGridView1.Rows[id].Cells[6].Style.BackColor = Color.LightPink;
                }
                dataGridView1.Columns["ID"].Visible = false;
                rowFullInfo[iter].size = mainBody[iter].size;
                id ++;
            }
                for (int iter = 0; iter < mainBody.Count; iter++)
                {
                    double usdRate =  Wallets.getUSDrateDate(mainBody[iter].time * 1000);
                    dataGridView1.Rows[iter].Cells[7].Value = Math.Round(Convert.ToDecimal(dataGridView1.Rows[iter].Cells[5].Value,
                        CultureInfo.InvariantCulture) * Convert.ToDecimal(usdRate, CultureInfo.InvariantCulture), 2);
                    dataGridView1.Rows[iter].Cells[6].Value = Math.Round(Convert.ToDecimal(dataGridView1.Rows[iter].Cells[4].Value,
                        CultureInfo.InvariantCulture) * Convert.ToDecimal(usdRate, CultureInfo.InvariantCulture), 2);
                }

        }
        //Парсинг всех холодных
        public  void parseAllBlockchainOperation()
        {

            IList<OperationAllData> mainBodyAll =   Wallets.getAllBlockchainOperation(wallets);
            int id = 0;
            rowFullInfo = new TransactionInfo[mainBodyAll.Count];
            for (int iter = 0; iter < mainBodyAll.Count; iter++)
            {
                rowFullInfo[iter] = new TransactionInfo();
                rowFullInfo[iter].Out = new Dictionary<int, string>();
                rowFullInfo[iter].inputs = new Dictionary<int, string>();
                
                dataGridView1.Rows.Add(1);
                dataGridView1.Rows[id].Cells["ID"].Value = iter;
                dataGridView1.Rows[id].Cells[5].Value = Math.Round(Convert.ToDouble(mainBodyAll[iter].balance, CultureInfo.InvariantCulture) / 100000000, 8);

                
                dataGridView1.Rows[id].Cells["Operation"].Value = mainBodyAll[iter].hash;
                dataGridView1.Rows[id].Cells["Operation"].ToolTipText = "Сумма =" + mainBodyAll[iter].result;
                dataGridView1.Rows[id].Cells["Data"].Value = Wallets.ToDateTimeFromUnix(mainBodyAll[iter].time).ToUniversalTime();
                dataGridView1.Rows[id].Cells["Data"].ToolTipText = "Сумма =" + mainBodyAll[iter].result;
                dataGridView1.Rows[id].Cells[4].Value = Math.Round(Convert.ToDecimal(mainBodyAll[iter].result, CultureInfo.InvariantCulture) / 100000000, 8);
                if (mainBodyAll[iter].result >= 0)
                {
                    dataGridView1.Rows[id].Cells[4].Style.BackColor = Color.LightGreen;
                    dataGridView1.Rows[id].Cells[6].Style.BackColor = Color.LightGreen;
                }
                else
                {
                    dataGridView1.Rows[id].Cells[4].Style.BackColor = Color.LightPink;
                    dataGridView1.Rows[id].Cells[6].Style.BackColor = Color.LightPink;
                }
                dataGridView1.Rows[id].Cells["Fee"].Value = mainBodyAll[iter].fee;

                dataGridView1.Columns["ID"].Visible = false;
                IList<OperationOut> outBody = mainBodyAll[iter].Out.Select(p => new OperationOut
                {
                    addr = (string)p["addr"],
                    value = (long)p["value"],
                    spent = (bool)p["spent"]
                }).ToList();
                //Считаем сумму out
                for (int iterBody = 0; iterBody < outBody.Count ; iterBody++)
                {
                    rowFullInfo[iter].Out.Add(iterBody, outBody[iterBody].addr + "     Value=" + Convert.ToString(outBody[iterBody].value, CultureInfo.InvariantCulture));


                }
                //outBody = parseOutBody(wallet, outBody);
                IList<OperationInputBody> inputBodyArgs = mainBodyAll[iter].inputs.Select(p => new OperationInputBody
                {
                    addr = (string)p["prev_out"]["addr"],
                    value = (long)p["prev_out"]["value"]
                }).ToList();
                //Считаем сумму input
                for (int iterBody = 0; iterBody < inputBodyArgs.Count ; iterBody++)
                {
                    rowFullInfo[iter].inputs.Add(iterBody, inputBodyArgs[iterBody].addr + "     Value=" + Convert.ToString(inputBodyArgs[iterBody].value, CultureInfo.InvariantCulture));
                }
                rowFullInfo[iter].size = mainBodyAll[iter].size;


                id++;
            }

                for (int iter = 0; iter < mainBodyAll.Count; iter++)
                {
                    double usdRate =  Wallets.getUSDrateDate(mainBodyAll[iter].time * 1000);
                    dataGridView1.Rows[iter].Cells[7].Value = Math.Round(Convert.ToDouble(dataGridView1.Rows[iter].Cells[5].Value, CultureInfo.InvariantCulture) * usdRate, 2);
                    dataGridView1.Rows[iter].Cells[6].Value = Math.Round(Convert.ToDouble(dataGridView1.Rows[iter].Cells[4].Value, CultureInfo.InvariantCulture) * usdRate, 2); ;
                }

        }

        //Парсинг NiceHash
        public  void parseNiceHashOperation(double balance)
        {
            IList<NiceHashBody> mainBodyAll =  Wallets.getNiceHashOperation(wallets);
            int id = 0;
            for (int iter = 0; iter < mainBodyAll.Count; iter++)
            {
                dataGridView1.Rows.Add(1);
                dataGridView1.Rows[id].Cells["ID"].Value = iter;
                dataGridView1.Rows[id].Cells["Operation"].Value = mainBodyAll[iter].txid;
                dataGridView1.Rows[id].Cells["Data"].Value = Wallets.ToDateTimeFromUnix(Convert.ToInt64(mainBodyAll[iter].time, CultureInfo.InvariantCulture)).ToLocalTime();
                dataGridView1.Rows[id].Cells[4].Value = Math.Round(Convert.ToDecimal(mainBodyAll[iter].value, CultureInfo.InvariantCulture), 8);
                dataGridView1.Rows[id].Cells[4].Style.BackColor = Color.LightGreen;
                dataGridView1.Rows[id].Cells[6].Style.BackColor = Color.LightGreen;
                //dataGridView1.Rows[id].Cells["Fee"].Value = mainBodyAll[iter].fee;

                dataGridView1.Columns["ID"].Visible = false;

                //Считаем сумму out                                      
                //outBody = parseOutBody(wallet, outBody);
                id++;
            }
            dataGridView1.Sort(dataGridView1.Columns["Data"], ListSortDirection.Descending);
            for (int iter = 0; iter < dataGridView1.Rows.Count; iter++)
            {
                if (iter == 0)
                {
                    dataGridView1.Rows[iter].Cells[5].Value = balance;
                }
                else
                {

                    dataGridView1.Rows[iter].Cells[5].Value = Math.Round(
                        Convert.ToDecimal(dataGridView1.Rows[iter - 1].Cells[5].Value, CultureInfo.InvariantCulture) -
                        Convert.ToDecimal(dataGridView1.Rows[iter - 1].Cells[4].Value, CultureInfo.InvariantCulture), 12);
                }
            }
                for (int iter = 0; iter < dataGridView1.Rows.Count; iter++)
                {

                    double usdRate =  Wallets.getUSDrateDate(
                        Wallets.ToUnixFromDateTime(Convert.ToDateTime(dataGridView1.Rows[iter].Cells["Data"].Value, CultureInfo.InvariantCulture))
                        * 1000, "BTC");
                    dataGridView1.Rows[iter].Cells[7].Value = Math.Round(Convert.ToDecimal(dataGridView1.Rows[iter].Cells[5].Value,
                        CultureInfo.InvariantCulture) * Convert.ToDecimal(usdRate, CultureInfo.InvariantCulture), 2);
                    dataGridView1.Rows[iter].Cells[6].Value = Math.Round(Convert.ToDecimal(dataGridView1.Rows[iter].Cells[4].Value,
                        CultureInfo.InvariantCulture) * Convert.ToDecimal(usdRate, CultureInfo.InvariantCulture), 2);
                }
        }


        //True if wallet in out| False if Wallet in input
        public bool findOperationDirection(string wallet,IList<OperationOut> outBody,IList<OperationInputBody> inputBody)
        {
            for (int iter = 0; iter < outBody.Count; iter++)
            {
                if (outBody[iter].addr == wallet)
                {
                    return true;
                }
            }
            for (int iter = 0; iter < inputBody.Count; iter++)
            {
                if (inputBody[iter].addr == wallet)
                {
                    return false;
                }
                
            }
            return false;
            
        }
        public long getSpentOut(string wallet,IList<OperationOut> outBody)
        {
            long result = 0;
            for (int iter = 0; iter < outBody.Count; iter++)
            {
                if (outBody[iter].addr == wallet)
                {
                    result += outBody[iter].value;
                }
            }
            return result;
        }
        public long getSpentInput(string wallet, IList<OperationInputBody> inputBody)
        {
            long result = 0;
            for (int iter = 0; iter < inputBody.Count; iter++)
            {
                if (inputBody[iter].addr == wallet)
                {
                    result += inputBody[iter].value;
                }
            }
            return result;
        }
        public IList<OperationOut> parseOutBody(string  wallet, IList<OperationOut> outBody)
        {
            bool walletThis=false;
            IList<OperationOut> result ;
            //Проверяем отправляем мы или получаем деньги
            for (int iter = 0; iter < outBody.Count; iter++)
            {
                if (outBody[iter].addr == wallet)
                {
                    walletThis = true;
                }
            }
            if (walletThis)
            {
                result = outBody.Where(t => t.addr == wallet).ToList();
            }
            else
            {
                result = outBody;
            }
            return result;
        }
        public IList<OperationInputBody> parseInputBody(string wallet, IList<OperationInputBody> inputBody)
        {
            bool walletThis=false;
            IList<OperationInputBody> result = new List<OperationInputBody>() ;
            //Проверяем отправляем мы или получаем деньги
            for (int iter = 0; iter < inputBody.Count; iter++)
            {
                if (inputBody[iter].addr == wallet)
                {
                    walletThis = true;
                }
            }
            if (walletThis)
            {
                Dictionary<String, long> sumArray = new Dictionary<String, long>();
                for (int iter = 0; iter < inputBody.Count; iter++)
                {
                    if (result.Count(t => t.addr == inputBody[iter].addr) !=0)
                    {
                        result.Single(t => t.addr == inputBody[iter].addr).value = 
                            result.Single(t => t.addr == inputBody[iter].addr).value + inputBody[iter].value;
                    }
                    else
                    {
                        result.Add(inputBody[iter]);
                    }
                }                
            }
            else
            {
                result = inputBody;
            }
            return result;
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && !dataGridView1.Rows[e.RowIndex].Cells["Operation"].Value.ToString().Contains("Order"))
            {
                if (wallet == "")
                {

                    OperationDetailForm detailForm = new OperationDetailForm(dataGridView1.Rows[e.RowIndex],
                        rowFullInfo[(int)dataGridView1.Rows[e.RowIndex].Cells["ID"].Value], wallets);
                    detailForm.Show();
                }
                else
                {
                    
                    OperationDetailForm detailForm = new OperationDetailForm(dataGridView1.Rows[e.RowIndex],
                        rowFullInfo[(int)dataGridView1.Rows[e.RowIndex].Cells["ID"].Value], wallet);
                    detailForm.Show();
                }
            }
            //dataGridView1.Rows[e.RowIndex].Cells[1].Value;
        }
    }
}
