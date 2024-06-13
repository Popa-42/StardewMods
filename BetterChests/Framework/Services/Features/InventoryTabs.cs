namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Adds inventory tabs to the side of the <see cref="ItemGrabMenu" />.</summary>
internal sealed class InventoryTabs : BaseFeature<InventoryTabs>
{
    private readonly IExpressionHandler expressionHandler;
    private readonly IIconRegistry iconRegistry;
    private readonly MenuHandler menuHandler;

    /// <summary>Initializes a new instance of the <see cref="InventoryTabs" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public InventoryTabs(
        IEventManager eventManager,
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        MenuHandler menuHandler,
        IModConfig modConfig)
        : base(eventManager, modConfig)
    {
        this.expressionHandler = expressionHandler;
        this.iconRegistry = iconRegistry;
        this.menuHandler = menuHandler;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.InventoryTabs != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate() =>
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);

    /// <inheritdoc />
    protected override void Deactivate() =>
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        if (this.menuHandler.Top?.Menu is not InventoryMenu inventoryMenu
            || this.menuHandler.Top.Container.InventoryTabs is not FeatureOption.Enabled)
        {
            return;
        }

        var x = Math.Min(
                this.menuHandler.Top.Menu.xPositionOnScreen,
                this.menuHandler.Bottom?.Menu.xPositionOnScreen ?? int.MaxValue)
            - Game1.tileSize
            - IClickableMenu.borderWidth;

        var y = inventoryMenu.inventory[0].bounds.Y;

        foreach (var tabData in this.Config.InventoryTabList)
        {
            if (!this.iconRegistry.TryGetIcon(tabData.Icon, out var icon))
            {
                continue;
            }

            var tabIcon = new InventoryTab(x, y, icon, tabData);
            tabIcon.Clicked += (sender, e) =>
            {
                e.PreventDefault();
                Log.Trace("{0}: Switching tab to {1}.", this.Id, tabIcon.Data.Label);
                Game1.playSound("drumkit6");
                _ = this.expressionHandler.TryParseExpression(tabIcon.Data.SearchTerm, out var expression);
                this.Events.Publish(new SearchChangedEventArgs(tabIcon.Data.SearchTerm, expression));
            };

            this.menuHandler.Top.Components.Add(tabIcon);

            y += Game1.tileSize;
        }
    }
}