namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewValley.Menus;

/// <summary>Adds a search bar to the top of the <see cref="ItemGrabMenu" />.</summary>
internal sealed class SearchItems : BaseFeature<SearchItems>
{
    private readonly IExpressionHandler expressionHandler;
    private readonly MenuHandler menuHandler;
    private readonly PerScreen<IExpression?> searchExpression = new();

    /// <summary>Initializes a new instance of the <see cref="SearchItems" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public SearchItems(
        IEventManager eventManager,
        IExpressionHandler expressionHandler,
        MenuHandler menuHandler,
        IModConfig modConfig)
        : base(eventManager, modConfig)
    {
        this.expressionHandler = expressionHandler;
        this.menuHandler = menuHandler;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.SearchItems != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Subscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Subscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Unsubscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        if (this.menuHandler.Top?.Container.SearchItems is not FeatureOption.Enabled
            || e.Top is not InventoryMenu inventoryMenu)
        {
            return;
        }

        var columns = inventoryMenu.capacity / inventoryMenu.rows;
        var width = Math.Min(12 * Game1.tileSize, Game1.uiViewport.Width);
        var x = columns switch
        {
            3 => inventoryMenu.inventory[1].bounds.Center.X - (width / 2),
            12 => inventoryMenu.inventory[5].bounds.Right - (width / 2),
            14 => inventoryMenu.inventory[6].bounds.Right - (width / 2),
            _ => (Game1.uiViewport.Width - width) / 2,
        };

        var y = inventoryMenu.yPositionOnScreen
            - (IClickableMenu.borderWidth / 2)
            - Game1.tileSize
            - (inventoryMenu.rows == 3 ? 25 : 4);

        var searchBar = new TextField(x, y, width);

        searchBar.ValueChanged += (_, value) =>
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Log.Trace("{0}: Searching for {1}", this.Id, value);
            }

            this.searchExpression.Value = this.expressionHandler.TryParseExpression(value, out var expression)
                ? expression
                : null;

            this.Events.Publish(new SearchChangedEventArgs(value, expression));
        };

        this.menuHandler.Top.Components.Add(searchBar);
    }

    private void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        if (e.Container.SearchItems is FeatureOption.Enabled && this.searchExpression.Value?.Equals(e.Item) == false)
        {
            e.UnHighlight();
        }
    }

    private void OnItemsDisplaying(ItemsDisplayingEventArgs e)
    {
        if (this.searchExpression.Value is null
            || this.Config.SearchItemsMethod is not (FilterMethod.Sorted
                or FilterMethod.GrayedOut
                or FilterMethod.Hidden))
        {
            return;
        }

        e.Edit(
            items => this.Config.SearchItemsMethod switch
            {
                FilterMethod.Sorted or FilterMethod.GrayedOut => items.OrderByDescending(
                    this.searchExpression.Value.Equals),
                FilterMethod.Hidden => items.Where(this.searchExpression.Value.Equals),
                _ => items,
            });
    }
}