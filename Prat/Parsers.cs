using System;
using System.Collections.Generic;
using System.Linq;

namespace Prat
{
	/// <summary>
	/// Provides basic <see cref="IParser{T}"/>s for building more useful parsers.
	/// </summary>
	public static partial class Parsers
	{
		private static Lazy<T> ToLazy<T>(T item) => new Lazy<T>(() => item);
		private static Lazy<T> ToLazy<T>(Func<T> factory) => new Lazy<T>(factory);

		#region Extensions
		/// <summary>
		/// Can be used as an entrypoint for using parsers.
		/// <b>Should not be used internally for implementation of parsers.</b>
		/// </summary>
		public static (T, string)? Parse<T>(this IParser<T> @this, string str) => @this.Parse(str.AsMemory()) switch
		{
			null => null,
			(T t, ReadOnlyMemory<char> cs) => (t, cs.ToString())
		};

		/// <summary>
		/// Returns a modified parser to that uses <paramref name="func"/> to
		/// transform the return value.
		/// </summary>
		/// <param name="func">Transforms the output value of the original parser.</param>
		public static IParser<B> Select<A, B>(this IParser<A> parser, Func<A, B> func)
		{
			return new Using<A, B>(ToLazy(parser), ToLazy<Func<A, IParser<B>>>(f => new Success<B>(ToLazy(func(f)))));
		}

		/// <summary>
		/// Returns a parser that use either <paramref name="this"/> parser or
		/// the <paramref name="other"/> parser.
		/// </summary>
		public static IParser<T> Or<T>(this IParser<T> @this, IParser<T> other)
		{
			return new Either<T>(ToLazy(@this), ToLazy(other));
		}
		/// <summary>
		/// Returns a parser that use either <paramref name="this"/> parser or
		/// the resulting parser of the <paramref name="otherFactory"/>.
		/// </summary>
		public static IParser<T> Or<T>(this IParser<T> @this, Func<IParser<T>> otherFactory)
		{
			return new Either<T>(ToLazy(@this), ToLazy(otherFactory));
		}

		/// <summary>
		/// Returns a parser that first parses using <paramref name="this"/>
		/// parser and then using the <paramref name="other"/> parser. Eventually
		/// returning the result of the <paramref name="other"/> parser.
		/// </summary>
		public static IParser<B> And<A, B>(this IParser<A> @this, IParser<B> other)
		{
			return new Both<A, B>(ToLazy(@this), ToLazy(other));
		}
		/// <summary>
		/// Returns a parser that first parses using <paramref name="this"/>
		/// parser and then using the resulting parser of the
		/// <paramref name="otherFactory"/>. Eventually returning the result of
		/// the <paramref name="otherFactory"/>s parser.
		/// </summary>
		public static IParser<B> And<A, B>(this IParser<A> @this, Func<IParser<B>> otherFactory)
		{
			return new Both<A, B>(ToLazy(@this), ToLazy(otherFactory));
		}

		/// <summary>
		/// Tries to parse with <paramref name="this"/> parser, if that fails it will
		/// instead return the <see cref="default"/> value of
		/// <typeparamref name="T"/>.
		/// </summary>
		public static IParser<T> OrDefault<T>(this IParser<T> @this)
		{
			return @this.Or(Success(default(T)));
		}
		/// <summary>
		/// Tries to parse with <paramref name="this"/> parser, if that fails it will
		/// instead return the <paramref name="defaultValue"/>.
		/// </summary>
		public static IParser<T> OrDefault<T>(this IParser<T> @this, T defaultValue)
		{
			return @this.Or(Success(defaultValue));
		}
		/// <summary>
		/// Tries to parse with <paramref name="this"/> parser, if that fails it will
		/// instead return the resulting value of the
		/// <paramref name="defaultValueFactory"/>.
		/// </summary>
		public static IParser<T> OrDefault<T>(this IParser<T> @this, Func<T> defaultValueFactory)
		{
			return @this.Or(Success(defaultValueFactory));
		}

