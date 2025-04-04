﻿using System.Text.Json;

namespace aweXpect.Json.Tests;

public sealed partial class ThatJsonElement
{
	public sealed class IsArray
	{
		public sealed class Tests
		{
			[Theory]
			[InlineData("[]")]
			[InlineData("[1, 2]")]
			public async Task WhenJsonIsAnArray_ShouldSucceed(string json)
			{
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).IsArray();

				await That(Act).DoesNotThrow();
			}

			[Theory]
			[InlineData("{}", "an object")]
			[InlineData("2", "a number")]
			[InlineData("\"foo\"", "a string")]
			public async Task WhenJsonIsNoArray_ShouldFail(string json, string kindString)
			{
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).IsArray();

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              is an array,
					              but it was {kindString} instead of an array
					              """);
			}
		}

		public sealed class NegatedTests
		{
			[Fact]
			public async Task IsArray_ShouldBeChainable()
			{
				string json = "[1, 2, 3]";
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it
						=> it.IsArray(o => o.At(0).Matching(1)).And.IsArray(o => o.At(2).Matching(3)));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is no array or $[0] does not match 1 or is no array or $[2] does not match 3,
					             but it was in [
					               1,
					               2,
					               3
					             ]
					             """);
			}

			[Theory]
			[InlineData("[]")]
			[InlineData("[1, 2]")]
			public async Task WhenJsonIsAnArray_ShouldFail(string json)
			{
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.IsArray());

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is no array,
					             but it was
					             """);
			}

			[Theory]
			[InlineData("{}")]
			[InlineData("2")]
			[InlineData("\"foo\"")]
			public async Task WhenJsonIsNoArray_ShouldSucceed(string json)
			{
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).DoesNotComplyWith(it => it.IsArray());

				await That(Act).DoesNotThrow();
			}
		}

		public sealed class ExpectationTests
		{
			[Fact]
			public async Task IsArray_ShouldBeChainable()
			{
				string json = "[1, 2, 3]";
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).IsArray(o => o.At(0).Matching(1)).And.IsArray(o => o.At(2).Matching(2));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an array and $[0] matches 1 and is an array and $[2] matches 2,
					             but it differed as $[2] was 3 instead of 2
					             """);
			}

			[Fact]
			public async Task IsArray_ShouldSupportExpectationsOnElements()
			{
				string json = "[[], [1], {}, {\"foo\": {}}]";
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).IsArray(elements => elements
						.At(0).AnArray().And
						.At(1).AnArray(a => a.With(1).Elements()).And
						.At(2).AnObject().And
						.At(3).AnObject(o => o.With("foo").AnObject()));

				await That(Act).DoesNotThrow();
			}

			[Fact]
			public async Task WhenIndexDoesNotExist_ShouldFail()
			{
				string json = "[1, 2, 3]";
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).IsArray(o => o.At(3).Matching(3));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an array and $[3] matches 3,
					             but it differed as index $[3] did not exist
					             """);
			}

			[Theory]
			[InlineData("{\"foo\":{}}", "an object")]
			[InlineData("{\"foo\":2}", "a number")]
			[InlineData("{\"foo\":\"foo\"}", "a string")]
			[InlineData("{\"foo\":true}", "true")]
			[InlineData("{\"foo\":false}", "false")]
			[InlineData("{\"foo\":null}", "null")]
			public async Task WhenInnerJsonIsNoArray_AndExpectElementAt_ShouldFail(string json, string kindString)
			{
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).IsObject(o => o.With("foo").AnArray(a => a.At(0).Matching(null)));

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              is an object and $.foo is an array and $.foo[0] matches null,
					              but it differed as $.foo was {kindString} instead of an array
					              """);
			}

			[Theory]
			[InlineData("{\"foo\":{}}", "an object")]
			[InlineData("{\"foo\":2}", "a number")]
			[InlineData("{\"foo\":\"foo\"}", "a string")]
			public async Task WhenInnerJsonIsNoArray_AndExpectElementCount_ShouldFail(string json, string kindString)
			{
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).IsObject(o => o.With("foo").AnArray(a => a.With(0).Elements()));

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              is an object and $.foo is an array with 0 elements,
					              but it differed as $.foo was {kindString} instead of an array
					              """);
			}

			[Theory]
			[InlineData("{\"foo\":{}}", "an object")]
			[InlineData("{\"foo\":2}", "a number")]
			[InlineData("{\"foo\":\"foo\"}", "a string")]
			public async Task WhenInnerJsonIsNoArray_ShouldFail(string json, string kindString)
			{
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).IsObject(o => o.With("foo").AnArray());

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              is an object and $.foo is an array,
					              but it differed as $.foo was {kindString} instead of an array
					              """);
			}

			[Theory]
			[InlineData("{}", "an object")]
			[InlineData("2", "a number")]
			[InlineData("\"foo\"", "a string")]
			public async Task WhenJsonIsNoArray_WithExpectations_ShouldFail(string json, string kindString)
			{
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).IsArray(o => o.At(0).AnArray());

				await That(Act).Throws<XunitException>()
					.WithMessage($"""
					              Expected that subject
					              is an array and $[0] is an array,
					              but it was {kindString} instead of an array
					              """);
			}

			[Theory]
			[InlineData(true)]
			[InlineData(false)]
			public async Task WithExpectations_ShouldConsiderIgnoreAdditionalProperties(
				bool ignoreAdditionalProperties)
			{
				string json = "[{\"foo\": 1, \"bar\": 2}, {\"foo\": 2, \"bar\": 4}]";
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).IsArray(o => o.At(0).Matching(new
					{
						foo = 1,
					}), o => o with
					{
						IgnoreAdditionalProperties = ignoreAdditionalProperties,
					});

				await That(Act).Throws<XunitException>().OnlyIf(!ignoreAdditionalProperties)
					.WithMessage("""
					             Expected that subject
					             is an array and $[0] matches new
					             					{
					             						foo = 1,
					             					},
					             but it differed as $[0].bar had unexpected 2
					             """);
			}
		}

		public sealed class WithNumberOfElementsTests
		{
			[Fact]
			public async Task WhenNull_ShouldNotAddFailureMessage()
			{
				JsonElement subject = FromString("{}");

				async Task Act()
					=> await That(subject).IsObject(o => o
						.With("foo").AnArray(a => a
							.With(1).Elements()));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an object and $.foo is an array with 1 element,
					             but it differed as property $.foo did not exist
					             """);
			}

			[Fact]
			public async Task WhenNumberDiffers_ShouldFail()
			{
				JsonElement subject = FromString("[1, 2]");

				async Task Act()
					=> await That(subject).IsArray(o => o
						.With(3).Elements());

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an array with 3 elements,
					             but it differed as $ had 2 elements
					             """);
			}

			[Theory]
			[InlineData("[]", 0)]
			[InlineData("[\"foo\"]", 1)]
			[InlineData("[1, 2, 3]", 3)]
			public async Task WhenNumberMatches_ShouldSucceed(string json, int expected)
			{
				JsonElement subject = FromString(json);

				async Task Act()
					=> await That(subject).IsArray(o => o
						.With(expected).Elements());

				await That(Act).DoesNotThrow();
			}
		}

		public sealed class WithArraysTests
		{
			[Fact]
			public async Task WhenArrayIsNull_ShouldNotAddFailureMessage()
			{
				JsonElement subject = FromString("{}");

				async Task Act()
					=> await That(subject).IsObject(o => o
						.With("foo").AnArray(a => a
							.WithArrays(p => p.With(0).Elements())));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an object and $.foo is an array and $.foo[0] is an array with 0 elements,
					             but it differed as property $.foo did not exist
					             """);
			}

			[Fact]
			public async Task WhenElementsArNoArrays_ShouldFail()
			{
				JsonElement subject = FromString("[{},{}]");

				async Task Act()
					=> await That(subject).IsArray(a => a
						.WithArrays(p => p.With(2).Elements()));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an array and $[0] is an array with 2 elements,
					             but it differed as $[0] was an object instead of an array
					             """);
			}

			[Fact]
			public async Task WhenExpectationIsNull_ShouldSkipIndex()
			{
				JsonElement subject = FromString("[[1],[2],[3]]");

				async Task Act()
					=> await That(subject).IsArray(e => e
						.WithArrays(o => o.At(0).Matching(1), null, o => o.At(0).Matching(2)));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an array and $[0] is an array and $[0][0] matches 1 and $[2] is an array and $[2][0] matches 2,
					             but it differed as $[2][0] was 3 instead of 2
					             """);
			}

			[Fact]
			public async Task WhenExpectationsMatchInDifferentOrder_ShouldFail()
			{
				JsonElement subject = FromString("""
				                                 [
				                                   [1],
				                                   [3],
				                                   [2],
				                                   [4]
				                                 ]
				                                 """);

				async Task Act()
					=> await That(subject).IsArray(e => e
						.WithArrays(
							o => o.At(0).Matching(1),
							o => o.At(0).Matching(2),
							o => o.At(0).Matching(3)));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an array and $[0] is an array and $[0][0] matches 1 and $[1] is an array and $[1][0] matches 2 and $[2] is an array and $[2][0] matches 3,
					             but it differed as
					               $[1][0] was 3 instead of 2 and
					               $[2][0] was 2 instead of 3
					             """);
			}

			[Fact]
			public async Task WhenExpectationsMatchInOrder_ShouldSucceed()
			{
				JsonElement subject = FromString("""
				                                 [
				                                   [1],
				                                   [2],
				                                   [3],
				                                   [4]
				                                 ]
				                                 """);

				async Task Act()
					=> await That(subject).IsArray(e => e
						.WithArrays(
							o => o.At(0).Matching(1),
							o => o.At(0).Matching(2),
							o => o.At(0).Matching(3)));

				await That(Act).DoesNotThrow();
			}
		}

		public sealed class WithElementsTests
		{
			[Fact]
			public async Task WhenArrayIsNull_ShouldNotAddFailureMessage()
			{
				JsonElement subject = FromString("{}");

				async Task Act()
					=> await That(subject).IsObject(o => o
						.With("foo").AnArray(a => a
							.WithElements(2, 3)));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an object and $.foo is an array and $.foo[0] matches 2 and $.foo[1] matches 3,
					             but it differed as property $.foo did not exist
					             """);
			}

			[Fact]
			public async Task WhenExpectationsMatchInDifferentOrder_ShouldFail()
			{
				JsonElement subject = FromString("""
				                                 [
				                                   3, null, false, 1.2, false, "bar"
				                                 ]
				                                 """);

				async Task Act()
					=> await That(subject).IsArray(e => e
						.WithElements(3, false, null, 1.2, false, "bar"));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an array and $[0] matches 3 and $[1] matches False and $[2] matches Null and $[3] matches 1.2 and $[4] matches False and $[5] matches "bar",
					             but it differed as
					               $[1] was Null instead of False and
					               $[2] was boolean False instead of Null
					             """);
			}

			[Fact]
			public async Task WhenExpectationsMatchInOrder_ShouldSucceed()
			{
				JsonElement subject = FromString("""
				                                 [
				                                   3, true, null, 1.2, false, "bar"
				                                 ]
				                                 """);

				async Task Act()
					=> await That(subject).IsArray(e => e
						.WithElements(3, true, null, 1.2, false, "bar"));

				await That(Act).DoesNotThrow();
			}
		}

		public sealed class WithObjectsTests
		{
			[Fact]
			public async Task WhenArrayIsNull_ShouldNotAddFailureMessage()
			{
				JsonElement subject = FromString("{}");

				async Task Act()
					=> await That(subject).IsObject(o => o
						.With("foo").AnArray(a => a
							.WithObjects(p => p.With(0).Properties())));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an object and $.foo is an array and $.foo[0] is an object with 0 properties,
					             but it differed as property $.foo did not exist
					             """);
			}

			[Fact]
			public async Task WhenElementsArNoObjects_ShouldFail()
			{
				JsonElement subject = FromString("[ [], [] ]");

				async Task Act()
					=> await That(subject).IsArray(a => a
						.WithObjects(p => p.With("foo").Matching(2)));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an array and $[0] is an object and $[0].foo matches 2,
					             but it differed as $[0] was an array instead of an object
					             """);
			}

			[Fact]
			public async Task WhenExpectationIsNull_ShouldSkipIndex()
			{
				JsonElement subject = FromString("[{},{},{\"foo\":true}]");

				async Task Act()
					=> await That(subject).IsArray(e => e
						.WithObjects(o => o.With(0).Properties(), null, o => o.With(0).Properties()));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an array and $[0] is an object with 0 properties and $[2] is an object with 0 properties,
					             but it differed as $[2] had 1 property
					             """);
			}

			[Fact]
			public async Task WhenExpectationsMatchInDifferentOrder_ShouldFail()
			{
				JsonElement subject = FromString("""
				                                 [
				                                   {"foo":1},
				                                   {"foo":3},
				                                   {"bar":2},
				                                   {"baz":4}
				                                 ]
				                                 """);

				async Task Act()
					=> await That(subject).IsArray(e => e
						.WithObjects(
							o => o.With("foo").Matching(1),
							o => o.With("bar").Matching(2),
							o => o.With("foo").Matching(3)));

				await That(Act).Throws<XunitException>()
					.WithMessage("""
					             Expected that subject
					             is an array and $[0] is an object and $[0].foo matches 1 and $[1] is an object and $[1].bar matches 2 and $[2] is an object and $[2].foo matches 3,
					             but it differed as
					               property $[1].bar did not exist and
					               property $[2].foo did not exist
					             """);
			}

			[Fact]
			public async Task WhenExpectationsMatchInOrder_ShouldSucceed()
			{
				JsonElement subject = FromString("""
				                                 [
				                                   {"foo":1},
				                                   {"bar":2},
				                                   {"foo":3},
				                                   {"baz":4}
				                                 ]
				                                 """);

				async Task Act()
					=> await That(subject).IsArray(e => e
						.WithObjects(
							o => o.With("foo").Matching(1),
							o => o.With("bar").Matching(2),
							o => o.With("foo").Matching(3)));

				await That(Act).DoesNotThrow();
			}
		}
	}
}
