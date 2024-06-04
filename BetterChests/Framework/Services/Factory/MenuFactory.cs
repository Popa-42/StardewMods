namespace StardewMods.BetterChests.Framework.Services.Factory;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.UI.Components;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.BetterChests;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.UI.Components;
using StardewMods.Common.UI.Menus;
using StardewValley.Menus;

/// <summary>Creates menu instances.</summary>
internal sealed class MenuFactory
{
    private readonly ConfigManager configManager;
    private readonly IExpressionHandler expressionHandler;
    private readonly IIconRegistry iconRegistry;

    /// <summary>Initializes a new instance of the <see cref="MenuFactory" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="expressionHandler">Dependency used for parsing expressions.</param>
    /// <param name="iconRegistry">Dependency used for registering and retrieving icons.</param>
    public MenuFactory(ConfigManager configManager, IExpressionHandler expressionHandler, IIconRegistry iconRegistry)
    {
        this.configManager = configManager;
        this.expressionHandler = expressionHandler;
        this.iconRegistry = iconRegistry;
    }

    /// <summary>Creates a menu of the given type.</summary>
    /// <param name="menuType">The menu type.</param>
    /// <param name="container">The storage container to open the menu for.</param>
    /// <returns>Returns the menu.</returns>
    public IClickableMenu CreateMenu(MenuType menuType, IStorageContainer? container = null) =>
        menuType switch
        {
            MenuType.Categorize when container is not null => this.CreateCategorizeMenu(container),
            MenuType.Config => this.CreateConfigMenu(),
            MenuType.Debug => this.CreateDebugMenu(),
            MenuType.Layout => this.CreateLayoutMenu(),
            MenuType.Sorting when container is not null => this.CreateSortingMenu(container),
            MenuType.Tabs => this.CreateTabsMenu(),
            _ => throw new ArgumentOutOfRangeException(nameof(menuType), menuType, null),
        };

    private BaseMenu CreateCategorizeMenu(IStorageContainer container)
    {
        var (menu, textField, expressionsEditor, itemSelector) = this.CreateSearchMenu();
        textField.Value = container.CategorizeChestSearchTerm;
        textField.ValueChanged += (_, _) => UpdateExpression();
        itemSelector.AddOperation(
            items => expressionsEditor.Expression is null
                ? Enumerable.Empty<Item>()
                : items.Where(expressionsEditor.Expression.Equals));

        UpdateExpression();

        var saveButton = this.GetSaveButton(menu.Bounds.Right + 4, menu.Bounds.Y + Game1.tileSize + 16);

        var stackButtonOff = this
            .iconRegistry.Icon(InternalIcon.NoStack)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(new Point(menu.Bounds.Right + 4, menu.Bounds.Y + ((Game1.tileSize + 16) * 2)))
            .HoverText(I18n.Button_IncludeExistingStacks_Name())
            .IsVisible(container.CategorizeChestIncludeStacks is not FeatureOption.Enabled)
            .Value;

        var stackButtonOn = this
            .iconRegistry.Icon(VanillaIcon.Stack)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(new Point(menu.Bounds.Right + 4, menu.Bounds.Y + ((Game1.tileSize + 16) * 2)))
            .HoverText(I18n.Button_IncludeExistingStacks_Name())
            .IsVisible(container.CategorizeChestIncludeStacks is FeatureOption.Enabled)
            .Value;

        var copyButton = this.GetCopyButton(menu.Bounds.Right + 4, menu.Bounds.Y + ((Game1.tileSize + 16) * 3));
        var pasteButton = this.GetPasteButton(menu.Bounds.Right + 4, menu.Bounds.Y + ((Game1.tileSize + 16) * 4));

        var okButton = this.GetOkButton(
            menu.Bounds.Right + 4,
            menu.Bounds.Bottom - Game1.tileSize - (IClickableMenu.borderWidth / 2));

        okButton.Clicked += (_, _) =>
        {
            Game1.playSound("bigDeSelect");
            menu.exitThisMenuNoSound();
            container.ShowMenu();
        };

        saveButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            container.CategorizeChestSearchTerm = textField.Value;
            container.CategorizeChestIncludeStacks = stackButtonOn.visible
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        };

        copyButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            DesktopClipboard.SetText(textField.Value);
        };

        pasteButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            var searchText = string.Empty;
            DesktopClipboard.GetText(ref searchText);
            textField.Value = searchText;
            UpdateExpression();
        };

        stackButtonOff.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            stackButtonOff.IsVisible = false;
            stackButtonOn.IsVisible = true;
        };

        stackButtonOn.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            stackButtonOn.IsVisible = false;
            stackButtonOff.IsVisible = true;
        };

        menu.Components.Add(saveButton);
        menu.Components.Add(stackButtonOff);
        menu.Components.Add(stackButtonOn);
        menu.Components.Add(copyButton);
        menu.Components.Add(pasteButton);
        menu.Components.Add(okButton);

        return menu;

        void UpdateExpression()
        {
            expressionsEditor.Expression =
                this.expressionHandler.TryParseExpression(textField.Value, out var expression) ? expression : null;

            itemSelector.RefreshItems();
        }
    }

    private BaseMenu CreateConfigMenu()
    {
        var menu = new BaseMenu();
        return menu;
    }

    private BaseMenu CreateDebugMenu()
    {
        var menu = new BaseMenu();
        return menu;
    }

    private BaseMenu CreateLayoutMenu()
    {
        var menu = new BaseMenu();
        return menu;
    }

    private (BaseMenu Menu, TextField Search, ExpressionsEditor Expressions, ItemSelector Items) CreateSearchMenu()
    {
        var menu = new BaseMenu();

        menu.AddPartition(
            PartitionType.Horizontal,
            new Point(0, menu.Bounds.Y + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40));

        menu.AddPartition(
            PartitionType.VerticalIntersecting,
            new Point(
                menu.Bounds.X + (IClickableMenu.borderWidth / 2) + 400,
                menu.Bounds.Y + (IClickableMenu.borderWidth / 2) + Game1.tileSize + 40));

        var expressionsEditor = new ExpressionsEditor(
            this.expressionHandler,
            this.iconRegistry,
            menu.Bounds.X + IClickableMenu.borderWidth,
            menu.Bounds.Y
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 12,
            340,
            448);

        var scrollExpressions = new VerticalScrollBar(
            this.iconRegistry,
            expressionsEditor.Bounds.Right + 4,
            expressionsEditor.Bounds.Y + 4,
            expressionsEditor.Bounds.Height);

        scrollExpressions.Attach(expressionsEditor);

        var itemSelector = new ItemSelector(
            menu.Bounds.X + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2) + 428,
            menu.Bounds.Y
            + IClickableMenu.spaceToClearSideBorder
            + (IClickableMenu.borderWidth / 2)
            + (Game1.tileSize * 2)
            + 8,
            7,
            5);

        var scrollInventory = new VerticalScrollBar(
            this.iconRegistry,
            itemSelector.Bounds.Right + 4,
            itemSelector.Bounds.Y + 4,
            itemSelector.Bounds.Height);

        scrollInventory.Attach(itemSelector);

        var textField = new TextField(
            menu.Bounds.X + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2),
            menu.Bounds.Y + IClickableMenu.spaceToClearSideBorder + (IClickableMenu.borderWidth / 2) + Game1.tileSize,
            menu.Bounds.Width - (IClickableMenu.spaceToClearSideBorder * 2) - IClickableMenu.borderWidth,
            string.Empty);

        // Events
        expressionsEditor.ExpressionsChanged += (_, expression) =>
        {
            if (!string.IsNullOrWhiteSpace(expression?.Text))
            {
                textField.Value = expression.Text;
            }

            scrollExpressions.IsVisible = !expressionsEditor.Overflow.Equals(Point.Zero);
            itemSelector.RefreshItems();
        };

        // Add components
        menu.Components.Add(expressionsEditor);
        menu.Components.Add(itemSelector);
        menu.Components.Add(textField);
        menu.Components.Add(scrollExpressions);
        menu.Components.Add(scrollInventory);

        return (menu, textField, expressionsEditor, itemSelector);
    }

    private BaseMenu CreateSortingMenu(IStorageContainer container)
    {
        var (menu, textField, expressionsEditor, itemSelector) = this.CreateSearchMenu();

        textField.Value = container.SortInventoryBy;
        textField.ValueChanged += (_, _) => UpdateExpression();

        if (this.expressionHandler.TryParseExpression(container.CategorizeChestSearchTerm, out var searchExpression))
        {
            itemSelector.AddOperation(items => items.Where(searchExpression.Equals));
        }

        itemSelector.AddOperation(SortItems);
        UpdateExpression();

        var saveButton = this.GetSaveButton(menu.Bounds.Right + 4, menu.Bounds.Y + Game1.tileSize + 16);
        var copyButton = this.GetCopyButton(menu.Bounds.Right + 4, menu.Bounds.Y + ((Game1.tileSize + 16) * 2));
        var pasteButton = this.GetPasteButton(menu.Bounds.Right + 4, menu.Bounds.Y + ((Game1.tileSize + 16) * 3));

        var okButton = this.GetOkButton(
            menu.Bounds.Right + 4,
            menu.Bounds.Bottom - Game1.tileSize - (IClickableMenu.borderWidth / 2));

        okButton.Clicked += (_, _) =>
        {
            Game1.playSound("bigDeSelect");
            menu.exitThisMenuNoSound();
            container.ShowMenu();
        };

        saveButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            container.SortInventoryBy = textField.Value;
        };

        copyButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            DesktopClipboard.SetText(textField.Value);
        };

        pasteButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            var searchText = string.Empty;
            DesktopClipboard.GetText(ref searchText);
            textField.Value = searchText;
            UpdateExpression();
        };

        menu.Components.Add(saveButton);
        menu.Components.Add(copyButton);
        menu.Components.Add(pasteButton);
        menu.Components.Add(okButton);

        return menu;

        IEnumerable<Item> SortItems(IEnumerable<Item> items)
        {
            if (expressionsEditor.Expression is null)
            {
                return items;
            }

            var itemsToReturn = items.ToList();
            itemsToReturn.Sort(expressionsEditor.Expression);
            return itemsToReturn;
        }

        void UpdateExpression()
        {
            expressionsEditor.Expression =
                this.expressionHandler.TryParseExpression(textField.Value, out var expression) ? expression : null;

            itemSelector.RefreshItems();
        }
    }

    private BaseMenu CreateTabsMenu()
    {
        var config = this.configManager.GetNew();

        var (menu, textField, expressionsEditor, itemSelector) = this.CreateSearchMenu();
        textField.ValueChanged += (_, _) => UpdateExpression();
        itemSelector.AddOperation(
            items => expressionsEditor.Expression is null
                ? Enumerable.Empty<Item>()
                : items.Where(expressionsEditor.Expression.Equals));

        var saveButton = this.GetSaveButton(menu.Bounds.Right + 4, menu.Bounds.Y + Game1.tileSize + 16);
        var copyButton = this.GetCopyButton(menu.Bounds.Right + 4, menu.Bounds.Y + ((Game1.tileSize + 16) * 2));
        var pasteButton = this.GetPasteButton(menu.Bounds.Right + 4, menu.Bounds.Y + ((Game1.tileSize + 16) * 3));

        var editButton = this
            .iconRegistry.Icon(VanillaIcon.ColorPicker)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(new Point(menu.Bounds.Right + 4, menu.Bounds.Y + ((Game1.tileSize + 16) * 4)))
            .HoverText(I18n.Ui_Edit_Tooltip())
            .Value;

        var addButton =
            this
                .iconRegistry.Icon(VanillaIcon.Plus)
                .Component(IconStyle.Button)
                .AsBuilder()
                .Location(new Point(menu.Bounds.Right + 4, menu.Bounds.Y + ((Game1.tileSize + 16) * 5)))
                .HoverText(I18n.Ui_Add_Tooltip())
                .Value;

        var removeButton =
            this
                .iconRegistry.Icon(VanillaIcon.Trash)
                .Component(IconStyle.Button)
                .AsBuilder()
                .Location(new Point(menu.Bounds.Right + 4, menu.Bounds.Y + ((Game1.tileSize + 16) * 6)))
                .HoverText(I18n.Ui_Remove_Tooltip())
                .Value;

        var okButton = this.GetOkButton(
            menu.Bounds.Right + 4,
            menu.Bounds.Bottom - Game1.tileSize - (IClickableMenu.borderWidth / 2));

        okButton.Clicked += (_, _) =>
        {
            Game1.playSound("bigDeSelect");
            menu.exitThisMenuNoSound();
        };

        saveButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");

            // Save config
        };

        copyButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            DesktopClipboard.SetText(textField.Value);
        };

        pasteButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            var searchText = string.Empty;
            DesktopClipboard.GetText(ref searchText);
            textField.Value = searchText;
            UpdateExpression();
        };

        editButton.Clicked += (_, _) =>
        {
            var activeTab = menu.Components.OfType<TabEditor>().FirstOrDefault(tab => tab.Active);
            if (activeTab is null || !this.iconRegistry.TryGetIcon(activeTab.Data.Icon, out var icon))
            {
                return;
            }

            var iconWithLabel = new InputWithIcon(this.iconRegistry, icon, activeTab.Data.Label);

            // icon selected event
            menu.SetChildMenu(iconWithLabel);
        };

        addButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            var tabData = new TabData
            {
                Icon = VanillaIcon.Plus.ToStringFast(),
                Label = I18n.Ui_NewTab_Name(),
            };

            AddTab(tabData, config.InventoryTabList.Count);
            config.InventoryTabList.Add(tabData);
        };

        removeButton.Clicked += (_, _) =>
        {
            Game1.playSound("drumkit6");
            var activeTab = menu.Components.OfType<TabEditor>().FirstOrDefault(tab => tab.Active);
            if (activeTab is null)
            {
                return;
            }

            config.InventoryTabList.RemoveAt(activeTab.Index);
            for (var index = menu.Components.IndexOf(activeTab); index < menu.Components.Count - 1; index++)
            {
                var current = (TabEditor)menu.Components[index];
                var next = (TabEditor)menu.Components[index + 1];
                (current.Index, next.Index) = (next.Index, current.Index);

                var currentY = current.bounds.Y;
                var nextY = next.bounds.Y;

                current.Location = new Point(current.bounds.X, nextY);
                next.Location = new Point(next.bounds.X, currentY);

                (menu.Components[index], menu.Components[index + 1]) =
                    (menu.Components[index + 1], menu.Components[index]);
            }

            menu.Components.RemoveAt(menu.Components.Count - 1);
        };

        menu.Components.Add(saveButton);
        menu.Components.Add(copyButton);
        menu.Components.Add(pasteButton);
        menu.Components.Add(editButton);
        menu.Components.Add(addButton);
        menu.Components.Add(removeButton);
        menu.Components.Add(okButton);

        for (var i = config.InventoryTabList.Count - 1; i >= 0; i--)
        {
            if (!this.iconRegistry.TryGetIcon(config.InventoryTabList[i].Icon, out _))
            {
                config.InventoryTabList.RemoveAt(i);
            }
        }

        for (var i = 0; i < config.InventoryTabList.Count; i++)
        {
            AddTab(config.InventoryTabList[i], i);

            if (i != 0)
            {
                continue;
            }

            textField.Value = config.InventoryTabList[i].SearchTerm;
            UpdateExpression();
        }

        return menu;

        void AddTab(TabData tabData, int index)
        {
            var icon = this.iconRegistry.Icon(tabData.Icon);
            var tabEditor = new TabEditor(
                this.iconRegistry,
                menu.Bounds.X - (Game1.tileSize * 2) - 256,
                menu.Bounds.Y + (Game1.tileSize * (index + 1)) + 16,
                (Game1.tileSize * 2) + 256,
                icon,
                tabData)
            {
                Active = index == 0,
                Index = index,
            };

            tabEditor.Clicked += (_, _) =>
            {
                Game1.playSound("drumkit6");
                textField.Value = tabEditor.Data.SearchTerm;
                UpdateExpression();
                var activeTab = menu.Components.OfType<TabEditor>().FirstOrDefault(tab => tab.Active);
                if (activeTab is not null)
                {
                    activeTab.Active = false;
                }

                tabEditor.Active = true;
            };

            tabEditor.MoveDown += (_, _) =>
            {
                if (tabEditor.Index >= config.InventoryTabList.Count - 1)
                {
                    return;
                }

                Game1.playSound("drumkit6");

                (config.InventoryTabList[tabEditor.Index], config.InventoryTabList[tabEditor.Index + 1]) = (
                    config.InventoryTabList[tabEditor.Index + 1], config.InventoryTabList[tabEditor.Index]);

                var newIndex = menu.Components.IndexOf(tabEditor);
                var current = (TabEditor)menu.Components[newIndex];
                var next = (TabEditor)menu.Components[newIndex + 1];
                (current.Index, next.Index) = (next.Index, current.Index);

                var currentY = current.bounds.Y;
                var nextY = next.bounds.Y;

                current.Location = new Point(current.bounds.X, nextY);
                next.Location = new Point(next.bounds.X, currentY);

                (menu.Components[newIndex], menu.Components[newIndex + 1]) =
                    (menu.Components[newIndex + 1], menu.Components[newIndex]);
            };

            tabEditor.MoveUp += (_, _) =>
            {
                if (tabEditor.Index <= 0)
                {
                    return;
                }

                Game1.playSound("drumkit6");

                (config.InventoryTabList[tabEditor.Index], config.InventoryTabList[tabEditor.Index - 1]) = (
                    config.InventoryTabList[tabEditor.Index - 1], config.InventoryTabList[tabEditor.Index]);

                var newIndex = menu.Components.IndexOf(tabEditor);
                var current = (TabEditor)menu.Components[newIndex];
                var next = (TabEditor)menu.Components[newIndex - 1];
                (current.Index, next.Index) = (next.Index, current.Index);

                var currentY = current.bounds.Y;
                var nextY = next.bounds.Y;

                current.Location = new Point(current.bounds.X, nextY);
                next.Location = new Point(next.bounds.X, currentY);

                (menu.Components[newIndex], menu.Components[newIndex - 1]) =
                    (menu.Components[newIndex - 1], menu.Components[newIndex]);
            };

            menu.Components.Add(tabEditor);
        }

        void UpdateExpression()
        {
            expressionsEditor.Expression =
                this.expressionHandler.TryParseExpression(textField.Value, out var expression) ? expression : null;

            itemSelector.RefreshItems();
        }
    }

    private TextureComponent GetCopyButton(int x, int y) =>
        this
            .iconRegistry.Icon(InternalIcon.Copy)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(new Point(x, y))
            .HoverText(I18n.Ui_Copy_Tooltip())
            .Value;

    private TextureComponent GetOkButton(int x, int y) =>
        this
            .iconRegistry.Icon(VanillaIcon.Ok)
            .Component(IconStyle.Transparent)
            .AsBuilder()
            .Location(new Point(x, y))
            .Value;

    private TextureComponent GetPasteButton(int x, int y) =>
        this
            .iconRegistry.Icon(InternalIcon.Paste)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(new Point(x, y))
            .HoverText(I18n.Ui_Paste_Tooltip())
            .Value;

    private TextureComponent GetSaveButton(int x, int y) =>
        this
            .iconRegistry.Icon(InternalIcon.Save)
            .Component(IconStyle.Button)
            .AsBuilder()
            .Location(new Point(x, y))
            .HoverText(I18n.Ui_Save_Name())
            .Value;
}