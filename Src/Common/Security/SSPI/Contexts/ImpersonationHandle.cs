//+Copyright (c) 2014, Kevin Thompson
//+All rights reserved.
//+
//+Redistribution and use in source and binary forms, with or without
//+modification, are permitted provided that the following conditions are met:
//+
//+1. Redistributions of source code must retain the above copyright notice, this
//+   list of conditions and the following disclaimer. 
//+2. Redistributions in binary form must reproduce the above copyright notice,
//+   this list of conditions and the following disclaimer in the documentation
//+   and/or other materials provided with the distribution.
//+
//+THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//+ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//+WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//+DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//+ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//+(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//+LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//+ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//+(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//+SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Common.Security.SSPI.Contexts
{
    /// <summary>
    /// Represents impersonation performed on a server on behalf of a client. 
    /// </summary>
    /// <remarks>
    /// The handle controls the lifetime of impersonation, and will revert the impersonation
    /// if it is disposed, or if it is finalized ie by being leaked and garbage collected.
    /// 
    /// If the handle is accidentally leaked while operations are performed on behalf of the user,
    /// impersonation may be reverted at any arbitrary time, perhaps during those operations.
    /// This may lead to operations being performed in the security context of the server, 
    /// potentially leading to security vulnerabilities.
    /// </remarks>
    public class ImpersonationHandle : IDisposable
    {
        private bool disposed;
        private ServerContext server;

        /// <summary>
        /// Initializes a new instance of the ImpersonationHandle. Does not perform impersonation.
        /// </summary>
        /// <param name="server">The server context that is performing impersonation.</param>
        internal ImpersonationHandle(ServerContext server)
        {
            this.server = server;
            this.disposed = false;
        }

        ~ImpersonationHandle()
        {
            Dispose( false );
        }

        /// <summary>
        /// Reverts the impersonation.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing )
        {
            if( disposing && this.disposed == false && this.server != null && this.server.Disposed == false )
            {
                this.server.RevertImpersonate();
            }
        }

    }
}
