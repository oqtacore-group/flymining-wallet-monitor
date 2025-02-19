using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Web;
using System.Net;
using System.Web.Script.Serialization;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using BitcoinInfoMiner;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace BitcoinInfoMiner
{
    public partial class MainWindow : Form
    {
        #region property

        public partial class RsOperationsData
        {
            public int rsID { get; set; }
            public int Id { get; set; }
            public System.DateTime Date { get; set; }
            public string TxID { get; set; }
            public string Oper_Type { get; set; }
            public string Wallet_Type { get; set; }
            public string Wallet { get; set; }
            public string Sum_Value { get; set; }
            public string Currency_Name { get; set; }
            public string Description { get; set; }
            public string Special_Kind { get; set; }
            public string Rest { get; set; }
        }
        string js;
        public string settingFileName = "flymining.ini";
        static SortedDictionary<DateTime, CSVData> fullWalletData;
        readonly static string logTestPath = Directory.GetCurrentDirectory() + "\\logReport\\Test.csv";
        readonly static string logTestPath2 = Directory.GetCurrentDirectory() + "\\logReport\\Test2.csv";
        readonly static string logRsPath = Directory.GetCurrentDirectory() + "\\logReport\\Rs.csv";
        const string defaultErrorState = "Not connected";
        const string configErrorState = "Config miner...";
        const string rebootErrorState = "Reboot miner...";
        public string minerLogin="root";
        public string minerPass = "root";
        public string userName;
        public string password;
        public class operParsing
        {
            public int withdrawCount;
            public List<DateTime> witdrawDates;
 
        }
        public static bool stopSorting = false;
        public static bool fullWalletInfoState = false;
        public struct Pool
        {
            public bool enabled;
            public string url;
            public string worker;
            public string psw;
            public bool postfixIp;
            public bool postfixNoChange;
            public bool postfixNone;
        }
        public struct ipRange
        {
            //public bool diffHigh;
            public int lowMinIP;
            public int lowMaxIP;
            public int highMinIP;
            public int highMaxIP;
        }
        public struct MinersState
        {
            public int Success;
            public int error;
            public int rebooting;
            public int config;
        }
        enum jsonMsgReply { Stats, Pools, Restart, EnablePool, Addpool, Removepool, Config, DisablePool };
        //0- Stats
        //1- Pools
        //2 -Restart
        //3 -enablePool
        //4 -addpool
        //5 -removepool
        //6 -config?
        
 
        public Pool pool1, pool2, pool3;
        public bool showOnlySuccess = true;
        public string[] antiRebootArray;
        public string[] antiReportArray;
        public bool parseReportNowTimeout = false;
        public int connectTimeout = 10;
        public int abnormalTemp = 90;
        public int minimalTemp = 10;
        public int apiCheckTimeout = 1800000;
        public int apiCheckStart = 1800000;
        public bool apiCheckState = true,HourlyReportFound=false;
        public bool onStartUp=false;
        public bool autoScan=false;
        public bool autoMonitoring=false;
        public int antMinerHashMin = 10000;
        public int monitoringTimeout = 30000; // Частота рефреша при мониторинге
        public int badCheckTimeout = 1800000; // Частота проверки статусов (При отключенном мониторинге)
        public int badCheckFirstStart = 10000; // Первая проверка статуса майнеров
        public bool detectHighTemp;
        public int operationSuccessCount = 0;//Переменная для определенния успешности операции
        public Dictionary<string, bool> operationDetails=new Dictionary<string,bool>();
        string command;
        string arg;
        bool monitoringStatus = false;// Автообновления состояние майнеров.
        bool connectingToSocketsStatus = false;// True, если в процессе обновления статусов майнеров.
        double p = 0;
        System.Threading.Timer monitoringTimer;
        System.Threading.Timer reportTimer;
        System.Threading.Timer reportNowTimer;
        System.Threading.Timer walletIncomeTimer;
        System.Threading.Timer usdRateTimer;
        System.Threading.Timer hourlyReport;
        private static readonly Regex rxNonDigits = new Regex(@"[^\d]+");// Для отбрасывания все не цифр

        private HttpServer myHttpServer = new HttpServer(58031);
        #endregion
        public MainWindow()
        {
            
            InitializeComponent();            
            initSettingsProperty();
            setLogFiles();
                Wallets.orderHistory = new Dictionary<int, OrderBody>();
                Wallets.walletInfo = new Dictionary<string, string[]>();
                Wallets.walletInfoDescription = new Dictionary<string, string[]>();
                Wallets.parseHistoricalRateHistory();
            readSettings();//Считываем настройки
            Wallets.getWalletsDescriptions();
            //Считываем range ip из ini. Создаем checkedButtons
            Wallets.parseMarket();
            getWalletsIncome();
            stopSorting = true;
            stopSorting = false;
           
             //создаем таймер


            TimerCallback walletIncomeCallBack = new TimerCallback(getWalletsIncome);
            walletIncomeTimer = new System.Threading.Timer(walletIncomeCallBack, null,
                30*60*1000, badCheckTimeout);       
            //httpThread = new Thread(() =>
            //{
            //    myHttpServer.Listen();
            //});
            //httpThread.Start();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Wallets.saveHistoricalInFile();
            saveSettings();
            //myHttpServer.Close();
            //httpThread.Abort();
        }
        /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////
        #region Settings || Logs
        //Sets value,before parsing settings
        private void initSettingsProperty()
        {
            pool1.enabled = false;
            pool2.enabled = false;
            pool3.enabled = false;
            pool1.url = "";
            pool2.url = "";
            pool3.url = "";
            pool1.worker = "";
            pool2.worker = "";
            pool3.worker = "";
            pool1.psw = "";
            pool2.psw = "";
            pool3.psw = "";
            pool1.postfixIp = true;
            pool2.postfixIp = true;
            pool3.postfixIp = true;
            pool1.postfixNoChange = false;
            pool2.postfixNoChange = false;
            pool3.postfixNoChange = false;
            pool1.postfixNone = false;
            pool2.postfixNone = false;
            pool3.postfixNone = false;

        }
        //Парсит файл настроек
        public void parseSettings()
        {
            try
            {
                string path = Directory.GetCurrentDirectory() + "\\" + settingFileName;
                if (File.Exists(path))
                {
                    string settingsData = File.ReadAllText(path, Encoding.Unicode);
                    string temp;
                    string[] tempArray;
                    string[] settingsArray = settingsData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int iter = 0; iter < settingsArray.Length; )
                    {
                        if (settingsArray[iter][0] == '[')
                        {
                            switch (settingsArray[iter])
                            {
                                case "[ui]":
                                    iter++;
                                    while (iter < settingsArray.Length && settingsArray[iter][0] != '[')
                                    {
                                        temp = settingsArray[iter].Remove(settingsArray[iter].IndexOf('='));
                                        switch (settingsArray[iter].Remove(settingsArray[iter].IndexOf('=')))
                                        {
                                            case "onlySuccessMiners":
                                                temp = settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1);
                                                showOnlySuccess = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool1Enabled":
                                                pool1.enabled = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool1Url":
                                                pool1.url = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool1Worker":
                                                pool1.worker = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool1Pwd":
                                                pool1.psw = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool1PostfixIp":
                                                pool1.postfixIp = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool1PostfixNoChange":
                                                pool1.postfixNoChange = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool1PostfixNone":
                                                pool1.postfixNone = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool2Enabled":
                                                pool2.enabled = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool2Url":
                                                pool2.url = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool2Worker":
                                                pool2.worker = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool2Pwd":
                                                pool2.psw = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool2PostfixIp":
                                                pool2.postfixIp = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool2PostfixNoChange":
                                                pool2.postfixNoChange = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool2PostfixNone":
                                                pool2.postfixNone = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool3Enabled":
                                                pool3.enabled = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool3Url":
                                                pool3.url = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool3Worker":
                                                pool3.worker = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool3Pwd":
                                                pool3.psw = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool3PostfixIp":
                                                pool3.postfixIp = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool3PostfixNoChange":
                                                pool3.postfixNoChange = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "pool3PostfixNone":
                                                pool3.postfixNone = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            default:
                                                iter++;
                                                break;
                                        }
                                    }
                                    break;
                                case "[scanner]":
                                    iter++;
                                    while (iter < settingsArray.Length && settingsArray[iter][0] != '[')
                                    {
                                        switch (settingsArray[iter].Remove(settingsArray[iter].IndexOf('=')))
                                        {
                                            case "AntiAutoReboot":
                                                antiRebootArray = settingsArray[iter].Substring(settingsArray[iter].IndexOf('=') + 1)
                                                   .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                iter++;
                                                break;
                                            case "AntiAutoReport":
                                                antiReportArray = settingsArray[iter].Substring(settingsArray[iter].IndexOf('=') + 1)
                                                   .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                                iter++;
                                                break;
                                            default:
                                                iter++;
                                                break;
                                        }
                                    }
                                    break;
                                case "[Wallet]":
                                    iter++;
                                    while (iter < settingsArray.Length && settingsArray[iter][0] != '[')
                                    {
                                        switch (settingsArray[iter].Remove(settingsArray[iter].IndexOf('=')))
                                        {
                                            case "Bittrex":
                                                tempArray = settingsArray[iter].Substring(settingsArray[iter].IndexOf('=') + 1)
                                                   .Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLowerInvariant()).ToArray(); ;
                                                Wallets.walletInfo.Add("Bittrex", tempArray);
                                                iter++;
                                                break;
                                            case "ETH":
                                                tempArray = settingsArray[iter].Substring(settingsArray[iter].IndexOf('=') + 1)
                                                   .Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLowerInvariant()).ToArray(); ;
                                                Wallets.walletInfo.Add("ETH", tempArray);
                                                iter++;
                                                break;
                                            case "LTC":
                                                tempArray = settingsArray[iter].Substring(settingsArray[iter].IndexOf('=') + 1)
                                                   .Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                                Wallets.walletInfo.Add("LTC", tempArray);
                                                iter++;
                                                break;
                                            case "NiceHash":
                                                tempArray = settingsArray[iter].Substring(settingsArray[iter].IndexOf('=') + 1)
                                                   .Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                                Wallets.walletInfo.Add("NiceHash", tempArray);
                                                iter++;
                                                break;
                                            case "BTC cold wallet":
                                                tempArray = settingsArray[iter].Substring(settingsArray[iter].IndexOf('=') + 1)
                                                    .Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                                Wallets.walletInfo.Add("BTC cold wallet", tempArray);
                                                iter++;
                                                break;
                                            default:
                                                iter++;
                                                break;
                                        }
                                    }
                                    break;
                                case "[configurator]":
                                    iter++;
                                    while (iter < settingsArray.Length && settingsArray[iter][0] != '[')
                                    {
                                        switch (settingsArray[iter].Remove(settingsArray[iter].IndexOf('=')))
                                        {
                                            case "workerNameIpParts":
                                                //pool1.enabled = Convert.ToBoolean(settingsArray[iter].Substring(settingsArray[iter].IndexOf('=') + 1));
                                                iter++;
                                                break;
                                            default:
                                                iter++;
                                                break;
                                        }
                                    }
                                    break;
                                case "[rebooter]":
                                    iter++;
                                    while (iter < settingsArray.Length && settingsArray[iter][0] != '[')
                                    {
                                        switch (settingsArray[iter].Remove(settingsArray[iter].IndexOf('=')))
                                        {
                                            default:
                                                iter++;
                                                break;
                                        }
                                    }
                                    break;
                                case "[highlight]":
                                    iter++;
                                    while (iter < settingsArray.Length && settingsArray[iter][0] != '[')
                                    {
                                        switch (settingsArray[iter].Remove(settingsArray[iter].IndexOf('=')))
                                        {
                                            case "highlightTempState":
                                                if (settingsArray[iter].Substring(settingsArray[iter].IndexOf('=') + 1) != "")
                                                {
                                                    detectHighTemp = Convert.ToBoolean(settingsArray[iter].Substring
                                                        (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                }
                                                iter++;
                                                break;
                                            case "highlightLowHashrates":
                                                antMinerHashMin = Convert.ToInt32(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            default:
                                                iter++;
                                                break;
                                        }
                                    }
                                    break;
                                case "[login]":
                                    iter++;
                                    while (iter < settingsArray.Length && settingsArray[iter][0] != '[')
                                    {
                                        switch (settingsArray[iter].Remove(settingsArray[iter].IndexOf('=')))
                                        {
                                            case "login":
                                                minerLogin = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                if (minerLogin == "")
                                                    minerLogin = "root";
                                                iter++;
                                                break;
                                            case "minerPasswords":
                                                minerPass = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                if (minerPass == "")
                                                    minerPass = "root";
                                                iter++;
                                                break;
                                            case "FlyMiningLogin":
                                                Log.flyMiningUserName = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "FlyMiningPasswords":
                                                Log.flyMiningPassword = Convert.ToString(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            default:
                                                iter++;
                                                break;
                                        }
                                    }
                                    break;
                                case "[feature]":
                                    iter++;
                                    while (iter < settingsArray.Length && settingsArray[iter][0] != '[')
                                    {
                                        switch (settingsArray[iter].Remove(settingsArray[iter].IndexOf('=')))
                                        {
                                            case "openMinerCPWithPassword":
                                                iter++;
                                                break;
                                            case "apiCheckTimeout":
                                                apiCheckTimeout = Convert.ToInt32(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "apiCheckStart":
                                                apiCheckStart = Convert.ToInt32(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "apiCheckState":
                                                apiCheckState = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "onStartUp":
                                                onStartUp = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "autoScan":
                                                autoScan = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            case "autoMonitoring":
                                                autoMonitoring = Convert.ToBoolean(settingsArray[iter].Substring
                                                    (settingsArray[iter].IndexOf('=') + 1), CultureInfo.InvariantCulture);
                                                iter++;
                                                break;
                                            default:
                                                iter++;
                                                break;
                                        }
                                    }
                                    break;
                                default:
                                    iter++;
                                    break;
                            }
                        }
                        else
                        {
                            iter++;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log.logDebug(Convert.ToString(ex));
            }

        }
        //Присваивает настройки из файла настроек
        public void readSettings()
        {
            //Считываем настройки
            //
            parseSettings();
            //Заполняем значения настроек на форму.
            ////Pool1
            //pool1RadioButtonMain.Checked = pool1.enabled;
            //pool1textBoxPool.Text = pool1.url;
            //pool1textBoxPWD.Text = pool1.psw;
            //pool1textBoxSub.Text = pool1.worker;
            //pool1postfixIp.Checked = pool1.postfixIp;
            //pool1postfixNoChange.Checked = pool1.postfixNoChange;
            //pool1postfixNone.Checked = pool1.postfixNone;
            ////Pool2
            //pool2RadioButtonMain.Checked = pool2.enabled;
            //pool2textBoxPool.Text = pool2.url;
            //pool2textBoxPWD.Text = pool2.psw;
            //pool2textBoxSub.Text = pool2.worker;
            //pool2postfixIp.Checked = pool2.postfixIp;
            //pool2postfixNoChange.Checked = pool2.postfixNoChange;
            //pool2postfixNone.Checked = pool2.postfixNone;
            ////Pool3
            //pool3RadioButtonMain.Checked = pool3.enabled;
            //pool3textBoxPool.Text = pool3.url;
            //pool3textBoxPWD.Text = pool3.psw;
            //pool3textBoxSub.Text = pool3.worker;
            //pool3postfixIp.Checked = pool3.postfixIp;
            //pool3postfixNoChange.Checked = pool3.postfixNoChange;
            //pool3postfixNone.Checked = pool3.postfixNone;

        }
        // Сохранение настроек
        public void saveSettings()
        {
            try { 
            string path = Directory.GetCurrentDirectory() + "\\" + settingFileName;
            if (!File.Exists(path))
            {
                using (var file = File.Create(path))
                { }
            }
            string[] writeSettings = new string[100];
            //[ui]
            //
            int iter = 0;
            writeSettings[iter++] = "[ui]";
            
            writeSettings[iter++] = "pool1Enabled=" + pool1.enabled.ToString();
            writeSettings[iter++] = "pool2Enabled=" + pool2.enabled.ToString();
            writeSettings[iter++] = "pool3Enabled=" + pool3.enabled.ToString();
            writeSettings[iter++] = "pool1Url=" + pool1.url;
            writeSettings[iter++] = "pool2Url=" + pool2.url;
            writeSettings[iter++] = "pool3Url=" + pool3.url;
            writeSettings[iter++] = "pool1Worker=" + pool1.worker;
            writeSettings[iter++] = "pool2Worker=" + pool2.worker;
            writeSettings[iter++] = "pool3Worker=" + pool3.worker;
            writeSettings[iter++] = "pool1Pwd=" + pool1.psw;
            writeSettings[iter++] = "pool2Pwd=" + pool2.psw;
            writeSettings[iter++] = "pool3Pwd=" + pool3.psw;
            writeSettings[iter++] = "pool1PostfixIp=" + pool1.postfixIp.ToString();
            writeSettings[iter++] = "pool2PostfixIp=" + pool2.postfixIp.ToString();
            writeSettings[iter++] = "pool3PostfixIp=" + pool3.postfixIp.ToString();
            writeSettings[iter++] = "pool1PostfixNoChange=" + pool1.postfixNoChange.ToString();
            writeSettings[iter++] = "pool2PostfixNoChange=" + pool2.postfixNoChange.ToString();
            writeSettings[iter++] = "pool3PostfixNoChange=" + pool3.postfixNoChange.ToString();
            writeSettings[iter++] = "pool1PostfixNone=" + pool1.postfixNone.ToString();
            writeSettings[iter++] = "pool2PostfixNone=" + pool2.postfixNone.ToString();
            writeSettings[iter++] = "pool3PostfixNone=" + pool3.postfixNone.ToString();

            //ipRangeGroups="LAN:192.168.1.101-192.168.1.220,!192.168.56.0-...255;#:192.168.1.2-..1.100"
            //[scanner]
            iter++;
            writeSettings[iter++] = "[scanner]";
            writeSettings[iter++] = "sessionTimeout=" + (connectTimeout / 100).ToString();
            if (antiRebootArray != null)
            {
                antiRebootArray = antiRebootArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                writeSettings[iter++] = "AntiAutoReboot=" + String.Join(";", antiRebootArray);
            }
            else
                writeSettings[iter++] = "AntiAutoReboot=" + "";
            if (antiReportArray != null)
            {
                antiReportArray = antiReportArray.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                writeSettings[iter++] = "AntiAutoReport=" + String.Join(";", antiReportArray);
            }
            else
                writeSettings[iter++] = "AntiAutoReport=" + "";
            //[Wallet]
            iter++;
            writeSettings[iter++] = "[Wallet]";


            if (Wallets.walletInfo != null && Wallets.walletInfo.Keys.Contains("Bittrex"))
            {
                string temp = "";
                for (int iterW = 0; iterW < Wallets.walletInfo["Bittrex"].Count(); iterW++)
                {
                    temp += Wallets.walletInfoDescription["Bittrex"][iterW] + ":" + Wallets.walletInfo["Bittrex"][iterW]+"\\";
                }
                writeSettings[iter++] = "Bittrex=" + temp;
            }
            else
                writeSettings[iter++] = "Bittrex=";
            
            if (Wallets.walletInfo != null && Wallets.walletInfo.Keys.Contains("LTC"))
            {
                string temp = "";
                for (int iterW = 0; iterW < Wallets.walletInfo["LTC"].Count(); iterW++)
                {
                    temp += Wallets.walletInfoDescription["LTC"][iterW] + ":" + Wallets.walletInfo["LTC"][iterW] + "\\";
                }
                writeSettings[iter++] = "LTC=" + temp;
            }
            else
                writeSettings[iter++] = "LTC=";
            if (Wallets.walletInfo != null && Wallets.walletInfo.Keys.Contains("NiceHash"))
            {
                string temp = "";
                for (int iterW = 0; iterW < Wallets.walletInfo["NiceHash"].Count(); iterW++)
                {
                    temp += Wallets.walletInfoDescription["NiceHash"][iterW] + ":" + Wallets.walletInfo["NiceHash"][iterW] + "\\";
                }
                writeSettings[iter++] = "NiceHash=" + temp;
            }
            else
                writeSettings[iter++] = "NiceHash=";
            if (Wallets.walletInfo != null && Wallets.walletInfo.Keys.Contains("ETH"))
            {
                string temp = "";
                for (int iterW = 0; iterW < Wallets.walletInfo["ETH"].Count(); iterW++)
                {
                    temp += Wallets.walletInfoDescription["ETH"][iterW] + ":" + Wallets.walletInfo["ETH"][iterW] + "\\";
                }
                writeSettings[iter++] = "ETH=" + temp;
            }
            else
                writeSettings[iter++] = "ETH=";
            if (Wallets.walletInfo != null && Wallets.walletInfo.Keys.Contains("BTC cold wallet"))
            {
                string temp = "";
                for (int iterW = 0; iterW < Wallets.walletInfo["BTC cold wallet"].Count(); iterW++)
                {
                    temp += Wallets.walletInfoDescription["BTC cold wallet"][iterW] + ":" + Wallets.walletInfo["BTC cold wallet"][iterW] + "\\";
                }
                writeSettings[iter++] = "BTC cold wallet=" + temp;
            }
            else
                writeSettings[iter++] = "BTC cold wallet=";
            //[configurator]
            iter++;
            writeSettings[iter++] = "[configurator]";
            writeSettings[iter++] = "sessionTimeout=" + (monitoringTimeout / 1000).ToString();
            writeSettings[iter++] = "workerNameIpParts=" + "";

            //[rebooter]
            iter++;
            writeSettings[iter++] = "[rebooter]";
            writeSettings[iter++] = "sessionTimeout=" + (badCheckTimeout / 60000).ToString();

            //[highlight]

            iter++;
            writeSettings[iter++] = "[highlight]";
            writeSettings[iter++] = "highlightTemperatureMoreThan=" + abnormalTemp.ToString();
            writeSettings[iter++] = "highlightTemperatureLessThan=" + minimalTemp.ToString();
            writeSettings[iter++] = "highlightTempState=" + detectHighTemp.ToString();
            writeSettings[iter++] = "highlightLowHashrates=" + antMinerHashMin.ToString();


            //isHighlightTemperature=true
            //highlightTemperatureMoreThan=90
            //highlightTemperatureLessThan=0
            //isHighlightWrongWorkerName=true
            //isHighlightLowHashrate=true
            //highlightLowHashrates=10001

            //[login]
            iter++;
            writeSettings[iter++] = "[login]";
            writeSettings[iter++] = "login=" + minerLogin;
            writeSettings[iter++] = "minerPasswords=" + minerPass;
            writeSettings[iter++] = "FlyMiningLogin=" + Log.flyMiningUserName;
            writeSettings[iter++] = "FlyMiningPasswords=" + Log.flyMiningPassword;                          
            //minerPasswords="QW50bWluZXI=:cm9vdA==:cm9vdA==&QXZhbG9u:cm9vdA==:"

            //[feature]
            iter++;
            writeSettings[iter++] = "[feature]";
            writeSettings[iter++] = "openMinerCPWithPassword=" + "";
            writeSettings[iter++] = "apiCheckTimeout=" + apiCheckTimeout.ToString();
            writeSettings[iter++] = "apiCheckStart=" + apiCheckStart.ToString();
            writeSettings[iter++] = "apiCheckState=" + apiCheckState.ToString();
            writeSettings[iter++] = "onStartUp=" + onStartUp.ToString();
            writeSettings[iter++] = "autoScan=" + autoScan.ToString();
            writeSettings[iter++] = "autoMonitoring=" + autoMonitoring.ToString();

            //openMinerCPWithPassword=true
            File.WriteAllLines(path, writeSettings, Encoding.Unicode);
            }
            catch (Exception ex)
            {
                Log.logDebug("Save:" + Convert.ToString(ex));
            }

        }
        //Check if all needed dires exist
        public void setLogFiles()
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\logDebug\\"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\logDebug\\");
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\logReport\\"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\logReport\\");
            if (!Directory.Exists(Log.logArchivePath))
                Directory.CreateDirectory(Log.logArchivePath);
            if (!File.Exists(Log.logDebugPath))
                using (var file = File.Create(Log.logDebugPath))
                { }
            if (!File.Exists(Directory.GetCurrentDirectory() +"\\"+ settingFileName))
            {
                using (var file = File.Create(Directory.GetCurrentDirectory() +"\\"+ settingFileName))
                { }
                try
                {
                    saveSettings();
                }
                catch(Exception ex)
                {
                    Log.logDebug("Save:"+Convert.ToString(ex));
                }
            }

            if (new FileInfo(Log.logDebugPath).Length != 0)
            {
                System.IO.File.WriteAllText(Log.logDebugPath, string.Empty, Encoding.Unicode);//Обнуляем прошлый дебаг
            }
            if (!File.Exists(Log.logReportPath))
                using (var file = File.Create(Log.logReportPath))
                { }

        }
        //Event on report Timeout timer end
        public void parsingReportNowTimeoutEnd(object obj)
        {
            parseReportNowTimeout = false;
            reportNowTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        
        //Открытие окна настроек
        private void buttonSettingWindow_Click(object sender, EventArgs e)
        {
            SettingForm settingWindow = new SettingForm(this);
            this.Enabled = false;
            settingWindow.Show();

        }
        #endregion
        /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////
        #region Functions Network
        //Testing function. Not used
        //Отправка HttpcLient запроса через Request

        //Отправка Get запроса через Request
        public string sendJson(string command, string arg = "", string host = "localhost", int port = 4028)
        {
            this.command = command;
            this.arg = arg;
            var webRequest = (WebRequest)WebRequest.Create("http://"+minerLogin+":"+minerPass+"@" + host + ":" + Convert.ToString(port, CultureInfo.InvariantCulture));
            webRequest.ContentType = "application/json";
            webRequest.Method = WebRequestMethods.Http.Post;
            try
            {
                using (var streamWriter = new StreamWriter( webRequest.GetRequestStream()))
                {
                    string json = "";
                    if (this.arg != "")
                        json = new JavaScriptSerializer().Serialize(new
                        {
                            command = this.command,
                            parameter = this.arg
                        });
                    else
                    {
                        json = new JavaScriptSerializer().Serialize(new
                        {
                            command = this.command
                        });
                    }
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = webRequest.GetResponse();
                string result = "";
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
                return result;
            }
            catch (Exception e)
            {
                Log.logDebug("SendJson "+Convert.ToString(e, CultureInfo.InvariantCulture));
                return Convert.ToString(e, CultureInfo.InvariantCulture);
            }
        }
        //Отправка Get запроса через Socket
        public string sendsocketJson(string command, string arg = "", string host = "localhost", int port = 4028)
        {
            this.command = command;
            this.arg = arg;
            string result = "";
            Socket socketJson = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);
            try
            {
                socketJson.ReceiveTimeout = connectTimeout;
                socketJson.SendTimeout = connectTimeout;


                //ConnectAsync(host, port, socketJson);
                //await socketJson.ConnectAsync(temp);
                //await socketJson.ConnectAsync(host, port);
                IAsyncResult resultConnect = socketJson.BeginConnect(host, port, null, null);
                bool Success = resultConnect.AsyncWaitHandle.WaitOne(2000, true);//connectTimeout
                if (socketJson.Connected)
                {
                    socketJson.EndConnect(resultConnect);
                }
                else
                {
                    socketJson.Close();
                    throw new ApplicationException("Failed to connect server.");
                }

                string json = "";
                if (this.arg != "")
                    json = new JavaScriptSerializer().Serialize(new
                    {
                        command = this.command,
                        parameter = this.arg
                    });
                else
                {
                    json = new JavaScriptSerializer().Serialize(new
                    {
                        command = this.command
                    });
                }
                byte[] msg = Encoding.UTF8.GetBytes(json);
                byte[] bytes = new byte[300];

                //await Task.Factory.FromAsync<int>(
                //    socketJson.BeginSend(msg, 0, msg.Length, SocketFlags.None, null, socketJson),
                //    socketJson.EndSend).ConfigureAwait(false);
                socketJson.Send(msg);

                //result += "End :" + Convert.ToString(0);  
                socketJson.Receive(bytes);


                result += Encoding.ASCII.GetString(bytes);
                int i = 0;
                while (i < 100)
                {
                    i++;
                    socketJson.Receive(bytes);

                    if (bytes != null && bytes.Any(b => b != 0))
                    {
                        result += Encoding.ASCII.GetString(bytes);
                        bytes = new byte[100];
                    }
                    else
                    {
                        break;
                    }
                }
                socketJson.Disconnect(false);


                return Convert.ToString(result, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Log.logDebug("SendSocketJson "+command+ "  "+Convert.ToString(e, CultureInfo.InvariantCulture));
                return Convert.ToString(e, CultureInfo.InvariantCulture);
            }
            //try
            //{
            //    System.Net.ServicePointManager.Expect100Continue = false;
            //    var url = "http://" + host +":"+Convert.ToString(port);
            //    HttpClientHandler handler = new HttpClientHandler();
            //    //handler.Credentials = new System.Net.NetworkCredential("root", "root");
            //    HttpClient client = new HttpClient();//handler);
            //    client.Timeout = new TimeSpan(0, 0,10);
            //    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Digest username=\"root\", realm=\"antMiner Configuration\",uri=\"/cgi-bin/set_network_conf.cgi\"");
            //    //client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
            //    var method=new HttpMethod(command);
            //    HttpRequestMessage jsonMsg = new HttpRequestMessage(method, url);
            //    var response = await client.SendAsync(jsonMsg);
            //    var responseString = await response.Content.ReadAsStringAsync();
            //    return Convert.ToString(response);
            //}
            //catch (Exception exception)
            //{
            //    return Convert.ToString(exception);
            //}
        }
        //Проверка,если сообщение успешно.
        public bool checkIfSuccess(string msg, int msgCode)
        {
            //0- Stats
            //1- Pools
            //2 -Restart
            //3 -enablePool
            //4 -addpool
            //5 -removepool
            //6 -config?


            return true;
        }
        #endregion
        /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////
        #region UI Functions




        // Button WalletInfo. Opening form with wallet info
        private void button8_Click(object sender, EventArgs e)
        {
            BTCBalanceForm btcForm = new BTCBalanceForm(this);
            this.Visible = false;
            btcForm.Show();
        }


 

        //Button for creating csv
        private  void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            getCSVData();
            DialogResult errorWindow = MessageBox.Show("Csv Files created in Directory: " + Log.logReportDir
                , "Result", MessageBoxButtons.OK);
            button1.Enabled = true;
        }
        #endregion
        /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////   /////////////////////////////////////////////////////////
        #region Wallet Functions
        //Calculates wallet income
        public  void getWalletsIncome(object obj = null)
        {
            getFullWalletDate();
            Dictionary<string, double> result = new Dictionary<string, double>();
            Dictionary<string, OperData> operData = new Dictionary<string, OperData>();
            try       
            {
                Wallets.parseMarket();
               
            }
            catch (Exception ex)
            {
                Log.logDebug("Parse Market Error " + Convert.ToString(ex));
            }
            foreach (string key in Wallets.walletInfo.Keys)
            {
                if (Wallets.walletInfo[key].Length > 0)
                {
                    switch (key)
                    {
                        case "Bittrex":
                            try
                            {
                                var bittrexResponse = JsonConvert.DeserializeObject<BittrexResponse>(Wallets.sendBittrexRequest("account/getbalances",
                                                Wallets.walletInfo[key][0], Wallets.walletInfo[key][1]));

                                string currence = "";
                                for (int iterMain = 0; iterMain < bittrexResponse.Result.Count(); iterMain++)
                                {
                                    currence = bittrexResponse.Result[iterMain].First.First.ToString();
                                    for (int iter = 0; iter < Wallets.walletInfo[key].Count(); iter += 2)
                                    {
                                        if (iter + 1 < Wallets.walletInfo[key].Count())
                                        {
                                            var bodyBTX = Wallets.getBittrexOperation(Wallets.walletInfo[key][iter], Wallets.walletInfo[key][iter + 1], currence);
                                            if (bodyBTX.Count != 0)
                                            {
                                                if (!result.Keys.Contains(currence))
                                                {
                                                    result.Add(currence, 0);
                                                }
                                                for (int innerIter = 0; innerIter < bodyBTX.Count; innerIter++)
                                                {
                                                    try
                                                    {
                                                        if (bodyBTX[innerIter].TxCost == 0 && (DateTime.Now -
                                                            (Convert.ToDateTime(bodyBTX[innerIter].date, CultureInfo.InvariantCulture))).Days < 1)
                                                        {
                                                            result[currence] += Math.Round(bodyBTX[innerIter].amount, 8);
                                                            operData.Add(bodyBTX[innerIter].TxId, new OperData
                                                            {
                                                                currency = currence,
                                                                date = Convert.ToDateTime(bodyBTX[innerIter].date, CultureInfo.InvariantCulture),
                                                                sum = Math.Round(bodyBTX[innerIter].amount, 8)
                                                            });
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log.logDebug(Convert.ToString(ex));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                Log.logDebug("Bittrex income Error " + Convert.ToString(ex));
                            }

                            break;
                        case "ETH":
                            try
                            {

                                var bodyETH = Wallets.getETHOperation(Wallets.walletInfo[key]);
                                if (bodyETH.Count != 0)
                                {
                                    if (!result.Keys.Contains("ETH"))
                                    {
                                        result.Add("ETH", 0);
                                    }

                                    for (int innerIter = 0; innerIter < bodyETH.Count; innerIter++)
                                    {
                                        
                                        if (Wallets.walletInfo[key].Contains(bodyETH[innerIter].to) && DateTime.Now.Subtract
                                            (Wallets.ToDateTimeFromUnix(bodyETH[innerIter].time).ToLocalTime()).Days < 1)
                                        {
                                            result["ETH"] += Math.Round(Convert.ToDouble(bodyETH[innerIter].value,
                                CultureInfo.InvariantCulture) / 1000000000000000000, 8);
                                            operData.Add(bodyETH[innerIter].hash, new OperData
                                            {
                                                currency = "ETH",
                                                date = Wallets.ToDateTimeFromUnix(bodyETH[innerIter].time).ToUniversalTime(),
                                                sum = Math.Round(Convert.ToDouble(bodyETH[innerIter].value,
                                CultureInfo.InvariantCulture) / 1000000000000000000, 8)
                                            });

                                        }
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                Log.logDebug("ETH Income Error " + Convert.ToString(ex));
                            }
                            break;
                        case "LTC":
                            try
                            {
                                var bodyLTC = Wallets.getLTCOperation(Wallets.walletInfo[key]);
                                if (bodyLTC.Count != 0)
                                {
                                    if (!result.Keys.Contains("LTC"))
                                    {
                                        result.Add("LTC", 0);
                                    }
                                    for (int innerIter = 0; innerIter < bodyLTC.Count; innerIter++)
                                    {

                                        if (bodyLTC[innerIter].state && DateTime.Now.Subtract
                                            (Wallets.ToDateTimeFromUnix(bodyLTC[innerIter].time).ToLocalTime()).Days < 1)
                                        {
                                            result["LTC"] += Math.Round(Convert.ToDouble(bodyLTC[innerIter].value,
                                CultureInfo.InvariantCulture), 8);
                                            operData.Add(bodyLTC[innerIter].txid, new OperData
                                            {
                                                currency = "LTC",
                                                date = Wallets.ToDateTimeFromUnix(bodyLTC[innerIter].time).ToUniversalTime(),
                                                sum = Math.Round(Convert.ToDouble(bodyLTC[innerIter].value,
                                CultureInfo.InvariantCulture), 8)
                                            });
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.logDebug("LTC Income Error " + Convert.ToString(ex));
                            }
                            break;
                        case "BTC cold wallet":
                            try
                            {
                                var bodyBTC =  Wallets.getAllBlockchainOperation(Wallets.walletInfo[key]);
                                if (bodyBTC.Count != 0)
                                {
                                    if (!result.Keys.Contains("BTC"))
                                    {
                                        result.Add("BTC", 0);
                                    }
                                    for (int innerIter = 0; innerIter < bodyBTC.Count; innerIter++)
                                    {
                                        if (bodyBTC[innerIter].result > 0 && DateTime.Now.Subtract
                                            (Wallets.ToDateTimeFromUnix(bodyBTC[innerIter].time).ToLocalTime()).Days < 1)
                                        {
                                            result["BTC"] += Math.Round(Convert.ToDouble(bodyBTC[innerIter].result,
                                CultureInfo.InvariantCulture) / 100000000, 8);
                                            operData.Add(bodyBTC[innerIter].hash, new OperData
                                            {
                                                currency = "BTC",
                                                date = Convert.ToDateTime(Wallets.ToDateTimeFromUnix(bodyBTC[innerIter].time).ToUniversalTime(), CultureInfo.InvariantCulture),
                                                sum = Math.Round(Convert.ToDouble(bodyBTC[innerIter].result,
                                CultureInfo.InvariantCulture) / 100000000, 8)
                                            });

                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.logDebug("BTC Income Error " + Convert.ToString(ex));
                            }
                            break;

                    }
                }
            }
            
        }
        // Calculate Rest foe FullWalletDate
        public  void calculateRestAll()
        {
            foreach (string key in Wallets.walletInfo.Keys)
            {               
                if (Wallets.walletInfo[key].Length > 0)
                {
                    switch (key)
                    {
                        case "Bittrex":
                            for (int iter = 0; iter < Wallets.walletInfo[key].Count(); iter += 2)
                            {
                                var bittrexResponse = JsonConvert.DeserializeObject<BittrexResponse>( Wallets.sendBittrexRequest("account/getbalances",
                                    Wallets.walletInfo[key][iter], Wallets.walletInfo[key][iter + 1]));
                                string currence = "";
                                for (int iterMain = 0; iterMain < bittrexResponse.Result.Count(); iterMain++)
                                {
                                    currence = bittrexResponse.Result[iterMain].First.First.ToString();
                                    if (iter + 1 < Wallets.walletInfo[key].Count())
                                    {
                                        calculateRestThisWallet(Wallets.walletInfo[key][iter], Convert.ToDouble(bittrexResponse.Result[iterMain].First.Next.First.ToString(), CultureInfo.InvariantCulture), currence);
                                    }
                                }
                            }
                            break;
                        case "ETH":
                            for (int iter = 0; iter < Wallets.walletInfo[key].Count(); iter++)
                            {
                                Dictionary<String, String> uriArgs = new Dictionary<string, string>();
                                uriArgs.Add("module", "account");
                                uriArgs.Add("action", "balance");
                                uriArgs.Add("address", Wallets.walletInfo[key][iter]);
                                JToken parsedBalance = JsonConvert.DeserializeObject<JToken>( Wallets.sendAsyncRequest("", Wallets.ethUri, uriArgs));
                                calculateRestThisWallet(Wallets.walletInfo[key][iter], Convert.ToDouble(Math.Round(Convert.ToDecimal(parsedBalance.Last.Last.ToString(), 
                                    CultureInfo.InvariantCulture) / 1000000000000000000, 12)), "ETH");
                            }
                            break;
                        case "LTC":
                            for (int iter = 0; iter < Wallets.walletInfo[key].Count(); iter++)
                            {

                                JToken parsedBalance = JsonConvert.DeserializeObject<JToken>(
                                     Wallets.sendAsyncRequest("address/LTC/" + Wallets.walletInfo[key][iter],Wallets.LTCUri));                               
                                calculateRestThisWallet(Wallets.walletInfo[key][iter], Convert.ToDouble(parsedBalance.First.Next.Last.First.Next.Next.Last.ToString(), CultureInfo.InvariantCulture), "LTC");
                            }
                            break;
                        case "BTC cold wallet":
                            for (int iter = 0; iter < Wallets.walletInfo[key].Count(); iter++)
                            {
                                calculateRestThisWallet(Wallets.walletInfo[key][iter], Convert.ToDouble( Wallets.sendAsyncRequest("q/addressbalance/" + Wallets.walletInfo[key][iter],
                                        Wallets.blockChainUri), CultureInfo.InvariantCulture) / 100000000, "BTC");
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        //
        public void calculateRestThisWallet(string wallet,double balance,string currence)
        {
            for (int iter = fullWalletData.Count - 1; iter >= 0; iter--)
            {
                if (fullWalletData.ElementAt(iter).Value.type == txType.orderBuy || fullWalletData.ElementAt(iter).Value.type == txType.orderSell)
                {
                    if (fullWalletData.ElementAt(iter).Value.Wallet == wallet && currence == "BTC")
                    {
                        fullWalletData.ElementAt(iter).Value.rest = balance;
                        if (fullWalletData.ElementAt(iter).Value.type == txType.orderBuy)
                        {
                            balance = balance + Math.Abs(fullWalletData.ElementAt(iter).Value.value) + Math.Abs(fullWalletData.ElementAt(iter).Value.fee);
                        }
                        if (fullWalletData.ElementAt(iter).Value.type == txType.orderSell)
                        {
                            balance = balance - Math.Abs(fullWalletData.ElementAt(iter).Value.value) + Math.Abs(fullWalletData.ElementAt(iter).Value.fee);
                        }
                    }
                    else
                    {
                        if (fullWalletData.ElementAt(iter).Value.Wallet == wallet
                            && fullWalletData.ElementAt(iter).Value.dopData.Substring(fullWalletData.ElementAt(iter).Value.dopData.IndexOf('-') + 1) == currence)
                        {
                            fullWalletData.ElementAt(iter).Value.restConvert = balance;
                            if (fullWalletData.ElementAt(iter).Value.type == txType.orderBuy)
                            {
                                balance = balance - Math.Abs(fullWalletData.ElementAt(iter).Value.valueConvert);
                            }
                            if (fullWalletData.ElementAt(iter).Value.type == txType.orderSell)
                            {
                                balance = balance + Math.Abs(fullWalletData.ElementAt(iter).Value.valueConvert);
                            }


                        }
                    }
                }
                else
                {
                    if (fullWalletData.ElementAt(iter).Value.Wallet == wallet && fullWalletData.ElementAt(iter).Value.currency == currence)
                    {
                        fullWalletData.ElementAt(iter).Value.rest = balance;
                        if (fullWalletData.ElementAt(iter).Value.type == txType.txD)
                        {
                            balance = balance - Math.Abs(fullWalletData.ElementAt(iter).Value.value);
                        }
                        if (fullWalletData.ElementAt(iter).Value.type == txType.txW)
                        {
                            balance = balance + Math.Abs(fullWalletData.ElementAt(iter).Value.value);
                        }


                    }
                }
            }
        }
        //Gets data for csv
        public  void getFullWalletDate(object obj = null)
        {
            fullWalletInfoState = true;
            try
            {             
                fullWalletData = new SortedDictionary<DateTime, CSVData>();
                Wallets.parseMarket();
                foreach (string key in Wallets.walletInfo.Keys)
                {
                    fullWalletInfoState = true;
                    if (Wallets.walletInfo[key].Length > 0)
                    {
                        switch (key)
                        {
                            case "Bittrex":

                                for (int iter = 0; iter < Wallets.walletInfo[key].Count(); iter += 2)
                                {
                                    var bittrexResponse = JsonConvert.DeserializeObject<BittrexResponse>( Wallets.sendBittrexRequest("account/getbalances",
                                        Wallets.walletInfo[key][iter], Wallets.walletInfo[key][iter + 1]));
                                    string currence = "";
                                    for (int iterMain = 0; iterMain < bittrexResponse.Result.Count(); iterMain++)
                                    {
                                        currence = bittrexResponse.Result[iterMain].First.First.ToString();
                                        if (iter + 1 < Wallets.walletInfo[key].Count())
                                        {
                                            var bodyBTX =  Wallets.getBittrexOperation(Wallets.walletInfo[key][iter], Wallets.walletInfo[key][iter + 1], currence);
                                            if (bodyBTX.Count != 0)
                                            {
                                                for (int innerIter = 0; innerIter < bodyBTX.Count; innerIter++)
                                                {
                                                    DateTime operationTime = Convert.ToDateTime(bodyBTX[innerIter].date, CultureInfo.InvariantCulture);
                                                    while (fullWalletData.Keys.Contains(operationTime))
                                                    {
                                                        //CSVData test = result[operationTime];
                                                        //test.fee = 0;
                                                        operationTime = operationTime.AddSeconds(1);
                                                    }
                                                    if (bodyBTX[innerIter].TxCost == 0)
                                                    {
                                                        fullWalletData.Add(operationTime,
                                                            new CSVData
                                                            {
                                                                Wallet = Wallets.walletInfo["Bittrex"][iter],
                                                                Descrtion = Wallets.walletInfoDescription[key][iter],
                                                                fee = bodyBTX[innerIter].TxCost,
                                                                id = bodyBTX[innerIter].TxId,
                                                                income = 0,
                                                                type = txType.txD,
                                                                usdRate = 0,
                                                                value = Math.Round(bodyBTX[innerIter].amount, 8),
                                                                valueConvert = 0,
                                                                dopData = currence + " mining",
                                                                currency = currence,
                                                                walletType="C"
                                                            });

                                                    }
                                                    else
                                                    {
                                                        fullWalletData.Add(operationTime,
                                                                   new CSVData
                                                                   {
                                                                       Wallet = Wallets.walletInfo["Bittrex"][iter],
                                                                       Descrtion = Wallets.walletInfoDescription[key][iter],
                                                                       fee = bodyBTX[innerIter].TxCost,
                                                                       id = bodyBTX[innerIter].TxId,
                                                                       income = 0,
                                                                       type = txType.txW,
                                                                       usdRate = 0,
                                                                       value = Math.Round(-1 * bodyBTX[innerIter].amount, 8),
                                                                       valueConvert = 0,
                                                                       dopData = currence + " Withdraw",
                                                                       currency = currence,
                                                                       walletType = "C"
                                                                   });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                break;
                            case "ETH":
                                for (int iter = 0; iter < Wallets.walletInfo[key].Count(); iter++)
                                {
                                    var bodyETH =  Wallets.getETHOperation(Wallets.walletInfo[key][iter]);
                                    if (bodyETH.Count != 0)
                                    {

                                        for (int innerIter = 0; innerIter < bodyETH.Count; innerIter++)
                                        {
                                            DateTime operationTime = Wallets.ToDateTimeFromUnix(bodyETH[innerIter].time).ToUniversalTime();
                                            while (fullWalletData.Keys.Contains(operationTime))
                                            {
                                                //CSVData test = result[operationTime];
                                                //test.fee = 0;
                                                operationTime = operationTime.AddSeconds(1);
                                            }
                                            if (Wallets.walletInfo[key].Contains(bodyETH[innerIter].to))
                                            {
                                                double temp = Math.Round(Convert.ToDouble((bodyETH[innerIter].gasUsed * bodyETH[innerIter].gasPrice) / 1000000000), 12);
                                                bodyETH[innerIter].fee = temp / 1000000000;
                                                fullWalletData.Add(operationTime,
                                                    new CSVData
                                                    {
                                                        Wallet = Wallets.walletInfo[key][iter],
                                                        Descrtion = Wallets.walletInfoDescription[key][iter],
                                                        fee = 0,
                                                        id = bodyETH[innerIter].hash,
                                                        income = 0,
                                                        type = txType.txD,
                                                        usdRate = 0,
                                                        value = Math.Round(Convert.ToDouble(bodyETH[innerIter].value,
                                                        CultureInfo.InvariantCulture) / 1000000000000000000, 12),
                                                        valueConvert = 0,
                                                        dopData = "ETH mining",
                                                        currency = "ETH",
                                                        walletType = "C"
                                                    });

                                            }
                                            else
                                            {
                                                double temp = Math.Round(Convert.ToDouble((bodyETH[innerIter].gasUsed * bodyETH[innerIter].gasPrice) / 1000000000), 12);
                                                bodyETH[innerIter].fee = temp / 1000000000;
                                                fullWalletData.Add(operationTime,
                                                           new CSVData
                                                           {
                                                               Wallet = Wallets.walletInfo[key][iter],
                                                               Descrtion = Wallets.walletInfoDescription[key][iter],
                                                               fee = bodyETH[innerIter].fee,
                                                               id = bodyETH[innerIter].hash,
                                                               income = 0,
                                                               type = txType.txW,
                                                               usdRate = 0,
                                                               value = -1 * Math.Round(Convert.ToDouble(bodyETH[innerIter].value,
                                                               CultureInfo.InvariantCulture) / 1000000000000000000, 12) - bodyETH[innerIter].fee,
                                                               valueConvert = 0,
                                                               dopData = "ETH Withdraw",
                                                               currency = "ETH",
                                                               walletType = "C"
                                                           });
                                            }
                                        }
                                    }
                                }
                                break;
                            case "LTC":
                                for (int iter = 0; iter < Wallets.walletInfo[key].Count(); iter++)
                                {
                                    var bodyLTC =  Wallets.getLTCOperation(Wallets.walletInfo[key][iter]);
                                    if (bodyLTC.Count != 0)
                                    {
                                        for (int innerIter = 0; innerIter < bodyLTC.Count; innerIter++)
                                        {

                                            DateTime operationTime = Wallets.ToDateTimeFromUnix(bodyLTC[innerIter].time).ToUniversalTime();
                                            while (fullWalletData.Keys.Contains(operationTime))
                                            {
                                                //CSVData test = result[operationTime];
                                                //test.fee = 0;
                                                operationTime = operationTime.AddSeconds(1);
                                            }
                                            if (bodyLTC[innerIter].state)
                                            {
                                                fullWalletData.Add(operationTime,
                                                    new CSVData
                                                    {
                                                        Wallet = Wallets.walletInfo[key][iter],
                                                        Descrtion = Wallets.walletInfoDescription[key][iter],
                                                        fee = 0,
                                                        id = bodyLTC[innerIter].txid,
                                                        income = 0,
                                                        type = txType.txD,
                                                        usdRate = 0,
                                                        value = Math.Round(Convert.ToDouble(bodyLTC[innerIter].value,
                                                        CultureInfo.InvariantCulture), 8),
                                                        valueConvert = 0,
                                                        dopData = "LTC mining",
                                                        currency = "LTC",
                                                        walletType = "C"
                                                    });

                                            }
                                            else
                                            {
                                                fullWalletData.Add(operationTime,
                                                           new CSVData
                                                           {
                                                               Wallet = Wallets.walletInfo[key][iter],
                                                               Descrtion = Wallets.walletInfoDescription[key][iter],
                                                               fee = bodyLTC[innerIter].fee,
                                                               id = bodyLTC[innerIter].txid,
                                                               income = 0,
                                                               type = txType.txW,
                                                               usdRate = 0,
                                                               value = -1 * Math.Round(Convert.ToDouble(bodyLTC[innerIter].value,
                                                               CultureInfo.InvariantCulture), 8),
                                                               valueConvert = 0,
                                                               dopData = "LTC Withdraw",
                                                               currency = "LTC",
                                                               walletType = "C"
                                                           });
                                            }
                                        }
                                    }
                                }
                                break;
                            case "BTC cold wallet":
                                for (int iter = 0; iter < Wallets.walletInfo[key].Count(); iter++)
                                {
                                    var bodyBTC =  Wallets.getBlockchainOperation(Wallets.walletInfo[key][iter]);
                                    if (bodyBTC.Count != 0)
                                    {
                                        for (int innerIter = 0; innerIter < bodyBTC.Count; innerIter++)
                                        {
                                            DateTime operationTime = Wallets.ToDateTimeFromUnix(bodyBTC[innerIter].time).ToUniversalTime();
                                            while (fullWalletData.Keys.Contains(operationTime))
                                            {
                                                //CSVData test = result[operationTime];
                                                //test.fee = 0;
                                                operationTime = operationTime.AddSeconds(1);
                                            }
                                            if (bodyBTC[innerIter].result > 0)
                                            {
                                                fullWalletData.Add(operationTime,
                                                    new CSVData
                                                    {
                                                        Wallet = Wallets.walletInfo[key][iter],
                                                        Descrtion = Wallets.walletInfoDescription[key][iter],
                                                        fee = 0,
                                                        id = bodyBTC[innerIter].hash,
                                                        income = 0,
                                                        type = txType.txD,
                                                        usdRate = 0,
                                                        value = Math.Round(Convert.ToDouble(bodyBTC[innerIter].result,
                                                        CultureInfo.InvariantCulture) / 100000000, 8),
                                                        valueConvert = 0,
                                                        dopData = "BTC mining",
                                                        currency = "BTC",
                                                        walletType = "C"
                                                    });

                                            }
                                            else
                                            {
                                                fullWalletData.Add(operationTime,
                                                           new CSVData
                                                           {
                                                               Wallet = Wallets.walletInfo[key][iter],
                                                               Descrtion = Wallets.walletInfoDescription[key][iter],
                                                               fee = Math.Round(Convert.ToDouble(bodyBTC[innerIter].fee,
                                                        CultureInfo.InvariantCulture) / 100000000, 8),
                                                               id = bodyBTC[innerIter].hash,
                                                               income = 0,
                                                               type = txType.txW,
                                                               usdRate = 0,
                                                               value = Math.Round(Convert.ToDouble(bodyBTC[innerIter].result,
                                                               CultureInfo.InvariantCulture) / 100000000, 8),
                                                               valueConvert = 0,
                                                               dopData = "BTC Withdraw",
                                                               currency = "BTC",
                                                               walletType = "C"
                                                           });
                                            }
                                        }
                                    }
                                }
                                break;

                        }
                    }
                }
                for (int iter = 0; iter < Wallets.walletInfo["Bittrex"].Count(); iter += 2)
                {
                    if (iter + 1 < Wallets.walletInfo["Bittrex"].Count())
                    {
                         Wallets.parseOrderHistory(Wallets.walletInfo["Bittrex"][iter], Wallets.walletInfo["Bittrex"][iter + 1]);
                        if (Wallets.orderHistory.Count != 0)
                        {
                            for (int innerIter = 0; innerIter < Wallets.orderHistory.Count; innerIter++)
                            {
                                DateTime operationTime = Convert.ToDateTime(Wallets.orderHistory[innerIter].Closed, CultureInfo.InvariantCulture);
                                while (fullWalletData.Keys.Contains(operationTime))
                                {
                                    //CSVData test = result[operationTime];
                                    //test.fee = 0;
                                    operationTime = operationTime.AddSeconds(1);
                                }
                                if (Wallets.orderHistory[innerIter].OrderType.Contains("SELL"))
                                {

                                    fullWalletData.Add(operationTime,
                                              new CSVData
                                              {
                                                  Wallet = Wallets.walletInfo["Bittrex"][iter],
                                                  Descrtion = "Test",
                                                  fee = Math.Round(Convert.ToDouble(Wallets.orderHistory[innerIter].Commission, CultureInfo.InvariantCulture), 8),
                                                  id = Wallets.orderHistory[innerIter].OrderUuid,
                                                  income = 0,
                                                  type = txType.orderSell,
                                                  usdRate = 0,
                                                  value = Math.Round(Convert.ToDouble(Wallets.orderHistory[innerIter].Price, CultureInfo.InvariantCulture), 8),
                                                  valueConvert = Math.Round(Convert.ToDouble(Wallets.orderHistory[innerIter].Quantity, CultureInfo.InvariantCulture), 8),
                                                  dopData = Wallets.orderHistory[innerIter].Exchange,
                                                  currency = Wallets.orderHistory[innerIter].Exchange.Substring
                                                  (Wallets.orderHistory[innerIter].Exchange.IndexOf('-') + 1),
                                                  walletType = "H"
                                              });

                                }
                                else
                                {
                                    fullWalletData.Add(operationTime,
                                                    new CSVData
                                                    {
                                                        Wallet = Wallets.walletInfo["Bittrex"][iter],
                                                        Descrtion = "Test",
                                                        fee = Math.Round(Convert.ToDouble(Wallets.orderHistory[innerIter].Commission, CultureInfo.InvariantCulture), 8),
                                                        id = Wallets.orderHistory[innerIter].OrderUuid,
                                                        income = 0,
                                                        type = txType.orderBuy,
                                                        usdRate = 0,
                                                        value = Math.Round(Convert.ToDouble(Wallets.orderHistory[innerIter].Price, CultureInfo.InvariantCulture), 8),
                                                        valueConvert = Math.Round(Convert.ToDouble(Wallets.orderHistory[innerIter].Quantity, CultureInfo.InvariantCulture), 8),
                                                        dopData = Wallets.orderHistory[innerIter].Exchange,
                                                        currency = Wallets.orderHistory[innerIter].Exchange.Substring
                                                        (Wallets.orderHistory[innerIter].Exchange.IndexOf('-') + 1),
                                                        walletType = "H"
                                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.logDebug("Wallet Data " + Convert.ToString(ex));
            }
            
            fullWalletInfoState = false;
            



        }
        //Parse wallet data and create Csv files
        public   void getCSVData(object obj = null)
        {
           
            calculateRestAll();
            //createExcelWalletInfo();
            //Create csv for RS

            string fileName = logRsPath;
            string result="";
            if (!File.Exists(fileName))
            {
                using (var file = File.Create(fileName))
                { }
            }
            Dictionary<string, operParsing> withDrawCountArray = new Dictionary<string, operParsing>();
            List<RsOperationsData> rsData = new List<RsOperationsData>();
            File.WriteAllText(fileName, "", Encoding.UTF8);
            using (FileStream stream = new FileStream(fileName, FileMode.Append))
            {
                int operatID = 0;
                string description = "";
                foreach (DateTime key in fullWalletData.Keys)
                {
                    string line = operatID.ToString();
                   
                    switch (fullWalletData[key].type)
                    {
                        case txType.orderSell:

                            description = "Converting " + Math.Abs(fullWalletData[key].valueConvert).ToString("#0.000000", CultureInfo.InvariantCulture) + " " +
                                fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1) + " in " + Math.Abs(fullWalletData[key].value - Math.Abs(fullWalletData[key].fee)).ToString("#0.000000", CultureInfo.InvariantCulture) + " " +
                                fullWalletData[key].dopData.Remove(fullWalletData[key].dopData.IndexOf('-')) + " commission " + Math.Abs(fullWalletData[key].fee).ToString("#0.000000", CultureInfo.InvariantCulture) + " BTC";
                            
                            line +="," + key.ToString(CultureInfo.InvariantCulture)  +  "," + fullWalletData[key].id + "," + "3" + ","
                                + fullWalletData[key].walletType + "," + fullWalletData[key].Wallet + "," + ((Math.Abs(fullWalletData[key].value)) * 1000000).ToString("#0.00", CultureInfo.InvariantCulture) + ","
                                + fullWalletData[key].dopData.Remove(fullWalletData[key].dopData.IndexOf('-')) + ","
                                + description + ",," + ((fullWalletData[key].rest + Math.Abs(fullWalletData[key].fee)) * 1000000).ToString("#0.00", CultureInfo.InvariantCulture) + "\r\n";

                            line +=(++operatID).ToString()+"," + key.ToString(CultureInfo.InvariantCulture)  +  "," + fullWalletData[key].id + "," + "3" + ","
                                + fullWalletData[key].walletType + "," + fullWalletData[key].Wallet + "," + (Math.Abs(fullWalletData[key].valueConvert) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture) + ","
                                + fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1) + ","
                                + description + ",," + ((fullWalletData[key].restConvert) * 1000000).ToString("#0.00", CultureInfo.InvariantCulture) + "\r\n";

                            line += (++operatID).ToString() + ","  + key.ToString(CultureInfo.InvariantCulture)  +  "," + fullWalletData[key].id + "," + "3" + ","
                                + "K" + "," + fullWalletData[key].Wallet + "," + (Math.Abs(fullWalletData[key].fee) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture) + ","
                                + "BTC" + "," + description + ",C," + (fullWalletData[key].rest * 1000000).ToString("#0.00", CultureInfo.InvariantCulture);
                            break;
                        case txType.orderBuy:
                            description = "Converting " + Math.Abs(fullWalletData[key].value).ToString("#0.000000", CultureInfo.InvariantCulture) + " " +
                                 fullWalletData[key].dopData.Remove(fullWalletData[key].dopData.IndexOf('-')) + " in " + Math.Abs(fullWalletData[key].valueConvert).ToString("#0.000000", CultureInfo.InvariantCulture) + " " +
                                fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1) + " commission " + Math.Abs(fullWalletData[key].fee).ToString("#0.000000", CultureInfo.InvariantCulture) + " BTC";

                            line +="," + key.ToString(CultureInfo.InvariantCulture)  +  "," + fullWalletData[key].id + "," + "3" + ","
                                + fullWalletData[key].walletType + "," + fullWalletData[key].Wallet + "," + ((Math.Abs(fullWalletData[key].value)) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture) + "," 
                                + fullWalletData[key].dopData.Remove(fullWalletData[key].dopData.IndexOf('-')) + ","
                                + description + ",," + ((fullWalletData[key].rest + Math.Abs(fullWalletData[key].fee))* 1000000).ToString("#0.00", CultureInfo.InvariantCulture) + "\r\n";

                            line +=(++operatID).ToString()+"," + key.ToString(CultureInfo.InvariantCulture)  +  "," + fullWalletData[key].id + "," + "3" + ","
                                + fullWalletData[key].walletType + "," + fullWalletData[key].Wallet  + "," + (Math.Abs(fullWalletData[key].valueConvert) * 1000000).ToString("#0.00", CultureInfo.InvariantCulture) + "," 
                                + fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1) + ","
                                + description + ",," + ((fullWalletData[key].restConvert) * 1000000).ToString("#0.00", CultureInfo.InvariantCulture) + "\r\n";

                            line += (++operatID).ToString() + ","  + key.ToString(CultureInfo.InvariantCulture)  +  "," + fullWalletData[key].id + "," + "3" + ","
                                + "K" + "," + fullWalletData[key].Wallet + "," + (Math.Abs(fullWalletData[key].fee) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture) + ","
                                + "BTC" + "," + description + ",C," + ((fullWalletData[key].rest) * 1000000).ToString("#0.00", CultureInfo.InvariantCulture);
                            break;
                        case txType.txD:
                            description = "Deposit " + Math.Abs(fullWalletData[key].value).ToString("#0.000000", CultureInfo.InvariantCulture) + " " +
                                fullWalletData[key].currency  + " commission " + Math.Abs(fullWalletData[key].fee).ToString("#0.000000", CultureInfo.InvariantCulture) + " "+fullWalletData[key].currency;

                                line += "," + key.ToString(CultureInfo.InvariantCulture)  + "," + fullWalletData[key].id + "," + "2" + ","
                                    + fullWalletData[key].walletType + "," + fullWalletData[key].Wallet + "," + ((Math.Abs(fullWalletData[key].value)-Math.Abs(fullWalletData[key].fee)) * 1000000).ToString("#0.00", CultureInfo.InvariantCulture) + ","
                                    + fullWalletData[key].currency + ","
                                    + description + ",," + ((fullWalletData[key].rest + Math.Abs(fullWalletData[key].fee)) * 1000000).ToString("#0.00", CultureInfo.InvariantCulture) + "\r\n";

                                line += (++operatID).ToString() +","+ key.ToString(CultureInfo.InvariantCulture) + "," + fullWalletData[key].id + "," + "2" + ","
                                    + "K" + "," + fullWalletData[key].Wallet + "," + (Math.Abs(fullWalletData[key].fee) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture) + ","
                                    + fullWalletData[key].currency + "," + description + ",S,"+(fullWalletData[key].rest * 1000000).ToString("#0.00", CultureInfo.InvariantCulture);
                            //}
                            break;
                        case txType.txW:
                            if (withDrawCountArray.Keys.Contains(fullWalletData[key].id))
                            {
                                withDrawCountArray[fullWalletData[key].id].withdrawCount++;
                                withDrawCountArray[fullWalletData[key].id].witdrawDates.Add(key);
                            }
                            else
                            {
                                withDrawCountArray.Add(fullWalletData[key].id,new operParsing {withdrawCount=1,
                                    witdrawDates=new List<DateTime>{key}} );
                            }
                            description = "Withdraw " + (Math.Abs(fullWalletData[key].value)-Math.Abs(fullWalletData[key].fee)).ToString("#0.000000", CultureInfo.InvariantCulture) + " " +
                                fullWalletData[key].currency + " commission " + Math.Abs(fullWalletData[key].fee).ToString("#0.000000", CultureInfo.InvariantCulture) + " " + fullWalletData[key].currency;

                                line += ","  + key.ToString(CultureInfo.InvariantCulture)  +  "," + fullWalletData[key].id + "," + "2" + ","
                                + fullWalletData[key].walletType + "," + fullWalletData[key].Wallet + "," + ((Math.Abs(fullWalletData[key].value)-Math.Abs(fullWalletData[key].fee)) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture) + ","
                                + fullWalletData[key].currency + "," + description + ",," + ((fullWalletData[key].rest + Math.Abs(fullWalletData[key].fee)) * 1000000).ToString("#0.00", CultureInfo.InvariantCulture) + "\r\n";

                                line += (++operatID).ToString() + ","  + key.ToString(CultureInfo.InvariantCulture)  +  "," + fullWalletData[key].id + "," + "2" + ","
                                    + "K" + "," + fullWalletData[key].Wallet + "," + (Math.Abs(fullWalletData[key].fee) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture) + ","
                                    + fullWalletData[key].currency + "," + description + ",S," + (fullWalletData[key].rest * 1000000).ToString("#0.00", CultureInfo.InvariantCulture);
                            //}                           
                            //line += "," + fullWalletData[key].dopData + "," + fullWalletData[key].Wallet + "," +
                            //    Math.Round(((Math.Abs(fullWalletData[key].value) + (Math.Abs(fullWalletData[key].fee))) * 1000000), 2).ToString("#.00", CultureInfo.InvariantCulture) + ","
                            //     + key.Day.ToString() + "," + key.Month.ToString() + "," + key.Year.ToString() + ";";
                            break;
                    }
                    if (line != "")
                    {                       
                        line += "\r\n";
                        result+=line.Replace("\r\n","\\");
                        

                        byte[] bytes = Encoding.Default.GetBytes(line);
                        operatID++;
                        stream.Write(bytes, 0, bytes.Count());
                    }
                }
            }
            List<String> parsingResult= result.Split(new char[]{'\\'},StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (string key in withDrawCountArray.Keys)
            {
                if (withDrawCountArray[key].withdrawCount>=1)
                {
                    List<int> depositRow = new List<int>();
                    for (int iter=0;iter<parsingResult.Count;iter++)
                    {
                        if (parsingResult[iter].Contains(key) && parsingResult[iter].Contains(",S,") && parsingResult[iter].Contains(",K,") && parsingResult[iter].Contains(",Withdraw "))
                        {
                            parsingResult[iter] = "";
                            string[] firstString = parsingResult[iter-1].Split(new char[] { ',' }, StringSplitOptions.None);
                            firstString[10] = (Convert.ToDouble(firstString[10], CultureInfo.InvariantCulture) + Math.Abs(Convert.ToDouble(firstString[6], CultureInfo.InvariantCulture)) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture);
                        }
                        if (parsingResult[iter].Contains(key)  && parsingResult[iter].Contains(",Deposit "))
                        {
                            depositRow.Add(iter);
                        }
                    }
                    if (depositRow.Count==0)
                    {
                        string description = "Deposit " + key.ToString(CultureInfo.InvariantCulture);
                        double resultValue = 0;
                        foreach (DateTime dateW in withDrawCountArray[key].witdrawDates )
                        {
                            resultValue += Math.Abs(fullWalletData[dateW].value);
                        }
                        resultValue += Math.Abs(fullWalletData[withDrawCountArray[key].witdrawDates[0]].fee);
                        string line =parsingResult.Count+ "," + withDrawCountArray[key].witdrawDates[0].AddMinutes(5).ToString(CultureInfo.InvariantCulture) + "," + key.ToString(CultureInfo.InvariantCulture) + "," + "2" + ","
                            + "C,InputThisWalletPlease," + (resultValue * 1000000).ToString("#0.00", CultureInfo.InvariantCulture) + ","
                            + fullWalletData[withDrawCountArray[key].witdrawDates[0]].currency + ","
                            + description + ",,";
                        parsingResult.Add(line);
                        line = parsingResult.Count + "," + withDrawCountArray[key].witdrawDates[0].AddMinutes(5).ToString(CultureInfo.InvariantCulture) + "," + key.ToString(CultureInfo.InvariantCulture) + "," + "2" + ","
                            + "K,InputThisWalletPlease," + (Math.Abs(fullWalletData[withDrawCountArray[key].witdrawDates[0]].fee) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture) + ","
                            + fullWalletData[withDrawCountArray[key].witdrawDates[0]].currency + ","
                            + description + ",S,";
                        parsingResult.Add(line);                                                   
                    }
                    if (depositRow.Count == 2)
                    {
                        string [] firstString = parsingResult[depositRow[0]].Split(new char[]{',',';'},StringSplitOptions.None);
                        string[] secondString = parsingResult[depositRow[1]].Split(new char[] { ',', ';' }, StringSplitOptions.None);
                        if (firstString[4]=="K")
                        {
                            
                            firstString[6] = (Math.Abs(fullWalletData[withDrawCountArray[key].witdrawDates[0]].fee) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture);
                            firstString[10] = secondString[10];
                            secondString[6] = (Convert.ToDouble(secondString[6],CultureInfo.InvariantCulture)+ Math.Abs(fullWalletData[withDrawCountArray[key].witdrawDates[0]].fee) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture);
                            secondString[10] = (Convert.ToDouble(secondString[10], CultureInfo.InvariantCulture) + Math.Abs(fullWalletData[withDrawCountArray[key].witdrawDates[0]].fee) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture);
                            firstString[8].Replace("0.000000", firstString[6].Substring(1));
                            secondString[8].Replace("0.000000", firstString[6].Substring(1));
                        }
                        else
                        {
                            secondString[6] = (Math.Abs(fullWalletData[withDrawCountArray[key].witdrawDates[0]].fee) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture);
                            secondString[10] = firstString[10];
                            firstString[6] = (Convert.ToDouble(firstString[6], CultureInfo.InvariantCulture) + Math.Abs(fullWalletData[withDrawCountArray[key].witdrawDates[0]].fee) * 1000000).ToString("#0.00", CultureInfo.InvariantCulture);
                            firstString[10] = (Convert.ToDouble(firstString[10], CultureInfo.InvariantCulture) + Math.Abs(fullWalletData[withDrawCountArray[key].witdrawDates[0]].fee) * -1000000).ToString("#0.00", CultureInfo.InvariantCulture);
                            firstString[8].Replace("0.000000", secondString[6].Substring(1));
                            secondString[8].Replace("0.000000", secondString[6].Substring(1));
                        }
                        parsingResult[depositRow[0]] = String.Join(",", firstString) ;
                        parsingResult[depositRow[1]] = String.Join(",", secondString) ;
                    }
                }
            }
            result = String.Join("\\", parsingResult);

            string[] tempMain = result.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            for (int iter=0;iter<tempMain.Length;iter++)
            {
                string[] temp = tempMain[iter].Split(new char[] { ',' }, StringSplitOptions.None);
                rsData.Add(new RsOperationsData
                {
                    rsID = iter,
                    Id = Convert.ToInt32(temp[0], CultureInfo.InvariantCulture),
                    Date = Convert.ToDateTime(temp[1], CultureInfo.InvariantCulture),
                    TxID = temp[2],
                    Oper_Type = temp[3],
                    Wallet_Type = temp[4],
                    Wallet = temp[5],
                    Sum_Value = temp[6],
                    Currency_Name = temp[7],
                    Description = temp[8],
                    Special_Kind = temp[9],
                    Rest = temp[10]
                });
            }
            Dictionary<string, List<RsOperationsData>> resultDic = new Dictionary<string, List<RsOperationsData>>();
            foreach (RsOperationsData row in rsData)
            {
                if (resultDic.Keys.Contains(row.TxID))
                {
                    resultDic[row.TxID].Add(row);
                }
                else
                {
                    resultDic.Add(row.TxID, new List<RsOperationsData>() { row });
                }
            }
            foreach (List<RsOperationsData> value in resultDic.Values)
            {
                int depositCount = 0, withdrawCount = 0;
                foreach (RsOperationsData row in value)
                {
                    if (row.Oper_Type == "2" && row.Special_Kind == "")
                    {
                        if (row.Description.Contains("Deposit") && !row.Sum_Value.Contains("-"))
                        {
                            depositCount++;
                        }
                        if (row.Description.Contains("Withdraw") && row.Sum_Value.Contains("-"))
                        {
                            withdrawCount++;
                        }
                    }
                }
                if (depositCount > 1 && withdrawCount == 1)
                {
                    RsOperationsData comission = new RsOperationsData();
                    string sum = "";
                    foreach (RsOperationsData row in value)
                    {
                        if (row.Special_Kind == "C")
                        {
                            if (row.Description.Contains("Withdraw"))
                            {
                                comission = row;
                            }
                            if (row.Description.Contains("Deposit"))
                            {
                                sum = row.Sum_Value;
                                rsData.Remove(row);
                            }
                        }
                    }
                    if (comission.Sum_Value != "0.00")
                        comission.Sum_Value = sum;
                }
                foreach (RsOperationsData row in value)
                {
                    if (row.Sum_Value == "0.00")
                    {
                        rsData.Remove(row);
                    }
                }
            }
            //File.WriteAllText(fileName, result.Replace("\\","\r\n"), Encoding.UTF8); 
            //await sentRsOperationData(result);
            Dictionary<string, string> tempDict = new Dictionary<string, string>();
            int id=0;
            rsData = rsData.Where(t => (DateTime.UtcNow - t.Date).TotalDays <= 7).ToList();
            foreach (RsOperationsData row in rsData)
            {
                id++;
                string rowString =id+","+  row.Id + "," + row.Date.Day + "," + row.Date.Month + "," + row.Date.Year + "," + row.TxID + "," + row.Oper_Type + "," + row.Wallet_Type + "," +
                    row.Wallet + "," + row.Sum_Value + "," + row.Currency_Name + "," + row.Description + "," + row.Special_Kind + "," + row.Rest + "\r\n";
                if (tempDict.Keys.Contains(row.TxID))
                {
                    tempDict[row.TxID] += rowString;
                }
                else
                {
                    tempDict[row.TxID] = rowString;
                }
            }
            string s = string.Join("\r\n", tempDict.Select(x => x.Value));
            File.WriteAllText(fileName, s, Encoding.UTF8); 




        }
        //Create excel file
        public void createExcelWalletInfo()
        {
            string fileName = logTestPath;
            if (!File.Exists(fileName))
            {
                using (var file = File.Create(fileName))
                { }
            }
            string[] headers = new string[100];
            headers[0] = "Date";
            headers[1] = "Wallet";
            headers[2] = "Descripton";
            headers[3] = "id";
            headers[4] = "type";
            headers[5] = "fee";
            headers[6] = "dopData";
            int id = 7;
            foreach (string key in Wallets.walletInfo.Keys)
            {
                foreach (string wallet in Wallets.walletInfo[key])
                {
                    if (!headers.Contains(wallet))
                    {
                        headers[id] = wallet;
                        id++;
                    }
                }
            }
            foreach (DateTime key in fullWalletData.Keys)
            {
                if (!headers.Contains(fullWalletData[key].Wallet))
                {
                    headers[id] = fullWalletData[key].Wallet;
                    id++;
                }
            }
            foreach (DateTime key in fullWalletData.Keys)
            {

                if (fullWalletData[key].type == txType.orderBuy || fullWalletData[key].type == txType.orderSell)
                {
                    string tempcurrence = fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1);
                    if (!headers.Contains(tempcurrence))
                    {
                        headers[id] = tempcurrence;
                        id++;
                    }
                }
            }
            headers[id] = "Convert:BTC";
            id++;
            foreach (DateTime key in fullWalletData.Keys)
            {

                if (fullWalletData[key].type == txType.orderBuy || fullWalletData[key].type == txType.orderSell)
                {
                    string tempcurrence = "Convert:" + fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1);
                    if (!headers.Contains(tempcurrence))
                    {
                        headers[id] = tempcurrence;
                        id++;
                    }
                }
            }
            foreach (DateTime key in fullWalletData.Keys)
            {
                if (fullWalletData[key].type != txType.orderBuy && fullWalletData[key].type != txType.orderSell &&
                    !headers.Contains(fullWalletData[key].dopData))
                {
                    headers[id] = fullWalletData[key].dopData;
                    id++;
                }
            }
            headers[id] = "Summary";
            id++;

            File.WriteAllText(fileName, String.Join(",", headers) + "\r\n", Encoding.Default);
            using (FileStream stream = new FileStream(fileName, FileMode.Append))
            {
                foreach (DateTime key in fullWalletData.Keys)
                {
                    string line = key.ToString() + "," + fullWalletData[key].Wallet + ","
                           + fullWalletData[key].Descrtion + "," + fullWalletData[key].id + ","
                            + fullWalletData[key].type + "," + fullWalletData[key].fee.ToString(CultureInfo.InvariantCulture) + ","
                           + fullWalletData[key].dopData;

                    switch (fullWalletData[key].type)
                    {
                        case txType.orderSell:
                            for (int iter = 7; iter < headers.Count(); iter++)
                            {
                                line += ",";
                                if (headers[iter] == "BTC")
                                {
                                    line += (fullWalletData[key].value - fullWalletData[key].fee).ToString(CultureInfo.InvariantCulture);
                                    continue;
                                }
                                else
                                {
                                    if (headers[iter] == "Convert:BTC")
                                    {
                                        line += (-1 * (fullWalletData[key].value)).ToString(CultureInfo.InvariantCulture);
                                        continue;
                                    }
                                }
                                if (headers[iter] == "Summary")
                                {
                                    line += ",=SUM(INDIRECT(\"RC[-" + (id - 7).ToString() + "]:RC[-1]\",FALSE)),INDIRECT(\"RC[-" + (id - 5).ToString() + "]\",FALSE)";
                                    continue;

                                }
                                string mainHeader = fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1);
                                string convertHeader = "Convert:" + fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1);
                                if (headers[iter] == mainHeader)
                                {
                                    line += (-1 * (fullWalletData[key].valueConvert)).ToString(CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    if (headers[iter] == convertHeader)
                                    {
                                        line += ((fullWalletData[key].valueConvert)).ToString(CultureInfo.InvariantCulture);
                                    }
                                }
                            }
                            break;
                        case txType.orderBuy:
                            for (int iter = 7; iter < headers.Count(); iter++)
                            {
                                line += ",";
                                if (headers[iter] == "BTC")
                                {
                                    line += (-1 * (fullWalletData[key].value + fullWalletData[key].fee)).ToString(CultureInfo.InvariantCulture);
                                    continue;
                                }
                                else
                                {
                                    if (headers[iter] == "Convert:BTC")
                                    {
                                        line += (fullWalletData[key].value).ToString(CultureInfo.InvariantCulture);
                                        continue;
                                    }
                                }
                                if (headers[iter] == "Summary")
                                {
                                    line += ",=SUM(INDIRECT(\"RC[-" + (id - 7).ToString() + "]:RC[-1]\",FALSE)),INDIRECT(\"RC[-" + (id - 5).ToString() + "]\",FALSE)";
                                    continue;

                                }
                                string mainHeader = fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1);
                                string convertHeader = "Convert:" + fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1);
                                if (headers[iter] == mainHeader)
                                {
                                    line += ((fullWalletData[key].valueConvert)).ToString(CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    if (headers[iter] == convertHeader)
                                    {
                                        line += (-1 * (fullWalletData[key].valueConvert)).ToString(CultureInfo.InvariantCulture);
                                    }
                                }
                            }
                            break;
                        case txType.txD:
                            for (int iter = 7; iter < headers.Count(); iter++)
                            {
                                line += ",";
                                if (headers[iter] == "Summary")
                                {
                                    line += ",=SUM(INDIRECT(\"RC[-" + (id - 7).ToString() + "]:RC[-1]\",FALSE)),INDIRECT(\"RC[-" + (id - 5).ToString() + "]\",FALSE)";
                                    continue;

                                }
                                if (headers[iter] == (fullWalletData[key].Wallet))
                                {
                                    line += fullWalletData[key].value.ToString();
                                }

                                else
                                {
                                    if (headers[iter] == (fullWalletData[key].dopData))
                                    {
                                        line += (-1 * (fullWalletData[key].value + fullWalletData[key].fee)).ToString();
                                    }
                                }
                            }
                            break;
                        case txType.txW:
                            for (int iter = 7; iter < headers.Count(); iter++)
                            {
                                line += ",";
                                if (headers[iter] == "Summary")
                                {
                                    line += ",=SUM(INDIRECT(\"RC[-" + (id - 7).ToString() + "]:RC[-1]\",FALSE)),INDIRECT(\"RC[-" + (id - 5).ToString() + "]\",FALSE)";

                                    continue;

                                }
                                if (headers[iter] == (fullWalletData[key].Wallet))
                                {
                                    line += fullWalletData[key].value.ToString();
                                }
                                else
                                {
                                    if (headers[iter] == (fullWalletData[key].dopData))
                                    {
                                        line += (-1 * (fullWalletData[key].value + fullWalletData[key].fee)).ToString();
                                    }
                                }
                            }
                            break;
                    }
                    line += "\r\n";
                    byte[] bytes = Encoding.Default.GetBytes(line);
                    stream.Write(bytes, 0, bytes.Count());
                }
                string sumLine = "," + ","
                           + "," + ","
                             + "," + ","
                           + ",";
                for (int iter = 7; iter < headers.Count(); iter++)
                {
                    sumLine += ",=SUM(INDIRECT(\"R[-" + fullWalletData.Keys.Count().ToString() + "]C:R[-1]C\",FALSE))";
                }
                sumLine += "\r\n";
                byte[] bytes1 = Encoding.Default.GetBytes(sumLine);
                stream.Write(bytes1, 0, bytes1.Count());
            }



            fileName = logTestPath2;
            if (!File.Exists(fileName))
            {
                using (var file = File.Create(fileName))
                { }
            }
            File.WriteAllText(fileName, String.Join(";", headers) + "\r\n", Encoding.Default);
            using (FileStream stream = new FileStream(fileName, FileMode.Append))
            {
                foreach (DateTime key in fullWalletData.Keys)
                {
                    string line = key.ToString() + ";" + fullWalletData[key].Wallet + ";"
                           + fullWalletData[key].Descrtion + ";" + fullWalletData[key].id + ";"
                            + fullWalletData[key].type + ";" + fullWalletData[key].fee.ToString(CultureInfo.InvariantCulture) + ";"
                           + fullWalletData[key].dopData;

                    switch (fullWalletData[key].type)
                    {
                        case txType.orderSell:
                            for (int iter = 7; iter < headers.Count(); iter++)
                            {
                                line += ";";
                                if (headers[iter] == "BTC")
                                {
                                    line += (fullWalletData[key].value - fullWalletData[key].fee).ToString(CultureInfo.InvariantCulture);
                                    continue;
                                }
                                else
                                {
                                    if (headers[iter] == "Convert:BTC")
                                    {
                                        line += (-1 * (fullWalletData[key].value)).ToString(CultureInfo.InvariantCulture);
                                        continue;
                                    }
                                }
                                if (headers[iter] == "Summary")
                                {
                                    line += ";=SUM(INDIRECT(\"RC[-" + (id - 7).ToString() + "]:RC[-1]\",FALSE)),INDIRECT(\"RC[-" + (id - 5).ToString() + "]\",FALSE)";
                                    continue;

                                }
                                string mainHeader = fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1);
                                string convertHeader = "Convert:" + fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1);
                                if (headers[iter] == mainHeader)
                                {
                                    line += (-1 * (fullWalletData[key].valueConvert)).ToString(CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    if (headers[iter] == convertHeader)
                                    {
                                        line += ((fullWalletData[key].valueConvert)).ToString(CultureInfo.InvariantCulture);
                                    }
                                }
                            }
                            break;
                        case txType.orderBuy:
                            for (int iter = 7; iter < headers.Count(); iter++)
                            {
                                line += ";";
                                if (headers[iter] == "BTC")
                                {
                                    line += (-1 * (fullWalletData[key].value + fullWalletData[key].fee)).ToString(CultureInfo.InvariantCulture);
                                    continue;
                                }
                                else
                                {
                                    if (headers[iter] == "Convert:BTC")
                                    {
                                        line += (fullWalletData[key].value).ToString(CultureInfo.InvariantCulture);
                                        continue;
                                    }
                                }
                                if (headers[iter] == "Summary")
                                {
                                    line += ";=SUM(INDIRECT(\"RC[-" + (id - 7).ToString() + "]:RC[-1]\",FALSE)),INDIRECT(\"RC[-" + (id - 5).ToString() + "]\",FALSE)";
                                    continue;

                                }
                                string mainHeader = fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1);
                                string convertHeader = "Convert:" + fullWalletData[key].dopData.Substring(fullWalletData[key].dopData.IndexOf('-') + 1);
                                if (headers[iter] == mainHeader)
                                {
                                    line += ((fullWalletData[key].valueConvert)).ToString(CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    if (headers[iter] == convertHeader)
                                    {
                                        line += (-1 * (fullWalletData[key].valueConvert)).ToString(CultureInfo.InvariantCulture);
                                    }
                                }
                            }
                            break;
                        case txType.txD:
                            for (int iter = 7; iter < headers.Count(); iter++)
                            {
                                line += ";";
                                if (headers[iter] == "Summary")
                                {
                                    line += ";=SUM(INDIRECT(\"RC[-" + (id - 7).ToString() + "]:RC[-1]\",FALSE)),INDIRECT(\"RC[-" + (id - 5).ToString() + "]\",FALSE)";
                                    continue;

                                }
                                if (headers[iter] == (fullWalletData[key].Wallet))
                                {
                                    line += fullWalletData[key].value.ToString();
                                }

                                else
                                {
                                    if (headers[iter] == (fullWalletData[key].dopData))
                                    {
                                        line += (-1 * (fullWalletData[key].value + fullWalletData[key].fee)).ToString();
                                    }
                                }
                            }
                            break;
                        case txType.txW:
                            for (int iter = 7; iter < headers.Count(); iter++)
                            {
                                line += ";";
                                if (headers[iter] == "Summary")
                                {
                                    line += ";=SUM(INDIRECT(\"RC[-" + (id - 7).ToString() + "]:RC[-1]\",FALSE)),INDIRECT(\"RC[-" + (id - 5).ToString() + "]\",FALSE)";
                                    continue;

                                }
                                if (headers[iter] == (fullWalletData[key].Wallet))
                                {
                                    line += fullWalletData[key].value.ToString();
                                }
                                else
                                {
                                    if (headers[iter] == (fullWalletData[key].dopData))
                                    {
                                        line += (-1 * (fullWalletData[key].value + fullWalletData[key].fee)).ToString();
                                    }
                                }
                            }
                            break;
                    }
                    line += "\r\n";
                    byte[] bytes = Encoding.Default.GetBytes(line);
                    stream.Write(bytes, 0, bytes.Count());
                }
                string sumLine = ";" + ";"
                           + ";" + ";"
                             + ";" + ";"
                           + ";";
                for (int iter = 7; iter < headers.Count(); iter++)
                {
                    sumLine += ";=Сумм(INDIRECT(\"R[-" + fullWalletData.Keys.Count().ToString() + "]C:R[-1]C\",FALSE))";
                }
                sumLine += "\r\n";
                byte[] bytes1 = Encoding.Default.GetBytes(sumLine);
                stream.Write(bytes1, 0, bytes1.Count());
            }

        }
        //Function for getting usdrate historical data
        public   void GetUsdRate(object obj = null)
        {
            if (fullWalletInfoState != true)
            {
                try
                {
                    foreach (DateTime date in fullWalletData.Keys)
                    {

                        if (fullWalletData[date].type != txType.orderBuy && fullWalletData[date].type != txType.orderSell)
                        {
                            
                            string header = fullWalletData[date].currency + ":" + Wallets.ToUnixFromDateTime(date).ToString() + "000";
                            if (!Wallets.historicalExcRate.Keys.Contains(header))
                            {
                                 Wallets.getUSDrateDate( Wallets.ToUnixFromDateTime(date) * 1000, fullWalletData[date].currency);
                            }
                        }
                        else
                        {
                            try
                            {
                                string header = fullWalletData[date].dopData.Substring(fullWalletData[date].dopData.IndexOf('-') + 1)
                                    + ":" + Wallets.ToUnixFromDateTime(date).ToString() + "000";
                                if (!Wallets.historicalExcRate.Keys.Contains(header))
                                {
                                     Wallets.getUSDrateDate(Wallets.ToUnixFromDateTime(date) * 1000, fullWalletData[date].dopData.Substring(fullWalletData[date].dopData.IndexOf('-')));
                                }
                                header = fullWalletData[date].dopData.Remove(fullWalletData[date].dopData.IndexOf('-'))
                                    + ":" + Wallets.ToUnixFromDateTime(date).ToString() + "000";
                                if (!Wallets.historicalExcRate.Keys.Contains(header))
                                {
                                     Wallets.getUSDrateDate(Wallets.ToUnixFromDateTime(date) * 1000, fullWalletData[date].dopData.Remove(fullWalletData[date].dopData.IndexOf('-')));
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.logDebug("Usd Rate For Orders:" + Convert.ToString(ex));
                            }
                        }
                    }
                    Wallets.saveHistoricalInFile();
                }
                catch (Exception) { }
            }

        }
        #endregion





         



    }
}
