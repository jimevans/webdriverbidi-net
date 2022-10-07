namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ProxyTests
{
    [Test]
    public void TestCanSerialize()
    {
        Proxy proxy = new Proxy();
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestCanSerializeWithHttpProxy()
    {
        Proxy proxy = new Proxy();
        proxy.HttpProxy = "http.proxy";
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("httpProxy"));
        Assert.That(serialized["httpProxy"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["httpProxy"]!.Value<string>(), Is.EqualTo("http.proxy"));
    }

    [Test]
    public void TestCanSerializeWithSslProxy()
    {
        Proxy proxy = new Proxy();
        proxy.SslProxy = "ssl.proxy";
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("sslProxy"));
        Assert.That(serialized["sslProxy"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["sslProxy"]!.Value<string>(), Is.EqualTo("ssl.proxy"));
    }

    [Test]
    public void TestCanSerializeWithFtpProxy()
    {
        Proxy proxy = new Proxy();
        proxy.FtpProxy = "ftp.proxy";
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("ftpProxy"));
        Assert.That(serialized["ftpProxy"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["ftpProxy"]!.Value<string>(), Is.EqualTo("ftp.proxy"));
    }

    [Test]
    public void TestCanSerializeWithProxyAutoConfigUrl()
    {
        Proxy proxy = new Proxy();
        proxy.ProxyAutoConfigUrl = "proxy.autoconfig.url";
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("proxyAutoconfigUrl"));
        Assert.That(serialized["proxyAutoconfigUrl"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["proxyAutoconfigUrl"]!.Value<string>(), Is.EqualTo("proxy.autoconfig.url"));
    }

    [Test]
    public void TestCanSerializeWithSocksProxy()
    {
        Proxy proxy = new Proxy();
        proxy.SocksProxy = "socks.proxy";
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("socksProxy"));
        Assert.That(serialized["socksProxy"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["socksProxy"]!.Value<string>(), Is.EqualTo("socks.proxy"));
    }

    [Test]
    public void TestCanSerializeWithSocksVersion()
    {
        Proxy proxy = new Proxy();
        proxy.SocksVersion = 4;
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("socksVersion"));
        Assert.That(serialized["socksVersion"]!.Type, Is.EqualTo(JTokenType.Integer));
        Assert.That(serialized["socksVersion"]!.Value<long>(), Is.EqualTo(4));
    }

    [Test]
    public void TestCanSerializeWithNoProxyAddresses()
    {
        Proxy proxy = new Proxy();
        proxy.NoProxyAddresses = new List<string>() { "no.proxy.address" };
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("noProxy"));
        Assert.That(serialized["noProxy"]!.Type, Is.EqualTo(JTokenType.Array));
        JArray? noProxyArray = serialized["noProxy"] as JArray;
        Assert.That(noProxyArray!.Count, Is.EqualTo(1));
        Assert.That(noProxyArray![0].Type, Is.EqualTo(JTokenType.String));
        Assert.That(noProxyArray![0].Value<string>(), Is.EqualTo("no.proxy.address"));
   }

    [Test]
    public void TestCanSerializeWithEmptyNoProxyAddresses()
    {
        Proxy proxy = new Proxy();
        proxy.NoProxyAddresses = new List<string>();
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("noProxy"));
        Assert.That(serialized["noProxy"]!.Type, Is.EqualTo(JTokenType.Array));
        JArray? noProxyArray = serialized["noProxy"] as JArray;
        Assert.That(noProxyArray!.Count, Is.EqualTo(0));
   }

    [Test]
    public void TestCanSerializeWithProxyTypeDirect()
    {
        Proxy proxy = new Proxy();
        proxy.Type = ProxyType.Direct;
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("proxyType"));
        Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("direct"));
    }

    [Test]
    public void TestCanSerializeWithProxyTypeManual()
    {
        Proxy proxy = new Proxy();
        proxy.Type = ProxyType.Manual;
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("proxyType"));
        Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("manual"));
    }

    [Test]
    public void TestCanSerializeWithProxyTypeSystem()
    {
        Proxy proxy = new Proxy();
        proxy.Type = ProxyType.System;
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("proxyType"));
        Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("system"));
    }

    [Test]
    public void TestCanSerializeWithProxyTypeAutoDetect()
    {
        Proxy proxy = new Proxy();
        proxy.Type = ProxyType.AutoDetect;
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("proxyType"));
        Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("autodetect"));
    }

    [Test]
    public void TestCanSerializeWithProxyTypeProxyAutoconfig()
    {
        Proxy proxy = new Proxy();
        proxy.Type = ProxyType.ProxyAutoConfig;
        string json = JsonConvert.SerializeObject(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(1));
        Assert.That(serialized.ContainsKey("proxyType"));
        Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("pac"));
    }

    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{}";
        Proxy? deserialized = JsonConvert.DeserializeObject<Proxy>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Type, Is.Null);
        Assert.That(deserialized.HttpProxy, Is.Null);
        Assert.That(deserialized.SslProxy, Is.Null);
        Assert.That(deserialized.FtpProxy, Is.Null);
        Assert.That(deserialized.ProxyAutoConfigUrl, Is.Null);
        Assert.That(deserialized.SocksProxy, Is.Null);
        Assert.That(deserialized.SocksVersion, Is.Null);
        Assert.That(deserialized.NoProxyAddresses, Is.Null);
    }

    [Test]
    public void TestCanDeserializeWithData()
    {
        string json = @"{ ""proxyType"": ""direct"", ""proxyAutoconfigUrl"": ""proxy.autoconfig.url"", ""httpProxy"": ""http.proxy"", ""sslProxy"": ""ssl.proxy"", ""ftpProxy"": ""ftp.proxy"", ""socksProxy"": ""socks.proxy"", ""socksVersion"": 4,""noProxy"": [""no.proxy.address""] }";
        Proxy? deserialized = JsonConvert.DeserializeObject<Proxy>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Type, Is.EqualTo(ProxyType.Direct));
        Assert.That(deserialized.HttpProxy, Is.EqualTo("http.proxy"));
        Assert.That(deserialized.SslProxy, Is.EqualTo("ssl.proxy"));
        Assert.That(deserialized.FtpProxy, Is.EqualTo("ftp.proxy"));
        Assert.That(deserialized.ProxyAutoConfigUrl, Is.EqualTo("proxy.autoconfig.url"));
        Assert.That(deserialized.SocksProxy, Is.EqualTo("socks.proxy"));
        Assert.That(deserialized.SocksVersion, Is.EqualTo(4));
        Assert.That(deserialized.NoProxyAddresses!.Count, Is.EqualTo(1));
        Assert.That(deserialized.NoProxyAddresses[0], Is.EqualTo("no.proxy.address"));
    }

    [Test]
    public void TestCanDeserializeWithProxyTypeManual()
    {
        string json = @"{ ""proxyType"": ""manual"" }";
        Proxy? deserialized = JsonConvert.DeserializeObject<Proxy>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Type, Is.EqualTo(ProxyType.Manual));
        Assert.That(deserialized.HttpProxy, Is.Null);
        Assert.That(deserialized.SslProxy, Is.Null);
        Assert.That(deserialized.FtpProxy, Is.Null);
        Assert.That(deserialized.ProxyAutoConfigUrl, Is.Null);
        Assert.That(deserialized.SocksProxy, Is.Null);
        Assert.That(deserialized.SocksVersion, Is.Null);
        Assert.That(deserialized.NoProxyAddresses, Is.Null);
    }

    [Test]
    public void TestCanDeserializeWithProxyTypeSystem()
    {
        string json = @"{ ""proxyType"": ""system"" }";
        Proxy? deserialized = JsonConvert.DeserializeObject<Proxy>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Type, Is.EqualTo(ProxyType.System));
        Assert.That(deserialized.HttpProxy, Is.Null);
        Assert.That(deserialized.SslProxy, Is.Null);
        Assert.That(deserialized.FtpProxy, Is.Null);
        Assert.That(deserialized.ProxyAutoConfigUrl, Is.Null);
        Assert.That(deserialized.SocksProxy, Is.Null);
        Assert.That(deserialized.SocksVersion, Is.Null);
        Assert.That(deserialized.NoProxyAddresses, Is.Null);
   }

    [Test]
    public void TestCanDeserializeWithProxyTypeAutoDetect()
    {
        string json = @"{ ""proxyType"": ""autodetect"" }";
        Proxy? deserialized = JsonConvert.DeserializeObject<Proxy>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Type, Is.EqualTo(ProxyType.AutoDetect));
        Assert.That(deserialized.HttpProxy, Is.Null);
        Assert.That(deserialized.SslProxy, Is.Null);
        Assert.That(deserialized.FtpProxy, Is.Null);
        Assert.That(deserialized.ProxyAutoConfigUrl, Is.Null);
        Assert.That(deserialized.SocksProxy, Is.Null);
        Assert.That(deserialized.SocksVersion, Is.Null);
        Assert.That(deserialized.NoProxyAddresses, Is.Null);
    }

    [Test]
    public void TestCanDeserializeWithProxyTypeProxyAutoconfig()
    {
        string json = @"{ ""proxyType"": ""pac"" }";
        Proxy? deserialized = JsonConvert.DeserializeObject<Proxy>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Type, Is.EqualTo(ProxyType.ProxyAutoConfig));
        Assert.That(deserialized.HttpProxy, Is.Null);
        Assert.That(deserialized.SslProxy, Is.Null);
        Assert.That(deserialized.FtpProxy, Is.Null);
        Assert.That(deserialized.ProxyAutoConfigUrl, Is.Null);
        Assert.That(deserialized.SocksProxy, Is.Null);
        Assert.That(deserialized.SocksVersion, Is.Null);
        Assert.That(deserialized.NoProxyAddresses, Is.Null);
    }

    [Test]
    public void TestDeserializeWithInvalidProxyTypeThrows()
    {
        string json = @"{ ""proxyType"": ""invalid"" }";
        Assert.That(() => JsonConvert.DeserializeObject<Proxy>(json), Throws.InstanceOf<JsonSerializationException>());
    }
}