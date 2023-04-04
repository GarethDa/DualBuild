using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Unity.Jobs;


public enum InstructionType
{
    PLAYER_DIED,
    POWERUP_USE,
    POSITION_CHANGE,
    ROTATION_CHANGE,
    VELOCITY_CHANGE,
    REGISTER_GAMEOBJECT,
    CREATE_ROOM,
    JOIN_ROOM,
    CHAT,
    CREATE_GAMEOBJECT,
    APPLY_PHYSICS,
    START_GAME,
    READY,
    REQUEST_LEVEL,
    ADD_SCORE
}


public class NetworkManager : MonoBehaviour
{
    
    //misc
    List<string> InstructionTypeCodes = new List<string>{"@","!","#","^","$", "~", "*", "=","%","&","?", "<", ">", "`", "/", ";", "¥" , "¤" };
    public bool isHost = false;
    public float secondsBetweenUpdates = 0.5f;
    float secondsSinceLastUpdate = 0f;
    public int TCPPort = 8888;
    public int UDPPortSending = 8889;
    public int UDPPortReceiving = 8889;
    public string serverIP;
    public float timeoutSeconds = 2f;
    int playersConnected = 0;
    string identifier;

    public static NetworkManager instance;
    public string roomKey = "";
    public string message = "";
    public string username = "John Gaming";
  


    //network and socket stuff
    private static byte[] buffer = new byte[512];
    private static IPHostEntry hostInfo;
    private static IPAddress ip;
    private static IPEndPoint endPointTCP;//the IP and port of the recipient for TCP
    public static EndPoint sendingEndPointUDP;
    public static EndPoint receivingEndPointUDP;
    private static Socket TCPSocket;
    private static Socket UDPSocketSending;
    private static Socket UDPSocketReceiving;
    Dictionary<NetworkScript, List<NetworkInstruction>> queuedTCPInstructions = new Dictionary<NetworkScript, List<NetworkInstruction>>();
    Dictionary<NetworkScript, List<NetworkInstruction>> queuedUDPInstructions = new Dictionary<NetworkScript, List<NetworkInstruction>>();
    Thread TCPListener;
    Thread UDPListener;

    //client only
    public static EndPoint receiver;

    //server only (now a console app)
    //List<NetworkedClient> clients = new List<NetworkedClient>();
    //List<List<int>> gameobjectIDs = new List<List<int>>();//for server only

    //booleans for async tasks
    bool hasSetupEndpoint = false;
    bool setupTCP = false;
    bool setupUDP = false;
    static Queue<string> incomingInstructions = new Queue<string>();
   

    public List<string> getCodes()
    {
        return InstructionTypeCodes;
    }

    public List<string> getDataFromInstruction(string instruction)
    {
        
        List<string> returner = new List<string>();
        string data = "";
        foreach(char c in instruction)
        {
            if(c == '|')
            {
               
                returner.Add(data);
                data = "";
                continue;
            }
            data += c;
        }
        if(data.Length > 0)
        {
            
            returner.Add(data);
          
        }
        return returner;
    }

    public List<string> decodeInstruction(string message)
    {//move to the client, cause the server simply sends the same message back to everyone else
        //string specialChars = "`~!@#$%^&*()-=_+{}:<>,.;'\\[]\"/?";//| taken out cause it means separation of data within an instruction
        string currentInstruction = "";
        List<string> returner = new List<string>();
        foreach (char c in message)
        {
            if (InstructionTypeCodes.Contains(c.ToString()))
            {
                //beginning of instruction, put the current instruction in the list
                if (currentInstruction.Length > 0)
                {
                    returner.Add(currentInstruction);
                    currentInstruction = "";
                }
               
            }
            currentInstruction += c;
        }
        //for the instruction that is at the end, it wont get added ^
        if (currentInstruction.Length > 0)
        {
            returner.Add(currentInstruction);
            currentInstruction = "";
        }
        return returner;
    }

   



    public void setupUDPClient()
    {
        UDPSocketSending = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        UDPSocketReceiving = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        UDPSocketReceiving.Blocking = true;
        UDPSocketSending.Blocking = true;
        //UDPSocket.Connect(endPointUDP);
        UDPListener = new Thread(UDPMessageListener);
        UDPListener.Start();
        setupUDP = true;
    }

