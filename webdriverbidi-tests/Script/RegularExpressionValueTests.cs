namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[TestFixture]
public class RegularExpressionValueTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""pattern"": ""myPattern"" }";
        RegularExpressionValue? regexProperties = JsonConvert.DeserializeObject<RegularExpressionValue>(json);
        Assert.That(regexProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(regexProperties!.Pattern, Is.EqualTo("myPattern"));
            Assert.That(regexProperties.Flags, Is.Null);
        });
    }

    [Test]
    public void TestDeserializeWithMissingPatternThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<RegularExpressionValue>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidPatternTypeThrows()
    {
        string json = @"{ ""pattern"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<RegularExpressionValue>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalFlags()
    {
        string json = @"{ ""pattern"": ""myPattern"", ""flags"": ""gi"" }";
        RegularExpressionValue? regexProperties = JsonConvert.DeserializeObject<RegularExpressionValue>(json);
        Assert.That(regexProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(regexProperties!.Pattern, Is.EqualTo("myPattern"));
            Assert.That(regexProperties.Flags, Is.Not.Null);
            Assert.That(regexProperties.Flags, Is.EqualTo("gi"));
        });
    }

    [Test]
    public void TestDeserializeWithInvalidFlagsTypeThrows()
    {
        string json = @"{ ""pattern"": ""myPattern"", ""flags"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<RegularExpressionValue>(json), Throws.InstanceOf<JsonReaderException>());
    }
}