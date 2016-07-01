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
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace Alachisoft.NosDB.Common.Communication
{
    public interface IConnection
    {
        float ClientsBytesSent { get; }

        float ClientBytesReceived { get; }

        void Bind(string ipAddress);

        bool Connect(string serverIP, int port);
        void SetDataListener(IConnectionDataListener listener);
        void Disconnect();
        
        bool Send(byte[] buffer, int offset, int count);
        
        bool Receive(byte[] buffer, int count);
        void ReceiveAsync(byte[] buffer, int count, Object context);
        
        bool IsConnected { get; }
    }
}
