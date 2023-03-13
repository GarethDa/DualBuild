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
        enum InstructionType
        {
            LOCAL_GAMEOBJECT_ID,
            POWERUP_USE,
            POSITION_CHANGE,
            ROTATION_CHANGE,
            VELOCITY_CHANGE,
            REGISTER_GAMEOBJECT,
            CREATE_ROOM,
            JOIN_ROOM,
            CHAT,
            CREATE_GAMEOBJECT
        }

        public static List<string> InstructionTypeCodes = new List<string> { "@", "!", "#", "^", "$", "~", "*", "=", "%", "&" };
        public static IPAddress address;
        public static int TCPPort = 8888;
        public static int UDPSendingPort = 8889;
        public static int UDPReceivingPort = 8890;
        //static string localHost = "127.0.0.1";
        private static byte[] buffer = new byte[512];
        private static IPHostEntry hostInfo;

        private static IPEndPoint endPointTCP;//the IP and port of the recipient for TCP
        public static EndPoint UDPSendingEndPoint;
        public static EndPoint UDPReceivingEndPoint;
        private static Socket TCPSocket;
        private static Socket UDPSocketSending;
        private static Socket UDPSocketReceiving;
        static int maxPlayersPerLobby = 4;
        static int codeLength = 4;


        static Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        static List<Socket> waitingRoomClients = new List<Socket>();
        
        public static Queue<Socket> queuedClientsTCP = new Queue<Socket>();
        public static Queue<EndPoint> queuedClientsUDP = new Queue<EndPoint>();
        public static Queue<ClientMessage> clientMessagesTCP = new Queue<ClientMessage>();
        public static Queue<ClientMessage> clientMessagesUDP = new Queue<ClientMessage>();
        public static Dictionary<Socket, string> clientRooms = new Dictionary<Socket, string>();


        static void Main(string[] args)
        {
            address = IPAddress.Parse(getIPAddress());
            setupEndPoint();
            startTCPServer();
            startUDPServer();
            Console.WriteLine("Started server on private IP:" + address.ToString());
            Program p = new Program();
            Thread clientThread = new Thread(new ThreadStart(() => p.clientConnect()));
            clientThread.Start();
            Console.WriteLine("Waiting for clients to connect...");
            //start UDP server

            while (true)
            {
                while (queuedClientsTCP.Count > 0)
                {

                    Socket newClient = queuedClientsTCP.Dequeue();
                    log("creating client", newClient);
                   
                    
                    Thread newClientThread = new Thread(new ThreadStart(() => p.listenForMessages(newClient)));
                    newClientThread.Start();
                   
                }
                //send info to all clients (where to send will be embeddedin the message itself)
                //break;

                List<Socket> removedFromRoom = new List<Socket>();


                while (clientMessagesTCP.Count>0)
                {
                    ClientMessage clientM = clientMessagesTCP.Dequeue();
                    
                        Socket client = clientM.client;

                        string mess = clientM.message;
                        if(mess.Length == 0)
                        {
                            continue;
                        }
                    List<string> instructions = decodeInstruction(mess);

                    actOnInstructions(client, client.LocalEndPoint, removedFromRoom, instructions);
                }

                while (clientMessagesUDP.Count > 0)
                {
                    ClientMessage clientM = clientMessagesUDP.Dequeue();

                    Socket client = clientM.client;

                    string mess = clientM.message;
                    if (mess.Length == 0)
                    {
                        continue;
                    }
                    List<string> instructions = decodeInstruction(mess);

                    actOnInstructions(client, client.LocalEndPoint, removedFromRoom, instructions);
                }




                foreach (Socket s in removedFromRoom)
                {
                    waitingRoomClients.Remove(s);
                    Console.WriteLine("Client removed from waiting room");

                }
                removedFromRoom.Clear();


               

                //now check all the sockets in the rooms and send messages to other clients accordingly
                

            }

        }

        public static void actOnInstructions(Socket client, EndPoint endPoint, List<Socket> removedFromRoom, List<string> instructions)
        {
           
            foreach (string inst in instructions)
            {
                //Console.WriteLine("PROCESSING:" + inst);
                Instruction adding = new Instruction("", true);
                string code = getInstructionCode(inst).ToString();
                string data = getAfterInstructionCode(inst);
                List<string> dataArguments = getDataFromInstruction(data);
                string identifier = dataArguments[dataArguments.Count - 1];
                if (code == getInstructionCode(InstructionType.CREATE_ROOM))
                {
                    Console.WriteLine("Client wants to make a new room");
                    //make new room (add to dictionary)
                    string alphabet = "ABCDEFGHIJKLMNOPQRSTUVQXYZ";
                    string roomKey = "";
                    while (!rooms.Keys.Contains(roomKey) && roomKey.Length == 0)
                    {
                        roomKey = "";
                        Random rand = new Random();
                        for (int i = 0; i < codeLength; i++)
                        {
                            roomKey += alphabet[rand.Next(0, alphabet.Count())];
                        }
                    }

                    Console.WriteLine("Room code:" + roomKey);
                    rooms.Add(roomKey, new Room(maxPlayersPerLobby));
                    if (rooms[roomKey].addPlayer(client,identifier,endPoint))
                    {
                        removedFromRoom.Add(client);
                        clientRooms.Add(client, roomKey);
                        //send message back to client about their room number
                        Socket[] newList = { client };
                        sendTCPMessage(getInstructionCode(InstructionType.CREATE_ROOM) + roomKey,client, identifier);
                        Console.WriteLine("Client moved to room");
                    }
                    else
                    {
                        Console.WriteLine("Problem adding client to room");
                    }




                }

                if (code == getInstructionCode(InstructionType.JOIN_ROOM))
                {
                    //join room

                    string joiningRoom = dataArguments[0];
                    Console.WriteLine("Room Code:" + joiningRoom);
                    if (rooms.ContainsKey(joiningRoom))
                    {
                        //get index of player

                        if (rooms[joiningRoom].addPlayer(client,identifier,endPoint))
                        {
                            removedFromRoom.Add(client);

                            clientRooms.Add(client, joiningRoom);
                            //send message that they joined the room
                            Console.WriteLine("Theres space in room:" + joiningRoom);
                            Socket[] newList = { client };
                            sendTCPMessage(getInstructionCode(InstructionType.JOIN_ROOM) + "o", client, identifier);
                        }
                        else
                        {

                            //send message that room is full
                            Console.WriteLine("Theres no space in room:" + joiningRoom);
                           
                            sendTCPMessage(getInstructionCode(InstructionType.JOIN_ROOM) + "x", client, identifier);

                        }
                    }
                    else
                    {
                        Console.WriteLine("Room doesnt exist:" + joiningRoom);

                    }
                    continue;
                }
                //if statements

                if (!clientRooms.ContainsKey(client))
                {
                    continue;
                }
                string roomCode = clientRooms[client];
                Socket[] connectedSockets = rooms[roomCode].connectedPlayersTCP;
               
                Room currentRoom = rooms[roomCode];



                List<Instruction> forwardInstructionsTCP = new List<Instruction>();
                List<Instruction> forwardInstructionsUDP = new List<Instruction>();
                List<string> redoingInstructions = new List<string>();
                int affectedObjectID = -1;
                List<string> instructionData = getDataFromInstruction(data);
               
                if (code == getInstructionCode(InstructionType.CHAT))//always UDP
                {
                    adding.message += getInstructionCode(InstructionType.CHAT) + instructionData[0];
                    adding.shouldSendToSender = false;//for testing
                    adding.IP = instructionData[1];
                    forwardInstructionsTCP.Add(adding);

                    
                }

                if (code == getInstructionCode(InstructionType.REGISTER_GAMEOBJECT))
                {

                    log("Registered Gameobject " + data, client);

                    int registeringIndex = currentRoom.registerGameObject();
                    Console.WriteLine("added -1 at index" + registeringIndex);
                    adding.message += (getInstructionCode(InstructionType.REGISTER_GAMEOBJECT) + dataArguments[0]) + ("|" + registeringIndex) ;

                    forwardInstructionsTCP.Add(adding);

                }

                if (code == getInstructionCode(InstructionType.CREATE_GAMEOBJECT))
                {
                   
                    string index = instructionData[0];
                    string id = instructionData[1];
                    //Console.WriteLine("raw registered GOdata ID:" + id+ " at index: " + index);

                    int registeringIndex = int.Parse(index);//int.Parse(data.Substring(0,data.IndexOf('|')));
                    int registeringID = int.Parse(id);

                    int playerIndex = currentRoom.getIndex(client);
                    currentRoom.GOTranslationTable[playerIndex][registeringIndex] = registeringID;
                    Console.WriteLine("registered gameobject with GOID: " + registeringID.ToString() + " at index: " + registeringIndex.ToString() + " at playerIndex: " + playerIndex.ToString());

                }
                if (code == getInstructionCode(InstructionType.POSITION_CHANGE))
                {
                    int clientGameobjectID = int.Parse(instructionData[1]);
                    string position = instructionData[0];

                    int clientIndex = currentRoom.getIndex(identifier);
                    //Console.WriteLine("Actual client index: " + clientIndex.ToString());
                    int indexToSearchAt = getIndexByGameObjectID(clientIndex, roomCode, clientGameobjectID);

                    for(int i = 0; i < maxPlayersPerLobby; i++)
                    {
                        if(currentRoom.GUIDs[i] == null)
                        {
                            continue;
                        }
                        if(currentRoom.GUIDs[i] == identifier)
                        {
                            continue;
                        }
                        Console.WriteLine("I: " + i.ToString());
                        int IDOfGOToModify = getGameObjectIDByIndex(i, roomCode, indexToSearchAt);
                        string messageToSend = getInstructionCode(InstructionType.POSITION_CHANGE) + position + "|" + IDOfGOToModify.ToString() ;
                        sendTCPMessage(messageToSend, client, identifier);
                    }
                }
                if (code == getInstructionCode(InstructionType.LOCAL_GAMEOBJECT_ID))
                {
                    affectedObjectID = int.Parse(data);
                    adding.shouldSendToSender = false;
                }


                forwardMessages(forwardInstructionsTCP, roomCode, client,true);
                //forwardMessages(forwardInstructionsUDP, roomCode, client,false);


                

            }
        }

        public static int getIndexByGameObjectID(int clientIndex, string roomCode, int ID)
        {
            Room currentRoom = rooms[roomCode];
            Console.WriteLine("Getting index: " + roomCode + " " + ID.ToString() + " client index: " + clientIndex.ToString());
           
            foreach(int i in currentRoom.GOTranslationTable[clientIndex])
            {
                Console.WriteLine(i);
            }
           
            return currentRoom.GOTranslationTable[clientIndex].IndexOf(ID);
        }

        public static int getGameObjectIDByIndex(int clientIndex,string roomCode, int indexInList)
        {
            Room currentRoom = rooms[roomCode];
            Console.WriteLine("Getting gameobject: " + roomCode + " " + indexInList.ToString() + " client index:" + clientIndex);
            return currentRoom.GOTranslationTable[clientIndex][indexInList];
        }

        public static void forwardMessages(List<Instruction> forwardInstructions, string roomCode, Socket client, bool TCP = true)

        {
            Socket[] connectedSockets = rooms[roomCode].connectedPlayersTCP;
            
            Room currentRoom = rooms[roomCode];
            if (forwardInstructions.Count == 0)
            {
                return;
            }
            foreach (Instruction instruction in forwardInstructions)
            {
                if (instruction.message == null)
                {
                    return;
                }
                if (instruction.message.Length == 0)
                {
                    return;
                }
                //send the message to all other clients
                for (int u = 0; u < maxPlayersPerLobby; u++)
                {
                    if (currentRoom.connectedPlayersTCP[u] == null)
                    {
                       
                            continue;
                        
                        
                    }
                    if (connectedSockets[u] == client)
                    {
                        if (!instruction.shouldSendToSender)
                        {
                            continue;
                        }
                    }
                   


                    
                        
                        sendTCPMessage(instruction.message, connectedSockets[u],currentRoom.GUIDs[u]);
                    
                    
                    log("Forwarded message:" + instruction.message, connectedSockets[u]);

                }

            }
        }


        public static List<string> decodeInstruction(string message)
        {//move to the client, cause the server simply sends the same message back to everyone else
            //string specialChars = "`~!@#$%^&*()-=_+{}:<>,.;'\\[]\"/?";//| TAKEN OUT cause it means a separation of data within an instruction
            string currentInstruction = "";
            List<string> returner = new List<string>();
            foreach (char c in message)
            {
                if (InstructionTypeCodes.Contains(c.ToString()))
                {
                    //beginning of instruction, put the current instruction in the list
                    if (currentInstruction.Length > 0)
                    {
                        //Console.WriteLine(currentInstruction);
                        returner.Add(currentInstruction);
                        currentInstruction = "";
                    }
                    
                }
                currentInstruction += c;
            }
            //for the instruction that is at the end, it wont get added ^
            if (currentInstruction.Length > 0)
            {
                //Console.WriteLine(currentInstruction);
                returner.Add(currentInstruction);
                currentInstruction = "";
            }
            return returner;
        }

        public static List<string> getDataFromInstruction(string instruction)
        {
            List<string> returner = new List<string>();
            string data = "";
            foreach (char c in instruction)
            {
                if (c == '|')
                {
                    returner.Add(data);
                    data = "";
                    continue;
                }
                data += c;
            }
            if (data.Length > 0)
            {
                returner.Add(data);

            }
            return returner;
        }

        static char getInstructionCode(string instruction)
        {
            return instruction[0];
        }

        static string getInstructionCode(InstructionType type)
        {
            return InstructionTypeCodes[(int)type].ToString();
        }
        static string getAfterInstructionCode(string instruction)
        {
            return instruction.Substring(1, instruction.Length - 1);
        }

        public static string removeGarbageCharacters(string s)
        {
            if(s == null)
            {
                return null;
            }
            string returner = "";
            string acceptedCharacters = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789`~!@#$%^&*()-=|_+{}:<>,.;'\\[]\"/?";
            foreach (char c in s)
            {
                if (!acceptedCharacters.Contains(c))
                {
                    continue;
                }
                returner += c;
            }
            //Console.WriteLine("Garbage removed data:" + returner);
            return returner;
        }

        public static string receiveTCPMessage(Socket s)
        {
            try
            {
                byte[] message = new byte[512];
                int size = 0;
                //Console.WriteLine("Listening for message...");


                size = s.Receive(message);
                if (size == 0)
                {
                    return null;
                }
                string data = Encoding.ASCII.GetString(message);
               
                return data;
            }
            catch(Exception e)
            {
                return null;
            }
           
        }

        public static string receiveUDPMessage(Socket s)
        {
            try
            {
                byte[] message = new byte[512];
                
                int receivedBytesCount = UDPSocketReceiving.ReceiveFrom(message, ref UDPReceivingEndPoint);
                
                if (receivedBytesCount == 0)
                {
                    return null;
                }
               
                //Console.WriteLine("Received from: {0} -> {1}", client.ToString(), Encoding.ASCII.GetString(buffer, 0, receivedBytesCount));
                
                string gottenData = Encoding.ASCII.GetString(message, 0, receivedBytesCount);
                return gottenData;
            }
            catch(Exception e)
            {
                return null;
            }
            
        }
/*
        public static void sendTCPMessage(string message, Socket[] users, int index)
        {
            Socket to = users[index];
            byte[] data = new byte[512];
            data = Encoding.ASCII.GetBytes(message);
            to.Send(data, 0, data.Length, SocketFlags.None);
            Console.WriteLine("Sending TCP:" + message + "| to:" + ((IPEndPoint)to.LocalEndPoint).Address.ToString());
        }
*/
        public static void sendTCPMessage(string message, Socket users, string ID)
        {
            message += "|" + ID;
            byte[] data = new byte[512];
            data = Encoding.ASCII.GetBytes(message);
            users.Send(data, 0, data.Length, SocketFlags.None);
            Console.WriteLine("Sending TCP:" + message + "| to:" + ((IPEndPoint)users.LocalEndPoint).Address.ToString());
        }

        /*
        public static void sendUDPMessage(string message, string IP)
        {
           

            byte[] data = new byte[512];
           

            IPEndPoint UDPEP = new IPEndPoint(IPAddress.Parse(IP),UDPSendingPort);
            data = Encoding.ASCII.GetBytes(message);
            UDPSocketSending.SendTo(data, data.Length,SocketFlags.None, UDPEP);
            Console.WriteLine("Sending UDP with IP:" + message + "| to:" + IP.ToString());
        }

        */

        public static void startTCPServer()
        {
            TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endPointTCP = new IPEndPoint(address, TCPPort);
            TCPSocket.Bind(endPointTCP);
            TCPSocket.Listen(100);
            

        }

        public static void startUDPServer()
        {
            UDPSocketSending = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);//this is our server
            UDPSocketReceiving = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);//this is our server

            UDPSendingEndPoint = new IPEndPoint(IPAddress.Any, UDPSendingPort);//can be from any ip address in the world. DNS sets the port
            UDPReceivingEndPoint = new IPEndPoint(IPAddress.Any, UDPReceivingPort);//can be from any ip address in the world. DNS sets the port
            
            UDPSocketSending.Blocking = false;
            UDPSocketReceiving.Blocking = false;

            UDPSocketReceiving.Bind(UDPReceivingEndPoint);
            UDPSocketReceiving.ReceiveTimeout = 15;
            UDPSocketSending.ReceiveTimeout = 15;


        }

        public void clientConnect()//add in room number later?
        {
            int connectedIndex = 0;
            while (true)
            {


                Socket newSocketTCP = default(Socket);
                
                
                    Console.Write(".");
                    newSocketTCP = TCPSocket.Accept();
                newSocketTCP.Blocking = false;
                newSocketTCP.ReceiveTimeout = 15;
               
                //clientMessages.Add(newSocket, new Queue<string>());
                queuedClientsTCP.Enqueue(newSocketTCP);
                queuedClientsUDP.Enqueue(newSocketTCP.LocalEndPoint);

                Console.Write("Client connected");
                Console.WriteLine("Client added to waiting room");

                    connectedIndex++;

            }
        }

        public void listenForMessages(Socket s)
        {
            while (s.Connected)
            {
                //log("start -----------------------", s);
                string TCPdata = removeGarbageCharacters(receiveTCPMessage(s));
                if (TCPdata != null)
                {
                    log("Received TCP: " + TCPdata, s);
                    //log("Added TCP to queue", s);
                    clientMessagesTCP.Enqueue(new ClientMessage(s, TCPdata));
                }
                //log("finished TCP", s);
                
                string UDPdata = removeGarbageCharacters(receiveUDPMessage(s));
                if (UDPdata != null)
                {
                  
                   // log("Added UDP to queue", s);
                    clientMessagesUDP.Enqueue(new ClientMessage(s, UDPdata));
                }
                //log("finished UDP", s);

            }
            


        }

        private static void setupEndPoint()
        {
            //https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services
            try
            {


                //hostInfo = await Dns.GetHostEntryAsync(serverIP);

                UDPSendingEndPoint = new IPEndPoint(address, UDPSendingPort);
                UDPReceivingEndPoint = new IPEndPoint(address, UDPReceivingPort);

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

        static void log(string m, Socket s)
        {
            Console.WriteLine("LOG:[" + m + "] FROM: [" + ((IPEndPoint)s.RemoteEndPoint).Address.ToString() + "]");

        }
    }

    public class Room
    {
        public Socket[] connectedPlayersTCP;
      
        public string[] GUIDs;
        public int currentRound;//0 for intermission
        public List<List<int>> GOTranslationTable = new List<List<int>>();
        

        public Room(int maxPlayersInLobby)
        {
            connectedPlayersTCP = new Socket[maxPlayersInLobby];
           
            GUIDs = new string[maxPlayersInLobby];
            for (int i = 0; i < maxPlayersInLobby; i++)
            {
                GOTranslationTable.Add(new List<int>());
            }

        }

        public int registerGameObject()
        {
            foreach (List<int> list in GOTranslationTable)
            {
                list.Add(-1);
            }
            return GOTranslationTable[0].Count-1;
        }


        public int getIndex(string ID)
        {
            for (int i = 0; i < GUIDs.Length; i++)
            {
                //Console.WriteLine("searching socket... " + i.ToString()); ;
                if (GUIDs[i] == ID)
                {
                    return i;
                }
            }
            return -1;
        }

        public int getIndex(Socket s)
        {
            for (int i = 0; i < connectedPlayersTCP.Length; i++)
            {
                //Console.WriteLine("searching socket... " + i.ToString()); ;
                if (connectedPlayersTCP[i] == s)
                {
                    return i;
                }
            }
            return -1;
        }

       

        public bool addPlayer(Socket s, string guid, EndPoint ep)
        {
            bool hasRoomInLobby = false;
            for (int i = 0; i < connectedPlayersTCP.Length; i++)
            {
                if (connectedPlayersTCP[i] == null)
                {
                    //theres space
                    connectedPlayersTCP[i] = s;
                    
                    GUIDs[i] = guid;
                    hasRoomInLobby = true;
                    break;
                }
            }
            return hasRoomInLobby;
        }


    }


    public class Instruction
    {
        public string message = "";
        //if its added to the list, it will be forwarded
        public bool shouldSendToSender = true;
        public string IP = "";

        public Instruction(string m, bool fs)
        {
            message = m;
           
            shouldSendToSender = fs;
        }

        public string getIP()
        {
            return IP;
        }
    }

    public class ClientMessage
    {
        public string message = "";
        public Socket client;

        public ClientMessage(Socket s, string m)
        {
            message = m;
            client = s;
        }
    }

}


