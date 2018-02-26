﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text; 
using System.Collections.Specialized;
using System.Data;
using System.Drawing;  

namespace app.Core.CacheFile
{
	/// <summary>
	/// serializer.
	/// Which provides binary serialization and deserialzation for .NET objects.
	/// Stands for 'Binary Object Indexed Serialization'.
	/// </summary>
	/// <Author>
	/// Salar Khalilzadeh
	/// </Author>
	public class Serializer
	{
		private int _serializeDepth;
		private readonly TypeCache _typeCache = new TypeCache();

		/// <summary>
		/// Character encoding for strings.
		/// </summary>
		public Encoding Encoding { get; set; }

		/// <summary>
		/// Initializing a new instance of serializar.
		/// </summary>
		public Serializer()
		{
			Encoding = Encoding.UTF8;
		}

		/// <summary>
		/// Serializing an object to binary format.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="output">The output of the serialization in binary.</param>
		/// <typeparam name="T">The object type.</typeparam>
		public void Serialize<T>(T obj, Stream output)
		{
			if (obj == null)
				throw new ArgumentNullException("obj", "Object cannot be null.");
			_serializeDepth = 0;
			var writer = new BinaryWriter(output, Encoding);

			WriteValue(writer, obj, typeof(T));
		}

		/// <summary>
		/// Serializing an object to binary format.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="type">The objects' type.</param>
		/// <param name="output">The output of the serialization in binary.</param>
		public void Serialize(object obj, Type type, Stream output)
		{
			if (obj == null)
				throw new ArgumentNullException("obj", "Object cannot be null.");
			_serializeDepth = 0;
			var writer = new BinaryWriter(output, Encoding);

			WriteValue(writer, obj, type);
		}

		/// <summary>
		/// Deserilizing binary data to a new instance.
		/// </summary>
		/// <param name="objectData">The binary data.</param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T>(Stream objectData)
		{
			var reader = new BinaryReader(objectData, Encoding);
			return (T)ReadMember(reader, typeof(T));
		}

		public object Deserialize(Type type, Stream objectData)
		{
			var reader = new BinaryReader(objectData, Encoding);
			return ReadMember(reader, type);
		}

		/// <summary>
		/// Deserilizing binary data to a new instance.
		/// </summary>
		/// <param name="objectBuffer">The binary data.</param>
		/// <param name="index">The index in buffer at which the stream begins.</param>
		/// <param name="count">The length of the stream in bytes.</param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T>(byte[] objectBuffer, int index, int count)
		{
			using (var mem = new MemoryStream(objectBuffer, index, count, false))
			{
				var reader = new BinaryReader(mem, Encoding);
				return (T)ReadMember(reader, typeof(T));
			}
		}


		/// <summary>
		/// Deserilizing binary data to a new instance.
		/// </summary>
		/// <param name="objectBuffer">The binary data.</param>
		/// <param name="type">The objects' type.</param>
		/// <param name="index">The index in buffer at which the stream begins.</param>
		/// <param name="count">The length of the stream in bytes.</param>
		/// <returns>New instance of the deserialized data.</returns>
		public object Deserialize(byte[] objectBuffer, Type type, int index, int count)
		{
			using (var mem = new MemoryStream(objectBuffer, index, count, false))
			{
				var reader = new BinaryReader(mem, Encoding);
				return ReadMember(reader, type);
			}
		}


		/// <summary>
		/// Removes all cached information about types.
		/// </summary>
		public void ClearCache()
		{
			_typeCache.ClearCache();
		}

		/// <summary>
		/// Reads type information and caches it.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		public void Initialize<T>()
		{
			_typeCache.Initialize<T>();
		}

		/// <summary>
		/// Reads type information and caches it.
		/// </summary>
		/// <param name="types">The objects types.</param>
		public void Initialize(params Type[] types)
		{
			_typeCache.Initialize(types);
		}


