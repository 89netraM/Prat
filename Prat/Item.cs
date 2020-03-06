namespace Prat
{
	class Item : IParser<char>
	{
		public (char, string)? Parse(string s)
		{
			if (s.Length == 0)
			{
				return null;
			}
			else
			{
				return (s[0], s.Substring(1));
			}
		}
	}
}