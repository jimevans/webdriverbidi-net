// <copyright file="Transport.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The transport object used for serializing and deserializing JSON data used in the WebDriver Bidi protocol.
/// It uses a <see cref="Connection"/> object to communicate with the remote end, and does no further processing
/// of the objects serialized or deserialized. Consumers of this class are expected to handle things like awaiting
/// the response of a WebDriver BiDi command message.
/// </summary>
public class Transport
{
    private readonly JsonSerializerOptions options = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    private readonly Connection connection;
    private readonly Dictionary<string, Type> eventMessageTypes = new();
    private PendingCommandCollection pendingCommands = new();
    private Dispatcher<string> incomingMessageQueue = new();
    private Dispatcher<EventMessage> eventDispatcher = new();
    private long nextCommandId = 0;
    private bool isConnected;

    /// <summary>
    /// Initializes a new instance of the <see cref="Transport"/> class.
    /// </summary>
    public Transport()
        : this(new Connection())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Transport"/> class with a given command timeout and connection.
    /// </summary>
    /// <param name="connection">The connection used to communicate with the protocol remote end.</param>
    public Transport(Connection connection)
    {
        this.connection = connection;
        this.incomingMessageQueue.ItemDispatched += this.OnIncomingMessageDispatched;
        this.eventDispatcher.ItemDispatched += this.OnEventDispatched;
        connection.DataReceived += this.OnConnectionDataReceived;
        connection.LogMessage += this.OnConnectionLogMessage;
    }

    /// <summary>
    /// Occurs when a message is logged.
    /// </summary>
    public event EventHandler<LogMessageEventArgs>? LogMessage;

    /// <summary>
    /// Occurs when an event is received from the protocol.
    /// </summary>
    public event EventHandler<EventReceivedEventArgs>? EventReceived;

    /// <summary>
    /// Occurs when an error is received from the protocol.
    /// </summary>
    public event EventHandler<ErrorReceivedEventArgs>? ErrorEventReceived;

    /// <summary>
    /// Occurs when an unknown message is received from the protocol.
    /// </summary>
    public event EventHandler<UnknownMessageReceivedEventArgs>? UnknownMessageReceived;

    /// <summary>
    /// Gets the ID of the last command to be added.
    /// </summary>
    protected long LastCommandId => this.nextCommandId;

    /// <summary>
    /// Asynchronously connects to the remote end web socket.
    /// </summary>
    /// <param name="websocketUri">The URI used to connect to the web socket.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task ConnectAsync(string websocketUri)
    {
        if (this.isConnected)
        {
            throw new WebDriverBiDiException($"The transport is already connected to {this.connection.ConnectedUrl}; you must disconnect before connecting to another URL");
        }

        if (!this.pendingCommands.IsAcceptingCommands)
        {
            this.pendingCommands = new PendingCommandCollection();
        }

        if (!this.incomingMessageQueue.IsDispatching)
        {
            this.incomingMessageQueue = new Dispatcher<string>();
            this.incomingMessageQueue.ItemDispatched += this.OnIncomingMessageDispatched;
        }

        if (!this.eventDispatcher.IsDispatching)
        {
            this.eventDispatcher = new Dispatcher<EventMessage>();
            this.eventDispatcher.ItemDispatched += this.OnEventDispatched;
        }

        // Reset the command counter for each connection.
        this.nextCommandId = 0;
        await this.connection.StartAsync(websocketUri).ConfigureAwait(false);
        this.isConnected = true;
    }

