namespace WebDriverBiDi.Protocol;

using Newtonsoft.Json.Linq;
using TestUtilities;
using PinchHitter;

[TestFixture]
public class TransportTests
{
    [Test]
    public async Task TestTransportCanSendCommand()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" }
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters }
        };

        TestConnection connection = new();
        Transport transport = new(connection);

        TestCommandParameters command = new(commandName);
        _ = await transport.SendCommandAsync(command);

        var dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();       
        Assert.That(dataValue, Is.EquivalentTo(expected));
    }

    [Test]
    public async Task TestTransportCanGetResponse()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        Transport transport = new(connection);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
        _ = Task.Run(() => 
        {
            Task.Delay(TimeSpan.FromMilliseconds(50));
            connection.RaiseDataReceivedEvent(@"{ ""type"": ""success"", ""id"": 1, ""result"": { ""value"": ""response value"" } }");
        });
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));
        var actualResult = command.Result;
        Assert.Multiple(() =>
        {
            Assert.That(actualResult!.IsError, Is.False);
            Assert.That(actualResult, Is.TypeOf<TestCommandResult>());
        });
        var convertedResult = actualResult as TestCommandResult;
        Assert.That(convertedResult!.Value, Is.EqualTo("response value"));
    }

    [Test]
    public async Task TestTransportCanGetResponseWithAdditionalData()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        Transport transport = new(connection);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
        _ = Task.Run(() => 
        {
            Task.Delay(TimeSpan.FromMilliseconds(50));
            connection.RaiseDataReceivedEvent(@"{ ""type"": ""success"", ""id"": 1, ""result"": { ""value"": ""response value"" }, ""extraDataName"": ""extraDataValue"" }");
        });
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(250));
        var actualResult = command.Result;
        Assert.Multiple(() =>
        {
            Assert.That(actualResult!.IsError, Is.False);
            Assert.That(actualResult, Is.TypeOf<TestCommandResult>());
        });
        var convertedResult = actualResult as TestCommandResult;
        Assert.Multiple(() =>
        {
            Assert.That(convertedResult!.Value, Is.EqualTo("response value"));
            Assert.That(convertedResult.AdditionalData, Has.Count.EqualTo(1));
            Assert.That(convertedResult.AdditionalData["extraDataName"], Is.EqualTo("extraDataValue"));
        });
    }

    [Test]
    public async Task TestTransportCanGetErrorResponse()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        Transport transport = new(connection);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
        _ = Task.Run(() => 
        {
            Task.Delay(TimeSpan.FromMilliseconds(50));
            connection.RaiseDataReceivedEvent(@"{ ""type"": ""error"", ""id"": 1, ""error"": ""unknown command"", ""message"": ""This is a test error message"" }");
        });
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(250));
        var actualResult = command.Result;
        Assert.Multiple(() =>
        {
            Assert.That(actualResult!.IsError, Is.True);
            Assert.That(actualResult, Is.InstanceOf<ErrorResult>());
        });
        var convertedResponse = actualResult as ErrorResult;
        Assert.Multiple(() =>
        {
            Assert.That(convertedResponse!.ErrorType, Is.EqualTo("unknown command"));
            Assert.That(convertedResponse!.ErrorMessage, Is.EqualTo("This is a test error message"));
            Assert.That(convertedResponse.StackTrace, Is.Null);
        });
    }

    [Test]
    public async Task TestTransportGetResponseWithThrownException()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        Transport transport = new(connection);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
        _ = Task.Run(() => 
        {
            Task.Delay(TimeSpan.FromMilliseconds(50));
            connection.RaiseDataReceivedEvent(@"{ ""type"": ""success"", ""id"": 1,  ""noResult"": { ""invalid"": ""unknown command"", ""message"": ""This is a test error message"" } }");
        });
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(250));
        Assert.That(command.ThrownException, Is.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Response did not contain properly formed JSON for response type"));
   }

    [Test]
    public async Task TestTransportLeavesCommandResultAndThrownExceptionNullWithoutResponse()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        Transport transport = new(connection);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
        Assert.Multiple(() =>
        {
            Assert.That(command.Result, Is.Null);
            Assert.That(command.ThrownException, Is.Null);
        });
    }

    [Test]
    public void TestTransportEventReceived()
    {
        string receivedName = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
        Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.EventReceived += (object? sender, EventReceivedEventArgs e) => {
            receivedName = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(@"{ ""type"": ""event"", ""method"": ""protocol.event"", ""params"": { ""paramName"": ""paramValue"" } }");
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.Multiple(() =>
        {
            Assert.That(receivedName, Is.EqualTo("protocol.event"));
            Assert.That(receivedData, Is.TypeOf<TestEventArgs>());
        });
        var convertedData = receivedData as TestEventArgs;
        Assert.That(convertedData!.ParamName, Is.EqualTo("paramValue"));
    }

    [Test]
    public void TestTransportErrorEventReceived()
    {
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
        Transport transport = new(connection);
        transport.ErrorEventReceived += (object? sender, ErrorReceivedEventArgs e) => {
            receivedData = e.ErrorData;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(@"{ ""type"": ""error"", ""id"": null, ""error"": ""unknown error"", ""message"": ""This is a test error message"" }");
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));

        Assert.That(receivedData, Is.TypeOf<ErrorResult>());
        var convertedData = receivedData as ErrorResult;
        Assert.Multiple(() =>
        {
            Assert.That(convertedData!.ErrorType, Is.EqualTo("unknown error"));
            Assert.That(convertedData.ErrorMessage, Is.EqualTo("This is a test error message"));
        });
    }

    [Test]
    public void TestTransportErrorEventReceivedWithNullValues()
    {
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (object? sender, UnknownMessageReceivedEventArgs e) =>
        {
            receivedData = e.Message;
            syncEvent.Set();
        };
        transport.ErrorEventReceived += (object? sender, ErrorReceivedEventArgs e) =>
        {
        };
        connection.RaiseDataReceivedEvent(@"{ ""type"": ""event"", ""method"": null }");
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestTransportLogsCommands()
    {
        List<LogMessageEventArgs> logs = new();
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.LogMessage += (sender, e) =>
        {
            logs.Add(e);
        };
        connection.RaiseLogMessageEvent("test log message", WebDriverBiDiLogLevel.Warn);
        Assert.That(logs, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(logs[0].Message, Is.EqualTo("test log message"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Warn));
        });
    }

    [Test]
    public void TestTransportLogsMalformedJsonMessages()
    {
        ManualResetEventSlim syncEvent = new(false);
        List<LogMessageEventArgs> logs = new();
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.LogMessage += (sender, e) =>
        {
            logs.Add(e);
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent("{ { }");
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Unexpected error parsing JSON message"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Error));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventWithMissingMessageType()
    {
        string json = @"{ ""id"": 1, ""result"": { ""value"": ""response value"" } }";
        string loggedEvent = string.Empty;
        ManualResetEventSlim syncEvent = new(false);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventWithInvalidMessageTypeValue()
    {
        string json = @"{ ""type"": ""invalid"", ""id"": 1, ""result"": { ""value"": ""response value"" } }";
        string loggedEvent = string.Empty;
        ManualResetEventSlim syncEvent = new(false);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventForSuccessMessageWithMissingId()
    {
        string json = @"{ ""type"": ""success"", ""result"": { ""value"": ""response value"" } }";
        string loggedEvent = string.Empty;
        ManualResetEventSlim syncEvent = new(false);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventForSuccessMessageWitInvalidIdDataType()
    {
        string json = @"{ ""type"": ""success"", ""id"": true, ""result"": { ""value"": ""response value"" } }";
        string loggedEvent = string.Empty;
        ManualResetEventSlim syncEvent = new(false);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventForSuccessMessageWitInvalidIdValue()
    {
        string json = @"{ ""type"": ""success"", ""id"": 1, ""result"": { ""value"": ""response value"" } }";
        string loggedEvent = string.Empty;
        ManualResetEventSlim syncEvent = new(false);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventForErrorMessageWithMissingId()
    {
        string json = @"{ ""type"": ""error"", ""error"": ""unknown error"", ""message"": ""This is a test error message"" }";
        string loggedEvent = string.Empty;
        List<LogMessageEventArgs> logs = new();
        CountdownEvent signaler = new(2);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            signaler.Signal();
        };
        transport.LogMessage += (sender, e) =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
            }

            signaler.Signal();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = signaler.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Unexpected error parsing error JSON"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Error));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventForErrorMessageWithMissingErrorProperty()
    {
        string json = @"{ ""type"": ""error"", ""id"": null, ""message"": ""This is a test error message"" }";
        string loggedEvent = string.Empty;
        List<LogMessageEventArgs> logs = new();
        CountdownEvent signaler = new(2);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            signaler.Signal();
        };
        transport.LogMessage += (sender, e) =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
            }

            signaler.Signal();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = signaler.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Unexpected error parsing error JSON"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Error));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventForErrorMessageWithMissingMessageProperty()
    {
        string json = @"{ ""type"": ""error"", ""id"": null, ""error"": ""unknown error"" }";
        string loggedEvent = string.Empty;
        List<LogMessageEventArgs> logs = new();
        CountdownEvent signaler = new(2);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            signaler.Signal();
        };
        transport.LogMessage += (sender, e) =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
            }

            signaler.Signal();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = signaler.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Unexpected error parsing error JSON"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Error));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventForEventMessageWithMissingMethod()
    {
        string json = @"{ ""type"": ""event"", ""params"": { ""paramName"": ""paramValue"" } }";
        string loggedEvent = string.Empty;
        ManualResetEventSlim syncEvent = new(false);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventForEventMessageWithMissingParams()
    {
        string json = @"{ ""type"": ""event"", ""method"": ""protocol.event"" }";
        string loggedEvent = string.Empty;
        ManualResetEventSlim syncEvent = new(false);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventForEventMessageWithUnregisteredEventMethod()
    {
        string json = @"{ ""type"": ""event"", ""method"": ""protocol.event"", ""params"": { ""paramName"": ""paramValue"" } }";
        string loggedEvent = string.Empty;
        ManualResetEventSlim syncEvent = new(false);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        });
    }

    [Test]
    public void TestTransportRaisesUnknownMessageEventForEventMessageWithMismatchingEventParameters()
    {
        string json = @"{ ""type"": ""event"", ""method"": ""protocol.event"", ""params"": { ""invalidParamName"": ""paramValue"" } }";
        string loggedEvent = string.Empty;
        List<LogMessageEventArgs> logs = new();
        CountdownEvent signaler = new(2);
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.UnknownMessageReceived += (sender, e) =>
        {
            loggedEvent = e.Message;
            signaler.Signal();
        };
        transport.LogMessage += (sender, e) =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
            }

            signaler.Signal();
        };
        connection.RaiseDataReceivedEvent(json);
        bool eventRaised = signaler.Wait(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Unexpected error parsing event JSON"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Error));
        });
    }

    [Test]
    public async Task TestTransportCanUseDefaultConnection()
    {
        ManualResetEvent connectionSyncEvent = new(false);
        static void dataReceivedHandler(object? sender, ServerDataReceivedEventArgs e) { }
        void connectionHandler(object? sender, ClientConnectionEventArgs e) { connectionSyncEvent.Set(); }
        Server server = new();
        server.DataReceived += dataReceivedHandler;
        server.ClientConnected += connectionHandler;
        server.Start();

        Transport transport = new();
        await transport.ConnectAsync($"ws://localhost:{server.Port}");
        bool connectionEventRaised = connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));

        server.Stop();
        server.DataReceived -= dataReceivedHandler;
        server.ClientConnected -= connectionHandler;
        Assert.That(connectionEventRaised, Is.True);
    }

    [Test]
    public async Task TestCannotConnectWhenAlreadyConnected()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync($"ws://localhost:1234");
        Assert.That(async () => await transport.ConnectAsync($"ws://localhost:5678"), Throws.InstanceOf<WebDriverBiDiException>().With.Message.StartsWith($"The transport is already connected to ws://localhost:1234"));
    }

    [Test]
    public async Task TestTransportDisconnectWithPendingIncomingMessagesWillProcess()
    {
        string receivedName = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
        TestTransport transport = new(connection)
        {
            MessageProcessingDelay = TimeSpan.FromMilliseconds(100)
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.EventReceived += (object? sender, EventReceivedEventArgs e) => {
            receivedName = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(@"{ ""type"": ""event"", ""method"": ""protocol.event"", ""params"": { ""paramName"": ""paramValue"" } }");
        await transport.DisconnectAsync();
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(eventRaised, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(receivedName, Is.EqualTo("protocol.event"));
            Assert.That(receivedData, Is.TypeOf<TestEventArgs>());
        });
        var convertedData = receivedData as TestEventArgs;
        Assert.That(convertedData!.ParamName, Is.EqualTo("paramValue"));
    }

    [Test]
    public async Task TestTransportCanReuseConnectionToDifferentUrl()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" }
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters }
        };

        TestConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws://example.com:1234");

        TestCommandParameters command = new(commandName);
        _ = await transport.SendCommandAsync(command);

        var dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();       
        Assert.That(dataValue, Is.EquivalentTo(expected));
        await transport.DisconnectAsync();

        await transport.ConnectAsync("ws://example.com:5678");
        _ = await transport.SendCommandAsync(command);

        dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();       
        Assert.That(dataValue, Is.EquivalentTo(expected));
        await transport.DisconnectAsync();
    }
}
