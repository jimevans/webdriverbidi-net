// <copyright file="IPointerSourceAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// Interface marking an action as an action used with a pointer input device.
/// </summary>
[JsonDerivedType(typeof(PointerDownAction))]
[JsonDerivedType(typeof(PointerUpAction))]
[JsonDerivedType(typeof(PointerMoveAction))]
public interface IPointerSourceAction
{
}
