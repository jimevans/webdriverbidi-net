namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class PerformActionsCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        PerformActionsCommandParameters properties = new("myContextId");
        Assert.That(properties.MethodName, Is.EqualTo("input.performActions"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        PerformActionsCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("actions"));
            Assert.That(serialized["actions"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["actions"], Is.Empty);
        });
    }
}
