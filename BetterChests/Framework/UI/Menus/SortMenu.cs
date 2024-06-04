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
    private readonly IExpression? searchExpression;

    /// <summary>Initializes a new instance of the <see cref="SortMenu" /> class.</summary>
    /// <param name="container">The container to categorize.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    public SortMenu(IStorageContainer container, IExpressionHandler expressionHandler, IIconRegistry iconRegistry)
        : base(expressionHandler, iconRegistry, container.SortInventoryBy)
    {
        this.searchExpression =
            expressionHandler.TryParseExpression(container.CategorizeChestSearchTerm, out var expression)
                ? expression
                : null;

        this.RefreshItems();

        var saveButton = iconRegistry
            .Icon(InternalIcon.Save)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(new Point(this.Bounds.Right + 4, this.Bounds.Y + Game1.tileSize + 16))
            .HoverText(I18n.Ui_Save_Name())
            .Value;

        var copyButton = iconRegistry
            .Icon(InternalIcon.Copy)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(new Point(this.Bounds.Right + 4, this.Bounds.Y + ((Game1.tileSize + 16) * 2)))
            .HoverText(I18n.Ui_Copy_Tooltip())
            .Value;

        var pasteButton = iconRegistry
            .Icon(InternalIcon.Paste)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(new Point(this.Bounds.Right + 4, this.Bounds.Y + ((Game1.tileSize + 16) * 3)))
            .HoverText(I18n.Ui_Paste_Tooltip())
            .Value;

        var okButton = iconRegistry
            .Icon(VanillaIcon.Ok)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(
                new Point(
                    this.Bounds.Right + 4,
                    this.Bounds.Bottom - Game1.tileSize - (IClickableMenu.borderWidth / 2)))
            .Value;

        okButton.Clicked += (_, _) =>
        {
            Game1.playSound("bigDeSelect");
            this.exitThisMenuNoSound();
            container.ShowMenu();
        };

        saveButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            container.SortInventoryBy = this.SearchText;
        };

        copyButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            DesktopClipboard.SetText(this.SearchText);
        };

        pasteButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            var searchText = string.Empty;
            DesktopClipboard.GetText(ref searchText);
            this.SearchText = searchText;
            this.UpdateExpression();
        };

        this.Components.Add(saveButton);
        this.Components.Add(copyButton);
        this.Components.Add(pasteButton);
        this.Components.Add(okButton);
    }

    /// <inheritdoc />
    protected override IEnumerable<Item> GetItems(IEnumerable<Item> items)
    {
        var itemsToReturn = this.searchExpression is null
            ? ItemRepository.GetItems().ToList()
            : ItemRepository.GetItems(this.searchExpression.Equals).ToList();

        if (this.Expression is not null)
        {
            itemsToReturn.Sort(this.Expression);
        }

        return itemsToReturn;
    }

    /// <inheritdoc />
    protected override bool HighlightMethod(Item item) => true;
}