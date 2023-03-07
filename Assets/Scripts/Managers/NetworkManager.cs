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

public class NetworkManager : MonoBehaviour
{
    //misc
    List<string> InstructionTypeCodes = new List<string>{"@","!","#","^","$", "~", "*", "=","%","&"};
    public bool isHost = false;
    public float secondsBetweenUpdates = 0.5f;
    float secondsSinceLastUpdate = 0f;
    public int TCPPort = 8888;
    public int UDPPort = 8889;
    public string serverIP;
    public float timeoutSeconds = 2f;
    int playersConnected = 0;

    public static NetworkManager instance;
    public string roomKey = "JOHN";
    public string message = "";

  


    //network and socket stuff
    private static byte[] buffer = new byte[512];
    private static IPHostEntry hostInfo;
    private static IPAddress ip;
    private static IPEndPoint endPointTCP;//the IP and port of the recipient for TCP
    private static IPEndPoint endPointUDP;//the IP and port of the recipient for UDP
    private static Socket TCPSocket;
    private static Socket UDPSocket;
    Dictionary<NetworkScript, List<NetworkInstruction>> queuedInstructions = new Dictionary<NetworkScript, List<NetworkInstruction>>();
    Thread TCPListener;

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

    public int getModifiedGameobjectID(List<string> instructions)
    {
        
        foreach(string s in instructions)
        {
            if(s[0].ToString() == InstructionTypeCodes[(int)InstructionType.LOCAL_GAMEOBJECT_ID])
            {
                return int.Parse(s.Substring(1, s.Length - 1));
            }
        }
        return -1;
    }

    //receive functions for TCP and UDP
    private void setupUDPClient()
    {
       
        
        Debug.Log("Setup UDP Client");
        setupUDP = true;

    }

    public void setupTCPClient()//https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services
    {
        
        
        TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
        

        TCPSocket.Connect(endPointTCP);


        TCPListener = new Thread(TCPMessageListener);
        TCPListener.Start();

        Debug.Log("Setup TCP Client");
        setupTCP = true;
        
    } 

    