    public bool setupTCPClient()//https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services
    {

        try
        {
            TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            TCPSocket.Connect(endPointTCP);


            TCPListener = new Thread(TCPMessageListener);
            TCPListener.Start();



            Debug.Log("Setup TCP Client");
            setupTCP = true;
            return true;
        }
        catch(Exception e)
        {
            return false;
        }
        
        
    } 

    


    private bool setupEndPoint()
    {
        //https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services
        try
        {


            //hostInfo = await Dns.GetHostEntryAsync(serverIP);
            ip = IPAddress.Parse(serverIP);//hostInfo.AddressList[0];
           
            sendingEndPointUDP = new IPEndPoint(ip, UDPPortSending);
            receivingEndPointUDP = new IPEndPoint(IPAddress.Any, UDPPortReceiving);

            endPointTCP = new IPEndPoint(ip, TCPPort);

            hasSetupEndpoint = true;
           
            Debug.Log("Setup endpoint");
            return true;
        }
        catch (Exception e)
        {
           
            if(e is SocketException)
            {
                Debug.LogError("Socket error, check IP");
            }
            else
            {
                Debug.LogError(e.Message);
            }
            return false;
        }
        
        

        
    }

    public string getIPAddress()
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

    public string getPlayerName()
    {
        //go onto AWS and get the name or something, maybe make it local??
        return "Jhon Gaming";
    }

    public string getGameObjectCode(GameObject o)
    {
        return o.GetInstanceID().ToString();
    }

    public void ignoreAndSendTCPMessage(string message)
    {
        sendTCPMessage(message);
    }
    


    public void queueTCPInstruction(NetworkScript script, NetworkInstruction instruction, bool replace = true)
    {
        //Debug.Log("Queued instruction " + instruction.getString());
        if (!queuedTCPInstructions.ContainsKey(script))
        {
            queuedTCPInstructions.Add(script, new List<NetworkInstruction>());
        }

        if (replace)
        {
            foreach (NetworkInstruction i in queuedTCPInstructions[script])//go through all instructions queued by the script
            {
                if (i.code == instruction.code)//check if an instruction with the same code has been issued
                {
                    i.codeData = instruction.codeData;//if so, replace it
                    return;
                }
            }
        }
        


        queuedTCPInstructions[script].Add(instruction);//at this point, there is a list of instructions made, and the type of instruction is unique, so add it in to the list
    }

