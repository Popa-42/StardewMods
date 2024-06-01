﻿namespace StardewMods.IsItCake;

using StardewMods.Common.Helpers;
using StardewMods.IsItCake.Framework;

/// <inheritdoc />
public sealed class ModEntry : StardewModdingAPI.Mod
{
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Initialize
        I18n.Init(this.Helper.Translation);
        Log.Monitor = this.Monitor;
        ModPatches.Init(this.ModManifest);
    }
}