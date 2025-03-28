﻿using System.Text;
using System.Text.Json;
using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Json;

namespace aweXpect;

/// <summary>
///     Expectations on <see cref="JsonElement" />? values.
/// </summary>
public static partial class ThatNullableJsonElement
{
	private sealed class MatchesConstraint(
		string it,
		ExpectationGrammars grammars,
		object? expected,
		string expectedExpression,
		JsonOptions options)
		: ConstraintResult.WithValue<JsonElement?>(grammars),
			IValueConstraint<JsonElement?>
	{
		private JsonElementValidator.JsonComparisonResult? _comparisonResult;

		public ConstraintResult IsMetBy(JsonElement? actual)
		{
			Actual = actual;
			if (actual == null)
			{
				Outcome = Outcome.Failure;
				return this;
			}

			using JsonDocument expectedDocument =
				JsonDocument.Parse(JsonSerializer.Serialize(expected), options.DocumentOptions);

			_comparisonResult = JsonElementValidator.Compare(
				actual.Value,
				expectedDocument.RootElement,
				options);

			Outcome = _comparisonResult.HasError ? Outcome.Failure : Outcome.Success;
			return this;
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			stringBuilder.Append("matches ").Append(expectedExpression);
			if (!options.IgnoreAdditionalProperties)
			{
				stringBuilder.Append(" exactly");
			}
		}

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (Actual == null)
			{
				stringBuilder.Append(it).Append(" was <null>");
				return;
			}

			stringBuilder.Append(it).Append(" differed as").Append(_comparisonResult);
		}

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			stringBuilder.Append("does not match ").Append(expectedExpression);
			if (!options.IgnoreAdditionalProperties)
			{
				stringBuilder.Append(" exactly");
			}
		}

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
		{
			stringBuilder.Append(it).Append(" did match in ");
			Formatter.Format(stringBuilder, Actual);
		}
	}


	private sealed class IsValueKindConstraint(string it, ExpectationGrammars grammars, JsonValueKind expected)
		: ConstraintResult.WithNotNullValue<JsonElement?>(it, grammars),
			IValueConstraint<JsonElement?>
	{
		public ConstraintResult IsMetBy(JsonElement? actual)
		{
			Actual = actual;
			Outcome = actual is not null && actual.Value.ValueKind == expected ? Outcome.Success : Outcome.Failure;
			return this;
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("is ").Append(JsonValidation.Format(expected, Grammars));

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(It).Append(" was ").Append(JsonValidation.Format(Actual!.Value.ValueKind))
				.Append(" instead of ").Append(JsonValidation.Format(expected));

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append("is ").Append(JsonValidation.Format(expected, Grammars));

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(It).Append(" was");
	}
}
