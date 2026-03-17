// <copyright file="AstPropertyOccurrence.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.CodeGen.Ast;

using System.Text.Json.Serialization;

/// <summary>
/// The occurrence bounds for a field: n is the minimum count, m is the maximum
/// (null means unbounded). A field is optional when n == 0.
/// </summary>
public record AstPropertyOccurrence(
    [property: JsonPropertyName("n")] int N,
    [property: JsonPropertyName("m")] int? M);
