namespace StardewMods.BetterChests.Framework.UI.Menus;

using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <summary>A menu for assigning categories to a container.</summary>
internal sealed class CategorizeMenu : SearchMenu
{
    private readonly IStorageContainer container;
    private readonly TextureComponent stackButtonOff;
    private readonly TextureComponent stackButtonOn;

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

        var saveButton = iconRegistry
            .Icon(InternalIcon.Save)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(new Point(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + Game1.tileSize + 16))
            .HoverText(I18n.Ui_Save_Name())
            .Value;

        this.stackButtonOff = iconRegistry
            .Icon(InternalIcon.NoStack)
            .Component(IconStyle.Button)
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

        var copyButton = iconRegistry
            .Icon(InternalIcon.Copy)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 3)))
            .HoverText(I18n.Ui_Copy_Tooltip())
            .Value;

        var pasteButton = iconRegistry
            .Icon(InternalIcon.Paste)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 4)))
            .HoverText(I18n.Ui_Paste_Tooltip())
            .Value;

        var okButton = iconRegistry
            .Icon(VanillaIcon.Ok)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + this.height - Game1.tileSize - (IClickableMenu.borderWidth / 2)))
            .Value;

        saveButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            if (!this.readyToClose())
            {
                return;
            }

            this.savedExpression = this.Expression.DeepClone();
            this.container.CategorizeChestSearchTerm = this.SearchText;
            this.container.CategorizeChestIncludeStacks = this.stackButtonOn.visible
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        };

        okButton.Clicked += (_, _) =>
        {
            Game1.playSound("bigDeSelect");
            this.exitThisMenuNoSound();
            this.container.ShowMenu();
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

        this.stackButtonOff.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            this.stackButtonOff.IsVisible = false;
            this.stackButtonOn.IsVisible = true;
        };

        this.stackButtonOn.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            this.stackButtonOn.IsVisible = false;
            this.stackButtonOff.IsVisible = true;
        };

        this.Components.Add(saveButton);
        this.Components.Add(this.stackButtonOff);
        this.Components.Add(this.stackButtonOn);
        this.Components.Add(copyButton);
        this.Components.Add(pasteButton);
        this.Components.Add(okButton);
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
}