    /// <summary>
    /// Asynchronously disconnects from the remote end web socket.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task DisconnectAsync()
    {
        // Steps in the disconnect process:
        // 1. Close the pending command collection to further addition of commands.
        // 2. Stop the connection from receiving further communication traffic.
        // 3. Stop dispatching incoming command responses. Note that the dispatcher
        //    will attempt to dispatch any pending responses already in the queue.
        //    Dispatching the pending responses in the queue and processing those
        //    responses  will also remove those commands from the pending command
        //    collection.
        // 4. Stop dispatching incoming event messages. Note that the dispatcher will
        //    attempt to dispatch any pending event messages already in the queue.
        // 5. Clear the pending command collection. This will also cancel any tasks
        //    associated with the remaining pending commands.
        this.pendingCommands.Close();
        await this.connection.StopAsync().ConfigureAwait(false);
        this.incomingMessageQueue.StopDispatching();
        this.eventDispatcher.StopDispatching();
        this.pendingCommands.Clear();
        this.isConnected = false;
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end.
    /// </summary>
    /// <param name="commandData">The command settings object containing all data required to execute the command.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the command ID is already in use.</exception>
    public virtual async Task<Command> SendCommandAsync(CommandParameters commandData)
    {
        long commandId = Interlocked.Increment(ref this.nextCommandId);
        Command command = new(commandId, commandData);
        this.pendingCommands.AddPendingCommand(command);
        string commandJson = JsonSerializer.Serialize(command);
        await this.connection.SendDataAsync(commandJson).ConfigureAwait(false);
        return command;
    }

    /// <summary>
    /// Registers an event message to be recognized when received from the connection.
    /// </summary>
    /// <typeparam name="T">The type of data to be returned in the event.</typeparam>
    /// <param name="eventName">The name of the event.</param>
    public virtual void RegisterEventMessage<T>(string eventName)
    {
        this.eventMessageTypes[eventName] = typeof(EventMessage<T>);
    }

    /// <summary>
    /// Raises the LogMessage event.
    /// </summary>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="e">The EventArgs containing information about the event.</param>
    protected virtual void OnLogMessage(object? sender, LogMessageEventArgs e)
    {
        if (this.LogMessage is not null)
        {
            this.LogMessage(this, e);
        }
    }

    /// <summary>
    /// Raises the EventReceived event.
    /// </summary>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="e">The EventArgs containing information about the event.</param>
    protected virtual void OnProtocolEventReceived(object? sender, EventReceivedEventArgs e)
    {
        if (this.EventReceived is not null)
        {
            this.EventReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the ErrorEventReceived event.
    /// </summary>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="e">The EventArgs containing information about the event.</param>
    protected virtual void OnProtocolErrorEventReceived(object? sender, ErrorReceivedEventArgs e)
    {
        if (this.ErrorEventReceived is not null)
        {
            this.ErrorEventReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the UnknownMessageReceived event.
    /// </summary>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="e">The EventArgs containing information about the event.</param>
    protected virtual void OnProtocolUnknownMessageReceived(object? sender, UnknownMessageReceivedEventArgs e)
    {
        if (this.UnknownMessageReceived is not null)
        {
            this.UnknownMessageReceived(this, e);
        }
    }

    private void OnConnectionDataReceived(object? sender, ConnectionDataReceivedEventArgs e)
    {
        this.incomingMessageQueue.TryDispatch(e.Data);
    }

    private void OnIncomingMessageDispatched(object sender, ItemDispatchedEventArgs<string> e)
    {
        this.ProcessMessage(e.DispatchedItem);
    }

    private void ProcessMessage(string messageData)
    {
        bool isProcessed = false;
        JsonDocument? message = null;
        try
        {
            message = JsonDocument.Parse(messageData);
        }
        catch (JsonException e)
        {
            this.Log($"Unexpected error parsing JSON message: {e.Message}", WebDriverBiDiLogLevel.Error);
        }

        if (message is not null)
        {
            if (message.RootElement.TryGetProperty("type", out JsonElement messageTypeToken) && messageTypeToken.ValueKind == JsonValueKind.String)
            {
                string messageType = messageTypeToken.GetString()!;
                if (messageType == "success")
                {
                    isProcessed = this.ProcessCommandResponseMessage(message.RootElement);
                }
                else if (messageType == "error")
                {
                    isProcessed = this.ProcessErrorMessage(message.RootElement);
                }
                else if (messageType == "event")
                {
                    isProcessed = this.ProcessEventMessage(message.RootElement);
                }
            }
            else
            {
                // TODO: Remove this else clause when the browser stable channels
                // have the message type property implemented.
                if (message.RootElement.TryGetProperty("error", out JsonElement errorToken) && errorToken.ValueKind == JsonValueKind.String)
                {
                    isProcessed = this.ProcessErrorMessage(message.RootElement);
                }
                else
                {
                    if (message.RootElement.TryGetProperty("id", out JsonElement idToken) && idToken.TryGetInt64(out long _))
                    {
                        isProcessed = this.ProcessCommandResponseMessage(message.RootElement);
                    }
                    else
                    {
                        if (message.RootElement.TryGetProperty("method", out JsonElement eventNameToken) && eventNameToken.ValueKind == JsonValueKind.String)
                        {
                            isProcessed = this.ProcessEventMessage(message.RootElement);
                        }
                    }
                }
            }
        }

        if (!isProcessed)
        {
            this.OnProtocolUnknownMessageReceived(this, new UnknownMessageReceivedEventArgs(messageData));
        }
    }

    private bool ProcessCommandResponseMessage(JsonElement message)
    {
        if (message.TryGetProperty("id", out JsonElement idToken) && idToken.ValueKind == JsonValueKind.Number && idToken.TryGetInt64(out long responseId))
        {
            if (this.pendingCommands.RemovePendingCommand(responseId, out Command executedCommand))
            {
                try
                {
                    if (message.Deserialize(executedCommand.ResponseType, this.options) is CommandResponseMessage response)
                    {
                        CommandResult commandResult = response.Result;
                        commandResult.AdditionalData = response.AdditionalData;
                        executedCommand.Result = commandResult;
                   }
                }
                catch (Exception ex)
                {
                    executedCommand.ThrownException = new WebDriverBiDiException($"Response did not contain properly formed JSON for response type (response JSON:{message})", ex);
                }

                return true;
            }
        }

        return false;
    }

    private bool ProcessErrorMessage(JsonElement message)
    {
        try
        {
            // If the message doesn't match the schema of an actual error message,
            // an exception will be thrown by the JSON serializer, and we can log
            // the malformed response.
            ErrorResponseMessage? errorMessage = message.Deserialize<ErrorResponseMessage>(this.options);
            if (errorMessage is not null)
            {
                if (errorMessage.CommandId.HasValue && this.pendingCommands.RemovePendingCommand(errorMessage.CommandId.Value, out Command executedCommand))
                {
                    executedCommand.Result = errorMessage.GetErrorResponseData();
                }
                else
                {
                    this.OnProtocolErrorEventReceived(this, new ErrorReceivedEventArgs(errorMessage.GetErrorResponseData()));
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            this.Log($"Unexpected error parsing error JSON: {ex.Message} (JSON: {message})", WebDriverBiDiLogLevel.Error);
        }

        return false;
    }

    private bool ProcessEventMessage(JsonElement message)
    {
        if (message.TryGetProperty("method", out JsonElement eventNameToken) && eventNameToken.ValueKind == JsonValueKind.String)
        {
            // We have already validated that the token is of type string,
            // and therefore will never be null.
            string eventName = eventNameToken.GetString()!;
            if (this.eventMessageTypes.TryGetValue(eventName, out Type eventMessageType))
            {
                try
                {
                    // Deserialize will correctly throw if the type does not match, meaning
                    // the eventMessageData variable can never be null.
                    EventMessage? eventMessageData = message.Deserialize(eventMessageType, this.options) as EventMessage;
                    this.eventDispatcher.TryDispatch(eventMessageData!);
                    return true;
                }
                catch (Exception ex)
                {
                    this.Log($"Unexpected error parsing event JSON: {ex.Message} (JSON: {message})", WebDriverBiDiLogLevel.Error);
                }
            }
        }

        return false;
    }

    private void OnEventDispatched(object sender, ItemDispatchedEventArgs<EventMessage> e)
    {
        this.OnProtocolEventReceived(this, new EventReceivedEventArgs(e.DispatchedItem));
    }

    private void Log(string message, WebDriverBiDiLogLevel level)
    {
        this.OnLogMessage(this, new LogMessageEventArgs(message, level, "Transport"));
    }

    private void OnConnectionLogMessage(object? sender, LogMessageEventArgs e)
    {
        this.OnLogMessage(sender, e);
    }
}
