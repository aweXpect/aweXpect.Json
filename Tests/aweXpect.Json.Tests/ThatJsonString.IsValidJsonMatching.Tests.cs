﻿using aweXpect.Equivalency;

namespace aweXpect.Json.Tests;

public sealed partial class ThatJsonString
{
	public sealed class IsValidJsonMatching
	{
		public sealed class Tests
		{
			[Theory]
			[InlineData("true", true, true)]
			[InlineData("true", false, false)]
			[InlineData("false", true, false)]
			[InlineData("false", false, true)]
			public async Task BooleanValue_ShouldSucceedWhenMatching(string subject, bool expected, bool isMatch)
			{
				string subjectString = subject == "true" ? "True" : "False";

				async Task Act()
					=> await That(subject).IsValidJsonMatching(expected);

				await That(Act).Throws<XunitException>().OnlyIf(!isMatch)
					.WithMessage($"""
					              Expected that subject
					              is valid JSON which matches expected,
					              but it differed as $ was {subjectString} instead of {expected}
					              """);
			}

			[Theory]
			[InlineData("42.1", 42.1, true)]
			[InlineData("1.2", 2.1, false)]
			public async Task DoubleValue_ShouldSucceedWhenMatching(string subject, double expected, bool isMatch)
			{
				async Task Act()
					=> await That(subject).IsValidJsonMatching(expected);

				await That(Act).Throws<XunitException>().OnlyIf(!isMatch)
					.WithMessage($"""
					              Expected that subject
					              is valid JSON which matches expected,
					              but it differed as $ was {subject} instead of *
					              """).AsWildcard();
			}

			[Theory]
			[InlineData("42", 42, true)]
			[InlineData("1", 2, false)]
			public async Task IntegerValue_ShouldSucceedWhenMatching(string subject, int expected, bool isMatch)
			{
				async Task Act()
					=> await That(subject).IsValidJsonMatching(expected);

				await That(Act).Throws<XunitException>().OnlyIf(!isMatch)
					.WithMessage($"""
					              Expected that subject
					              is valid JSON which matches expected,
					              but it differed as $ was {subject} instead of {expected}
					              """);
			}

			[Theory]
			[InlineData("null", true)]
			[InlineData("{}", false)]
			public async Task NullValue_ShouldSucceedWhenMatching(string subject, bool isMatch)
			{
				async Task Act()
					=> await That(subject).IsValidJsonMatching(null);

				await That(Act).Throws<XunitException>().OnlyIf(!isMatch)
					.WithMessage($"""
					              Expected that subject
					              is valid JSON which matches null,
					              but it differed as $ was object {subject} instead of Null
					              """);
			}

			[Fact]
			public async Task ShouldSupportNestedObjects()
			{
				string subject = "[{\"foo\": 2}, {\"foo\": 3}, {\"foo\": 4, \"bar\": 2}]";

				await That(subject).IsValidJsonMatching([
					new
					{
						foo = 2,
					},
					new
					{
						foo = 3,
					},
					new
					{
						foo = 4,
					},
				]);
			}

			[Theory]
			[InlineData("\"foo\"", "foo", true)]
			[InlineData("\"foo\"", "bar", false)]
			public async Task StringValue_ShouldSucceedWhenMatching(string subject, string expected, bool isMatch)
			{
				async Task Act()
					=> await That(subject).IsValidJsonMatching(expected, o => o.IgnoringAdditionalProperties());

				await That(Act).Throws<XunitException>().OnlyIf(!isMatch)
					.WithMessage($"""
					              Expected that subject
					              is valid JSON which matches expected,
					              but it differed as $ was {subject} instead of "{expected}"
					              """);
			}

			[Fact]
			public async Task WhenSubjectIsNull_ShouldFail()
			{
				string? subject = null;

				async Task Act()
					=> await That(subject).IsValidJsonMatching(null);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is valid JSON which matches null,
					             but it was <null>
					             """);
			}

			[Fact]
			public async Task WhenSubjectIsNullJson_WhenMatchingAgainstNull_ShouldSucceed()
			{
				string? subject = "null";

				async Task Act()
					=> await That(subject).IsValidJsonMatching(null);

				await That(Act).DoesNotThrow();
			}
		}

		public sealed class ArrayTests
		{
			[Theory]
			[MemberData(nameof(MatchingArrayValues))]
			public async Task MatchingValues_ShouldSucceed(string[] expected, string subject)
			{
				async Task Act()
					=> await That(subject).IsValidJsonMatching(expected);

				await That(Act).DoesNotThrow();
			}

			[Theory]
			[MemberData(nameof(NotMatchingArrayValues))]
			public async Task NotMatchingValues_ShouldFail(string[] expected, string subject, string errorMessage)
			{
				async Task Act()
					=> await That(subject).IsValidJsonMatching(expected);

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              is valid JSON which matches expected,
					              but it differed {errorMessage}
					              """);
			}

