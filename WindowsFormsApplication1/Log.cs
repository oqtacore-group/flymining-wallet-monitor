using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
namespace BitcoinInfoMiner
{
    class Log
    {
        public static readonly string logDebugPath = Directory.GetCurrentDirectory() + "\\logDebug\\logDebug.txt";
        public static readonly string logReportPath = Directory.GetCurrentDirectory() + "\\logReport\\logReport.txt";
        public static readonly string logReportDir = Directory.GetCurrentDirectory() + "\\logReport\\";
        public static readonly string log2ReportPath = Directory.GetCurrentDirectory() + "\\logReport\\logReport2.txt";
        public static readonly string logArchivePath = Directory.GetCurrentDirectory() + "\\logReport\\Archive\\";
        public static Dictionary<string, string> ipSituation=new Dictionary<string,string>();
        public static string flyMiningUserName="";
        public static string flyMiningPassword="REDACTED";

        public static  string[] emailForBadStatus;//Эмейлы на который будут отправлятся отчеты
        public static void logDebug(string msg)
        {

                if (!msg.Contains("\n\r"))
                    msg += "\n\r";
                File.AppendAllText(logDebugPath, msg, Encoding.Unicode);
        }
        public static void logDebug2(string msg)
        {
            if (!msg.Contains("\n\r"))
                msg += "\n\r";
            File.AppendAllText(log2ReportPath, msg, Encoding.Unicode);
        }
        ////Report log
        //public static void logReport(string msg)
        //{
        //    if (!msg.Contains("\n\r"))
        //        msg += "\n\r";
        //    File.AppendAllText(logReportPath, msg, Encoding.Unicode);
        //}
        //Report by Row 
        public static void logReport(string msg, DataGridViewRow row)
        {
            
            if (!msg.Contains("\n\r"))
                msg += "\n\r";
            if (!ipSituation.Keys.Contains((string)row.Cells["IP"].Value))
                ipSituation.Add((string)row.Cells["IP"].Value, msg);
            else
                ipSituation[(string)row.Cells["IP"].Value] += msg;
            msg = DateTime.Now.ToString() + " Problem with " + (string)row.Cells["IP"].Value + ":" + msg;
            File.AppendAllText(logReportPath, msg, Encoding.Unicode);
        }
        //Archiving log
        public static void logArchive()
        {
            if (new FileInfo(logReportPath).Length != 0)
            {
                parseReportFile();
                string archivePath = logArchivePath + "logArchive" + DateTime.Now.ToString("dd.MM.yyyy") + ".txt";
                if (File.Exists(archivePath))
                {
                    File.AppendAllText(archivePath, File.ReadAllText(logReportPath), Encoding.Unicode);
                }
                else
                {
                    File.Move(logReportPath, archivePath);
                }
                File.Create(logReportPath);
            }
        }
        public static void logArchive(string parsedString)
        {
            if (parsedString != "")
            {
                sendEmail(parsedString);
                string archivePath = logArchivePath + "logArchive" + DateTime.Now.ToString("dd.MM.yyyy") + ".txt";
                if (File.Exists(archivePath))
                {
                    File.AppendAllText(archivePath, File.ReadAllText(logReportPath), Encoding.Unicode);
                }
                else
                {
                    File.Move(logReportPath, archivePath);
                }
                File.Create(logReportPath);
            }

        }
        public static void logJustArchive()
        {
                //sendEmail(parsedString);
                string archivePath = logArchivePath + "logArchive" + DateTime.Now.ToString("dd.MM.yyyy") + ".txt";
                if (File.Exists(archivePath))
                {
                    File.AppendAllText(archivePath, File.ReadAllText(logReportPath), Encoding.Unicode);
                }
                else
                {
                    File.Move(logReportPath, archivePath);
                }
                File.Create(logReportPath);

        }
        public static  void sendEmail(string msg)
        {
            //MailMessage mail = new MailMessage("you@yourcompany.com", "user@hotmail.com");
            //SmtpClient client = new SmtpClient();
            //client.Port = 587;
            //client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //client.UseDefaultCredentials = false;
            //client.Host = "smtp.gmail.com";
            //mail.Subject = "this is a test email.";
            //mail.Body = "this is my test email body";
            //client.Send(mail);
            try
            {
                for (int iter = 0; iter < emailForBadStatus.Length; iter++)
                {
                    using (var client = new HttpClient())
                    {
                        var values = new Dictionary<string, string>
                        {
                        { "msg", msg}
                        };
                        var content = new FormUrlEncodedContent(values);
                        try
                        {
                            var response =  client.PostAsync("http://api.flysecure.ru/test/SendReportmailPost?id=" + Log.flyMiningUserName + "&key=" + Log.flyMiningPassword+ "&email=" + emailForBadStatus[iter], content);
                            //var response = await client.PostAsync("api.Flysecure.ru//Test/getTableData?id=" + flyMiningUserName + "&key=" + flyMiningPassword, content);
                            var responseString = response.Status;

                        }
                        catch (Exception ex)
                        {
                            Log.logDebug("Table data send : " + Convert.ToString(ex));
                        }
                        //await client.GetAsync("http://api.flysecure.ru/home/SendReportmail?email=" + emailForBadStatus[iter] + "&msg=" + msg);
                    }
                    //MailMessage mail = new MailMessage();
                    //mail.From = new MailAddress("info@flystat.ru");
                    //mail.To.Add(new MailAddress(emailForBadStatus[iter]));
                    //mail.Subject = "Flymining Report";
                    //mail.Sender = new MailAddress("info@flystat.ru", emailForBadStatus[iter]);
                    //mail.BodyEncoding = System.Text.Encoding.UTF8;
                    //mail.SubjectEncoding = System.Text.Encoding.UTF8;
                    ////System.Net.Mail.Attachment attachment;
                    ////attachment = new System.Net.Mail.Attachment(logReportPath);
                    ////mail.Attachments.Add(attachment);
                    //mail.Body = msg;
                    //mail.IsBodyHtml = true;

                    //SmtpClient client = new SmtpClient();
                    //client.Host = "212.24.53.144";
                    //client.Port = 587;
                    ////client.EnableSsl = (useSSL == "yes" ? true : false);
                    //client.UseDefaultCredentials = false;
                    //client.Credentials = new NetworkCredential("REDACTED", "REDACTED");
                    //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //client.Send(mail);
                    //mail.Dispose();
                }
            }
            catch (Exception e)
            {
                logDebug("Send Mail "+Convert.ToString(e, CultureInfo.InvariantCulture));
            }

        }
        public static void parseReportFile()
        {
            string[] parseArray = File.ReadAllText(logReportPath, Encoding.Unicode).Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<String, int> parsedReport = new Dictionary<string, int>();
            for (int iter = 0; iter < parseArray.Length; iter++)
            {
                if (parseArray[iter].IndexOf("Problem with") > 0)
                {
                    string parsedString = parseArray[iter].Substring(parseArray[iter].IndexOf("Problem with"));
                    if (!parsedReport.Keys.Contains(parsedString))
                    {
                        parsedReport[parsedString] = 0;
                    }
                    else
                    {
                        parsedReport[parsedString]++;
                    }
                }
            }
            //if (!File.Exists(logReportPath+".txt"))
            //    File.Create(logReportPath + ".txt");
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, int> pair in parsedReport)
            {
                if (pair.Value >= 5)
                {
                    string parsedStart = "", parsedEnd = "";
                    for (int iter = 0; iter < parseArray.Length; iter++)
                    {
                        if (parseArray[iter].Contains(pair.Key) && parsedStart == "")
                        {
                            parsedStart = parseArray[iter].Remove(parseArray[iter].IndexOf(pair.Key));
                        }
                        else
                        {
                            if (parseArray[iter].Contains(pair.Key))
                            {
                                parsedEnd = parseArray[iter].Remove(parseArray[iter].IndexOf(pair.Key));
                            }
                        }
                    }
                    builder.Append(pair.Key).Append(":").Append('\n').Append(parsedStart).Append(" - ").Append(parsedEnd).Append('\n');
                }
            }
            string result = builder.ToString();
            File.WriteAllText(logReportPath, result, Encoding.Unicode);

        }
    }
}
