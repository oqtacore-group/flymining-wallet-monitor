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
namespace BitcoinInfoMiner
{
    #region Responce Classes
    public class OrderResponce
    {
        public bool success { get; set; }
        public string message { get; set; }
        public JArray result { get; set; }

    }
    public class OrderBody
    {
        public string OrderUuid { get; set; }
        public string Limit { get; set; }
        public string Closed { get; set; }
        public string Opened { get; set; }
        public string Exchange { get; set; }
        public string OrderType { get; set; }
        public string Quantity { get; set; }
        public double PricePerUnit { get; set; }
        public string Price { get; set; }
        public string Commission { get; set; }
    }
    public class BittrexResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public JToken Result { get; set; }
    }

    public class OperationArray
    {
        public JArray inputs { get; set; }
        public JArray Out { get; set; }
        public string result { get; set; }
        public string time { get; set; }
    }
    public class OperationInput
    {
        public JArray inputs { get; set; }
    }

    #endregion

    #region Classes ForParsing Responce
    public class BittrexBody
    {
        public string date { get; set; }
        public string TxId { get; set; }
        public double amount { get; set; }
        public string address { get; set; }
        public double TxCost { get; set; }
    }
    public class ETHResponce
    {
        public string status { get; set; }
        public JArray result { get; set; }
    }
    public class ETHBody
    {
        public string hash { get; set; }
        public long gasUsed { get; set; }
        public long gasPrice { get; set; }
        public long time { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public long value { get; set; }
        public double fee { get; set; }
    }
    public class LTCResponce
    {
        public string status { get; set; }
        //public double balance { get; set; }
        public JToken data { get; set; }
    }

    public class LTCBody
    {
        public string txid { get; set; }
        public string from { get; set; }
        public long time { get; set; }
        public JArray inputs { get; set; }
        public JArray outputs { get; set; }
        public double value { get; set; }
        public double fee { get; set; }
        public bool state { get; set; }
    }
    public class NiceHashResponce
    {
        public string method { get; set; }
        //public double balance { get; set; }
        public JToken result { get; set; }
    }
    public class NiceHashBody
    {
        public string txid { get; set; }
        public string time { get; set; }
        public string value { get; set; }
        public string fee { get; set; }
        public string type { get; set; }
    }
    public class OperationArrayParse
    {
        public long final_balance { get; set; }
        public JArray inputs { get; set; }
        public JArray Out { get; set; }
        public string result { get; set; }
        public long time { get; set; }
        public string hash { get; set; }
        public int size { get; set; }
    }
    public class OperationOut
    {
        public string addr { get; set; }
        public long value { get; set; }
        public bool spent { get; set; }
    }
    public class OperationInputBody
    {
        public string addr { get; set; }
        public long value { get; set; }
    }
    public class OperationResponse
    {
        public string hash160 { get; set; }
        public string address { get; set; }
        public string n_tx { get; set; }
        public string total_received { get; set; }
        public string total_sent { get; set; }
        public string final_balance { get; set; }
        public JArray txs { get; set; }
    }
    public class OperationAllResponse
    {
        public JArray txs { get; set; }
    }
    public class OperationAllData
    {
        public string hash { get; set; }
        public long fee { get; set; }
        public long result { get; set; }
        public long balance { get; set; }
        public long time { get; set; }
        public JArray inputs { get; set; }
        public JArray Out { get; set; }
        public int size { get; set; }
    }
    public struct OperationInfo
    {
        public long outAllSum;
        public long inputAllSum;
        public long outThisWallet;
        public long inputThisWallet;
        public long outSpent;
    }
    public class TransactionInfo
    {
        public Dictionary<int, String> inputs { get; set; }
        public Dictionary<int, String> Out { get; set; }
        public int size { get; set; }

    }
    #endregion
    public enum txType {orderSell,orderBuy,txD,txW }
    public class CSVData
    {
        public string Wallet { get; set; } //Wallet id
        public txType type { get; set; } //tx Type 
        public string Descrtion { get; set; } //Wallet description
        public double value { get; set; } //value of tx
        public double valueConvert { get; set; } //only for order.
        public double usdRate { get; set; } //usdRate for fee (yep)
        public string id { get; set; } //hash
        public double income { get; set; } // Calculated income. Only for tx
        public double fee { get; set; } //Fee
        public string dopData { get; set; }//Dop data if Order then (BTC-ETH) else mining column
        public string currency { get; set; }// Currency of value
        public string walletType { get; set; }// Currency of value
        public double rest { get; set; }//Остаток на балансе (value)
        public double restConvert { get; set; }//Остаток на балансе для второй части ордера(valueConvert)
        public string outputWallets { get; set; }
    }
    public class OperData
    {
        public string currency { get; set; }
        public double sum { get; set; }
        public DateTime date { get; set; }
    }

    class Wallets
    {
        public static Dictionary<string, double> historicalExcRate;
        public static Dictionary<int, OrderBody> orderHistory;
        public static string bittrexUri = "https://bittrex.com/api/v1.1/";
        public static string LTCUri = "https://chain.so/api/v2/";
        public static string blockChainUri = "https://blockchain.info/";
        public static string ethUri = "http://api.etherscan.io/api";
        public static string NiceHashUri = "https://api.nicehash.com/api";
        public static string stockMarketUri = "https://api.coinmarketcap.com/v1/ticker/";        
        public static string bittrexOldOrderHistory = "BittrexOrders";
        public  static string settingFileName = "logReport//HistoricalRates.txt";
        public static readonly Encoding encoding = Encoding.UTF8;
        private static HttpClient httpClient =new HttpClient();
        public static  Dictionary<string, double> marketInfo;
        public static Dictionary<string, string[]> walletInfo;
        public static Dictionary<string, string[]> walletInfoDescription;
        private static bool parseMarketState = false;
        //Send Bittrex Request without parameters
        public static  string sendBittrexRequest(string command, string apikey, string secret)
        {
            return  sendBittrexRequest(command, apikey, secret, new Dictionary<string, string>());
        }
        private static string byteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
                sbinary += buff[i].ToString("X2"); /* hex format */
            return sbinary;
        }
        //Send Bittrex Request
        public static  string sendBittrexRequest(string command, string apikey, string secret, IDictionary<string, string> addParameters)
        {
            try
            {
                IDictionary<string, string> parameters = new Dictionary<string, string>(addParameters);
                var nonce = DateTime.Now.Ticks;
                parameters.Add("apikey", apikey);
                parameters.Add("nonce", nonce.ToString());
                var parameterString = convertParameterListToString(parameters);
                var completeUri = Wallets.bittrexUri + command + "?" + parameterString;
                var uriBytes = encoding.GetBytes(completeUri);
                var request = new HttpRequestMessage(HttpMethod.Get, completeUri);
                using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret)))
                {
                    var hash = hmac.ComputeHash(uriBytes);
                    var hashText = byteToString(hash);
                    request.Headers.Add("apisign", hashText);
                }
                HttpResponseMessage response = httpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                    return "False";
                var content = response.Content.ReadAsStringAsync().Result;

                return content;
            }
            catch (Exception e)
            {
                Log.logDebug("Bittrex request "+Convert.ToString(e, CultureInfo.InvariantCulture));
                return "False";
            }

        }
        //Send normal Request without args
        public static string sendAsyncRequest(string command, string uri)
        {
            return  sendAsyncRequest(command, uri, new Dictionary<string, string>());
        }
        //Send normal Request 
        public  static string sendAsyncRequest(string command, string uri, IDictionary<string, string> addParameters)
        {
            try
            {
                
                IDictionary<string, string> parameters = new Dictionary<string, string>(addParameters);
                var nonce = DateTime.Now.Ticks;
                var parameterString = convertParameterListToString(parameters);
                var completeUri = uri + command + "?" + parameterString;
                var uriBytes = encoding.GetBytes(completeUri);
                var request = new HttpRequestMessage(HttpMethod.Get, completeUri);
                HttpResponseMessage response = httpClient.SendAsync(request).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                    return "Wait";


                return content;
            }
            catch (Exception e)
            {
                Log.logDebug("Async request "+Convert.ToString(e, CultureInfo.InvariantCulture));
                return "False";
            }

        }
        //Convert List to single string
        private static string convertParameterListToString(IDictionary<string, string> parameters)
        {
            if (parameters.Count == 0) return "";

            return parameters.Select(param => System.Uri.EscapeDataString(param.Key) + "=" + System.Uri.EscapeDataString(param.Value)).Aggregate((l, r) => l + "&" + r);
        }
        //ParsedAll blockchain data
        public static  IList<OperationAllData> getAllBlockchainOperation(string[] wallets)
        {
            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("active", String.Join("|", wallets));
                args.Add("n", "100");

                string temop =  Wallets.sendAsyncRequest("ru/multiaddr", Wallets.blockChainUri, args);
                OperationAllResponse operationResponse = new OperationAllResponse();
                operationResponse = JsonConvert.DeserializeObject<OperationAllResponse>(temop);

                //JArray blogPostArray = JArray.Parse(operationResponse3.txs);

                IList<OperationAllData> mainBodyAll = operationResponse.txs.Select(p => new OperationAllData
                {

                    fee = (long)p["fee"],
                    hash = (string)p["hash"],
                    result = (long)p["result"],
                    time = (long)p["time"],
                    inputs = (JArray)p["inputs"],
                    Out = (JArray)p["out"],
                    balance = (long)p["balance"],
                    size = (int)p["size"]
                }).ToList();
                return mainBodyAll;
            }
            catch (Exception ex)
            {
                Log.logDebug("getAllBlockChainOperation " + Convert.ToString(ex));
                return new List<OperationAllData>();
            }
        }
        //Parse this blockchain wallet
        public static  IList<OperationAllData> getBlockchainOperation(string wallet)
        {
            try
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("active", wallet);
                args.Add("n", "100");

                string temop =  Wallets.sendAsyncRequest("ru/multiaddr", Wallets.blockChainUri, args);
                OperationAllResponse operationResponse = new OperationAllResponse();
                operationResponse = JsonConvert.DeserializeObject<OperationAllResponse>(temop);

                //JArray blogPostArray = JArray.Parse(operationResponse3.txs);

                IList<OperationAllData> mainBodyAll = operationResponse.txs.Select(p => new OperationAllData
                {

                    fee = (long)p["fee"],
                    hash = (string)p["hash"],
                    result = (long)p["result"],
                    time = (long)p["time"],
                    inputs = (JArray)p["inputs"],
                    Out = (JArray)p["out"],
                    balance = (long)p["balance"],
                    size = (int)p["size"]
                }).ToList();
                return mainBodyAll;
            }
            catch (Exception ex)
            {
                Log.logDebug("getBlockChainOperation " + Convert.ToString(ex));
                return new List<OperationAllData>();
            }
        }
        //Get current currence rates
        public static  void parseMarket()
        {
            string json =  Wallets.sendAsyncRequest("", Wallets.stockMarketUri);
            try
            {
                if (!parseMarketState&&json != "False")
                {
                    parseMarketState = true;
                    var marketResponse = JsonConvert.DeserializeObject<JToken>(json);
                    marketInfo = new Dictionary<string, double>();//[bittrexResponse.Result.Count()];
                    for (int iter = 0; iter < marketResponse.Count(); iter++)
                    {
                        if (!marketInfo.Keys.Contains(marketResponse[iter].First.Next.Next.First.ToString()))
                            marketInfo.Add(marketResponse[iter].First.Next.Next.First.ToString(),
                                Convert.ToDouble(marketResponse[iter].First.Next.Next.Next.Next.First.ToString(), CultureInfo.InvariantCulture));
                        //temp[iter] = bittrexResponse.Result[iter].First.Next.ToString();
                    }
                    parseMarketState = false;
                }
            }
            catch (Exception ex)
            {
                parseMarketState = false;
                Log.logDebug("parseMarket "+Convert.ToString(ex));
            }
        }
        //Parse this wallet info
        public static  IList<ETHBody> getETHOperation(string wallet)
        {
            return  getETHOperation(new string[] { wallet });
        }
        //Parse this wallets info
        public static  IList<ETHBody> getETHOperation(string[] wallets)
        {
            try
            {
                IList<ETHBody> mainBodyAll = new List<ETHBody>();
                //foreach (string wallet in wallets)
                //{
                //    Dictionary<string, string> args = new Dictionary<string, string>();
                //    args.Add("module", "account");
                //    args.Add("action", "txlistinternal");
                //    args.Add("address", wallet);
                //    args.Add("sort", "desc");
                //    string temop = await Wallets.sendAsyncRequest("", Wallets.ethUri, args);
                //    ETHResponce operationResponse = new ETHResponce();
                //    operationResponse = JsonConvert.DeserializeObject<ETHResponce>(temop);

                //    //JArray blogPostArray = JArray.Parse(operationResponse3.txs);

                //    IList<ETHBody> mainBody = operationResponse.result.Select(p => new ETHBody
                //    {
                //        hash = (string)p["hash"],
                //        from = (string)p["from"],
                //        //gasPrice = (long)p["gasPrice"],
                //        gasUsed = (long)p["gasUsed"],
                //        time = (long)p["timeStamp"],
                //        to = (string)p["to"],
                //        value = (long)p["value"],

                //    }).ToList();
                //    for (int iter = 0; iter < mainBody.Count;iter++ )
                //        mainBodyAll.Add(mainBody[iter]);
                //}
                foreach (string wallet in wallets)
                {
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args.Add("module", "account");
                    args.Add("action", "txlist");
                    args.Add("address", wallet);
                    args.Add("sort", "desc");
                    string temop =  Wallets.sendAsyncRequest("", Wallets.ethUri, args);
                    ETHResponce operationResponse = new ETHResponce();
                    operationResponse = JsonConvert.DeserializeObject<ETHResponce>(temop);

                    //JArray blogPostArray = JArray.Parse(operationResponse3.txs);

                    IList<ETHBody> mainBody = operationResponse.result.Select(p => new ETHBody
                    {
                        hash = (string)p["hash"],
                        from = (string)p["from"],
                        gasPrice = (long)p["gasPrice"],
                        gasUsed = (long)p["gasUsed"],
                        time = (long)p["timeStamp"],
                        to = (string)p["to"],
                        value = (long)p["value"],

                    }).ToList();
                    for (int iter = 0; iter < mainBody.Count; iter++)
                        mainBodyAll.Add(mainBody[iter]);
                }
                return mainBodyAll;
            }
            catch (Exception ex)
            {
                Log.logDebug("getETHOperation " + Convert.ToString(ex));
                return new List<ETHBody>();
            }
         
        }
        //Parse this wallet info
        public static  IList<LTCBody> getLTCOperation(string wallet)
        {
            return  getLTCOperation(new string[] { wallet });
        }
        //Parse this wallets info
        public static  IList<LTCBody> getLTCOperation(string[] wallets)
        {
            try
            {
                IList<LTCBody> mainBodyAll = new List<LTCBody>();
                foreach (string wallet in wallets)
                {
                    string temop =  Wallets.sendAsyncRequest("address/LTC/" + wallet, Wallets.LTCUri);
                    //operationResponse = new JToken();
                    LTCResponce operationResponse = JsonConvert.DeserializeObject<LTCResponce>(temop);
                    JToken temp = operationResponse.data["txs"];
                    //JArray blogPostArray = JArray.Parse(operationResponse3.txs);
                    IList<LTCBody> mainBody = new List<LTCBody>();
                    for (int iter = 0; iter < temp.Count(); iter++)
                    {
                        if (temp[iter].ToString().Contains("outgoing"))
                        {
                            double tempFee=(double)temp[iter]["outgoing"]["value"];
                            foreach (JToken token in (JArray)temp[iter]["outgoing"]["outputs"])
                            {
                                tempFee -= (double)token["value"];
                            }
                            if (tempFee < 0)
                            {
                                temop =  Wallets.sendAsyncRequest("tx/LTC/" + (string)temp[iter]["txid"], Wallets.LTCUri);
                                operationResponse = JsonConvert.DeserializeObject<LTCResponce>(temop);
                                tempFee = (double)operationResponse.data["sent_value"];
                                foreach (JToken token in (JArray)operationResponse.data["outputs"])
                                {
                                    tempFee -= (double)token["value"];
                                }
                                mainBody.Add(new LTCBody
                                {
                                    time = (long)temp[iter]["time"],
                                    value = (double)operationResponse.data["sent_value"],
                                    txid = (string)temp[iter]["txid"],
                                    outputs = (JArray)temp[iter]["outgoing"]["outputs"],
                                    state = false,
                                    from = wallet,
                                    fee = tempFee

                                });
                                //"https://chain.so/api/v2/"
                                //https://chain.so/api/v2/tx/DOGE/6f47f0b2e1ec762698a9b62fa23b98881b03d052c9d8cb1d16bb0b04eb3b7c5b
                            }
                            else
                            {
                                mainBody.Add(new LTCBody
                                {
                                    time = (long)temp[iter]["time"],
                                    value = (double)temp[iter]["outgoing"]["value"],
                                    txid = (string)temp[iter]["txid"],
                                    outputs = (JArray)temp[iter]["outgoing"]["outputs"],
                                    state = false,
                                    from = wallet,
                                    fee = tempFee

                                });
                            }
                        }
                        else
                        {
                            mainBody.Add(new LTCBody
                            {
                                time = (long)temp[iter]["time"],
                                value = (double)temp[iter]["incoming"]["value"],
                                txid = (string)temp[iter]["txid"],
                                inputs = (JArray)temp[iter]["incoming"]["inputs"],
                                state = true,
                                from = wallet
                                //fee=()

                            });
                        }
                    }
                    for (int iter = 0; iter < mainBody.Count; iter++)
                        mainBodyAll.Add(mainBody[iter]);
                }
                return mainBodyAll;
            }
            catch (Exception ex)
            {
                Log.logDebug("getLTCOperation " + Convert.ToString(ex));
                return new List<LTCBody>();
            }
        }
        //Parse this wallet info
        public static IList<NiceHashBody> getNiceHashOperation(string wallet)
        {
            return  getNiceHashOperation(new string[] { wallet });
        }
        //Parse this wallets info
        public static  IList<NiceHashBody> getNiceHashOperation(string[] wallets)
        {
            try
            {
                IList<NiceHashBody> mainBodyAll = new List<NiceHashBody>();
                foreach (string wallet in wallets)
                {
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args.Add("method", "stats.provider.payments");
                    args.Add("addr", wallet);
                    string temop =  Wallets.sendAsyncRequest("", Wallets.NiceHashUri, args);
                    NiceHashResponce operationResponse = new NiceHashResponce();
                    operationResponse = JsonConvert.DeserializeObject<NiceHashResponce>(temop);

                    //JArray blogPostArray = JArray.Parse(operationResponse3.txs);
                    JArray payments =(JArray) operationResponse.result.First.Next.First;
                    IList<NiceHashBody> mainBody = payments.Select(p => new NiceHashBody
                    {                        
                        fee = (string)p["fee"],
                        time = (string)p["time"],
                        value = (string)p["amount"],                        
                    }).ToList();
                    for (int iter = 0; iter < mainBody.Count; iter++)
                        mainBodyAll.Add(mainBody[iter]);
                }
                return mainBodyAll;
            }
            catch (Exception ex)
            {
                Log.logDebug("getNiceHashOperation " + Convert.ToString(ex));
                return new List<NiceHashBody>();
            }
        }
        //Parse this wallet info
        public static List<BittrexBody> getBittrexOperation(string wallet, string secret, string currency)
        {
            try
            {
                Dictionary<string, string> arg = new Dictionary<string, string>();
                arg.Add("currency", currency);
                string temW = "";
                temW =  Wallets.sendBittrexRequest("account/getwithdrawalhistory",
                     wallet, secret, arg);
                string temD = "";
                temD =  Wallets.sendBittrexRequest("account/getdeposithistory",
                             wallet, secret, arg);
                BittrexResponse operationResponse = JsonConvert.DeserializeObject<BittrexResponse>(temW);
                IList<BittrexBody> mainBodyW = operationResponse.Result.Select(p => new BittrexBody
                {
                    address = (string)p["Address"],
                    amount = (double)p["Amount"] + (double)p["TxCost"],
                    date = (string)p["Opened"],
                    TxCost = (double)p["TxCost"],
                    TxId = (string)p["TxId"]
                }).ToList();
                operationResponse = JsonConvert.DeserializeObject<BittrexResponse>(temD);
                IList<BittrexBody> mainBodyD = operationResponse.Result.Select(p => new BittrexBody
                {
                    address = (string)p["CryptoAddress"],
                    amount = (double)p["Amount"],
                    date = (string)p["LastUpdated"],
                    TxCost = 0,
                    TxId = (string)p["TxId"]
                }).ToList();
                List<BittrexBody> mainBodyAll = new List<BittrexBody>(mainBodyW.Count +
                                    mainBodyD.Count);
                mainBodyAll.AddRange(mainBodyW);
                mainBodyAll.AddRange(mainBodyD);
                return mainBodyAll;
            }
            catch(Exception ex)
            {
                Log.logDebug("getBittrexOperation " + Convert.ToString(ex));
                return new List<BittrexBody>();
            }
        }
        //Find usd rate for this unix date
        public static double getUSDrateDate(long unixDate, string coin = "BTC")
        {

            if (historicalExcRate.Keys.Contains(coin + ':' + Convert.ToString(unixDate, CultureInfo.InvariantCulture)))
            {
                return historicalExcRate[coin + ':' + Convert.ToString(unixDate, CultureInfo.InvariantCulture)];
            }
            else
            {
                try
                {
                    
                    Dictionary<string, string> temp = new Dictionary<string, string>();
                    temp.Add("start", Convert.ToString(unixDate, CultureInfo.InvariantCulture));
                    if (Wallets.ToUnixFromDateTime(DateTime.Now) * 1000 > unixDate + 120000)
                    {
                        temp.Add("end", Convert.ToString(unixDate + 120000, CultureInfo.InvariantCulture));
                    }
                    else
                        temp.Add("end", Convert.ToString(Wallets.ToUnixFromDateTime(DateTime.Now) * 1000, CultureInfo.InvariantCulture));
                    string json = "";
                    if (coin == "BCC")
                        json =  Wallets.sendAsyncRequest("", "https://api.bitfinex.com/v2/candles/trade:1m:tBCHUSD/hist", temp);
                    else
                        json=  Wallets.sendAsyncRequest("", "https://api.bitfinex.com/v2/candles/trade:1m:t" + coin + "USD/hist", temp);
                    if (json == "Wait")
                        return 0;
                    if (json =="[]")
                    {
                        saveMarketRate(Convert.ToString(unixDate, CultureInfo.InvariantCulture), coin, "0");
                        return 0;
                    }
                    string[] real = json.Split(new char[] { '{', '}', '[', ']', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (real.Count() > 3)
                    {
                        saveMarketRate(Convert.ToString(unixDate, CultureInfo.InvariantCulture), coin, real[3]);
                        return Convert.ToDouble(real[3], CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch(Exception ex)
                {
                    Log.logDebug("UsdRate" + Convert.ToString(ex));
                    return 0;
                }
            }
            //textBox1.Text = json;
        }
        //From DateTime to Unix
        public static long ToUnixFromDateTime(DateTime dateTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixDateTime = Convert.ToInt64((dateTime.ToUniversalTime() - epoch).TotalSeconds);
            return unixDateTime;
        }
        //FromUnixto DateTime
        public static DateTime ToDateTimeFromUnix(long unixDateTime)
        {
            var timeSpan = TimeSpan.FromSeconds(unixDateTime);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var utcDateTime = epoch.Add(timeSpan).ToUniversalTime();
            return utcDateTime;
        }
        //Add rate for this name currence for time date
        public static void saveMarketRate(string time, string name, string rate)
        {
            try
            {
                historicalExcRate.Add(name + ':' + time, Math.Round(Convert.ToDouble(rate, CultureInfo.InvariantCulture), 2));
            }
            catch(Exception ex)
            {
                Log.logDebug("saveMarketRate" + Convert.ToString(ex));
            }
        }
        //Save saved usd rates
        public static void saveHistoricalInFile()
        {
            try
            {
                string parsedText = "";
                foreach (string key in historicalExcRate.Keys)
                {
                    parsedText += key + '=' + Convert.ToString(Math.Round(historicalExcRate[key], 2), CultureInfo.InvariantCulture) + "\r\n";
                }
                File.WriteAllText(Directory.GetCurrentDirectory() + "\\" + Wallets.settingFileName, parsedText, Encoding.Unicode);
            }
            catch(Exception ex)
            {
                Log.logDebug("SaveHistorical" + Convert.ToString(ex));
            }
        }
        //Parse saved historical rates
        public  static void parseHistoricalRateHistory()
        {
            try
            {
                historicalExcRate = new Dictionary<string, double>();
                if (File.Exists(Directory.GetCurrentDirectory() + "\\" + Wallets.settingFileName))
                {
                    string parseString = File.ReadAllText(Directory.GetCurrentDirectory() + "\\" + Wallets.settingFileName);
                    string[] settingsArray = parseString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int iter = 0; iter < settingsArray.Length; iter++)
                    {
                        if (settingsArray[iter].IndexOf(':') > 0)
                        {
                            string name = settingsArray[iter].Remove(settingsArray[iter].IndexOf(':'));
                            string time = settingsArray[iter].Substring(settingsArray[iter].IndexOf(':') + 1);
                            if (time.IndexOf('=') > 0)
                            {
                                string rate = time.Substring(time.IndexOf('=') + 1);
                                time = time.Remove(time.IndexOf('='));
                                saveMarketRate(time, name, rate);
                            }
                        }
                    }
                }
                else
                {
                    File.Create(Directory.GetCurrentDirectory() + "\\" + Wallets.settingFileName);
                }
            }            
            catch(Exception ex)
            {
                Log.logDebug("parseHistoricalRateHistory" + Convert.ToString(ex));
            }
        }
        //Parse order history from file
        public static  void parseOrderHistory(string wallet,string secret)
        {
            string fileName = Directory.GetCurrentDirectory() + "//" + Wallets.bittrexOldOrderHistory + wallet + ".csv";
            if (!File.Exists(fileName))
            {
                using (var file = File.Create(fileName))
                { }
            }

            //string[] fileText = records.ToString().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            string[] fileText = File.ReadAllText(fileName, Encoding.Unicode).Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (fileText.Count() > 0)
            {
                Dictionary<string, string[]> result = new Dictionary<string, string[]>();
                string[] columnName = fileText[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int iter = 0; iter < columnName.Count(); iter++)
                    result.Add(columnName[iter], new string[fileText.Count() - 1]);

                for (int iter = 1; iter < fileText.Count(); iter++)
                {
                    string[] columnValue = fileText[iter].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int iterIner = 0; iterIner < columnValue.Count(); iterIner++)
                    {
                        result[columnName[iterIner]][iter - 1] = columnValue[iterIner];
                    }
                }
                Wallets.orderHistory.Clear();
                for (int iter = 0; iter < fileText.Count() - 1; iter++)
                {

                    if (result["Closed"][iter] != null)
                    {
                        DateTime temp = DateTime.ParseExact(result["Closed"][iter], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        Wallets.orderHistory.Add(iter, new OrderBody
                        {
                            Closed = result["Closed"][iter],
                            Exchange = result["Exchange"][iter],
                            OrderType = result["Type"][iter],
                            Quantity = result["Quantity"][iter],
                            PricePerUnit = 0,
                            Price = result["Price"][iter],
                            Commission = result["CommissionPaid"][iter],
                            OrderUuid = result["OrderUuid"][iter],
                            Opened = result["Opened"][iter],
                            Limit = result["Limit"][iter],
                        });
                    }
                }
            }
             parseWallets(wallet,secret);

        }
        //Parse order history from api
        public static  void parseWallets(string wallet,string secret)
        {
            string responce = "";
            responce =  Wallets.sendBittrexRequest("account/getorderhistory",
                         wallet, secret);

            OrderResponce operationResponse = new OrderResponce();
            operationResponse = JsonConvert.DeserializeObject<OrderResponce>(responce);
            if (operationResponse.success != false)
            {
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
                    try
                    {
                        mainBodyAll[iter].Closed = DateTime.ParseExact(mainBodyAll[iter].Closed, "MM'/'dd'/'yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        mainBodyAll[iter].Opened = DateTime.ParseExact(mainBodyAll[iter].Opened, "MM'/'dd'/'yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                    }
                    catch(Exception ex)
                    {
                        Log.logDebug("parse Wallets Date Converion:" + Convert.ToString(ex));
                    }
                    for (int i = 0; i < Wallets.orderHistory.Count; i++)
                    {
                        if (Wallets.orderHistory[i].Closed == mainBodyAll[iter].Closed)
                            check = true;
                    }
                    if (!check)
                    {
                        try
                        {
                            Wallets.orderHistory.Add(Wallets.orderHistory.Count, mainBodyAll[iter]);
                        }
                        catch (Exception ex)
                        {
                            Log.logDebug("parseWallets " + Convert.ToString(ex));
                            check = true;
                        }
                    }


                }
            }
            
        }
        //Parse Setting to get descriptions
        public static void getWalletsDescriptions()
        {
            foreach (string key in walletInfo.Keys)
            {
                walletInfoDescription.Add(key,new string[walletInfo[key].Count()]);
                for(int iter=0;iter<walletInfo[key].Count();iter++)
                {
                    if (walletInfo[key][iter].IndexOf(":")>=0)
                    {                           
                        walletInfoDescription[key][iter]=walletInfo[key][iter].Remove(walletInfo[key][iter].IndexOf(":"));
                        walletInfo[key][iter] = walletInfo[key][iter].Substring(walletInfo[key][iter].IndexOf(":") + 1);
                        //settingsArray[iter].Substring(settingsArray[iter].IndexOf('=') + 1)
                    }
                    else
                    {
                        walletInfoDescription[key][iter] = "";
                    }
                }
            }
        }

    }

}