			[Fact]
			public async Task WhenElementsAreInDifferentOrder_ShouldFail()
			{
				string subject = "[1, 2]";

				async Task Act()
					=> await That(subject).IsValidJsonMatching([2, 1,]);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is valid JSON which matches [2, 1,],
					             but it differed as
					               $[0] was 1 instead of 2 and
					               $[1] was 2 instead of 1
					             """);
			}

			[Fact]
			public async Task WhenExpectedContainsAdditionalElements_ShouldFail()
			{
				string subject = "[1, 2]";

				async Task Act()
					=> await That(subject).IsValidJsonMatching([1, 2, 3,]);

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is valid JSON which matches [1, 2, 3,],
					             but it differed as $[2] had missing 3
					             """);
			}

			[Fact]
			public async Task WhenSubjectContainsAdditionalElements_ShouldSucceed()
			{
				string subject = "[1, 2, 3]";

				async Task Act()
					=> await That(subject).IsValidJsonMatching([1, 2,], o => o.IgnoringAdditionalProperties());

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenSubjectContainsAdditionalElements_WhenNotIgnoringAdditionalProperties_ShouldFail()
			{
				string subject = "[1, 2, 3]";

				async Task Act()
					=> await That(subject).IsValidJsonMatching([1, 2,], o => o.IgnoringAdditionalProperties(false));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is valid JSON which matches [1, 2,] exactly,
					             but it differed as $[2] had unexpected 3
					             """);
			}

			public static TheoryData<string[], string> MatchingArrayValues
				=> new()
				{
					{
						[], "[]"
					},
					{
						[], "[\"foo\"]"
					},
					{
						[
							"foo", "bar",
						],
						"[\"foo\", \"bar\"]"
					},
				};

			public static TheoryData<string[], string, string> NotMatchingArrayValues
				=> new()
				{
					{
						[
							"foo",
						],
						"[]", "as $[0] had missing \"foo\""
					},
					{
						[
							"bar", "foo",
						],
						"[\"foo\", \"bar\"]", """
						                      as
						                        $[0] was "foo" instead of "bar" and
						                        $[1] was "bar" instead of "foo"
						                      """
					},
				};
		}

		public sealed class ObjectTests
		{
			[Theory]
			[InlineData("{}", "$.foo was missing")]
			[InlineData("{\"foo\": 2}")]
			public async Task ShouldFailIfPropertyIsMissing(string subject,
				string? errorMessage = null)
			{
				async Task Act()
					=> await That(subject).IsValidJsonMatching(new
					{
						foo = 2,
					});

				await That(Act).Throws<XunitException>().OnlyIf(errorMessage != null)
					.WithMessage($$"""
					               Expected that subject
					               is valid JSON which matches new
					               					{
					               						foo = 2,
					               					},
					               but it differed as {{errorMessage}}
					               """);
			}

			[Theory]
			[InlineData("{}")]
			[InlineData("{\"foo\": 1}")]
			public async Task WhenExpectedIsEmpty_ShouldSucceed(string subject)
			{
				async Task Act()
					=> await That(subject).IsValidJsonMatching(new object());

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenPropertyDoesNotMatchItIs_ShouldFail()
			{
				string subject = "{\"bar\": 2}";

				async Task Act()
					=> await That(subject).IsValidJsonMatching(new
					{
						bar = It.Is<int>().That.IsGreaterThan(3),
					});

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is valid JSON which matches new
					             					{
					             						bar = It.Is<int>().That.IsGreaterThan(3),
					             					},
					             but it differed as $.bar was 2
					             """);
			}

			[Fact]
			public async Task WhenPropertyHasDifferentValue_ShouldFail()
			{
				string subject = "{\"bar\": 2}";

				async Task Act()
					=> await That(subject).IsValidJsonMatching(new
					{
						bar = 3,
					});

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is valid JSON which matches new
					             					{
					             						bar = 3,
					             					},
					             but it differed as $.bar was 2 instead of 3
					             """);
			}

			[Fact]
			public async Task WhenPropertyMatchesItIs_ShouldSucceed()
			{
				string subject = """
				                 {
				                   "foo": "xyz",
				                   "bar": 2,
				                   "baz1": true,
				                   "baz2": false,
				                   "empty": null
				                 }
				                 """;

				async Task Act()
					=> await That(subject).IsValidJsonMatching(new
					{
						foo = It.Is<string>().That.EndsWith("yz"),
						bar = It.Is<int>().That.IsGreaterThan(1),
						baz1 = It.Is<bool>().That.IsTrue(),
						baz2 = It.Is<bool>().That.IsFalse(),
						empty = It.Is<object?>().That.IsNull(),
					});

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenSubjectHasAdditionalProperties_ShouldSucceed()
			{
				string subject = "{\"foo\": null, \"bar\": 2}";

				async Task Act()
					=> await That(subject).IsValidJsonMatching(new
					{
						bar = 2,
					}, o => o.IgnoringAdditionalProperties());

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenSubjectHasAdditionalProperties_WhenNotIgnoringAdditionalProperties_ShouldFail()
			{
				string subject = "{\"foo\": null, \"bar\": 2}";

				async Task Act()
					=> await That(subject).IsValidJsonMatching(new
					{
						bar = 2,
					}, o => o.IgnoringAdditionalProperties(false));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is valid JSON which matches new
					             					{
					             						bar = 2,
					             					} exactly,
					             but it differed as $.foo had unexpected Null
					             """);
			}
		}
	}
}
