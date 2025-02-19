using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BitcoinInfoMiner
{
    public class MinerModel
    {
        public string ip;
        public string status;
        public string type;
        public string hashRateRT;
        public string hashRateAverage;
        public string temperature;
        public string fanSpeed;
        public string elapsed;
        public string pool1;
        public string pool2;
        public string pool3;
        public string worker1;
        public string worker2;
        public string worker3;

        public MinerModel()
        {
            ip="";
            status = "";
            type = "";
            hashRateRT = "";
            hashRateAverage = "";
            temperature = "";
            fanSpeed = "";
            elapsed = "";
            pool1 = "";
            worker1 = "";
            pool2 = "";
            worker2 = "";
            pool3 = "";
            worker3 = "";
        }
    }

    class MinerDB
    {
    }
}
