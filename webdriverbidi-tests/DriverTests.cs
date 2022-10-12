namespace WebDriverBidi;

using TestUtilities;

[TestFixture]
public class DriverTests
{
    [Test]
    public async Task CanExecuteCommand()
    {
        ManualResetEvent syncEvent = new ManualResetEvent(false);
        TestConnection connection = new TestConnection();
        connection.DataSendComplete += delegate(object? sender, EventArgs e)
        {
            syncEvent.Set();
        };

        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new Driver(transport);

        string commandName = "module.command";
        TestCommand command = new TestCommand(commandName);
        var task = Task.Run(() => driver.ExecuteCommand<TestCommandResult>(command));
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));

        connection.RaiseDataReceivedEvent(@"{ ""id"": 1, ""result"": { ""value"": ""command result value"" } }");
        task.Wait(TimeSpan.FromSeconds(3));
        Assert.That(task.Result.Value, Is.EqualTo("command result value"));
    }

    [Test]
    public async Task CanExecuteCommandWithError()
    {
        ManualResetEvent syncEvent = new ManualResetEvent(false);
        TestConnection connection = new TestConnection();
        connection.DataSendComplete += delegate(object? sender, EventArgs e)
        {
            syncEvent.Set();
        };

        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new Driver(transport);

        string commandName = "module.command";
        TestCommand command = new TestCommand(commandName);
        Assert.That(() => {
            var task = Task.Run(() => driver.ExecuteCommand<TestCommandResult>(command));
            syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
            connection.RaiseDataReceivedEvent(@"{ ""id"": 1, ""error"": ""unknown command"", ""message"": ""This is a test error message"" }");
            task.Wait(TimeSpan.FromSeconds(3));
        }, Throws.InstanceOf<AggregateException>().With.InnerException.TypeOf<WebDriverBidiException>().With.Message.Contains("'unknown command' error executing command module.command: This is a test error message"));
    }

    [Test]
    public async Task CanExecuteReceiveErrorWithoutCommand()
    {
        ErrorResponse? response = null;
        ManualResetEvent syncEvent = new ManualResetEvent(false);
        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new Driver(transport);
        driver.UnexpectedErrorReceived += delegate(object? sender, ProtocolErrorReceivedEventArgs e)
        {
            response = e.ErrorData;
            syncEvent.Set();
        };

        connection.RaiseDataReceivedEvent(@"{ ""id"": null, ""error"": ""unknown command"", ""message"": ""This is a test error message"" }");
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.ErrorType, Is.EqualTo("unknown command"));
        Assert.That(response.ErrorMessage, Is.EqualTo("This is a test error message"));
    }

    [Test]
    public async Task CanReceiveKnownEvent()
    {
        string receivedEvent = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new ManualResetEvent(false);

        string eventName = "module.event";
        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new Driver(transport);
        driver.RegisterEvent(eventName, typeof(TestEventArgs));
        driver.EventReceived += delegate(object? sender, ProtocolEventReceivedEventArgs e)
        {
            receivedEvent = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
        };

        connection.RaiseDataReceivedEvent(@"{ ""method"": ""module.event"", ""params"": { ""paramName"": ""paramValue"" } }");
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(receivedEvent, Is.EqualTo(eventName));
        Assert.That(receivedData, Is.Not.Null);
        Assert.That(receivedData, Is.TypeOf<TestEventArgs>());
        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.That(convertedData!.ParamName, Is.EqualTo("paramValue"));
    }

    [Test]
    public async Task TestUnregisteredEventRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        ManualResetEvent syncEvent = new ManualResetEvent(false);

        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new Driver(transport);
        driver.UnknownMessageReceived += delegate(object? sender, ProtocolUnknownMessageReceivedEventArgs e)
        {
            receivedMessage = e.Message;
            syncEvent.Set();
        };

        string serialized = @"{ ""method"": ""module.event"", ""params"": { ""paramName"": ""paramValue"" } }";
        connection.RaiseDataReceivedEvent(serialized);
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(receivedMessage, Is.EqualTo(serialized));
    }

    [Test]
    public async Task TestUnconformingDataRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        ManualResetEvent syncEvent = new ManualResetEvent(false);

        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new Driver(transport);
        driver.UnknownMessageReceived += delegate(object? sender, ProtocolUnknownMessageReceivedEventArgs e)
        {
            receivedMessage = e.Message;
            syncEvent.Set();
        };

        string serialized = @"{ ""someProperty"": ""someValue"", ""params"": { ""thisMessage"": ""matches no protocol message"" } }";
        connection.RaiseDataReceivedEvent(serialized);
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(receivedMessage, Is.EqualTo(serialized));
    }

}