		/// <summary>
		/// Parses first one thing using <paramref name="this"/> parser, and
		/// then a sequence of things using the <paramref name="many"/> parser.
		/// Returns a sequence with the first thing followed by the sequence of
		/// many things.
		/// </summary>
		public static IParser<IEnumerable<T>> PlusMany<T>(this IParser<T> @this, IParser<IEnumerable<T>> many)
		{
			return new Using<T, IEnumerable<T>>(ToLazy(@this), ToLazy<Func<T, IParser<IEnumerable<T>>>>(t => many.Select(ts => ts.Prepend(t))));
		}
		/// <summary>
		/// Parses first one thing using <paramref name="this"/> parser, and
		/// then a sequence of things using the resulting parser of the
		/// <paramref name="manyFactory"/>. Returns a sequence with the first
		/// thing followed by the sequence of many things.
		/// </summary>
		public static IParser<IEnumerable<T>> PlusMany<T>(this IParser<T> @this, Func<IParser<IEnumerable<T>>> manyFactory)
		{
			return new Using<T, IEnumerable<T>>(ToLazy(@this), ToLazy<Func<T, IParser<IEnumerable<T>>>>(t => manyFactory().Select(ts => ts.Prepend(t))));
		}

		/// <summary>
		/// Parses using <paramref name="this"/> parser zeror or more times and
		/// returns a sequence of all parsed items.
		/// </summary>
		public static IParser<IEnumerable<T>> ZeroOrMore<T>(this IParser<T> @this)
		{
			return @this.OnceOrMore().OrDefault(Enumerable.Empty<T>());
		}

		/// <summary>
		/// Parses using <paramref name="this"/> parser once or more times and
		/// returns a sequence of all parsed items.
		/// </summary>
		public static IParser<IEnumerable<T>> OnceOrMore<T>(this IParser<T> @this)
		{
			return @this.PlusMany(() => @this.ZeroOrMore());
		}
		#endregion Extensions

		#region Basic
		/// <summary>
		/// Fails without reading anything.
		/// Used for building more complicated parsers.
		/// </summary>
		public static IParser<T> Failure<T>() => new Failure<T>();

		/// <summary>
		/// Succeeds and returns <paramref name="value"/> without reading anything.
		/// </summary>
		public static IParser<T> Success<T>(T value)
		{
			return new Success<T>(ToLazy(value));
		}
		/// <summary>
		/// Succeeds and returns the resulting value of the
		/// <paramref name="valueFactory"/> without reading anything.
		/// </summary>
		public static IParser<T> Success<T>(Func<T> valueFactory)
		{
			return new Success<T>(ToLazy(valueFactory));
		}

		/// <summary>
		/// Reads and returns the first <see cref="char"/> if one is avalible.
		/// Otherwise fails without reading anything.
		/// </summary>
		public static IParser<char> Item() => new Item();

		/// <summary>
		/// Reads and returns the first <see cref="char"/> if it satisfies the <paramref name="predicate"/>.
		/// Otherwise fails without reading anything.
		/// </summary>
		public static IParser<char> Sat(Predicate<char> predicate) => new Using<char, char>
		(
			ToLazy<IParser<char>>(new Item()),
			ToLazy<Func<char, IParser<char>>>(c => predicate(c) ? Success(c) : Failure<char>())
		);
		#endregion Basic

