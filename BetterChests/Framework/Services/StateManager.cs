namespace StardewMods.BetterChests.Framework.Services;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;

internal sealed class StateManager
{
    private readonly PerScreen<IClickableMenu?> actualMenu = new();
    private readonly IEventManager eventManager;

    /// <summary>Initializes a new instance of the <see cref="StateManager" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    public StateManager(IEventManager eventManager)
    {
        // Initialize
        this.eventManager = eventManager;

        // Events
        eventManager.Subscribe<UpdateTickingEventArgs>(this.OnUpdateTicking);
        eventManager.Subscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
    }

    /// <summary>Gets the current menu.</summary>
    public IClickableMenu? ActiveMenu
    {
        get => this.actualMenu.Value;
        private set => this.actualMenu.Value = value;
    }

    private static IClickableMenu? GetActualMenu(IClickableMenu? menu) =>
        menu switch
        {
            null => null,
            _ when menu.GetChildMenu() is
                { } childMenu => StateManager.GetActualMenu(childMenu),
            GameMenu gameMenu => StateManager.GetActualMenu(gameMenu.GetCurrentPage()),
            _ => menu,
        };

    private static IClickableMenu? GetBottomMenu(IClickableMenu parentMenu) =>
        parentMenu switch
        {
            InventoryPage inventoryPage => inventoryPage.inventory,
            ItemGrabMenu itemGrabMenu => itemGrabMenu.inventory,
            ShopMenu shopMenu => shopMenu.inventory,
            _ when parentMenu.GetParentMenu() is
                { } parent => StateManager.GetBottomMenu(parent),
            _ => null,
        };

    private static IClickableMenu? GetTopMenu(IClickableMenu parentMenu) =>
        parentMenu switch
        {
            ItemGrabMenu
            {
                showReceivingMenu: true,
            } itemGrabMenu => itemGrabMenu.ItemsToGrabMenu,
            ShopMenu shopMenu => shopMenu,
            _ when parentMenu.GetParentMenu() is
                { } parent => StateManager.GetTopMenu(parent),
            _ => null,
        };

    private void OnUpdateTicked(UpdateTickedEventArgs e) => this.UpdateMenu();

    private void OnUpdateTicking(UpdateTickingEventArgs e) => this.UpdateMenu();

    private void UpdateMenu()
    {
        var menu = StateManager.GetActualMenu(Game1.activeClickableMenu);
        if (menu == this.ActiveMenu)
        {
            return;
        }

        this.ActiveMenu = menu;
        if (menu is null)
        {
            this.eventManager.Publish(new InventoryMenuChangedEventArgs(null, null, null));
            return;
        }

        var topMenu = StateManager.GetTopMenu(menu);
        var bottomMenu = StateManager.GetBottomMenu(menu);
        this.eventManager.Publish(new InventoryMenuChangedEventArgs(menu, topMenu, bottomMenu));
    }
}