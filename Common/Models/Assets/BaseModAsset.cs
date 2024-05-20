namespace StardewMods.Common.Models.Assets;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;

/// <summary>Represents an asset to be loaded.</summary>
internal abstract class BaseModAsset : ITrackedAsset
{
    /// <summary>Initializes a new instance of the <see cref="BaseModAsset" /> class.</summary>
    /// <param name="priority">The load priority.</param>
    protected BaseModAsset(AssetLoadPriority priority) => this.Priority = priority;

    /// <summary>Gets the priority for loading the asset.</summary>
    public AssetLoadPriority Priority { get; }

    /// <inheritdoc />
    public abstract void ProvideAsset(AssetRequestedEventArgs e);
}