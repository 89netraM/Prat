namespace Prat
{
	class Failure<T> : IParser<T>
	{
		public (T, string)? Parse(string s) => null;
	}
}