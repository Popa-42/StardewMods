#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using StardewMods.FauxCore.Common.Helpers;

#else
namespace StardewMods.Common.UI.Components;

using System.Globalization;
using Microsoft.Xna.Framework;
using StardewMods.Common.Helpers;
#endif

/// <summary>Menu for selecting an item from a list of values.</summary>
/// <typeparam name="TOption">The option type.</typeparam>
internal sealed class OptionSelector<TOption> : BaseComponent
{
    private readonly IEnumerable<TOption> allOptions;
    private readonly GetLabelMethod getLabel;
    private readonly List<HighlightMethod> highlights = [];
    private readonly List<Operation> operations = [];
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
        this.Color = Color.Wheat;
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
            Math.Clamp(textBounds.Max(textBound => textBound.X) + spacing + 8, minWidth, maxWidth),
            textBounds.Take(maxOptions).Sum(textBound => textBound.Y) + 8);

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

    /// <summary>Get the label for an option.</summary>
    /// <param name="option">The option.</param>
    /// <returns>The label text.</returns>
    public string GetLabel(TOption option) => this.getLabel(option);

    /// <summary>Refreshes the icons by applying the operations to them.</summary>
    [MemberNotNull(nameof(OptionSelector<TOption>.options))]
    public void RefreshOptions()
    {
        this.Components.Clear();
        this.options = this.operations.Aggregate(this.allOptions, (current, operation) => operation(current)).ToList();
        foreach (var option in this.options)
        {
            var index = this.Components.Count;
            var component = new ButtonComponent(
                this.Bounds.X + 4,
                this.Bounds.Y + (this.textHeight * index) + 4,
                this.Bounds.Width - 8,
                this.textHeight,
                index.ToString(CultureInfo.InvariantCulture),
                this.GetLabel(option))
            {
                Color = this.Color.Muted(),
                SourceRect = new Rectangle(405, 375, 5, 5),
            };

            component.Clicked += (_, _) => this.CurrentIndex = int.Parse(component.Name, CultureInfo.InvariantCulture);
            component.CursorOver += (_, _) => component.Color = this.Color.Highlight();
            component.CursorOut += (_, _) => component.Color = this.Color.Muted();
            component.Rendering += (_, _) => component.TextColor =
                this.HighlightOption(option) ? Game1.textColor : Game1.unselectedOptionColor;

            this.Components.Add(component);
        }

        this.Overflow = new Point(
            0,
            this.Components.OfType<ICustomComponent>().Any()
                ? this.Components.OfType<ICustomComponent>().Last().Bounds.Bottom
                - this.Bounds.Y
                - this.Bounds.Height
                + 4
                : 0);
    }

    private static string GetDefaultLabel(TOption item) =>
        item switch { string s => s, _ => item?.ToString() ?? string.Empty };

    private bool HighlightOption(TOption option) =>
        !this.highlights.Any() || this.highlights.All(highlight => highlight(option));
}