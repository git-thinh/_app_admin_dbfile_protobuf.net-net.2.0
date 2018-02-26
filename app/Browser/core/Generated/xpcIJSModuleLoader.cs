// --------------------------------------------------------------------------------------------
// Version: MPL 1.1/GPL 2.0/LGPL 2.1
// 
// The contents of this file are subject to the Mozilla Public License Version
// 1.1 (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
// http://www.mozilla.org/MPL/
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
// for the specific language governing rights and limitations under the
// License.
// 
// <remarks>
// Generated by IDLImporter from file xpcIJSModuleLoader.idl
// 
// You should use these interfaces when you access the COM objects defined in the mentioned
// IDL/IDH file.
// </remarks>
// --------------------------------------------------------------------------------------------
namespace Gecko
{
	using System;
	using System.Runtime.InteropServices;
	using System.Runtime.InteropServices.ComTypes;
	using System.Runtime.CompilerServices;
	
	
	/// <summary>
    ///This Source Code Form is subject to the terms of the Mozilla Public
    /// License, v. 2.0. If a copy of the MPL was not distributed with this
    /// file, You can obtain one at http://mozilla.org/MPL/2.0/. </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("4f94b21f-2920-4bd9-8251-5fb60fb054b2")]
	public interface xpcIJSModuleLoader
	{
		
		/// <summary>
        /// To be called from JavaScript only.
        ///
        /// Synchronously loads and evaluates the js file located at
        /// aResourceURI with a new, fully privileged global object.
        ///
        /// If 'targetObj' is specified and equal to null, returns the
        /// module's global object. Otherwise (if 'targetObj' is not
        /// specified, or 'targetObj' is != null) looks for a property
        /// 'EXPORTED_SYMBOLS' on the new global object. 'EXPORTED_SYMBOLS'
        /// is expected to be an array of strings identifying properties on
        /// the global object.  These properties will be installed as
        /// properties on 'targetObj', or, if 'targetObj' is not specified,
        /// on the caller's global object. If 'EXPORTED_SYMBOLS' is not
        /// found, an error is thrown.
        ///
        /// @param resourceURI A resource:// URI string to load the module from.
        /// @param targetObj  the object to install the exported properties on.
        /// If this parameter is a primitive value, this method throws
        /// an exception.
        /// @returns the module code's global object.
        ///
        /// The implementation maintains a hash of registryLocation->global obj.
        /// Subsequent invocations of importModule with 'registryLocation'
        /// pointing to the same file will not cause the module to be re-evaluated,
        /// but the symbols in EXPORTED_SYMBOLS will be exported into the
        /// specified target object and the global object returned as above.
        ///
        /// (This comment is duplicated to nsIXPCComponents_Utils.)
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		Gecko.JsVal Import([MarshalAs(UnmanagedType.LPStruct)] nsAUTF8StringBase aResourceURI, ref Gecko.JsVal targetObj, System.IntPtr jsContext, int argc);
		
		/// <summary>
        /// Imports the JS module at aResourceURI to the JS object
        /// 'targetObj' (if != null) as described for importModule() and
        /// returns the module's global object.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		System.IntPtr ImportInto([MarshalAs(UnmanagedType.LPStruct)] nsAUTF8StringBase aResourceURI, System.IntPtr targetObj, System.IntPtr cc);
		
		/// <summary>
        /// Unloads the JS module at aResourceURI. Existing references to the module
        /// will continue to work but any subsequent import of the module will
        /// reload it and give new reference. If the JS module hasn't yet been imported
        /// then this method will do nothing.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void Unload([MarshalAs(UnmanagedType.LPStruct)] nsAUTF8StringBase aResourceURI);
		
		/// <summary>
        /// Returns true if the js file located at 'registryLocation' location has
        /// been loaded previously via the import method above. Returns false
        /// otherwise.
        ///
        /// @param resourceURI A resource:// URI string representing the location of
        /// the js file to be checked if it is already loaded or not.
        /// @returns boolean, true if the js file has been loaded via import. false
        /// otherwise
        /// </summary>
		[return: MarshalAs(UnmanagedType.U1)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		bool IsModuleLoaded([MarshalAs(UnmanagedType.LPStruct)] nsAUTF8StringBase aResourceURI);
	}
}
