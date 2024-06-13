namespace StardewMods.BetterChests.Framework.Services;

using System.Globalization;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.UI.Overlays;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Handles changes to the active menu.</summary>
internal sealed class MenuHandler : BaseService<MenuHandler>
{
    private static MenuHandler instance = null!;

    private readonly PerScreen<MenuOverlay?> bottomOverlay = new();
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<bool> drawBg = new();
    private readonly IEventManager eventManager;
    private readonly PerScreen<ServiceLock?> focus = new();
    private readonly IInputHelper inputHelper;
    private readonly StateManager stateManager;
    private readonly PerScreen<MenuOverlay?> topOverlay = new();

    /// <summary>Initializes a new instance of the <see cref="MenuHandler" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modEvents">Dependency used for managing access to SMAPI events.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    /// <param name="stateManager">Dependency used for managing state.</param>
    public MenuHandler(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IInputHelper inputHelper,
        IModEvents modEvents,
        IPatchManager patchManager,
        StateManager stateManager)
    {
        // Initialize
        MenuHandler.instance = this;
        this.containerFactory = containerFactory;
        this.eventManager = eventManager;
        this.inputHelper = inputHelper;
        this.stateManager = stateManager;

        // Events
        eventManager.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        eventManager.Subscribe<ButtonReleasedEventArgs>(this.OnButtonReleased);
        eventManager.Subscribe<CursorMovedEventArgs>(this.OnCursorMoved);
        eventManager.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        eventManager.Subscribe<MouseWheelScrolledEventArgs>(this.OnMouseWheelScrolled);
        eventManager.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        eventManager.Subscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
        eventManager.Subscribe<WindowResizedEventArgs>(this.OnWindowResized);
        modEvents.Display.RenderingActiveMenu += this.OnRenderingActiveMenu_First;
        modEvents.Display.RenderingActiveMenu += this.OnRenderingActiveMenu_Last;

        // Patches
        patchManager.Add(
            this.UniqueId,
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(InventoryMenu),
                    nameof(InventoryMenu.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(int)]),
                AccessTools.DeclaredMethod(typeof(MenuHandler), nameof(MenuHandler.InventoryMenu_draw_prefix)),
                PatchType.Prefix),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(InventoryMenu),
                    nameof(InventoryMenu.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(int)]),
                AccessTools.DeclaredMethod(typeof(MenuHandler), nameof(MenuHandler.InventoryMenu_draw_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(InventoryMenu),
                    nameof(InventoryMenu.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(int)]),
                AccessTools.DeclaredMethod(
                    typeof(MenuHandler),
                    nameof(MenuHandler.InventoryMenu_highlightMethod_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(InventoryMenu), nameof(InventoryMenu.hover)),
                AccessTools.DeclaredMethod(
                    typeof(MenuHandler),
                    nameof(MenuHandler.InventoryMenu_highlightMethod_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(InventoryMenu), nameof(InventoryMenu.leftClick)),
                AccessTools.DeclaredMethod(
                    typeof(MenuHandler),
                    nameof(MenuHandler.InventoryMenu_highlightMethod_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(InventoryMenu), nameof(InventoryMenu.rightClick)),
                AccessTools.DeclaredMethod(
                    typeof(MenuHandler),
                    nameof(MenuHandler.InventoryMenu_highlightMethod_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(InventoryMenu), nameof(InventoryMenu.tryToAddItem)),
                AccessTools.DeclaredMethod(
                    typeof(MenuHandler),
                    nameof(MenuHandler.InventoryMenu_highlightMethod_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools
                    .GetDeclaredConstructors(typeof(ItemGrabMenu))
                    .Single(ctor => ctor.GetParameters().Length > 5),
                AccessTools.DeclaredMethod(
                    typeof(MenuHandler),
                    nameof(MenuHandler.ItemGrabMenu_constructor_transpiler)),
                PatchType.Transpiler));

        patchManager.Patch(this.UniqueId);
    }

    /// <summary>Gets the bottom overlay.</summary>
    public MenuOverlay? Bottom
    {
        get => this.bottomOverlay.Value;
        private set => this.bottomOverlay.Value = value;
    }

    /// <summary>Gets the top overlay.</summary>
    public MenuOverlay? Top
    {
        get => this.topOverlay.Value;
        private set => this.topOverlay.Value = value;
    }

    /// <summary>Determines if the specified source object can receive focus.</summary>
    /// <param name="source">The object to check if it can receive focus.</param>
    /// <returns><c>true</c> if the source object can receive focus; otherwise, <c>false</c>.</returns>
    public bool CanFocus(object source) => this.focus.Value is null || this.focus.Value.Source == source;

    /// <summary>Tries to request focus for a specific object.</summary>
    /// <param name="source">The object that needs focus.</param>
    /// <param name="serviceLock">
    /// An optional output parameter representing the acquired service lock, or null if failed to
    /// acquire.
    /// </param>
    /// <returns><c>true</c> if focus was successfully acquired; otherwise, <c>false</c>.</returns>
    public bool TryGetFocus(object source, [NotNullWhen(true)] out IServiceLock? serviceLock)
    {
        serviceLock = null;
        if (this.focus.Value is not null && this.focus.Value.Source != source)
        {
            return false;
        }

        if (this.focus.Value is not null && this.focus.Value.Source == source)
        {
            serviceLock = this.focus.Value;
            return true;
        }

        this.focus.Value = new ServiceLock(source, this);
        serviceLock = this.focus.Value;
        return true;
    }

    private static void DrawOverlay(SpriteBatch spriteBatch)
    {
        MenuHandler.instance.Top?.draw(spriteBatch);
        MenuHandler.instance.Bottom?.draw(spriteBatch);
    }

    private static object? GetChestContext(Item? sourceItem, object? context) =>
        context switch
        {
            Chest chest => chest,
            SObject
            {
                heldObject.Value: Chest heldChest,
            } => heldChest,
            Building building when building.GetBuildingChest("Output") is
                { } outputChest => outputChest,
            GameLocation location when location.GetFridge() is
                { } fridge => fridge,
            _ => sourceItem,
        };

    private static InventoryMenu.highlightThisItem GetHighlightMethod(
        InventoryMenu.highlightThisItem originalMethod,
        InventoryMenu menu)
    {
        var overlay = menu.Equals(MenuHandler.instance.Top?.Menu)
            ? MenuHandler.instance.Top
            : menu.Equals(MenuHandler.instance.Bottom?.Menu)
                ? MenuHandler.instance.Bottom
                : null;

        if (overlay is null)
        {
            return originalMethod;
        }

        return HighlightMethod;

        bool HighlightMethod(Item item)
        {
            var itemHighlightingEventArgs = new ItemHighlightingEventArgs(overlay.Container, item);
            MenuHandler.instance.eventManager.Publish(itemHighlightingEventArgs);
            return itemHighlightingEventArgs.IsHighlighted;
        }
    }

    private static int GetMenuCapacity(int capacity, object? context)
    {
        switch (context)
        {
            case Item item when MenuHandler.instance.containerFactory.TryGetOne(item, out var container):
            case Building building when MenuHandler.instance.containerFactory.TryGetOne(building, out container):
                return container.ResizeChest switch
                {
                    ChestMenuOption.Small => 9,
                    ChestMenuOption.Medium => 36,
                    ChestMenuOption.Large => 70,
                    _ when capacity is > 70 or -1 => 70,
                    _ => capacity,
                };

            default: return capacity > 70 ? 70 : capacity;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_postfix(InventoryMenu __instance, ref MenuOverlay? __state)
    {
        if (__state?.Container is null)
        {
            return;
        }

        // Restore original
        __instance.actualInventory = __state.Container.Items;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_prefix(InventoryMenu __instance, ref MenuOverlay? __state)
    {
        __state = __instance.Equals(MenuHandler.instance.Top?.Menu)
            ? MenuHandler.instance.Top
            : __instance.Equals(MenuHandler.instance.Bottom?.Menu)
                ? MenuHandler.instance.Bottom
                : null;

        if (__state?.Container is null)
        {
            return;
        }

        // Apply operations
        var itemsDisplayingEventArgs = new ItemsDisplayingEventArgs(__state.Container);
        MenuHandler.instance.eventManager.Publish(itemsDisplayingEventArgs);
        __instance.actualInventory = itemsDisplayingEventArgs.Items.ToList();

        var defaultName = int.MaxValue.ToString(CultureInfo.InvariantCulture);
        var emptyIndex = -1;
        for (var index = 0; index < __instance.inventory.Count; ++index)
        {
            if (index >= __instance.actualInventory.Count)
            {
                __instance.inventory[index].name = defaultName;
                continue;
            }

            if (__instance.actualInventory[index] is null)
            {
                // Iterate to next empty index
                while (++emptyIndex < __state.Container.Items.Count && __state.Container.Items[emptyIndex] is not null)
                {
                    // Do nothing
                }

                if (emptyIndex >= __state.Container.Items.Count)
                {
                    __instance.inventory[index].name = defaultName;
                    continue;
                }

                __instance.inventory[index].name = emptyIndex.ToString(CultureInfo.InvariantCulture);
                continue;
            }

            var actualIndex = __state.Container.Items.IndexOf(__instance.actualInventory[index]);
            __instance.inventory[index].name =
                actualIndex > -1 ? actualIndex.ToString(CultureInfo.InvariantCulture) : defaultName;
        }
    }

    private static IEnumerable<CodeInstruction>
        InventoryMenu_highlightMethod_transpiler(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchEndForward(
                new CodeMatch(
                    instruction => instruction.LoadsField(
                        AccessTools.DeclaredField(typeof(InventoryMenu), nameof(InventoryMenu.highlightMethod)))))
            .Repeat(
                codeMatcher => codeMatcher
                    .Advance(1)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(
                            OpCodes.Call,
                            AccessTools.DeclaredMethod(typeof(MenuHandler), nameof(MenuHandler.GetHighlightMethod)))))
            .InstructionEnumeration();

    private static IEnumerable<CodeInstruction>
        ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchStartForward(new CodeMatch(OpCodes.Stloc_1))
            .Advance(-1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_S, (short)16),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(MenuHandler), nameof(MenuHandler.GetChestContext))))
            .MatchStartForward(
                new CodeMatch(
                    instruction => instruction.Calls(
                        AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)))))
            .Advance(1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_S, (short)16),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(MenuHandler), nameof(MenuHandler.GetMenuCapacity))))
            .MatchStartForward(
                new CodeMatch(
                    instruction => instruction.Calls(
                        AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)))))
            .Advance(1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_S, (short)16),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.DeclaredMethod(typeof(MenuHandler), nameof(MenuHandler.GetMenuCapacity))))
            .InstructionEnumeration();

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (this.stateManager.ActiveMenu is null || (this.Top is null && this.Bottom is null))
        {
            return;
        }

        var cursor = e.Cursor.GetScaledScreenPixels().ToPoint();
        switch (e.Button)
        {
            case SButton.MouseLeft or SButton.ControllerA:
                this.Top?.receiveLeftClick(cursor.X, cursor.Y);
                this.Bottom?.receiveLeftClick(cursor.X, cursor.Y);
                return;
            case SButton.MouseRight or SButton.ControllerB:
                this.Top?.receiveRightClick(cursor.X, cursor.Y);
                this.Bottom?.receiveRightClick(cursor.X, cursor.Y);
                return;
        }
    }

    private void OnButtonReleased(ButtonReleasedEventArgs e)
    {
        if (this.stateManager.ActiveMenu is null
            || (this.Top is null && this.Bottom is null)
            || e.Button is not SButton.MouseLeft)
        {
            return;
        }

        var cursor = e.Cursor.GetScaledScreenPixels().ToPoint();
        this.Top?.releaseLeftClick(cursor.X, cursor.Y);
        this.Bottom?.releaseLeftClick(cursor.X, cursor.Y);
    }

    private void OnCursorMoved(CursorMovedEventArgs e)
    {
        if (this.stateManager.ActiveMenu is null || (this.Top is null && this.Bottom is null))
        {
            return;
        }

        var cursor = e.NewPosition.GetScaledScreenPixels().ToPoint();
        this.Top?.performHoverAction(cursor.X, cursor.Y);
        this.Bottom?.performHoverAction(cursor.X, cursor.Y);
    }

    [Priority(int.MaxValue)]
    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        this.Top = e.Top is not null && this.containerFactory.TryGetOne(e.Top, out var topContainer)
            ? new MenuOverlay(this.Top, topContainer, e.Top)
            : null;

        this.Bottom = e.Bottom is not null && this.containerFactory.TryGetOne(e.Bottom, out var bottomContainer)
            ? new MenuOverlay(this.Bottom, bottomContainer, e.Bottom)
            : null;
    }

    private void OnMouseWheelScrolled(MouseWheelScrolledEventArgs e)
    {
        if (this.stateManager.ActiveMenu is null || (this.Top is null && this.Bottom is null))
        {
            return;
        }

        this.Top?.receiveScrollWheelAction(e.Delta);
        this.Bottom?.receiveScrollWheelAction(e.Delta);
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (this.stateManager.ActiveMenu is null || (this.Top is null && this.Bottom is null))
        {
            return;
        }

        this.Top?.draw(e.SpriteBatch);
        this.Bottom?.draw(e.SpriteBatch);
    }

    [EventPriority((EventPriority)int.MaxValue)]
    private void OnRenderingActiveMenu_First(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (Game1.options.showClearBackgrounds
            || this.stateManager.ActiveMenu is not ItemGrabMenu
            {
                drawBG: true,
            } itemGrabMenu)
        {
            this.drawBg.Value = false;
            return;
        }

        // Disable drawing background
        itemGrabMenu.drawBG = false;
        this.drawBg.Value = true;
    }

    [EventPriority((EventPriority)int.MinValue)]
    private void OnRenderingActiveMenu_Last(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (!this.drawBg.Value || this.stateManager.ActiveMenu is not ItemGrabMenu itemGrabMenu)
        {
            return;
        }

        // Redraw background (this allows components to be drawn under the menu on a higher priority event)
        itemGrabMenu.drawBG = true;
        e.SpriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.5f);
    }

    private void OnUpdateTicked(UpdateTickedEventArgs e)
    {
        if (this.stateManager.ActiveMenu is null
            || (this.Top is null && this.Bottom is null)
            || !this.inputHelper.IsDown(SButton.MouseLeft))
        {
            return;
        }

        var cursor = this.inputHelper.GetCursorPosition().GetScaledScreenPixels().ToPoint();
        this.Top?.leftClickHeld(cursor.X, cursor.Y);
        this.Bottom?.leftClickHeld(cursor.X, cursor.Y);
    }

    private void OnWindowResized(WindowResizedEventArgs e)
    {
        if (this.stateManager.ActiveMenu is null || (this.Top is null && this.Bottom is null))
        {
            return;
        }

        var oldBounds = new Rectangle(0, 0, e.OldSize.X, e.OldSize.Y);
        var newBounds = new Rectangle(0, 0, e.NewSize.X, e.NewSize.Y);
        this.Top?.gameWindowSizeChanged(oldBounds, newBounds);
        this.Bottom?.gameWindowSizeChanged(oldBounds, newBounds);
    }

    private sealed class ServiceLock(object source, MenuHandler menuHandler) : IServiceLock
    {
        public object Source => source;

        public void Release()
        {
            if (menuHandler.focus.Value == this)
            {
                menuHandler.focus.Value = null;
            }
        }
    }
}