#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Common.UI.Components;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;
#endif

/// <summary>Dropdown menu with icon selector.</summary>
internal sealed class IconDropdown : BaseMenu
{
    private EventHandler<IIcon?>? iconSelected;

    /// <summary>Initializes a new instance of the <see cref="IconDropdown" /> class.</summary>
    /// <param name="anchor">The component to anchor the dropdown to.</param>
    /// <param name="icons">The icons to pick from.</param>
    /// <param name="rows">This rows of icons to display.</param>
    /// <param name="columns">The columns of icons to display.</param>
    /// <param name="getHoverText">A function which returns the icon hover text.</param>
    /// <param name="scale">The icon scale.</param>
    /// <param name="spacing">The spacing between icons.</param>
    public IconDropdown(
        ClickableComponent anchor,
        IEnumerable<IIcon> icons,
        int rows,
        int columns,
        IconSelector.GetHoverTextMethod? getHoverText = null,
        float scale = 3f,
        int spacing = 8)
        : base(anchor.bounds.Left, anchor.bounds.Bottom)
    {
        var iconSelector = new IconSelector(
            icons,
            rows,
            columns,
            getHoverText,
            scale,
            spacing,
            this.Bounds.X,
            this.Bounds.Y);

        iconSelector.SelectionChanged += (_, icon) => this.iconSelected?.InvokeAll(this, icon);

        this.Components.Add(iconSelector);
        this.Size = new Point(iconSelector.Bounds.Width + spacing, iconSelector.Bounds.Height + spacing);

        // Default position is bottom-right
        var anchorOffset = anchor is ICustomComponent customComponent ? customComponent.Offset : Point.Zero;
        this.Location += anchorOffset;

        // Adjust position if dropdown is out of bounds
        this.Location = new Point(
            this.Bounds.Right <= Game1.uiViewport.Width
                ? this.Bounds.X
                : anchor.bounds.Right - this.Bounds.Width + anchorOffset.X,
            this.Bounds.Bottom <= Game1.uiViewport.Height
                ? this.Bounds.Y
                : anchor.bounds.Top - this.Bounds.Height + anchorOffset.Y);

        // Add scrollbar if needed
    }

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<IIcon?> IconSelected
    {
        add => this.iconSelected += value;
        remove => this.iconSelected -= value;
    }

    /// <inheritdoc />
    protected override bool TryLeftClick(Point cursor)
    {
        this.exitThisMenuNoSound();
        return false;
    }

    /// <inheritdoc />
    protected override bool TryRightClick(Point cursor)
    {
        this.exitThisMenuNoSound();
        return false;
    }
}