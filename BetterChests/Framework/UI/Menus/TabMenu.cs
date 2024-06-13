namespace StardewMods.BetterChests.Framework.UI.Menus;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Helpers;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;

/// <summary>Menu for customizing tabs.</summary>
internal sealed class TabMenu : SearchMenu
{
    private readonly IModConfig config;
    private readonly IIconRegistry iconRegistry;

    private TabEditor? activeTab;

    /// <summary>Initializes a new instance of the <see cref="TabMenu" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    public TabMenu(ConfigManager configManager, IExpressionHandler expressionHandler, IIconRegistry iconRegistry)
        : base(expressionHandler, iconRegistry, string.Empty)
    {
        // Initialize
        this.iconRegistry = iconRegistry;
        this.config = configManager.GetNew();

        var saveButton = iconRegistry
            .Icon(InternalIcon.Save)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(new Point(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + Game1.tileSize + 16))
            .HoverText(I18n.Ui_Save_Name())
            .Value;

        var copyButton = iconRegistry
            .Icon(InternalIcon.Copy)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 2)))
            .HoverText(I18n.Ui_Copy_Tooltip())
            .Value;

        var pasteButton = iconRegistry
            .Icon(InternalIcon.Paste)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 3)))
            .HoverText(I18n.Ui_Paste_Tooltip())
            .Value;

        var editButton = iconRegistry
            .Icon(VanillaIcon.ColorPicker)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 4)))
            .HoverText(I18n.Ui_Edit_Tooltip())
            .Value;

        var addButton = iconRegistry
            .Icon(VanillaIcon.Plus)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 5)))
            .HoverText(I18n.Ui_Add_Tooltip())
            .Value;

        var removeButton = iconRegistry
            .Icon(VanillaIcon.Trash)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(
                new Point(
                    this.xPositionOnScreen + this.width + 4,
                    this.yPositionOnScreen + ((Game1.tileSize + 16) * 6)))
            .HoverText(I18n.Ui_Remove_Tooltip())
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

        // Events
        saveButton.Clicked += this.OnClickedSave;
        copyButton.Clicked += this.OnClickedCopy;
        pasteButton.Clicked += this.OnClickedPaste;
        addButton.Clicked += this.OnClickedAdd;
        removeButton.Clicked += this.OnClickedRemove;
        okButton.Clicked += this.OnClickedOk;

        // Add components
        this.Components.Add(copyButton);
        this.Components.Add(pasteButton);
        this.Components.Add(editButton);
        this.Components.Add(addButton);
        this.Components.Add(removeButton);
        this.Components.Add(okButton);

        for (var i = this.config.InventoryTabList.Count - 1; i >= 0; i--)
        {
            if (!this.iconRegistry.TryGetIcon(this.config.InventoryTabList[i].Icon, out _))
            {
                this.config.InventoryTabList.RemoveAt(i);
            }
        }

        for (var i = 0; i < this.config.InventoryTabList.Count; i++)
        {
            var tabData = this.config.InventoryTabList[i];
            var icon = this.iconRegistry.Icon(tabData.Icon);
            var tabIcon = new TabEditor(
                this.iconRegistry,
                this.xPositionOnScreen - (Game1.tileSize * 2) - 256,
                this.yPositionOnScreen + (Game1.tileSize * (i + 1)) + 16,
                (Game1.tileSize * 2) + 256,
                icon,
                tabData)
            {
                Active = i == 0,
                Index = i,
            };

            tabIcon.Clicked += this.OnClickedIcon;
            tabIcon.MoveDown += this.OnMoveDown;
            tabIcon.MoveUp += this.OnMoveUp;

            this.Components.Add(tabIcon);

            if (i != 0)
            {
                continue;
            }

            this.activeTab = tabIcon;
            this.SearchText = tabData.SearchTerm;
            this.UpdateExpression();
        }
    }

    /// <inheritdoc />
    protected override bool HighlightMethod(Item item) => true;

    private void OnClickedAdd(object? sender, UiEventArgs e)
    {
        e.PreventDefault();
        Game1.playSound("drumkit6");
        var tabData = new TabData
        {
            Icon = VanillaIcon.Plus.ToStringFast(),
            Label = I18n.Ui_NewTab_Name(),
        };

        var icon = this.iconRegistry.Icon(VanillaIcon.Plus);
        var tabIcon = new TabEditor(
            this.iconRegistry,
            this.xPositionOnScreen - (Game1.tileSize * 2) - 256,
            this.yPositionOnScreen + (Game1.tileSize * (this.config.InventoryTabList.Count + 1)) + 16,
            (Game1.tileSize * 2) + 256,
            icon,
            tabData)
        {
            Active = false,
            Index = this.config.InventoryTabList.Count,
        };

        tabIcon.Clicked += this.OnClickedIcon;
        tabIcon.MoveDown += this.OnMoveDown;
        tabIcon.MoveUp += this.OnMoveUp;

        this.Components.Add(tabIcon);
        this.config.InventoryTabList.Add(tabData);
    }

    private void OnClickedCopy(object? sender, UiEventArgs e)
    {
        e.PreventDefault();
        Game1.playSound("drumkit6");
        DesktopClipboard.SetText(this.SearchText);
    }

    private void OnClickedIcon(object? sender, UiEventArgs e)
    {
        if (sender is not TabEditor tabEditor)
        {
            return;
        }

        e.PreventDefault();
        Game1.playSound("drumkit6");
        this.SearchText = tabEditor.Data.SearchTerm;
        this.UpdateExpression();
        if (this.activeTab is not null)
        {
            this.activeTab.Active = false;
        }

        this.activeTab = tabEditor;
        if (this.activeTab is not null)
        {
            this.activeTab.Active = true;
        }
    }

    private void OnClickedOk(object? sender, UiEventArgs e)
    {
        e.PreventDefault();
        Game1.playSound("bigDeSelect");
        this.exitThisMenuNoSound();
    }

    private void OnClickedPaste(object? sender, UiEventArgs e)
    {
        e.PreventDefault();
        Game1.playSound("drumkit6");
        var searchText = string.Empty;
        DesktopClipboard.GetText(ref searchText);
        this.SearchText = searchText;
        this.UpdateExpression();
    }

    private void OnClickedRemove(object? sender, UiEventArgs e)
    {
        e.PreventDefault();
        Game1.playSound("drumkit6");
        if (this.activeTab is null)
        {
            return;
        }

        this.config.InventoryTabList.RemoveAt(this.activeTab.Index);
        for (var index = this.Components.IndexOf(this.activeTab); index < this.Components.Count - 1; index++)
        {
            var current = (TabEditor)this.Components[index];
            var next = (TabEditor)this.Components[index + 1];
            (current.Index, next.Index) = (next.Index, current.Index);

            var currentY = current.bounds.Y;
            var nextY = next.bounds.Y;

            current.Location = new Point(current.bounds.X, nextY);
            next.Location = new Point(next.bounds.X, currentY);

            (this.Components[index], this.Components[index + 1]) = (this.Components[index + 1], this.Components[index]);
        }

        this.Components.RemoveAt(this.Components.Count - 1);
        this.activeTab = null;
    }

    private void OnClickedSave(object? sender, UiEventArgs e)
    {
        e.PreventDefault();
        Game1.playSound("drumkit6");
        if (this.activeTab is not null)
        {
            this.config.InventoryTabList[this.activeTab.Index].SearchTerm = this.SearchText;
        }
    }

    private void OnMoveDown(object? sender, UiEventArgs e)
    {
        e.PreventDefault();
        if (sender is not TabEditor tabEditor || tabEditor.Index >= this.config.InventoryTabList.Count - 1)
        {
            return;
        }

        Game1.playSound("drumkit6");

        (this.config.InventoryTabList[tabEditor.Index], this.config.InventoryTabList[tabEditor.Index + 1]) = (
            this.config.InventoryTabList[tabEditor.Index + 1], this.config.InventoryTabList[tabEditor.Index]);

        var index = this.Components.IndexOf(tabEditor);
        var current = (TabEditor)this.Components[index];
        var next = (TabEditor)this.Components[index + 1];
        (current.Index, next.Index) = (next.Index, current.Index);

        var currentY = current.bounds.Y;
        var nextY = next.bounds.Y;

        current.Location = new Point(current.bounds.X, nextY);
        next.Location = new Point(next.bounds.X, currentY);

        (this.Components[index], this.Components[index + 1]) = (this.Components[index + 1], this.Components[index]);
    }

    private void OnMoveUp(object? sender, UiEventArgs e)
    {
        e.PreventDefault();
        if (sender is not TabEditor
            {
                Index: > 0,
            } tabEditor)
        {
            return;
        }

        Game1.playSound("drumkit6");
        (this.config.InventoryTabList[tabEditor.Index], this.config.InventoryTabList[tabEditor.Index - 1]) = (
            this.config.InventoryTabList[tabEditor.Index - 1], this.config.InventoryTabList[tabEditor.Index]);

        var index = this.Components.IndexOf(tabEditor);
        var current = (TabEditor)this.Components[index];
        var previous = (TabEditor)this.Components[index - 1];
        (current.Index, previous.Index) = (previous.Index, current.Index);

        var currentY = current.bounds.Y;
        var previousY = previous.bounds.Y;

        current.Location = new Point(current.bounds.X, previousY);
        previous.Location = new Point(previous.bounds.X, currentY);

        (this.Components[index], this.Components[index - 1]) = (this.Components[index - 1], this.Components[index]);
    }
}