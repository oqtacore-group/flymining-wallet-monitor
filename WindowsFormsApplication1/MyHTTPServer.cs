using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace BitcoinInfoMiner
{
    public class HttpServer
    {
        #region Fields

        private int Port;
        private TcpListener Listener;
        private bool IsActive = true;

        #endregion
        
        private string _defaultResponse = "<loading>";

        public void SetDefaultResponse(string newResponse)
        {
            _defaultResponse = newResponse;
        }

        public string GetDefaultResponse()
        {
            return _defaultResponse;
        }

        #region Public Methods
        public HttpServer(int port)
        {
            this.Port = port;
        }

        public void Listen()
        {
            this.Listener = new TcpListener(IPAddress.Any, this.Port);
            this.Listener.Start();
            while (this.IsActive)
            {
                try
                {
                    TcpClient s = this.Listener.AcceptTcpClient();
                    Thread thread = new Thread(() =>
                    {
                        this.HandleClient(s);
                    });
                    thread.Start();
                    Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    Log.logDebug("Listen " + Convert.ToString(ex));
                }           
            }
        }
        public void Close()
        {
            //this.IsActive = false;
            
           
            this.Listener.Stop();
           
        }

        #endregion

        #region Fields

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB
        

        #endregion
        

        #region Public Methods

        public void HandleClient(TcpClient tcpClient)
        {
            Stream inputStream = GetInputStream(tcpClient);
            Stream outputStream = GetOutputStream(tcpClient);

            // route and handle the request...
            HttpResponse response = new HttpResponse
            {
                StatusCode = "200",
                Content = Encoding.ASCII.GetBytes(GetDefaultResponse()),
                ContentAsUTF8 = GetDefaultResponse()
            };

            Console.WriteLine("{0} {1}", response.StatusCode, "");
            // build a default response for errors
            if (response.Content == null)
            {
                if (response.StatusCode != "200")
                {
                    response.ContentAsUTF8 = string.Format("{0} {1} <p> {2}", response.StatusCode, "", response.ReasonPhrase);
                }
            }

            WriteResponse(outputStream, response);

            //outputStream.Flush();
            //outputStream.Close();
            //outputStream = null;

            //inputStream.Close();
            //inputStream = null;

        }

        #endregion

        // this formats the HTTP response...
        private static void WriteResponse(Stream stream, HttpResponse response)
        {
            if (response.Content == null)
            {
                response.Content = new byte[] { };
            }
            

            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type"))
            {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.Content.Length.ToString();

            Write(stream, string.Format("HTTP/1.0 {0} {1}\r\n", response.StatusCode, response.ReasonPhrase));
            Write(stream, string.Join("\r\n", response.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            Write(stream, "\r\n\r\n");

            stream.Write(response.Content, 0, response.Content.Length);
        }
        


        #region Private Methods

        private static string Readline(Stream stream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = stream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        private static void Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        protected virtual Stream GetOutputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected virtual Stream GetInputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        #endregion
    }
}

