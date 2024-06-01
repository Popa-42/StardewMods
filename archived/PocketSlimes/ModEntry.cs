namespace StardewMods.PocketSlimes;

using StardewMods.Common.Helpers;
using StardewMods.PocketSlimes.Framework;

/// <inheritdoc />
public sealed class ModEntry : StardewModdingAPI.Mod
{
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Initialize
        Log.Monitor = this.Monitor;
        ModPatches.Init(this.ModManifest);
    }
}