    private void setupEndPoint()
    {
        //https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services
        try
        {


            //hostInfo = await Dns.GetHostEntryAsync(serverIP);
            ip = IPAddress.Parse(serverIP);//hostInfo.AddressList[0];
           
            endPointUDP = new IPEndPoint(ip, UDPPort);

            endPointTCP = new IPEndPoint(ip, TCPPort);

            hasSetupEndpoint = true;
           
            Debug.Log("Setup endpoint");
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


    public void registerGameObject(GameObject g)
    {
        //send request to server
    }

    public void createAndRegisterGameObject(string prefab, Transform objectTransform)
    {
        //create gameobject in world
        GameObject newObject = Resources.Load<GameObject>(prefab);
        registerGameObject(newObject);
       





    }

    public void queueInstruction(NetworkScript script, NetworkInstruction instruction)
    {
        if (!queuedInstructions.ContainsKey(script))
        {
            queuedInstructions.Add(script, new List<NetworkInstruction>());
        }

        foreach(NetworkInstruction i in queuedInstructions[script])//go through all instructions queued by the script
        {
            if(i.code == instruction.code)//check if an instruction with the same code has been issued
            {
                i.codeData = instruction.codeData;//if so, replace it
                return;
            }
        }
        queuedInstructions[script].Add(instruction);//at this point, there is a list of instructions made, and the type of instruction is unique, so add it in to the list
    }


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
       // gameobjectIDs.Add(new List<int>());//reserve list of gameobjects at index
        setupNetworking();
        
    }

    private void Update()
    {
       
        if (!setupTCP)
        {
            return;
        }
        

        secondsSinceLastUpdate += Time.deltaTime;
        if(secondsSinceLastUpdate < secondsBetweenUpdates)
        {
            return;
           
        }
        secondsSinceLastUpdate = 0;


       while(incomingInstructions.Count > 0)
        {
            Debug.Log("Received:" + incomingInstructions.Peek());
            executeInstructions(decodeInstruction(incomingInstructions.Dequeue()));
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
                string TCPMessage = Encoding.ASCII.GetString(receivedTCP,0,receivedTCPSize);
                incomingInstructions.Enqueue(TCPMessage);
                
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
            
            if(code == getInstructionCode(InstructionType.LOCAL_GAMEOBJECT_ID))
            {
                affectedObject = (GameObject)EditorUtility.InstanceIDToObject(int.Parse(data));
            }
            if (code == getInstructionCode(InstructionType.POSITION_CHANGE))
            {
                waitForAfter.Add(inst);
            }

            if (code == getInstructionCode(InstructionType.REGISTER_GAMEOBJECT))
            {//will be to create an object with a string prefab
                Debug.Log("creating object " + data);

                List<string> instructionData = getDataFromInstruction(data);
                string prefab = instructionData[0];
                
                string index = instructionData[1];
                affectedObject = GameManager.createGameObject(prefab);
                sendTCPMessage(getInstructionCode(InstructionType.CREATE_GAMEOBJECT) + index.ToString() + "|" + affectedObject.GetInstanceID().ToString());

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
        if (code == getInstructionCode(InstructionType.POSITION_CHANGE))
        {//will be to create an object with a string prefab
            Vector3 dataPosition = JsonUtility.FromJson<Vector3>(data);
            g.transform.position = dataPosition;
           
        }

        if (code == getInstructionCode(InstructionType.VELOCITY_CHANGE))
        {
            
        }

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

    public void sendTCPMessage(string message)
    {
        TCPSocket.Send(Encoding.ASCII.GetBytes(message), 0, message.Length, SocketFlags.None);
        Debug.Log("sent message " + message);
    }

    public void setupNetworking()
    {
        setupEndPoint();
       


        //setupTCPClient();
        //setupUDPClient();
    }

    public string getInstructionCode(InstructionType type)
    {
        return InstructionTypeCodes[(int)type].ToString();
    }

    public void createRoom()
    {
        sendTCPMessage(getInstructionCode(InstructionType.CREATE_ROOM));
    }

    public void joinRoom()
    {
        sendTCPMessage(getInstructionCode(InstructionType.JOIN_ROOM) + roomKey);
    }

    public void requestGameObjectCreation()
    {
        sendTCPMessage(getInstructionCode(InstructionType.REGISTER_GAMEOBJECT) + message + getInstructionCode(InstructionType.POSITION_CHANGE) + JsonUtility.ToJson(gameObject.transform.position));
    }
}

public class NetworkInstruction
{
    public string localGameObject;//the gameobject in our NVE that we want to affect
    public string localIPAddress;//for indexing the gameobject later
    public InstructionType code;
    public object codeData; //(converted to a string later through JsonUtility)

    public NetworkInstruction(GameObject affected, InstructionType instruction, object data )
    {
        localGameObject = NetworkManager.instance.getGameObjectCode(affected);
        localIPAddress = NetworkManager.instance.getIPAddress();
        code = instruction;
        codeData = data;
    }

    public string getString()
    {
        return NetworkManager.instance.getCodes()[(int)code] + JsonUtility.ToJson(codeData);
    }
   
}

public enum InstructionType
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
/*
 * CODES FOR NETWORKED STRING
 * @ (int) - gameobject ID to modify
 * ! (powerupType) - powerup used (giving powerups can be local) (called in onUse for powerups)
 * # (Transform) - set transform change
 * $ (Vec3) - velocity change (for dead reckoning)
 * 
 * 
 * TCP requests wont be replaced, UDP requests of the same type will be replaced with the latest version
 * to send data, call the function XXXX which will check if there is already an instruction to modify the same type. if there is, then it will be replaced.
 */

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
            NetworkManager.instance.setupTCPClient();
        }

        if (GUILayout.Button("Create room"))
        {
            NetworkManager.instance.createRoom();
        }

        if (GUILayout.Button("Join room"))
        {
            NetworkManager.instance.joinRoom();
        }

        if (GUILayout.Button("Send Message"))
        {
            NetworkManager.instance.sendTCPMessage(NetworkManager.instance.message);
        }

        if (GUILayout.Button("Create gameobject"))
        {
            NetworkManager.instance.requestGameObjectCreation();
        }
    }
}



