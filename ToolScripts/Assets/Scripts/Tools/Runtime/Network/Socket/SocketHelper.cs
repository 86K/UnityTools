namespace Tools
{
    using UnityEngine;
    using System.Net.Sockets;
    using System.Text;
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using System.Threading;

    /// <summary>
    /// 基于TCP的原生Socket辅助器。
    /// 传输的是字节数据，前四个字节是数据长度。
    /// </summary>
    public class SocketHelper
    {
        private Socket socket;
         
        private byte[] dataBuffer;
        private readonly List<byte> cacheBuffer = new List<byte>();
        
        private bool isConnected;
        private bool isInited;

        private readonly Queue<byte[]> sendQueue = new Queue<byte[]>();
        private readonly Queue<string> receiveQueue = new Queue<string>();
        
        Thread sendThread;
        
        private void Init()
        {
            if (isInited)
                return;

            isInited = true;
            isConnected = false;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            sendThread = new Thread(SendMessageHandler)
            {
                IsBackground = true
            };
        }
        
        /// <summary>
        /// 连接服务端socket。
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            if (isConnected)
                return;
            
            Init();
            
            ConnectedSocket(ip, port);
        }

        private async void ConnectedSocket(string ip, int port)
        {
            //连接服务器
            try
            {
                await socket.ConnectAsync(ip, port);
                isConnected = socket.Connected;
                Debug.LogError($"Socket连接状态'{isConnected}'");

                if (isConnected)
                {
                    //开始接收信息
                    ReceiveData();
                    //发送心跳
                    //StartCoroutine(SendPong());

                    sendThread.Start();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// 接受服务端消息。
        /// </summary>
        void ReceiveData()
        {
            //Socket异步接收数据,buffer存储接收到的数据,offset从零开始计数,size要接收的字节数,SocketFlags值的按位组合
            dataBuffer = new byte[1024];
            socket.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ReceiveCallback, null);
        }

        void ReceiveCallback(System.IAsyncResult iar)
        {
            int len = socket.EndReceive(iar);
            if (len == 0)
            {
                return;
            }

            // 将新接收的数据添加到缓存中
            cacheBuffer.AddRange(new List<byte>(dataBuffer).GetRange(0, len));

            // 处理缓存中的数据，尝试解析出完整的消息
            int offset = 0;
            while (offset < cacheBuffer.Count)
            {
                if (cacheBuffer.Count - offset >= 4)
                {
                    int msgLen = BitConverter.ToInt32(cacheBuffer.ToArray(), offset);
                    if (cacheBuffer.Count - offset >= msgLen + 4)
                    {
                        byte[] msgBytes = cacheBuffer.GetRange(offset + 4, msgLen).ToArray();
                        // 把消息从byte还原为string，可以打印验证
                        string msg = Encoding.UTF8.GetString(msgBytes, 0, msgBytes.Length);
                        // 使用时，从receiveQueue取数据即可
                        receiveQueue.Enqueue(msg);
                        
                        offset += msgLen + 4;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            if (offset < cacheBuffer.Count)
            {
                // 如果还有剩余未处理的数据（半包情况），提取出来继续等待后续接收的数据一起处理
                byte[] remainingBytes = cacheBuffer.GetRange(offset, cacheBuffer.Count - offset).ToArray();
                cacheBuffer.Clear();
                cacheBuffer.AddRange(remainingBytes);
            }
            else
            {
                // 完整拆包后，清空缓存buffer
                cacheBuffer.Clear();
            }

            // 继续接收数据
            ReceiveData();
        }
        
        /// <summary>
        /// 发送消息给服务端socket。
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(string msg)
        {
            if (!isConnected) return;

            //将字符串转换为UTF-8编码的字节数组
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            List<byte> sendBytes = new List<byte>();
            sendBytes.AddRange(BitConverter.GetBytes(bytes.Length));
            sendBytes.AddRange(bytes);
            sendQueue.Enqueue(sendBytes.ToArray());
        }

        public void HandleMessage()
        {
            if (socket != null && isConnected)
            {
                while (receiveQueue.Count > 0)
                {
                    // NOTE：把消息广播出去
                    var msg = receiveQueue.Dequeue();
                    // 
                }
            }
        }
        
        private void SendMessageHandler(object obj)
        {
            while (isConnected)
            {
                //如果消息队列中有消息，则发送消息
                if (sendQueue.Count > 0)
                {
                    socket.Send(sendQueue.Dequeue());
                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// 关闭连接。
        /// </summary>
        public void Close()
        {
            Debug.LogError("关闭Socket连接");
            if (socket != null)
            {
                socket.Close();
                isConnected = false;
                socket = null;
            }

            if (sendThread != null)
            {
                sendThread.Abort();
            }

            sendQueue.Clear();
            receiveQueue.Clear();
            cacheBuffer.Clear();
        }

        /// <summary>
        /// 发送心跳消息。
        /// </summary>
        /// <returns></returns>
        IEnumerator SendPong()
        {
            if (!isConnected)
            {
                yield break;
            }

            byte[] bytes = Encoding.UTF8.GetBytes("Pong");
            List<byte> byteList = new List<byte>();
            byteList.AddRange(BitConverter.GetBytes(bytes.Length));
            byteList.AddRange(bytes);
            byte[] bytesArray = byteList.ToArray();
            int len = bytesArray.Length;
            while (true)
            {
                yield return null;
                socket.BeginSend(bytesArray, 0, len, SocketFlags.None, null, null);
            }
        }
    }
}