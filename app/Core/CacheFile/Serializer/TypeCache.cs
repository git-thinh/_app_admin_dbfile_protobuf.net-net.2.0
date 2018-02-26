using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection; 
using System.Reflection.Emit; 
using System.Collections.Specialized;
using System.Data;
using System.Drawing; 
 
namespace app.Core.CacheFile
{
	/// <summary>
	/// Cached information about types, for internal use.
	/// </summary>
	class TypeCache
	{
		internal delegate void GenericSetter(object target, object value);
		internal delegate object GenericGetter(object target);
		internal delegate object GenericConstructor();

         
		private readonly Hashtable _constructorCache = new Hashtable();
		private readonly Hashtable _cache;
		public TypeCache()
		{
			_cache = new Hashtable();
			_constructorCache = new Hashtable();
		} 

		/// <summary>
		/// Removes all cached information about types.
		/// </summary>
		public void ClearCache()
		{
			lock (_cache)
			{
				_cache.Clear();
			}
		}
		/// <summary>
		/// Removes a cached entry.
		/// </summary>
		/// <param name="type">The object type.</param>
		public void RemoveEntry(Type type)
		{
			lock (_cache)
			{
				_cache.Remove(type);
			}
		}

		/// <summary>
		/// Reads type information and caches it.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		public void Initialize<T>()
		{
			var type = typeof(T);
			InitializeInternal(type);
		}

		/// <summary>
		/// Reads type information and caches it.
		/// </summary>
		/// <param name="types">The objects types.</param>
		public void Initialize(params Type[] types)
		{
			foreach (var t in types)
			{
				InitializeInternal(t);
			}
		}

		internal object CreateInstance(Type t)
		{ 
			// Read from cache 
			var info = _constructorCache[t] as GenericConstructor; 
			if (info == null)
			{
				ConstructorInfo ctor = t.GetConstructor(Type.EmptyTypes);
				if (ctor == null)
				{
					// Falling back to default parameterless constructor.
					return Activator.CreateInstance(t, null);
				}
                 
				var dynamicCtor = new DynamicMethod("_", t, Type.EmptyTypes, t, true); 

				var il = dynamicCtor.GetILGenerator();

				il.Emit(OpCodes.Newobj, ctor);
				il.Emit(OpCodes.Ret);

				info = (GenericConstructor)dynamicCtor.CreateDelegate(typeof(GenericConstructor));

				_constructorCache[t] = info;
			}
			if (info == null)
				throw new MissingMethodException(string.Format("No parameterless constructor defined for '{0}'.", t));
			return info.Invoke(); 
		}
		internal object CreateInstanceDirect(Type t)
		{
			// Falling back to default parameterless constructor.
			return Activator.CreateInstance(t, null);
		}

		internal MemberInfo GetTypeInfo(Type type, bool generate)
		{ 
			var memInfo = _cache[type] as MemberInfo;
			if (memInfo != null)
			{
				return memInfo;
			} 

			if (generate)
			{
				memInfo = ReadMemberInfo(type);
				CacheInsert(type, memInfo);
			}
			return memInfo;
		}

		private void InitializeInternal(Type type)
		{
			if (!_cache.ContainsKey(type))
			{
				var info = ReadMemberInfo(type);
				CacheInsert(type, info);
			}
		}
		private void CacheInsert(Type type, MemberInfo memInfo)
		{
			lock (_cache)
			{
				if (!_cache.ContainsKey(type))
				{
					_cache.Add(type, memInfo);
				}
			}
		}

