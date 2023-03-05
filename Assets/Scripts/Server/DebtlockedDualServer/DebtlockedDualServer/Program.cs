using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
namespace DebtlockedDualServer
{
    class Program
    {
        
        public static IPAddress address;
        public static int TCPPort = 8888;
        public static int UDPPort = 8889;
        private static byte[] buffer = new byte[512];
        private static IPHostEntry hostInfo;
       
        private static IPEndPoint endPointTCP;//the IP and port of the recipient for TCP
        private static IPEndPoint endPointUDP;//the IP and port of the recipient for UDP
        private static Socket TCPSocket;
        private static Socket UDPSocket;
        static int maxPlayersPerLobby = 4;
        static int codeLength = 4;
        static Dictionary<string, Socket[]> rooms = new Dictionary<string, Socket[]>();
        static List<Socket> waitingRoomClients = new List<Socket>();
        static Queue<Socket> queuedWaitingRoomClients = new Queue<Socket>();
        static void Main(string[] args)
        {
            address = IPAddress.Parse(getIPAddress());
            setupEndPoint();
            startTCPServer();
            
            while (true) {
                //send info to all clients (where to send will be embeddedin the message itself)
                //break;
                List<Socket> removedFromRoom = new List<Socket>();
                foreach(Socket s in waitingRoomClients)
                {
                    if (s == null)
                    {
                        continue;
                    }
                    byte[] message = new byte[512];
                    int size = s.Receive(message);
                    if (size == 0)
                    {
                        continue;
                    }//no message is being sent???
                    string data = Encoding.ASCII.GetString(message);
                    Console.WriteLine(data);
                    //all we have to check for is if the client requests to be put in a room, or wants to make their own room.
                    //if they dont want either, then we will just route their packets to all other clients in roomClients
                    //put in room, add them to list of clients in room
                    //if make new room, spin up a new thread (fuck it)
                    
                    if(data[0] == '+')
                    {
                        Console.WriteLine("Client wants to make a new room");
                        //make new room (add to dictionary)
                        string alphabet ="ABCDEFGHIJKLMNOPQRSTUVQXYZ";
                        string roomCode ="";
                        while(!rooms.Keys.Contains(roomCode) && roomCode.Length == 0)
                        {
                            roomCode ="";
                                Random rand = new Random();
                            for (int i = 0; i < codeLength; i++)
                            {
                                roomCode += alphabet[rand.Next(0, alphabet.Count())];
                            }
                        }

                        Console.WriteLine("Room code:" + roomCode + "|");
                                rooms.Add(roomCode,new Socket[maxPlayersPerLobby]);
                        removedFromRoom.Add(s);
                        //send message back to client about their room number
                        sendTCPMessage("+" + roomCode,s);
                        Console.WriteLine("Client moved to room");
                        

                        continue;
                    }
                    if (data[0] == '=')
                    {
                        //join room
                        data = data.Substring(1, data.Length-1);
                        bool containsRoom = false;
                        Console.WriteLine(rooms.ElementAt(0).Key.ToString() + "|");
                        foreach(string roomCode in rooms.Keys)
                        {
                            if (roomCode.Equals(data))
                            {
                                containsRoom = true;
                                break;
                            }
                        }
                        if (containsRoom)
                        {
                            //get index of player
                            bool hasRoomInLobby = false;
                            for(int i = 0; i < maxPlayersPerLobby; i++)
                            {
                                if(rooms[data][i] == null)
                                {
                                    //theres space
                                    rooms[data][i] = s;
                                    removedFromRoom.Add(s);
                                    hasRoomInLobby = true;
                                    break;
                                }
                            }
                            if (!hasRoomInLobby)
                            {
                                //send message that room is full
                                Console.WriteLine("Theres no space in room:" + data);

                                sendTCPMessage("=x", s);
                            }
                            else
                            {
                                //send message that they joined the room
                                Console.WriteLine("Theres space in room:" + data);

                                sendTCPMessage("=o", s);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Room doesnt exist:" + data);

                        }
                        continue;
                    }
                    
                }

                foreach(Socket s in removedFromRoom)
                {
                    waitingRoomClients.Remove(s);
                    Console.WriteLine("Client removed from waiting room");

                }
                removedFromRoom.Clear();

                
                while(queuedWaitingRoomClients.Count > 0)
                {
                    waitingRoomClients.Add(queuedWaitingRoomClients.Dequeue());
                    Console.WriteLine("Client added to waiting room");

                }
               
                //now check all the sockets in the rooms and send messages to other clients accordingly
                foreach (string soc in rooms.Keys)
                {
                    Socket[] connectedPlayers = rooms[soc];
                    //check for message from all clients
                    //if there is one, send it to everyone else
                }

            }

        }

        public static void sendTCPMessage(string message, Socket to)
        {
            byte[] data = new byte[512];
            data = Encoding.ASCII.GetBytes(message);
            to.Send(data, 0, data.Length, SocketFlags.None);
        }

        public static void startTCPServer()
        {
            TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endPointTCP = new IPEndPoint(address,TCPPort);
            TCPSocket.Bind(endPointTCP);
            TCPSocket.Listen(100);
            Console.WriteLine("Started server on private IP:" + address.ToString());
            Program p = new Program();
            Thread clientThread = new Thread(new ThreadStart(() => p.clientConnect()));
            clientThread.Start();
            Console.WriteLine("Waiting for clients to connect...");
           
        }

        public void clientConnect()//add in room number later?
        {
            int connectedIndex = 0;
            while (true)
            {
                

                Socket newSocket = default(Socket);
                while (true)
                {
                    Console.Write(".");
                    newSocket = TCPSocket.Accept();
                    queuedWaitingRoomClients.Enqueue(newSocket);
                    Console.Write("Client connected");

                    connectedIndex++;
                    if (connectedIndex == 1)
                    {
                        //   break;
                    }
                }
            }
        }

        private static void setupEndPoint()
        {
            //https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services
            try
            {


                //hostInfo = await Dns.GetHostEntryAsync(serverIP);
                
                endPointUDP = new IPEndPoint(address, UDPPort);

                endPointTCP = new IPEndPoint(address, TCPPort);

               

                Console.WriteLine("Setup endpoint");
            }
            catch (Exception e)
            {

                if (e is SocketException)
                {
                    Console.WriteLine("Socket error, check IP");
                }
                else
                {
                    Console.WriteLine(e.Message);
                }
            }




        }

        public static string getIPAddress()
        {
           
            //https://stackoverflow.com/questions/6803073/get-local-ip-address
            var host = Dns.GetHostEntry(Dns.GetHostName());
           
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}