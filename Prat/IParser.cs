using System;
#if NETSTANDARD2_1
using System.Collections.Generic;
#endif

namespace Prat
{
	/// <summary>
	/// A parser reads the provided string from begining to end and tries to parse as much as possible of it.
	/// It returns a tuple of the parsed part converted to any type <typeparamref name="T"/>, and the unparsed part of the string.
	/// If the parser can not parse anything it should return <see cref="null"/>.
	/// </summary>
	/// <typeparam name="T">The type of the outputs from this parser.</typeparam>
	public interface IParser<T>
	{
		(T, ReadOnlyMemory<char>)? Parse(ReadOnlyMemory<char> s);

#if NETSTANDARD2_1
		public static IParser<T> operator >(IParser<T> left, IParser<T> right) => Parsers.KeepRight(left, right);
		public static IParser<T> operator <(IParser<T> left, IParser<T> right) => Parsers.KeepLeft(left, right);

		public static IParser<T> operator |(IParser<T> left, IParser<T> right) => Parsers.Either(left, right);
		public static IParser<T> operator |(IParser<T> parser, T defaultValue) => Parsers.OneOrDefault(parser, defaultValue);

		public static IParser<IEnumerable<T>> operator +(IParser<T> one, Func<IParser<IEnumerable<T>>> manyFactory) => Parsers.OnePlusMany(one, manyFactory);
#endif
	}
}