using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Drawing;
//using BitcoinInfoMiner;
namespace BitcoinInfoMiner
{
    public partial class BTCBalanceForm : Form
    {

        
        public Dictionary<string, DataGridView> dataGridArray;
        public Dictionary<string, Label> labelArray;
        

 class DataRecord
    {
        //Should have properties which correspond to the Column Names in the file
        //i.e. CommonName,FormalName,TelephoneCode,CountryCode
        public String OrderUuid { get; set; }
        public String Exchange { get; set; }
        public String Type { get; set; }
        public String Quantity { get; set; }
             public String Limit { get; set; }
        public String CommissionPaid { get; set; }
        public String Price { get; set; }
        public String Opened { get; set; }
     public String Closed { get; set; }
    }


        MainWindow parent;


        public BTCBalanceForm(MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
            panel1.HorizontalScroll.Maximum = 0;
            panel1.AutoScroll = false;
            panel1.VerticalScroll.Visible = false;
            panel1.AutoScroll = true;
            Wallets.orderHistory = new Dictionary<int, OrderBody>();

            for (int iter = 0; iter < Wallets.walletInfo["Bittrex"].Count();iter+=2 )
                Wallets.parseOrderHistory(Wallets.walletInfo["Bittrex"][iter], Wallets.walletInfo["Bittrex"][iter+1]);
            
            createDataGrid();   
            
           
        }

