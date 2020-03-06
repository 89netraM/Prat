using System;

namespace Prat
{
	class Using<A, B> : IParser<B>
	{
		private readonly Lazy<IParser<A>> First;
		private readonly Lazy<Func<A, IParser<B>>> SecondFactory;

		internal Using(Lazy<IParser<A>> first, Lazy<Func<A, IParser<B>>> secondFactory)
		{
			First = first ?? throw new ArgumentNullException(nameof(first));
			SecondFactory = secondFactory ?? throw new ArgumentNullException(nameof(secondFactory));
		}

		public (B, string)? Parse(string s) => First.Value.Parse(s) switch
		{
			(A v, string rest) => SecondFactory.Value(v).Parse(rest),
			_ => null
		};
	}
}