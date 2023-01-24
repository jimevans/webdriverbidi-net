// <copyright file="Driver.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

using WebDriverBidi.BrowsingContext;
using WebDriverBidi.Log;
using WebDriverBidi.Script;
using WebDriverBidi.Session;

/// <summary>
/// Object containing commands to drive a browser using the WebDriver Bidi protocol.
/// </summary>
public class Driver
{
    private readonly ProtocolTransport transport;
    private readonly Dictionary<string, ProtocolModule> modules = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Driver" /> class.
    /// </summary>
    public Driver()
        : this(new ProtocolTransport())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Driver" /> class with the specified <see cref="ProtocolTransport" />.
    /// </summary>
    /// <param name="transport">The protocol transport object used to communicate with the browser.</param>
    public Driver(ProtocolTransport transport)
    {
        this.transport = transport;
        this.transport.EventReceived += this.OnTransportEventReceived;
        this.transport.ErrorEventReceived += this.OnTransportErrorEventReceived;
        this.transport.UnknownMessageReceived += this.OnTransportUnknownMessageReceived;
        this.transport.LogMessage += this.OnTransportLogMessage;
        this.RegisterModule(new BrowsingContextModule(this));
        this.RegisterModule(new SessionModule(this));
        this.RegisterModule(new ScriptModule(this));
        this.RegisterModule(new LogModule(this));
    }

    /// <summary>
    /// Raised when a protocol event is received from protocol transport.
    /// </summary>
    public event EventHandler<ProtocolEventReceivedEventArgs>? EventReceived;

    /// <summary>
    /// Raised when a protocol error is received from protocol transport.
    /// </summary>
    public event EventHandler<ProtocolErrorReceivedEventArgs>? UnexpectedErrorReceived;

    /// <summary>
    /// Raised when an unknown message is received from protocol transport.
    /// </summary>
    public event EventHandler<ProtocolUnknownMessageReceivedEventArgs>? UnknownMessageReceived;

    /// <summary>
    /// Raised when a log message is emitted by this driver.
    /// </summary>
    public event EventHandler<LogMessageEventArgs>? LogMessage;

    /// <summary>
    /// Gets the browsingContext module as described in the WebDriver Bidi protocol.
    /// </summary>
    public BrowsingContextModule BrowsingContext => this.GetModule<BrowsingContextModule>(BrowsingContextModule.BrowsingContextModuleName);

    /// <summary>
    /// Gets the sessiom module as described in the WebDriver Bidi protocol.
    /// </summary>
    public SessionModule Session => this.GetModule<SessionModule>(SessionModule.SessionModuleName);

    /// <summary>
    /// Gets the script module as described in the WebDriver Bidi protocol.
    /// </summary>
    public ScriptModule Script => this.GetModule<ScriptModule>(ScriptModule.ScriptModuleName);

    /// <summary>
    /// Gets the log module as described in the WebDriver Bidi protocol.
    /// </summary>
    public LogModule Log => this.GetModule<LogModule>(LogModule.LogModuleName);

    /// <summary>
    /// Asynchronously starts the communication with the remote end of the WebDriver Bidi protocol.
    /// </summary>
    /// <param name="url">The URL of the remote end.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task Start(string url)
    {
        await this.transport.Connect(url);
    }

