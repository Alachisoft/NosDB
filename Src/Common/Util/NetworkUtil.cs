// /*
// * Copyright (c) 2016, Alachisoft. All Rights Reserved.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// * http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Alachisoft.NosDB.Common.Communication;

namespace Alachisoft.NosDB.Common.Util
{
    public class NetworkUtil
    {
        public const int DEFAULT_CS_HOST_PORT = 9950;
        public const int DEFAULT_DB_HOST_PORT = 9960;
        public const int DEFAULT_DISTRIBUTOR_HOST_PORT = 9970;
        private static IPAddress _localIP;

        public static void ReadFromTcpSocket(Socket socket,byte[] buffer)
        {
            ReadFromTcpSocket(socket, buffer, 0, buffer.Length);
        }
        public static void ReadFromTcpSocket(Socket socket, byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new IndexOutOfRangeException();

            while(count >0)
            {
                int receivedBytes = socket.Receive(buffer, offset, count, SocketFlags.None);

                if (receivedBytes == 0)
                {
                    throw  new SocketException();;
                }

                offset += receivedBytes;
                count -= receivedBytes;

            }
        }

        public static void ReadFromTcpConnection(IConnection connection, byte[] buffer)
        {
            ReadFromTcpConnection(connection, buffer, 0, buffer.Length);
        }

        public static void ReadFromTcpConnection(IConnection connection, byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
                throw new IndexOutOfRangeException();

            if (!connection.Receive(buffer, count))
                throw new SocketException();
        }

        public static IPAddress GetLocalIPAddress()
        {
            if (_localIP != null)
                return _localIP;

            string ip = ConfigurationSettings.AppSettings["LocalIP"];
            if (ip != null)
            {
                return IPAddress.Parse(ip);
            }
            
            System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            if (hostEntry.AddressList != null)
            { 
                foreach (System.Net.IPAddress addr in hostEntry.AddressList)
                {
                    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return addr;

                    }
                }
            }
            return System.Net.IPAddress.Parse("127.0.0.1");
        }

        public static IPAddress GetVerifedLocalIP(string localIP)
        {
            if (string.IsNullOrEmpty(localIP))
                throw new Exception("IPAddress cannot be null.");

            System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            if (hostEntry.AddressList != null)
            {
                foreach (System.Net.IPAddress addr in hostEntry.AddressList)
                {
                    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && addr.ToString().Equals(localIP))
                    {
                        _localIP = IPAddress.Parse(localIP);
                        return addr;
                    }
                }
            }
            throw new Exception("Invalid IPAddress specified. value '"+localIP+"'");
        }
    }
}
