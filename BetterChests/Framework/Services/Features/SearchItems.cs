namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
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
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new(() => true);
    private readonly MenuHandler menuHandler;
    private readonly PerScreen<TextField> searchBar;
    private readonly PerScreen<IExpression?> searchExpression = new();

    /// <summary>Initializes a new instance of the <see cref="SearchItems" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="menuHandler">Dependency used for managing the current menu.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public SearchItems(
        IEventManager eventManager,
        IExpressionHandler expressionHandler,
        IInputHelper inputHelper,
        MenuHandler menuHandler,
        IModConfig modConfig)
        : base(eventManager, modConfig)
    {
        this.expressionHandler = expressionHandler;
        this.inputHelper = inputHelper;
        this.menuHandler = menuHandler;

        this.searchBar = new PerScreen<TextField>(
            () =>
            {
                var textField = new TextField(0, 0, Math.Min(12 * Game1.tileSize, Game1.uiViewport.Width));
                textField.ValueChanged += (_, value) =>
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

                return textField;
            });
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.SearchItems != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<RenderingActiveMenuEventArgs>(this.OnRenderingActiveMenu);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Subscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Subscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
        this.Events.Subscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<RenderingActiveMenuEventArgs>(this.OnRenderingActiveMenu);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<InventoryMenuChangedEventArgs>(this.OnInventoryMenuChanged);
        this.Events.Unsubscribe<ItemHighlightingEventArgs>(this.OnItemHighlighting);
        this.Events.Unsubscribe<ItemsDisplayingEventArgs>(this.OnItemsDisplaying);
        this.Events.Unsubscribe<SearchChangedEventArgs>(this.OnSearchChanged);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        var container = this.menuHandler.Top.Container;
        if (container is null
            || !this.isActive.Value
            || this.menuHandler.CurrentMenu is not ItemGrabMenu
            || !this.menuHandler.CanFocus(this))
        {
            return;
        }

        var cursor = this.inputHelper.GetCursorPosition().GetScaledScreenPixels().ToPoint();
        switch (e.Button)
        {
            case SButton.MouseLeft or SButton.ControllerA:
                if (this.searchBar.Value.TryLeftClick(cursor))
                {
                    this.inputHelper.Suppress(e.Button);
                }

                break;

            case SButton.MouseRight or SButton.ControllerB:
                if (this.searchBar.Value.TryRightClick(cursor))
                {
                    this.inputHelper.Suppress(e.Button);
                }

                break;

            case SButton.Escape when this.menuHandler.CurrentMenu.readyToClose():
                this.inputHelper.Suppress(e.Button);
                Game1.playSound("bigDeSelect");
                this.menuHandler.CurrentMenu.exitThisMenu();
                break;

            case SButton.Escape: return;

            default:
                if (this.searchBar.Value.Selected
                    && e.Button is not (SButton.LeftShift
                        or SButton.RightShift
                        or SButton.LeftAlt
                        or SButton.RightAlt
                        or SButton.LeftControl
                        or SButton.RightControl))
                {
                    this.inputHelper.Suppress(e.Button);
                }

                return;
        }
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (this.menuHandler.Top.Container?.SearchItems is not FeatureOption.Enabled)
        {
            return;
        }

        // Toggle Search Bar
        if (this.Config.Controls.ToggleSearch.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ToggleSearch);
            this.isActive.Value = !this.isActive.Value;
            return;
        }

        // Copy Search
        if (this.searchBar.Value.Selected && this.Config.Controls.Copy.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.Copy);
            DesktopClipboard.SetText(this.searchBar.Value.Value);
            return;
        }

        // Paste Search
        if (this.searchBar.Value.Selected && this.Config.Controls.Paste.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.Paste);
            var searchText = string.Empty;
            DesktopClipboard.GetText(ref searchText);
            this.searchBar.Value.Value = searchText;
            _ = this.expressionHandler.TryParseExpression(searchText, out var expression);
            this.Events.Publish(new SearchChangedEventArgs(searchText, expression));
            return;
        }

        // Clear Search
        if (this.isActive.Value && this.Config.Controls.ClearSearch.JustPressed())
        {
            this.searchBar.Value.Value = string.Empty;
            this.searchExpression.Value = null;
            this.Events.Publish(new SearchChangedEventArgs(string.Empty, null));
        }
    }

    private void OnInventoryMenuChanged(InventoryMenuChangedEventArgs e)
    {
        var container = this.menuHandler.Top.Container;
        var top = this.menuHandler.Top;
        if (top.InventoryMenu is null || container?.SearchItems is not FeatureOption.Enabled)
        {
            return;
        }

        var width = this.searchBar.Value.Bounds.Width;
        var x = top.Columns switch
        {
            3 => top.InventoryMenu.inventory[1].bounds.Center.X - (width / 2),
            12 => top.InventoryMenu.inventory[5].bounds.Right - (width / 2),
            14 => top.InventoryMenu.inventory[6].bounds.Right - (width / 2),
            _ => (Game1.uiViewport.Width - width) / 2,
        };

        var y = top.InventoryMenu.yPositionOnScreen
            - (IClickableMenu.borderWidth / 2)
            - Game1.tileSize
            - (top.Rows == 3 ? 25 : 4);

        this.searchBar.Value.Location = new Point(x, y);
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

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        var container = this.menuHandler.Top.Container;
        if (!this.isActive.Value || container is null)
        {
            return;
        }

        var cursor = this.inputHelper.GetCursorPosition().GetScaledScreenPixels().ToPoint();
        this.searchBar.Value.Draw(e.SpriteBatch, cursor);
    }

    private void OnRenderingActiveMenu(RenderingActiveMenuEventArgs e)
    {
        if (!this.isActive.Value || this.menuHandler.CurrentMenu is not ItemGrabMenu)
        {
            return;
        }

        var cursor = this.inputHelper.GetCursorPosition().GetScaledScreenPixels().ToPoint();
        this.searchBar.Value.Update(cursor);
    }

    private void OnSearchChanged(SearchChangedEventArgs e)
    {
        this.searchExpression.Value = e.SearchExpression;
        if (!this.isActive.Value || this.menuHandler.CurrentMenu is not ItemGrabMenu)
        {
            return;
        }

        this.searchBar.Value.Value = e.SearchTerm;
    }
}