		private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		private static long ConvertDateTimeToEpochTime(DateTime value)
		{
			DateTime d = new DateTime(value.Ticks - value.Ticks % 10000000L);
			return (long)(d - UnixEpoch).TotalSeconds;
		}

		private static DateTime ConvertToDateTimeFromEpoch(long seconds)
		{
			return UnixEpoch.AddSeconds((double)seconds);
		}

		private MemberInfo GetMemberInfo(Type objType)
		{
			return _typeCache.GetTypeInfo(objType, true);
		}


		#region Serialization methods

		private void WriteObject(BinaryWriter writer, MemberInfo itemMemInfo, object obj)
		{
			if (obj == null)
			{
				// null indicator
				WriteNullableType(writer, true);
				return;
			}

			var type = obj.GetType();

			var itemType = _typeCache.GetTypeInfo(type, true);
			var _itemTypeInfo = itemType as TypeInfo;

			_serializeDepth++;
			// Use this member info if avaiable. it is more accurate because it came from the object holder,
			// not the object itseld.
			if (itemMemInfo.IsContainerObject && itemMemInfo.IsNullable)
			{
				//This is a nullable struct and is not null
				WriteNullableType(writer, false);
			}

			if (_itemTypeInfo != null)
			{
				// writing the members
				for (int i = 0; i < _itemTypeInfo.Members.Length; i++)
				{
					var mem = _itemTypeInfo.Members[i];
					if (mem.MemberType == EnMemberType.Property)
					{
						var value = mem.PropertyGetter(obj);
						WriteValue(writer, mem, value);
					}
					else if (mem.MemberType == EnMemberType.Field)
					{
                        var finfo = (System.Reflection.FieldInfo)mem.Info;
						var value = finfo.GetValue(obj);
						WriteValue(writer, mem, value);
					}
				}
			}

			_serializeDepth--;
		}

		private void WriteNullableType(BinaryWriter writer, bool isnull)
		{
			writer.Write(isnull ? (byte)1 : (byte)0);
		}

		/// <summary>
		/// Also called by root
		/// </summary>
		void WriteValue(BinaryWriter writer, object value, Type type)
		{
			var bionType = _typeCache.GetTypeInfo(type, true);
			if (!bionType.IsSupportedPrimitive)
			{
				if (value == null)
				{
					WriteNullableType(writer, true);
					return;
				}
			}
			WriteValue(writer, bionType, value);
		}

		void WriteValue(BinaryWriter writer, object value, MemberInfo bionType)
		{
			if (!bionType.IsSupportedPrimitive)
			{
				if (value == null)
				{
					WriteNullableType(writer, true);
					return;
				}
			}
			WriteValue(writer, bionType, value);
		}

		void WriteValue(BinaryWriter writer, object value)
		{
			if (value == null)
			{
				WriteNullableType(writer, true);
				return;
			}

			var objType = value.GetType();
			var bionType = _typeCache.GetTypeInfo(objType, true);

			WriteValue(writer, bionType, value);
		}

