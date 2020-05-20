﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace YoutubeGameBarWidget.WebServer
{
    class WebServer : IDisposable
    {
        private const uint bufferSize = 8192;
        private StreamSocketListener listener;
        private StorageFolder videoUIDirectory = Windows.ApplicationModel.Package.Current.InstalledLocation;

        public void Dispose()
        {
            this.listener.Dispose();
        }

        public WebServer()
        {
            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += (sender, eventArgs) => ProcessRequestAsync(eventArgs.Socket);
            this.listener.BindServiceNameAsync("54523");
        }

        public async void ProcessRequestAsync(StreamSocket socket)
        {
            StringBuilder request = new StringBuilder();

            using (IInputStream input = socket.InputStream)
            {
                byte[] data = new byte[bufferSize];
                IBuffer buffer = data.AsBuffer();
                uint dataRead = bufferSize;

                while (dataRead == bufferSize)
                {
                    await input.ReadAsync(buffer, bufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }

            using (IOutputStream output = socket.OutputStream)
            {
                string requestMethod = request.ToString().Split('\n')[0];
                string[] requestParts = requestMethod.Split(' ');

                if (requestParts[0] == "GET") { await WriteResponseAsync(requestParts[1], output); }
                else { throw new InvalidDataException("Request Method not supported: " + requestParts[0]); }
            }
        }

        private async Task WriteResponseAsync(string path, IOutputStream output)
        {
            using (Stream response = output.AsStreamForWrite())
            {
                bool exists = true;
                string filePath = "VideoUI\\";
                byte[] headerArray;

                try
                {
                    if(Regex.IsMatch(path, "/[?](.+)"))
                    {
                         filePath += "index.html";
                    }
                    else
                    {
                        filePath += path.Replace('/', '\\');
                    }
                    
                    using (Stream fs = await videoUIDirectory.OpenStreamForReadAsync(filePath))
                    {
                        string headerString = String.Format("HTTP/1.1 200 OK\r\n" +
                                                      "Content-Length: {0}\r\n" +
                                                      "Connection: close\r\n\r\n",
                                                      fs.Length);
                        headerArray = Encoding.UTF8.GetBytes(headerString);
                        await response.WriteAsync(headerArray, 0, headerArray.Length);
                        await fs.CopyToAsync(response);
                    }
                }
                catch (FileNotFoundException)
                {
                    exists = false;
                    headerArray = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n" +
                                                         "Content-Length:0\r\n" +
                                                         "Connection: close\r\n\r\n");
                    await response.WriteAsync(headerArray, 0, headerArray.Length);
                }

                await response.FlushAsync();
            }
        }

    }
}
