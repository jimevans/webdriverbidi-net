namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class NewCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        NewCommandSettings properties = new();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }

    [Test]
    public void TestCanSerializeWithAlwaysMatch()
    {
        NewCommandSettings properties = new()
        {
            AlwaysMatch = new CapabilitiesRequest() { BrowserName = "greatBrowser" }
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("alwaysMatch"));
            Assert.That(serialized["alwaysMatch"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? alwaysMatch = serialized["alwaysMatch"] as JObject;
        Assert.That(alwaysMatch, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(alwaysMatch!.ContainsKey("browserName"));
            Assert.That(alwaysMatch["browserName"]!.Value<string>(), Is.EqualTo("greatBrowser"));
        });
    }

    [Test]
    public void TestCanSerializeWithFirstMatch()
    {
        NewCommandSettings properties = new();
        properties.FirstMatch.Add(new CapabilitiesRequest() { BrowserName = "greatBrowser" });
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("firstMatch"));
            Assert.That(serialized["firstMatch"]!.Type, Is.EqualTo(JTokenType.Array));
        });
        JArray? firstMatch = serialized["firstMatch"] as JArray;
        Assert.That(firstMatch, Is.Not.Null);
        Assert.That(firstMatch, Has.Count.EqualTo(1));
        Assert.That(firstMatch![0].Type, Is.EqualTo(JTokenType.Object));

        JObject? firstMatchElement = firstMatch[0] as JObject;
        Assert.Multiple(() =>
        {
            Assert.That(firstMatchElement, Has.Count.EqualTo(1));
            Assert.That(firstMatchElement!.ContainsKey("browserName"));
            Assert.That(firstMatchElement["browserName"]!.Value<string>(), Is.EqualTo("greatBrowser"));
        });
    }
}