		#region Combinatory
		/// <summary>
		/// Tries to parse the input with the <paramref name="first"/> parser
		/// and returns it's value. If that fails it will instead try to parse
		/// using the <paramref name="second"/> parser, returning it's value no
		/// matter what.
		/// </summary>
		public static IParser<T> Either<T>(IParser<T> first, IParser<T> second)
		{
			return new Either<T>(ToLazy(first), ToLazy(second));
		}
		/// <summary>
		/// Tries to parse the input with the resulting parser of the
		/// <paramref name="firstFactory"/> and returns it's value. If that
		/// fails it will instead try to parse using the
		/// <paramref name="second"/> parser, returning it's value no matter
		/// what.
		/// </summary>
		public static IParser<T> Either<T>(Func<IParser<T>> firstFactory, IParser<T> second)
		{
			return new Either<T>(ToLazy(firstFactory), ToLazy(second));
		}
		/// <summary>
		/// Tries to parse the input with the <paramref name="first"/> parser
		/// and returns it's value. If that fails it will instead try to parse
		/// using the resulting parser of the <paramref name="secondFactory"/>,
		/// returning it's value no matter what.
		/// </summary>
		public static IParser<T> Either<T>(IParser<T> first, Func<IParser<T>> secondFactory)
		{
			return new Either<T>(ToLazy(first), ToLazy(secondFactory));
		}
		/// <summary>
		/// Tries to parse the input with the resulting parser of the
		/// <paramref name="firstFactory"/> and returns it's value. If that
		/// fails it will instead try to parse using the resulting parser of
		/// the <paramref name="secondFactory"/>, returning it's value no
		/// matter what.
		/// </summary>
		public static IParser<T> Either<T>(Func<IParser<T>> firstFactory, Func<IParser<T>> secondFactory)
		{
			return new Either<T>(ToLazy(firstFactory), ToLazy(secondFactory));
		}

		/// <summary>
		/// Parses the input with the <paramref name="first"/> parser and
		/// passing it's result to the factory of the second parser. Then
		/// continues to parse with the second parser.
		/// </summary>
		/// <param name="secondFactory">
		/// A function that creates a parser based on the first parsers output.
		/// </param>
		public static IParser<B> Using<A, B>(IParser<A> first, Func<A, IParser<B>> secondFactory)
		{
			return new Using<A, B>(ToLazy(first), ToLazy(secondFactory));
		}
		/// <summary>
		/// Parses the input with the resulting parser of the
		/// <paramref name="firstFactory"/> and passing it's result to the
		/// factory of the second parser. Then continues to parse with the
		/// second parser.
		/// </summary>
		/// <param name="secondFactory">
		/// A function that creates a parser based on the first parsers output.
		/// </param>
		public static IParser<B> Using<A, B>(Func<IParser<A>> firstFactory, Func<A, IParser<B>> secondFactory)
		{
			return new Using<A, B>(ToLazy(firstFactory), ToLazy(secondFactory));
		}
		/// <summary>
		/// Parses the input with the <paramref name="first"/> parser and
		/// passing it's result to the factory of the second parser. Then
		/// continues to parse with the second parser.
		/// </summary>
		/// <param name="secondFactoryFactory">
		/// A function that creates a function that creates a parser based on
		/// the first parsers output.
		/// </param>
		public static IParser<B> Using<A, B>(IParser<A> first, Func<Func<A, IParser<B>>> secondFactoryFactory)
		{
			return new Using<A, B>(ToLazy(first), ToLazy(secondFactoryFactory));
		}
		/// <summary>
		/// Parses the input with the resulting parser of the
		/// <paramref name="firstFactory"/> and passing it's result to the
		/// factory of the second parser. Then continues to parse with the
		/// second parser.
		/// </summary>
		/// <param name="secondFactoryFactory">
		/// A function that creates a function that creates a parser based on
		/// the first parsers output.
		/// </param>
		public static IParser<B> Using<A, B>(Func<IParser<A>> firstFactory, Func<Func<A, IParser<B>>> secondFactoryFactory)
		{
			return new Using<A, B>(ToLazy(firstFactory), ToLazy(secondFactoryFactory));
		}

