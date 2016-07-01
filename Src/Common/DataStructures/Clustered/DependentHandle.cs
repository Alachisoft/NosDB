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
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace System.Runtime.CompilerServices
{
    [ComVisible(false)]
    struct DependentHandle
    {
        #region Constructors
#if FEATURE_CORECLR
        [System.Security.SecuritySafeCritical] // auto-generated
#else
        [System.Security.SecurityCritical]
#endif
        public DependentHandle(Object primary, Object secondary)
        {
            IntPtr handle = (IntPtr)0;
            nInitialize(primary, secondary, out handle);
            // no need to check for null result: nInitialize expected to throw OOM.
            _handle = handle;
        }
        #endregion

        #region Public Members
        public bool IsAllocated
        {
            get
            {
                return _handle != (IntPtr)0;
            }
        }

        // Getting the secondary object is more expensive than getting the first so
        // we provide a separate primary-only accessor for those times we only want the
        // primary.
#if FEATURE_CORECLR
        [System.Security.SecuritySafeCritical] // auto-generated
#else
        [System.Security.SecurityCritical]
#endif
        public Object GetPrimary()
        {
            Object primary;
            nGetPrimary(_handle, out primary);
            return primary;
        }

#if FEATURE_CORECLR
        [System.Security.SecuritySafeCritical] // auto-generated
#else
        [System.Security.SecurityCritical]
#endif
        public void GetPrimaryAndSecondary(out Object primary, out Object secondary)
        {
            nGetPrimaryAndSecondary(_handle, out primary, out secondary);
        }

        //----------------------------------------------------------------------
        // Forces dependentHandle back to non-allocated state (if not already there)
        // and frees the handle if needed.
        //----------------------------------------------------------------------
        [System.Security.SecurityCritical]
        public void Free()
        {
            if (_handle != (IntPtr)0)
            {
                IntPtr handle = _handle;
                _handle = (IntPtr)0;
                nFree(handle);
            }
        }
        #endregion

        #region Private Members
        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.AppDomain)]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void nInitialize(Object primary, Object secondary, out IntPtr dependentHandle);

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void nGetPrimary(IntPtr dependentHandle, out Object primary);

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void nGetPrimaryAndSecondary(IntPtr dependentHandle, out Object primary, out Object secondary);

        [System.Security.SecurityCritical]
        [ResourceExposure(ResourceScope.None)]
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void nFree(IntPtr dependentHandle);
        #endregion

        #region Private Data Member
        private IntPtr _handle;
        #endregion

    }
}