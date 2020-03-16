using System;

namespace Prat
{
	class Failure<T> : IParser<T>
	{
		public (T, ReadOnlyMemory<char>)? Parse(ReadOnlyMemory<char> s) => null;
	}
}