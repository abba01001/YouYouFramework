using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPServer
{
    public class ClientSocket
    {
        public static int CLIENT_BEGIN_ID = 1;
        public int clientID;
        private Socket socket;
        private ServerSocket serverSocket;

        public ClientSocket(Socket clientSocket, ServerSocket serverSocket)
        {
            socket = clientSocket;
            this.serverSocket = serverSocket;

            clientID = CLIENT_BEGIN_ID;
            ++CLIENT_BEGIN_ID;
        }

        //发送消息
        public void SendMsg(byte[] msg)
        {
            if(socket != null)
                socket.Send(msg);
        }

        //接收消息
        public void ReceiveClientMsg()
        {
            if (socket == null)
                return;
            if(socket.Available > 0)
            {
                byte[] msgBytes = new byte[1024*1024];
                int msgLength = socket.Receive(msgBytes);
                //ThreadPool.QueueUserWorkItem(HandleMsg, Encoding.UTF8.GetString(msgBytes, 0, msgLength));


                // 处理客户端发送的消息
                string message = Encoding.UTF8.GetString(msgBytes, 0, msgLength);
                if (message == "客户端断开连接")
                {
                    // 关闭服务端socket
                    Close();
                    return;
                }


                byte[] tempMsg = new byte[msgLength];
                Buffer.BlockCopy(msgBytes, 0, tempMsg, 0, msgLength);
                serverSocket.BroadcastMsg(tempMsg, clientID);
            }
        }
        ////处理消息
        //private void HandleMsg(object obj)
        //{
        //    string msg = obj as string;
        //    Console.WriteLine("客户端{0}：{1}", socket.RemoteEndPoint.ToString(), msg);
        //    serverSocket.BroadcastMsg(msg, clientID);
        //}

        //释放连接
        public void Close()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }
    }
}
