// <copyright file="UnhandledError.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// An unhandled error received during execution of transport functions.
/// </summary>
public class UnhandledError
{
    private UnhandledErrorType errorType;
    private Exception exception;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnhandledError"/> class.
    /// </summary>
    /// <param name="errorType">The type of unhandled error.</param>
    /// <param name="exception">The <see cref="Exception"/> to be thrown by the unhandled error.</param>
    public UnhandledError(UnhandledErrorType errorType, Exception exception)
    {
        this.errorType = errorType;
        this.exception = exception;
    }

    /// <summary>
    /// Gets the type of unhandled error.
    /// </summary>
    public UnhandledErrorType ErrorType => this.errorType;

    /// <summary>
    /// Gets the exception thrown by the unhandled error.
    /// </summary>
    public Exception Exception => this.exception;
}
