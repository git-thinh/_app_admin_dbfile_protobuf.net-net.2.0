 

namespace app.Core.CacheFile
{
	class TypeInfo : MemberInfo
	{
		public MemberInfo[] Members;
#if DEBUG
		public override string ToString()
		{
			return string.Format("{0}: {1}: {2}: Members= {3}", MemberType, KnownType, Info,
								 (Members != null) ? (Members.Length) : 0);
		}
#endif
	}
}
