namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Globalization;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>Locks items in inventory so they cannot be stashed.</summary>
internal sealed class LockItem : BaseFeature<LockItem>
{
    private readonly IInputHelper inputHelper;
    private readonly MenuHandler menuHandler;
    private readonly StateManager stateManager;

    /// <summary>Initializes a new instance of the <see cref="LockItem" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="stateManager">Dependency used for managing state.</param>
    public LockItem(
        IEventManager eventManager,
        IInputHelper inputHelper,
        MenuHandler menuHandler,
        IModConfig modConfig,
        StateManager stateManager)
        : base(eventManager, modConfig)
    {
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;
        this.stateManager = stateManager;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.LockItem != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
        this.Events.Subscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
    }

    private bool IsUnlocked(IHaveModData item) =>
        !item.modData.TryGetValue(this.UniqueId, out var locked) || locked != "Locked";

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!this.Config.LockItemHold || e.Button is not SButton.MouseLeft || !this.Config.Controls.LockSlot.IsDown())
        {
            return;
        }

        var cursor = e.Cursor.GetScaledScreenPixels().ToPoint();
        if (!this.TryGetMenu(cursor, out var inventoryMenu))
        {
            return;
        }

        var slot = inventoryMenu.inventory.FirstOrDefault(slot => slot.bounds.Contains(cursor));
        var index = slot?.name.GetInt(-1) ?? -1;
        if (index == -1)
        {
            return;
        }

        var item = inventoryMenu.actualInventory.ElementAtOrDefault(index);
        if (item is null)
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        this.ToggleLock(item);
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (this.Config.LockItemHold || !this.Config.Controls.LockSlot.JustPressed())
        {
            return;
        }

        var cursor = e.Cursor.GetScaledScreenPixels().ToPoint();
        if (!this.TryGetMenu(cursor, out var inventoryMenu))
        {
            return;
        }

        var slot = inventoryMenu.inventory.FirstOrDefault(slot => slot.bounds.Contains(cursor));
        var index = slot?.name.GetInt(-1) ?? -1;
        if (index == -1)
        {
            return;
        }

        var item = inventoryMenu.actualInventory.ElementAtOrDefault(index);
        if (item is null)
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.LockSlot);
        this.ToggleLock(item);
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        if (this.menuHandler.Top is not null && e.Top is InventoryMenu topMenu)
        {
            this.SetupLockedItems(this.menuHandler.Top, topMenu);
        }

        if (this.menuHandler.Bottom is not null && e.Bottom is InventoryMenu bottomMenu)
        {
            this.SetupLockedItems(this.menuHandler.Bottom, bottomMenu);
        }
    }

    private void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        if (this.stateManager.ActiveMenu is not InventoryPage && !this.IsUnlocked(e.Item))
        {
            e.UnHighlight();
        }
    }

    [Priority(int.MaxValue)]
    private void OnItemTransferring(ItemTransferringEventArgs e)
    {
        if (!this.IsUnlocked(e.Item))
        {
            e.PreventTransfer();
        }
    }

    private void SetupLockedItems(BaseMenu overlay, InventoryMenu inventoryMenu)
    {
        for (var index = 0; index < inventoryMenu.inventory.Count; index++)
        {
            var slot = inventoryMenu.inventory[index];
            var component = new TextureComponent(
                index.ToString(CultureInfo.InvariantCulture),
                new Rectangle(slot.bounds.X + 6, slot.bounds.Y + 6, 14, 16),
                Game1.mouseCursors,
                new Rectangle(107, 442, 7, 8),
                2f);

            component.Rendering += (_, _) =>
            {
                var item = inventoryMenu.actualInventory.ElementAtOrDefault(slot.name.GetInt(-1));
                component.Color = Color.White * (item is not null && !this.IsUnlocked(item) ? 1f : 0f);
            };

            overlay.Components.Add(component);
        }
    }

    private void ToggleLock(Item item)
    {
        if (this.IsUnlocked(item))
        {
            Log.Info("{0}: Locking item {1}", this.Id, item.DisplayName);
            item.modData[this.UniqueId] = "Locked";
        }
        else
        {
            Log.Info("{0}: Unlocking item {1}", this.Id, item.DisplayName);
            item.modData.Remove(this.UniqueId);
        }
    }

    private bool TryGetMenu(Point cursor, [NotNullWhen(true)] out InventoryMenu? inventoryMenu)
    {
        if (this.menuHandler.Top?.Menu is InventoryMenu topMenu && topMenu.isWithinBounds(cursor.X, cursor.Y))
        {
            inventoryMenu = topMenu;
            return true;
        }

        if (this.menuHandler.Bottom?.Menu is InventoryMenu bottomMenu && bottomMenu.isWithinBounds(cursor.X, cursor.Y))
        {
            inventoryMenu = bottomMenu;
            return true;
        }

        inventoryMenu = null;
        return false;
    }
}