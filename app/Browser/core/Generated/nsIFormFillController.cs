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
// Generated by IDLImporter from file nsIFormFillController.idl
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
    /// nsIFormFillController is an interface for controlling form fill behavior
    /// on HTML documents.  Any number of docShells can be controller concurrently.
    /// While a docShell is attached, all HTML documents that are loaded within it
    /// will have a focus listener attached that will listen for when a text input
    /// is focused.  When this happens, the input will be bound to the
    /// global nsIAutoCompleteController service.
    /// </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("07f0a0dc-f6e9-4cdd-a55f-56d770523a4c")]
	public interface nsIFormFillController
	{
		
		/// <summary>
        /// Start controlling form fill behavior for the given browser
        ///
        /// @param docShell - The docShell to attach to
        /// @param popup - The popup to show when autocomplete results are available
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void AttachToBrowser([MarshalAs(UnmanagedType.Interface)] nsIDocShell docShell, [MarshalAs(UnmanagedType.Interface)] nsIAutoCompletePopup popup);
		
		/// <summary>
        /// Stop controlling form fill behavior for the given browser
        ///
        /// @param docShell - The docShell to detach from
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void DetachFromBrowser([MarshalAs(UnmanagedType.Interface)] nsIDocShell docShell);
		
		/// <summary>
        /// Mark the specified <input> element as being managed by password manager.
        /// Autocomplete requests will be handed off to the password manager, and will
        /// not be stored in form history.
        ///
        /// @param aInput - The HTML <input> element to tag
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void MarkAsLoginManagerField([MarshalAs(UnmanagedType.Interface)] nsIDOMHTMLInputElement aInput);
	}
}
