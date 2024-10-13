using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using Protocols;

namespace TCPServer
{
    public class ClientSocket
    {
        public static int CLIENT_BEGIN_ID = 1;
        public static int CLIENT_COUNT = 0;
        public int clientID;
        private Socket socket;
        private ServerSocket serverSocket;

        public ClientSocket(Socket clientSocket, ServerSocket serverSocket)
        {
            socket = clientSocket;
            this.serverSocket = serverSocket;
            clientID = CLIENT_BEGIN_ID++;
            CLIENT_COUNT++;
        }

        public void SendMsg(byte[] msg)
        {
            if (socket == null)
            {
                return; // 直接返回，避免继续执行
            }
            try
            {
                socket.Send(msg);
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SendMsg SocketException: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"SendMsg Exception: {e.Message}");
            }
        }

        public void Close()
        {
            try
            {
                CLIENT_COUNT--;
                Console.WriteLine("IP:{0}断开...已连接IP数{1}", socket.RemoteEndPoint.ToString(), ClientSocket.CLIENT_COUNT);
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException) { /* 处理已关闭的socket */ }
            finally
            {
                socket.Close();
                socket = null;
            }
        }

        // 接收消息
        public void ReceiveClientMsg()
        {
            if (socket == null) return;

            try
            {
                byte[] msgBytes = new byte[1024];
                int msgLength = socket.Receive(msgBytes);
                if (msgLength > 0)
                {
                    // 根据接收到的字节长度创建新的字节数组
                    byte[] tempMsg = new byte[msgLength];
                    Buffer.BlockCopy(msgBytes, 0, tempMsg, 0, msgLength);

                    // 反序列化消息
                    BaseMessage receivedMsg = BaseMessage.Parser.ParseFrom(tempMsg);
                    HandleMessage(receivedMsg);
                }
                else
                {
                    Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"ReceiveClientMsg SocketException: {e.Message}");
                Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"ReceiveClientMsg Exception: {e.Message}");
            }
        }

        private void HandleMessage(BaseMessage message)
        {
            // 根据消息类型处理接收到的消息
            string messageContent = Encoding.UTF8.GetString(message.Data.ToArray()); // 将字节数组转换为字符串

            switch (message.Type)
            {
                case MsgType.Hello:
                    Console.WriteLine($"收到HELLO消息: {messageContent}");
                    break;
                case MsgType.Exit:
                    Console.WriteLine($"收到EXIT消息: {messageContent}");
                    break;
                // 添加其他消息处理逻辑
                default:
                    Console.WriteLine($"收到未知消息类型: {message.Type}");
                    break;
            }
        }

    }
}
