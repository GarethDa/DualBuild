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
            DATA,
            CREATE_GAMEOBJECT
        }

        public static List<string> InstructionTypeCodes = new List<string> { "@", "!", "#", "^", "$", "~", "*", "=", "%", "&" };
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


        static Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        static List<Socket> waitingRoomClients = new List<Socket>();
        static Queue<Socket> queuedWaitingRoomClients = new Queue<Socket>();
        public Dictionary<Socket, Queue<string>> clientMessages = Dictionary<Socket, Queue<string>>;
        static void Main(string[] args)
        {
            address = IPAddress.Parse(getIPAddress());
            setupEndPoint();
            startTCPServer();

            while (true)
            {
                //send info to all clients (where to send will be embeddedin the message itself)
                //break;
                List<Socket> removedFromRoom = new List<Socket>();
                foreach (Socket s in waitingRoomClients)
                {
                    if (s == null)
                    {
                        continue;
                    }
                    log("waiting on", s);
                    string data = removeGarbageCharacters(receiveTCPMessage(s));
                    if (data == null)
                    {
                        continue;
                    }
                    //all we have to check for is if the client requests to be put in a room, or wants to make their own room.
                    //if they dont want either, then we will just route their packets to all other clients in roomClients
                    //put in room, add them to list of clients in room
                    //if make new room, spin up a new thread (fuck it)

                    if (data[0].ToString() == getInstructionCode(InstructionType.CREATE_ROOM))
                    {
                        Console.WriteLine("Client wants to make a new room");
                        //make new room (add to dictionary)
                        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVQXYZ";
                        string roomCode = "";
                        while (!rooms.Keys.Contains(roomCode) && roomCode.Length == 0)
                        {
                            roomCode = "";
                            Random rand = new Random();
                            for (int i = 0; i < codeLength; i++)
                            {
                                roomCode += alphabet[rand.Next(0, alphabet.Count())];
                            }
                        }

                        Console.WriteLine("Room code:" + roomCode + "|");
                        rooms.Add(roomCode, new Room(maxPlayersPerLobby));
                        if (rooms[roomCode].addPlayer(s))
                        {
                            removedFromRoom.Add(s);
                            //send message back to client about their room number
                            sendTCPMessage(getInstructionCode(InstructionType.CREATE_ROOM) + roomCode, s);
                            Console.WriteLine("Client moved to room");
                        }
                        else
                        {
                            Console.WriteLine("Problem adding client to room");
                        }
                        


                        continue;
                    }
                    if (data[0].ToString() == getInstructionCode(InstructionType.JOIN_ROOM))
                    {
                        //join room

                        string joiningRoom = data.Substring(1, 4);
                        Console.WriteLine("Room Code:" + joiningRoom + "|");
                        if (rooms.ContainsKey(joiningRoom))
                        {
                            //get index of player

                            if (rooms[joiningRoom].addPlayer(s))
                            {
                                removedFromRoom.Add(s);

                                //send message that they joined the room
                                Console.WriteLine("Theres space in room:" + joiningRoom);

                                sendTCPMessage(getInstructionCode(InstructionType.JOIN_ROOM) + "o", s);
                            }
                            else
                            {

                                //send message that room is full
                                Console.WriteLine("Theres no space in room:" + joiningRoom);

                                sendTCPMessage(getInstructionCode(InstructionType.JOIN_ROOM) + "x", s);

                            }
                        }
                        else
                        {
                            Console.WriteLine("Room doesnt exist:" + joiningRoom);

                        }
                        continue;
                    }

                }

                foreach (Socket s in removedFromRoom)
                {
                    waitingRoomClients.Remove(s);
                    Console.WriteLine("Client removed from waiting room");

                }
                removedFromRoom.Clear();


                while (queuedWaitingRoomClients.Count > 0)
                {
                    waitingRoomClients.Add(queuedWaitingRoomClients.Dequeue());
                    Console.WriteLine("Client added to waiting room");

                }

                //now check all the sockets in the rooms and send messages to other clients accordingly
                foreach (string soc in rooms.Keys)
                {
                    Socket[] connectedPlayers = rooms[soc].connectedPlayers;
                    Room currentRoom = rooms[soc];
                    //check for message from all clients
                    //if there is one, send it to everyone else
                    //Console.Write("!");
                    for (int i = 0; i < maxPlayersPerLobby; i++)
                    {
                        //Console.WriteLine(soc);
                        if (currentRoom.connectedPlayers[i] == null)
                        {
                            continue;
                        }
                        //Console.WriteLine("player: " + i.ToString());
                        

                        Socket client = currentRoom.connectedPlayers[i];
                        log("waiting on", client);
                        string message = removeGarbageCharacters(receiveTCPMessage(client));
                        if (message == null)
                        {
                            continue;
                        }
                        log("Received Message:" + message, client);
                        //do the translation for gameobject using the table if it has the character for gameobject affected (@)
                        List<string> instructions = decodeInstruction(message);
                        List<Instruction> forwardInstructions = new List<Instruction>();
                        List<string> redoingInstructions = new List<string>();
                        int affectedObjectID = -1;
                        foreach (string inst in instructions)
                        {
                            Instruction adding = new Instruction("", true);
                            string code = getInstructionCode(inst).ToString();
                            string data = getAfterInstructionCode(inst);


                            if (code == getInstructionCode(InstructionType.REGISTER_GAMEOBJECT))
                            {
                                log("Registered Gameobject " + data, client);
                                int registeringIndex = currentRoom.registerGameObject();
                                adding.message  += (getInstructionCode(InstructionType.REGISTER_GAMEOBJECT) + data);
                                adding.message  += ("|" + registeringIndex);
                                forwardInstructions.Add(adding);

                            }
                            
                                if (code == getInstructionCode(InstructionType.CREATE_GAMEOBJECT))
                            {
                                List<string> instructionData = getDataFromInstruction(data);
                                string index = instructionData[0];
                                string id = instructionData[1];

                                int registeringIndex = int.Parse(index);//int.Parse(data.Substring(0,data.IndexOf('|')));
                                int registeringID = int.Parse(id);

                                currentRoom.GOTranslationTable[currentRoom.getIndex(client)][registeringIndex] = registeringID;
                               

                            }
                            if (code == getInstructionCode(InstructionType.POSITION_CHANGE))
                            {
                                //
                                adding.message += (getInstructionCode(InstructionType.POSITION_CHANGE) + data);
                                adding.shouldSendToSender = false;
                            }
                            if (code == getInstructionCode(InstructionType.LOCAL_GAMEOBJECT_ID))
                            {
                                affectedObjectID = int.Parse(data);
                                adding.shouldSendToSender = false;
                            }
                        }

                        foreach (string inst in redoingInstructions)
                        {
                            string code = getInstructionCode(inst).ToString();
                            string data = getAfterInstructionCode(inst);
                            Instruction adding = new Instruction("", true);
                            
                        }


                        
                        if (forwardInstructions.Count == 0)
                        {
                            continue;
                        }
                        foreach(Instruction instruction in forwardInstructions)
                        {
                            if(instruction.message == null)
                            {
                                return;
                            }
                            if(instruction.message.Length == 0)
                            {
                                return;
                            }
                            //send the message to all other clients
                            for (int u = 0; u < maxPlayersPerLobby; u++)
                            {
                                if(currentRoom.connectedPlayers[u] == null)
                                {
                                    continue;
                                }
                                if (!instruction.shouldSendToSender)
                                {
                                    if (connectedPlayers[u] == client)
                                    {
                                        continue;
                                    }
                                }

                                sendTCPMessage(instruction.message, connectedPlayers[u]);
                            }
                        }
                        

                    }
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
                        Console.WriteLine(currentInstruction);
                        returner.Add(currentInstruction);
                        currentInstruction = "";
                    }
                    
                }
                currentInstruction += c;
            }
            //for the instruction that is at the end, it wont get added ^
            if (currentInstruction.Length > 0)
            {
                Console.WriteLine(currentInstruction);
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
            Console.WriteLine("Garbage removed data:" + returner);
            return returner;
        }

        public static string receiveTCPMessage(Socket s)
        {
            byte[] message = new byte[512];
            int size = s.Receive(message);
            if (size == 0)
            {
                return null;
            }
            string data = Encoding.ASCII.GetString(message);
            Console.WriteLine(data);
            return data;
        }

        public static void sendTCPMessage(string message, Socket to)
        {
            byte[] data = new byte[512];
            data = Encoding.ASCII.GetBytes(message);
            to.Send(data, 0, data.Length, SocketFlags.None);
            Console.WriteLine("Sending:" + message + "| to:" + ((IPEndPoint)to.RemoteEndPoint).Address.ToString());
        }

        public static void startTCPServer()
        {
            TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            endPointTCP = new IPEndPoint(address, TCPPort);
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
                
                    Console.Write(".");
                    newSocket = TCPSocket.Accept();
                    queuedWaitingRoomClients.Enqueue(newSocket);
                Program p = new Program();
                Thread clientThread = new Thread(new ThreadStart(() => p.listenForMessages(newSocket)));
                clientThread.Start();
                Console.Write("Client connected");

                    connectedIndex++;

                
            }
        }

        public void listenForMessages(Socket s)
        {

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

        static void log(string m, Socket s)
        {
            Console.WriteLine("LOG:[" + m + "] FROM: [" + ((IPEndPoint)s.RemoteEndPoint).Address.ToString() + "]");

        }
    }

    public class Room
    {
        public Socket[] connectedPlayers;
        public int currentRound;//0 for intermission
        public List<List<int>> GOTranslationTable = new List<List<int>>();
        

        public Room(int maxPlayersInLobby)
        {
            connectedPlayers = new Socket[maxPlayersInLobby];
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

        public void addGameObject(Socket s, int index, int id)
        {

        }

        public int getIndex(Socket s)
        {
            for (int i = 0; i < connectedPlayers.Length; i++)
            {
                if (connectedPlayers[i] == s)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool addPlayer(Socket s)
        {
            bool hasRoomInLobby = false;
            for (int i = 0; i < connectedPlayers.Length; i++)
            {
                if (connectedPlayers[i] == null)
                {
                    //theres space
                    connectedPlayers[i] = s;

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

        public Instruction(string m, bool fs)
        {
            message = m;
           
            shouldSendToSender = fs;
        }
    }

}


