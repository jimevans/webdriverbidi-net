// <copyright file="EventInvoker.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Object containing data about a WebDriver Bidi event.
/// </summary>
public abstract class EventInvoker
{
    /// <summary>
    /// Invokes the event.
    /// </summary>
    /// <param name="eventData">The data used to pass to the event for invocation.</param>
    /// <param name="additionalData">Additional data passed to the event for invocation.</param>
    public abstract void InvokeEvent(object eventData, ReceivedDataDictionary additionalData);
}
