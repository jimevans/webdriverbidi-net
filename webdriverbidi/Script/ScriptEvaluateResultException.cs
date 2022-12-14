// <copyright file="ScriptEvaluateResultException.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object representing the evaluation of a script that throws an exception.
/// </summary>
public class ScriptEvaluateResultException : ScriptEvaluateResult
{
    private ExceptionDetails result = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptEvaluateResultException"/> class.
    /// </summary>
    [JsonConstructor]
    internal ScriptEvaluateResultException()
        : base()
    {
    }

    /// <summary>
    /// Gets the exception details of the script evaluation.
    /// </summary>
    [JsonProperty("exceptionDetails")]
    public ExceptionDetails ExceptionDetails { get => this.result; internal set => this.result = value; }
}