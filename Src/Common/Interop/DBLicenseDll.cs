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
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;


namespace Alachisoft.NosDB.Common.Util
{
	/// <summary>
	/// Utility class to help with interop tasks.
	/// </summary>
	public class DBLicenseDll
	{ 

        internal const string DLL_LICENSE = "dblicense";

		/// <summary>
		/// Declare the structure, which is the parameter of ReadEvaluationData. 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct EvaluationData 
		{
			public short	Year;
			public short	Month;
			public short	Days;
			public short	ExtensionVal;
			public short	Extensions;
			public short	Period;
			public short	ActivationStatus;
			public short	Res3;
		};

        public enum ActivationStatus
        {
            EVAL = 80,
            ACTIVATED,
            DEACTIVATED
        }
        /// <summary>
        /// Returns the number of processors on the system.
        /// </summary>        
        [DllImport(DLL_LICENSE, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumProcessors();

        /// <summary>
        /// Returns the total number of cores available in the system.
        /// </summary>
        [DllImport(DLL_LICENSE, CallingConvention = CallingConvention.Cdecl)]

        public static extern int GetNumCores();

        /// <summary>
        /// Returns 0 or 1, If VM based OS found returns 1 else 0
        /// </summary> 
        [DllImport(DLL_LICENSE, CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsEmulatedOS();

        /// <summary>
        /// Returns a list of mac addresses found on the system.
        /// </summary>
        [DllImport(DLL_LICENSE, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetAdaptersAddressList(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder list);

        [DllImport(DLL_LICENSE, CallingConvention = CallingConvention.Cdecl)]
		public static extern void ReadActivationCode(
			[Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder code, short prodId);
 
        [DllImport(DLL_LICENSE, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReadInstallCode(
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder code, short prodId);
 
        [DllImport(DLL_LICENSE, CallingConvention = CallingConvention.Cdecl)]
		public static extern int ReadEvaluationData(
			int version, 
			ref EvaluationData time, short prodId);
 
      
    }
}
