namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Menu for sorting options.</summary>
internal sealed class SortMenu : SearchMenu
{
    private readonly IStorageContainer container;
    private readonly ClickableTextureComponent copyButton;
    private readonly ClickableTextureComponent okButton;
    private readonly ClickableTextureComponent pasteButton;
    private readonly ClickableTextureComponent saveButton;
    private readonly IExpression? searchExpression;

    /// <summary>Initializes a new instance of the <see cref="SortMenu" /> class.</summary>
    /// <param name="container">The container to categorize.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    public SortMenu(IStorageContainer container, IExpressionHandler expressionHandler, IIconRegistry iconRegistry)
        : base(expressionHandler, iconRegistry, container.SortInventoryBy)
    {
        this.container = container;
        this.searchExpression =
            expressionHandler.TryParseExpression(container.CategorizeChestSearchTerm, out var expression)
                ? expression
                : null;

        this.RefreshItems();

        this.saveButton = iconRegistry
            .Icon(InternalIcon.Save)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(new Point(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + Game1.tileSize + 16))
            .HoverText(I18n.Ui_Save_Name())
            .Value;

        this.copyButton = iconRegistry
            .Icon(InternalIcon.Copy)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 2)))
            .HoverText(I18n.Ui_Copy_Tooltip())
            .Value;

        this.pasteButton = iconRegistry
            .Icon(InternalIcon.Paste)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 3)))
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
        this.allClickableComponents.Add(this.copyButton);
        this.allClickableComponents.Add(this.pasteButton);
        this.allClickableComponents.Add(this.okButton);
    }

    /// <inheritdoc />
    protected override List<Item> GetItems()
    {
        var items = this.searchExpression is null
            ? ItemRepository.GetItems().ToList()
            : ItemRepository.GetItems(this.searchExpression.Equals).ToList();

        if (this.Expression is not null)
        {
            items.Sort(this.Expression);
        }

        return items;
    }

    /// <inheritdoc />
    protected override bool HighlightMethod(Item item) => true;

    /// <inheritdoc />
    protected override bool TryLeftClick(Point cursor)
    {
        if (this.saveButton.bounds.Contains(cursor) && this.readyToClose())
        {
            Game1.playSound("drumkit6");
            this.container.SortInventoryBy = this.SearchText;
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
            this.SearchText = searchText;
            this.UpdateExpression();
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