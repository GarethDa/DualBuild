using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public bool isServer = false;
    public static string hostIP;
    public static int port = 8888;
    public float secondsBetweenUpdates = 0.5f;
    float secondsElapsed = 0f;

    //client
    private static byte[] outBuffer = new byte[512];
    private static IPEndPoint serverEndPoint;
    private static Socket client;//client socket to server (client > server)
    private List<NetData> miscDataToSend = new List<NetData>();//game state changes, dodgeball throws stuff like that
    private NetData playerTransform = new NetData();//player movement and rotation



    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        if (isServer)
        {
            startServer();
        }
        StartClient();
    }


    public static void StartClient()
    {
        try
        {
            IPAddress ip = IPAddress.Parse(hostIP);

            serverEndPoint = new IPEndPoint(ip, port);

            client = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);



        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public static void startServer()
    {
        //just receiving data, buffer for sending
        byte[] buffer = new byte[512];
        //getting ip from machine automatically
        IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());

        IPAddress ip = hostInfo.AddressList[01];//all network interfaces on your machine
        //0 is v6, 1 is v4

        //Console.WriteLine("HOST: {0} , IP {1}", hostInfo.HostName, ip.ToString());

        IPEndPoint localEndPoint = new IPEndPoint(ip, port);

        Socket server = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);//this is our server

        EndPoint client = new IPEndPoint(IPAddress.Any, 0);//can be from any ip address in the world. DNS sets the port

        //we dont have to listen for anything

        try
        {
            server.Bind(localEndPoint);

            Console.WriteLine("waiting for data");

            //lets always receive data

            while (true)
            {
                int receivedBytesCount = server.ReceiveFrom(buffer, ref client);
                //server.sendto 
                if (receivedBytesCount == 0)
                {
                    continue;
                }
               // Console.WriteLine("Received from: {0} -> {1}", client.ToString(), Encoding.ASCII.GetString(buffer, 0, receivedBytesCount));
            }

           
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR: {0}", e.Message);
        }

    }

   

    // Update is called once per frame
    void Update()
    {
        secondsElapsed += Time.deltaTime;
        if(secondsElapsed >= secondsBetweenUpdates)
        {
            //send data to server
            playerTransform = new NetData(GameManager.instance.clientPlayer.transform);

            outBuffer = new byte[512];
            outBuffer = Encoding.ASCII.GetBytes(playerTransform.getDataString());
            client.SendTo(outBuffer, serverEndPoint);

            foreach (NetData data in miscDataToSend)
            {
                outBuffer = new byte[512];
                outBuffer = Encoding.ASCII.GetBytes(data.getDataString());
                client.SendTo(outBuffer, serverEndPoint);
                
            }
        }
    }
}

public class NetData
{
    string sendingData = "";

    public NetData() { }


    public NetData(Transform t)
    {
        sendingData += t.position.ToString() + "\n";
        sendingData += t.rotation.eulerAngles.ToString() + "\n";
        sendingData += t.localScale.ToString() + "\n";
    }

    public string getDataString()
    {
        return sendingData;
    }
}


