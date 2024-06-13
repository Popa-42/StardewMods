namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.UI.Overlays;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>Configure storages individually.</summary>
internal sealed class ConfigureChest : BaseFeature<ConfigureChest>
{
    private readonly ConfigManager configManager;
    private readonly ContainerFactory containerFactory;
    private readonly ContainerHandler containerHandler;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IIconRegistry iconRegistry;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<IStorageContainer?> lastContainer = new();
    private readonly MenuFactory menuFactory;
    private readonly MenuHandler menuHandler;
    private readonly StateManager stateManager;

    /// <summary>Initializes a new instance of the <see cref="ConfigureChest" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="containerHandler">Dependency used for handling operations by containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="menuFactory">Dependency used for creating menus.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="stateManager">Dependency used for managing state.</param>
    public ConfigureChest(
        ConfigManager configManager,
        ContainerFactory containerFactory,
        ContainerHandler containerHandler,
        IEventManager eventManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IIconRegistry iconRegistry,
        IInputHelper inputHelper,
        MenuFactory menuFactory,
        MenuHandler menuHandler,
        StateManager stateManager)
        : base(eventManager, configManager)
    {
        this.configManager = configManager;
        this.containerFactory = containerFactory;
        this.containerHandler = containerHandler;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.iconRegistry = iconRegistry;
        this.inputHelper = inputHelper;
        this.menuFactory = menuFactory;
        this.menuHandler = menuHandler;
        this.stateManager = stateManager;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive =>
        this.Config.DefaultOptions.ConfigureChest != FeatureOption.Disabled
        && this.genericModConfigMenuIntegration.IsLoaded;

    private IIcon CategorizeIcon => this.iconRegistry.Icon(InternalIcon.Categorize);

    private IIcon ConfigureIcon => this.iconRegistry.Icon(InternalIcon.Config);

    private IIcon SortIcon => this.iconRegistry.Icon(InternalIcon.Sort);

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Subscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<ItemHighlightingEventArgs>(ConfigureChest.OnItemHighlighting);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Unsubscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(ConfigureChest.OnItemHighlighting);
    }

    private static void OnItemHighlighting(ItemHighlightingEventArgs e)
    {
        if (Game1.activeClickableMenu?.GetChildMenu() is OptionDropdown<KeyValuePair<string, string>>)
        {
            e.UnHighlight();
        }
    }

    private string GetHoverText(IIcon icon)
    {
        if (icon.Id == this.ConfigureIcon.Id)
        {
            return I18n.Configure_Options_Name();
        }

        if (icon.Id == this.CategorizeIcon.Id)
        {
            return I18n.Configure_Categorize_Name();
        }

        if (icon.Id == this.SortIcon.Id)
        {
            return I18n.Configure_Sorting_Name();
        }

        return string.Empty;
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !this.Config.Controls.ConfigureChest.JustPressed()
            || (!this.containerFactory.TryGetOne(Game1.player, Game1.player.CurrentToolIndex, out var container)
                && !this.containerFactory.TryGetOne(Game1.player.currentLocation, e.Cursor.Tile, out container)))
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ConfigureChest);
        this.ShowMenu(container, this.ConfigureIcon);
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        if (this.menuHandler.Top?.Container.ConfigureChest is FeatureOption.Enabled)
        {
            this.SetupConfigureChest(this.menuHandler.Top);
        }

        if (this.menuHandler.Bottom?.Container.ConfigureChest is FeatureOption.Enabled)
        {
            this.SetupConfigureChest(this.menuHandler.Bottom);
        }
    }

    private void OnMenuChanged(MenuChangedEventArgs e)
    {
        if (this.lastContainer.Value is null
            || e.OldMenu?.GetType().Name != "SpecificModConfigMenu"
            || e.NewMenu?.GetType().Name == "SpecificModConfigMenu")
        {
            return;
        }

        this.configManager.SetupMainConfig();

        if (e.NewMenu?.GetType().Name != "ModConfigMenu")
        {
            this.lastContainer.Value = null;
            return;
        }

        this.lastContainer.Value.ShowMenu();
        this.lastContainer.Value = null;
    }

    private void SetupConfigureChest(MenuOverlay overlay)
    {
        var location = this.stateManager.ActiveMenu switch
        {
            ItemGrabMenu itemGrabMenu when overlay == this.menuHandler.Top =>
                new Point(overlay.Menu.xPositionOnScreen - Game1.tileSize - 36, itemGrabMenu.yPositionOnScreen + 4),
            ItemGrabMenu itemGrabMenu when overlay == this.menuHandler.Bottom => new Point(
                itemGrabMenu.xPositionOnScreen - Game1.tileSize,
                itemGrabMenu.yPositionOnScreen + (int)(itemGrabMenu.height / 2f) + 4),
            InventoryPage => new Point(
                overlay.Menu.xPositionOnScreen - Game1.tileSize - 36,
                overlay.Menu.yPositionOnScreen + 24),
            ShopMenu shopMenu when !shopMenu.tabButtons.Any() && overlay == this.menuHandler.Top => new Point(
                shopMenu.xPositionOnScreen - Game1.tileSize + 4,
                shopMenu.yPositionOnScreen + Game1.tileSize + 24),
            ShopMenu when overlay == this.menuHandler.Bottom => new Point(
                overlay.Menu.xPositionOnScreen - Game1.tileSize - 20,
                overlay.Menu.yPositionOnScreen + 24),
            _ => Point.Zero,
        };

        if (location.Equals(Point.Zero))
        {
            return;
        }

        var icon = !string.IsNullOrWhiteSpace(overlay.Container.StorageIcon)
            && this.iconRegistry.TryGetIcon(overlay.Container.StorageIcon, out var storageIcon)
                ? storageIcon
                : this.iconRegistry.Icon(VanillaIcon.Chest);

        var component =
            icon
                .Component(IconStyle.Transparent, "icon")
                .AsBuilder()
                .Location(location)
                .Size(new Point(Game1.tileSize, Game1.tileSize + 12))
                .Value;

        component.Clicked += (_, e) =>
        {
            e.PreventDefault();
            var options = new List<IIcon>
            {
                this.ConfigureIcon,
                this.CategorizeIcon,
                this.SortIcon,
            };

            var dropdown = new IconDropdown(component, options, 3, 1, this.GetHoverText);
            dropdown.IconSelected += (_, i) => this.ShowMenu(overlay.Container, i);
            Game1.activeClickableMenu?.SetChildMenu(dropdown);
        };

        overlay.Components.Add(component);
    }

    private void ShowMenu(IStorageContainer container, IIcon? icon)
    {
        this.lastContainer.Value = container;
        if (icon is null || icon.Id == this.ConfigureIcon.Id)
        {
            this.containerHandler.Configure(container);
            return;
        }

        if (icon.Id == this.CategorizeIcon.Id)
        {
            Game1.activeClickableMenu = this.menuFactory.CreateMenu(MenuType.Categorize, container);
            return;
        }

        if (icon.Id == this.SortIcon.Id)
        {
            Game1.activeClickableMenu = this.menuFactory.CreateMenu(MenuType.Sorting, container);
        }
    }
}