    public void queueUDPInstruction(NetworkScript script, NetworkInstruction instruction)
    {
        if (!queuedUDPInstructions.ContainsKey(script))
        {
            queuedUDPInstructions.Add(script, new List<NetworkInstruction>());
        }

        foreach (NetworkInstruction i in queuedUDPInstructions[script])//go through all instructions queued by the script
        {
            if (i.code == instruction.code)//check if an instruction with the same code has been issued
            {
                i.codeData = instruction.codeData;//if so, replace it
                return;
            }
        }
        queuedUDPInstructions[script].Add(instruction);//at this point, there is a list of instructions made, and the type of instruction is unique, so add it in to the list
    }


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        // gameobjectIDs.Add(new List<int>());//reserve list of gameobjects at index
        identifier = Guid.NewGuid().ToString();
        Debug.Log(identifier);
        setupNetworking();
        
    }

    private void Update()
    {
       
        if (!setupTCP)
        {
            return;
        }
        while (incomingInstructions.Count > 0)
        {
            //Debug.Log("Received:" + incomingInstructions.Peek());
            executeInstructions(decodeInstruction(incomingInstructions.Dequeue()));
        }

        secondsSinceLastUpdate += Time.deltaTime;
        if(secondsSinceLastUpdate < secondsBetweenUpdates)
        {
            return;
           
        }
        secondsSinceLastUpdate = 0;

        
            foreach(NetworkScript key in queuedTCPInstructions.Keys)
            {
                foreach(NetworkInstruction instruction in queuedTCPInstructions[key])
                {
                    sendTCPMessage(instruction.getString()); ;
                }
            queuedTCPInstructions[key].Clear();
            }
            
        

       
            foreach (NetworkScript key in queuedUDPInstructions.Keys)
            {
                foreach (NetworkInstruction instruction in queuedUDPInstructions[key])
                {
                    sendUDPMessage(instruction.getString()); ;
                }
            queuedUDPInstructions[key].Clear();
        }

        


    }

   

    private void TCPMessageListener()
    {
        while (true)
        {
            //Debug.Log("start receiving");
            byte[] receivedTCP = new byte[512];
            int receivedTCPSize = TCPSocket.Receive(receivedTCP);
            //Debug.Log("Called receiving");
            if (receivedTCPSize > 0)
            {
                //parse message
                string TCPMessage = Encoding.UTF8.GetString(receivedTCP,0,receivedTCPSize);
                incomingInstructions.Enqueue(TCPMessage);
                //Debug.Log("RECEIVED TCP MESSAGE: " + TCPMessage);
            }
        }
    }

    private void UDPMessageListener()
    {
        UDPSocketReceiving.Bind(receivingEndPointUDP);
        while (true)
        {
            Debug.Log("start receiving UDP");
            byte[] receivedUDP = new byte[512];
            //IPEndPoint serverIPEP = new IPEndPoint( ((IPEndPoint)(UDPSocket.RemoteEndPoint)).Address, UDPPortServer);
            //EndPoint serverEP = (EndPoint)serverIPEP;
            int receivedUDPSize = UDPSocketReceiving.ReceiveFrom(receivedUDP, ref receivingEndPointUDP);
            Debug.Log("Called receiving UDP");
            if (receivedUDPSize > 0)
            {
                //parse message
                string UDPMessage = Encoding.UTF8.GetString(receivedUDP, 0, receivedUDPSize);
                incomingInstructions.Enqueue(UDPMessage);
                Debug.Log("RECEIVED UDP MESSAGE: " + UDPMessage);

            }
        }
    }
    char getInstructionCode(string instruction)
    {
        return instruction[0];
    }
    string getAfterInstructionCode(string instruction)
    {
        return instruction.Substring(1, instruction.Length-1);
    }
    public void executeInstructions(List<string> instructions)
    {
        GameObject affectedObject = null;
        List<string> waitForAfter = new List<string>();//this is in case a transform change is before the @ for an object, we can get the @ first, and just repeat the transform change instruction after the object has been specified
        foreach (string inst in instructions)
        {
            string code = getInstructionCode(inst).ToString();
            string data = getAfterInstructionCode(inst);
            List<string> instructionData = getDataFromInstruction(data);
            string receivedIdentifier = instructionData[instructionData.Count - 1];
            bool isMyInstruction = true;
            //Debug.Log(receivedIdentifier + " " + identifier);
            if (!receivedIdentifier.Equals(identifier))
            {
                
                if (!receivedIdentifier.Equals(Guid.Empty.ToString()))
                {
                    isMyInstruction = false;
                }
               
            }
            
            if (code == getInstructionCode(InstructionType.CHAT))
            {
                RoundManager.instance.currentPlayers[0].GetComponentInChildren<ChatBoxBehaviour>().QueueMessage(instructionData[0]);
                Debug.Log("CHAT MESSAGE: " + instructionData[0]);
            }
            if (code == getInstructionCode(InstructionType.ADD_SCORE))
            {
                //todo, add GO and score to round manager if args > 0
                //else, start round 64
                Debug.Log(data);
               if(!instructionData[0].Contains("X"))
                {
                    Debug.Log(instructionData[0]);
                    Debug.Log(instructionData[1]);
                    int index = int.Parse(instructionData[1]);
                    int score = int.Parse(instructionData[0]);

                    
                    for(int i = 0; i < PlayerManager.instance.networkedPlayerTransform.childCount; i++)
                    {
                        GameObject child = PlayerManager.instance.networkedPlayerTransform.GetChild(i).gameObject;
                        if(child.GetComponent<NetworkedScores>().getIndex() == index)
                        {
                            child.GetComponent<NetworkedScores>().addScore(score);
                            RoundManager.instance.addScore(child, score);
                            break;
                        }
                    }
                   
                   // RoundManager.instance.addScore(toAffect, score);
                    Debug.Log("ADDED SCORE");
                }
                else
                {
                    //end the game  
                    
                    RoundManager.instance.loadLevelExpress(64);
                    RoundManager.instance.endRoundCleanup();
                    RoundManager.instance.startRound("ending game networking");
                    //RoundManager.instance.startRound("ending game networking");
                    Debug.Log("END GAME");
                }
            }
            if (code == getInstructionCode(InstructionType.PLAYER_DIED))
            {
                //NOT for scoring
                int GOID = int.Parse(instructionData[0]);
                Debug.Log(instructionData[0]);
                GameObject toAffect = (GameObject)EditorUtility.InstanceIDToObject(GOID);
                if (toAffect == null)
                {
                    return;
                }
                if (!toAffect.tag.Contains("Player"))
                {
                    return;
                }
                RoundManager.instance.addToDeath(toAffect);
            }
            
            if (code == getInstructionCode(InstructionType.START_GAME))
            {
                requestGameObjectCreation("PlayerSingle");
                RoundManager.instance.setGameStarted(true);
            }
            if (code == getInstructionCode(InstructionType.CREATE_ROOM))
            {
                EventManager.onGetRoomKey?.Invoke(null,new StringArgs(instructionData[0]));
                if (instructionData.Count > 2)
                {
                    RoundManager.instance.playerIndexOffset = int.Parse(instructionData[1]);
                }
            }
            if (code == getInstructionCode(InstructionType.JOIN_ROOM))
            {
                if(instructionData.Count == 0)
                {
                    //could not join room
                    Debug.Log("EMPTY");
                    continue;
                }
                EventManager.onNewPlayerJoined?.Invoke(null, new StringArgs(instructionData[0]));
                
            }
            if (code == getInstructionCode(InstructionType.APPLY_PHYSICS))
            {
                Debug.Log("APPLY PHYSICS ");
                NetworkPhysicsData dataPhysics = JsonUtility.FromJson<NetworkPhysicsData>(instructionData[0]);
                int GOID = int.Parse(instructionData[1]);
                Debug.Log(GOID);
                GameObject toAffect = (GameObject)EditorUtility.InstanceIDToObject(GOID);
                if(toAffect == null || dataPhysics == null)
                {
                    continue;
                }
                toAffect.GetComponent<NetworkedPhysics>().setData(dataPhysics);

            }
            if (code == getInstructionCode(InstructionType.POSITION_CHANGE))
            {//will be to create an object with a string prefab
                Vector3 dataPosition = JsonUtility.FromJson<Vector3>(instructionData[0]);
                int GOID = int.Parse(instructionData[1]);
                Debug.Log(GOID);
                GameObject toAffect = (GameObject)EditorUtility.InstanceIDToObject(GOID);
                if(toAffect == null)
                {
                    Debug.Log("GO NULL");
                }
                if(toAffect.GetComponent<NetworkedPosition>() == null)
                {
                    Debug.Log("GO COMPONENT NULL");
                }
                if (dataPosition == null)
                {
                    Debug.Log("DATA NULL");
                }
                toAffect.GetComponent<NetworkedPosition>().setData(dataPosition);
                //Debug.Log("Moved GO with ID " + instructionData[1] + " to position: " + dataPosition.ToString());
            }

            if (code == getInstructionCode(InstructionType.VELOCITY_CHANGE))
            {//will be to create an object with a string prefab
                Vector3 dataVel = JsonUtility.FromJson<Vector3>(instructionData[0]);
                GameObject toAffect = (GameObject)EditorUtility.InstanceIDToObject(int.Parse(instructionData[1]));
                toAffect.GetComponent<NetworkedVelocity>().setData(dataVel);
                //Debug.Log("Changed velocity GO with ID " + instructionData[1] + " to velocity: " + dataVel.ToString());
            }
            if (code == getInstructionCode(InstructionType.ROTATION_CHANGE))
            {//will be to create an object with a string prefab
                Vector4 dataRot = JsonUtility.FromJson<Vector4>(instructionData[0]);
                GameObject toAffect = (GameObject)EditorUtility.InstanceIDToObject(int.Parse(instructionData[1]));
                toAffect.GetComponent<NetworkedRotation>().setData(dataRot);
               // Debug.Log("Changed rotation GO with ID " + instructionData[1] + " to Rotation: " + dataRot.ToString());
            }

            if (code == getInstructionCode(InstructionType.REGISTER_GAMEOBJECT))
            {//will be to create an object with a string prefab
                Debug.Log("creating object " + data);
                
               
                string prefab = instructionData[0];
                
                string index = instructionData[1];
                string playerIndex = instructionData[2];
                string objectName = instructionData[3];
                bool createPlayerNetworked = false;
                bool createdPlayerType = false;
                if(prefab=="PlayerSingle" && !isMyInstruction)
                {
                    createPlayerNetworked = true;
                    
                }
                if(prefab.Contains("Player"))
                {
                    createdPlayerType = true;
                }
                if (createPlayerNetworked)
                {
                    prefab = "PlayerNetworked";
                    Debug.Log("PLAYER NETWORKED ID:");
                }
                else
                {
                    Debug.Log("PLAYER ID:");
                }
                
                affectedObject = Instantiate<GameObject>(Resources.Load<GameObject>(prefab));
                if (createdPlayerType)
                {
                    affectedObject.GetComponent<NetworkedScores>().setData(int.Parse(playerIndex));
                    affectedObject.GetComponent<NetworkedScores>().setUserName(objectName);
                    affectedObject.transform.Find("PlayerObj").GetComponentInChildren<Renderer>().material = PlayerManager.instance.getRoboMat(int.Parse(playerIndex));
                }
                if (!createPlayerNetworked)
                {
                    affectedObject.transform.SetParent(PlayerManager.instance.transform);
                }
                else
                {
                    affectedObject.transform.SetParent(PlayerManager.instance.networkedPlayerTransform);
                }
                Debug.Log(affectedObject.GetInstanceID());
                sendTCPMessage(getInstructionCode(InstructionType.CREATE_GAMEOBJECT) + index.ToString() + "|" + affectedObject.GetInstanceID().ToString() + "|" + prefab);

            }
            if (code == getInstructionCode(InstructionType.REQUEST_LEVEL))
            {
                //Debug.Log(instructionData[0] + " " + data + " " + instructionData[1]);
               
                int levelToLoad = int.Parse(instructionData[0]);
                int offset = int.Parse(instructionData[1]);
                Debug.Log("GOT LEVEL REQUEST " + levelToLoad + " WITH OFFSET " + offset);

                if (RoundManager.instance.loadLevelExpress(levelToLoad))
                {
                    RoundManager.instance.endRoundCleanup();
                    RoundManager.instance.startRound("netwroking");
                }
                
                

            }
                if (code == getInstructionCode(InstructionType.READY))
            {
                int levelToLoad = int.Parse(instructionData[0]);
                if (levelToLoad != -1)
                {
                    //everyone ready, so we send them in 5 seconds
                    Debug.Log("SET READY");
                    RoundManager.instance.setEveryoneReady();
                    
                }
                else
                {
                    //stop ready timer
                    Debug.Log("SET NOT READY");
                    RoundManager.instance.setEveryoneNotReady();

                }
                
            }

                if (code == getInstructionCode(InstructionType.POWERUP_USE))
            {
                List<float> list = JsonUtility.FromJson<List<float>>(instructionData[0]);
                int GOID = int.Parse(instructionData[1]);
                GameObject toAffect = (GameObject)EditorUtility.InstanceIDToObject(GOID);
                powerUpList type = (powerUpList)list[0];
                //STATUSES: 0 - pickup 1- use 2- effect 3- end
                int status = (int)list[1];

                if (type == powerUpList.Bomb)
                {
                    if (status == 1)
                    {
                        //threw the bomb
                        //spawn new bomb locally and throw it in the direction of the affected gameobject
                        GameObject newBomb = Instantiate(Resources.Load<GameObject>("Element Prefabs/Projectiles/Bomb"));
                        CharacterAiming playerAiming = PlayerManager.instance.transform.GetChild(0).GetComponent<CharacterAiming>();
                        newBomb.GetComponent<BombBehaviour>().setThrown(true);

                        newBomb.transform.position += new Vector3(2 * toAffect.transform.forward.x, 0f, 2 * toAffect.transform.forward.z);

                        //Throw the ball forward, multiplied by the throwing force
                        newBomb.GetComponent<Rigidbody>().AddForce(toAffect.transform.forward * (playerAiming.getThrowForce() + list[2]) + Vector3.up * (playerAiming.getThrowModifier()));

                    }
                }
            }
           

            if (affectedObject == null)
            {
                waitForAfter.Add(inst);
                continue;
            }
            modifyGameObject(affectedObject, inst);
           
        }

        if (affectedObject == null)
        {
            return;
        }
        foreach (string inst in waitForAfter)
        {
            modifyGameObject(affectedObject, inst);
        }
    }

    public void modifyGameObject(GameObject g, string inst)
    {
        string code = getInstructionCode(inst).ToString();
        string data = getAfterInstructionCode(inst);
        List<string> instructionData = getDataFromInstruction(data);
        

        
        if (code == getInstructionCode(InstructionType.POWERUP_USE))
        {
            //talk to the powerup script (going to wait for gareths push for this one)
        }
       /*
        if (code == getInstructionCode(InstructionType.CREATE_GAMEOBJECT))
        {

        }
       */

    }

    public string encodeInstruction(InstructionType type, string m)
    {

        return getInstructionCode(type) + m;
    }

    public NetworkInstruction getInstruction(InstructionType type, string m)
    {
        NetworkInstruction instruction = new NetworkInstruction(type,m);
        return instruction;
    }

    private void sendTCPMessage(string message)
    {
        message += "|" + identifier;
        TCPSocket.Send(Encoding.UTF8.GetBytes(message), 0, message.Length, SocketFlags.None);
        //Debug.Log("sent message TCP:" + message);
    }

    private void sendUDPMessage(string message)
    {
        //Debug.Log("sending message UDP:" + message);
        message += "|" + identifier;
        UDPSocketSending.SendTo(Encoding.UTF8.GetBytes(message), sendingEndPointUDP);
        Debug.Log("sent message UDP:" + message);
    }

    public bool setupNetworking()
    {
        return setupEndPoint();
       


        //setupTCPClient();
        //setupUDPClient();
    }

    public string getInstructionCode(InstructionType type)
    {
        return InstructionTypeCodes[(int)type].ToString();
    }

    public void createRoom()
    {
        sendTCPMessage(getInstructionCode(InstructionType.CREATE_ROOM) + username);
    }

    public void joinRoom()
    {
        sendTCPMessage(getInstructionCode(InstructionType.JOIN_ROOM) + roomKey + "|" + username);
    }

    public void requestGameObjectCreation(string prefab)
    {
        sendTCPMessage(getInstructionCode(InstructionType.REGISTER_GAMEOBJECT) + prefab + "|" + username);
    }

    public void startGame()
    {
        sendTCPMessage(getInstructionCode(InstructionType.START_GAME));
    }

    public bool connectToServer()
    {
        if (setupNetworking())
        {
            if (setupTCPClient())
            {
                return true;
            }        
        }
        return false;
    }
}

