using System;
using System.Collections.Generic;
using System.Linq;

namespace Prat
{
	public partial class Parsers
	{
		public static IParser<IBNFNode> FromBNF(string bnf, string mainRule)
		{
			switch (Syntax.Parse(bnf))
			{
				case (var rules, _):
					IDictionary<string, IEnumerable<IEnumerable<TermRule>>> ruleMap = rules.ToDictionary(p => p.name, p => p.expression);
					ruleMap["EOL"] = new[] { new[] { TermRule.Literal("\n") }, new[] { TermRule.Literal("\r\n") } };

					IParser<IBNFNode> ExpressionToParser(string ruleName, IEnumerable<IEnumerable<TermRule>> expression)
					{
						return Best(
							expression.Select(e => new Func<IParser<IEnumerable<IBNFNode>>>(() => ListToParser(e)))
						).Select(c => (IBNFNode)new RuleBNFNode(ruleName, c));
					}
					IParser<IEnumerable<IBNFNode>> ListToParser(IEnumerable<TermRule> list)
					{
						return All(list.Select(TermToParser));
					}
					IParser<IBNFNode> TermToParser(TermRule term)
					{
						if (term.IsRuleName)
						{
							return ExpressionToParser(term.Value, ruleMap[term.Value]);
						}
						else
						{
							return Common.String(term.Value).Select(s => (IBNFNode)new LiteralBNFNode(s));
						}
					}
					return ExpressionToParser(mainRule, ruleMap[mainRule]);
				default:
					return null;
			}
		}

		private static IParser<IEnumerable<char>> OptionalWhitespace { get; } = Sat(c => Char.IsWhiteSpace(c) && !(c == '\n' || c== '\r')).ZeroOrMore();

		private static IParser<string> RuleName { get; } = KeepLeft(
			OptionalWhitespace
				.And(Common.Char('<'))
				.And(
					Sat(c => Char.IsLetterOrDigit(c) || c == '-')
						.OnceOrMore()
				)
				.Select(cs => String.Concat(cs)),
			Common.Char('>')
				.And(OptionalWhitespace)
		);

		private static IParser<string> RuleDefinition { get; } = KeepLeft(RuleName, Common.String("::="));

		private static IParser<string> Literal { get; } = Common.Char('\'')
			.And(KeepLeft(
				Sat(c => c != '\'').ZeroOrMore(),
				Common.Char('\'')
			))
			.Or(
				Common.Char('"')
					.And(KeepLeft(
						Sat(c => c != '"').ZeroOrMore(),
						Common.Char('"')
					))
			)
			.Select(cs => String.Concat(cs));

		private static IParser<TermRule> Term { get; } = Literal.Select(TermRule.Literal).Or(RuleName.Select(TermRule.RuleName));

		private static IParser<IEnumerable<TermRule>> List { get; } = Chain(
			Term,
			OptionalWhitespace
		);

		private static IParser<IEnumerable<IEnumerable<TermRule>>> Expression { get; } = Chain(
			List,
			OptionalWhitespace
				.And(Common.Char('|'))
				.And(OptionalWhitespace)
		);

		private static IParser<(string name, IEnumerable<IEnumerable<TermRule>> expression)> Rule { get; } = Using(
			RuleDefinition,
			n => OptionalWhitespace
				.And(Expression)
				.Select(ts => (n, ts))
			);

		private static IParser<IEnumerable<(string name, IEnumerable<IEnumerable<TermRule>> expression)>> Syntax { get; } = Chain(
			Rule,
			OptionalWhitespace
				.And(
					Common.String("\n")
						.Or(Common.String("\r\n"))
				)
		);

		private readonly struct TermRule
		{
			public static TermRule Literal(string value) => new TermRule(false, value);
			public static TermRule RuleName(string name) => new TermRule(true, name);

			public bool IsRuleName { get; }
			public string Value { get; }

			private TermRule(bool isRuleName, string value)
			{
				IsRuleName = isRuleName;
				Value = value ?? throw new ArgumentNullException(nameof(value));
			}
		}
	}

	public interface IBNFNode
	{
		string Show();
	}
	public readonly struct RuleBNFNode : IBNFNode
	{
		public string Name { get; }
		public IEnumerable<IBNFNode> Children { get; }

		internal RuleBNFNode(string name, IEnumerable<IBNFNode> children)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Children = children ?? throw new ArgumentNullException(nameof(children));
		}

		public string Show() => String.Concat(Children.Select(n => n.Show()));

		public override string ToString()
		{
			return $"({Name}: [{String.Join(", ", Children)}])";
		}
	}
	public readonly struct LiteralBNFNode : IBNFNode
	{
		public string Value { get; }

		internal LiteralBNFNode(string value)
		{
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public string Show() => Value;

		public override string ToString()
		{
			return $"({Value})";
		}
	}
}