		/// <summary>
		/// Parses the input first with the <paramref name="left"/> parser and
		/// then with the <paramref name="right"/> parser, eventually
		/// returning the result from the left parser.
		/// </summary>
		public static IParser<A> KeepLeft<A, B>(IParser<A> left, IParser<B> right)
		{
			return new Using<A, A>(ToLazy(left), ToLazy<Func<A, IParser<A>>>(l => new Both<B, A>(ToLazy(right), ToLazy(Success(l)))));
		}
		/// <summary>
		/// Parses the input first with the resulting parser of the
		/// <paramref name="leftFactory"/> and then with the
		/// <paramref name="right"/> parser, eventually returning the result
		/// from the left parser.
		/// </summary>
		public static IParser<A> KeepLeft<A, B>(Func<IParser<A>> leftFactory, IParser<B> right)
		{
			return new Using<A, A>(ToLazy(leftFactory), ToLazy<Func<A, IParser<A>>>(l => new Both<B, A>(ToLazy(right), ToLazy(Success(l)))));
		}
		/// <summary>
		/// Parses the input first with the <paramref name="left"/> parser and
		/// then with the resulting parser of the
		/// <paramref name="rightFactory"/>, eventually returning the result
		/// from the left parser.
		/// </summary>
		public static IParser<A> KeepLeft<A, B>(IParser<A> left, Func<IParser<B>> rightFactory)
		{
			return new Using<A, A>(ToLazy(left), ToLazy<Func<A, IParser<A>>>(l => new Both<B, A>(ToLazy(rightFactory), ToLazy(Success(l)))));
		}
		/// <summary>
		/// Parses the input first with the resulting parser of the
		/// <paramref name="leftFactory"/> and then with the resulting parser
		/// of the <paramref name="rightFactory"/>, eventually returning the
		/// result from the left parser.
		/// </summary>
		public static IParser<A> KeepLeft<A, B>(Func<IParser<A>> leftFactory, Func<IParser<B>> rightFactory)
		{
			return new Using<A, A>(ToLazy(leftFactory), ToLazy<Func<A, IParser<A>>>(l => new Both<B, A>(ToLazy(rightFactory), ToLazy(Success(l)))));
		}

		/// <summary>
		/// Parses the input first with the <paramref name="left"/> parser and
		/// then with the <paramref name="right"/> parser, eventually
		/// returning the result from the right parser.
		/// </summary>
		public static IParser<B> KeepRight<A, B>(IParser<A> left, IParser<B> right)
		{
			return new Both<A, B>(ToLazy(left), ToLazy(right));
		}
		/// <summary>
		/// Parses the input first with the resulting parser of the
		/// <paramref name="leftFactory"/> and then with the
		/// <paramref name="right"/> parser, eventually returning the result
		/// from the right parser.
		/// </summary>
		public static IParser<B> KeepRight<A, B>(Func<IParser<A>> leftFactory, IParser<B> right)
		{
			return new Both<A, B>(ToLazy(leftFactory), ToLazy(right));
		}
		/// <summary>
		/// Parses the input first with the <paramref name="left"/> parser and
		/// then with the resulting parser of the
		/// <paramref name="rightFactory"/>, eventually returning the result
		/// from the right parser.
		/// </summary>
		public static IParser<B> KeepRight<A, B>(IParser<A> left, Func<IParser<B>> rightFactory)
		{
			return new Both<A, B>(ToLazy(left), ToLazy(rightFactory));
		}
		/// <summary>
		/// Parses the input first with the resulting parser of the
		/// <paramref name="leftFactory"/> and then with the resulting parser
		/// of the <paramref name="rightFactory"/>, eventually returning the
		/// result from the right parser.
		/// </summary>
		public static IParser<B> KeepRight<A, B>(Func<IParser<A>> leftFactory, Func<IParser<B>> rightFactory)
		{
			return new Both<A, B>(ToLazy(leftFactory), ToLazy(rightFactory));
		}