        //Generate DataGrid for all Wallets
        public  void createDataGrid()
        {
            panel1.Controls.Clear();
            dataGridArray = new Dictionary<string, DataGridView>();
            labelArray = new Dictionary<string, Label>();
            int yCord=10;
            foreach (string key in Wallets.walletInfo.Keys)
            {
                switch(key)
                {
                    case "BTC cold wallet":
                        try
                        {
                            if (Wallets.walletInfo[key].Length > 0)
                            {
                                dataGridArray.Add(key, new DataGridView());
                                labelArray.Add(key, new Label());
                                labelArray[key] = new Label();
                                labelArray[key].Text = key;
                                labelArray[key].AutoSize = true;
                                labelArray[key].Font = new System.Drawing.Font("Arial", 12);

                                setDefaultSettingsDataGrid(key);
                                for (int iter = 0; iter < Wallets.walletInfo[key].Length; iter++)
                                {
                                    parseBlockchain( Wallets.sendAsyncRequest("q/addressbalance/" + Wallets.walletInfo[key][iter],
                                        Wallets.blockChainUri), iter, key, dataGridArray[key]);

                                }
                                foreach (DataGridViewColumn column in dataGridArray[key].Columns)
                                {
                                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                                }
                                labelArray[key].Location = new System.Drawing.Point(panel1.Width / 2 - labelArray[key].Width, yCord);
                                yCord += labelArray[key].Height + 10;
                                dataGridArray[key].Location = new System.Drawing.Point(0, yCord);
                                dataGridArray[key].Size = new System.Drawing.Size(panel1.Width,
                                    dataGridArray[key].Rows.GetRowsHeight(DataGridViewElementStates.None) + dataGridArray[key].ColumnHeadersHeight);
                                this.panel1.Controls.Add(labelArray[key]);
                                this.panel1.Controls.Add(dataGridArray[key]);
                                //this.Controls.Add(dataGridArray[key]);
                                yCord += dataGridArray[key].Height;

                                //dataGridArray[key].Show();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.logDebug("BTC\n\r" + Convert.ToString(ex));
                        }
                        break;
                    case "LTC":
                        try 
                        {
                            if (Wallets.walletInfo[key].Length > 0)
                            {
                                dataGridArray.Add(key, new DataGridView());
                                labelArray.Add(key, new Label());
                                labelArray[key] = new Label();
                                labelArray[key].Text = key;
                                labelArray[key].AutoSize = true;
                                labelArray[key].Font = new System.Drawing.Font("Arial", 12);

                                setDefaultSettingsDataGrid(key);
                                for (int iter = 0; iter < Wallets.walletInfo[key].Length; iter++)
                                {
                                    parseLTC( Wallets.sendAsyncRequest("address/LTC/" + Wallets.walletInfo[key][iter],
                                        Wallets.LTCUri), iter, key, dataGridArray[key]);
                                }
                                foreach (DataGridViewColumn column in dataGridArray[key].Columns)
                                {
                                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                                }
                                labelArray[key].Location = new System.Drawing.Point(panel1.Width / 2 - labelArray[key].Width, yCord);
                                yCord += labelArray[key].Height + 10;
                                dataGridArray[key].Location = new System.Drawing.Point(0, yCord);
                                dataGridArray[key].Size = new System.Drawing.Size(panel1.Width,
                                    dataGridArray[key].Rows.GetRowsHeight(DataGridViewElementStates.None) + dataGridArray[key].ColumnHeadersHeight);
                                this.panel1.Controls.Add(labelArray[key]);
                                this.panel1.Controls.Add(dataGridArray[key]);
                                //this.Controls.Add(dataGridArray[key]);
                                yCord += dataGridArray[key].Height;

                                //dataGridArray[key].Show();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.logDebug("LTC\n\r" + Convert.ToString(ex));
                        }
                        break;
                    case "NiceHash":
                        try
                        {
                            if (Wallets.walletInfo[key].Length > 0)
                            {
                                dataGridArray.Add(key, new DataGridView());
                                labelArray.Add(key, new Label());
                                labelArray[key] = new Label();
                                labelArray[key].Text = key;
                                labelArray[key].AutoSize = true;
                                labelArray[key].Font = new System.Drawing.Font("Arial", 12);

                                setDefaultSettingsDataGrid(key);
                                for (int iter = 0; iter < Wallets.walletInfo[key].Length; iter += 3)
                                {
                                    if (iter + 2 < Wallets.walletInfo[key].Length)
                                    {
                                        Dictionary<String, String> uriArgs = new Dictionary<string, string>();
                                        uriArgs.Add("method", "balance");
                                        uriArgs.Add("id", Wallets.walletInfo[key][iter + 1]);
                                        uriArgs.Add("key", Wallets.walletInfo[key][iter + 2]);
                                        parseNiceHash( Wallets.sendAsyncRequest("", Wallets.NiceHashUri, uriArgs),
                                            iter, key, dataGridArray[key]);
                                    }
                                }
                                foreach (DataGridViewColumn column in dataGridArray[key].Columns)
                                {
                                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                                }
                                labelArray[key].Location = new System.Drawing.Point(panel1.Width / 2 - labelArray[key].Width, yCord);
                                yCord += labelArray[key].Height + 10;
                                dataGridArray[key].Location = new System.Drawing.Point(0, yCord);
                                dataGridArray[key].Size = new System.Drawing.Size(panel1.Width,
                                    dataGridArray[key].Rows.GetRowsHeight(DataGridViewElementStates.None) + dataGridArray[key].ColumnHeadersHeight);
                                this.panel1.Controls.Add(labelArray[key]);
                                this.panel1.Controls.Add(dataGridArray[key]);
                                //this.Controls.Add(dataGridArray[key]);
                                yCord += dataGridArray[key].Height;

                                //dataGridArray[key].Show();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.logDebug("NiceHash\n\r" + Convert.ToString(ex));
                        }
                        break;
                    case "Bittrex":
                        try
                        {
                            if (Wallets.walletInfo[key].Length > 0)
                            {
                                dataGridArray.Add(key, new DataGridView());
                                labelArray.Add(key, new Label());
                                labelArray[key] = new Label();
                                labelArray[key].AutoSize = true;
                                labelArray[key].Text = key+"\r\n Double click on currency balance will open\r\n Detailed Transaction history";
                                labelArray[key].Font = new System.Drawing.Font("Arial", 12);
                                

                                setDefaultSettingsDataGrid(key);
                                for (int iter = 0; iter < Wallets.walletInfo[key].Length; iter += 2)
                                {
                                    if (iter + 1 < Wallets.walletInfo[key].Length)
                                        parseBittrex( Wallets.sendBittrexRequest("account/getbalances",
                                            Wallets.walletInfo[key][iter], Wallets.walletInfo[key][iter + 1]), iter, key, dataGridArray[key]);
                                }
                                foreach (DataGridViewColumn column in dataGridArray[key].Columns)
                                {
                                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                                }
                                labelArray[key].Location = new System.Drawing.Point(panel1.Width / 2 - labelArray[key].Width / 2, yCord);
                                yCord += labelArray[key].PreferredHeight + 5;

                                dataGridArray[key].Location = new System.Drawing.Point(0, yCord);
                                dataGridArray[key].Size = new System.Drawing.Size(panel1.Width,
                                    dataGridArray[key].Rows.GetRowsHeight(DataGridViewElementStates.None) + dataGridArray[key].ColumnHeadersHeight);
                                this.panel1.Controls.Add(labelArray[key]);
                                this.panel1.Controls.Add(dataGridArray[key]);
                                yCord += dataGridArray[key].Height;
                                //dataGridArray[key].Show();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.logDebug("Bittrex\n\r" + Convert.ToString(ex));
                        }
                        break;
                    case "ETH":
                        try
                        {
                            if (Wallets.walletInfo[key].Length > 0)
                            {
                                dataGridArray.Add(key, new DataGridView());
                                labelArray.Add(key, new Label());
                                labelArray[key] = new Label();
                                labelArray[key].Text = key;
                                labelArray[key].AutoSize = true;
                                labelArray[key].Font = new System.Drawing.Font("Arial", 12);

                                setDefaultSettingsDataGrid(key);
                                for (int iter = 0; iter < Wallets.walletInfo[key].Length; iter++)
                                {
                                    Dictionary<String, String> uriArgs = new Dictionary<string, string>();
                                    uriArgs.Add("module", "account");
                                    uriArgs.Add("action", "balance");
                                    uriArgs.Add("address", Wallets.walletInfo[key][iter]);
                                    parseETH( Wallets.sendAsyncRequest("", Wallets.ethUri, uriArgs), iter, key, dataGridArray[key]);
                                }
                                foreach (DataGridViewColumn column in dataGridArray[key].Columns)
                                {
                                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                                }
                                labelArray[key].Location = new System.Drawing.Point(panel1.Width / 2 - labelArray[key].Width, yCord);
                                yCord += labelArray[key].Height + 10;
                                dataGridArray[key].Location = new System.Drawing.Point(0, yCord);
                                dataGridArray[key].Size = new System.Drawing.Size(panel1.Width,
                                    dataGridArray[key].Rows.GetRowsHeight(DataGridViewElementStates.None) + dataGridArray[key].ColumnHeadersHeight);
                                this.panel1.Controls.Add(labelArray[key]);
                                this.panel1.Controls.Add(dataGridArray[key]);
                                //this.Controls.Add(dataGridArray[key]);
                                yCord += dataGridArray[key].Height;

                                //dataGridArray[key].Show();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.logDebug("ETH\n\r" + Convert.ToString(ex));
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        //Default setting of generated Grids
        private void setDefaultSettingsDataGrid(string key)
        {
            dataGridArray[key].AllowUserToAddRows = false;
            dataGridArray[key].ScrollBars = ScrollBars.Horizontal;
            dataGridArray[key].AllowUserToDeleteRows = false;
            dataGridArray[key].AllowUserToResizeColumns = true;
            dataGridArray[key].AllowUserToResizeRows = false;
            dataGridArray[key].EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridArray[key].Name = key;
            dataGridArray[key].CellDoubleClick += new DataGridViewCellEventHandler(doubleClickOperations);
            dataGridArray[key].CellClick += new DataGridViewCellEventHandler(clickOperations);
            dataGridArray[key].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            labelArray[key].TextAlign = ContentAlignment.MiddleCenter;
            //dataGridArray[key].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        //Parse bittrex responce and fill grid
        private void parseBittrex(string json, int walletID, string walletKey, DataGridView grid)
        {
            if (json != "False")
            {
                var bittrexResponse = JsonConvert.DeserializeObject<BittrexResponse>(json);

                if (!grid.Columns.Contains("Wallet"))//Проверяем пустая ли таблица
                {       
                    //Создаем заголовки
                    grid.Columns.Add("Wallet", "Wallet");
                    grid.Columns.Add("Description", "Description");
                    grid.Columns["Wallet"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    grid.Rows.Add(4);
                    grid.Rows[0].Cells["Wallet"].Value = Wallets.walletInfo[walletKey][walletID];
                    grid.Rows[0].Cells["Description"].Value = Wallets.walletInfoDescription[walletKey][walletID];
                    for (int iter = 0; iter < bittrexResponse.Result.Count(); iter++)
                    {
                        if (bittrexResponse.Result[iter].First.Next.First.ToString() != "0")
                        {
                            grid.Columns.Add(bittrexResponse.Result[iter].First.First.ToString(),
                                bittrexResponse.Result[iter].First.First.ToString());
                            grid.Rows[0].Cells[bittrexResponse.Result[iter].First.First.ToString()].Value = bittrexResponse.Result[iter].First.Next.First.ToString();
                        }
                        //temp[iter] = bittrexResponse.Result[iter].First.Next.ToString();
                    }
                    for (int iter = 0; iter < bittrexResponse.Result.Count(); iter++)
                    {
                        if (bittrexResponse.Result[iter].First.Next.First.ToString() == "0")
                        {
                            grid.Columns.Add(bittrexResponse.Result[iter].First.First.ToString(),
                                bittrexResponse.Result[iter].First.First.ToString());
                            grid.Rows[0].Cells[bittrexResponse.Result[iter].First.First.ToString()].Value = bittrexResponse.Result[iter].First.Next.First.ToString();
                        }
                        //temp[iter] = bittrexResponse.Result[iter].First.Next.ToString();
                    }
                    //Три последние строчки являются строками сумм
                    DataGridViewButtonColumn checkColumn = new DataGridViewButtonColumn();
                    checkColumn.Name = "Button";
                    checkColumn.HeaderText = "Details";
                    //checkColumn.ToolTipText = "Report on very high or very low Temp, or Low hashrate";
                    checkColumn.Width = 200;
                    checkColumn.MinimumWidth = 100;
                    checkColumn.ReadOnly = true;
                    checkColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
                    grid.Columns.Add(checkColumn);
                    grid.Rows[grid.Rows.Count - 4].Cells["Button"].Value = "Order History";
                    grid.Rows[grid.Rows.Count - 3].Cells["Wallet"].Value = "Sum";
                    grid.Rows[grid.Rows.Count - 2].Cells["Wallet"].Value = "Sum in $";
                    grid.Rows[grid.Rows.Count - 1].Cells["Wallet"].Value = "Sum in BTC";
                }
                else
                {
                    //Если не пустая вставляем и заполняем новую строчку
                    grid.Rows.Insert(grid.Rows.Count - 3, 1);
                    grid.Rows[grid.Rows.Count - 3].Cells["Wallet"].Value = Wallets.walletInfo[walletKey][walletID];
                    grid.Rows[grid.Rows.Count - 3].Cells["Description"].Value = Wallets.walletInfoDescription[walletKey][walletID];
                    grid.Rows[grid.Rows.Count - 3].Cells["Button"].Value = "Order History";
                    for (int iter = 0; iter < bittrexResponse.Result.Count(); iter++)
                    {
                        if (!grid.Columns.Contains(bittrexResponse.Result[iter].First.First.ToString()))
                        {
                            grid.Columns.Add(bittrexResponse.Result[iter].First.First.ToString(),
                                bittrexResponse.Result[iter].First.First.ToString());
                        }
                        grid.Rows[grid.Rows.Count-4].Cells[bittrexResponse.Result[iter].First.First.ToString()].Value = bittrexResponse.Result[iter].First.Next.First.ToString();
                        //temp[iter] = bittrexResponse.Result[iter].First.Next.ToString();
                    }
                }
                //Считаем сумму столбцов и переводим их в $ и BTC. Заполняем соответствующие строки сумм
                for (int iterMain = 2; iterMain < grid.Columns.Count-1; iterMain++)
                {
                    double tempNum = 0;
                    for (int iter = 0; iter < grid.Rows.Count - 3; iter++)
                    {
                        tempNum += Convert.ToDouble(grid.Rows[iter].Cells[iterMain].Value, CultureInfo.InvariantCulture);
                    }
                    grid.Rows[grid.Rows.Count-3].Cells[iterMain].Value =Math.Round(tempNum,8);
                    if (Wallets.marketInfo.Keys.Contains(grid.Columns[iterMain].HeaderText))
                    {

                        grid.Rows[grid.Rows.Count - 2].Cells[iterMain].Value = Math.Round(tempNum * Wallets.marketInfo[grid.Columns[iterMain].HeaderText],2);
                        grid.Rows[grid.Rows.Count - 1].Cells[iterMain].Value = Math.Round(Convert.ToDecimal(grid.Rows[grid.Rows.Count - 2].Cells[iterMain].Value) / Convert.ToDecimal(Wallets.marketInfo["BTC"]), 8);
                    }
                    else
                    {
                        if (grid.Columns[iterMain].HeaderText == "BCC")
                        {
                            grid.Rows[grid.Rows.Count - 2].Cells[iterMain].Value = Math.Round(tempNum * Wallets.marketInfo["BCH"], 2);
                            grid.Rows[grid.Rows.Count - 1].Cells[iterMain].Value = Math.Round(Convert.ToDecimal(grid.Rows[grid.Rows.Count - 2].Cells[iterMain].Value) / Convert.ToDecimal(Wallets.marketInfo["BTC"]), 8);
                        }
                        else
                        {
                            grid.Rows[grid.Rows.Count - 2].Cells[iterMain].Value = "No data";
                            grid.Rows[grid.Rows.Count - 1].Cells[iterMain].Value = "No data";
                        }
                    }
                }
                //Считаем общую сумму сумм в тултип  .
                double columnSum=0;
                for (int iter = 2; iter < grid.Columns.Count-1; iter++)
                {
                    if (Convert.ToString(grid.Rows[grid.Rows.Count - 2].Cells[iter].Value, CultureInfo.InvariantCulture) != "No data")
                        columnSum += Convert.ToDouble(grid.Rows[grid.Rows.Count - 2].Cells[iter].Value, CultureInfo.InvariantCulture);
                }
                grid.Rows[grid.Rows.Count - 2].Cells[1].ToolTipText = columnSum.ToString();
                columnSum = 0;
                for (int iter = 2; iter < grid.Columns.Count; iter++)
                {
                    if (Convert.ToString(grid.Rows[grid.Rows.Count - 1].Cells[iter].Value, CultureInfo.InvariantCulture) != "No data")
                        columnSum += Convert.ToDouble(grid.Rows[grid.Rows.Count - 1].Cells[iter].Value, CultureInfo.InvariantCulture);
                }
                grid.Rows[grid.Rows.Count - 1].Cells[1].ToolTipText = columnSum.ToString();

            }
        }
        //Parse Blickchain responce and fill grid
        private void parseBlockchain(string balance,int walletID,string walletKey, DataGridView grid)
        {
            if (balance != "False")
            {

                if (!grid.Columns.Contains("Wallet"))//Проверяем пустая ли таблица
                {
                    //Создаем заголовки
                    
                    grid.Columns.Add("Wallet", "Wallet");
                    grid.Columns["Wallet"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    grid.Columns.Add("Description", "Description");
                    grid.Columns.Add("Balance BTC", "Balance BTC");
                    grid.Columns.Add("Balance $", "Balance $");
                    DataGridViewButtonColumn checkColumn = new DataGridViewButtonColumn();
                    checkColumn.Name = "Button";
                    checkColumn.HeaderText = "Details";
                    //checkColumn.ToolTipText = "Report on very high or very low Temp, or Low hashrate";                    
                    checkColumn.Width = 200;
                    checkColumn.ReadOnly = true;
                    checkColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
                    grid.Columns.Add(checkColumn);
                    grid.Rows.Add(2);
                    //Три последние строчки являются строками сумм
                    grid.Rows[0].Cells["Wallet"].Value =Wallets.walletInfo[walletKey][walletID] ;
                    grid.Rows[0].Cells["Description"].Value = Wallets.walletInfoDescription[walletKey][walletID];
                    grid.Rows[0].Cells["Balance BTC"].Value = Convert.ToDouble(balance, CultureInfo.InvariantCulture) / 100000000;
                    grid.Rows[0].Cells["Balance $"].Value =Math.Round( Wallets.marketInfo["BTC"] * 
                        Convert.ToDouble(balance, CultureInfo.InvariantCulture) / 100000000,2);
                    grid.Rows[0].Cells["Button"].Value = "Transaction History";
                    grid.Rows[grid.Rows.Count - 1].Cells["Wallet"].Value = "Sum";
                    grid.Rows[grid.Rows.Count - 1].Cells["Button"].Value = "Transaction History of all Wallets";
                }
                else
                {
                    //Если не пустая вставляем и заполняем новую строчку
                    grid.Rows.Insert(grid.Rows.Count - 1, 1);
                    //Три последние строчки являются строками сумм
                    grid.Rows[grid.Rows.Count - 2].Cells["Wallet"].Value = Wallets.walletInfo[walletKey][walletID];
                    grid.Rows[grid.Rows.Count - 2].Cells["Description"].Value = Wallets.walletInfoDescription[walletKey][walletID];
                    grid.Rows[grid.Rows.Count - 2].Cells["Balance BTC"].Value = Convert.ToDouble(balance, CultureInfo.InvariantCulture) / 100000000;
                    grid.Rows[grid.Rows.Count - 2].Cells["Button"].Value = "Transaction History";
                    grid.Rows[grid.Rows.Count - 2].Cells["Balance $"].Value = Math.Round(Wallets.marketInfo["BTC"] 
                        * Convert.ToDouble(balance, CultureInfo.InvariantCulture) / 100000000,2);//temp[iter] = bittrexResponse.Result[iter].First.Next.ToString();
                }
                
        
                //Считаем сумму столбцов и переводим их в $ и BTC. Заполняем соответствующие строки сумм
                for (int iterMain = 2; iterMain < grid.Columns.Count-1; iterMain++)
                {
                    double tempNum = 0;
                    for (int iter = 0; iter < grid.Rows.Count - 1; iter++)
                    {
                        tempNum +=Convert.ToDouble(grid.Rows[iter].Cells[iterMain].Value, CultureInfo.InvariantCulture);
                    }
                    grid.Rows[grid.Rows.Count - 1].Cells[iterMain].Value = Math.Round(tempNum,8);
                }
            }
        }
        //Parse ETH responce and fill grid
        private void parseETH(string balance,int walletID,string walletKey, DataGridView grid)
        {
            if (balance != "False")
            {
                JToken parsedBalance = JsonConvert.DeserializeObject<JToken>(balance);
                balance = parsedBalance.Last.Last.ToString();
                if (!grid.Columns.Contains("Wallet"))//Проверяем пустая ли таблица
                {
                    //Создаем заголовки
                    grid.Columns.Add("Wallet", "Wallet");
                    grid.Columns["Wallet"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    //grid.Columns["Wallet"].MinimumWidth = 150;
                    grid.Columns.Add("Description", "Description");
                    grid.Columns.Add("Balance ETH", "Balance ETH");
                    grid.Columns.Add("Balance $", "Balance $");
                    grid.Columns.Add("Balance BTC", "Balance BTC");
                    DataGridViewButtonColumn checkColumn = new DataGridViewButtonColumn();
                    checkColumn.Name = "Button";
                    checkColumn.HeaderText = "Details";
                    //checkColumn.ToolTipText = "Report on very high or very low Temp, or Low hashrate";
                    checkColumn.Width = 200;
                    checkColumn.ReadOnly = true;
                    checkColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                    grid.Columns.Add(checkColumn);
                    grid.Rows.Add(2);
                    //Три последние строчки являются строками сумм
                    grid.Rows[0].Cells["Wallet"].Value = Wallets.walletInfo[walletKey][walletID];
                    grid.Rows[0].Cells["Description"].Value = Wallets.walletInfoDescription[walletKey][walletID];
                    grid.Rows[0].Cells["Balance ETH"].Value = Math.Round(Convert.ToDecimal(balance, CultureInfo.InvariantCulture) / 1000000000000000000,12);
                    grid.Rows[0].Cells["Balance $"].Value = Math.Round(Wallets.marketInfo["ETH"] * 
                        Convert.ToDouble(balance, CultureInfo.InvariantCulture) / 1000000000000000000,2);

                    grid.Rows[0].Cells["Balance BTC"].Value = Math.Round(Convert.ToDecimal(Wallets.marketInfo["ETH"]) *
        Convert.ToDecimal(balance, CultureInfo.InvariantCulture) / Convert.ToDecimal(Wallets.marketInfo["BTC"] * 1000000000000000000), 8);
                    grid.Rows[0].Cells["Button"].Value = "Transaction History";
                        grid.Rows[grid.Rows.Count - 1].Cells["Wallet"].Value = "Sum";


                }
                else
                {
                    //Если не пустая вставляем и заполняем новую строчку
                    grid.Rows.Insert(grid.Rows.Count - 1, 1);
                    //Три последние строчки являются строками сумм
                    grid.Rows[grid.Rows.Count - 2].Cells["Wallet"].Value = Wallets.walletInfo[walletKey][walletID];
                    grid.Rows[grid.Rows.Count - 2].Cells["Description"].Value = Wallets.walletInfoDescription[walletKey][walletID];
                    grid.Rows[grid.Rows.Count - 2].Cells["Balance ETH"].Value =Math.Round( Convert.ToDecimal(balance, CultureInfo.InvariantCulture) / 1000000000000000000,12);
                    grid.Rows[grid.Rows.Count - 2].Cells["Balance $"].Value = Math.Round(Wallets.marketInfo["ETH"] * Convert.ToDouble(balance, CultureInfo.InvariantCulture) / 1000000000000000000,2);//temp[iter] = bittrexResponse.Result[iter].First.Next.ToString();
                    grid.Rows[grid.Rows.Count - 2].Cells["Balance BTC"].Value = Math.Round(Convert.ToDecimal(Wallets.marketInfo["ETH"]) *
        Convert.ToDecimal(balance, CultureInfo.InvariantCulture) / Convert.ToDecimal(Wallets.marketInfo["BTC"] * 1000000000000000000), 8);
                    grid.Rows[grid.Rows.Count - 2].Cells["Button"].Value = "Transaction History";
                }


                //Считаем сумму столбцов и переводим их в $ и BTC. Заполняем соответствующие строки сумм
                for (int iterMain = 2; iterMain < grid.Columns.Count-1; iterMain++)
                {
                    double tempNum = 0;
                    for (int iter = 0; iter < grid.Rows.Count - 1; iter++)
                    {
                        tempNum += Convert.ToDouble(grid.Rows[iter].Cells[iterMain].Value, CultureInfo.InvariantCulture);
                    }
                    grid.Rows[grid.Rows.Count - 1].Cells[iterMain].Value = Math.Round(Convert.ToDecimal(tempNum),8);
                }
            }
        }
        //Parse LTC responce and fill grid
        private void parseLTC(string balance, int walletID,string walletKey, DataGridView grid)
        {
            if (balance != "False")
            {
                JToken parsedBalance = JsonConvert.DeserializeObject<JToken>(balance);
                balance = parsedBalance.First.Next.Last.First.Next.Next.Last.ToString();
                if (!grid.Columns.Contains("Wallet"))//Проверяем пустая ли таблица
                {
                    //Создаем заголовки
                    grid.Columns.Add("Wallet", "Wallet");
                    grid.Columns["Wallet"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    grid.Columns.Add("Description", "Description");
                    grid.Columns.Add("Balance LTC", "Balance LTC");
                    grid.Columns.Add("Balance $", "Balance $");
                    grid.Columns.Add("Balance BTC", "Balance BTC");
                    DataGridViewButtonColumn checkColumn = new DataGridViewButtonColumn();
                    checkColumn.Name = "Button";
                    checkColumn.HeaderText = "Details";
                    //checkColumn.ToolTipText = "Report on very high or very low Temp, or Low hashrate";
                    checkColumn.Width = 200;
                    checkColumn.ReadOnly = true;
                    checkColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
                   
                    grid.Columns.Add(checkColumn);
                    grid.Rows.Add(2);
                    //Три последние строчки являются строками сумм
                    grid.Rows[0].Cells["Wallet"].Value = Wallets.walletInfo[walletKey][walletID];
                    grid.Rows[0].Cells["Description"].Value = Wallets.walletInfoDescription[walletKey][walletID];
                    grid.Rows[0].Cells["Balance LTC"].Value = Convert.ToDouble(balance, CultureInfo.InvariantCulture) ;
                    grid.Rows[0].Cells["Balance $"].Value = Math.Round(Wallets.marketInfo["LTC"] * Convert.ToDouble(balance, CultureInfo.InvariantCulture),2);
                    grid.Rows[0].Cells["Balance BTC"].Value = Math.Round(Wallets.marketInfo["LTC"] * Convert.ToDouble(balance, CultureInfo.InvariantCulture)
                        / Wallets.marketInfo["BTC"], 8);
                    ((DataGridViewButtonCell)grid.Rows[0].Cells["Button"]).Value = "Transaction History";
                    grid.Rows[grid.Rows.Count - 1].Cells["Wallet"].Value = "Sum";

                }
                else
                {
                    //Если не пустая вставляем и заполняем новую строчку
                    grid.Rows.Insert(grid.Rows.Count - 1, 1);
                    //Три последние строчки являются строками сумм
                    grid.Rows[grid.Rows.Count - 2].Cells["Wallet"].Value = Wallets.walletInfo[walletKey][walletID];
                    grid.Rows[grid.Rows.Count - 2].Cells["Description"].Value = Wallets.walletInfoDescription[walletKey][walletID];
                    grid.Rows[grid.Rows.Count - 2].Cells["Balance LTC"].Value = Convert.ToDouble(balance, CultureInfo.InvariantCulture) ;
                    grid.Rows[grid.Rows.Count - 2].Cells["Balance $"].Value = Math.Round(Wallets.marketInfo["LTC"] * Convert.ToDouble(balance, CultureInfo.InvariantCulture),2);//temp[iter] = bittrexResponse.Result[iter].First.Next.ToString();
                    grid.Rows[grid.Rows.Count - 2].Cells["Balance BTC"].Value = Math.Round(Wallets.marketInfo["LTC"] *
                        Convert.ToDouble(balance, CultureInfo.InvariantCulture) / Wallets.marketInfo["BTC"], 8);
                    ((DataGridViewButtonCell)grid.Rows[grid.Rows.Count - 2].Cells["Button"]).Value = "Transaction History";
                }


                //Считаем сумму столбцов и переводим их в $ и BTC. Заполняем соответствующие строки сумм
                for (int iterMain = 2; iterMain < grid.Columns.Count-1; iterMain++)
                {
                    double tempNum = 0;
                    for (int iter = 0; iter < grid.Rows.Count - 1; iter++)
                    {
                        tempNum += Convert.ToDouble(grid.Rows[iter].Cells[iterMain].Value, CultureInfo.InvariantCulture);
                    }
                    grid.Rows[grid.Rows.Count - 1].Cells[iterMain].Value = Math.Round(tempNum,8);
                }
            }
        }



        private void parseNiceHash(string balance, int walletID, string walletKey, DataGridView grid)
        {
            if (balance != "False")
            {
                JToken parsedBalance = JsonConvert.DeserializeObject<JToken>(balance);
                balance = parsedBalance.First.First.Last.Last.ToString();
                if (!grid.Columns.Contains("Wallet"))//Проверяем пустая ли таблица
                {
                    //Создаем заголовки
                    grid.Columns.Add("Wallet", "Wallet");
                    grid.Columns["Wallet"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    grid.Columns.Add("Description", "Description");
                    grid.Columns.Add("Balance BTC", "Balance BTC");
                    grid.Columns.Add("Balance $", "Balance $");
                    DataGridViewButtonColumn checkColumn = new DataGridViewButtonColumn();
                    checkColumn.Name = "Button";
                    checkColumn.HeaderText = "Details";
                    //checkColumn.ToolTipText = "Report on very high or very low Temp, or Low hashrate";
                    checkColumn.Width = 200;
                    checkColumn.ReadOnly = true;
                    checkColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
                    grid.Columns.Add(checkColumn);
                    grid.Rows.Add(2);
                    //Три последние строчки являются строками сумм
                    grid.Rows[0].Cells["Wallet"].Value = Wallets.walletInfo[walletKey][walletID];
                    grid.Rows[0].Cells["Description"].Value = Wallets.walletInfoDescription[walletKey][walletID];
                    grid.Rows[0].Cells["Balance BTC"].Value = Convert.ToDouble(balance, CultureInfo.InvariantCulture);
                    grid.Rows[0].Cells["Balance $"].Value = Math.Round(Wallets.marketInfo["BTC"] * Convert.ToDouble(balance, CultureInfo.InvariantCulture),2);
                    grid.Rows[grid.Rows.Count - 1].Cells["Wallet"].Value = "Sum";
                    ((DataGridViewButtonCell)grid.Rows[0].Cells["Button"]).Value = "Payment History";

                }
                else
                {
                    //Если не пустая вставляем и заполняем новую строчку
                    grid.Rows.Insert(grid.Rows.Count - 1, 1);
                    //Три последние строчки являются строками сумм
                    grid.Rows[grid.Rows.Count - 2].Cells["Wallet"].Value = Wallets.walletInfo[walletKey][walletID];
                    grid.Rows[grid.Rows.Count - 2].Cells["Description"].Value = Wallets.walletInfoDescription[walletKey][walletID];
                    grid.Rows[grid.Rows.Count - 2].Cells["Balance BTC"].Value = Convert.ToDouble(balance, CultureInfo.InvariantCulture);
                    grid.Rows[grid.Rows.Count - 2].Cells["Balance $"].Value = Math.Round(Wallets.marketInfo["BTC"] * Convert.ToDouble(balance, CultureInfo.InvariantCulture),2);//temp[iter] = bittrexResponse.Result[iter].First.Next.ToString();
                    ((DataGridViewButtonCell)grid.Rows[grid.Rows.Count - 2].Cells["Button"]).Value = "Payment History";
                }


                //Считаем сумму столбцов и переводим их в $ и BTC. Заполняем соответствующие строки сумм
                for (int iterMain = 2; iterMain < grid.Columns.Count-1; iterMain++)
                {
                    double tempNum = 0;
                    for (int iter = 0; iter < grid.Rows.Count - 1; iter++)
                    {
                        tempNum += Convert.ToDouble(grid.Rows[iter].Cells[iterMain].Value, CultureInfo.InvariantCulture);
                    }
                    grid.Rows[grid.Rows.Count - 1].Cells[iterMain].Value = Math.Round(tempNum,8);
                }
            }
        }
        //Get rates for today

        //Get UsdToCoin rate for unixDate


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private  void button2_Click(object sender, EventArgs e)
        {
            IDictionary<string, string> parameter = new Dictionary<string, string>();
            //textBox1.Text = await sendBittrexRequest("account/getwithdrawalhistory", apiBitTrexKey, apiBitTrexSecret);
            // temop = await sendBittrexRequest("account/getorderhistory", apiBitTrexKey, apiBitTrexSecret);
            //parseBlockchain(await sendAsyncRequest("q/addressbalance/" + "16ULEgXuB4PNhbJ38Vk7wQStJY222u3Zxn", blockChainUri),
            //    "16ULEgXuB4PNhbJ38Vk7wQStJY222u3Zxn", dataGridView1);
            //temop+="";
        }
        //Settings Button
        private void button4_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            BTCSettingsForm setting = new BTCSettingsForm(this);
            setting.Show();
        }

        //On Form closing
        private void BTCBalanceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            parent.Visible = true;

            Wallets.saveHistoricalInFile();
        }
        public void setNewDescription(string wallet,DataGridViewCell cell)
        {
            InputDialogForm testDialog = new InputDialogForm(this,
        "Write new description for \r\n" + wallet + "  Wallet",
        "Description editing");

            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if (testDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Read the contents of testDialog's TextBox.
                cell.Value = testDialog.textBoxResult.Text;
            }
            testDialog.Dispose();
        }
        //On  click in Grids
        public void clickOperations(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView temp = (DataGridView)sender;
            switch (temp.Name)
            {
                case "BTC cold wallet":
                    if (e.ColumnIndex == temp.ColumnCount - 1 && e.RowIndex >= 0 && e.RowIndex < (temp.Rows.Count - 1))
                    {
                        openOperationnsBlockchain(temp.Rows[e.RowIndex]);
                    }
                    else
                    {
                        if (e.ColumnIndex == temp.ColumnCount - 1 && e.RowIndex >= temp.Rows.Count - 1)
                        {
                            openAllOperationnsBlockchain(temp.Rows[e.RowIndex]);
                        }
                    }
                    break;
                case "Bittrex":
                    if (e.ColumnIndex == temp.ColumnCount - 1 && e.RowIndex >= 0 && e.RowIndex < (temp.Rows.Count - 3))
                    {
                        openOrderBittrex(temp.Rows[e.RowIndex]);
                    }
                    break;
                case "LTC":
                    if (e.ColumnIndex == temp.ColumnCount - 1 && e.RowIndex >= 0 && e.RowIndex < (temp.Rows.Count - 1))
                    {
                        openOperationnsLTC(temp.Rows[e.RowIndex]);
                    }
                    break;
                case "NiceHash":
                    if (e.ColumnIndex == temp.ColumnCount - 1 && e.RowIndex >= 0 && e.RowIndex < (temp.Rows.Count - 1))
                    {
                        openOperationnsNiceHash(temp.Rows[e.RowIndex]);
                    }
                    break;
                case "ETH":
                    if (e.ColumnIndex == temp.ColumnCount - 1 && e.RowIndex >= 0 && e.RowIndex < (temp.Rows.Count - 1))
                    {
                        openOperationnsETH(temp.Rows[e.RowIndex]);
                    }
                    break;
                default:
                    break;
            }
        }
        //On Double clich in Grids
        public void doubleClickOperations(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView temp = (DataGridView)sender;
            switch (temp.Name)
            {
                case "BTC cold wallet":
                    if (e.ColumnIndex == 1 &&  e.RowIndex >= 0 && e.RowIndex < (temp.Rows.Count - 1))
                    {
                        setNewDescription(temp.Rows[e.RowIndex].Cells[0].Value.ToString(), temp.Rows[e.RowIndex].Cells[e.ColumnIndex]);
                        Wallets.walletInfoDescription[temp.Name][e.RowIndex] = temp.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                    }                
                    break;
                case "Bittrex":
                    if (e.ColumnIndex > 1 && e.ColumnIndex < temp.ColumnCount - 1 && e.RowIndex >= 0 && e.RowIndex < (temp.Rows.Count - 3))
                    {
                        openOperationnsBittrex(temp.Rows[e.RowIndex], temp.Columns[e.ColumnIndex].HeaderText);
                    }
                    else
                    {
                        if (e.ColumnIndex == 1 && e.RowIndex >= 0 && e.RowIndex < (temp.Rows.Count - 3))
                        {
                            setNewDescription(temp.Rows[e.RowIndex].Cells[0].Value.ToString(), temp.Rows[e.RowIndex].Cells[e.ColumnIndex]);
                            Wallets.walletInfoDescription[temp.Name][Convert.ToInt32(e.RowIndex / 2)] = temp.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                        }
                    }
                   
                    break;
                case "LTC":
                    if (e.ColumnIndex == 1 && e.RowIndex >= 0 && e.RowIndex < (temp.Rows.Count - 1))
                    {
                        setNewDescription(temp.Rows[e.RowIndex].Cells[0].Value.ToString(), temp.Rows[e.RowIndex].Cells[e.ColumnIndex]);
                        Wallets.walletInfoDescription[temp.Name][e.RowIndex] = temp.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    }

                    break;
                case "NiceHash":
                    if (e.ColumnIndex == 1 && e.RowIndex >= 0 && e.RowIndex < (temp.Rows.Count - 1))
                    {
                        setNewDescription(temp.Rows[e.RowIndex].Cells[0].Value.ToString(), temp.Rows[e.RowIndex].Cells[e.ColumnIndex]);
                        Wallets.walletInfoDescription[temp.Name][Convert.ToInt32(e.RowIndex / 3)] = temp.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    }
                    break;
                case "ETH":
                    if (e.ColumnIndex == 1 && e.RowIndex >= 0 && e.RowIndex < (temp.Rows.Count - 1))
                    {
                        setNewDescription(temp.Rows[e.RowIndex].Cells[0].Value.ToString(), temp.Rows[e.RowIndex].Cells[e.ColumnIndex]);
                        Wallets.walletInfoDescription[temp.Name][e.RowIndex] = temp.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    }

                    break;
                default:
                    break;
            }
        }
        //Save USDToName rate for time


        public void  openOrderBittrex(DataGridViewRow row)
        {
            BittrexOrdersForm tempForm = new BittrexOrdersForm(this, row.Cells["Wallet"].Value.ToString());
            tempForm.Show();
            //parent.Visible = true;
        }
        public void openOperationnsBittrex(DataGridViewRow row,string currency)
        {
            string[] tempAll = Wallets.walletInfo["Bittrex"];

            OperationForm tempForm = new OperationForm(this, tempAll, "Bittrex",currency, Convert.ToDouble(row.Cells[currency].Value.ToString().Replace(',','.'), CultureInfo.InvariantCulture));
            tempForm.Show();
            //parent.Visible = true;
        }
        public void openOperationnsBlockchain(DataGridViewRow row)
        {
            OperationForm tempForm = new OperationForm(this,new string[]{(string) row.Cells["Wallet"].Value},row.DataGridView.Name);
            tempForm.Show();
        }
        public void openOperationnsETH(DataGridViewRow row)
        {
            string[] tempAll = Wallets.walletInfo["ETH"];
            OperationForm tempForm = new OperationForm(this, tempAll, "ETH", Convert.ToDouble(row.Cells["Balance ETH"].Value, CultureInfo.InvariantCulture));
            tempForm.Show();
        }
        public void openOperationnsLTC(DataGridViewRow row)
        {
            string[] tempAll = Wallets.walletInfo["LTC"];
            OperationForm tempForm = new OperationForm(this, tempAll, "LTC", Convert.ToDouble(row.Cells["Balance LTC"].Value, CultureInfo.InvariantCulture));
            tempForm.Show();
        }
        public void openOperationnsNiceHash(DataGridViewRow row)
        {
            string[] tempAll = new string[Convert.ToInt32(Wallets.walletInfo["NiceHash"].Count()/3)];
            for (int iter = 0,j=0; iter < Wallets.walletInfo["NiceHash"].Count(); iter = iter + 3,j++)
            {
                tempAll[j] = Wallets.walletInfo["NiceHash"][iter];
            }                                
            OperationForm tempForm = new OperationForm(this, tempAll, "NiceHash", Convert.ToDouble(row.Cells["Balance BTC"].Value, CultureInfo.InvariantCulture));
            tempForm.Show();
        }
        public void openAllOperationnsBlockchain(DataGridViewRow row)
        {
            string[] tempAll = Wallets.walletInfo["BTC cold wallet"];
            OperationForm tempForm = new OperationForm(this, tempAll, row.DataGridView.Name);
            tempForm.Show();
        }
        //Button refresh
        private  void button3_Click(object sender, EventArgs e)
        {
            
             Wallets.parseMarket();
             createDataGrid();
            
        }

        public void saveOrderHistory(string wallet)
        {
            string fileName = Directory.GetCurrentDirectory() + "//" + Wallets.bittrexOldOrderHistory + wallet + ".csv";
            if (!File.Exists(fileName))
            {
                using (var file = File.Create(fileName))
                { }

            }
            File.WriteAllText(fileName, "OrderUuid,Exchange,Type,Quantity,Limit,CommissionPaid,Price,Opened,Closed\r\n", Encoding.Unicode);
            using( FileStream stream=new FileStream(fileName,FileMode.Append))
            {
                for (int iter = 0; iter < Wallets.orderHistory.Count; iter++)
                {
                    string result = Wallets.orderHistory[iter].OrderUuid.ToString(CultureInfo.InvariantCulture) + "," + Wallets.orderHistory[iter].Exchange.ToString(CultureInfo.InvariantCulture) + ","
                        + Wallets.orderHistory[iter].OrderType.ToString(CultureInfo.InvariantCulture) + "," + Wallets.orderHistory[iter].Quantity.ToString(CultureInfo.InvariantCulture) + ","
                        + Wallets.orderHistory[iter].Limit.ToString(CultureInfo.InvariantCulture) + "," + Wallets.orderHistory[iter].Commission.ToString(CultureInfo.InvariantCulture) + ","
                         + Wallets.orderHistory[iter].Price.ToString(CultureInfo.InvariantCulture) + "," + Wallets.orderHistory[iter].Opened.ToString(CultureInfo.InvariantCulture) + ","
                        + Wallets.orderHistory[iter].Closed.ToString(CultureInfo.InvariantCulture) + "\r\n";
                    byte[] bytes = Encoding.Unicode.GetBytes(result);
                    stream.Write(bytes,0,bytes.Count());
                }               
            }
        }

        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            foreach (string key in dataGridArray.Keys)
            {
                dataGridArray[key].Width = panel1.Width - 20;
                labelArray[key].Location = new System.Drawing.Point(panel1.Width / 2 - labelArray[key].Width,  labelArray[key].Location.Y);
                dataGridArray[key].AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }

        }
    }
}
