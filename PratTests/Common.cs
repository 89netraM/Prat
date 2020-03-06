using NUnit.Framework;

namespace Prat.Tests
{
	public class CommonTests
	{
		[Test]
		public void CharTests()
		{
			switch (Parsers.Common.Char('a').Parse("abcd"))
			{
				case (char read, string rest):
					Assert.AreEqual('a', read);
					Assert.AreEqual("bcd", rest);
					break;
				default:
					Assert.Fail("Should be able to read 'a' from \"abcd\".");
					break;
			}
		}

		[Test]
		public void IntegerTest()
		{
			switch (Parsers.Common.Integer().Parse("123"))
			{
				case (int read, string rest):
					Assert.AreEqual(123, read);
					Assert.AreEqual("", rest);
					break;
				default:
					Assert.Fail("Should be able to read 123 from \"123\".");
					break;
			}

			switch (Parsers.Common.Integer().Parse("-123abc"))
			{
				case (int read, string rest):
					Assert.AreEqual(-123, read);
					Assert.AreEqual("abc", rest);
					break;
				default:
					Assert.Fail("Should be able to read -123 from \"-123abc\".");
					break;
			}
		}

		[Test]
		public void DoubleTest()
		{
			switch (Parsers.Common.Double().Parse("123"))
			{
				case (double read, string rest):
					Assert.AreEqual(123d, read);
					Assert.AreEqual("", rest);
					break;
				default:
					Assert.Fail("Should be able to read an integer (123) as a double from \"123\".");
					break;
			}

			switch (Parsers.Common.Double().Parse("123.456"))
			{
				case (double read, string rest):
					Assert.AreEqual(123.456d, read);
					Assert.AreEqual("", rest);
					break;
				default:
					Assert.Fail("Should be able to read -123.456 from \"-123.456abc\".");
					break;
			}

			switch (Parsers.Common.Double().Parse("-123.456abc"))
			{
				case (double read, string rest):
					Assert.AreEqual(-123.456, read);
					Assert.AreEqual("abc", rest);
					break;
				default:
					Assert.Fail("Should be able to read -123.456 from \"-123.456abc\".");
					break;
			}
		}

		[Test]
		public void BoolTest()
		{
			switch (Parsers.Common.Bool().Parse("false"))
			{
				case (bool read, string rest):
					Assert.AreEqual(false, read);
					Assert.AreEqual("", rest);
					break;
				default:
					Assert.Fail("Should be able to read false from \"false\".");
					break;
			}

			switch (Parsers.Common.Bool().Parse("trueabc"))
			{
				case (bool read, string rest):
					Assert.AreEqual(true, read);
					Assert.AreEqual("abc", rest);
					break;
				default:
					Assert.Fail("Should be able to read true from \"trueabc\".");
					break;
			}
		}
	}
}