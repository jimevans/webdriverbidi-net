namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CallFunctionCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        CallFunctionCommandParameters properties = new("myFunction", new RealmTarget("myRealm"), true);
        Assert.That(properties.MethodName, Is.EqualTo("script.callFunction"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        CallFunctionCommandParameters properties = new("myFunction", new RealmTarget("myRealm"), true);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("functionDeclaration"));
            Assert.That(serialized["functionDeclaration"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("target"));
            Assert.That(serialized["target"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Contains.Key("awaitPromise"));
            Assert.That(serialized["awaitPromise"]!.Type, Is.EqualTo(JTokenType.Boolean));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalValues()
    {
        CallFunctionCommandParameters properties = new("myFunction", new RealmTarget("myRealm"), true);
        properties.Arguments.Add(LocalValue.String("myArgument"));
        properties.ThisObject = LocalValue.String("thisObject");
        properties.ResultOwnership = ResultOwnership.None;
        properties.SerializationOptions = new()
        {
            MaxDomDepth = 1,
        };
        properties.UserActivation = true;
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(8));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("functionDeclaration"));
            Assert.That(serialized["functionDeclaration"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("target"));
            Assert.That(serialized["target"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Contains.Key("awaitPromise"));
            Assert.That(serialized["awaitPromise"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized, Contains.Key("arguments"));
            Assert.That(serialized["arguments"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized, Contains.Key("this"));
            Assert.That(serialized["this"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Contains.Key("resultOwnership"));
            Assert.That(serialized["resultOwnership"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized, Contains.Key("serializationOptions"));
            Assert.That(serialized["serializationOptions"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Contains.Key("userActivation"));
            Assert.That(serialized["userActivation"]!.Type, Is.EqualTo(JTokenType.Boolean));
        });
    }
}