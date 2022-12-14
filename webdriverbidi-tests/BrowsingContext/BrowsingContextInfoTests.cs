namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[TestFixture]
public class BrowsingContextInfoTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""children"": [] }";
        BrowsingContextInfo? info = JsonConvert.DeserializeObject<BrowsingContextInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Url, Is.EqualTo("http://example.com"));
            Assert.That(info.Children, Is.Not.Null);
            Assert.That(info.Children, Is.Empty);
            Assert.That(info.Parent, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithChildren()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""children"": [{ ""context"": ""childContextId"", ""url"": ""http://example.com/subdirectory"", ""children"": [] }] }";
        BrowsingContextInfo? info = JsonConvert.DeserializeObject<BrowsingContextInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Url, Is.EqualTo("http://example.com"));
            Assert.That(info.Children, Is.Not.Null);
            Assert.That(info.Children, Has.Count.EqualTo(1));
            Assert.That(info.Parent, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithOptionalParent()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""children"": [], ""parent"": ""parentContextId"" }";
        BrowsingContextInfo? info = JsonConvert.DeserializeObject<BrowsingContextInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Url, Is.EqualTo("http://example.com"));
            Assert.That(info.Children, Is.Not.Null);
            Assert.That(info.Children, Has.Count.EqualTo(0));
            Assert.That(info.Parent, Is.Not.Null);
            Assert.That(info.Parent, Is.EqualTo("parentContextId"));
        });
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingContextThrows()
    {
        string json = @"{ ""url"": ""http://example.com"", ""children"": [] }";
        Assert.That(() => JsonConvert.DeserializeObject<BrowsingContextInfo>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingUrlThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""children"": [] }";
        Assert.That(() => JsonConvert.DeserializeObject<BrowsingContextInfo>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingChildrenThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"" }";
        Assert.That(() => JsonConvert.DeserializeObject<BrowsingContextInfo>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidContextTypeThrows()
    {
        string json = @"{ ""context"": {}, ""url"": ""http://example.com"", ""children"": [] }";
        Assert.That(() => JsonConvert.DeserializeObject<BrowsingContextInfo>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidUrlTypeThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": {}, ""children"": [] }";
        Assert.That(() => JsonConvert.DeserializeObject<BrowsingContextInfo>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidChildrenTypeThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""children"": ""invalid"" }";
        Assert.That(() => JsonConvert.DeserializeObject<BrowsingContextInfo>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidParentTypeThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""children"": [], ""parent"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<BrowsingContextInfo>(json), Throws.InstanceOf<JsonReaderException>());
    }
}