		private MemberInfo ReadObject(Type type)
		{
			bool readFields = true, readProps = true;

			var objectAttr = type.GetCustomAttributes(typeof(ContractAttribute), false);
			if (objectAttr.Length > 0)
			{
				var _itemContract = objectAttr[0] as ContractAttribute;
				if (_itemContract != null)
				{
					readFields = _itemContract.Fields;
					readProps = _itemContract.Properties;
				}
			}
			var typeInfo = new TypeInfo
			{
				MemberType = EnMemberType.Object,
				KnownType = EnKnownType.Unknown,
				IsContainerObject = true,
				IsStruct = type.IsValueType
			};

			var members = new List<MemberInfo>();

			if (readProps)
			{
				var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

				foreach (var p in props)
				{
					if (p.CanWrite)
					{
						var index = -1;
						var memProp = p.GetCustomAttributes(typeof(MemberAttribute), false);
						MemberAttribute _itemMember;
						if (memProp.Length > 0 && (_itemMember = (memProp[0] as MemberAttribute)) != null)
						{
							if (!_itemMember.Included)
								continue;
							index = _itemMember.Index;
						}

						var info = ReadMemberInfo(p.PropertyType);
						info.PropertyGetter = GetPropertyGetter(type, p);
						//info.PropertySetter = CreateSetMethod(p);
						info.PropertySetter = GetPropertySetter(type, p);
						info.Info = p;
						info.MemberType = EnMemberType.Property;

						if (index > -1)
						{
							members.Insert(index, info);
						}
						else
						{
							members.Add(info);
						}
					}
				}
			}

			if (readFields)
			{
				var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
				foreach (var f in fields)
				{
					var index = -1;
					var memProp = f.GetCustomAttributes(typeof(MemberAttribute), false);
					MemberAttribute _itemMember;
					if (memProp.Length > 0 && (_itemMember = (memProp[0] as MemberAttribute)) != null)
					{
						if (!_itemMember.Included)
							continue;
						index = _itemMember.Index;
					}

					var info = ReadMemberInfo(f.FieldType);
					info.Info = f;
					info.MemberType = EnMemberType.Field;
					if (index > -1)
					{
						members.Insert(index, info);
					}
					else
					{
						members.Add(info);
					}
				}
			}

			typeInfo.Members = members.ToArray();

			CacheInsert(type, typeInfo);
			return typeInfo;
		}

