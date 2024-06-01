﻿namespace StardewMods.StackQuality;

using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.StackQuality;
using StardewMods.StackQuality.Framework;
using StardewMods.StackQuality.UI;

/// <inheritdoc />
public sealed class ModEntry : StardewModdingAPI.Mod
{
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Initialize
        Log.Monitor = this.Monitor;
        Helpers.Init(this.Helper);
        Integrations.Init(this.Helper);
        ModPatches.Init(this.Helper, this.ModManifest, (IStackQualityApi)this.GetApi());

        // Events
        this.Helper.Events.Display.RenderedActiveMenu += ModEntry.OnRenderedActiveMenu;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new Api();
    }

    [EventPriority(EventPriority.Low)]
    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!Helpers.IsSupported || Game1.activeClickableMenu.GetChildMenu() is not ItemQualityMenu itemQualityMenu)
        {
            return;
        }

        itemQualityMenu.Draw(e.SpriteBatch);
    }
}