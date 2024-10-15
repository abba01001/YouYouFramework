using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protocols;
using Protocols.Item;

namespace TCPServer
{
    public class ClientSocket
    {
        public static int CLIENT_BEGIN_ID = 1;
        public static int CLIENT_COUNT = 0;
        public int clientID;
        private Socket socket;

        public RequestHandler Request;
        public ResponseHandler Response;

        public ClientSocket(Socket clientSocket)
        {
            socket = clientSocket;
            Request = new RequestHandler(socket);
            Response = new ResponseHandler(socket);
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

        // 发送消息
        public void SendMessage<T>(MsgType messageType, T data) where T : IMessage<T>
        {
            ServerSocket.Logger.LogMessage(this.socket, $"{data.ToString()}");
            // 将数据对象序列化为字节数组
            byte[] byteArrayData = data.ToByteArray();
            var message = new BaseMessage
            {
                //MessageId = currentMessageId++, // 获取唯一消息ID并递增
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), // 获取当前时间戳
                SenderId = socket.RemoteEndPoint.ToString(), // 设置发送者ID
                Type = messageType,
                Data = ByteString.CopyFrom(byteArrayData) // 直接将序列化后的字节数组放入 Data
            };
            byte[] messageBytes = message.ToByteArray();
            SendMsg(messageBytes);
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
        public void ReceiveMsg()
        {
            if (socket == null) return;
            try
            {
                byte[] msgBytes = new byte[1024];
                int msgLength = socket.Receive(msgBytes);
                Console.WriteLine($"收到信息: {msgBytes}");
                if (msgLength > 0)
                {
                    byte[] tempMsg = new byte[msgLength];
                    Buffer.BlockCopy(msgBytes, 0, tempMsg, 0, msgLength);
                    Console.WriteLine($"收到信息: {BitConverter.ToString(tempMsg)}");
                    BaseMessage receivedMsg = BaseMessage.Parser.ParseFrom(tempMsg);
                    Console.WriteLine($"收到的消息类型: {receivedMsg.Type}");
                    Response.HandleResponse(receivedMsg);
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
    }
}
