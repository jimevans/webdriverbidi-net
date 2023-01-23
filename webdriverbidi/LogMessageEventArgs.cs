// <copyright file="LogMessageEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Object containing event data for events raised when a log message is received from a WebDriver Bidi connection.
/// </summary>
public class LogMessageEventArgs : WebDriverBidiEventArgs
{
    private readonly string message;
    private readonly WebDriverBidiLogLevel level;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogMessageEventArgs" /> class with a log message and log level.
    /// </summary>
    /// <param name="message">The message sent to the log.</param>
    /// <param name="level">The log level of the message sent to the log.</param>
    public LogMessageEventArgs(string message, WebDriverBidiLogLevel level)
    {
        this.message = message;
        this.level = level;
    }

    /// <summary>
    /// Gets the text of the message sent to the log.
    /// </summary>
    public string Message => this.message;

    /// <summary>
    /// Gets the log level of the message sent to the log.
    /// </summary>
    public WebDriverBidiLogLevel Level => this.level;
}