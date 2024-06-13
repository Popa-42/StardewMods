namespace StardewMods.BetterChests.Framework.UI.Overlays;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

internal sealed class MenuOverlay : BaseMenu
{
    public MenuOverlay(MenuOverlay? previous, IStorageContainer container, IClickableMenu menu)
    {
        this.Container = container;
        this.Menu = menu;
    }

    /// <summary>Gets the container associated with the menu.</summary>
    public IStorageContainer Container { get; }

    /// <summary>Gets the menu that this menu is an overlay for.</summary>
    public IClickableMenu Menu { get; }

    /// <summary>Adds a component to the overlay.</summary>
    /// <param name="component">The component to add.</param>
    public void AddComponent(ClickableComponent component) => this.Components.Add(component);

    /// <inheritdoc />
    protected override void Draw(SpriteBatch spriteBatch, Point cursor) { }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch spriteBatch, Point cursor) { }
}