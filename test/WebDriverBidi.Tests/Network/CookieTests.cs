namespace WebDriverBidi.Network;

using Newtonsoft.Json;

[TestFixture]
public class CookieTests
{
    [Test]
    public void TestCanDeserializeCookie()
    {
        string json = @"{ ""name"": ""cookieName"", ""value"": ""cookieValue"", ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""strict"", ""size"": 100 }";
        Cookie? cookie = JsonConvert.DeserializeObject<Cookie>(json);
        Assert.That(cookie, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(cookie!.Name, Is.EqualTo("cookieName"));
            Assert.That(cookie!.Value, Is.EqualTo("cookieValue"));
            Assert.That(cookie.BinaryValue, Is.Null);
            Assert.That(cookie.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(cookie.Path, Is.EqualTo("/cookiePath"));
            Assert.That(cookie.Secure, Is.False);
            Assert.That(cookie.HttpOnly, Is.False);
            Assert.That(cookie.SameSite, Is.EqualTo(CookieSameSiteValue.Strict));
            Assert.That(cookie.Size, Is.EqualTo(100));
            Assert.That(cookie.Expires, Is.Null);
            Assert.That(cookie.EpochExpires, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeCookieWithSameSiteLax()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""name"": ""cookieName"", ""value"": ""cookieValue"", ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""lax"", ""size"": 100 }";
        Cookie? cookie = JsonConvert.DeserializeObject<Cookie>(json);
        Assert.That(cookie, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(cookie!.Name, Is.EqualTo("cookieName"));
            Assert.That(cookie!.Value, Is.EqualTo("cookieValue"));
            Assert.That(cookie.BinaryValue, Is.Null);
            Assert.That(cookie.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(cookie.Path, Is.EqualTo("/cookiePath"));
            Assert.That(cookie.Secure, Is.False);
            Assert.That(cookie.HttpOnly, Is.False);
            Assert.That(cookie.SameSite, Is.EqualTo(CookieSameSiteValue.Lax));
            Assert.That(cookie.Size, Is.EqualTo(100));
            Assert.That(cookie.Expires, Is.Null);
            Assert.That(cookie.EpochExpires, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeCookieWithSameSiteNone()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""name"": ""cookieName"", ""value"": ""cookieValue"", ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""none"", ""size"": 100 }";
        Cookie? cookie = JsonConvert.DeserializeObject<Cookie>(json);
        Assert.That(cookie, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(cookie!.Name, Is.EqualTo("cookieName"));
            Assert.That(cookie!.Value, Is.EqualTo("cookieValue"));
            Assert.That(cookie.BinaryValue, Is.Null);
            Assert.That(cookie.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(cookie.Path, Is.EqualTo("/cookiePath"));
            Assert.That(cookie.Secure, Is.False);
            Assert.That(cookie.HttpOnly, Is.False);
            Assert.That(cookie.SameSite, Is.EqualTo(CookieSameSiteValue.None));
            Assert.That(cookie.Size, Is.EqualTo(100));
            Assert.That(cookie.Expires, Is.Null);
            Assert.That(cookie.EpochExpires, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeCookieWithBinaryValue()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""name"": ""cookieName"", ""binaryValue"": [ 65, 66, 67 ], ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""lax"", ""size"": 100 }";
        Cookie? cookie = JsonConvert.DeserializeObject<Cookie>(json);
        Assert.That(cookie, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(cookie!.Name, Is.EqualTo("cookieName"));
            Assert.That(cookie!.Value, Is.Null);
            Assert.That(cookie.BinaryValue, Is.EqualTo(new byte[] { 65, 66, 67 }));
            Assert.That(cookie.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(cookie.Path, Is.EqualTo("/cookiePath"));
            Assert.That(cookie.Secure, Is.False);
            Assert.That(cookie.HttpOnly, Is.False);
            Assert.That(cookie.SameSite, Is.EqualTo(CookieSameSiteValue.Lax));
            Assert.That(cookie.Size, Is.EqualTo(100));
            Assert.That(cookie.Expires, Is.Null);
            Assert.That(cookie.EpochExpires, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeCookieWithExpiration()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""name"": ""cookieName"", ""value"": ""cookieValue"", ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""lax"", ""size"": 100, ""expires"": " + milliseconds + @" }";
        Cookie? cookie = JsonConvert.DeserializeObject<Cookie>(json);
        Assert.That(cookie, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(cookie!.Name, Is.EqualTo("cookieName"));
            Assert.That(cookie!.Value, Is.EqualTo("cookieValue"));
            Assert.That(cookie.BinaryValue, Is.Null);
            Assert.That(cookie.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(cookie.Path, Is.EqualTo("/cookiePath"));
            Assert.That(cookie.Secure, Is.False);
            Assert.That(cookie.HttpOnly, Is.False);
            Assert.That(cookie.SameSite, Is.EqualTo(CookieSameSiteValue.Lax));
            Assert.That(cookie.Size, Is.EqualTo(100));
            Assert.That(cookie.Expires, Is.EqualTo(expireTime));
            Assert.That(cookie.EpochExpires, Is.EqualTo(milliseconds));
        });
    }

    [Test]
    public void TestDeserializeWithMissingNameThrows()
    {
        string json = @"{ ""value"": ""cookieValue"", ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""lax"", ""size"": 100 }";
        Assert.That(() => JsonConvert.DeserializeObject<Cookie>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'name' not found in JSON"));
    }

    [Test]
    public void TestDeserializeWithMissingDomainThrows()
    {
        string json = @"{ ""name"": ""cookieName"", ""value"": ""cookieValue"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""lax"", ""size"": 100 }";
        Assert.That(() => JsonConvert.DeserializeObject<Cookie>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'domain' not found in JSON"));
    }

    [Test]
    public void TestDeserializeWithMissingPathThrows()
    {
        string json = @"{ ""name"": ""cookieName"", ""value"": ""cookieValue"", ""domain"": ""cookieDomain"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""lax"", ""size"": 100 }";
        Assert.That(() => JsonConvert.DeserializeObject<Cookie>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'path' not found in JSON"));
    }

    [Test]
    public void TestDeserializeWithMissingSecureThrows()
    {
        string json = @"{ ""name"": ""cookieName"", ""value"": ""cookieValue"", ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""httpOnly"": false, ""sameSite"": ""lax"", ""size"": 100 }";
        Assert.That(() => JsonConvert.DeserializeObject<Cookie>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'secure' not found in JSON"));
    }

    [Test]
    public void TestDeserializeWithMissingHttpOnlyThrows()
    {
        string json = @"{ ""name"": ""cookieName"", ""value"": ""cookieValue"", ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""sameSite"": ""lax"", ""size"": 100 }";
        Assert.That(() => JsonConvert.DeserializeObject<Cookie>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'httpOnly' not found in JSON"));
    }

    [Test]
    public void TestDeserializeWithMissingSameSiteThrows()
    {
        string json = @"{ ""name"": ""cookieName"", ""value"": ""cookieValue"", ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""size"": 100 }";
        Assert.That(() => JsonConvert.DeserializeObject<Cookie>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'sameSite' not found in JSON"));
    }

    [Test]
    public void TestDeserializeWithMissingSizeThrows()
    {
        string json = @"{ ""name"": ""cookieName"", ""value"": ""cookieValue"", ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""lax"" }";
        Assert.That(() => JsonConvert.DeserializeObject<Cookie>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'size' not found in JSON"));
    }

    [Test]
    public void TestDeserializeWithInvalidSameSiteValueThrows()
    {
        string json = @"{ ""name"": ""cookieName"", ""value"": ""cookieValue"", ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""invalid"", ""size"": 10 }";
        Assert.That(() => JsonConvert.DeserializeObject<Cookie>(json), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("value 'invalid' is not valid for enum type"));
    }
}