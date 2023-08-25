// <copyright file="Dispatcher{T}.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Protocol;

using System.Threading.Channels;

/// <summary>
/// A class that asynchronously dispatches items in a first-in-first-out manner,
/// using a separate thread for dispatching the items.
/// </summary>
/// <typeparam name="T">The type of items to be dispatched.</typeparam>
public class Dispatcher<T>
{
    private readonly Channel<T> queue;
    private readonly Task monitorTask;
    private bool isDispatching;

    /// <summary>
    /// Initializes a new instance of the <see cref="Dispatcher{T}"/> class.
    /// </summary>
    public Dispatcher()
    {
        this.queue = Channel.CreateUnbounded<T>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = true,
        });
        this.monitorTask = Task.Run(() => this.MonitorQueue());
        this.monitorTask.ConfigureAwait(false);
        this.isDispatching = true;
    }

    /// <summary>
    /// Occurs when an item is dispatched.
    /// </summary>
    public event EventHandler<ItemDispatchedEventArgs<T>>? ItemDispatched;

    /// <summary>
    /// Gets a value indicating whether the dispatcher is currently dispatching items.
    /// </summary>
    public bool IsDispatching => this.isDispatching;

    /// <summary>
    /// Adds an item to the queue to be dispatched.
    /// </summary>
    /// <param name="itemToDispatch">The item to be dispatched.</param>
    /// <returns><see langword="true"/> if the item was queued for dispatching; otherwise, false.</returns>
    public bool Dispatch(T itemToDispatch)
    {
        return this.queue.Writer.TryWrite(itemToDispatch);
    }

    /// <summary>
    /// Asynchronously shuts down the dispatcher.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Shutdown()
    {
        // Attempt to wait for the channel to empty before marking the
        // writer as complete and waiting for the monitor task to end.
        while (this.queue.Reader.TryPeek(out _))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        this.queue.Writer.Complete();
        this.monitorTask.Wait();
        this.isDispatching = false;
    }

    /// <summary>
    /// Raises the ItemDispatched event.
    /// </summary>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="e">The EventArgs containing information about the event.</param>
    protected virtual void OnItemDispatched(object? sender, ItemDispatchedEventArgs<T> e)
    {
        if (this.ItemDispatched is not null)
        {
            this.ItemDispatched(this, e);
        }
    }

    private async Task MonitorQueue()
    {
        while (await this.queue.Reader.WaitToReadAsync())
        {
            this.DispatchPendingItems();
        }
    }

    private void DispatchPendingItems()
    {
        while (this.queue.Reader.TryRead(out T? item))
        {
            this.OnItemDispatched(this, new ItemDispatchedEventArgs<T>(item));
        }
   }
}
