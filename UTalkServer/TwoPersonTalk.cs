using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UTalkServer
{
    public class TwoPersonTalk
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private static ManualResetEvent client1Done = new ManualResetEvent(false);
        private static ManualResetEvent client2Done = new ManualResetEvent(false);

        static int i = 0;//控制，只有两个连接
        static Socket[] clientSocket = new Socket[2];
        public static string ServerIP = string.Empty;
        public TwoPersonTalk() { }

        public static void StartTalking()
        {
            IPEndPoint sEndp = new IPEndPoint(IPAddress.Parse(ServerIP), 11000);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(sEndp);
            listener.Listen(0);
            Console.WriteLine("Start listening...");
            Thread th1 = new Thread(Client1Run);
            Thread th2 = new Thread(Client2Run);
            
            //AsynAccecpt
            while (true)
            {
                //控制连接个数小于等于2
                allDone.Reset();
                if (i < 2) allDone.Set();
                if(i == 2)
                {
                    th1.Start();
                    th2.Start();
                }
                allDone.WaitOne(); 
                
                allDone.Reset();
                listener.BeginAccept(new AsyncCallback(AcceptCallBack), listener);
                allDone.WaitOne();
            }
        }
        public static void Client1Run()
        {
            while(true)
            {
                client1Done.Reset();
                StateObject state = new StateObject(1);
                state.workSocket = clientSocket[0];
                clientSocket[0].BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallBack), state);
                client1Done.WaitOne();
            }
        }
        public static void Client2Run()
        {
            while (true)
            {
                client2Done.Reset();
                StateObject state = new StateObject(2);
                state.workSocket = clientSocket[1];
                clientSocket[1].BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallBack), state);
                client2Done.WaitOne();
            }
        }
        public static void AcceptCallBack(IAsyncResult ar)
        {
            Socket handle = (Socket)ar.AsyncState;
            Socket serverSocket = handle.EndAccept(ar);
            clientSocket[i] = serverSocket;
            Console.WriteLine("成功连接："+serverSocket.RemoteEndPoint.ToString());
            ++i;
            allDone.Set();
        }
        public static void ReceiveCallBack(IAsyncResult ar)
        {
            string content = string.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handle = state.workSocket;
            int bytes = handle.EndReceive(ar);
            state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytes));
            if (state.sb.ToString().IndexOf("<EOF>") > -1)//读取完毕
            {
                content = state.sb.ToString();
                Console.WriteLine("From " + state.workSocket.RemoteEndPoint.ToString() + ": " + content.Split('*')[0]);
                string[] data = content.Split(':');
                if(state.client == 1)
                {
                    client1Done.Set();
                    Send(clientSocket[1], content);
                }
                if(state.client == 2)
                {
                    client2Done.Set();
                    Send(clientSocket[0], content);
                }
            }
            else
            {
                handle.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallBack), state);
            }
        }
        public static void Send(Socket handle, string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            handle.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), handle);
        }
        public static void SendCallBack(IAsyncResult ar)
        {
            Socket handle = (Socket)ar.AsyncState;
            int bytesSent = handle.EndSend(ar);
        }
    }
}
