using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace UTalk
{
    public class TalkToPerson
    {
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);
        // The response from the remote device.     
        private static string response = string.Empty;
        public static string ServerIP = string.Empty;

        public static void StartClient()
        {
            IPEndPoint ipEndp = new IPEndPoint(IPAddress.Parse(ServerIP), 11000);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.BeginConnect(ipEndp, new AsyncCallback(ConnectCallBack), clientSocket);
            connectDone.WaitOne();

            Thread send = new Thread(Send);
            Thread receive = new Thread(Receive);
            send.Start(clientSocket);
            receive.Start(clientSocket);
        }
        public static void ConnectCallBack(IAsyncResult ar)
        {
            Socket clientSocket = (Socket)ar.AsyncState;
            clientSocket.EndConnect(ar);
            Console.WriteLine("已连接到：" + clientSocket.RemoteEndPoint.ToString());
            connectDone.Set();
        }
        public static void Send(Object clientSocket)
        {
            while (true)
            {
                sendDone.Reset();
                StateObject stateSend = new StateObject();
                string content = Console.ReadLine();
                Console.SetCursorPosition(0, Console.CursorTop-1);
                Console.WriteLine("Me: "+content);
                string sendString = content + "*<EOF>";
                //send
                stateSend.buffer = Encoding.UTF8.GetBytes(sendString);
                stateSend.workSocket = (Socket)clientSocket;
                stateSend.workSocket.BeginSend(stateSend.buffer, 0, stateSend.buffer.Length, 0,
                    new AsyncCallback(SendCallBack), stateSend);
                sendDone.WaitOne();
            }
        }
        public static void SendCallBack(IAsyncResult ar)
        {
            sendDone.Set();
            StateObject state = (StateObject)ar.AsyncState;
            int sendBytes = state.workSocket.EndSend(ar);
            //Console.WriteLine("Sent {0} bytes to Server:{1}", sendBytes, state.workSocket.RemoteEndPoint.ToString());
        }
        public static void Receive(Object clientSocket)
        {
            while(true)
            {
                receiveDone.Reset();
                StateObject state = new StateObject();
                state.workSocket = (Socket)clientSocket;
                state.workSocket.BeginReceive(state.buffer, 0, state.buffer.Length, 0,
                    new AsyncCallback(ReceiveCallBack), state);
                receiveDone.WaitOne();
            }
        }
        public static void ReceiveCallBack(IAsyncResult ar)
        {
            
            StateObject state = (StateObject)ar.AsyncState;
            int receiveBytes = state.workSocket.EndReceive(ar);
            state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, receiveBytes));
            if (state.sb.ToString().IndexOf("<EOF>") > -1)//读取完毕
            {
                receiveDone.Set();
                response = state.sb.ToString();
                Console.WriteLine("Ta: " + response.Split('*')[0]);
            }
            else
            {
                state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallBack), state);
            }
        }
    }
}
