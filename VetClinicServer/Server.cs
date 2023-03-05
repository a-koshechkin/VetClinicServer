using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Web;

namespace VetClinicServer
{
    public sealed class Server
    {
        public bool IsRunning { get; private set; } = false;

        private int timeout = 8;
        private Encoding encoderUTF8 = Encoding.UTF8;
        private Socket serverSocket;

        public bool Start(IPAddress ipAddress, int port, int maxNOfCon)
        {
            if (IsRunning)
                return false;

            try
            {
                // tcp/ip сокет (ipv4)
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(ipAddress, port));
                serverSocket.Listen(maxNOfCon);
                serverSocket.ReceiveTimeout = timeout;
                serverSocket.SendTimeout = timeout;
                IsRunning = true;
                Logger.Log($"Server started on port: {port}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Server start exception. Reason: {ex.Message}", LogType.Error);
                return false;
            }
            // Наш поток ждет новые подключения и создает новые потоки.
            Thread requestListenerT = new Thread(() =>
            {
                while (IsRunning)
                {
                    Socket clientSocket;
                    try
                    {
                        clientSocket = serverSocket.Accept();
                        // Создаем новый поток для нового клиента и продолжаем слушать сокет.
                        Thread requestHandler = new Thread(() =>
                        {
                            clientSocket.ReceiveTimeout = timeout;
                            clientSocket.SendTimeout = timeout;
                            try { HandleMessage(clientSocket); }
                            catch
                            {
                                try { clientSocket.Close(); } catch { }
                            }
                        });
                        requestHandler.Start();
                    }
                    catch { }
                }
            });
            requestListenerT.Start();
            return true;
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                try { serverSocket.Close(); }
                catch { }
                serverSocket = null;
                Logger.Log("Server stopped");
            }
        }

        private void HandleMessage(Socket clientSocket)
        {
            byte[] buffer = new byte[10240]; // 10 kb, just in case
            int receivedBCount = clientSocket.Receive(buffer); // Получаем запрос
            string strReceived = encoderUTF8.GetString(buffer, 0, receivedBCount);

            // Парсим запрос
            string httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));
            var connectorError = DatabaseConnector.GetErrorString();
            if (!string.IsNullOrEmpty(connectorError))
            {
                SendResponse(clientSocket, Response.InternalError(connectorError));
            }
            else if (httpMethod.Equals("GET") || httpMethod.Equals("POST"))
                SendResponse(clientSocket, ParseMessage(strReceived));
            else
                SendResponse(clientSocket, Response.NotImplemented());
        }

        private Response ParseMessage(string messageData)
        {
            //get the message data (last line)
            string messageForm = string.Empty;
            var messageStringsArray = messageData.Split('\n');
            if (messageStringsArray.Length > 1)
                messageForm = messageStringsArray[messageStringsArray.Length - 1];

            if (StringExtensions.IsNullOrWhiteSpace(messageForm))
            {
                var response = Response.BadRequest("Empty message");
                Logger.Log(response.StatusString, LogType.Error);
                return response;
            }


            //divide form type and encoded data from message data
            try
            {
               
                
                //handle data
                if (!StringExtensions.IsNullOrWhiteSpace(messageForm))
                {

                    try
                    {
                        var requestData = JsonConvert.DeserializeObject<RequestData>(messageForm);
                        //send all scenes data for admin
                        if (requestData.RequestType == "Request")
                        {
                            return ProcessRequest(requestData);
                        }
                        //error
                        else
                        {
                            var response = Response.BadRequest($"Request {requestData.RequestType} was not recognised");
                            Logger.Log(response.StatusString, LogType.Error);
                            return response;
                        }
                    }
                    catch (Exception ex)
                    {
                        var response = Response.BadRequest($"Deserialisation error. Reason: {ex.Message}");
                        Logger.Log(response.StatusString, LogType.Error);
                        return response;
                    }
                }
                else
                {
                    var response = Response.BadRequest("Empty message type");
                    Logger.Log(response.StatusString, LogType.Error);
                    return response;
                }

            }
            catch (Exception ex)
            {
                var response = Response.BadRequest($"Decrypt error. Reason: {ex.Message}");
                Logger.Log(response.StatusString, LogType.Error);
                return response;
            }
        }

        private static Response ProcessRequest(RequestData requestData)
        {
            try
            {
                return DatabaseConnector.ProcessRequest(requestData);
            }
            catch (Exception ex)
            {
                var response = Response.BadRequest($"Request error: {ex.Message}");
                Logger.Log(response.StatusString, LogType.Error);
                return response;
            }
        }

        private void SendResponse(Socket clientSocket, Response response)
        {
            try
            {
                byte[] bContent = encoderUTF8.GetBytes(response.StatusString);
                byte[] bHeader = encoderUTF8.GetBytes(
                                    "HTTP/1.1 " + response.StatusCode + "\r\n"
                                  + "Server: Learning Game Constructor Web Server\r\n"
                                  + "Access-Control-Allow-Credentials: true\r\n"
                                  + "Access-Control-Allow-Headers: Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time\r\n"
                                  + "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n"
                                  + "Access-Control-Allow-Origin: *\r\n"
                                  + "Content-Length: " + bContent.Length.ToString() + "\r\n"
                                  + "Connection: close\r\n"
                                  + "Content-Type: text" + "\r\n\r\n");
                clientSocket.Send(bHeader);
                clientSocket.Send(bContent);
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                Logger.Log($"Response sendind error. Reason: {ex.Message}");
            }
        }
    }
}
