﻿namespace StardewMods.BetterChests.Interfaces;

using StardewMods.Common.Integrations.BetterChests;

/// <summary>
///     Represents <see cref="IStorageData" /> with parent-child relationship.
/// </summary>
internal interface IStorageNode
{
    /// <summary>
    ///     Gets or sets the <see cref="IStorageData" />.
    /// </summary>
    public IStorageData Data { get; set; }

    /// <summary>
    ///     Gets or sets the parent <see cref="IStorageData" />.
    /// </summary>
    public IStorageData Parent { get; set; }
}