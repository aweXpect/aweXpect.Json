using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using aweXpect.Results;

namespace aweXpect.Json;

internal class ExpectationJsonConverter : JsonConverter<Expectation>
{
	private const string Prefix = "Expectation:::";
	public override bool CanConvert(Type typeToConvert)
		=> typeof(Expectation).IsAssignableFrom(typeToConvert);

	private Dictionary<Guid, Expectation> _expectations = new();
	public override Expectation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var guid = Guid.Parse(reader.GetString()!);
		if (_expectations.TryGetValue(guid, out var expectation))
		{
			return expectation;
		}

		return default;
	}

	public override void Write(Utf8JsonWriter writer, Expectation value, JsonSerializerOptions options)
	{
		var guid = Guid.NewGuid();
		_expectations[guid] = value;
		writer.WriteStringValue(Prefix + guid.ToString());
	}

	public bool TryGetExpectation(JsonElement element, [NotNullWhen(true)] out Expectation? expectation)
	{
		if (element.ValueKind == JsonValueKind.String)
		{
			var value = element.GetString();
			if (value?.StartsWith(Prefix) == true &&
			    Guid.TryParse(value[Prefix.Length..], out var guid) &&
			    _expectations.TryGetValue(guid, out expectation))
			{
				return true;
			}
		}

		expectation = null;
		return false;
	}
}
