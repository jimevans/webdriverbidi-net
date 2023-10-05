namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class PointerUpActionTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        PointerUpAction properties = new(0);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalWidthAndHeight()
    {
        PointerUpAction properties = new(0)
        {
            Width = 1,
            Height = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("width"));
            Assert.That(serialized["width"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["width"]!.Value<long>(), Is.EqualTo(1));
            Assert.That(serialized, Contains.Key("height"));
            Assert.That(serialized["height"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["height"]!.Value<long>(), Is.EqualTo(1));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalPressureProperties()
    {
        PointerUpAction properties = new(0)
        {
            Pressure = 1,
            TangentialPressure = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("pressure"));
            Assert.That(serialized["pressure"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["pressure"]!.Value<double>(), Is.EqualTo(1));
            Assert.That(serialized, Contains.Key("tangentialPressure"));
            Assert.That(serialized["tangentialPressure"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["tangentialPressure"]!.Value<double>(), Is.EqualTo(1));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalTwistProperty()
    {
        PointerUpAction properties = new(0)
        {
            Twist = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("twist"));
            Assert.That(serialized["twist"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["twist"]!.Value<ulong>(), Is.EqualTo(1));
        });
    }

    [Test]
    public void TestSettingTwistPropertyToInvalidValueThrows()
    {
        PointerUpAction properties = new(0);
        Assert.That(() => properties.Twist = 360, Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Twist value must be between 0 and 359"));
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalAngleProperties()
    {
        PointerUpAction properties = new(0)
        {
            AzimuthAngle = 1,
            AltitudeAngle = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("altitudeAngle"));
            Assert.That(serialized["altitudeAngle"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["altitudeAngle"]!.Value<double>(), Is.EqualTo(1));
            Assert.That(serialized, Contains.Key("azimuthAngle"));
            Assert.That(serialized["azimuthAngle"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["azimuthAngle"]!.Value<double>(), Is.EqualTo(1));
        });
    }

    [Test]
    public void TestSettingAnglePropertiesToInvalidValueThrows()
    {
        PointerDownAction properties = new(0);
        Assert.Multiple(() =>
        {
            Assert.That(() => properties.AltitudeAngle = -0.01, Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("AltitudeAngle value must be between 0 and 1.5707963267948966 (pi / 2) inclusive"));
            Assert.That(() => properties.AltitudeAngle = 1.58, Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("AltitudeAngle value must be between 0 and 1.5707963267948966 (pi / 2) inclusive"));
            Assert.That(() => properties.AzimuthAngle = -0.01, Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("AzimuthAngle value must be between 0 and 6.283185307179586 (2 * pi) inclusive"));
            Assert.That(() => properties.AzimuthAngle = 6.29, Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("AzimuthAngle value must be between 0 and 6.283185307179586 (2 * pi) inclusive"));
        });
    }
}
