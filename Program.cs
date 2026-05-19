using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SecureBankSystem
{
    class Client
    {
        private const string SERVER_IP = "127.0.0.1";
        private const int TCP_PORT = 8000;
        private const int UDP_PORT = 8001;

        static void Main(string[] args)
        {
           
            Console.WriteLine(" SecureBank Client Application");
            Console.WriteLine("==================================================\n");

            while (true)
            {
                Console.WriteLine("\n--- Main Menu ---");
                Console.WriteLine("1. Access Banking & Support Services (TCP)");
                Console.WriteLine("2. Check Live Exchange Rates Ticker (UDP)");
                Console.WriteLine("3. Exit Application");
                Console.Write("Select a service (1-3): ");

                string choice = Console.ReadLine()?.Trim();

                if (choice == "1")
                {
                    RunTcpClient();
                }
                else if (choice == "2")
                {
                    RunUdpClient();
                }
                else if (choice == "3")
                {
                    Console.WriteLine("Thank you for using SecureBank System. Goodbye!");
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid choice. Please select a valid option.");
                }
            }
        }

        private static void RunTcpClient()
        {
           
            Socket tcpClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(SERVER_IP), TCP_PORT);

            try
            {
                Console.WriteLine($"[TCP CLIENT] Connecting to SecureBank Server at {SERVER_IP}:{TCP_PORT}...");
                tcpClientSocket.Connect(serverEndPoint);
                Console.WriteLine("[TCP CLIENT] Connected successfully.");

                byte[] buffer = new byte[1024];

                int bytesReceived = tcpClientSocket.Receive(buffer);
                string serverGreeting = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                Console.Write($"\n[SERVER] {serverGreeting}");

               
                while (true)
                {
                    Console.WriteLine("\nEnter Command (or type 'EXIT' to return to Main Menu):");
                    Console.Write("> ");
                    string command = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(command)) continue;

                    if (command.ToUpper() == "EXIT")
                    {
                        break;
                    }

                
                    byte[] requestBytes = Encoding.ASCII.GetBytes(command);
                    tcpClientSocket.Send(requestBytes);
bytesReceived = tcpClientSocket.Receive(buffer);

                    if (bytesReceived == 0)
                    {
                        Console.WriteLine("[TCP CLIENT] Connection closed by the server.");
                        break;
                    }

                    string serverResponse = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                    Console.WriteLine($"[SERVER RESPONSE] {serverResponse.Trim()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP CLIENT ERROR] Communication failed: {ex.Message}");
            }
            finally
            {
                try
                {
                    tcpClientSocket.Shutdown(SocketShutdown.Both);
                }
                catch { /* تجاهل الخطأ إذا كان السوكيت مغلقاً بالفعل */ }

                tcpClientSocket.Close();
                Console.WriteLine("[TCP CLIENT] Connection closed. Resources cleaned up.");
            }
        }

        private static void RunUdpClient()
        {
            Socket udpClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverUdpEndPoint = new IPEndPoint(IPAddress.Parse(SERVER_IP), UDP_PORT);

            try
            {
                string requestPayload = "GET_RATES";
                byte[] requestBytes = Encoding.ASCII.GetBytes(requestPayload);

                Console.WriteLine($"\n[UDP CLIENT] Sending datagram request '{requestPayload}' to {SERVER_IP}:{UDP_PORT}...");

                udpClientSocket.SendTo(requestBytes, serverUdpEndPoint);

                byte[] incomingBuffer = new byte[1024];

                EndPoint remoteServerEndPoint = new IPEndPoint(IPAddress.Any, 0);

                Console.WriteLine("[UDP CLIENT] Awaiting datagram response from server...");
                int bytesReceived = udpClientSocket.ReceiveFrom(incomingBuffer, ref remoteServerEndPoint);

                string responseData = Encoding.ASCII.GetString(incomingBuffer, 0, bytesReceived);
                Console.WriteLine($"[UDP CLIENT] Intercepted payload from {remoteServerEndPoint}:");
                Console.WriteLine(responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UDP CLIENT ERROR] Failed to fetch data: {ex.Message}");
            }
            finally
            {
                udpClientSocket.Close();
            }
        }
    }
}