		void WriteValue(BinaryWriter writer, MemberInfo itemMemInfo, object value)
		{
			if (!itemMemInfo.IsSupportedPrimitive && !itemMemInfo.IsContainerObject)
			{
				if (value == null)
				{
					WriteNullableType(writer, true);
					return;
				}
				else if (itemMemInfo.IsNullable)
				{
					WriteNullableType(writer, false);
				}
			}

			switch (itemMemInfo.KnownType)
			{
				case EnKnownType.Unknown:

					if (itemMemInfo.IsContainerObject)
					{
						WriteObject(writer, itemMemInfo, value);
					}
					else if (itemMemInfo.IsStringDictionary)
					{
						WriteStringDictionary(writer, value as IDictionary);
					}
					else if (itemMemInfo.IsDictionary)
					{
						WriteDictionary(writer, value as IDictionary);
					}
					else if (itemMemInfo.IsCollection || itemMemInfo.IsArray)
					{
						if (itemMemInfo.IsGeneric)
						{
							WriteGenericList(writer, value as IEnumerable);
						}
						else
						{
							WriteArray(writer, value);
						}
					}
					break;

				case EnKnownType.Int16:
					if (value == null || itemMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(writer, (short?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(writer, (short)value);
					}
					break;

				case EnKnownType.UInt16:
					if (value == null || itemMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(writer, (ushort?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(writer, (ushort)value);
					}
					break;

				case EnKnownType.Int32:
					if (value == null || itemMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(writer, (int?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(writer, (int)value);
					}

					break;

				case EnKnownType.Int64:
					if (value == null || itemMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(writer, (long?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(writer, (long)value);
					}
					break;

				case EnKnownType.UInt64:
					if (value == null || itemMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(writer, (ulong?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(writer, (ulong)value);
					}
					break;

				case EnKnownType.UInt32:
					if (value == null || itemMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(writer, (uint?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(writer, (uint)value);
					}
					break;

				case EnKnownType.Double:
					if (value == null || itemMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarDecimal(writer, (double?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarDecimal(writer, (double)value);
					}
					break;

				case EnKnownType.Decimal:
					if (value == null || itemMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarDecimal(writer, (decimal?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarDecimal(writer, (decimal)value);
					}
					break;

				case EnKnownType.Single:
					if (value == null || itemMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarDecimal(writer, (float?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarDecimal(writer, (float)value);
					}
					break;

				case EnKnownType.Byte:
					writer.Write((byte)value);
					break;

				case EnKnownType.SByte:
					writer.Write((sbyte)value);
					break;

				case EnKnownType.ByteArray:
					WriteBytes(writer, (byte[])value);
					break;

				case EnKnownType.String:
					WriteString(writer, value as string);
					break;

				case EnKnownType.Char:
					writer.Write((ushort)((char)value));
					break;

				case EnKnownType.Bool:
					writer.Write((byte)(((bool)value) ? 1 : 0));
					break;

				case EnKnownType.Enum:
					WriteEnum(writer, (Enum)value);
					break;

				case EnKnownType.DateTime:
					WriteDateTime(writer, (DateTime)value);
					break;
				case EnKnownType.DateTimeOffset:
					WriteDateTimeOffset(writer, (DateTimeOffset)value);
					break;

				case EnKnownType.TimeSpan:
					WriteTimeSpan(writer, (TimeSpan)value);
					break;
                     
				case EnKnownType.DataSet:
					WriteDataset(writer, value as DataSet);
					break;

				case EnKnownType.DataTable:
					WriteDataTable(writer, value as DataTable);
					break;
				case EnKnownType.NameValueColl:
					WriteCollectionNameValue(writer, value as NameValueCollection);
					break;

				case EnKnownType.Color:
					WriteColor(writer, (Color)value);
					break; 

				case EnKnownType.Version:
					WriteVersion(writer, value as Version);
					break;

				case EnKnownType.DbNull:
					// Do not write anything, it is already written as Nullable object. //WriteNullableType(true);
					break;

				case EnKnownType.Guid:
					WriteGuid(writer, (Guid)value);
					break;

				case EnKnownType.Uri:
					WriteUri(writer, value as Uri);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

         
		private void WriteCollectionNameValue(BinaryWriter writer, NameValueCollection nameValue)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, nameValue.Count);

			foreach (string key in nameValue)
			{
				WriteValue(writer, key);
				WriteValue(writer, nameValue[key]);
			}
		}

		private void WriteColor(BinaryWriter writer, Color c)
		{
			int argb = c.ToArgb();
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, argb);
		}

		private string GetXmlSchema(DataTable dt)
		{
			using (var writer = new StringWriter())
			{
				dt.WriteXmlSchema(writer);
				return writer.ToString();
			}
		}

		private void WriteDataset(BinaryWriter writer, DataSet ds)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, ds.Tables.Count);

			foreach (DataTable table in ds.Tables)
			{
				WriteDataTable(writer, table);
			}
		}

		private void WriteDataTable(BinaryWriter writer, DataTable table)
		{
			if (string.IsNullOrEmpty(table.TableName))
				table.TableName = "tbl_" + DateTime.Now.Ticks.GetHashCode().ToString();
			WriteString(writer, GetXmlSchema(table));

			// Int32
			PrimitivesConvertion.WriteVarInt(writer, table.Rows.Count);

			var colsCount = table.Columns.Count;
			foreach (DataRow row in table.Rows)
			{
				WriteDataRow(writer, row, colsCount);
			}
		}

		private void WriteDataRow(BinaryWriter writer, DataRow row, int columnCount)
		{
			var values = new Dictionary<int, object>();
			for (int i = 0; i < columnCount; i++)
			{
				var val = row[i];
				if (val != null && !Convert.IsDBNull(val))
					values.Add(i, val);
			}

			// count of non-null columns
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, values.Count);

			foreach (var value in values)
			{
				// Int32
				PrimitivesConvertion.WriteVarInt(writer, value.Key);

				WriteValue(writer, value.Value);
			}
		} 

		private void WriteEnum(BinaryWriter writer, Enum e)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, (int)((object)e));
		}

		private void WriteGenericList(BinaryWriter writer, IEnumerable array)
		{
			int count = 0;
			var col = array as ICollection;
			if (col != null)
				count = (int)col.Count;
			else
			{
				foreach (object obj in array)
					count++;
			}
			var itemType = GetMemberInfo(array.GetType().GetGenericArguments()[0]);
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, count);
			foreach (object obj in array)
			{
				WriteValue(writer, obj, itemType);
			}
		}
		private void WriteArray(BinaryWriter writer, object array)
		{
			var arr = array as Array;
			if (arr == null)
			{
				var enumurable = (IEnumerable)array;

				int count = 0;
				var col = array as ICollection;
				if (col != null)
				{
					count = (int)col.Count;
				}
				else
				{
					foreach (object obj in enumurable)
						count++;
				}

				// Int32
				PrimitivesConvertion.WriteVarInt(writer, count);
				foreach (object obj in enumurable)
				{
					WriteValue(writer, obj);
				}
			}
			else
			{
				// Int32
				PrimitivesConvertion.WriteVarInt(writer, arr.Length);

				var type = ReflectionHelper.FindUnderlyingArrayElementType(arr.GetType());
				var bionType = _typeCache.GetTypeInfo(type, true);
				for (int i = 0; i < arr.Length; i++)
				{
					WriteValue(writer, arr.GetValue(i), bionType);
				}
			}
		}

		private void WriteBytes(BinaryWriter writer, byte[] bytes)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, bytes.Length);
			writer.Write(bytes);
		}

		private void WriteDictionary(BinaryWriter writer, IDictionary dic)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, dic.Count);

			var theType = dic.GetType();
			var genericTypes = ReflectionHelper.FindUnderlyingGenericDictionaryElementType(theType);

			if (genericTypes == null)
			{
				var dictionaryType = ReflectionHelper.FindUnderlyingGenericElementType(theType);
				genericTypes = dictionaryType.GetGenericArguments();
			}

			var keyType = GetMemberInfo(genericTypes[0]);
			var valType = GetMemberInfo(genericTypes[1]);

			foreach (DictionaryEntry entry in dic)
			{
				WriteValue(writer, entry.Key, keyType);
				WriteValue(writer, entry.Value, valType);
			}
		}

		private void WriteStringDictionary(BinaryWriter writer, IDictionary dic)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, dic.Count);

			var genericType = dic.GetType().GetGenericArguments();
			var keyType = GetMemberInfo(typeof(string));
			var valType = GetMemberInfo(genericType[1]);

			foreach (DictionaryEntry entry in dic)
			{
				WriteValue(writer, entry.Key.ToString(), keyType);
				WriteValue(writer, entry.Value, valType);
			}
		}

		[Obsolete]
		private void WritePairString(BinaryWriter writer, string name, object value)
		{
			WriteString(writer, name);
			WriteValue(writer, value);
		}

		[Obsolete]
		private void WritePairObject(BinaryWriter writer, object key, object value)
		{
			WriteValue(writer, key);
			WriteValue(writer, value);
		}

		private void WriteDateTime(BinaryWriter writer, DateTime dateTime)
		{
			var dt = dateTime;
			var kind = (byte)dt.Kind;

			if (dateTime == DateTime.MinValue)
			{
				writer.Write(kind);
				// min datetime indicator
				PrimitivesConvertion.WriteVarInt(writer, 0L);
			}
			else if (dateTime == DateTime.MaxValue)
			{
				writer.Write(kind);
				// max datetime indicator
				PrimitivesConvertion.WriteVarInt(writer, 1L);
			}
			else
			{
				writer.Write(kind);
				//Int64
				PrimitivesConvertion.WriteVarInt(writer, dt.Ticks);
			}
		}
		private void WriteDateTimeOffset(BinaryWriter writer, DateTimeOffset dateTimeOffset)
		{
			var dt = dateTimeOffset;
			var offset = dateTimeOffset.Offset;
			short offsetMinutes;
			unchecked
			{
				offsetMinutes = (short)((offset.Hours * 60) + offset.Minutes);
			}
			// int16
			PrimitivesConvertion.WriteVarInt(writer, offsetMinutes);

			// int64
			PrimitivesConvertion.WriteVarInt(writer, dt.Ticks);
		}

		private void WriteTimeSpan(BinaryWriter writer, TimeSpan timeSpan)
		{
			// Int64
			PrimitivesConvertion.WriteVarInt(writer, timeSpan.Ticks);
		}
		private void WriteVersion(BinaryWriter writer, Version version)
		{
			WriteString(writer, version.ToString());
		}

		private void WriteString(BinaryWriter writer, string str)
		{
			if (str == null)
			{
				PrimitivesConvertion.WriteVarInt(writer, (int?)null);
			}
			else if (str.Length == 0)
			{
				PrimitivesConvertion.WriteVarInt(writer, (int?)0);
			}
			else
			{
				var strBytes = Encoding.GetBytes(str);
				// Int32
				PrimitivesConvertion.WriteVarInt(writer, (int?)strBytes.Length);
				writer.Write(strBytes);
			}
		}

		private void WriteGuid(BinaryWriter writer, Guid g)
		{
			if (g == Guid.Empty)
			{
				// Int32
				PrimitivesConvertion.WriteVarInt(writer, 0);
				return;
			}

			var data = g.ToByteArray();
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, data.Length);
			writer.Write(data);
		}
		private void WriteUri(BinaryWriter writer, Uri uri)
		{
			WriteString(writer, uri.OriginalString);
		}
		#endregion

		#region Deserialization methods

		private object ReadObject(BinaryReader reader, Type type)
		{
			var bionType = _typeCache.GetTypeInfo(type, true) as TypeInfo;


			var members = bionType.Members;
			var resultObj = _typeCache.CreateInstance(type);

			// Read the members
			ReadMembers(reader, resultObj, members);
			return resultObj;
		}

		private void ReadMembers(BinaryReader reader, object obj, MemberInfo[] memberList)
		{
			var objectMemberCount = memberList.Length;
			var memberProcessed = 0;
			var dataLeng = reader.BaseStream.Length;
			var data = reader.BaseStream;

			// while all members are processed
			while (memberProcessed < objectMemberCount &&
				   data.Position < dataLeng)
			{
				// the member from member list according to the index
				var memInfo = memberList[memberProcessed];
				memberProcessed++;

				// set the value
				if (memInfo.MemberType == EnMemberType.Property)
				{
					var pinfo = memInfo.Info as PropertyInfo;

					// read the value
					var value = ReadMember(reader, memInfo, pinfo.PropertyType);

					// using the setter
					memInfo.PropertySetter(obj, value);
				}
				else
				{
					var finfo = memInfo.Info as System.Reflection.FieldInfo;

					// read the value
					var value = ReadMember(reader, memInfo, finfo.FieldType);

					ReflectionHelper.SetValue(obj, value, finfo);
				}

			}
		}

		private object ReadMember(BinaryReader reader, Type memType)
		{
			var memInfo = _typeCache.GetTypeInfo(memType, true);
			return ReadMember(reader, memInfo, memType);
		}

		private object ReadMember(BinaryReader reader, MemberInfo memInfo, Type memType)
		{
			if ((memInfo.IsNullable && memInfo.IsContainerObject) ||
				(memInfo.IsNullable && !memInfo.IsSupportedPrimitive && (!memInfo.IsContainerObject || memInfo.IsStruct)))
			{
				bool isNull = reader.ReadByte() != 0;

				if (isNull)
				{
					return null;
				}
			}
			var actualMemberType = memType;
			if (memInfo.IsNullable && memInfo.NullableUnderlyingType != null)
			{
				actualMemberType = memInfo.NullableUnderlyingType;
			}

			switch (memInfo.KnownType)
			{
				case EnKnownType.Unknown:

					if (memInfo.IsContainerObject)
					{
						return ReadObject(reader, actualMemberType);
					}
					else if (memInfo.IsStringDictionary)
					{
						return ReadStringDictionary(reader, actualMemberType);
					}
					else if (memInfo.IsDictionary)
					{
						return ReadDictionary(reader, actualMemberType);
					}
					else if (memInfo.IsCollection)
					{
						if (memInfo.IsGeneric)
						{
							return ReadGenericList(reader, actualMemberType);
						}
						return ReadArray(reader, actualMemberType);
					}
					else if (memInfo.IsArray)
					{
						return ReadArray(reader, actualMemberType);
					}

					break;

				case EnKnownType.Int16:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt16Nullable(reader);
					}
					return PrimitivesConvertion.ReadVarInt16(reader);

				case EnKnownType.UInt16:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarUInt16Nullable(reader);
					}
					return PrimitivesConvertion.ReadVarUInt16(reader);

				case EnKnownType.Int32:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt32Nullable(reader);
					}
					return PrimitivesConvertion.ReadVarInt32(reader);

				case EnKnownType.Int64:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt64Nullable(reader);
					}
					return PrimitivesConvertion.ReadVarInt64(reader);

				case EnKnownType.UInt64:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarUInt64Nullable(reader);
					}
					return PrimitivesConvertion.ReadVarUInt64(reader);

				case EnKnownType.UInt32:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarUInt32Nullable(reader);
					}
					return PrimitivesConvertion.ReadVarUInt32(reader);
				case EnKnownType.Double:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarDoubleNullable(reader);
					}
					return PrimitivesConvertion.ReadVarDouble(reader);

