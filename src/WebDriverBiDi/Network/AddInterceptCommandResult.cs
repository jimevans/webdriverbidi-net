// <copyright file="AddInterceptCommandResult.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Text.Json.Serialization;

/// <summary>
/// Result for adding an intercept for network traffic using the network.addIntercept command.
/// </summary>
public class AddInterceptCommandResult : CommandResult
{
    private string interceptId = string.Empty;

    [JsonConstructor]
    public AddInterceptCommandResult()
    {
    }

    /// <summary>
    /// Gets the screenshot image data as a base64-encoded string.
    /// </summary>
    [JsonPropertyName("intercept")]
    [JsonRequired]
    [JsonInclude]
    public string InterceptId { get => this.interceptId; internal set => this.interceptId = value; }
}
