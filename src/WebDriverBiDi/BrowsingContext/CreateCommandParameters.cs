// <copyright file="CreateCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.create command.
/// </summary>
public class CreateCommandParameters : CommandParameters<CreateCommandResult>
{
    private CreateType createType;

    private string? referenceContextId;
    private bool? background;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommandParameters" /> class.
    /// </summary>
    /// <param name="createType">The type of browsing context to create.</param>
    public CreateCommandParameters(CreateType createType)
    {
        this.createType = createType;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.create";

    /// <summary>
    /// Gets or sets the type of browsing context (tab or window) to create.
    /// </summary>
    [JsonPropertyName("type")]
    public CreateType CreateType { get => this.createType; set => this.createType = value; }

    /// <summary>
    /// Gets or sets the ID of the browsing context to reference within the newly created context.
    /// </summary>
    [JsonPropertyName("referenceContext")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReferenceContextId { get => this.referenceContextId; set => this.referenceContextId = value; }

    /// <summary>
    /// Gets or sets a value indicating whether to create the new browsing context in the background.
    /// </summary>
    [JsonPropertyName("background")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsCreatedInBackground { get => this.background; set => this.background = value; }
}
