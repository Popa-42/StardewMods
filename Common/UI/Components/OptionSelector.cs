#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FauxCore.Common.Helpers;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewValley.Menus;
#endif

/// <summary>Menu for selecting an item from a list of values.</summary>
/// <typeparam name="TOption">The option type.</typeparam>
internal sealed class OptionSelector<TOption> : BaseComponent
{
    private readonly IEnumerable<TOption> allOptions;
    private readonly GetLabelMethod getLabel;
    private readonly List<HighlightMethod> highlights = [];
    private readonly List<Operation> operations = [];
    private readonly int spacing;
    private readonly int textHeight;

    private int currentIndex = -1;
    private List<TOption> options;
    private EventHandler<TOption?>? selectionChanged;

    /// <summary>Initializes a new instance of the <see cref="OptionSelector{TOption}" /> class.</summary>
    /// <param name="allOptions">The options to pick from.</param>
    /// <param name="minWidth">The minimum width.</param>
    /// <param name="maxWidth">The maximum width.</param>
    /// <param name="maxOptions">The maximum number of options to display.</param>
    /// <param name="getLabel">A function which returns the label of an option.</param>
    /// <param name="spacing">The spacing between options.</param>
    /// <param name="x">The x-position.</param>
    /// <param name="y">The y-position.</param>
    public OptionSelector(
        IEnumerable<TOption> allOptions,
        int minWidth = 0,
        int maxWidth = int.MaxValue,
        int maxOptions = int.MaxValue,
        GetLabelMethod? getLabel = null,
        int spacing = 16,
        int? x = null,
        int? y = null)
        : base(x ?? 0, y ?? 0, 800, 600, "selectOption")
    {
        this.spacing = spacing;
        this.getLabel = getLabel ?? OptionSelector<TOption>.GetDefaultLabel;
        this.allOptions = allOptions
            .Where(
                option => this.GetLabel(option).Trim().Length >= 3
                    && !this.GetLabel(option).StartsWith("id_", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var textBounds =
            this.allOptions.Select(option => Game1.smallFont.MeasureString(this.GetLabel(option)).ToPoint()).ToList();

        this.textHeight = textBounds.Max(textBound => textBound.Y);
        this.Size = new Point(
            Math.Clamp(textBounds.Max(textBound => textBound.X) + this.spacing, minWidth, maxWidth),
            textBounds.Take(maxOptions).Sum(textBound => textBound.Y) + 16);

        this.RefreshOptions();
    }

    /// <summary>Get the label for an option.</summary>
    /// <param name="option">The option.</param>
    /// <returns>The label text.</returns>
    public delegate string GetLabelMethod(TOption option);

    /// <summary>Highlight an option.</summary>
    /// <param name="option">The option to highlight.</param>
    /// <returns><c>true</c> if the option should be highlighted; otherwise, <c>false</c>.</returns>
    public delegate bool HighlightMethod(TOption option);

    /// <summary>An operation to perform on the options.</summary>
    /// <param name="options">The original options.</param>
    /// <returns>Returns a modified list of options.</returns>
    public delegate IEnumerable<TOption> Operation(IEnumerable<TOption> options);

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<TOption?> SelectionChanged
    {
        add => this.selectionChanged += value;
        remove => this.selectionChanged -= value;
    }

    /// <summary>Gets the currently selected icon.</summary>
    public TOption? CurrentSelection => this.options.ElementAtOrDefault(this.CurrentIndex);

    /// <summary>Gets the options.</summary>
    public IReadOnlyList<TOption> Options => this.options;

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

    /// <summary>Get the label for an option.</summary>
    /// <param name="option">The option.</param>
    /// <returns>The label text.</returns>
    public string GetLabel(TOption option) => this.getLabel(option);

    /// <summary>Refreshes the icons by applying the operations to them.</summary>
    [MemberNotNull(nameof(OptionSelector<TOption>.options))]
    public void RefreshOptions()
    {
        this.ClearComponents();
        this.options = this.operations.Aggregate(this.allOptions, (current, operation) => operation(current)).ToList();

        foreach (var option in this.options)
        {
            var index = this.Components.Count;
            var component = new ButtonComponent(
                this.Bounds.X + 8,
                this.Bounds.Y + (this.textHeight * index),
                this.Bounds.Width,
                this.textHeight,
                index.ToString(CultureInfo.InvariantCulture),
                this.GetLabel(option));

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
                if (sender is not ButtonComponent buttonComponent)
                {
                    return;
                }

                buttonComponent.TextColor =
                    this.HighlightOption(option) ? Game1.textColor : Game1.unselectedOptionColor;
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
        var currentComponent = this.Components.ElementAtOrDefault(this.CurrentIndex);
        if (currentComponent is not null)
        {
            spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(
                    currentComponent.Bounds.X + this.Offset.X,
                    currentComponent.Bounds.Y + this.Offset.Y,
                    currentComponent.Bounds.Width - this.spacing,
                    currentComponent.Bounds.Height),
                new Rectangle(0, 0, 1, 1),
                Color.Wheat,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0.975f);
        }

        foreach (var component in this.Components)
        {
            var index = int.Parse(component.Name, CultureInfo.InvariantCulture);
            spriteBatch.DrawString(
                Game1.smallFont,
                component.Label,
                component.Bounds.Location.ToVector2() + this.Offset.ToVector2(),
                this.HighlightOption(this.options[index]) ? Game1.textColor : Game1.unselectedOptionColor);
        }
    }

    private static string GetDefaultLabel(TOption item) =>
        item switch { string s => s, _ => item?.ToString() ?? string.Empty };

    private bool HighlightOption(TOption option) =>
        !this.highlights.Any() || this.highlights.All(highlight => highlight(option));
}