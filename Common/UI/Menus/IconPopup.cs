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

/// <summary>Popup menu for selecting an icon.</summary>
internal sealed class IconPopup : BaseMenu
{
    private readonly IconSelector iconSelector;
    private readonly List<string> sources;
    private readonly TextField textField;

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

        this.iconSelector.AddHighlight(this.HighlightIcon);
        this.iconSelector.AddOperation(this.SortIcons);
        this.Components.Add(this.iconSelector);

        this.Size = new Point(this.iconSelector.Bounds.Width + spacing, this.iconSelector.Bounds.Height + spacing);
        this.Location = new Point(
            ((Game1.uiViewport.Width - this.Bounds.Width) / 2) + IClickableMenu.borderWidth,
            ((Game1.uiViewport.Height - this.Bounds.Height) / 2) + IClickableMenu.borderWidth);

        this.textField =
            new TextField(this.xPositionOnScreen - 12, this.yPositionOnScreen, this.width, string.Empty)
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

        var dropdownButton = iconRegistry
            .Icon(VanillaIcon.Dropdown)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(new Point(this.xPositionOnScreen + this.width - 16, this.yPositionOnScreen))
            .Value;

        // Should this update textField.Value to icon.HoverText?
        // this.iconSelector.SelectionChanged += (_, icon) => this.iconSelected?.InvokeAll(this, icon);
        this.textField.ValueChanged += (_, _) => this.iconSelector.RefreshIcons();

        okButton.Clicked += (_, _) =>
        {
            if (this.iconSelector.CurrentSelection is not null)
            {
                this.iconSelected?.InvokeAll(this, this.iconSelector.CurrentSelection);
            }

            this.exitThisMenuNoSound();
        };

        cancelButton.Clicked += (_, _) => this.exitThisMenuNoSound();
        dropdownButton.Clicked += (_, _) =>
        {
            var dropdown = new OptionDropdown<string>(this.textField, this.sources, this.Bounds.Width, maxOptions: 10);
            dropdown.OptionSelected += (_, value) => this.textField.Value = value ?? this.textField.Value;
            this.SetChildMenu(dropdown);
        };

        // Add scrollbar
        this.Components.Add(this.textField);
        this.Components.Add(okButton);
        this.Components.Add(cancelButton);
        this.Components.Add(dropdownButton);
    }

    /// <summary>Event raised when the selection changes.</summary>
    public event EventHandler<IIcon?> IconSelected
    {
        add => this.iconSelected += value;
        remove => this.iconSelected -= value;
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
            case Keys.Tab when this.textField.Selected && !string.IsNullOrWhiteSpace(this.textField.Value):
                this.textField.Value = this.sources.FirstOrDefault(
                        source => source.Contains(this.textField.Value, StringComparison.OrdinalIgnoreCase))
                    ?? this.textField.Value;

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
        icon.Source.Contains(this.textField.Value, StringComparison.OrdinalIgnoreCase)
        || this.iconSelector.GetHoverText(icon).Contains(this.textField.Value, StringComparison.OrdinalIgnoreCase);

    private IEnumerable<IIcon> SortIcons(IEnumerable<IIcon> icons) =>
        icons.OrderByDescending(this.HighlightIcon).ThenBy(this.iconSelector.GetHoverText);
}