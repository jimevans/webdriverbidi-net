namespace WebDriverBidi.Script;

using System.Numerics;
using Newtonsoft.Json;

[TestFixture]
public class RemoteValueTests
{
    [Test]
    public void TestCanDeserializeStringRemoteValue()
    {
        string json = @"{ ""type"": ""string"", ""value"": ""myValue"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("string"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<string>());
        Assert.That(remoteValue.ValueAs<string>(), Is.EqualTo("myValue"));
    }

    [Test]
    public void TestDeserializingInvalidStringRemoteValueThrows()
    {
        string json = @"{ ""type"": ""string"", ""value"": 7 }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("'value' property for string must be a non-null string"));
    }

    [Test]
    public void TestCanDeserializeIntegerRemoteValue()
    {
        string json = @"{ ""type"": ""number"", ""value"": 1 }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("number"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<long>());
        Assert.That(remoteValue.ValueAs<long>(), Is.EqualTo(1));
    }

    [Test]
    public void TestCanDeserializeFloatRemoteValue()
    {
        string json = @"{ ""type"": ""number"", ""value"": 3.14 }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("number"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<double>());
        Assert.That(remoteValue.ValueAs<double>(), Is.EqualTo(3.14));
    }

    [Test]
    public void TestCanDeserializeNaNRemoteValue()
    {
        string json = @"{ ""type"": ""number"", ""value"": ""NaN"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("number"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<double>());
        Assert.That(remoteValue.ValueAs<double>(), Is.EqualTo(double.NaN));
    }

    [Test]
    public void TestCanDeserializeNegativeZeroRemoteValue()
    {
        string json = @"{ ""type"": ""number"", ""value"": ""-0"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("number"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<decimal>());
        Assert.That(remoteValue.ValueAs<decimal>(), Is.EqualTo(decimal.Negate(decimal.Zero)));
    }

    [Test]
    public void TestCanDeserializeInfinityRemoteValue()
    {
        string json = @"{ ""type"": ""number"", ""value"": ""Infinity"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("number"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<double>());
        Assert.That(remoteValue.ValueAs<double>(), Is.EqualTo(double.PositiveInfinity));
    }

    [Test]
    public void TestCanDeserializeNegativeInfinityRemoteValue()
    {
        string json = @"{ ""type"": ""number"", ""value"": ""-Infinity"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("number"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<double>());
        Assert.That(remoteValue.ValueAs<double>(), Is.EqualTo(double.NegativeInfinity));
    }
    
