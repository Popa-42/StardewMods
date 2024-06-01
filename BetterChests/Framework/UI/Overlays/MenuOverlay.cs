namespace StardewMods.BetterChests.Framework.UI.Overlays;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

internal sealed class MenuOverlay : BaseMenu
{
    private readonly IClickableMenu menu;

    public MenuOverlay(IIconRegistry iconRegistry, IClickableMenu menu)
    {
        this.menu = menu;

        // Initialize
        var downArrow = iconRegistry.Icon(VanillaIcon.ArrowDown).Component(IconStyle.Transparent).AsBuilder().Value;
        var upArrow = iconRegistry.Icon(VanillaIcon.ArrowUp).Component(IconStyle.Transparent).AsBuilder().Value;
        var textField = new TextField(0, 0, Math.Min(12 * Game1.tileSize, Game1.uiViewport.Width));

        // Events
        downArrow.Clicked += (_, _) => { };
        upArrow.Clicked += (_, _) => { };
        textField.ValueChanged += (_, _) => { };

        // Add components
        this.Components.Add(downArrow);
        this.Components.Add(upArrow);
        this.Components.Add(textField);
    }

    /// <inheritdoc />
    protected override void Draw(SpriteBatch spriteBatch, Point cursor) { }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch spriteBatch, Point cursor) { }
}