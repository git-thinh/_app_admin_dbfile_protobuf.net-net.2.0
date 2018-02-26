using System;
 
namespace app.Core.CacheFile
{
	/// <summary>
	/// Can be used for classes and structs to specify that the serializer should serialize fields and properties or not.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ContractAttribute : Attribute
	{
		/// <summary>
		/// Specifies that fields should be serialized or not.
		/// </summary>
		public bool Fields { get; set; }

		/// <summary>
		/// Specifies that properties should be serialized or not.
		/// </summary>
		public bool Properties { get; set; }

		/// <summary>
		/// Can be used for classes and structs to specify that the serializer should serialize fields or properties.
		/// </summary>
		/// <param name="fields">Specifies that fields should be serialized or not.</param>
		/// <param name="properties">Specifies that properties should be serialized or not.</param>
		public ContractAttribute(bool fields, bool properties)
		{
			Fields = fields;
			Properties = properties;
		}

		/// <summary>
		/// Can be used for classes and structs to specify that the serializer should serialize fields or properties.
		/// </summary>
		public ContractAttribute()
			: this(true, true)
		{ }
	}
}
