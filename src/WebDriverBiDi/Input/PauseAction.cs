// <copyright file="PauseAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// An action to pause the execution of a device.
/// </summary>
public class PauseAction : INoneSourceAction, IKeySourceAction, IPointerSourceAction, IWheelSourceAction
{
    private readonly string actionType = "pause";
    private TimeSpan? duration;

    /// <summary>
    /// Gets the type of the action.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type => this.actionType;

    /// <summary>
    /// Gets or sets the duration of the pause.
    /// </summary>
    [JsonIgnore]
    public TimeSpan? Duration { get => this.duration; set => this.duration = value; }

    /// <summary>
    /// Gets the duration of the pause for serialization purposes.
    /// </summary>
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal ulong? SerializedDuration
    {
        get
        {
            if (!this.duration.HasValue)
            {
                return null;
            }

            return Convert.ToUInt64(this.duration.Value.TotalMilliseconds);
        }
    }
}