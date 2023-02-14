using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Client : MonoBehaviour
{
    private static byte[] outBuffer = new byte[512];
    private static IPEndPoint serverEndPoint;
    private static Socket client;
    public GameObject cube;

    public static void StartClient()
    {
        try
        {
            IPAddress ip = IPAddress.Parse("10.150.13.231");//10.160.18.158 is mine

            serverEndPoint = new IPEndPoint(ip, 8888);

            client = new Socket(ip.AddressFamily,SocketType.Dgram,ProtocolType.Udp);

            

        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartClient();
        
    }

    // Update is called once per frame
    void Update()
    {
        outBuffer = Encoding.ASCII.GetBytes(cube.transform.position.ToString());
        client.SendTo(outBuffer, serverEndPoint);
    }
}
