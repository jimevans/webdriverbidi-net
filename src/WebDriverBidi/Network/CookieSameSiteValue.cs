// <copyright file="CookieSameSiteValue.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Enumerated values for the sameSite property of a cookie.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<CookieSameSiteValue>))]
public enum CookieSameSiteValue
{
    /// <summary>
    /// The cookie adheres to the Strict same-site policy.
    /// </summary>
    Strict,

    /// <summary>
    /// The cookie adheres to the Lax same-site policy.
    /// </summary>
    Lax,

    /// <summary>
    /// The cookie adheres to no same-site policy.
    /// </summary>
    None,
}