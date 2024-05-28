namespace StardewMods.BetterChests.Framework.UI.Menus;

using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>A menu for assigning categories to a container.</summary>
internal sealed class CategorizeMenu : SearchMenu
{
    private readonly IStorageContainer container;
    private readonly ClickableTextureComponent copyButton;
    private readonly ClickableTextureComponent okButton;
    private readonly ClickableTextureComponent pasteButton;
    private readonly ClickableTextureComponent saveButton;
    private readonly ClickableTextureComponent stackButtonOff;
    private readonly ClickableTextureComponent stackButtonOn;

    private IExpression? savedExpression;

    /// <summary>Initializes a new instance of the <see cref="CategorizeMenu" /> class.</summary>
    /// <param name="container">The container to categorize.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    public CategorizeMenu(IStorageContainer container, IExpressionHandler expressionHandler, IIconRegistry iconRegistry)
        : base(expressionHandler, iconRegistry, container.CategorizeChestSearchTerm)
    {
        this.container = container;
        this.savedExpression =
            expressionHandler.TryParseExpression(container.CategorizeChestSearchTerm, out var expression)
                ? expression
                : null;

        this.saveButton = iconRegistry
            .Icon(InternalIcon.Save)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(new Point(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + Game1.tileSize + 16))
            .HoverText(I18n.Ui_Save_Name())
            .Value;

        this.stackButtonOff = iconRegistry
            .Icon(InternalIcon.NoStack)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 2)))
            .HoverText(I18n.Button_IncludeExistingStacks_Name())
            .IsVisible(this.container.CategorizeChestIncludeStacks is not FeatureOption.Enabled)
            .Value;

        this.stackButtonOn = iconRegistry
            .Icon(VanillaIcon.Stack)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 2)))
            .HoverText(I18n.Button_IncludeExistingStacks_Name())
            .IsVisible(this.container.CategorizeChestIncludeStacks is FeatureOption.Enabled)
            .Value;

        this.copyButton = iconRegistry
            .Icon(InternalIcon.Copy)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 3)))
            .HoverText(I18n.Ui_Copy_Tooltip())
            .Value;

        this.pasteButton = iconRegistry
            .Icon(InternalIcon.Paste)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 4)))
            .HoverText(I18n.Ui_Paste_Tooltip())
            .Value;

        this.okButton = iconRegistry
            .Icon(VanillaIcon.Ok)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + this.height - Game1.tileSize - (IClickableMenu.borderWidth / 2)))
            .Value;

        this.allClickableComponents.Add(this.saveButton);
        this.allClickableComponents.Add(this.stackButtonOff);
        this.allClickableComponents.Add(this.stackButtonOn);
        this.allClickableComponents.Add(this.copyButton);
        this.allClickableComponents.Add(this.pasteButton);
        this.allClickableComponents.Add(this.okButton);
    }

    /// <inheritdoc />
    protected override List<Item> GetItems()
    {
        var items = base.GetItems();
        return items;
    }

    /// <inheritdoc />
    protected override bool HighlightMethod(Item item) =>
        this.savedExpression is null || this.savedExpression.Equals(item);

    /// <inheritdoc />
    protected override bool TryLeftClick(Point cursor)
    {
        if (this.saveButton.bounds.Contains(cursor) && this.readyToClose())
        {
            Game1.playSound("drumkit6");
            this.savedExpression = this.Expression.DeepClone();
            this.container.CategorizeChestSearchTerm = this.SearchText;
            this.container.CategorizeChestIncludeStacks = this.stackButtonOn.visible
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;

            return true;
        }

        if (this.stackButtonOn.bounds.Contains(cursor))
        {
            Game1.playSound("drumkit6");
            (this.stackButtonOn.visible, this.stackButtonOff.visible) =
                (this.stackButtonOff.visible, this.stackButtonOn.visible);

            return true;
        }

        if (this.copyButton.bounds.Contains(cursor))
        {
            Game1.playSound("drumkit6");
            DesktopClipboard.SetText(this.SearchText);
            return true;
        }

        if (this.pasteButton.bounds.Contains(cursor))
        {
            Game1.playSound("drumkit6");
            var searchText = string.Empty;
            DesktopClipboard.GetText(ref searchText);
            this.SetSearchText(searchText, true);
            return true;
        }

        if (this.okButton.bounds.Contains(cursor))
        {
            Game1.playSound("bigDeSelect");
            this.exitThisMenuNoSound();
            this.container.ShowMenu();
            return true;
        }

        return false;
    }
}