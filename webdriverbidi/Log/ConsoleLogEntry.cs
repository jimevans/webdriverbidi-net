// <copyright file="ConsoleLogEntry.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Log;

using Newtonsoft.Json;
using WebDriverBidi.Script;

/// <summary>
/// Represents a console log entry in the browser.
/// </summary>
public class ConsoleLogEntry : LogEntry
{
    private string method = string.Empty;
    private List<RemoteValue> args = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleLogEntry" /> class.
    /// </summary>
    internal ConsoleLogEntry()
        : base()
    {
    }

    /// <summary>
    /// Gets the method for the console log entry.
    /// </summary>
    [JsonProperty("method")]
    [JsonRequired]
    public string Method { get => this.method; internal set => this.method = value; }

    /// <summary>
    /// Gets the read-only list of arguments for the console log entry.
    /// </summary>
    public IList<RemoteValue> Args => this.args.AsReadOnly();

    /// <summary>
    /// Gets or sets the arguments of the console log entry for serialization purposes.
    /// </summary>
    [JsonProperty("args")]
    [JsonRequired]
    internal List<RemoteValue> SerializableArgs { get => this.args; set => this.args = value; }
}