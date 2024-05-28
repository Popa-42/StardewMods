#if IS_FAUXCORE
namespace StardewMods.FauxCore.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.FauxCore.Common.Helpers;
using StardewMods.FauxCore.Common.Models.Events;
using StardewMods.FauxCore.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Common.UI.Components;
using StardewValley.Menus;

#else
namespace StardewMods.Common.UI.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;
#endif

/// <summary>Popup menu for selecting an icon.</summary>
internal sealed class IconPopup : BaseMenu
{
    private readonly IconSelector iconSelector;
    private readonly List<string> sources;
    private readonly TextField textField;

    private string currentText;
    private EventHandler<IIcon?>? iconSelected;

    /// <summary>Initializes a new instance of the <see cref="IconPopup" /> class.</summary>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="getHoverText">A function which returns the icon hover text.</param>
    /// <param name="scale">The icon scale.</param>
    /// <param name="spacing">The spacing between icons.</param>
    public IconPopup(
        IIconRegistry iconRegistry,
        IconSelector.GetHoverTextMethod? getHoverText = null,
        float scale = 3f,
        int spacing = 8)
    {
        var icons = iconRegistry.GetIcons().ToList();
        this.sources = icons.Select(icon => icon.Source).Distinct().ToList();
        this.sources.Sort();

        this.iconSelector = new IconSelector(
            icons,
            5,
            5,
            getHoverText,
            scale,
            spacing,
            this.Bounds.X,
            this.Bounds.Y + 48);

        this.iconSelector.SelectionChanged += (_, icon) => this.iconSelected?.InvokeAll(this, icon);
        this.iconSelector.AddHighlight(this.HighlightIcon);
        this.iconSelector.AddOperation(this.SortIcons);
        this.AddComponent(this.iconSelector);

        this.Size = new Point(this.iconSelector.Bounds.Width + spacing, this.iconSelector.Bounds.Height + spacing);
        this.Location = new Point(
            ((Game1.uiViewport.Width - this.Bounds.Width) / 2) + IClickableMenu.borderWidth,
            ((Game1.uiViewport.Height - this.Bounds.Height) / 2) + IClickableMenu.borderWidth);

        this.currentText = string.Empty;
        this.textField = new TextField(
            this.xPositionOnScreen - 12,
            this.yPositionOnScreen,
            this.width,
            () => this.CurrentText,
            value => this.CurrentText = value)
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

        okButton.Clicked += this.OnConfirm;

        var cancelButton = iconRegistry
            .Icon(VanillaIcon.Cancel)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + ((this.width + IClickableMenu.borderWidth) / 2),
                    this.yPositionOnScreen + this.height + Game1.tileSize))
            .Value;

        cancelButton.Clicked += this.OnCancel;

        var dropdown = iconRegistry
            .Icon(VanillaIcon.Dropdown)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(new Point(this.xPositionOnScreen + this.width - 16, this.yPositionOnScreen))
            .Value;

        dropdown.Clicked += this.OnDropdown;

        // Add scrollbar
        this.AddComponent(this.textField);
        this.AddComponent(okButton);
        this.AddComponent(cancelButton);
        this.AddComponent(dropdown);
    }

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<IIcon?> IconSelected
    {
        add => this.iconSelected += value;
        remove => this.iconSelected -= value;
    }

    /// <summary>Gets or sets the current text.</summary>
    public string CurrentText
    {
        get => this.currentText;
        set
        {
            if (this.currentText.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                this.currentText = value;
                return;
            }

            this.currentText = value;
            this.iconSelector.RefreshIcons();
        }
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        switch (key)
        {
            case Keys.Escape when this.readyToClose():
                this.exitThisMenuNoSound();
                return;
            case Keys.Enter when this.readyToClose() && this.iconSelector.CurrentSelection is not null:
                this.iconSelected?.InvokeAll(this, this.iconSelector.CurrentSelection);
                this.exitThisMenuNoSound();
                return;
            case Keys.Tab when this.textField.Selected && !string.IsNullOrWhiteSpace(this.CurrentText):
                this.CurrentText =
                    this.sources.FirstOrDefault(
                        source => source.Contains(this.CurrentText, StringComparison.OrdinalIgnoreCase))
                    ?? this.CurrentText;

                this.textField.Reset();
                break;
        }
    }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch spriteBatch, Point cursor) =>
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.5f);

    private bool HighlightIcon(IIcon icon) =>
        icon.Source.Contains(this.CurrentText, StringComparison.OrdinalIgnoreCase)
        || this.iconSelector.GetHoverText(icon).Contains(this.CurrentText, StringComparison.OrdinalIgnoreCase);

    private void OnCancel(object? sender, UiEventArgs e) => this.exitThisMenuNoSound();

    private void OnConfirm(object? sender, UiEventArgs e)
    {
        if (this.iconSelector.CurrentSelection is not null)
        {
            this.iconSelected?.InvokeAll(this, this.iconSelector.CurrentSelection);
        }

        this.exitThisMenuNoSound();
    }

    private void OnDropdown(object? sender, UiEventArgs e)
    {
        var dropdown = new OptionDropdown<string>(this.textField, this.sources, this.Bounds.Width, maxOptions: 10);

        dropdown.OptionSelected += (_, value) =>
        {
            this.CurrentText = value ?? this.CurrentText;
            this.textField.Reset();
        };

        this.SetChildMenu(dropdown);
    }

    private IEnumerable<IIcon> SortIcons(IEnumerable<IIcon> icons) =>
        icons.OrderByDescending(this.HighlightIcon).ThenBy(this.iconSelector.GetHoverText);
}