using System;

namespace Prat
{
	class Both<A, B> : IParser<B>
	{
		private readonly Lazy<IParser<A>> Left;
		private readonly Lazy<IParser<B>> Right;

		internal Both(Lazy<IParser<A>> left, Lazy<IParser<B>> right)
		{
			Left = left ?? throw new ArgumentNullException(nameof(left));
			Right = right ?? throw new ArgumentNullException(nameof(right));
		}

		public (B, ReadOnlyMemory<char>)? Parse(ReadOnlyMemory<char> s) => Left.Value.Parse(s) switch
		{
			(_, ReadOnlyMemory<char> rest) => Right.Value.Parse(rest),
			_ => null
		};
	}
}