		/// <summary>
		/// Tries to parse with the <paramref name="parser"/>, if that fails
		/// it will instead return the <see cref="default"/> value of
		/// <typeparamref name="T"/>.
		/// </summary>
		public static IParser<T> OneOrDefault<T>(IParser<T> parser)
		{
			return Either(parser, Success(default(T)));
		}
		/// <summary>
		/// Tries to parse with the resulting parser of the
		/// <paramref name="parserFactory"/>, if that fails it will instead
		/// return the <see cref="default"/> value of <typeparamref name="T"/>.
		/// </summary>
		public static IParser<T> OneOrDefault<T>(Func<IParser<T>> parserFactory)
		{
			return Either(parserFactory, Success(default(T)));
		}
		/// <summary>
		/// Tries to parse with the <paramref name="parser"/>, if that fails
		/// it will instead return the <paramref name="defaultValue"/>.
		/// </summary>
		public static IParser<T> OneOrDefault<T>(IParser<T> parser, T defaultValue)
		{
			return Either(parser, Success(defaultValue));
		}
		/// <summary>
		/// Tries to parse with the resulting parser of the
		/// <paramref name="parserFactory"/>, if that fails it will instead
		/// return the <paramref name="defaultValue"/>.
		/// </summary>
		public static IParser<T> OneOrDefault<T>(Func<IParser<T>> parserFactory, T defaultValue)
		{
			return Either(parserFactory, Success(defaultValue));
		}
		/// <summary>
		/// Tries to parse with the <paramref name="parser"/>, if that fails
		/// it will instead return the resulting value of the
		/// <paramref name="defaultValueFactory"/>.
		/// </summary>
		public static IParser<T> OneOrDefault<T>(IParser<T> parser, Func<T> defaultValueFactory)
		{
			return Either(parser, Success(defaultValueFactory));
		}
		/// <summary>
		/// Tries to parse with the resulting parser of the
		/// <paramref name="parserFactory"/>, if that fails it will instead
		/// return the resulting value of the
		/// <paramref name="defaultValueFactory"/>.
		/// </summary>
		public static IParser<T> OneOrDefault<T>(Func<IParser<T>> parserFactory, Func<T> defaultValueFactory)
		{
			return Either(parserFactory, Success(defaultValueFactory));
		}

		/// <summary>
		/// Parses first one thing using the <paramref name="one"/> parser, and
		/// then a sequence of things using the <paramref name="many"/> parser.
		/// Returns a sequence with the first thing followed by the sequence of
		/// many things.
		/// </summary>
		public static IParser<IEnumerable<T>> OnePlusMany<T>(IParser<T> one, IParser<IEnumerable<T>> many)
		{
			return new Using<T, IEnumerable<T>>(ToLazy(one), ToLazy<Func<T, IParser<IEnumerable<T>>>>(f => many.Select(x => x.Prepend(f))));
		}
		/// <summary>
		/// Parses first one thing using the resulting parser of the 
		/// <paramref name="oneFactory"/>, and then a sequence of things using
		/// the <paramref name="many"/> parser. Returns a sequence with the
		/// first thing followed by the sequence of many things.
		/// </summary>
		public static IParser<IEnumerable<T>> OnePlusMany<T>(Func<IParser<T>> oneFactory, IParser<IEnumerable<T>> many)
		{
			return new Using<T, IEnumerable<T>>(ToLazy(oneFactory), ToLazy<Func<T, IParser<IEnumerable<T>>>>(f => many.Select(x => x.Prepend(f))));
		}
		/// <summary>
		/// Parses first one thing using <paramref name="one"/> parser, and
		/// then a sequence of things using the resulting parser of the
		/// <paramref name="manyFactory"/>. Returns a sequence with the first
		/// thing followed by the sequence of things.
		/// </summary>
		public static IParser<IEnumerable<T>> OnePlusMany<T>(IParser<T> one, Func<IParser<IEnumerable<T>>> manyFactory)
		{
			return new Using<T, IEnumerable<T>>(ToLazy(one), ToLazy<Func<T, IParser<IEnumerable<T>>>>(f => manyFactory().Select(x => x.Prepend(f))));
		}
		/// <summary>
		/// Parses first one thing using the resulting parser of the 
		/// <paramref name="oneFactory"/>, and then a sequence of things using
		/// the resulting parser of the <paramref name="manyFactory"/>. Returns
		/// a sequence with the first thing followed by the sequence of things.
		/// </summary>
		public static IParser<IEnumerable<T>> OnePlusMany<T>(Func<IParser<T>> oneFactory, Func<IParser<IEnumerable<T>>> manyFactory)
		{
			return new Using<T, IEnumerable<T>>(ToLazy(oneFactory), ToLazy<Func<T, IParser<IEnumerable<T>>>>(f => manyFactory().Select(x => x.Prepend(f))));
		}

