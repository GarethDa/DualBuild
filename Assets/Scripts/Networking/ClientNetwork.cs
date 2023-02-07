using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientNetwork : MonoBehaviour
{
    public GameObject myPlayer;
    private static byte[] outBuffer = new byte[512];
    private static IPEndPoint remoteEP;
    private static Socket clientSoc;
    

    public static void StartClient()
	{
        try
        {
            IPAddress ip = IPAddress.Parse("192.168.2.58");
            remoteEP = new IPEndPoint(ip, 8888);

            clientSoc = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        } catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
	}

    void Start()
    {
        myPlayer = GameObject.Find("PlayerSingle");
		StartClient();
    }

    // Update is called once per frame
    void Update()
    {
        outBuffer = Encoding.ASCII.GetBytes(myPlayer.transform.position.x.ToString());

        clientSoc.SendTo(outBuffer, remoteEP);
    }
}
