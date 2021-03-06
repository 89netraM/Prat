﻿using System;

namespace Prat
{
	class Either<T> : IParser<T>
	{
		private readonly Lazy<IParser<T>> First;
		private readonly Lazy<IParser<T>> Second;

		internal Either(Lazy<IParser<T>> first, Lazy<IParser<T>> second)
		{
			First = first ?? throw new ArgumentNullException(nameof(first));
			Second = second ?? throw new ArgumentNullException(nameof(second));
		}

		public (T, ReadOnlyMemory<char>)? Parse(ReadOnlyMemory<char> s) => First.Value.Parse(s) switch
		{
			null => Second.Value.Parse(s),
			ValueTuple<T, ReadOnlyMemory<char>> t => t
		};
	}
}