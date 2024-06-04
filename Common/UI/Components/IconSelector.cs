#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;

#else
namespace StardewMods.Common.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
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
            columns * ((int)(scale * 16) + spacing),
            rows * ((int)(scale * 16) + spacing),
            "selectIcon")
    {
        this.Color = Color.Wheat;
        this.getHoverText = getHoverText ?? IconSelector.GetUniqueId;
        this.allIcons = allIcons;
        this.columns = columns;
        this.scale = scale;
        this.spacing = spacing;
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
    public void AddHighlight(HighlightMethod highlight) => this.highlights.Add(highlight);

    /// <summary>Add an operation that will be applied to the icons.</summary>
    /// <param name="operation">The operation to perform.</param>
    public void AddOperation(Operation operation) => this.operations.Add(operation);

    /// <summary>Get the hover text for an icon.</summary>
    /// <param name="icon">The icon.</param>
    /// <returns>The hover text.</returns>
    public string GetHoverText(IIcon icon) => this.getHoverText(icon);

    /// <summary>Refreshes the icons by applying the operations to them.</summary>
    [MemberNotNull(nameof(IconSelector.icons))]
    public void RefreshIcons()
    {
        this.Components.Clear();
        this.icons = this.operations.Aggregate(this.allIcons, (current, operation) => operation(current)).ToList();
        foreach (var icon in this.icons)
        {
            var index = this.Components.Count;
            var component = icon
                .Component(IconStyle.Transparent, index.ToString(CultureInfo.InvariantCulture), this.scale)
                .AsBuilder()
                .Location(
                    this.Bounds.X + (index % this.columns * (this.length + this.spacing)) + (this.spacing / 2),
                    this.Bounds.Y + (index / this.columns * (this.length + this.spacing)) + (this.spacing / 2))
                .HoverText(this.GetHoverText(icon))
                .Value;

            component.Location += new Point(
                (int)((this.length - (icon.Area.Width * component.Scale)) / 2f),
                (int)((this.length - (icon.Area.Height * component.Scale)) / 2f));

            component.Clicked += (_, _) => this.CurrentIndex = int.Parse(component.Name, CultureInfo.InvariantCulture);
            component.Rendering += (_, _) =>
                component.Color = this.HighlightIcon(icon) ? Color.White : Color.White * 0.25f;

            this.Components.Add(component);
        }

        this.Overflow = new Point(
            0,
            this.Components.OfType<ICustomComponent>().Any()
                ? this.Components.OfType<ICustomComponent>().Last().Bounds.Bottom
                - this.Bounds.Y
                - this.Bounds.Height
                + this.spacing
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
                    this.bounds.X + (this.CurrentIndex % this.columns * (this.length + this.spacing)) - this.Offset.X,
                    this.bounds.Y + (this.CurrentIndex / this.columns * (this.length + this.spacing)) - this.Offset.Y,
                    this.length + this.spacing,
                    this.length + this.spacing),
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