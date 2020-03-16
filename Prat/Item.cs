using System;

namespace Prat
{
	class Item : IParser<char>
	{
		public (char, ReadOnlyMemory<char>)? Parse(ReadOnlyMemory<char> s)
		{
			if (s.Length == 0)
			{
				return null;
			}
			else
			{
				return (s.Span[0], s.Slice(1));
			}
		}
	}
}