		/// <summary>
		/// Parses using the resulting parser of
		/// <paramref name="parserFactory"/> zeror or more times and returns a
		/// sequence of all parsed items.
		/// </summary>
		public static IParser<IEnumerable<T>> ZeroOrMore<T>(Func<IParser<T>> parserFactory)
		{
			return new Either<IEnumerable<T>>(ToLazy(OnceOrMore(parserFactory)), ToLazy(Success(Enumerable.Empty<T>())));
		}

		/// <summary>
		/// Parses using the resulting parser of
		/// <paramref name="parserFactory"/> one or more times and returns a
		/// sequence of all parsed items.
		/// </summary>
		public static IParser<IEnumerable<T>> OnceOrMore<T>(Func<IParser<T>> parserFactory)
		{
			return OnePlusMany(parserFactory, () => ZeroOrMore(parserFactory));
		}

		/// <summary>
		/// Parses once using <paramref name="parser"/>, followed by as many
		/// parses as possible with the joint of <paramref name="separator"/>
		/// and <paramref name="parser"/>. Returns a sequence with the results
		/// of the parsings with <paramref name="parser"/>.
		/// </summary>
		public static IParser<IEnumerable<T>> Chain<T, S>(IParser<T> parser, IParser<S> separator)
		{
			return OnePlusMany(parser, () => ZeroOrMore(KeepRight(separator, parser)));
		}
		/// <summary>
		/// Parses once using resulting parser of the
		/// <paramref name="parserFactory"/>, followed by as many parses as
		/// possible with the joint of <paramref name="separator"/> and
		/// the <paramref name="parserFactory"/>s parser. Returns a sequence
		/// with the results of the parsings with
		/// <paramref name="parserFactory"/>s parser.
		/// </summary>
		public static IParser<IEnumerable<T>> Chain<T, S>(Func<IParser<T>> parserFactory, IParser<S> separator)
		{
			return OnePlusMany(parserFactory, () => ZeroOrMore(KeepRight(separator, parserFactory)));
		}
		/// <summary>
		/// Parses once using <paramref name="parser"/>, followed by as many
		/// parses as possible with the joint of the resulting parser of the
		/// <paramref name="separatorFactory"/> and <paramref name="parser"/>.
		/// Returns a sequence with the results of the parsings with
		/// <paramref name="parser"/>.
		/// </summary>
		public static IParser<IEnumerable<T>> Chain<T, S>(IParser<T> parser, Func<IParser<S>> separatorFactory)
		{
			return OnePlusMany(parser, () => ZeroOrMore(KeepRight(separatorFactory, parser)));
		}
		/// <summary>
		/// Parses once using resulting parser of the
		/// <paramref name="parserFactory"/>, followed by as many parses as
		/// possible with the joint of the resulting parser of the
		/// <paramref name="separatorFactory"/> and the
		/// <paramref name="parserFactory"/>s parser. Returns a sequence with
		/// the results of the parsings with <paramref name="parserFactory"/>s
		/// parser.
		/// </summary>
		public static IParser<IEnumerable<T>> Chain<T, S>(Func<IParser<T>> parserFactory, Func<IParser<S>> separatorFactory)
		{
			return OnePlusMany(parserFactory, () => ZeroOrMore(KeepRight(separatorFactory, parserFactory)));
		}

