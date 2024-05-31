#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.UI.Components;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.Common.Helpers;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;
#endif

/// <summary>Dropdown menu with option selector.</summary>
/// <typeparam name="TOption">The option type.</typeparam>
internal sealed class OptionDropdown<TOption> : BaseMenu
{
    private EventHandler<TOption?>? optionSelected;

    /// <summary>Initializes a new instance of the <see cref="OptionDropdown{TOption}" /> class.</summary>
    /// <param name="anchor">The component to anchor the dropdown to.</param>
    /// <param name="options">The options to pick from.</param>
    /// <param name="minWidth">The minimum width.</param>
    /// <param name="maxWidth">The maximum width.</param>
    /// <param name="maxOptions">The maximum number of items to display.</param>
    /// <param name="getLabel">A function which returns the label of an item.</param>
    /// <param name="spacing">The spacing between options.</param>
    public OptionDropdown(
        ClickableComponent anchor,
        IEnumerable<TOption> options,
        int minWidth = 0,
        int maxWidth = int.MaxValue,
        int maxOptions = int.MaxValue,
        OptionSelector<TOption>.GetLabelMethod? getLabel = null,
        int spacing = 16)
        : base(anchor.bounds.Left, anchor.bounds.Bottom)
    {
        var optionSelector = new OptionSelector<TOption>(options, minWidth, maxWidth, maxOptions, getLabel, spacing);
        optionSelector.SelectionChanged += (_, option) => this.optionSelected?.InvokeAll(this, option);

        this.Components.Add(optionSelector);
        this.Size = new Point(optionSelector.Bounds.Width + spacing, optionSelector.Bounds.Height + spacing);

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
    public event EventHandler<TOption?> OptionSelected
    {
        add => this.optionSelected += value;
        remove => this.optionSelected -= value;
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