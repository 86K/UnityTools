using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tools
{
    public static class NetworkHelper
    {
        /// <summary>
        /// 检查网络是否可用
        /// </summary>
        /// <returns></returns>
        public static bool CheckNetworkAvailable()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface netInterface in networkInterfaces)
            {
                if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    if (netInterface.OperationalStatus == OperationalStatus.Up)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 从网络获取当前准确时间（基于可信 NTP 协议）
        /// </summary>
        public static async UniTask<DateTime> GetDateTimeByNetwork()
        {
            // 若失败则 fallback 本地时间
            DateTime fallback = DateTime.Now;

            try
            {
                // 可靠 NTP 时间服务器（Google / Microsoft / NIST）
                string[] servers =
                {
                    "time.google.com",
                    "time.windows.com",
                    "time.nist.gov",
                };

                foreach (var server in servers)
                {
                    try
                    {
                        DateTime ntpTime = await QueryNtpTime(server);
                        return ntpTime;
                    }
                    catch
                    {
                        // 尝试下一个服务器
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Get datetime by network failed：\n{e}");
            }

            return fallback;
        }

        /// <summary>
        /// 查询 NTP 服务器时间（RFC 5905 标准）
        /// </summary>
        static async UniTask<DateTime> QueryNtpTime(string host)
        {
            const int ntpPort = 123;
            byte[] ntpData = new byte[48];
            ntpData[0] = 0x1B; // LI=0, VN=3/4, Mode=3 (Client)

            using UdpClient udp = new UdpClient();
            udp.Client.ReceiveTimeout = 3000;
            udp.Connect(host, ntpPort);

            await udp.SendAsync(ntpData, ntpData.Length);
            var result = await udp.ReceiveAsync();
            byte[] buffer = result.Buffer;

            const byte offsetTransmitTime = 40;

            ulong intPart = (ulong)buffer[offsetTransmitTime] << 24 |
                            (ulong)buffer[offsetTransmitTime + 1] << 16 |
                            (ulong)buffer[offsetTransmitTime + 2] << 8 |
                            buffer[offsetTransmitTime + 3];

            ulong fractPart = (ulong)buffer[offsetTransmitTime + 4] << 24 |
                              (ulong)buffer[offsetTransmitTime + 5] << 16 |
                              (ulong)buffer[offsetTransmitTime + 6] << 8 |
                              buffer[offsetTransmitTime + 7];

            ulong milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000UL);

            DateTime ntpEpoch = new DateTime(1900, 1, 1);
            DateTime networkDateTime = ntpEpoch.AddMilliseconds((long)milliseconds).ToLocalTime();

            return networkDateTime;
        }

        
    }
}