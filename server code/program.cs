using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace SecureBankSystem
{
    class Server
    {
 
        private const string IP_ADDRESS = "127.0.0.1";
        private const int TCP_PORT = 8000;
        private const int UDP_PORT = 8001;
        private static decimal globalBalance = 00.00m; 
        private static readonly object balanceLock = new object();
        private static readonly object logLock = new object();
        private static readonly string logFilePath = "server_log.txt";
        private const string AUTH_USER = "admin";
        private const string AUTH_PASS = "123";

        static void Main(string[] args)
        {
          
            Console.WriteLine(" SecureBank Transaction & Support System");
            Console.WriteLine("==================================================\n");

           
            Thread udpThread = new Thread(StartUdpListener) { IsBackground = true };
            udpThread.Start();
            Socket tcpServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Parse(IP_ADDRESS), TCP_PORT);

            try
            {
                tcpServerSocket.Bind(tcpEndPoint);
                tcpServerSocket.Listen(10); 
                Console.WriteLine($"[TCP SERVER] Started successfully. Listening on {IP_ADDRESS}:{TCP_PORT}...");

                while (true)
                {
                    Socket clientSocket = tcpServerSocket.Accept();
                    Console.WriteLine($"[TCP SERVER] Client connected from: {clientSocket.RemoteEndPoint}");
     Thread clientThread = new Thread(() => HandleTcpClient(clientSocket));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP SERVER ERROR] {ex.Message}");
            }
            finally
            {
                
                tcpServerSocket.Close();
            }
        }

        private static void HandleTcpClient(Socket clientSocket)
        {
            string endpointStr = clientSocket.RemoteEndPoint.ToString();
            byte[] buffer = new byte[1024]; 
            bool isAuthenticated = false;   

            try
            {
                string welcomeMsg = "AUTH_REQUIRED: Please login using 'LOGIN username password'\n";
                clientSocket.Send(Encoding.ASCII.GetBytes(welcomeMsg));

                while (true)
                {
                    int receivedLength = clientSocket.Receive(buffer);

                    if (receivedLength == 0)
                    {
                        Console.WriteLine($"[TCP SERVER] Client {endpointStr} disconnected gracefully.");
                        break;
                    }

                    string rawRequest = Encoding.ASCII.GetString(buffer, 0, receivedLength).Trim();
                    Console.WriteLine($"[TCP SERVER] Received from {endpointStr}: \"{rawRequest}\"");

                    string responseMessage;

                    if (!isAuthenticated)
                    {
                        if (rawRequest.ToUpper().StartsWith("LOGIN "))
                        {
                            string[] parts = rawRequest.Split(' ');
                            if (parts.Length == 3 && parts[1] == AUTH_USER && parts[2] == AUTH_PASS)
                            {
                                isAuthenticated = true;
                                responseMessage = "AUTH_SUCCESS: Welcome to SecureBank. Commands: BALANCE, DEPOSIT:amt, WITHDRAW:amt, CHAT:msg, HELP\n";
                            }
                            else
                            {
                                responseMessage = "AUTH_FAILED: Invalid username or password. Try again.\n";
                            }
                        }
                        else
                        {
                            responseMessage = "AUTH_REQUIRED: You must log in first. Format: LOGIN username password\n";
                        }
                    }
                    else
                    {
                        responseMessage = ProcessTcpRequest(rawRequest) + "\n";
                    }

                    byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);
                    clientSocket.Send(responseBytes);

                    LogToText($"Client {endpointStr} -> Request: [{rawRequest}] | Response: [{responseMessage.Trim()}]");
                }
            }
            catch (SocketException)
            {
                Console.WriteLine($"[TCP SERVER] Client {endpointStr} connection lost unexpectedly.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP SERVER ERROR] Exception while handling client {endpointStr}: {ex.Message}");
            }
            finally
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
                catch { /* تجاهل الخطأ إذا كانت السوكيت مغلقة بالفعل */ }

                clientSocket.Close();
                Console.WriteLine($"[TCP SERVER] Cleaned up resources for client {endpointStr}.");
            }
        }

        private static string ProcessTcpRequest(string request)
        {
            string cmd = request.Trim();
            string upperCmd = cmd.ToUpper();


            if (upperCmd == "BALANCE")
            {
                lock (balanceLock)
                {
                    return $"SUCCESS: Your current balance is ${globalBalance}";
                }
            }

            if (upperCmd.StartsWith("DEPOSIT:"))
            {
                string[] parts = cmd.Split(':');
                if (parts.Length == 2 && decimal.TryParse(parts[1], out decimal amount) && amount > 0)
                {
                    lock (balanceLock)
                    {
                        globalBalance += amount;
                        return $"SUCCESS: Deposited ${amount}. New Balance: ${globalBalance}";
                    }
                }
                return "ERROR: Invalid deposit format or amount. Use 'DEPOSIT:amount'";
            }

            if (upperCmd.StartsWith("WITHDRAW:"))
            {
                string[] parts = cmd.Split(':');
                if (parts.Length == 2 && decimal.TryParse(parts[1], out decimal amount) && amount > 0)
                {
                    lock (balanceLock)
                    {
                        if (globalBalance >= amount)
                        {
                            globalBalance -= amount;
                            return $"SUCCESS: Withdrew ${amount}. New Balance: ${globalBalance}";
                        }
                        else
                        {
                            return $"ERROR: Insufficient funds. Transaction denied. Current Balance: ${globalBalance}";
                        }
                    }
                }
                return "ERROR: Invalid withdraw format or amount. Use 'WITHDRAW:amount'";
            }






            if (upperCmd.StartsWith("CHAT:") || upperCmd.Contains("HELP") || upperCmd.Contains("HOURS") || upperCmd.Contains("LOAN"))
            {
                string chatContent = upperCmd.StartsWith("CHAT:") ? upperCmd.Substring(5) : upperCmd;

                if (chatContent.Contains("HELP"))
                {
                    return "CHATBOT: Available operations are [BALANCE], [DEPOSIT:value], [WITHDRAW:value], or ask about our [HOURS] and [LOAN] services.";
                }
                if (chatContent.Contains("HOURS"))
                {
                    return "CHATBOT: SecureBank physical branches are open Sunday to Thursday, from 8:30 AM to 3:00 PM.";
                }
                if (chatContent.Contains("LOAN"))
                {
                    return "CHATBOT: We offer personal loans up to $50,000 with a competitive interest rate. Type 'HELP' to learn more.";
                }

                return "CHATBOT: Thank you for messaging support. Your query has been filed. Type HELP for a list of dynamic automated keywords.";
            }

            return "ERROR: Command not recognized. Type HELP for support instructions.";
        }





        private static void StartUdpListener()
        {
            Socket udpServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint udpEndPoint = new IPEndPoint(IPAddress.Parse(IP_ADDRESS), UDP_PORT);

            try
            {
                udpServerSocket.Bind(udpEndPoint);
                Console.WriteLine($"[UDP SERVER] Live Ticker Service started. Bound to {IP_ADDRESS}:{UDP_PORT}...");

                byte[] incomingBuffer = new byte[1024];

                EndPoint remoteClientEndPoint = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    int bytesReceived = udpServerSocket.ReceiveFrom(incomingBuffer, ref remoteClientEndPoint);
                    string udpRequest = Encoding.ASCII.GetString(incomingBuffer, 0, bytesReceived).Trim();

                    Console.WriteLine($"[UDP SERVER] Datagram intercepted from {remoteClientEndPoint}: \"{udpRequest}\"");

                    if (udpRequest == "GET_RATES")
                    {
                        string ratesPayload = "TICKER_DATA: USD/EGP=50.50 | EUR/EGP=52.10 | GBP/EGP=61.35 | TIMESTAMP=" + DateTime.Now.ToString("HH:mm:ss");
                        byte[] responsePayload = Encoding.ASCII.GetBytes(ratesPayload);

                        udpServerSocket.SendTo(responsePayload, remoteClientEndPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UDP SERVER ERROR] {ex.Message}");
            }
            finally
            {
                udpServerSocket.Close();
            }
        }

        private static void LogToText(string logLine)
        {
            lock (logLock)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(logFilePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logLine}");
                    }
                }
                catch
                {
                }
            }
        }
    }
}
