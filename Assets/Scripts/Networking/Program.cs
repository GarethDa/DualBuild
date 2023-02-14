using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class UDPServer
{

    public static void startServer()
    {
        //just receiving data, buffer for sending
        byte[] buffer = new byte[512];
        //getting ip from machine automatically
        IPHostEntry hostInfo = Dns.GetHostEntry(Dns.GetHostName());

        IPAddress ip = hostInfo.AddressList[01];//all network interfaces on your machine
        //0 is v6, 1 is v4

        Console.WriteLine("HOST: {0} , IP {1}",hostInfo.HostName,ip.ToString());

        IPEndPoint localEndPoint = new IPEndPoint(ip,8888);

        Socket server = new Socket(ip.AddressFamily, SocketType.Dgram,ProtocolType.Udp);//this is our server

        EndPoint client = new IPEndPoint(IPAddress.Any,0);//can be from any ip address in the world. DNS sets the port

        //we dont have to listen for anything

        try
        {
            server.Bind(localEndPoint);

            Console.WriteLine("waiting for data");

            //lets always receive data

            while (true)
            {
                int receivedBytesCount = server.ReceiveFrom(buffer,ref client);
                //server.sendto 
                if(receivedBytesCount == 0)
                {
                    continue;
                }
                Console.WriteLine("Received from: {0} -> {1}", client.ToString(), Encoding.ASCII.GetString(buffer,0,receivedBytesCount));
            }

            server.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR: {0}", e.Message);
        }
    
    }
    static void Main(string[] args)
    {
        startServer();
    }
}
