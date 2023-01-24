namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class StatusCommandParametersTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        StatusCommandParameters properties = new();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }
}