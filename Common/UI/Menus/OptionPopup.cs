#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Common.UI.Components;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;
#endif

/// <summary>Dropdown menu with option selector.</summary>
/// <typeparam name="TOption">The option type.</typeparam>
internal sealed class OptionPopup<TOption> : BaseMenu
{
    private readonly OptionSelector<TOption> optionSelector;
    private readonly TextField textField;

    private EventHandler<TOption?>? optionSelected;

    /// <summary>Initializes a new instance of the <see cref="OptionPopup{TOption}" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="initialText">The initial value of the text field.</param>
    /// <param name="options">The options to pick from.</param>
    /// <param name="maxOptions">The maximum number of items to display.</param>
    /// <param name="getLabel">A function which returns the label of an item.</param>
    /// <param name="spacing">The spacing between options.</param>
    public OptionPopup(
        IIconRegistry iconRegistry,
        string? initialText,
        IEnumerable<TOption> options,
        int maxOptions = int.MaxValue,
        OptionSelector<TOption>.GetLabelMethod? getLabel = null,
        int spacing = 16)
        : base(width: 400)
    {
        this.optionSelector = new OptionSelector<TOption>(
            options,
            400,
            400,
            maxOptions,
            getLabel,
            spacing,
            this.Bounds.X,
            this.Bounds.Y);

        this.optionSelector.AddHighlight(this.HighlightOption);
        this.optionSelector.AddOperation(this.SortOptions);
        this.Components.Add(this.optionSelector);

        this.Size = new Point(this.optionSelector.Bounds.Width + spacing, this.optionSelector.Bounds.Height + spacing);
        this.Location = new Point(
            ((Game1.uiViewport.Width - this.Bounds.Width) / 2) + IClickableMenu.borderWidth,
            ((Game1.uiViewport.Height - this.Bounds.Height) / 2) + IClickableMenu.borderWidth);

        this.textField = new TextField(
            this.Bounds.X - 12,
            this.Bounds.Y,
            this.Bounds.Width,
            initialText ?? string.Empty)
        {
            Selected = true,
        };

        var okButton = iconRegistry
            .Icon(VanillaIcon.Ok)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + ((this.width - IClickableMenu.borderWidth) / 2) - Game1.tileSize,
                    this.yPositionOnScreen + this.height + Game1.tileSize))
            .Value;

        var cancelButton = iconRegistry
            .Icon(VanillaIcon.Cancel)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + ((this.width + IClickableMenu.borderWidth) / 2),
                    this.yPositionOnScreen + this.height + Game1.tileSize))
            .Value;

        this.optionSelector.SelectionChanged += (_, option) =>
            this.textField.Value = option is not null ? this.optionSelector.GetLabel(option) : string.Empty;

        this.textField.ValueChanged += (_, _) => this.optionSelector.RefreshOptions();

        okButton.Clicked += (_, _) =>
        {
            if (this.optionSelector.CurrentSelection is not null)
            {
                this.optionSelected?.InvokeAll(this, this.optionSelector.CurrentSelection);
            }

            this.exitThisMenuNoSound();
        };

        cancelButton.Clicked += (_, _) => this.exitThisMenuNoSound();

        // Add scrollbar
        this.Components.Add(this.textField);
        this.Components.Add(okButton);
        this.Components.Add(cancelButton);
    }

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<TOption?> OptionSelected
    {
        add => this.optionSelected += value;
        remove => this.optionSelected -= value;
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        switch (key)
        {
            case Keys.Escape when this.readyToClose():
                this.exitThisMenuNoSound();
                return;
            case Keys.Enter when this.readyToClose() && this.optionSelector.CurrentSelection is not null:
                this.optionSelected?.InvokeAll(this, this.optionSelector.CurrentSelection);
                this.exitThisMenuNoSound();
                return;
            case Keys.Tab when this.textField.Selected
                && !string.IsNullOrWhiteSpace(this.textField.Value)
                && this.optionSelector.Options.Any():
                this.textField.Value = this.optionSelector.GetLabel(this.optionSelector.Options[0]);
                break;
        }
    }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch spriteBatch, Point cursor) =>
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.5f);

    private bool HighlightOption(TOption option) =>
        this.optionSelector.GetLabel(option).Contains(this.textField.Value, StringComparison.OrdinalIgnoreCase);

    private IEnumerable<TOption> SortOptions(IEnumerable<TOption> options) =>
        options.OrderByDescending(this.HighlightOption).ThenBy(this.optionSelector.GetLabel);
}