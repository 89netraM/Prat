using System;
using System.Collections.Generic;
using System.Linq;

namespace Prat
{
	class Best<T> : IParser<T>
	{
		private readonly IEnumerable<Lazy<IParser<T>>> ParserFactories;

		internal Best(IEnumerable<Lazy<IParser<T>>> parserFactories)
		{
			ParserFactories = parserFactories ?? throw new ArgumentNullException(nameof(parserFactories));
		}

		public (T, ReadOnlyMemory<char>)? Parse(ReadOnlyMemory<char> s)
		{
			IEnumerable<(T, ReadOnlyMemory<char>)?> successfulParsings = ParserFactories
				.Select(p => p.Value.Parse(s))
				.Where(r => r.HasValue);
			if (successfulParsings.Any())
			{
				return successfulParsings
					.Aggregate((a, b) => a.Value.Item2.Length < b.Value.Item2.Length ? a : b);
			}
			else
			{
				return null;
			}
		}
	}
}
