namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Customize how an inventory gets sorted.</summary>
internal sealed class SortInventory : BaseFeature<SortInventory>
{
    private readonly ContainerHandler containerHandler;
    private readonly IExpressionHandler expressionHandler;
    private readonly IIconRegistry iconRegistry;
    private readonly MenuHandler menuHandler;
    private readonly StateManager stateManager;

    /// <summary>Initializes a new instance of the <see cref="SortInventory" /> class.</summary>
    /// <param name="containerHandler">Dependency used for handling operations by containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="stateManager">Dependency used for managing state.</param>
    public SortInventory(
        ContainerHandler containerHandler,
        IEventManager eventManager,
        IExpressionHandler expressionHandler,
        IIconRegistry iconRegistry,
        MenuHandler menuHandler,
        IModConfig modConfig,
        StateManager stateManager)
        : base(eventManager, modConfig)
    {
        this.containerHandler = containerHandler;
        this.expressionHandler = expressionHandler;
        this.iconRegistry = iconRegistry;
        this.menuHandler = menuHandler;
        this.stateManager = stateManager;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.SortInventory != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ContainerSortingEventArgs>(this.OnContainerSorting);
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ContainerSortingEventArgs>(this.OnContainerSorting);
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
    }

    private void OnContainerSorting(ContainerSortingEventArgs e)
    {
        if (e.Container.SortInventory is not FeatureOption.Enabled
            || !this.expressionHandler.TryParseExpression(e.Container.SortInventoryBy, out var expression))
        {
            return;
        }

        var copy = e.Container.Items.ToList();
        copy.Sort(expression);
        e.Container.Items.OverwriteWith(copy);
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        if (this.menuHandler.Bottom?.Container.SortInventory is not FeatureOption.Enabled
            || this.stateManager.ActiveMenu is not ItemGrabMenu itemGrabMenu)
        {
            return;
        }

        // Add new organize button to the bottom inventory menu
        var x = itemGrabMenu.okButton.bounds.X;
        var y = itemGrabMenu.okButton.bounds.Y - Game1.tileSize - 16;

        var organizeButton =
            this
                .iconRegistry.Icon(VanillaIcon.Organize)
                .Component(IconStyle.Transparent)
                .AsBuilder()
                .Location(new Point(x, y))
                .HoverText(Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"))
                .Value;

        organizeButton.Clicked += (_, uiEventArgs) =>
        {
            switch (uiEventArgs.Button)
            {
                case SButton.MouseLeft or SButton.ControllerA:
                    this.containerHandler.Sort(this.menuHandler.Bottom.Container);
                    break;
                case SButton.MouseRight or SButton.ControllerB:
                    this.containerHandler.Sort(this.menuHandler.Bottom.Container, true);
                    break;
                default: return;
            }

            uiEventArgs.PreventDefault();
            Game1.playSound("Ship");
        };

        itemGrabMenu.trashCan.bounds.Y -= Game1.tileSize;
        itemGrabMenu.okButton.upNeighborID = organizeButton.myID;
        itemGrabMenu.trashCan.downNeighborID = organizeButton.myID;

        this.menuHandler.Bottom.Components.Add(organizeButton);
    }
}