		/// <summary>
		/// Returns a sequence from parsing the input in order with each parser once.
		/// If not all parsers succeeds this whole parsing fails.
		/// </summary>
		/// <param name="parsers">An ordered sequence of parsers.</param>
		public static IParser<IEnumerable<T>> All<T>(IEnumerable<IParser<T>> parsers) => All(parsers.ToArray());
		/// <summary>
		/// Returns a sequence from parsing the input in order with each parser once.
		/// If not all parsers succeeds this whole parsing fails.
		/// </summary>
		/// <param name="parsers">An ordered sequence of parsers.</param>
		public static IParser<IEnumerable<T>> All<T>(params IParser<T>[] parsers)
		{
			return parsers.Aggregate(Success(Enumerable.Empty<T>()), (all, p) => Using(all, ts => p.Select(t => ts.Append(t))));
		}
		/// <summary>
		/// Returns a sequence from parsing the input in order with each parser once.
		/// If not all parsers succeeds this whole parsing fails.
		/// </summary>
		/// <param name="parserFactories">An ordered sequence of parsers factories.</param>
		public static IParser<IEnumerable<T>> All<T>(IEnumerable<Func<IParser<T>>> parserFactories) => All(parserFactories.ToArray());
		/// <summary>
		/// Returns a sequence from parsing the input in order with each parser once.
		/// If not all parsers succeeds this whole parsing fails.
		/// </summary>
		/// <param name="parserFactories">An ordered sequence of parsers factories.</param>
		public static IParser<IEnumerable<T>> All<T>(params Func<IParser<T>>[] parserFactories)
		{
			return parserFactories.Aggregate(Success(Enumerable.Empty<T>()), (all, p) => Using(all, ts => p().Select(t => ts.Append(t))));
		}

		/// <summary>
		/// Tries all parsers, and uses the one that could parse the longest.
		/// </summary>
		/// <remarks>
		/// <b>Warning!</b> As this parser will try all provided parsers to determin
		/// the best, it is expensive and best avoided.
		/// </remarks>
		/// <param name="parsers">A collection of parsers.</param>
		public static IParser<T> Best<T>(IEnumerable<IParser<T>> parsers) => Best(parsers.ToArray());
		/// <summary>
		/// Tries all parsers, and uses the one that could parse the longest.
		/// </summary>
		/// <remarks>
		/// <b>Warning!</b> As this parser will try all provided parsers to determin
		/// the best, it is expensive and best avoided.
		/// </remarks>
		/// <param name="parsers">A collection of parsers.</param>
		public static IParser<T> Best<T>(params IParser<T>[] parsers)
		{
			return new Best<T>(parsers.Select(ToLazy));
		}
		/// <summary>
		/// Tries all parsers, and uses the one that could parse the longest.
		/// </summary>
		/// <remarks>
		/// <b>Warning!</b> As this parser will try all provided parsers to determin
		/// the best, it is expensive and best avoided.
		/// </remarks>
		/// <param name="parserFactories">A collection of parsers factories.</param>
		public static IParser<T> Best<T>(IEnumerable<Func<IParser<T>>> parserFactories) => Best(parserFactories.ToArray());
		/// <summary>
		/// Tries all parsers, and uses the one that could parse the longest.
		/// </summary>
		/// <remarks>
		/// <b>Warning!</b> As this parser will try all provided parsers to determin
		/// the best, it is expensive and best avoided.
		/// </remarks>
		/// <param name="parserFactories">A collection of parsers factories.</param>
		public static IParser<T> Best<T>(params Func<IParser<T>>[] parserFactories)
		{
			return new Best<T>(parserFactories.Select(ToLazy));
		}
		#endregion Combinatory
	}
}