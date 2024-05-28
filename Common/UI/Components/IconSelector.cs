#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
#endif

/// <summary>Component for selecting an icon.</summary>
internal sealed class IconSelector : BaseComponent
{
    private readonly IEnumerable<IIcon> allIcons;
    private readonly int columns;
    private readonly GetHoverTextMethod getHoverText;
    private readonly List<HighlightMethod> highlights = [];
    private readonly int length;
    private readonly List<Operation> operations = [];
    private readonly int spacing;

    private int currentIndex = -1;
    private List<IIcon> icons;
    private EventHandler<IIcon?>? selectionChanged;

    /// <summary>Initializes a new instance of the <see cref="IconSelector" /> class.</summary>
    /// <param name="allIcons">The icons to pick from.</param>
    /// <param name="rows">This rows of icons to display.</param>
    /// <param name="columns">The columns of icons to display.</param>
    /// <param name="getHoverText">A function which returns the icon hover text.</param>
    /// <param name="scale">The icon scale.</param>
    /// <param name="spacing">The spacing between icons.</param>
    /// <param name="x">The x-position.</param>
    /// <param name="y">The y-position.</param>
    public IconSelector(
        IEnumerable<IIcon> allIcons,
        int rows,
        int columns,
        GetHoverTextMethod? getHoverText = null,
        float scale = Game1.pixelZoom,
        int spacing = 16,
        int? x = null,
        int? y = null)
        : base(
            x ?? 0,
            y ?? 0,
            (columns * ((int)(scale * 16) + spacing)) + spacing,
            (rows * ((int)(scale * 16) + spacing)) + spacing,
            "selectIcon")
    {
        this.allIcons = allIcons;
        this.columns = columns;
        this.spacing = spacing;
        this.getHoverText = getHoverText ?? IconSelector.GetUniqueId;
        this.length = (int)Math.Floor(scale * 16);
        this.RefreshIcons();
    }

    /// <summary>Get the hover text for an icon.</summary>
    /// <param name="icon">The icon.</param>
    /// <returns>The hover text.</returns>
    public delegate string GetHoverTextMethod(IIcon icon);

    /// <summary>Highlight an icon.</summary>
    /// <param name="icon">The icon to highlight.</param>
    /// <returns><c>true</c> if the icon should be highlighted; otherwise, <c>false</c>.</returns>
    public delegate bool HighlightMethod(IIcon icon);

    /// <summary>An operation to perform on the icons.</summary>
    /// <param name="icons">The original icons.</param>
    /// <returns>Returns a modified list of icons.</returns>
    public delegate IEnumerable<IIcon> Operation(IEnumerable<IIcon> icons);

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<IIcon?> SelectionChanged
    {
        add => this.selectionChanged += value;
        remove => this.selectionChanged -= value;
    }

    /// <summary>Gets the currently selected icon.</summary>
    public IIcon? CurrentSelection => this.icons.ElementAtOrDefault(this.CurrentIndex);

    /// <summary>Gets the current index.</summary>
    public int CurrentIndex
    {
        get => this.currentIndex;
        private set
        {
            this.currentIndex = value;
            this.selectionChanged?.InvokeAll(this, this.CurrentSelection);
        }
    }

    /// <summary>Add a highlight operation that will be applied to the items.</summary>
    /// <param name="highlight">The highlight operation.</param>
    /// <returns>Returns the menu.</returns>
    public IconSelector AddHighlight(HighlightMethod highlight)
    {
        this.highlights.Add(highlight);
        return this;
    }

    /// <summary>Add an operation that will be applied to the icons.</summary>
    /// <param name="operation">The operation to perform.</param>
    /// <returns>Returns the menu.</returns>
    public IconSelector AddOperation(Operation operation)
    {
        this.operations.Add(operation);
        return this;
    }

    /// <inheritdoc />
    public override void DrawUnder(SpriteBatch b, Point cursor) =>
        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            OptionsDropDown.dropDownBGSource,
            this.bounds.X,
            this.bounds.Y,
            this.bounds.Width,
            this.bounds.Height,
            Color.White,
            Game1.pixelZoom,
            false,
            0.97f);

    /// <summary>Get the hover text for an icon.</summary>
    /// <param name="icon">The icon.</param>
    /// <returns>The hover text.</returns>
    public string GetHoverText(IIcon icon) => this.getHoverText(icon);

    /// <summary>Refreshes the icons by applying the operations to them.</summary>
    [MemberNotNull(nameof(IconSelector.icons))]
    public void RefreshIcons()
    {
        this.ClearComponents();
        this.icons = this.operations.Aggregate(this.allIcons, (current, operation) => operation(current)).ToList();
        foreach (var icon in this.icons)
        {
            var index = this.Components.Count;
            var iconScale = (float)this.length / Math.Max(icon.Area.Width, icon.Area.Height);
            var component = icon
                .Component(IconStyle.Transparent, index.ToString(CultureInfo.InvariantCulture), iconScale)
                .AsBuilder()
                .Location(
                    this.bounds.X
                    + (index % this.columns * (this.length + this.spacing))
                    + (int)((this.length - (icon.Area.Width * iconScale)) / 2f)
                    + this.spacing,
                    this.bounds.Y
                    + (index / this.columns * (this.length + this.spacing))
                    + (int)((this.length - (icon.Area.Height * iconScale)) / 2f)
                    + this.spacing)
                .HoverText(this.GetHoverText(icon))
                .Value;

            component.Clicked += (sender, _) =>
            {
                if (sender is not ICustomComponent customComponent)
                {
                    return;
                }

                this.CurrentIndex = int.Parse(customComponent.Name, CultureInfo.InvariantCulture);
            };

            component.Rendering += (sender, _) =>
            {
                if (sender is not ICustomComponent customComponent)
                {
                    return;
                }

                customComponent.Color = this.HighlightIcon(icon) ? Color.White : Color.White * 0.25f;
            };

            this.AddComponent(component);
        }

        this.Overflow = new Point(
            0,
            this.Components.Any()
                ? Math.Max(0, this.Components[^1].Bounds.Bottom - this.Bounds.Y - this.Bounds.Height + this.spacing)
                : 0);
    }

    /// <inheritdoc />
    protected override void DrawInFrame(SpriteBatch spriteBatch, Point cursor)
    {
        if (this.CurrentIndex != -1)
        {
            spriteBatch.Draw(
                Game1.mouseCursors,
                new Rectangle(
                    this.bounds.X
                    + (this.CurrentIndex % this.columns * (this.length + this.spacing))
                    + this.spacing
                    - this.Offset.X,
                    this.bounds.Y
                    + (this.CurrentIndex / this.columns * (this.length + this.spacing))
                    + this.spacing
                    - this.Offset.Y,
                    this.length,
                    this.length),
                new Rectangle(194, 388, 16, 16),
                Color.White,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0.975f);
        }

        base.DrawInFrame(spriteBatch, cursor);
    }

    private static string GetUniqueId(IIcon icon) => icon.UniqueId;

    private bool HighlightIcon(IIcon icon) =>
        !this.highlights.Any() || this.highlights.All(highlight => highlight(icon));
}