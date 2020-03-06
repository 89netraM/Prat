using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Prat
{
	public static partial class Parsers
	{
		/// <summary>
		/// Provides common ready-made <see cref="IParser{T}"/>s.
		/// </summary>
		public static class Common
		{
			/// <summary>
			/// Reads and returns the first <see cref="char"/> if it is equal to <paramref name="c"/>.
			/// Otherwise fails without reading anything.
			/// </summary>
			public static IParser<char> Char(char c) => Sat(c.Equals);

			/// <summary>
			/// Reads the whole string <paramref name="s"/> or nothing.
			/// </summary>
			public static IParser<string> String(string s)
			{
				return All(s.Select(Char)).Select(System.String.Concat);
			}

			private static IParser<IEnumerable<char>> Number()
			{
				return Sat(System.Char.IsDigit).OnceOrMore();
			}

			private static IParser<IEnumerable<char>> DecimalNumber()
			{
				return All(
					Number(),
					Char('.').Select(x => Enumerable.Repeat(x, 1)),
					Number().OrDefault(Enumerable.Empty<char>())
				).Select(x => x.SelectMany(x => x));
			}
			private static IParser<IEnumerable<char>> Negative(IParser<IEnumerable<char>> after)
			{
				return Char('-').Or(Char('+')).OrDefault('+').PlusMany(() => after);
			}

			/// <summary>
			/// Reads one integer that can be negative.
			/// </summary>
			/// <remarks>
			/// Can only read these formats:
			/// <list type="bullet">
			/// <item>1234567890</item>
			/// <item>+1234567890</item>
			/// <item>-1234567890</item>
			/// </list>
			/// </remarks>
			public static IParser<int> Integer()
			{
				return Negative(Number())
					.Select(cs => Convert.ToInt32(System.String.Concat(cs)));
			}

			/// <summary>
			/// Reads one double that can be negative.
			/// </summary>
			/// <remarks>
			/// Can only read these formats:
			/// <list type="bullet">
			/// <item>1234567890</item>
			/// <item>1234567890.0987654321</item>
			/// <item>+1234567890.0987654321</item>
			/// <item>-1234567890.0987654321</item>
			/// </list>
			/// </remarks>
			public static IParser<double> Double()
			{
				return Negative(DecimalNumber().Or(Number()))
					.Select(cs => Convert.ToDouble(System.String.Concat(cs), CultureInfo.InvariantCulture));
			}

			/// <summary>
			/// Reads either "false" or "true" and returns it's boolean value.
			/// </summary>
			public static IParser<bool> Bool()
			{
				return String("false").Or(String("true"))
					.Select(cs => Convert.ToBoolean(System.String.Concat(cs)));
			}
		}
	}
}