				case EnKnownType.Decimal:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarDecimalNullable(reader);
					}
					return PrimitivesConvertion.ReadVarDecimal(reader);

				case EnKnownType.Single:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarSingleNullable(reader);
					}
					return PrimitivesConvertion.ReadVarSingle(reader);

				case EnKnownType.Byte:
					return reader.ReadByte();

				case EnKnownType.SByte:
					return reader.ReadSByte();

				case EnKnownType.ByteArray:
					return ReadBytes(reader);

				case EnKnownType.String:
					return ReadString(reader);

				case EnKnownType.Char:
					var charByte = reader.ReadUInt16();
					return (char)charByte;

				case EnKnownType.Bool:
					return ReadBoolean(reader);

				case EnKnownType.Enum:
					return ReadEnum(reader, actualMemberType);

				case EnKnownType.DateTime:
					return ReadDateTime(reader);

				case EnKnownType.DateTimeOffset:
					return ReadDateTimeOffset(reader);

				case EnKnownType.TimeSpan:
					return ReadTimeSpan(reader);
                     
				case EnKnownType.DataSet:
					return ReadDataset(reader, actualMemberType);

				case EnKnownType.DataTable:
					return ReadDataTable(reader);

				case EnKnownType.NameValueColl:
					return ReadCollectionNameValue(reader, actualMemberType);

				case EnKnownType.Color:
					return ReadColor(reader); 

				case EnKnownType.Version:
					return ReadVersion(reader); 
				case EnKnownType.DbNull:
					return DBNull.Value; 

				case EnKnownType.Guid:
					return ReadGuid(reader);

				case EnKnownType.Uri:
					return ReadUri(reader);

				default:
					throw new ArgumentOutOfRangeException();
			}
			return null;
		}

		private object ReadEnum(BinaryReader reader, Type type)
		{
			var val = PrimitivesConvertion.ReadVarInt32(reader);
			return Enum.ToObject(type, val);
		}

		private object ReadArray(BinaryReader reader, Type type)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);

			var itemType = type.GetElementType();
			if (itemType == null)
			{
				itemType = ReflectionHelper.FindUnderlyingGenericElementType(type);

				if (itemType == null)
					throw new ArgumentException("Unknown 'Object' array type is not supported.\n" + type);
			}

			IList lst;
			if (type.IsArray)
			{
				var arr = ReflectionHelper.CreateArray(itemType, count);
				lst = arr as IList;

				for (int i = 0; i < count; i++)
				{
					var val = ReadMember(reader, itemType);
					lst[i] = val;
				}
				return arr;
			}
			else
			{
				lst = _typeCache.CreateInstance(type) as IList;

				for (int i = 0; i < count; i++)
				{
					var val = ReadMember(reader, itemType);
					lst.Add(val);
				}
				return lst;
			}

		}

		private IList ReadGenericList(BinaryReader reader, Type type)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);

			var typeToCreate = type;
			var itemType = type.GetGenericArguments()[0];

			var ilistBase = typeof(IList<>);
			if (ilistBase == type.GetGenericTypeDefinition())
			{
				typeToCreate = typeof(List<>).MakeGenericType(itemType);
			}

			var listObj = (IList)_typeCache.CreateInstanceDirect(typeToCreate);

			for (int i = 0; i < count; i++)
			{
				var val = ReadMember(reader, itemType);
				listObj.Add(val);
			}

			return listObj;
		}

		private IList ReadIListImpl(BinaryReader reader, Type type)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);

			var listObj = (IList)_typeCache.CreateInstance(type);

			var itemType = type.GetElementType();
			if (itemType == null)
				throw new ArgumentException("Unknown ICollection implementation is not supported.\n" + type.ToString());

			for (int i = 0; i < count; i++)
			{
				var val = ReadMember(reader, itemType);
				listObj.Add(val);
			}

			return listObj;
		}
		private byte[] ReadBytes(BinaryReader reader)
		{
			var length = PrimitivesConvertion.ReadVarInt32(reader);
			return reader.ReadBytes(length);
		}
         
		private object ReadColor(BinaryReader reader)
		{
			return Color.FromArgb(PrimitivesConvertion.ReadVarInt32(reader));
		}
		private object ReadCollectionNameValue(BinaryReader reader, Type type)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);
			var nameValue = (NameValueCollection)_typeCache.CreateInstance(type);
			var strType = typeof(string);
			for (int i = 0; i < count; i++)
			{
				var name = ReadMember(reader, strType) as string;
				var val = ReadMember(reader, strType) as string;
				nameValue.Add(name, val);
			}
			return nameValue;
		}
		private DataTable ReadDataTable(BinaryReader reader)
		{
			var dt = _typeCache.CreateInstance(typeof(DataTable)) as DataTable;

			var schema = ReadString(reader);
			//dt.TableName = name;
			SetXmlSchema(dt, schema);

			var cols = dt.Columns;

			var rowCount = PrimitivesConvertion.ReadVarInt32(reader);
			for (int i = 0; i < rowCount; i++)
			{
				var row = dt.Rows.Add();
				ReadDataRow(reader, row, cols);
			}
			return dt;
		}

		private void ReadDataRow(BinaryReader reader, DataRow row, DataColumnCollection cols)
		{
			var itemCount = PrimitivesConvertion.ReadVarInt32(reader);
			var colCount = cols.Count;
			var passedIndex = 0;
			while (passedIndex < colCount && passedIndex < itemCount)
			{
				passedIndex++;

				var colIndex = PrimitivesConvertion.ReadVarInt32(reader);
				var col = cols[colIndex];
				var val = ReadMember(reader, col.DataType);
				row[col] = val ?? DBNull.Value;
			}

		}

		private object ReadDataset(BinaryReader reader, Type memType)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);
			var ds = _typeCache.CreateInstance(memType) as DataSet;
			for (int i = 0; i < count; i++)
			{
				var dt = ReadDataTable(reader);
				ds.Tables.Add(dt);
			}
			return ds;
		}
		private void SetXmlSchema(DataTable dt, string schema)
		{
			using (var reader = new StringReader(schema))
			{
				dt.ReadXmlSchema(reader);
			}
		} 

		private object ReadDictionary(BinaryReader reader, Type memType)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);

			var typeToCreate = memType;
			var genericArgs = memType.GetGenericArguments();

			var genericBase = typeof(IDictionary<,>);
			if (genericBase == memType.GetGenericTypeDefinition())
			{
				typeToCreate = typeof(Dictionary<,>).MakeGenericType(genericArgs);
			}

			var dic = _typeCache.CreateInstanceDirect(typeToCreate) as IDictionary;

			var keyType = genericArgs[0];
			var valType = genericArgs[1];

			for (int i = 0; i < count; i++)
			{
				var key = ReadMember(reader, keyType);
				var val = ReadMember(reader, valType);
				dic.Add(key, val);
			}
			return dic;
		}

		private object ReadStringDictionary(BinaryReader reader, Type memType)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);
			var dic = _typeCache.CreateInstance(memType) as IDictionary;

			var genericType = memType.GetGenericArguments();
			var keyType = genericType[0];
			var valType = genericType[1];

			for (int i = 0; i < count; i++)
			{
				var key = ReadMember(reader, keyType);
				var val = ReadMember(reader, valType);
				dic.Add(key, val);
			}
			return dic;
		}

		private TimeSpan ReadTimeSpan(BinaryReader reader)
		{
			var ticks = PrimitivesConvertion.ReadVarInt64(reader);
			return new TimeSpan(ticks);
		}
		private object ReadVersion(BinaryReader reader)
		{
			return new Version(ReadString(reader));
		}

		private DateTime ReadDateTime(BinaryReader reader)
		{
			var kind = reader.ReadByte();
			var ticks = PrimitivesConvertion.ReadVarInt64(reader);
			if (ticks == 0L)
			{
				return DateTime.MinValue;
			}
			if (ticks == 1L)
			{
				return DateTime.MaxValue;
			}

			return new DateTime(ticks, (DateTimeKind)kind);
		}

		private DateTimeOffset ReadDateTimeOffset(BinaryReader reader)
		{
			var offsetMinutes = PrimitivesConvertion.ReadVarInt16(reader);

			var ticks = PrimitivesConvertion.ReadVarInt64(reader);

			return new DateTimeOffset(ticks, TimeSpan.FromMinutes(offsetMinutes));
		}

		private bool ReadBoolean(BinaryReader reader)
		{
			return reader.ReadByte() != 0;
		}

		private string ReadString(BinaryReader reader)
		{
			int? length = PrimitivesConvertion.ReadVarInt32Nullable(reader);
			if (length == null)
			{
				return null;
			}
			else if (length == 0)
			{
				return string.Empty;
			}
			else
			{
				var strBuff = reader.ReadBytes(length.Value);
				return Encoding.GetString(strBuff, 0, strBuff.Length);
			}
		}

		private object ReadGuid(BinaryReader reader)
		{
			var gbuff = ReadBytes(reader);
			if (gbuff.Length == 0)
				return Guid.Empty;
			return new Guid(gbuff);
		}
		private object ReadUri(BinaryReader reader)
		{
			var uri = ReadString(reader);
			return new Uri(uri);
		}
		#endregion

	}
}
