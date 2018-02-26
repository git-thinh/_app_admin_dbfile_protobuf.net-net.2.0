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
// Generated by IDLImporter from file nsIMutableArray.idl
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
    /// nsIMutableArray
    /// A separate set of methods that will act on the array. Consumers of
    /// nsIArray should not QueryInterface to nsIMutableArray unless they
    /// own the array.
    ///
    /// As above, it is legal to add null elements to the array. Note also
    /// that null elements can be created as a side effect of
    /// insertElementAt(). Conversely, if insertElementAt() is never used,
    /// and null elements are never explicitly added to the array, then it
    /// is guaranteed that queryElementAt() will never return a null value.
    ///
    /// Any of these methods may throw NS_ERROR_OUT_OF_MEMORY when the
    /// array must grow to complete the call, but the allocation fails.
    /// </summary>
	[ComImport()]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("af059da0-c85b-40ec-af07-ae4bfdc192cc")]
	public interface nsIMutableArray : nsIArray
	{
		
		/// <summary>
        /// length
        ///
        /// number of elements in the array.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		new uint GetLengthAttribute();
		
		/// <summary>
        /// queryElementAt()
        ///
        /// Retrieve a specific element of the array, and QueryInterface it
        /// to the specified interface. null is a valid result for
        /// this method, but exceptions are thrown in other circumstances
        ///
        /// @param index position of element
        /// @param uuid the IID of the requested interface
        /// @param result the object, QI'd to the requested interface
        ///
        /// @throws NS_ERROR_NO_INTERFACE when an entry exists at the
        /// specified index, but the requested interface is not
        /// available.
        /// @throws NS_ERROR_ILLEGAL_VALUE when index > length-1
        ///
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		new System.IntPtr QueryElementAt(uint index, ref System.Guid uuid);
		
		/// <summary>
        /// indexOf()
        ///
        /// Get the position of a specific element. Note that since null is
        /// a valid input, exceptions are used to indicate that an element
        /// is not found.
        ///
        /// @param startIndex The initial element to search in the array
        /// To start at the beginning, use 0 as the
        /// startIndex
        /// @param element    The element you are looking for
        /// @returns a number >= startIndex which is the position of the
        /// element in the array.
        /// @throws NS_ERROR_FAILURE if the element was not in the array.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		new uint IndexOf(uint startIndex, [MarshalAs(UnmanagedType.Interface)] nsISupports element);
		
		/// <summary>
        /// enumerate the array
        ///
        /// @returns a new enumerator positioned at the start of the array
        /// @throws NS_ERROR_FAILURE if the array is empty (to make it easy
        /// to detect errors), or NS_ERROR_OUT_OF_MEMORY if out of memory.
        /// </summary>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		new nsISimpleEnumerator Enumerate();
		
		/// <summary>
        /// appendElement()
        ///
        /// Append an element at the end of the array.
        ///
        /// @param element The element to append.
        /// @param weak    Whether or not to store the element using a weak
        /// reference.
        /// @throws NS_ERROR_FAILURE when a weak reference is requested,
        /// but the element does not support
        /// nsIWeakReference.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void AppendElement([MarshalAs(UnmanagedType.Interface)] nsISupports element, [MarshalAs(UnmanagedType.U1)] bool weak);
		
		/// <summary>
        /// removeElementAt()
        ///
        /// Remove an element at a specific position, moving all elements
        /// stored at a higher position down one.
        /// To remove a specific element, use indexOf() to find the index
        /// first, then call removeElementAt().
        ///
        /// @param index the position of the item
        ///
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void RemoveElementAt(uint index);
		
		/// <summary>
        /// insertElementAt()
        ///
        /// Insert an element at the given position, moving the element
        /// currently located in that position, and all elements in higher
        /// position, up by one.
        ///
        /// @param element The element to insert
        /// @param index   The position in the array:
        /// If the position is lower than the current length
        /// of the array, the elements at that position and
        /// onwards are bumped one position up.
        /// If the position is equal to the current length
        /// of the array, the new element is appended.
        /// An index lower than 0 or higher than the current
        /// length of the array is invalid and will be ignored.
        ///
        /// @throws NS_ERROR_FAILURE when a weak reference is requested,
        /// but the element does not support
        /// nsIWeakReference.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void InsertElementAt([MarshalAs(UnmanagedType.Interface)] nsISupports element, uint index, [MarshalAs(UnmanagedType.U1)] bool weak);
		
		/// <summary>
        /// replaceElementAt()
        ///
        /// Replace the element at the given position.
        ///
        /// @param element The new element to insert
        /// @param index   The position in the array
        /// If the position is lower than the current length
        /// of the array, an existing element will be replaced.
        /// If the position is equal to the current length
        /// of the array, the new element is appended.
        /// If the position is higher than the current length
        /// of the array, empty elements are appended followed
        /// by the new element at the specified position.
        /// An index lower than 0 is invalid and will be ignored.
        ///
        /// @param weak    Whether or not to store the new element using a weak
        /// reference.
        ///
        /// @throws NS_ERROR_FAILURE when a weak reference is requested,
        /// but the element does not support
        /// nsIWeakReference.
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void ReplaceElementAt([MarshalAs(UnmanagedType.Interface)] nsISupports element, uint index, [MarshalAs(UnmanagedType.U1)] bool weak);
		
		/// <summary>
        /// clear()
        ///
        /// clear the entire array, releasing all stored objects
        /// </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		void Clear();
	}
}