public class NetworkInstruction
{
    public string localGameObject;//the gameobject in our NVE that we want to affect
    public string localIPAddress;//for indexing the gameobject later
    public InstructionType code;
    public string codeData; //(converted to a string later through JsonUtility)

    public NetworkInstruction(GameObject affected, InstructionType instruction, object data )
    {
        localGameObject = NetworkManager.instance.getGameObjectCode(affected);
        localIPAddress = NetworkManager.instance.getIPAddress();
        code = instruction;
        codeData = JsonUtility.ToJson(data);
    }

    public NetworkInstruction(InstructionType type, string s)
    {
        code = type;
        codeData = s;
    }

    public string getString()
    {
        return NetworkManager.instance.getCodes()[(int)code] + codeData;
    }
   
}



public class NetworkedClient {
    IPAddress address;
    string name;
    Socket clientSocket;

    public NetworkedClient(IPAddress ip, Socket socket, string playerName)
    {
        name = playerName;
        address = ip;
        clientSocket = socket;
    }

    public Socket getSocket()
    {
        return clientSocket;
    }

    public IPAddress getAddress()
    {
        return address;

    }

    public string getPlayerName()
    {
        return name;
    }
}


[CustomEditor(typeof(NetworkManager))]
public class NetworkManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

       
        if (GUILayout.Button("Connect to server"))
        {
            
           // NetworkManager.instance.setupUDPClient();

        }

        if (GUILayout.Button("Create room"))
        {
            NetworkManager.instance.createRoom();
            
        }

        if (GUILayout.Button("Join room"))
        {
            NetworkManager.instance.joinRoom();
         
        }

        if (GUILayout.Button("CreatePlayer"))
        {
           
            NetworkManager.instance.requestGameObjectCreation("PlayerSingle");
        }

        /*
        if (GUILayout.Button("Send Message"))
        {
            NetworkManager.instance.sendTCPMessage();
        }
        */


       
        
    }
}



