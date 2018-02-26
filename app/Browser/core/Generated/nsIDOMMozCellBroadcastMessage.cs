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
// Generated by IDLImporter from file nsIDOMMozCellBroadcastMessage.idl
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
    /// MozCellBroadcastMessage encapsulates Cell Broadcast short message service
    /// (CBS) messages.
    /// </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("dc729df4-f1d8-11e3-b00d-d3332542c557")]
	public interface nsIDOMMozCellBroadcastMessage
	{
		
		/// <summary>
        /// The Service Id in the device where the message is received from.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		uint GetServiceIdAttribute();
		
		/// <summary>
        /// Indication of the geographical area over which the Message Code is unique,
        /// and the display mode.
        ///
        /// Possible values are: "cell-immediate", "plmn", "location-area" and "cell".
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetGsmGeographicalScopeAttribute([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase aGsmGeographicalScope);
		
		/// <summary>
        /// The Message Code differentiates between messages from the same source and
        /// type (e.g., with the same Message Identifier).
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		ushort GetMessageCodeAttribute();
		
		/// <summary>
        /// Source and type of the message. For example, "Automotive Association"
        /// (= source), "Traffic Reports" (= type) could correspond to one value. The
        /// Message Identifier is coded in binary.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		ushort GetMessageIdAttribute();
		
		/// <summary>
        /// ISO-639-1 language code for this message. Null if unspecified.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetLanguageAttribute([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase aLanguage);
		
		/// <summary>
        /// Text message carried by the message.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetBodyAttribute([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase aBody);
		
		/// <summary>
        /// Possible values are "normal", "class-0", "class-1", "class-2", "class-3",
        /// "user-1", and "user-2".
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetMessageClassAttribute([MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "Gecko.CustomMarshalers.AStringMarshaler")] nsAStringBase aMessageClass);
		
		/// <summary>
        /// System time stamp at receival.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		long GetTimestampAttribute();
		
		/// <summary>
        /// Additional ETWS-specific info.
        /// </summary>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		nsIDOMMozCellBroadcastEtwsInfo GetEtwsAttribute();
		
		/// <summary>
        /// Service Category.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		int GetCdmaServiceCategoryAttribute();
	}
	
	/// <summary>
    /// ETWS (Earthquake and Tsunami Warning service) Primary Notification message
    /// specific information.
    /// </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("af009d9a-f5e8-4573-a6ee-a85118465bed")]
	public interface nsIDOMMozCellBroadcastEtwsInfo
	{
		
		/// <summary>
        /// Warning type. Possible values are "earthquake", "tsunami",
        /// "earthquake-tsunami", "test" and "other".
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void GetWarningTypeAttribute([MarshalAs(UnmanagedType.LPStruct)] nsACStringBase aWarningType);
		
		/// <summary>
        /// Emergency user alert indication. It is used to command mobile terminals to
        /// activate emergency user alert upon the reception of ETWS primary
        /// notification.
        /// </summary>
		[return: MarshalAs(UnmanagedType.U1)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		bool GetEmergencyUserAlertAttribute();
		
		/// <summary>
        /// Message popup indication. It is used to command mobile terminals to
        /// activate message popup upon the reception of ETWS primary notification.
        /// </summary>
		[return: MarshalAs(UnmanagedType.U1)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		bool GetPopupAttribute();
	}
}