    /// <summary>
    /// Asynchronously stops the communication with the remote end of the WebDriver Bidi protocol.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task Stop()
    {
        await this.transport.Disconnect();
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end of the WebDriver Bidi protocol and waits for a response.
    /// </summary>
    /// <typeparam name="T">The expected type of the result of the command.</typeparam>
    /// <param name="command">The object containing settings for the command, including parameters.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBidiException">Thrown if an error occurs during the execution of the command.</exception>
    public virtual async Task<T> ExecuteCommand<T>(CommandData command)
        where T : ResponseData
    {
        var result = await this.transport.SendCommandAndWait(command);
        if (result is null)
        {
            throw new WebDriverBidiException("Received null response from transport for SendCommandAndWait");
        }

        if (result.IsError)
        {
            if (result is not ErrorResponseData errorResponse)
            {
                throw new WebDriverBidiException("Received null converting error response from transport for SendCommandAndWait");
            }

            throw new WebDriverBidiException($"Received '{errorResponse.ErrorType}' error executing command {command.MethodName}: {errorResponse.ErrorMessage}");
        }

        if (result is not T convertedResult)
        {
            throw new WebDriverBidiException("Received null converting response from transport for SendCommandAndWait");
        }

        return convertedResult;
    }

    /// <summary>
    /// Registers a module for use with this driver.
    /// </summary>
    /// <param name="module">The module object.</param>
    public void RegisterModule(ProtocolModule module)
    {
        this.modules[module.ModuleName] = module;
    }

    /// <summary>
    /// Gets a module from the set of registered modules for this driver.
    /// </summary>
    /// <typeparam name="T">A module object which is a subclass of <see cref="ProtocolModule"/>.</typeparam>
    /// <param name="moduleName">The name of the module to return.</param>
    /// <returns>The protocol module object.</returns>
    public T GetModule<T>(string moduleName)
        where T : ProtocolModule
    {
        if (!this.modules.ContainsKey(moduleName))
        {
            throw new WebDriverBidiException($"Module '{moduleName}' is not registered with this driver");
        }

        ProtocolModule module = this.modules[moduleName];
        if (module is not T)
        {
            throw new WebDriverBidiException($"Module '{moduleName}' is registered with this driver, but the module object is not of type {typeof(T)}");
        }

        return (T)module;
    }

    /// <summary>
    /// Registers an event to be raised by the remote end of the WebDriver Bidi protocol.
    /// </summary>
    /// <typeparam name="T">The type of data that will be raised by the event.</typeparam>
    /// <param name="eventName">The name of the event to raise.</param>
    public virtual void RegisterEvent<T>(string eventName)
    {
        this.transport.RegisterEventMessage<T>(eventName);
    }

    /// <summary>
    /// Raises the UnexpectedErrorReceived event.
    /// </summary>
    /// <param name="e">The event args used when raising the event.</param>
    protected virtual void OnUnexpectedError(ProtocolErrorReceivedEventArgs e)
    {
        if (this.UnexpectedErrorReceived is not null)
        {
            this.UnexpectedErrorReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the EventReceived event.
    /// </summary>
    /// <param name="e">The event args used when raising the event.</param>
    protected virtual void OnEventReceived(ProtocolEventReceivedEventArgs e)
    {
        if (this.EventReceived is not null)
        {
            this.EventReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the UnknownMessageReceived event.
    /// </summary>
    /// <param name="e">The event args used when raising the event.</param>
    protected virtual void OnUnknownMessageReceived(ProtocolUnknownMessageReceivedEventArgs e)
    {
        if (this.UnknownMessageReceived is not null)
        {
            this.UnknownMessageReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the LogMessage event.
    /// </summary>
    /// <param name="e">The event args used when raising the event.</param>
    protected virtual void OnLogMessage(LogMessageEventArgs e)
    {
        if (this.LogMessage is not null)
        {
            this.LogMessage(this, e);
        }
    }

    private void OnTransportEventReceived(object? sender, ProtocolEventReceivedEventArgs e)
    {
        this.OnEventReceived(e);
    }

    private void OnTransportErrorEventReceived(object? sender, ProtocolErrorReceivedEventArgs e)
    {
        this.OnUnexpectedError(e);
    }

    private void OnTransportUnknownMessageReceived(object? sender, ProtocolUnknownMessageReceivedEventArgs e)
    {
        this.OnUnknownMessageReceived(e);
    }

    private void OnTransportLogMessage(object? sender, LogMessageEventArgs e)
    {
        this.OnLogMessage(e);
    }
}