    [Test]
    public void TestDeserializingInvalidSpecialNumericRemoteValueThrows()
    {
        string json = @"{ ""type"": ""number"", ""value"": ""invalid"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("invalid value 'invalid' for 'value' property of number"));
    }
    
    [Test]
    public void TestDeserializingInvalidNumericRemoteValueThrows()
    {
        string json = @"{ ""type"": ""number"", ""value"": true }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("invalid type Boolean for 'value' property of number"));
    }

    [Test]
    public void TestCanDeserializeBooleanRemoteValue()
    {
        string json = @"{ ""type"": ""boolean"", ""value"": true }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("boolean"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<bool>());
        Assert.That(remoteValue.ValueAs<bool>(), Is.EqualTo(true));
    }

    [Test]
    public void TestDeserializingInvalidBooleanRemoteValueThrows()
    {
        string json = @"{ ""type"": ""boolean"", ""value"": ""hello"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("'value' property for boolean must be a boolean value"));
    }

    [Test]
    public void TestCanDeserializeBigIntRemoteValue()
    {
        string json = @"{ ""type"": ""bigint"", ""value"": ""123"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("bigint"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<BigInteger>());
        Assert.That(remoteValue.ValueAs<BigInteger>(), Is.EqualTo(new BigInteger(123)));
    }

    [Test]
    public void TestDeserializingInvalidBigIntRemoteValueThrows()
    {
        string json = @"{ ""type"": ""bigint"", ""value"": true }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("bigint must have a non-null 'value' property whose value is a string"));
    }

    [Test]
    public void TestDeserializingInvalidBigIntValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""bigint"", ""value"": ""some value"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("cannot parse invalid value 'some value' for bigint"));
    }

    [Test]
    public void TestCanDeserializeDateRemoteValue()
    {
        string json = @"{ ""type"": ""date"", ""value"": ""2020-07-19T23:47:26.056-4:00"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("date"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<DateTime>());
        Assert.That(remoteValue.ValueAs<DateTime>(), Is.EqualTo(new DateTime(2020, 07, 19, 23, 47, 26, 56, DateTimeKind.Utc)));
    }

    [Test]
    public void TestDeserializingInvalidDateRemoteValueThrows()
    {
        string json = @"{ ""type"": ""date"", ""value"": true }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("date must have a non-null 'value' property whose value is a string"));
    }

    [Test]
    public void TestDeserializingInvalidDateValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""date"", ""value"": ""some value"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("cannot parse invalid value 'some value' for date"));
    }

    [Test]
    public void TestDeserializingRegularExpressionRemoteValue()
    {
        string json = @"{ ""type"": ""regexp"", ""value"": { ""pattern"": ""myPattern"", ""flags"": ""gi"" } }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("regexp"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<RegularExpressionProperties>());
        Assert.That(remoteValue.ValueAs<RegularExpressionProperties>(), Is.EqualTo(new RegularExpressionProperties("myPattern", "gi")));
    }

    [Test]
    public void TestDeserializingRegularExpressionWithNullFlagsRemoteValue()
    {
        string json = @"{ ""type"": ""regexp"", ""value"": { ""pattern"": ""myPattern"" } }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("regexp"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<RegularExpressionProperties>());
        Assert.That(remoteValue.ValueAs<RegularExpressionProperties>(), Is.EqualTo(new RegularExpressionProperties("myPattern")));
    }

    [Test]
    public void TestDeserializingInvalidRegularExpressionValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""regexp"", ""value"": ""some value"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("regexp must have a non-null 'value' property whose value is an object"));
    }

    [Test]
    public void TestDeserializingNodeRemoteValue()
    {
        string json = @"{ ""type"": ""node"", ""value"": { ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0 } }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("node"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<NodeProperties>());
        var nodeProperties = remoteValue.ValueAs<NodeProperties>();
        Assert.That(nodeProperties!.NodeType, Is.EqualTo(1));
        Assert.That(nodeProperties!.NodeValue, Is.EqualTo(string.Empty));
        Assert.That(nodeProperties!.ChildNodeCount, Is.EqualTo(0));
    }

    [Test]
    public void TestDeserializingInvalidNodeValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""node"", ""value"": ""some value"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("node must have a non-null 'value' property whose value is an object"));
    }

    [Test]
    public void TestDeserializingArrayRemoteValue()
    {
        string json = @"{ ""type"": ""array"", ""value"": [ { ""type"": ""string"", ""value"": ""stringValue"" }, { ""type"": ""number"", ""value"": 123 }, { ""type"": ""boolean"", ""value"": true } ] }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("array"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<RemoteValueList>());
        var arrayValue = remoteValue.ValueAs<RemoteValueList>();
        Assert.That(arrayValue, Is.Not.Null);
        Assert.That(arrayValue!.Count, Is.EqualTo(3));
        Assert.That(arrayValue![0].ValueAs<string>, Is.EqualTo("stringValue"));
        Assert.That(arrayValue![1].ValueAs<long>, Is.EqualTo(123));
        Assert.That(arrayValue![2].ValueAs<bool>, Is.EqualTo(true));
    }

    [Test]
    public void TestDeserializingInvalidArrayValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""array"", ""value"": ""some value"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("array must have a non-null 'value' property whose value is an array"));
    }

    [Test]
    public void TestDeserializingInvalidArrayElementValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""array"", ""value"": [ ""stringValue"", 123, true ] }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("each element for array must be an object"));
    }

    [Test]
    public void TestDeserializingSetRemoteValue()
    {
        string json = @"{ ""type"": ""set"", ""value"": [ { ""type"": ""string"", ""value"": ""stringValue"" }, { ""type"": ""number"", ""value"": 123 }, { ""type"": ""boolean"", ""value"": true } ] }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("set"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<RemoteValueList>());
        var arrayValue = remoteValue.ValueAs<RemoteValueList>();
        Assert.That(arrayValue, Is.Not.Null);
        Assert.That(arrayValue!.Count, Is.EqualTo(3));
        Assert.That(arrayValue![0].ValueAs<string>, Is.EqualTo("stringValue"));
        Assert.That(arrayValue![1].ValueAs<long>, Is.EqualTo(123));
        Assert.That(arrayValue![2].ValueAs<bool>, Is.EqualTo(true));
    }

    [Test]
    public void TestDeserializingInvalidSetValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""set"", ""value"": ""some value"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("set must have a non-null 'value' property whose value is an array"));
    }

    [Test]
    public void TestDeserializingInvalidSetElementValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""set"", ""value"": [ ""stringValue"", 123, true ] }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("each element for set must be an object"));
    }

    [Test]
    public void TestDeserializingMapRemoteValue()
    {
        string json = @"{ ""type"": ""map"", ""value"": { ""stringProperty"": { ""type"": ""string"", ""value"": ""stringValue"" }, ""numberProperty"": { ""type"": ""number"", ""value"": 123 }, ""booleanProperty"": { ""type"": ""boolean"", ""value"": true } } }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("map"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<Dictionary<string, RemoteValue>>());
        var dictionaryValue = remoteValue.ValueAs<Dictionary<string, RemoteValue>>();
        Assert.That(dictionaryValue, Is.Not.Null);
        Assert.That(dictionaryValue!.Count, Is.EqualTo(3));
        Assert.That(dictionaryValue.ContainsKey("stringProperty"));
        Assert.That(dictionaryValue!["stringProperty"].ValueAs<string>, Is.EqualTo("stringValue"));
        Assert.That(dictionaryValue.ContainsKey("numberProperty"));
        Assert.That(dictionaryValue!["numberProperty"].ValueAs<long>, Is.EqualTo(123));
        Assert.That(dictionaryValue.ContainsKey("booleanProperty"));
        Assert.That(dictionaryValue!["booleanProperty"].ValueAs<bool>, Is.EqualTo(true));
    }

    [Test]
    public void TestDeserializingInvalidMapValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""map"", ""value"": ""some value"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("map must have a non-null 'value' property whose value is an object"));
    }

    [Test]
    public void TestDeserializingInvalidMapElementValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""map"", ""value"": { ""stringProperty"": ""stringValue"", ""numberProperty"": 123, ""booleanProperty"": true } }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("each property value for map must be an object"));
    }

    [Test]
    public void TestDeserializingObjectRemoteValue()
    {
        string json = @"{ ""type"": ""object"", ""value"": { ""stringProperty"": { ""type"": ""string"", ""value"": ""stringValue"" }, ""numberProperty"": { ""type"": ""number"", ""value"": 123 }, ""booleanProperty"": { ""type"": ""boolean"", ""value"": true } } }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("object"));
        Assert.That(remoteValue.HasValue);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
        Assert.That(remoteValue.Value, Is.InstanceOf<Dictionary<string, RemoteValue>>());
        var dictionaryValue = remoteValue.ValueAs<Dictionary<string, RemoteValue>>();
        Assert.That(dictionaryValue, Is.Not.Null);
        Assert.That(dictionaryValue!.Count, Is.EqualTo(3));
        Assert.That(dictionaryValue.ContainsKey("stringProperty"));
        Assert.That(dictionaryValue!["stringProperty"].ValueAs<string>, Is.EqualTo("stringValue"));
        Assert.That(dictionaryValue.ContainsKey("numberProperty"));
        Assert.That(dictionaryValue!["numberProperty"].ValueAs<long>, Is.EqualTo(123));
        Assert.That(dictionaryValue.ContainsKey("booleanProperty"));
        Assert.That(dictionaryValue!["booleanProperty"].ValueAs<bool>, Is.EqualTo(true));
    }

    [Test]
    public void TestDeserializingInvalidObjectValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""object"", ""value"": ""some value"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("object must have a non-null 'value' property whose value is an object"));
    }

    [Test]
    public void TestDeserializingInvalidObjectElementValueRemoteValueThrows()
    {
        string json = @"{ ""type"": ""object"", ""value"": { ""stringProperty"": ""stringValue"", ""numberProperty"": 123, ""booleanProperty"": true } }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("each property value for object must be an object"));
    }

    [Test]
    public void TestDeserializingSymbolRemoteValue()
    {
        string json = @"{ ""type"": ""symbol"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("symbol"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingFunctionRemoteValue()
    {
        string json = @"{ ""type"": ""function"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("function"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingWeakMapRemoteValue()
    {
        string json = @"{ ""type"": ""weakmap"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("weakmap"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingWeakSetRemoteValue()
    {
        string json = @"{ ""type"": ""weakset"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("weakset"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingIteratorRemoteValue()
    {
        string json = @"{ ""type"": ""iterator"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("iterator"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingGeneratorRemoteValue()
    {
        string json = @"{ ""type"": ""generator"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("generator"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingErrorRemoteValue()
    {
        string json = @"{ ""type"": ""error"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("error"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingProxyRemoteValue()
    {
        string json = @"{ ""type"": ""proxy"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("proxy"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingPromiseRemoteValue()
    {
        string json = @"{ ""type"": ""promise"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("promise"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingTypedArrayRemoteValue()
    {
        string json = @"{ ""type"": ""typedarray"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("typedarray"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingArrayBufferRemoteValue()
    {
        string json = @"{ ""type"": ""arraybuffer"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("arraybuffer"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingWindowRemoteValue()
    {
        string json = @"{ ""type"": ""window"" }";
        RemoteValue? remoteValue = JsonConvert.DeserializeObject<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue!.Type, Is.EqualTo("window"));
        Assert.That(remoteValue.HasValue, Is.False);
        Assert.That(remoteValue.Handle, Is.Null);
        Assert.That(remoteValue.InternalId, Is.Null);
    }

    [Test]
    public void TestDeserializingRemoteValueWithMissingTypeThrows()
    {
        string json = @"{ ""value"": ""myValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("must contain a 'type' property"));
    }

    [Test]
    public void TestDeserializingRemoteValueWithInvalidTypeThrows()
    {
        string json = @"{ ""type"": 3, ""value"": ""myValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RemoteValue>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("type property must be a string"));
    }
}