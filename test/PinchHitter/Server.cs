// <copyright file="Server.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// An abstract base class for a server listening on a port for TCP messages and able
/// to process incoming data received on that port.
/// </summary>
public abstract class Server
{
    private readonly TcpListener listener;
    private readonly CancellationTokenSource listenerCancelationTokenSource = new();
    private readonly List<string> serverLog = new();
    private Socket? clientSocket;
    private int port = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class listening on a random port.
    /// </summary>
    protected Server()
        : this(0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class listening on a specific port.
    /// </summary>
    /// <param name="port">The port on which to listen. Passing zero (0) for the port will select a random port.</param>
    protected Server(int port)
    {
        this.port = port;
        this.listener = new(new IPEndPoint(IPAddress.Loopback, this.port));
    }

    /// <summary>
    /// Event raised when data is received by the server.
    /// </summary>
    public event EventHandler<WebServerDataReceivedEventArgs>? DataReceived;

    /// <summary>
    /// Gets the port on which the server is listening for connections.
    /// </summary>
    public int Port => this.port;

    /// <summary>
    /// Gets the read-only communication log of the server.
    /// </summary>
    public IList<string> Log => this.serverLog.AsReadOnly();

    /// <summary>
    /// Gets a value indicating whether the server has a current client socket assigned.
    /// </summary>
    protected bool HasClientSocket => this.clientSocket is not null;

    /// <summary>
    /// Gets a value indicating whether the server should continue listening for incoming connections.
    /// </summary>
    protected virtual bool ContinueRunning
    {
        get { return !this.listenerCancelationTokenSource.Token.IsCancellationRequested; }
    }

    /// <summary>
    /// Starts the server listening for incoming connections.
    /// </summary>
    public void Start()
    {
        this.listener.Start();
        IPEndPoint? localEndpoint = this.listener.LocalEndpoint as IPEndPoint;
        if (localEndpoint is not null)
        {
            this.port = localEndpoint.Port;
        }

        Task.Run(() => this.ReceiveData().ConfigureAwait(false));
    }

    /// <summary>
    /// Stops the server from listening for incomeing connections.
    /// </summary>
    public void Stop()
    {
        this.listenerCancelationTokenSource.Cancel();
        this.listener.Stop();
    }

    /// <summary>
    /// Asynchrounously forcibly disconnects the server without following the appropriate shutdown procedure.
    /// </summary>
    /// <returns>A Task indicating the shutdown is complete.</returns>
    public virtual Task Disconnect()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously sends data to the client requesing data from this server.
    /// </summary>
    /// <param name="data">A byte array representing the data to be sent.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebServerException">Thrown when there is no client socket connected.</exception>
    protected async Task SendData(byte[] data)
    {
        if (this.clientSocket is null)
        {
            throw new Exception("No attached client");
        }

        int bytesSent = await this.clientSocket.SendAsync(data, SocketFlags.None);
        this.serverLog.Add($"SEND {bytesSent} bytes");
    }

    /// <summary>
    /// Asynchronously processes incoming data from the client.
    /// </summary>
    /// <param name="buffer">A byte array buffer containing the data.</param>
    /// <param name="receivedLength">The length of the data in the buffer.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected abstract Task ProcessIncomingData(byte[] buffer, int receivedLength);

    /// <summary>
    /// Adds a message to the server log.
    /// </summary>
    /// <param name="message">The message to add.</param>
    protected void LogMessage(string message)
    {
        this.serverLog.Add(message);
    }

    /// <summary>
    /// Raises the DataReceived event.
    /// </summary>
    /// <param name="e">The WebServerDataReceivedEventArgs object containing information about the DataReceived event.</param>
    protected virtual void OnDataReceived(WebServerDataReceivedEventArgs e)
    {
        if (this.DataReceived is not null)
        {
            this.DataReceived(this, e);
        }
    }

    private async Task ReceiveData()
    {
        this.clientSocket = await this.listener.AcceptSocketAsync(this.listenerCancelationTokenSource.Token);
        this.LogMessage("Socket connected");
        while (this.ContinueRunning)
        {
            byte[] buffer = new byte[1024];
            using NetworkStream networkStream = new(this.clientSocket);
            using MemoryStream memoryStream = new();
            do
            {
                // Use a NetworkStream to read the data from the socket, then write
                // the received data to a MemoryStream. This allows the server to
                // read requests that exceed the size of the buffer.
                int receivedLength = await networkStream.ReadAsync(buffer, this.listenerCancelationTokenSource.Token);
                await memoryStream.WriteAsync(buffer.AsMemory(0, receivedLength), this.listenerCancelationTokenSource.Token);
            }
            while (networkStream.DataAvailable);

            if (memoryStream.Length > 0)
            {
                // Reset the memory stream position, and copy the data into a buffer
                // suitable for processing.
                int totalReceived = Convert.ToInt32(memoryStream.Length);
                memoryStream.Position = 0;
                byte[] messageBytes = new byte[totalReceived];
                await memoryStream.ReadAsync(messageBytes.AsMemory(0, totalReceived), this.listenerCancelationTokenSource.Token);
                await this.ProcessIncomingData(messageBytes, totalReceived);
            }
        }

        this.clientSocket.Close();
    }
}