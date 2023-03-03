using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEditor;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    //misc
    public List<string> InstructionTypeCodes = new List<string>{"@","!","#","$","+"};
    public bool isServer = false;
    public float secondsBetweenUpdates = 0.5f;
    public int TCPPort = 8888;
    public int UDPPort = 8889;
    public string serverIP;
    public float timeoutSeconds = 2f;
    int playersConnected = 0;

    public static NetworkManager instance;

  


    //network and socket stuff
    private static byte[] buffer = new byte[512];
    private static IPHostEntry hostInfo;
    private static IPAddress ip;
    private static IPEndPoint endPointTCP;//the IP and port of the recipient for TCP
    private static IPEndPoint endPointUDP;//the IP and port of the recipient for UDP
    private static Socket TCPSocket;
    private static Socket UDPSocket;
    Dictionary<NetworkScript, List<NetworkInstruction>> queuedInstructions = new Dictionary<NetworkScript, List<NetworkInstruction>>();


    //client only
    public static EndPoint receiver;

    //server only (now a console app)
    //List<NetworkedClient> clients = new List<NetworkedClient>();
    //List<List<int>> gameobjectIDs = new List<List<int>>();//for server only

    //booleans for async tasks
    bool hasSetupEndpoint = false;
    bool setupTCP = false;
    bool setupUDP = false;

    //receive functions for TCP and UDP
    private void setupUDPClient()
    {
        if (isServer)
        {
            return;
        }
        
        Debug.Log("Setup UDP Client");
        setupUDP = true;

    }

    private async void setupTCPClient()//https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services
    {
        if (isServer)
        {
            return;
        }
        
        TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
       
        await TCPSocket.ConnectAsync(endPointTCP);
        Debug.Log("Setup TCP Client");
        setupTCP = true;
        
    } 

    private void setupTCPServer()
    {
        if(endPointTCP == null)
        {
            Debug.Log("ENDPOINT NULL");
        }
        
        TCPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);

        try
        {
            TCPSocket.Bind(endPointTCP);
            TCPSocket.Listen(4);//# of players, pretty much
            //TCPSocket.Accept(); //adding this in makes the editor always get stuck on EditorPlayMode or some crap, never add this line in here
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        setupTCP = true;
        Debug.Log("Setup TCP Server");
    }
    private void setupUDPServer()
    {
        setupUDP = true;
        Debug.Log("Setup UDP Server");
    }

    private void setupEndPoint()
    {
        //https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services
        try
        {


            //hostInfo = await Dns.GetHostEntryAsync(serverIP);
            ip = IPAddress.Parse(serverIP);//hostInfo.AddressList[0];
            if (isServer)
            {
                ip = IPAddress.Any;//so we listen for any incoming connections
            }
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

    public void waitForNewClient()
    {
        //Socket client = TCPSocket.Accept();//will stop here
        TCPSocket.Accept();
        playersConnected++;
        //registerClient(client.)
    }

    public void registerClient(IPAddress address, Socket s, string name)
    {
       
        
          
        
        
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
        if(playersConnected == 0)
        {
            return;
        }
        int recv = TCPSocket.Receive(buffer);
        if (recv > 0)
        {
            Debug.Log(Encoding.ASCII.GetString(buffer, 0, recv));

            //byte[] sending = new byte[512];
            recv = 0;

        }
    }

    public void setupNetworking()
    {
        setupEndPoint();
        if (isServer)
        {
            setupTCPServer();
            setupUDPServer();
            return;
        }


        setupTCPClient();
        setupUDPClient();
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
        return NetworkManager.instance.InstructionTypeCodes[(int)code] + JsonUtility.ToJson(codeData);
    }
   
}

public enum InstructionType
{
    LOCAL_GAMEOBJECT_ID,
    POWERUP_USE,
    TRANSFORM_CHANGE,
    VELOCITY_CHANGE,
    REGISTER_GAMEOBJECT
}
/*
 * CODES FOR NETWORKED STRING
 * @ (int) - gameobject ID to modify
 * ! (powerupType) - powerup used (giving powerups can be local) (called in onUse for powerups)
 * # (Transform) - set transform change
 * $ (Vec3) - velocity change (for dead reckoning)
 * 
 * | - used to separate instructions
 * 
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

       
        if (GUILayout.Button("Wait for client"))
        {
            NetworkManager.instance.waitForNewClient();
        }
    }
}



