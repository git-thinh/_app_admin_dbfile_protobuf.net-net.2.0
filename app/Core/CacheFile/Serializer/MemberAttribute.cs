using System;
 
namespace app.Core.CacheFile
{
	/// <summary>
	/// Specifies a field or peroperty settings for serialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class MemberAttribute : Attribute
	{
		/// <summary>
		/// In which order should this member be serialized.
		/// </summary>
		public int Index { get; private set; }

		/// <summary>
		/// Specifies that should this member be included in serialization.
		/// </summary>
		public bool Included { get; private set; }

		/// <summary>
		/// Specifies a field or peroperty settings for serialization.
		/// </summary>
		/// <param name="index">In which order should this member be serialized.</param>
		/// <param name="included">Specifies that should this member be included in serialization.</param>
		public MemberAttribute(int index, bool included)
		{
			Index = index;
			Included = included;
		}

		/// <summary>
		/// Specifies a field or peroperty settings for serialization.
		/// </summary>
		public MemberAttribute()
			: this(-1, true)
		{ }

		/// <summary>
		/// Specifies a field or peroperty settings for serialization.
		/// </summary>
		/// <param name="index">In which order should this member be serialized.</param>
		public MemberAttribute(int index)
			: this(index, true)
		{ }

		/// <summary>
		/// Specifies a field or peroperty settings for serialization.
		/// </summary>
		/// <param name="included">Specifies that should this member be included in serialization.</param>
		public MemberAttribute(bool included)
			: this(-1, included)
		{ }
	}
}