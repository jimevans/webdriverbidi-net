// <copyright file="EventInvoker{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Object containing data about a WebDriver Bidi event where the data type is specifically known.
/// </summary>
/// <typeparam name="T">The type of the data for the event.</typeparam>
public class EventInvoker<T> : EventInvoker
{
    private readonly Func<EventInfo<T>, Task> asyncInvokerDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventInvoker{T}"/> class.
    /// </summary>
    /// <param name="asyncInvokerDelegate">The asynchronous delegate to use when invoking the event.</param>
    public EventInvoker(Func<EventInfo<T>, Task> asyncInvokerDelegate)
    {
        this.asyncInvokerDelegate = asyncInvokerDelegate;
    }

    /// <summary>
    /// Asynchronously invokes the event.
    /// </summary>
    /// <param name="eventData">The data to use when invoking the event.</param>
    /// <param name="additionalData">Additional data passed to the event for invocation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">
    /// Thrown when the type of the event data is not the type associated with this event data class.
    /// </exception>
    public override async Task InvokeEventAsync(object eventData, ReceivedDataDictionary additionalData)
    {
        if (eventData is not T typedEventData)
        {
            throw new WebDriverBiDiException($"Unable to cast received event data to {typeof(T)}");
        }

        EventInfo<T> invocationData = new(typedEventData, additionalData);
        await this.asyncInvokerDelegate(invocationData);
    }
}
