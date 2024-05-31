namespace StardewMods.BetterChests.Framework.UI.Overlays;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Menus;

internal sealed class MenuOverlay : BaseMenu
{
    public MenuOverlay(IIconRegistry iconRegistry)
    {
        var downArrow = iconRegistry.Icon(VanillaIcon.ArrowDown).Component(IconStyle.Transparent).AsBuilder().Value;
        var upArrow = iconRegistry.Icon(VanillaIcon.ArrowUp).Component(IconStyle.Transparent).AsBuilder().Value;

        downArrow.Clicked += (_, _) => { };
        upArrow.Clicked += (_, _) => { };

        this.Components.Add(downArrow);
        this.Components.Add(upArrow);
    }

    /// <inheritdoc />
    protected override void Draw(SpriteBatch spriteBatch, Point cursor) { }
}