		private MemberInfo ReadMemberInfo(Type memType)
		{
			if (memType == typeof(string))
			{
				return new MemberInfo
				{
					KnownType = EnKnownType.String,
					IsNullable = true,
					IsSupportedPrimitive = true,
				};
			}
			Type memActualType = memType;
			Type underlyingTypeNullable;
			bool isNullable = ReflectionHelper.IsNullable(memType, out underlyingTypeNullable);

			// check the underling type
			if (isNullable && underlyingTypeNullable != null)
			{
				memActualType = underlyingTypeNullable;
			}
			else
			{
				underlyingTypeNullable = null;
			}


			if (memActualType == typeof(char))
			{
				// is struct and uses Nullable<>
				return new MemberInfo
				{
					KnownType = EnKnownType.Char,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(bool))
			{
				// is struct and uses Nullable<>
				return new MemberInfo
				{
					KnownType = EnKnownType.Bool,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(DateTime))
			{
				// is struct and uses Nullable<>
				return new MemberInfo
				{
					KnownType = EnKnownType.DateTime,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(DateTimeOffset))
			{
				// is struct and uses Nullable<>
				return new MemberInfo
				{
					KnownType = EnKnownType.DateTimeOffset,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (memActualType == typeof(byte[]))
			{
				return new MemberInfo
				{
					KnownType = EnKnownType.ByteArray,
					IsNullable = isNullable,
					IsArray = true,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(Enum)))
			{
				return new MemberInfo
				{
					KnownType = EnKnownType.Enum,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(Array)))
			{
				return new MemberInfo
				{
					KnownType = EnKnownType.Unknown,
					IsNullable = isNullable,
					IsArray = true,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}

			var isGenericType = memActualType.IsGenericType;
			Type[] interfaces = null;
			if (isGenericType)
			{
				//// no more checking for a dictionary with its first argumnet as String
				//if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)) &&
				//	memActualType.GetGenericArguments()[0] == typeof(string))
				//	return new MemberInfo
				//	{
				//		KnownType = EnKnownType.Unknown,
				//		IsNullable = isNullable,
				//		IsDictionary = true,
				//		IsStringDictionary = true,
				//		IsGeneric = true,
				//		NullableUnderlyingType = underlyingTypeNullable,
				//	};

				interfaces = memActualType.GetInterfaces();

				if (ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(IDictionary<,>)) ||
					memActualType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
					return new MemberInfo
					{
						KnownType = EnKnownType.Unknown,
						IsNullable = isNullable,
						IsDictionary = true,
						IsGeneric = true,
						NullableUnderlyingType = underlyingTypeNullable,
					};
                 
			}


			if (ReflectionHelper.CompareInterface(memActualType, typeof(IDictionary)))
			{
				return new MemberInfo
				{
					KnownType = EnKnownType.Unknown,
					IsNullable = isNullable,
					IsDictionary = true,
					IsGeneric = true,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			// the IDictionary should be checked before IList<>
			if (isGenericType)
			{
				if (ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(IList<>)) ||
					ReflectionHelper.CompareInterfaceGenericTypeDefinition(interfaces, typeof(ICollection<>)))
					return new MemberInfo
					{
						KnownType = EnKnownType.Unknown,
						IsNullable = isNullable,
						IsGeneric = true,
						IsCollection = true,
						IsArray = true,
						NullableUnderlyingType = underlyingTypeNullable,
					};
			}
             
			if (ReflectionHelper.CompareSubType(memActualType, typeof(NameValueCollection)))
			{
				return new MemberInfo
				{
					KnownType = EnKnownType.NameValueColl,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			} 
			// checking for IList and ICollection should be after NameValueCollection
			if (ReflectionHelper.CompareInterface(memActualType, typeof(IList)) ||
				ReflectionHelper.CompareInterface(memActualType, typeof(ICollection)))
			{
				return new MemberInfo
				{
					KnownType = EnKnownType.Unknown,
					IsNullable = isNullable,
					IsGeneric = memActualType.IsGenericType,
					IsCollection = true,
					IsArray = true,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}

             
			if (memActualType == typeof(Color))
			{
				// is struct and uses Nullable<>
				return new MemberInfo
				{
					KnownType = EnKnownType.Color,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataSet)))
			{
				return new MemberInfo
				{
					KnownType = EnKnownType.DataSet,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}
			if (ReflectionHelper.CompareSubType(memActualType, typeof(DataTable)))
			{
				return new MemberInfo
				{
					KnownType = EnKnownType.DataTable,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			} 

			if (memActualType == typeof(TimeSpan))
			{
				// is struct and uses Nullable<>
				return new MemberInfo
				{
					KnownType = EnKnownType.TimeSpan,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}

			if (memActualType == typeof(Version))
			{
				return new MemberInfo
				{
					KnownType = EnKnownType.Version,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			}

			MemberInfo output;
			if (TryReadNumber(memActualType, out output))
			{
				output.IsNullable = isNullable;
				output.NullableUnderlyingType = underlyingTypeNullable;
				return output;
			}
			if (memActualType == typeof(Guid))
			{
				// is struct and uses Nullable<>
				return new MemberInfo
				{
					KnownType = EnKnownType.Guid,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			} 
			if (memActualType == typeof(DBNull))
			{
				// ignore!
				return new MemberInfo
				{
					KnownType = EnKnownType.DbNull,
					IsNullable = isNullable,
					NullableUnderlyingType = underlyingTypeNullable,
				};
			} 

			var objectMemInfo = ReadObject(memType);
			objectMemInfo.NullableUnderlyingType = underlyingTypeNullable;
			objectMemInfo.IsNullable = isNullable;

			return objectMemInfo;
		}

		static Type UnNullify(Type type)
		{
			return Nullable.GetUnderlyingType(type) ?? type;
		}
		/// <summary>
		/// Slower convertion
		/// </summary>
		private bool IsNumber(Type memType, out MemberInfo output)
		{
			if (memType.IsClass)
			{
				output = null;
				return false;
			}
			output = null;
			switch (Type.GetTypeCode(UnNullify(memType)))
			{
				case TypeCode.Int16:
					output = new MemberInfo
					{
						KnownType = EnKnownType.Int16,
						IsSupportedPrimitive = true,
					};
					break;

				case TypeCode.Int32:
					output = new MemberInfo
					{
						KnownType = EnKnownType.Int32,
						IsSupportedPrimitive = true,
					};
					break;

				case TypeCode.Int64:
					output = new MemberInfo
					{
						KnownType = EnKnownType.Int64,
						IsSupportedPrimitive = true,
					};
					break;
				case TypeCode.Single:
					output = new MemberInfo { KnownType = EnKnownType.Single };
					break;
				case TypeCode.Double:
					output = new MemberInfo { KnownType = EnKnownType.Double };
					break;
				case TypeCode.Decimal:
					output = new MemberInfo { KnownType = EnKnownType.Decimal };
					break;

				case TypeCode.Byte:
					output = new MemberInfo { KnownType = EnKnownType.Byte };
					break;
				case TypeCode.SByte:
					output = new MemberInfo { KnownType = EnKnownType.SByte };
					break;

				case TypeCode.UInt16:
					output = new MemberInfo { KnownType = EnKnownType.UInt16 };
					break;
				case TypeCode.UInt32:
					output = new MemberInfo { KnownType = EnKnownType.UInt32 };
					break;
				case TypeCode.UInt64:
					output = new MemberInfo { KnownType = EnKnownType.UInt64 };
					break;
			}
			return output != null;
		}

		private bool TryReadNumber(Type memType, out MemberInfo output)
		{
			if (memType.IsClass)
			{
				output = null;
				return false;
			}
			if (memType == typeof(int))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Int32,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(long))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Int64,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(short))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Int16,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(double))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Double,
				};
			}
			else if (memType == typeof(decimal))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Decimal,
				};
			}
			else if (memType == typeof(float))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Single,
				};
			}
			else if (memType == typeof(byte))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Byte,
				};
			}
			else if (memType == typeof(sbyte))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.SByte,
				};
			}
			else if (memType == typeof(ushort))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.UInt16,
				};
			}
			else if (memType == typeof(uint))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.UInt32,
				};
			}
			else if (memType == typeof(ulong))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.UInt64,
				};
			}
			else
			{
				output = null;
				return false;
			}
			return true;
		}
		private bool TryReadNumber__Nullable(Type memType, out MemberInfo output)
		{
			if (memType.IsClass)
			{
				output = null;
				return false;
			}
			if (memType == typeof(int) || memType == typeof(int?))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Int32,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(long) || memType == typeof(long?))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Int64,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(short) || memType == typeof(short?))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Int16,
					IsSupportedPrimitive = true,
				};
			}
			else if (memType == typeof(double) || memType == typeof(double?))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Double,
				};
			}
			else if (memType == typeof(decimal) || memType == typeof(decimal?))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Decimal,
				};
			}
			else if (memType == typeof(float) || memType == typeof(float?))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Single,
				};
			}
			else if (memType == typeof(byte) || memType == typeof(byte?))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.Byte,
				};
			}
			else if (memType == typeof(sbyte) || memType == typeof(sbyte?))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.SByte,
				};
			}
			else if (memType == typeof(ushort) || memType == typeof(ushort?))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.UInt16,
				};
			}
			else if (memType == typeof(uint) || memType == typeof(uint?))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.UInt32,
				};
			}
			else if (memType == typeof(ulong) || memType == typeof(ulong?))
			{
				output = new MemberInfo
				{
					KnownType = EnKnownType.UInt64,
				};
			}
			else
			{
				output = null;
				return false;
			}
			return true;
		}

		/// <summary>
		///  Creates a dynamic setter for the property
		/// </summary>
		/// <author>
		/// Gerhard Stephan 
		/// http://jachman.wordpress.com/2006/08/22/2000-faster-using-dynamic-method-calls/
		/// </author> 
		private GenericSetter CreateSetMethod(PropertyInfo propertyInfo)
		{
			/*
			* If there's no setter return null
			*/
			MethodInfo setMethod = propertyInfo.GetSetMethod();
			if (setMethod == null)
				return null;

			/*
			* Create the dynamic method
			*/
			var arguments = new Type[2];
			arguments[0] = arguments[1] = typeof(object);

             
			var setter = new DynamicMethod(
			  String.Concat("_Set", propertyInfo.Name, "_"),
			  typeof(void), arguments, propertyInfo.DeclaringType);
 


			ILGenerator generator = setter.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
			generator.Emit(OpCodes.Ldarg_1);

			if (propertyInfo.PropertyType.IsClass)
				generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
			else
				generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);

			generator.EmitCall(OpCodes.Callvirt, setMethod, null);
			generator.Emit(OpCodes.Ret);

			/*
			* Create the delegate and return it
			*/
			return (GenericSetter)setter.CreateDelegate(typeof(GenericSetter));
		} 

		/// <summary>
		/// Creates a dynamic getter for the property
		/// </summary>
		/// <author>
		/// Gerhard Stephan 
		/// http://jachman.wordpress.com/2006/08/22/2000-faster-using-dynamic-method-calls/
		/// </author> 
		private GenericGetter CreateGetMethod(PropertyInfo propertyInfo)
		{
			/*
			* If there's no getter return null
			*/
			MethodInfo getMethod = propertyInfo.GetGetMethod();
			if (getMethod == null)
				return null;

			/*
			* Create the dynamic method
			*/
			var arguments = new Type[1];
			arguments[0] = typeof(object);
             
			var getter = new DynamicMethod(
			  String.Concat("_Get", propertyInfo.Name, "_"),
			  typeof(object), arguments, propertyInfo.DeclaringType); 

			ILGenerator generator = getter.GetILGenerator();
			generator.DeclareLocal(typeof(object));
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
			generator.EmitCall(OpCodes.Callvirt, getMethod, null);

			if (!propertyInfo.PropertyType.IsClass)
				generator.Emit(OpCodes.Box, propertyInfo.PropertyType);

			generator.Emit(OpCodes.Ret);

			/*
			* Create the delegate and return it
			*/
			return (GenericGetter)getter.CreateDelegate(typeof(GenericGetter));
		} 

		//private Func<T, object> MakeDelegate_2<T, U>(MethodInfo @get)
		//{
		//	var f = (Func<T, U>)Delegate.CreateDelegate(typeof(Func<T, U>), @get);
		//	return t => f(t);
		//}

		private GenericGetter MakeDelegate(MethodInfo @get)
		{
			var f = (GenericGetter)Delegate.CreateDelegate(typeof(GenericGetter), @get);
			return t => f(t);
		}

		private GenericGetter GetPropertyGetter(Type objType, PropertyInfo propertyInfo)
		{ 
			if (objType.IsInterface)
			{
				throw new Exception("Type is an interface or abstract class and cannot be instantiated.");
			}
			if (objType.IsValueType &&
				!objType.IsPrimitive &&
				!objType.IsArray &&
				objType != typeof(string))
			{
				// this is a fallback to slower method.
				var method = propertyInfo.GetGetMethod(true);

				// generating the caller.
				return new GenericGetter(target => method.Invoke(target, null));
			}
			else
			{
				//var method = objType.GetMethod("get_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
				var method = propertyInfo.GetGetMethod(true);
				return GetFastGetterFunc(propertyInfo, method);
			} 
		}

		private Function<object, object, object> GetPropertySetter(Type objType, PropertyInfo propertyInfo)
		{ 
			if (objType.IsValueType &&
				!objType.IsPrimitive &&
				!objType.IsArray &&
				objType != typeof(string))
			{
				// this is a fallback to slower method.
				var method = propertyInfo.GetSetMethod(true);

				// generating the caller.
				return new Function<object, object, object>((target, value) => method.Invoke(target, new object[] { value }));
			}
			else
			{
				//var method = objType.GetMethod("set_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
				var method = propertyInfo.GetSetMethod(true);
				return GetFastSetterFunc(propertyInfo, method);
			} 

			////var method = objType.GetMethod("set_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
			//var method = propertyInfo.GetSetMethod();
			//return GetFastSetterFunc(propertyInfo, method);
		}

		/// <summary>
		/// http://social.msdn.microsoft.com/Forums/en-US/netfxbcl/thread/8754500e-4426-400f-9210-554f9f2ad58b/
		/// </summary>
		/// <returns></returns> 
		private GenericGetter GetFastGetterFunc(PropertyInfo p, MethodInfo getter) // untyped cast from Func<T> to Func<object> 
		{ 
			var g = new DynamicMethod("_", typeof(object), new[] { typeof(object) }, p.DeclaringType, true);
 
			var il = g.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);//load the delegate from function parameter
			il.Emit(OpCodes.Castclass, p.DeclaringType);//cast
			il.Emit(OpCodes.Callvirt, getter);//calls it's get method

			if (p.PropertyType.IsValueType)
				il.Emit(OpCodes.Box, p.PropertyType);//box

			il.Emit(OpCodes.Ret);

			//return (bool)((xViewModel)param1).get_IsEnabled();

			var _func = (GenericGetter)g.CreateDelegate(typeof(GenericGetter));
			return _func;
		} 

		/// <summary>
		/// http://social.msdn.microsoft.com/Forums/en-US/netfxbcl/thread/8754500e-4426-400f-9210-554f9f2ad58b/
		/// </summary> 
		private Function<object, object, object> GetFastSetterFunc(PropertyInfo p, MethodInfo setter)
		{ 
			var s = new DynamicMethod("_", typeof(object), new[] { typeof(object), typeof(object) }, p.DeclaringType, true);
 
			var il = s.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Castclass, p.DeclaringType);

			il.Emit(OpCodes.Ldarg_1);
			if (p.PropertyType.IsClass)
			{
				il.Emit(OpCodes.Castclass, p.PropertyType);
			}
			else
			{
				il.Emit(OpCodes.Unbox_Any, p.PropertyType);
			}


			il.EmitCall(OpCodes.Callvirt, setter, null);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ret);

			//(xViewModel)param1.set_IsEnabled((bool)param2)
			// return param1;

			var _func = (Function<object, object, object>)s.CreateDelegate(typeof(Function<object, object, object>));
			return _func;
		} 

		private GenericGetter GetPropertyGetter_(Type objType, PropertyInfo propertyInfo)
		{
			var method = objType.GetMethod("get_" + propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var dlg = Delegate.CreateDelegate(typeof(GenericGetter), method);
			return (GenericGetter)dlg;
		}

		private GenericGetter GetPropertyGetter(object obj, string propertyName)
		{
			var t = obj.GetType();
			var method = t.GetMethod("get_" + propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var dlg = Delegate.CreateDelegate(t, obj, method);
			return (GenericGetter)dlg;
		}

		private GenericSetter GetPropertySetter(object obj, string propertyName)
		{
			var t = obj.GetType();
			var method = t.GetMethod("set_" + propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var dlg = Delegate.CreateDelegate(t, obj, method);
			return (GenericSetter)dlg;
		}


		//ParameterExpression arg = Expression.Parameter(typeof(Person));
		//Expression expr = Expression.Property(arg, propertyName);
		//Expression<Func<Person, string>> get = Expression.Lambda<Func<Person, string>>(expr, arg);
		//Getter = get.Compile();


		//var member = get.Body;
		//		var param = Expression.Parameter(typeof(string), "value");
		//		Setter =
		//Expression.Lambda<Action<Person, string>>(Expression.Assign(member, param), get.Parameters[0], param)
		//.Compile();

	}
}
