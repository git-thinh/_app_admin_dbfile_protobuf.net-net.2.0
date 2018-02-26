using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
 
namespace app.Core.CacheFile
{
	class MemberInfo
	{
		public bool IsNullable;
		public bool IsGeneric;
		public bool IsStringDictionary = false;
		public bool IsDictionary;
		public bool IsCollection;
		public bool IsArray;
		public bool IsSupportedPrimitive;

		/// <summary>
		/// Has Fields or Properties
		/// </summary>
		public bool IsContainerObject;

		/// <summary>
		/// IsValueType
		/// </summary>
		public bool IsStruct;

		internal EnMemberType MemberType;
		internal EnKnownType KnownType;
		public System.Reflection.MemberInfo Info;
		public Type NullableUnderlyingType;

		public Function<object, object, object> PropertySetter;
		public TypeCache.GenericGetter PropertyGetter;

#if DEBUG
		public override string ToString()
		{
			return string.Format("{0}: {1}: {2}", MemberType, KnownType, Info);
		}
#endif
	}
}
