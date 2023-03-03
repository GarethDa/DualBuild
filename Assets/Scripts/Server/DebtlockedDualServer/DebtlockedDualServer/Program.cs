﻿using System;
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
        private static byte[] buffer = new byte[512];
        private static Socket server;
        private static byte[] outBuffer = new byte[512];
        private static string outMsg = "";
        private static float[] pos;
        static void Main(string[] args)
        {
            Console.WriteLine("Starting the server....");
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse("172.31.63.101");//private IP of the server
            server.Bind(new IPEndPoint(ip, 8888));
            server.Listen(10);
            server.BeginAccept(new AsyncCallback(AcceptCallback), null);
            Console.Read();
        }
        private static void AcceptCallback(IAsyncResult result)
        {
            Socket client = server.EndAccept(result);
            Console.WriteLine("Client connected!!!   IP:");
            client.BeginReceive(buffer, 0, buffer.Length, 0,
                new AsyncCallback(ReceiveCallback), client);
        }
        private static void ReceiveCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            int rec = socket.EndReceive(result);
            //lec05
            //byte[] data = new byte[rec];
            //Array.Copy(buffer, data, rec);
            //lec06
            pos = new float[rec / 4];
            Buffer.BlockCopy(buffer, 0, pos, 0, rec);
            //string msg = Encoding.ASCII.GetString(data);
            //Console.WriteLine("Recv: " + msg);
            socket.BeginSend(buffer, 0, buffer.Length, 0,
                new AsyncCallback(SendCallback), socket);
            Console.WriteLine("Received X:" + pos[0] + " Y:" + pos[1] + " Z:" +
pos[2]);
            //lec05
            //socket.BeginSend(data, 0, data.Length, 0, 
            //    new AsyncCallback(SendCallback), socket);
            socket.BeginReceive(buffer, 0, buffer.Length, 0,
                new AsyncCallback(ReceiveCallback), socket);
        }
        private static void SendCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        }
    }
}