using System;

namespace Prat
{
	class Success<T> : IParser<T>
	{
		private readonly Lazy<T> A;

		internal Success(Lazy<T> a)
		{
			A = a;
		}

		public (T, ReadOnlyMemory<char>)? Parse(ReadOnlyMemory<char> s)
		{
			return (A.Value, s);
		}
	}
}