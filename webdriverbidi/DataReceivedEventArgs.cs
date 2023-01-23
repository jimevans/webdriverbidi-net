// <copyright file="DataReceivedEventArgs.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi;

/// <summary>
/// Object containing event data for events raised when data is received from a WebDriver Bidi connection.
/// </summary>
public class DataReceivedEventArgs : WebDriverBidiEventArgs
{
    private readonly string data;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataReceivedEventArgs" /> class.
    /// </summary>
    /// <param name="data">The data received from the connection.</param>
    public DataReceivedEventArgs(string data)
    {
        this.data = data;
    }

    /// <summary>
    /// Gets the data received from the connection.
    /// </summary>
    public string Data => this.data;
}